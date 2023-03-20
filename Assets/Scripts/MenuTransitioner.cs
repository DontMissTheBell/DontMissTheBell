using Cinemachine;
using UnityEngine;

public class MenuTransitioner : MonoBehaviour
{
    private CinemachineVirtualCamera currentCamera;

    // Start is called before the first frame update
    private void Start()
    {
        Application.targetFrameRate = 60;
        // Finds default camera
        currentCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CinemachineBrain>()
            .ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineVirtualCamera>();
    }


    public void UpdateCamera(CinemachineVirtualCamera target)
    {
        currentCamera.Priority--;

        currentCamera = target;

        currentCamera.Priority++;
    }

    public void StartGame()
    {
        Globals.Instance.StartCoroutine(Globals.Instance.TriggerLoadingScreen("Test"));
    }
}