using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Globals : MonoBehaviour
{
    public static Globals instance { get; private set; }
    // Avoid duplication
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        gamePaused = 0.0f == Time.timeScale;
    }

    // Global variables
    public bool gamePaused;
    public bool levelComplete;
}
