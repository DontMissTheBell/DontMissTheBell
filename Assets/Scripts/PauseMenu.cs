using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PauseMenu : MonoBehaviour
{

    [SerializeField] GameObject pressEscText;

    // Update is called once per frame

    private void Awake()
    {
    }
    private void Update()
    {
        if (!Globals.HasUsedPause && !Globals.Instance.cutsceneActive) 
        {
            pressEscText.SetActive(true);
        }
        // If Pause button pressed
        if (Input.GetButtonDown("Cancel") && !Globals.Instance.levelComplete)
        {
            Time.timeScale = 1.0f - Time.timeScale;
            Globals.Instance.gamePaused = 0.0f == Time.timeScale;
            gameObject.GetComponentInChildren<Canvas>(true).enabled = 0.0f == Time.timeScale;
            Cursor.lockState = Globals.Instance.gamePaused ? CursorLockMode.None : CursorLockMode.Locked;

            pressEscText.SetActive(false);
            

            Globals.HasUsedPause = true;
        }
            
    }
}