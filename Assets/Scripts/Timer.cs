using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public float timeLimit;

    [SerializeField] private TMP_Text timerText;

    private float currentTime;

    private void Start()
    {
        currentTime = timeLimit;
    }

    private void Update()
    {
        if (!Globals.Instance.levelComplete && currentTime != 0)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0)
            {
                currentTime = 0;
                Debug.Log("Lose");
            }

            timerText.text = currentTime.ToString("F2");
        }
    }
}