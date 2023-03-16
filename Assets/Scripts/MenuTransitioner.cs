using Cinemachine;
using UnityEngine;

public class MenuTransitioner : MonoBehaviour
{
    public CinemachineVirtualCamera currentCamera;

    // Start is called before the first frame update
    private void Start()
    {
        currentCamera.Priority++;
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