using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        // If Pause button pressed
        if (Input.GetButtonDown("Cancel") && !Globals.Instance.levelComplete)
        {
            Time.timeScale = 1.0f - Time.timeScale;
            Globals.Instance.gamePaused = 0.0f == Time.timeScale;
            gameObject.GetComponentInChildren<Canvas>(true).enabled = 0.0f == Time.timeScale;
            if (Time.timeScale == 1.0f)
                Cursor.lockState = CursorLockMode.Locked;
            else
                Cursor.lockState = CursorLockMode.None; 
        }
    }
}