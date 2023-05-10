using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Globals : MonoBehaviour
{
    private const float LoadDuration = 1.0f;

    private static Globals _instance;

    // Debug API toggle
    [SerializeField] private bool devAPI;

    // Global variables
    public bool gamePaused;
    public bool levelComplete;
    public bool gameResumed;

    public bool cutsceneActive;
    public event EventHandler CutsceneOver;

    public string replayToStart;


    // Loading screen
    private RectTransform loadingScreen;
    public string APIEndpoint => devAPI ? "http://localhost:8080/api" : "https://dmtb.catpowered.net/api";

    public static Globals Instance
    {
        get
        {
            if (!_instance)
                _instance = FindObjectOfType(typeof(Globals)) as Globals;
            if (_instance) return _instance;

            var obj = new GameObject("GlobalsHost");
            _instance = obj.AddComponent<Globals>();
            DontDestroyOnLoad(obj);

            return _instance;
        }
    }

    // Avoid duplication
    private void Awake()
    {
        if (Instance != this) Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
        loadingScreen = GameObject.FindGameObjectWithTag("LoadingScreen").GetComponent<RectTransform>();

        gamePaused = 0.0f == Time.timeScale;
        gameResumed = 1.0f == Time.timeScale;
    }

    public void EndCutscene()
    {
        cutsceneActive = false;
        CutsceneOver?.Invoke(this, EventArgs.Empty);
    }

    // Transition manager
    public IEnumerator TriggerLoadingScreen(string sceneName = "", int sceneId = -1)
    {
        // Start animation
        loadingScreen.DORotate(Vector3.zero, LoadDuration);
        yield return new WaitForSeconds(LoadDuration);
        DOTween.KillAll();

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;

        // Reset level-relative variables
        gamePaused = false;
        levelComplete = false;

        if (sceneId >= 0)
        {
            // Convert BuildIndex (levelId) to scene name
            sceneName = SceneUtility.GetScenePathByBuildIndex(sceneId);
            var slashLocation = sceneName.LastIndexOf('/');
            sceneName = sceneName[slashLocation..];
            var dotLocation = sceneName.LastIndexOf('.');
            sceneName = sceneName[..dotLocation];
            SceneManager.LoadSceneAsync(sceneId);
        }
        else
        {
            SceneManager.LoadSceneAsync(sceneName);
        }

        // Limit FPS on menu to save power
        Application.targetFrameRate = sceneName switch
        {
            "Main Menu" => 60,
            _ => 300
        };

        // Finish animation
        yield return new WaitForSeconds(0.25f);
        loadingScreen.DORotate(Vector3.left * 90, LoadDuration);
    }
}