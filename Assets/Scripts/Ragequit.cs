using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Ragequit : MonoBehaviour
{
    public void Rage(bool loadMenu)
    {
        Time.timeScale = 1.0f;
        Globals.Instance.StartCoroutine(Globals.Instance.TriggerLoadingScreen(loadMenu ? "Main Menu" : SceneManager.GetActiveScene().name));
    }
}
