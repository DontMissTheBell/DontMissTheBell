using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        Dialogue = Globals.Instance.levelComplete ? Dialogue1 : Dialogue2;
        if (Globals.Instance.cutsceneActive)
        {
            if (Globals.Instance.runningLate)
            {
<<<<<<< HEAD
                Dialogue = Dialogue2;
            }
            else
            {
                Dialogue = Dialogue1;
=======
                StartCoroutine(StartCutscene());
            }
            else
            {
                TimerObject.SetActive(false);
                StartCoroutine(StartCutscene());
>>>>>>> 224353158309dae77285d9b989d504863b185da6
            }
            Dialogue.gameObject.SetActive(true);
            TimerObject.SetActive(false);
            StartCoroutine(StartCutscene()); 
        }
        else
        {
            Dialogue.gameObject.SetActive(false);

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
        Globals.Instance.EndCutscene();

        Dialogue.gameObject.SetActive(false);

        CutsceneCamera.enabled = false;

        TimerObject.SetActive(true);

        if (Globals.Instance.levelComplete)
        {
            Globals.Instance.StartCoroutine(Globals.Instance.TriggerLoadingScreen("Main Menu"));
        }
    }

}
