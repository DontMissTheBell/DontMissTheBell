using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ragequit : MonoBehaviour
{
    public void LoadMenu()
    {
        Time.timeScale = 1.0f;
        Globals.Instance.StartCoroutine(Globals.Instance.TriggerLoadingScreen("Main Menu"));
    }
}
