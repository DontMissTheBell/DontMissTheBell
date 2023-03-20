using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Globals : MonoBehaviour
{
    // Loading screen
    private RectTransform loadingScreen;
    private const float LoadDuration = 1.0f;

    // Debug API toggle
    [SerializeField] private bool devAPI; 

    // Global variables
    public bool gamePaused;
    public bool levelComplete;
    public string APIEndpoint => devAPI ? "http://localhost:8080/api" : "https://dmtb.catpowered.net/api";
    public string replayToStart;

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
    public IEnumerator TriggerLoadingScreen(string sceneName = "", int sceneId = -1)
    {
        loadingScreen.DORotate(Vector3.zero, LoadDuration);
        yield return new WaitForSeconds(LoadDuration);
        DOTween.KillAll();

        if (sceneId >= 0)
        {
            // Convert BuildIndex (levelId) to scene name
            sceneName = SceneUtility.GetScenePathByBuildIndex(sceneId);
            var slashLocation = sceneName.LastIndexOf('/');
            sceneName = sceneName[slashLocation..];
            var dotLocation = sceneName.LastIndexOf('.');
            sceneName = sceneName[..dotLocation];
            SceneManager.LoadScene(sceneId);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }

        Application.targetFrameRate = sceneName switch
        {
            "Main Menu" => 60,
            _ => 300
        };
        yield return new WaitForSeconds(0.25f);
        loadingScreen.DORotate(Vector3.left * 90, LoadDuration);
    }
}