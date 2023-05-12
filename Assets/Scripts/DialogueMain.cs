using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogueMain : MonoBehaviour
{
    [SerializeField] private float delay;

    [SerializeField] private Cutscene Cutscene;

    [SerializeField] private GameObject clickToSkip;

    public GameObject[] DialogueBoxes;

    private GameObject currentAvatar;
    private GameObject lastAvatar;

    public void StartWritingText(int index)
    {
        DialogueBoxes[index].SetActive(true);

        clickToSkip.SetActive(true);


        if (index > 0) lastAvatar = currentAvatar;


        currentAvatar = DialogueBoxes[index].GetComponent<DialogueBox>().Avatar;

        if (index > 0 && lastAvatar != currentAvatar) lastAvatar.SetActive(false);

        currentAvatar.SetActive(true);

        StartCoroutine(WriteText(DialogueBoxes[index].GetComponent<Text>()));
    }

    private IEnumerator WriteText(Text textDisplay)
    {
        var input = textDisplay.text;

        var skipBox = false;

        textDisplay.text = "";
        foreach (var t in input)
        {
            while (Globals.Instance.gamePaused) yield return null;

            textDisplay.text += t;

            if (Input.GetMouseButton(1) || Input.GetMouseButtonDown(0)) skipBox = true;

            if (!skipBox) yield return new WaitForSeconds(delay);
        }

        yield return new WaitForSeconds(delay);

        //clickToSkip.SetActive(false);

        while (!(Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))) yield return null;

        while (Globals.Instance.gamePaused) yield return null;

        Cutscene.writingDialogue = false;

        textDisplay.text = "";
    }
}