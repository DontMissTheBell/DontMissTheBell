using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Win : MonoBehaviour
{
    public GameObject homework;
    Objectives objectives;

    private void Start()
    {
        objectives = homework.GetComponent<Objectives>();
    }
    private void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.name == "MainCharacter" && objectives.HomeworkCompleted)
        {
                Debug.Log("Win");
               //output 
        }
    }
}


