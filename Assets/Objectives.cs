using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Objectives : MonoBehaviour
{
    public bool HomeworkCompleted;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "MainCharacter")
        {
            HomeworkCompleted = true;
        }
    }
}
