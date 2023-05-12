using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using CompressionLevel = System.IO.Compression.CompressionLevel;

public class GhostManager : MonoBehaviour
{
    private const int CurrentVer = 2;

    private const int MinimumVer = 2;

    // This is the camera object
    public Transform cameraTransform;
    public PlayerMovement playerMovement;

    // Enable this to make the script record for this object
    public bool shouldRecord;

    // Enable this to playback a ghost recording (DEBUG)
    public bool shouldReplay;
    public bool shouldPlayCutscene;
    public string ghostID;
    private MemoryStream compressedGhostData;
    private bool currentlyRecording, playbackBegun, ghostDownloaded;
    private BinaryWriter dataWriter;
    private string endpoint;
    private MemoryStream ghostData;
    private Quaternion idealCameraRotation;
    private ReplayFrame[] replayFrames;
    private uint replayLength, currentFrameIndex;

    private void Start()
    {
        LevelCompleteEvent += LevelComplete;

        endpoint = Globals.Instance.APIEndpoint;
        idealCameraRotation = cameraTransform.rotation;
        if (!string.IsNullOrEmpty(Globals.Instance.replayToStart))
        {
            shouldRecord = false;
            shouldReplay = true;
            shouldPlayCutscene = false;
            ghostID = Globals.Instance.replayToStart;
            Globals.Instance.replayToStart = "";
        }

        if (Globals.Instance.cutsceneActive)
            Globals.Instance.CutsceneOver += HandleCutsceneOverEvent;
        else if (shouldRecord)
            SetupRecording();
        else if (shouldReplay) SetupReplay();
    }

    private void Update()
    {
        if (Globals.Instance.levelComplete && currentlyRecording) FinishRecording();
        if (cameraTransform.localRotation != idealCameraRotation && playbackBegun &&
            currentFrameIndex + 1 != replayLength)
            cameraTransform.rotation =
                Quaternion.LerpUnclamped(cameraTransform.rotation, idealCameraRotation, Time.deltaTime * 10);
    }

    private void FixedUpdate()
    {
        if (currentlyRecording)
        {
            // Create a new frame of the replay
            ReplayFrame replayFrame = new();

            // Save new player transform data into frame if it has changed
            if (transform.hasChanged)
            {
                transform.hasChanged = false;
                replayFrame.Transform = transform;
            }

            // Save new camera transform data into frame if it has changed
            if (cameraTransform.hasChanged)
            {
                cameraTransform.hasChanged = false;
                replayFrame.CameraTransform = cameraTransform;
            }

            replayFrame.cameraState = playerMovement.cameraState;

            replayFrame.Write(dataWriter);
            replayLength++;
        }
        else if (shouldReplay && !playbackBegun && ghostDownloaded)
        {
            try
            {
                replayFrames = DecodeGhostData();
                playbackBegun = true;
                shouldReplay = false;
            }
            catch (InvalidDataException e)
            {
                Debug.Log($"Replay data incompatible or corrupt: {e.Message}");
                shouldReplay = false;
            }
        }
        else
        {
            switch (playbackBegun)
            {
                // Stop when we reach the end of the replay
                case true when currentFrameIndex + 1 != replayLength:
                    {
                        // Set player transform to data in current frame
                        replayFrames[currentFrameIndex].ExportTransform(transform);
                        // Update ideal camera rotation
                        if (replayFrames[currentFrameIndex].hasCameraTransform)
                            idealCameraRotation = Quaternion.Euler(replayFrames[currentFrameIndex].cameraRotation);
                        //Debug.Log(idealCameraRotation);
                        // We need to move on to the next frame's data
                        currentFrameIndex++;
                        break;
                    }
                case true:
                    playbackBegun = false;
                    LevelCompleteEvent?.Invoke();
                    break;
            }
        }
    }

    private event EmptyEvent LevelCompleteEvent;

    private void LevelComplete()
    {
        shouldRecord = false;
        shouldReplay = false;
        Globals.Instance.levelComplete = true;
        Thread.Sleep(1000);
        Globals.Instance.StartCoroutine(Globals.Instance.TriggerLoadingScreen(shouldPlayCutscene ? "EndCutscene" : "Main Menu"));
    }

    private void HandleCutsceneOverEvent(object sender, EventArgs e)
    {
        SetupRecording();
    }
    private void SetupRecording()
    {
        Debug.Log("Recording started!");
        ghostData = new MemoryStream();
        dataWriter = new BinaryWriter(ghostData);
        currentlyRecording = true;
    }

    private void SetupReplay()
    {
        GetComponent<PlayerMovement>().enabled = false;
        GetComponent<CharacterController>().enabled = false;
        GetComponentInChildren<MouseLookMainCharacter>().enabled = false;
        StartCoroutine(DownloadGhost());
    }

    private static MemoryStream Compress(Stream uncompressedData)
    {
        MemoryStream compressedData = new();
        uncompressedData.Seek(0, SeekOrigin.Begin);

        using GZipStream compressor = new(compressedData, CompressionLevel.Optimal, true);
        uncompressedData.CopyTo(compressor);
        compressor.Close();

        compressedData.Seek(0, SeekOrigin.Begin);
        return compressedData;
    }

    private static MemoryStream Decompress(Stream compressedData)
    {
        MemoryStream uncompressedData = new();
        using GZipStream decompress = new(compressedData, CompressionMode.Decompress, true);
        decompress.CopyTo(uncompressedData);
        return uncompressedData;
    }

    private ReplayFrame[] DecodeGhostData()
    {
        var totalSize = 0;
        List<ReplayFrame> frames = new();

        // Get size of footer (last int)
        var buffer = new byte[sizeof(int)];
        ghostData.Seek(-sizeof(int), SeekOrigin.End);
        ghostData.Read(buffer, 0, sizeof(int));

        // Seek to footer beginning
        ghostData.Seek(-BitConverter.ToInt32(buffer, 0), SeekOrigin.End);

        // Check footer begins with 0xFF marker
        if (ghostData.ReadByte() != 0xFF) throw new InvalidDataException("Footer does not start with expected byte");

        // Get replay length from footer (position 2)
        ghostData.Read(buffer, 0, sizeof(uint));
        replayLength = BitConverter.ToUInt32(buffer, 0);

        // Check version is in compatible range
        ghostData.Read(buffer, 0, sizeof(int));
        var dataVersion = BitConverter.ToUInt32(buffer, 0);
        if (dataVersion is not (>= MinimumVer and <= CurrentVer))
            throw new InvalidDataException("Version is not supported by this build of the game");

        // Reset position
        ghostData.Seek(0, SeekOrigin.Begin);

        for (var frameIndex = 0; frameIndex < replayLength; frameIndex++)
        {
            var frameSize = 2;

            if (ghostData.ReadByte() == 0x01)
            {
                frameSize += ReplayFrame.PositionSize + ReplayFrame.RotationSize;
                ghostData.Seek(ReplayFrame.PositionSize + ReplayFrame.RotationSize, SeekOrigin.Current);
            }

            if (ghostData.ReadByte() == 0x01) frameSize += ReplayFrame.CameraSize;

            ghostData.Seek(totalSize, SeekOrigin.Begin);
            totalSize += frameSize;

            var frameData = new byte[frameSize];
            ghostData.Read(frameData, 0, frameSize);

            ReplayFrame replayFrame = new(byteArray: frameData);
            frames.Add(replayFrame);
        }

        return frames.ToArray();
    }

    private void FinishRecording()
    {
        currentlyRecording = false;

        // Store original length
        var origDataSize = (uint)ghostData.Length;
        // Start marker (1 byte)
        dataWriter.Write((byte)0xFF);
        // Number of frames recorded (int32, 4 bytes)
        dataWriter.Write(replayLength);
        // Version code (int32, 4 bytes)
        dataWriter.Write(CurrentVer);
        // Reserved space (target 64 byte footer total)
        dataWriter.Write(new byte[48]);
        // Footer size (int32, 4 bytes)
        var footerSize = (uint)ghostData.Length - origDataSize + sizeof(uint);
        dataWriter.Write(footerSize);

        Debug.Log($"Final raw data size: {ghostData.Length / 1024}KB");
        compressedGhostData = Compress(ghostData);
        Debug.Log($"Final ghost size: {compressedGhostData.Length / 1024}KB");
        StartCoroutine(UploadGhost(compressedGhostData.ToArray(), Globals.Instance.playerID, SceneManager.GetActiveScene().buildIndex,
            replayLength));
    }

    private IEnumerator UploadGhost(byte[] ghostData, int playerId, int levelId, uint length)
    {
        var www = UnityWebRequest.Put($"{endpoint}/v1/submit-ghost", ghostData);
        www.SetRequestHeader("X-Player-Id", playerId.ToString());
        www.SetRequestHeader("X-Player-Secret", Globals.Instance.playerSecret.ToString());
        www.SetRequestHeader("X-Level-Id", levelId.ToString());
        www.SetRequestHeader("X-Replay-Length", length.ToString());
        yield return www.SendWebRequest();

        Debug.Log(www.result != UnityWebRequest.Result.Success
            ? www.error
            : $"Upload complete! {www.downloadHandler.text}");

        LevelCompleteEvent?.Invoke();
    }

    private IEnumerator DownloadGhost()
    {
        var www = UnityWebRequest.Get($"{endpoint}/v1/get-ghost/{ghostID}");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            compressedGhostData = new MemoryStream(www.downloadHandler.data, true);
            ghostData = Decompress(compressedGhostData);
            ghostDownloaded = true;
            Debug.Log($"Successfully downloaded {ghostData.Length / 1024}KB replay");
        }
    }

    private delegate void EmptyEvent();
}

public class ReplayFrame
{
    public const int PositionSize = 3 * sizeof(float);
    public const int RotationSize = 3 * sizeof(float);
    public const int CameraSize = 3 * sizeof(float);
    public PlayerMovement.CameraState cameraState;
    public Vector3 cameraRotation;
    public bool hasCameraTransform;
    private bool hasTransform;

    private Vector3 position, eulerAngles;

    // Class initializer, optional parameter is transform and camera transform
    public ReplayFrame(Transform tr = null, Transform cameraTr = null, byte[] byteArray = null, PlayerMovement.CameraState cameraSt = PlayerMovement.CameraState.Standard)
    {
        if (byteArray != null) AsByteArray = byteArray;
        if (tr) Transform = tr;
        if (cameraTr) CameraTransform = cameraTr;
        cameraState = cameraSt;
    }

    // Extracts the position and rotation data from a given transform,
    // we don't want the whole structure in order to reduce size
    public Transform Transform
    {
        set
        {
            position = value.position;
            eulerAngles = value.eulerAngles;
            hasTransform = true;
        }
    }

    // We only need the rotation of the camera
    public Transform CameraTransform
    {
        set
        {
            cameraRotation = value.eulerAngles;
            hasCameraTransform = true;
        }
    }

    private byte[] AsByteArray
    {
        get
        {
            var bytes = new byte[3];
            // Save cameraState as first byte
            bytes[0] = (byte) cameraState;
            if (hasTransform)
            {
                // Create byte array of correct size to convert floats into
                var transformBytes = new byte[1 + PositionSize + RotationSize];
                transformBytes[0] = 0x01;

                Buffer.BlockCopy(BitConverter.GetBytes(position.x), 0, transformBytes, 1 + 0 * sizeof(float),
                    sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(position.y), 0, transformBytes, 1 + 1 * sizeof(float),
                    sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(position.z), 0, transformBytes, 1 + 2 * sizeof(float),
                    sizeof(float));

                Buffer.BlockCopy(BitConverter.GetBytes(eulerAngles.x), 0, transformBytes, 1 + 3 * sizeof(float),
                    sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(eulerAngles.y), 0, transformBytes, 1 + 4 * sizeof(float),
                    sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(eulerAngles.z), 0, transformBytes, 1 + 5 * sizeof(float),
                    sizeof(float));

                // Create a byte array to merge our arrays into, -1 because we are overwriting the middle byte
                var newBytes = new byte[transformBytes.Length + bytes.Length - 1];
                // Starts array with all of transformBytes data, leaving 0x00 at the end
                Buffer.BlockCopy(transformBytes, 0, newBytes, 1, transformBytes.Length);
                // Overwrites old instance of bytes
                bytes = newBytes;
            }

            if (!hasCameraTransform) return bytes;
            {
                // Create byte array of correct size to convert floats into
                var cameraRotationBytes = new byte[1 + CameraSize];
                cameraRotationBytes[0] = 0x01;

                Buffer.BlockCopy(BitConverter.GetBytes(cameraRotation.x), 0, cameraRotationBytes, 1 + 0 * sizeof(float),
                    sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(cameraRotation.y), 0, cameraRotationBytes, 1 + 1 * sizeof(float),
                    sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(cameraRotation.z), 0, cameraRotationBytes, 1 + 2 * sizeof(float),
                    sizeof(float));

                // Create a byte array to merge our arrays into, -1 because we are overwriting the last byte
                var newBytes = new byte[cameraRotationBytes.Length + bytes.Length - 1];
                // Copies existing data into new array
                Buffer.BlockCopy(bytes, 0, newBytes, 0, bytes.Length);
                // Copies camera transform into array, overwriting the last byte
                Buffer.BlockCopy(cameraRotationBytes, 0, newBytes, bytes.Length - 1, cameraRotationBytes.Length);
                // Overwrites old instance of bytes
                bytes = newBytes;
            }

            // Logs frame in hexadecimal
            //Debug.Log(BitConverter.ToString(bytes));
            return bytes;
        }
        set
        {
            var cameraEulerAnglesBytes = new byte[CameraSize];

            cameraState = (PlayerMovement.CameraState) value[0];

            if (value[1] == 0x01)
            {
                hasTransform = true;

                // Create arrays to fill with split data
                var positionBytes = new byte[PositionSize];
                var rotationBytes = new byte[RotationSize];

                // Copy position and rotation into respective arrays
                Buffer.BlockCopy(value, 2, positionBytes, 0, PositionSize);
                Buffer.BlockCopy(value, 2 + PositionSize, rotationBytes, 0, RotationSize);

                // Unpack to Vector3s
                position.x = BitConverter.ToSingle(positionBytes, 0 * sizeof(float));
                position.y = BitConverter.ToSingle(positionBytes, 1 * sizeof(float));
                position.z = BitConverter.ToSingle(positionBytes, 2 * sizeof(float));

                eulerAngles.x = BitConverter.ToSingle(rotationBytes, 0 * sizeof(float));
                eulerAngles.y = BitConverter.ToSingle(rotationBytes, 1 * sizeof(float));
                eulerAngles.z = BitConverter.ToSingle(rotationBytes, 2 * sizeof(float));

                // If there is camera data after the player transform
                //Debug.Log(value[POSITION_SIZE + ROTATION_SIZE + 1]);
                if (value[PositionSize + RotationSize + 1] == 0x01)
                {
                    hasCameraTransform = true;
                    Buffer.BlockCopy(value, 3 + PositionSize + RotationSize, cameraEulerAnglesBytes, 0, CameraSize);
                }
            }
            // If there is only camera data (no player transform)
            else if (value[2] == 0x01)
            {
                hasCameraTransform = true;
                Buffer.BlockCopy(value, 3, cameraEulerAnglesBytes, 0, CameraSize);
            }

            // Unpack to Vector3
            if (!hasCameraTransform) return;
            cameraRotation.x = BitConverter.ToSingle(cameraEulerAnglesBytes, 0 * sizeof(float));
            cameraRotation.y = BitConverter.ToSingle(cameraEulerAnglesBytes, 1 * sizeof(float));
            cameraRotation.z = BitConverter.ToSingle(cameraEulerAnglesBytes, 2 * sizeof(float));
            // Logs frame in hexadecimal
            // Debug.Log(BitConverter.ToString(value));
        }
    }

    // Exports stored transform data into given structure
    public void ExportTransform(Transform tr)
    {
        // Do nothing if we don't have transform stored in this frame
        if (!hasTransform) return;
        tr.position = position;
        tr.eulerAngles = eulerAngles;
    }

    // Writes data into stream using given BinaryWriter
    public void Write(BinaryWriter dataWriter)
    {
        dataWriter.Write(AsByteArray);
    }
}