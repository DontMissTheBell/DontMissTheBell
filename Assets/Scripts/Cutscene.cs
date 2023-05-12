using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cutscene : MonoBehaviour
{

    DialogueMain Dialogue;
    [SerializeField] DialogueMain Dialogue1; 
    [SerializeField] DialogueMain Dialogue2; 
    [SerializeField] Camera CutsceneCamera;

    [SerializeField] GameObject TimerObject;


    public bool writingDialogue;


    void Awake()
    {
        if (string.IsNullOrEmpty(Globals.Instance.replayToStart))
        {
            Globals.Instance.cutsceneActive = true;
        }

    }
    void Start()
    {
        if (Globals.Instance.cutsceneActive)
        {
            if (Globals.Instance.runningLate)
            {
                Dialogue = Dialogue2;
            }
            else
            {
                Dialogue = Dialogue1;
            }

            if (Globals.Instance.firstRun)
            {
                Dialogue = Dialogue2;
            }
            else
            {
                Dialogue = Dialogue1;
            }
            Dialogue.gameObject.SetActive(true);
            TimerObject.SetActive(false);
            StartCoroutine(StartCutscene()); 
        }
        else
        {
            CutsceneCamera.enabled = false;

            TimerObject.SetActive(true);
        }
    }

    private IEnumerator StartCutscene()
    {
        for(int i=0; i < Dialogue.DialogueBoxes.Length; i++)
        {
            writingDialogue = true;
            Dialogue.StartWritingText(i);

            while (writingDialogue)
            {
                yield return null;
            }

        }
        Globals.Instance.firstRun = false;

        Globals.Instance.EndCutscene();

        Dialogue.gameObject.SetActive(false);

        CutsceneCamera.enabled = false;

        TimerObject.SetActive(true);



        if (SceneManager.GetActiveScene().name == "EndCutscene")
        {
            Globals.Instance.StartCoroutine(Globals.Instance.TriggerLoadingScreen("Main Menu")); 
        }
    }

}
