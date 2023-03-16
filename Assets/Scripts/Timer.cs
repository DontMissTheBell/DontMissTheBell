using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class Timer : MonoBehaviour
{
    [FormerlySerializedAs("TimeLimit")] public float timeLimit;

    [FormerlySerializedAs("TimerText")] [SerializeField]
    private TMP_Text timerText;

    private float currentTime;

    private void Start()
    {
        currentTime = timeLimit;
    }

    private void Update()
    {
        if (!Globals.instance.levelComplete && currentTime != 0)
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