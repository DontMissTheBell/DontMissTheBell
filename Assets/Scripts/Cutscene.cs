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
    [SerializeField] GameObject clickToStart;


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
        if (!Globals.Instance.skipCutscene)
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
        }

        Dialogue.gameObject.SetActive(false);

        CutsceneCamera.enabled = false;

        if (Globals.Instance.skipCutscene)
        {
            clickToStart.SetActive(true);
            while(!Input.GetMouseButtonDown(0))
            {
                yield return null;
            }
            clickToStart.SetActive(false);
        }

        Globals.Instance.firstRun = false;

        Globals.Instance.EndCutscene();

        TimerObject.SetActive(true);

        Globals.Instance.skipCutscene = false;



        if (SceneManager.GetActiveScene().name == "EndCutscene")
        {
            Globals.Instance.StartCoroutine(Globals.Instance.TriggerLoadingScreen("Main Menu")); 
        }
    }

}
