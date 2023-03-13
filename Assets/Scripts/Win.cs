using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Win : MonoBehaviour
{
    public GameObject homework;
    Objectives objectives;
    public Timer timer;

    private void Start()
    {
        objectives = homework.GetComponent<Objectives>();
    }
    private void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.name == "MainCharacter" && objectives.HomeworkCompleted)
        {
            Debug.Log("Win");
            Time.timeScale = 0.0f;
            Globals.instance.gamePaused = true;
            Globals.instance.levelComplete = true;
            
            //output 
        }
    }
}


