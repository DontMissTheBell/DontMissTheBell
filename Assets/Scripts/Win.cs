using UnityEngine;

public class Win : MonoBehaviour
{
    public GameObject homework;
    public Timer timer;
    private Objectives objectives;

    private void Start()
    {
        objectives = homework.GetComponent<Objectives>();
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.name == "MainCharacter" && objectives.homeworkCompleted)
        {
            Debug.Log("Win");
            Time.timeScale = 0.0f;
            Globals.Instance.gamePaused = true;
            Globals.Instance.levelComplete = true;

            //output 
        }
    }
}