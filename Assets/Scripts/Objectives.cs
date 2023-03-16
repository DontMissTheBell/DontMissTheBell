using UnityEngine;
using UnityEngine.Serialization;

public class Objectives : MonoBehaviour
{
    [FormerlySerializedAs("HomeworkCompleted")]
    public bool homeworkCompleted;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "MainCharacter")
        {
            homeworkCompleted = true;
            Debug.Log("Homework Obtained");
        }
    }
}