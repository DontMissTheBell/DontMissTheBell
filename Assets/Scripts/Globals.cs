using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Globals : MonoBehaviour
{
    public static Globals instance { get; private set; }
    // Avoid duplication
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        gamePaused = 0.0f == Time.timeScale;
    }

    // Transition variables
    [SerializeField] private RectTransform TransitionTop;
    [SerializeField] private RectTransform TransitionBottom;
    [SerializeField] private RectTransform LoadingTop;
    [SerializeField] private RectTransform LoadingBottom;

    // Global variables
    public bool gamePaused;
    public bool levelComplete;

    // Transition manager
    private IEnumerator ScreenTransition(string sceneName, float duration)
    {
        TransitionBottom.DOAnchorPosY(TransitionBottom.anchoredPosition.y + 545, duration).SetEase(Ease.InOutExpo);
        TransitionTop.DOAnchorPosY(TransitionTop.anchoredPosition.y + -545, duration).SetEase(Ease.InOutExpo);
        LoadingTop.DOAnchorPosY(LoadingTop.anchoredPosition.y + 545, duration).SetEase(Ease.InOutExpo);
        LoadingBottom.DOAnchorPosY(LoadingBottom.anchoredPosition.y - 545, duration).SetEase(Ease.InOutExpo);
        yield return new WaitForSeconds(duration);
        SceneManager.LoadScene(sceneName);
        yield return new WaitForSeconds(0.25f);
        TransitionBottom.DOAnchorPosY(TransitionBottom.anchoredPosition.y + -545, duration).SetEase(Ease.InOutExpo);
        TransitionTop.DOAnchorPosY(TransitionTop.anchoredPosition.y + 545, duration).SetEase(Ease.InOutExpo);
        LoadingTop.DOAnchorPosY(LoadingTop.anchoredPosition.y - 545, duration).SetEase(Ease.InOutExpo);
        LoadingBottom.DOAnchorPosY(LoadingBottom.anchoredPosition.y + 545, duration).SetEase(Ease.InOutExpo);

    }
    public static void LoadScene(string sceneName, float duration)
    {
        instance.StartCoroutine(instance.ScreenTransition(sceneName, duration));
    }
}
