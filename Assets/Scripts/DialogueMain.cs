using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueMain : MonoBehaviour
{

    [SerializeField] private float delay;

    [SerializeField] private Cutscene Cutscene;

    [SerializeField] GameObject clickToSkip;

    private GameObject currentAvatar;
    private GameObject lastAvatar;

    public GameObject[] DialogueBoxes;

    public void StartWritingText(int index)
    {
        DialogueBoxes[index].SetActive(true);

        clickToSkip.SetActive(true);


        if (index > 0) 
        {
            lastAvatar = currentAvatar;
        }
        

        currentAvatar = DialogueBoxes[index].GetComponent<DialogueBox>().Avatar;

        if (index > 0 && lastAvatar != currentAvatar)
        {
        lastAvatar.SetActive(false);
        }

        currentAvatar.SetActive(true);

        StartCoroutine(WriteText(DialogueBoxes[index].GetComponent<Text>()));
    }

    public IEnumerator WriteText(Text textDisplay)
    {
        string input = textDisplay.text;

        bool skipBox = false;

        textDisplay.text = "";
        for (int i = 0; i < input.Length; i++)
        {
            textDisplay.text += input[i];

            if (Input.GetMouseButton(1))
            {
                skipBox = true;
            }

            if (!skipBox)
            {
            yield return new WaitForSeconds(delay);
            }
        }

        yield return new WaitForSeconds(delay);

        clickToSkip.SetActive(false);

        while(!Input.GetMouseButtonDown(0))
        {
            yield return null;
        }

        Cutscene.writingDialogue = false;

        textDisplay.text = "";        
    }  

}
