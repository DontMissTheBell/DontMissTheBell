using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Globals : MonoBehaviour
{
    // Loading screen
    private RectTransform loadingScreen;
    private const float LoadDuration = 1.0f;

    // Global variables
    public bool gamePaused;
    public bool levelComplete;

    public static Globals Instance { get; private set; }

    // Avoid duplication
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
        DontDestroyOnLoad(gameObject);

        loadingScreen = GameObject.FindGameObjectWithTag("LoadingScreen").GetComponent<RectTransform>();

        gamePaused = 0.0f == Time.timeScale;
    }

    // Transition manager
    public IEnumerator TriggerLoadingScreen(string sceneName)
    {
        loadingScreen.DORotate(Vector3.zero, LoadDuration);
        yield return new WaitForSeconds(LoadDuration);
        DOTween.KillAll();
        SceneManager.LoadScene(sceneName);
        yield return new WaitForSeconds(0.25f);
        loadingScreen.DORotate(Vector3.left * 90, LoadDuration);
        yield return new WaitForSeconds(LoadDuration);
    }
}