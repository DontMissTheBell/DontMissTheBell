using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Win : MonoBehaviour
{
    public GameObject homework;
    public Timer timer;
    private Objectives objectives;

    [SerializeField] private GameObject getHomework;

    private bool displayingHomeworkText = false;

    private void Start()
    {
        objectives = homework.GetComponent<Objectives>();
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.name == "MainCharacter" && objectives.homeworkCompleted)
        {
            Debug.Log("Win");
            Globals.Instance.gamePaused = true;
            Globals.Instance.levelComplete = true;

            //output 
        }
        if (collider.gameObject.name == "MainCharacter" && !objectives.homeworkCompleted && !displayingHomeworkText)
        {
            StartCoroutine(GetHomeworkText());
        }
    }

    private IEnumerator GetHomeworkText()
    {
        getHomework.SetActive(true);

        displayingHomeworkText = true;

        yield return new WaitForSeconds(5);

        getHomework.SetActive(false);

        displayingHomeworkText = false;
    }
}