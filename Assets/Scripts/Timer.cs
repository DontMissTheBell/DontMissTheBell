using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public float TimeLimit;
    private float CurrentTime;
    private bool TimerStarted;
    [SerializeField] TMP_Text TimerText;

    private void Start()
    {
        CurrentTime = TimeLimit;
        TimerStarted = true;
    }

    private void Update()
    {
        if (TimerStarted)
        {
            CurrentTime -= Time.deltaTime;
            if (CurrentTime <= 0)
            {
                TimerStarted = false;
                CurrentTime = 0;
                Debug.Log("Lose");
            }
            TimerText.text = CurrentTime.ToString("F0");
        }
    }
}