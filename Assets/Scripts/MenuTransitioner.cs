using Cinemachine;
using UnityEngine;

public class MenuTransitioner : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera currentCamera;

    // Start is called before the first frame update
    private void Start()
    {
        Application.targetFrameRate = 60;
    }


    public void UpdateCamera(CinemachineVirtualCamera target)
    {
        currentCamera.Priority--;

        currentCamera = target;

        currentCamera.Priority++;
    }

    public void StartGame()
    {
        Globals.Instance.StartCoroutine(Globals.Instance.TriggerLoadingScreen("LSelector"));
    }
    public void BTmain()
    {
        Globals.Instance.StartCoroutine(Globals.Instance.TriggerLoadingScreen("Main Menu"));
    }
    public void Exit()
    {
        Application.Quit();
    }

}