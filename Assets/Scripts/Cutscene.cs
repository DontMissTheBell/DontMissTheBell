using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cutscene : MonoBehaviour
{
    [SerializeField] private DialogueMain Dialogue1;
    [SerializeField] private DialogueMain Dialogue2;
    [SerializeField] private Camera CutsceneCamera;

    [SerializeField] private GameObject TimerObject;
    [SerializeField] private GameObject clickToStart;


    public bool writingDialogue;

    private DialogueMain Dialogue;


    private void Awake()
    {
        if (string.IsNullOrEmpty(Globals.Instance.replayToStart)) Globals.Instance.cutsceneActive = true;
    }

    private void Start()
    {
        if (Globals.Instance.cutsceneActive)
        {
            Dialogue = Globals.Instance.levelComplete
                ? Globals.Instance.runningLate ? Dialogue2 : Dialogue1
                : Globals.Instance.firstRun ? Dialogue2 : Dialogue1;
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
            for (var i = 0; i < Dialogue.DialogueBoxes.Length; i++)
            {
                writingDialogue = true;
                Dialogue.StartWritingText(i);

                while (writingDialogue) yield return null;
            }

        Dialogue.gameObject.SetActive(false);

        CutsceneCamera.enabled = false;

        if (Globals.Instance.skipCutscene)
        {
            clickToStart.SetActive(true);
            while (!Input.GetMouseButtonDown(0)) yield return null;
            clickToStart.SetActive(false);
        }

        Globals.Instance.firstRun = false;

        Globals.Instance.EndCutscene();

        TimerObject.SetActive(true);

        Globals.Instance.skipCutscene = false;


        if (SceneManager.GetActiveScene().name == "EndCutscene")
            Globals.Instance.StartCoroutine(Globals.Instance.TriggerLoadingScreen("Main Menu"));
    }
}