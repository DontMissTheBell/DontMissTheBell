using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cutscene : MonoBehaviour
{

    [SerializeField] DialogueMain Dialogue; 
    [SerializeField] Camera CutsceneCamera;

    [SerializeField] GameObject TimerObject;

    public bool writingDialogue;
    


    void Start()
    {
        Globals.Instance.cutsceneActive = true;

        TimerObject.SetActive(false);

        StartCoroutine(StartCutscene());
        
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
        Globals.Instance.cutsceneActive = false;

        Dialogue.gameObject.SetActive(false);

        CutsceneCamera.enabled = false;

        TimerObject.SetActive(true);
    }

}
