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
        Globals.Instance.StartCoroutine(Globals.Instance.TriggerLoadingScreen("Level1"));
    }
}