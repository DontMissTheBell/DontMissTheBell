using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // If Pause button pressed
        if (Input.GetButtonDown("Cancel")) {
            Time.timeScale = 1.0f - Time.timeScale;
            Globals.instance.gamePaused = 0.0f == Time.timeScale;
            gameObject.GetComponentInChildren<Canvas>(includeInactive: true).enabled = 0.0f == Time.timeScale;
        }
    }
}
