using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class LevelSelector : MonoBehaviour
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
    public void BTmain()
    {
        Globals.Instance.StartCoroutine(Globals.Instance.TriggerLoadingScreen("Main Menu"));
    }
    public void Test()
    {
        Globals.Instance.StartCoroutine(Globals.Instance.TriggerLoadingScreen("Test"));
    }
    public void Exit()
    {
        Application.Quit();
    }



}
