using System;
using System.Collections;
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
    private Vector3 lastCameraRotate;
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

        using (GZipStream compressor = new(compressedData, System.IO.Compression.CompressionLevel.Optimal, true))
        {
            uncompressedData.CopyTo(compressor);
        }

        compressedData.Seek(0, SeekOrigin.Begin);
        return compressedData;
    }
    private void Awake()
    {
        if (shouldRecord)
        {
            SetupRecording();
        } else if (shouldReplay)
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
            if (cameraTransform.eulerAngles != lastCameraRotate)
            {
                lastCameraRotate = cameraTransform.eulerAngles;
                replayFrame.CameraTransform = cameraTransform;
            }
            replayFrame.Write(dataWriter);
        } else if (shouldReplay && !playbackBegun && ghostDownloaded)
        {
             //if (ghostData.Read(0x00) == 0x01)
            //{

            //} else if (ghostData.Read(1) == 0x02)
            //{

            //}
        } else if (playbackBegun)
        {

        }
    }
    private void Update()
    {
        if (Globals.instance.levelComplete && currentlyRecording)
        {
            FinishRecording();
        }
    }
    public void FinishRecording()
    {
        currentlyRecording = false;
        compressedGhostData = new MemoryStream();
        Debug.Log($"Final raw data size: {ghostData.Length / 1024}KB");
        compressedGhostData = Compress(ghostData);
        Debug.Log($"Final ghost size: {compressedGhostData.Length}B");
        StartCoroutine(UploadGhost(compressedGhostData.ToArray()));
    }
    private IEnumerator UploadGhost(byte[] ghostData)
    {
        UnityWebRequest www = UnityWebRequest.Put("http://localhost:8080/api/v1/submit-ghost", ghostData);
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
        UnityWebRequest www = UnityWebRequest.Get($"http://localhost:8080/api/v1/get-ghost/{ghostID}");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            ghostData = new(www.downloadHandler.data);
            ghostDownloaded = true;
            Debug.Log($"Successfully downloaded {ghostData.Length / 1024}KB replay"); 
        }
    }
}

public class ReplayFrame
{
    private Vector3 position, eulerAngles, cameraEulerAngles;
    private bool hasTransform, hasCameraTransform;
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
    public Transform CameraTransform {
        set {
            cameraEulerAngles = value.eulerAngles;
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
                byte[] transformBytes = new byte[1 + (sizeof(float) * 6)];
                transformBytes[0] = 0x01;
                Buffer.BlockCopy(BitConverter.GetBytes(position.x), 0, transformBytes, 1 + 0 * sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(position.y), 0, transformBytes, 1 + 1 * sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(position.z), 0, transformBytes, 1 + 2 * sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(eulerAngles.x), 0, transformBytes, 1 + 3 * sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(eulerAngles.y), 0, transformBytes, 1 + 4 * sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(eulerAngles.z), 0, transformBytes, 1 + 5 * sizeof(float), sizeof(float));
                byte[] newBytes = new byte[transformBytes.Length + bytes.Length];
                Buffer.BlockCopy(transformBytes, 0, newBytes, 0, transformBytes.Length);
                Buffer.BlockCopy(bytes, 1, newBytes, transformBytes.Length, bytes.Length - 1);
                bytes = newBytes;
            }
            if (hasCameraTransform)
            {
                byte[] cameraTransformBytes = new byte[1 + (sizeof(float) * 3)];
                cameraTransformBytes[0] = 0x01;
                Buffer.BlockCopy(BitConverter.GetBytes(cameraEulerAngles.x), 0, cameraTransformBytes, 1 + 0 * sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(cameraEulerAngles.y), 0, cameraTransformBytes, 1 + 1 * sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(cameraEulerAngles.z), 0, cameraTransformBytes, 1 + 2 * sizeof(float), sizeof(float));
                byte[] newBytes = new byte[cameraTransformBytes.Length + bytes.Length];
                Buffer.BlockCopy(bytes, 0, newBytes, 0, bytes.Length);
                Buffer.BlockCopy(cameraTransformBytes, 0, newBytes, bytes.Length - 1, cameraTransformBytes.Length);
                bytes = newBytes;
            }
            // Debug.Log(BitConverter.ToString(bytes));
            return bytes;
        }
    }
    // Class initialiser, optional parameter is transform and camera transform
    public ReplayFrame(Transform tr = null, Transform cameraTr = null)
    {
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