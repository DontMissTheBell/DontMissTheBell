using UnityEngine;

public class Objectives : MonoBehaviour
{
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