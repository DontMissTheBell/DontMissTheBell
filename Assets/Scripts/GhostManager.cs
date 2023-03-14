using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.Networking;

public class GhostManager : MonoBehaviour
{
    private MemoryStream ghostData;
    private MemoryStream compressedGhostData;
    private BinaryWriter dataWriter;
    private bool currentlyRecording, playbackBegun, ghostDownloaded;
    private ReplayFrame[] replayFrames;
    private Vector3 lastCameraRotation;
    private Quaternion idealCameraRotation;
    private Int32 replayLength, currentFrameIndex = 0;
    // This is the camera object
    public Transform cameraTransform;
    // Enable this to make the script record for this object
    public bool shouldRecord;
    // Enable this to playback a ghost recording (DEBUG)
    public bool shouldReplay;
    public string ghostID;

    private void SetupRecording()
    {
        ghostData = new();
        dataWriter = new(ghostData);
        currentlyRecording = true;
    }
    private void SetupReplay()
    {
        StartCoroutine(DownloadGhost(ghostID));
    }
    private MemoryStream Compress(Stream uncompressedData)
    {
        MemoryStream compressedData = new();
        uncompressedData.Seek(0, SeekOrigin.Begin);

        using GZipStream compressor = new(compressedData, System.IO.Compression.CompressionLevel.Optimal, true);
        uncompressedData.CopyTo(compressor);

        compressedData.Seek(0, SeekOrigin.Begin);
        return compressedData;
    }
    private MemoryStream Decompress(MemoryStream compressedData)
    {
        MemoryStream uncompressedData = new();
        using GZipStream decompressor = new(compressedData, CompressionMode.Decompress, true);
        decompressor.CopyTo(uncompressedData);
        return uncompressedData;
    }
    private void Awake()
    {
        idealCameraRotation = cameraTransform.rotation;
        if (shouldRecord)
        {
            SetupRecording();
        }
        else if (shouldReplay)
        {
            SetupReplay();
        }
    }
    private void FixedUpdate()
    {
        if (currentlyRecording)
        {
            ReplayFrame replayFrame = new();
            if (transform.hasChanged)
            {
                transform.hasChanged = false;
                replayFrame.Transform = transform;
            }
            // hasChanged isn't working, probably becaue its a child object
            if (cameraTransform.localEulerAngles != lastCameraRotation)
            {
                lastCameraRotation = cameraTransform.localEulerAngles;
                replayFrame.CameraTransform = cameraTransform;
            }
            replayFrame.Write(dataWriter);
            replayLength++;
        }
        else if (shouldReplay && !playbackBegun && ghostDownloaded)
        {
            replayFrames = DecodeGhostData();
            playbackBegun = true;
        }
        else if (playbackBegun && currentFrameIndex + 1 != replayLength) // Stop when we reach the end of the replay
        {
            // Set player transform to data in current frame
            replayFrames[currentFrameIndex].ExportTransform(transform);
            // Update ideal camera rotation
            if (replayFrames[currentFrameIndex].hasCameraTransform)
            {
                idealCameraRotation = Quaternion.Euler(replayFrames[currentFrameIndex].cameraRotation);
            }

            // We need to move on to the next frame's data
            currentFrameIndex++;
        }
    }
    private void Update()
    {
        if (Globals.instance.levelComplete && currentlyRecording)
        {
            FinishRecording();
        }
        if (cameraTransform.localRotation != idealCameraRotation)
        {
            cameraTransform.localRotation = Quaternion.LerpUnclamped(cameraTransform.localRotation, idealCameraRotation, Time.deltaTime * 2);
        }
    }
    private ReplayFrame[] DecodeGhostData()
    {
        int totalSize = 0;
        List<ReplayFrame> frames = new();

        ghostData.Seek(-sizeof(Int32), SeekOrigin.End);
        byte[] replayLengthBytes = new byte[sizeof(Int32)];
        ghostData.Read(replayLengthBytes, 0, sizeof(Int32));
        replayLength = BitConverter.ToInt32(replayLengthBytes, 0);
        //Debug.Log($"Replay length: {replayLength}");

        ghostData.Seek(0, SeekOrigin.Begin);

        for (int frameIndex = 0; frameIndex < replayLength; frameIndex++)
        {
            int frameSize = 2;

            if (ghostData.ReadByte() == 0x01)
            {
                frameSize += ReplayFrame.POSITION_SIZE + ReplayFrame.ROTATION_SIZE;
                ghostData.Seek(ReplayFrame.POSITION_SIZE + ReplayFrame.ROTATION_SIZE, SeekOrigin.Current);

            }
            if (ghostData.ReadByte() == 0x01)
            {
                frameSize += ReplayFrame.CAMERA_SIZE;
            }

            ghostData.Seek(totalSize, SeekOrigin.Begin);
            totalSize += frameSize;

            byte[] frameData = new byte[frameSize];
            ghostData.Read(frameData, 0, frameSize);

            ReplayFrame replayFrame = new(byteArray: frameData);
            frames.Add(replayFrame);
        }
        return frames.ToArray();
    }
    public void FinishRecording()
    {
        currentlyRecording = false;
        dataWriter.Write(replayLength);
        Debug.Log($"Final raw data size: {ghostData.Length / 1024}KB");
        compressedGhostData = Compress(ghostData);
        Debug.Log($"Final ghost size: {compressedGhostData.Length / 1024}KB");
        StartCoroutine(UploadGhost(compressedGhostData.ToArray()));
    }
    private IEnumerator UploadGhost(byte[] ghostData)
    {
        UnityWebRequest www = UnityWebRequest.Put("https://dmtb.catpowered.net/api/v1/submit-ghost", ghostData);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log($"Upload complete! {www.downloadHandler.text}");
        }
    }
    private IEnumerator DownloadGhost(string ghostID)
    {
        UnityWebRequest www = UnityWebRequest.Get($"https://dmtb.catpowered.net/api/v1/get-ghost/{ghostID}");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            compressedGhostData = new(www.downloadHandler.data, true);
            ghostData = Decompress(compressedGhostData);
            ghostDownloaded = true;
            Debug.Log($"Successfully downloaded {ghostData.Length / 1024}KB replay");
        }
    }
}

public class ReplayFrame
{
    public const int POSITION_SIZE = 3 * sizeof(float);
    public const int ROTATION_SIZE = 3 * sizeof(float);
    public const int CAMERA_SIZE = 3 * sizeof(float);
    private Vector3 position, eulerAngles;
    public Vector3 cameraRotation;
    public bool hasTransform, hasCameraTransform;
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
            cameraRotation = value.localEulerAngles;
            hasCameraTransform = true;
        }
    }
    public byte[] AsByteArray
    {
        get
        {
            byte[] bytes = new byte[2];
            if (hasTransform)
            {
                // Create byte array of correct size to convert floats into
                byte[] transformBytes = new byte[1 + POSITION_SIZE + ROTATION_SIZE];
                transformBytes[0] = 0x01;

                Buffer.BlockCopy(BitConverter.GetBytes(position.x), 0, transformBytes, 1 + 0 * sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(position.y), 0, transformBytes, 1 + 1 * sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(position.z), 0, transformBytes, 1 + 2 * sizeof(float), sizeof(float));

                Buffer.BlockCopy(BitConverter.GetBytes(eulerAngles.x), 0, transformBytes, 1 + 3 * sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(eulerAngles.y), 0, transformBytes, 1 + 4 * sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(eulerAngles.z), 0, transformBytes, 1 + 5 * sizeof(float), sizeof(float));

                // Create a byte array to merge our arrays into, -1 because we are overwriting the first byte
                byte[] newBytes = new byte[transformBytes.Length + bytes.Length - 1];
                // Starts array with all of transformBytes data, leaving 0x00 at the end
                Buffer.BlockCopy(transformBytes, 0, newBytes, 0, transformBytes.Length);
                // Overwrites old instance of bytes
                bytes = newBytes;
            }
            if (hasCameraTransform)
            {
                // Create byte array of correct size to convert floats into
                byte[] cameraRotationBytes = new byte[1 + CAMERA_SIZE];
                cameraRotationBytes[0] = 0x01;

                Buffer.BlockCopy(BitConverter.GetBytes(cameraRotation.x), 0, cameraRotationBytes, 1 + 0 * sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(cameraRotation.y), 0, cameraRotationBytes, 1 + 1 * sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(cameraRotation.z), 0, cameraRotationBytes, 1 + 2 * sizeof(float), sizeof(float));

                // Create a byte array to merge our arrays into, -1 because we are overwriting the last byte
                byte[] newBytes = new byte[cameraRotationBytes.Length + bytes.Length - 1];
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
            byte[] cameraEulerAnglesBytes = new byte[CAMERA_SIZE];

            if (value[0] == 0x01)
            {
                hasTransform = true;

                byte[] positionBytes = new byte[POSITION_SIZE];
                byte[] rotationBytes = new byte[ROTATION_SIZE];
                Buffer.BlockCopy(value, 1, positionBytes, 0, POSITION_SIZE);
                Buffer.BlockCopy(value, 1 + POSITION_SIZE, rotationBytes, 0, ROTATION_SIZE);

                position.x = BitConverter.ToSingle(positionBytes, 0 * sizeof(float));
                position.y = BitConverter.ToSingle(positionBytes, 1 * sizeof(float));
                position.z = BitConverter.ToSingle(positionBytes, 2 * sizeof(float));

                eulerAngles.x = BitConverter.ToSingle(rotationBytes, 0 * sizeof(float));
                eulerAngles.y = BitConverter.ToSingle(rotationBytes, 1 * sizeof(float));
                eulerAngles.z = BitConverter.ToSingle(rotationBytes, 2 * sizeof(float));

                if (value[POSITION_SIZE + ROTATION_SIZE] == 0x01)
                {
                    hasCameraTransform = true;
                    Buffer.BlockCopy(value, 2 + POSITION_SIZE + ROTATION_SIZE, cameraEulerAnglesBytes, 0, CAMERA_SIZE);
                };
            }
            else if (value[1] == 0x01)
            {
                hasCameraTransform = true;
                Buffer.BlockCopy(value, 2, cameraEulerAnglesBytes, 0, CAMERA_SIZE);
            }
            if (hasCameraTransform)
            {
                cameraRotation.x = BitConverter.ToSingle(cameraEulerAnglesBytes, 0 * sizeof(float));
                cameraRotation.y = BitConverter.ToSingle(cameraEulerAnglesBytes, 1 * sizeof(float));
                cameraRotation.z = BitConverter.ToSingle(cameraEulerAnglesBytes, 2 * sizeof(float));
            }
            // Logs frame in hexadecimal
            // Debug.Log(BitConverter.ToString(value));
        }
    }
    // Class initialiser, optional parameter is transform and camera transform
    public ReplayFrame(Transform tr = null, Transform cameraTr = null, byte[] byteArray = null)
    {
        if (byteArray != null)
        {
            AsByteArray = byteArray;
        }
        if (tr)
        {
            Transform = tr;
        }
        if (cameraTr)
        {
            CameraTransform = cameraTr;
        }
    }
    // Exports stored transform data into given structure
    public Transform ExportTransform(Transform tr)
    {
        if (hasTransform)
        {
            tr.position = position;
            tr.eulerAngles = eulerAngles;
        }
        return tr;
    }
    // Writes data into stream using given BinaryWriter
    public void Write(BinaryWriter dataWriter)
    {
        dataWriter.Write(AsByteArray);
    }
}