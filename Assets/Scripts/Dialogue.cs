using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour
{
    [SerializeField] private float delay;
    [SerializeField] private string input;
    private Text textDisplay;


    private void Awake()
    {
        textDisplay = GetComponent<Text>();

        StartCoroutine(WriteText(input, textDisplay));
    }

    private IEnumerator WriteText(string input, Text textDisplay)
    {
        for (int i = 0; i < input.Length; i++)
        {
            textDisplay.text += input[i];
            yield return new WaitForSeconds(delay);
        }
    }             

}
