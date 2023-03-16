using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class Globals : MonoBehaviour
{
    // Transition variables
    [FormerlySerializedAs("TransitionTop")] [SerializeField]
    private RectTransform transitionTop;

    [FormerlySerializedAs("TransitionBottom")] [SerializeField]
    private RectTransform transitionBottom;

    [FormerlySerializedAs("LoadingTop")] [SerializeField]
    private RectTransform loadingTop;

    [FormerlySerializedAs("LoadingBottom")] [SerializeField]
    private RectTransform loadingBottom;

    // Global variables
    public bool gamePaused;
    public bool levelComplete;

    public static Globals instance { get; private set; }

    // Avoid duplication
    private void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        instance = this;
        DontDestroyOnLoad(gameObject);

        gamePaused = 0.0f == Time.timeScale;
    }

    // Transition manager
    private IEnumerator ScreenTransition(string sceneName, float duration)
    {
        transitionBottom.DOAnchorPosY(transitionBottom.anchoredPosition.y + 545, duration).SetEase(Ease.InOutExpo);
        transitionTop.DOAnchorPosY(transitionTop.anchoredPosition.y + -545, duration).SetEase(Ease.InOutExpo);
        loadingTop.DOAnchorPosY(loadingTop.anchoredPosition.y + 545, duration).SetEase(Ease.InOutExpo);
        loadingBottom.DOAnchorPosY(loadingBottom.anchoredPosition.y - 545, duration).SetEase(Ease.InOutExpo);
        yield return new WaitForSeconds(duration);
        SceneManager.LoadScene(sceneName);
        yield return new WaitForSeconds(0.25f);
        transitionBottom.DOAnchorPosY(transitionBottom.anchoredPosition.y + -545, duration).SetEase(Ease.InOutExpo);
        transitionTop.DOAnchorPosY(transitionTop.anchoredPosition.y + 545, duration).SetEase(Ease.InOutExpo);
        loadingTop.DOAnchorPosY(loadingTop.anchoredPosition.y - 545, duration).SetEase(Ease.InOutExpo);
        loadingBottom.DOAnchorPosY(loadingBottom.anchoredPosition.y + 545, duration).SetEase(Ease.InOutExpo);
    }

    public static void LoadScene(string sceneName, float duration)
    {
        instance.StartCoroutine(instance.ScreenTransition(sceneName, duration));
    }
}