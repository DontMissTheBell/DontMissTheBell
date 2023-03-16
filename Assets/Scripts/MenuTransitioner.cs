using Cinemachine;
using UnityEngine;

public class MenuTransitioner : MonoBehaviour
{
    private CinemachineVirtualCamera currentCamera;

    // Start is called before the first frame update
    private void Start()
    {
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
        Destroy(gameObject);
        Globals.LoadScene("Test", 0.5f);
    }
}