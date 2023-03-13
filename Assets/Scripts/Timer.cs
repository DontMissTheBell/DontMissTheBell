using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public float TimeLimit;
    private float CurrentTime;
    [SerializeField] TMP_Text TimerText;

    private void Start()
    {
        CurrentTime = TimeLimit;
    }

    private void Update()
    {
        if (!Globals.instance.levelComplete && CurrentTime != 0)
        {
            CurrentTime -= Time.deltaTime;
            if (CurrentTime <= 0)
            {
                CurrentTime = 0;
                Debug.Log("Lose");
            }
            TimerText.text = CurrentTime.ToString("F2");
        }
    }
}