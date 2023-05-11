using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public float minuteLimit = 5; // 5 minutes = late

    [SerializeField] private TMP_Text timerText;

    private float currentTime;

    private void Update()
    {
        if (!Globals.Instance.levelComplete && !Globals.Instance.runningLate)
        {
            currentTime += Time.deltaTime;
            if (currentTime >= minuteLimit * 60) 
            {
                Debug.Log("Lose");
                Globals.Instance.runningLate = true;
            }

            timerText.text = currentTime >= minuteLimit * 60 ? "Late" : currentTime.ToString("F2");
        }
    }
}