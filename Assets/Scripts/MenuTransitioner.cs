using System;
using Cinemachine;
using TMPro;
using UnityEngine;

public class MenuTransitioner : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera currentCamera;
    [SerializeField] protected CinemachineVirtualCamera optionsCamera;
    public GameObject usernamePanel;
    public TextMeshProUGUI usernamePlaceholderText;

    // Start is called before the first frame update
    private void Start()
    {
        Application.targetFrameRate = 60;
        if (string.IsNullOrWhiteSpace(Globals.Username))
        {
            UpdateCamera(optionsCamera);
            ToggleUsernamePanel(true);
        }
    }

    public void ToggleUsernamePanel(bool toggle)
    {
        usernamePanel.SetActive(toggle);
        usernamePlaceholderText.text = Globals.Username;
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