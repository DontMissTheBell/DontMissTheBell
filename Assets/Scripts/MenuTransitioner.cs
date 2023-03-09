using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;

public class MenuTransitioner : MonoBehaviour
{
    public CinemachineVirtualCamera currentCamera;
    // Start is called before the first frame update
    void Start()
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
        GameManager.LoadScene("Test", 0.5f);
    }
}

