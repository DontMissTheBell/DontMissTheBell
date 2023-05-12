using DG.Tweening;
using System;
using System.Collections;
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
    public bool runningLate;
    public bool dynamicFOV;
    public bool firstRun;

    public int playerID;
    public Guid playerSecret;

    public static string Username
    {
        get => PlayerPrefs.GetString("username");
        set => PlayerPrefs.SetString("username", value);
    }

    public bool cutsceneActive;
    public event EventHandler CutsceneOver;

    public string replayToStart;


    // Loading screen
    private RectTransform loadingScreen;
    public string APIEndpoint => devAPI ? "http://localhost:8080/api" : "https://api.catpowered.net/dmtb";

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
        if (!PlayerPrefs.HasKey("player_id") || !PlayerPrefs.HasKey("player_secret"))
        {
            firstRun = true;
            // Generate a random player ID on first run
            playerID = new System.Random().Next();
            PlayerPrefs.SetInt("player_id", playerID);
            playerSecret = Guid.NewGuid();
            PlayerPrefs.SetString("player_secret", playerSecret.ToString());
        }
        else
        {
            playerID = PlayerPrefs.GetInt("player_id");
            playerSecret = new Guid(PlayerPrefs.GetString("player_secret"));
        }

        if (!PlayerPrefs.HasKey("dynamic_fov"))
        {
            dynamicFOV = true;
            PlayerPrefs.SetInt("dynamic_fov", Convert.ToInt32(dynamicFOV));
        }
        else
        {
            dynamicFOV = Convert.ToBoolean(PlayerPrefs.GetInt("dynamic_fov"));
        }
    }

    public void EndCutscene()
    {
        cutsceneActive = false;
        CutsceneOver?.Invoke(this, EventArgs.Empty);
    }

    // Transition manager
    public IEnumerator TriggerLoadingScreen(string sceneName = "", int sceneId = -1)
    {
        cutsceneActive = false;
        // Start animation
        loadingScreen.DORotate(Vector3.zero, LoadDuration);
        yield return new WaitForSeconds(LoadDuration);
        DOTween.KillAll();

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;

        // Reset level-relative variables
        gamePaused = false;
        levelComplete = false;
        if (sceneName != "EndCutscene")
        {
            runningLate = false;
        }

        if (sceneId >= 0)
        {
            sceneName = GetSceneNameFromId(sceneId);
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

    public static string GetSceneNameFromId(int id)
    {
        var sceneName = SceneUtility.GetScenePathByBuildIndex(id);
        var slashLocation = sceneName.LastIndexOf('/');
        sceneName = sceneName[slashLocation..];
        var dotLocation = sceneName.LastIndexOf('.');
        return sceneName[..dotLocation].TrimStart('/');
    }
}