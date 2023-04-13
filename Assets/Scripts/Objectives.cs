using DG.Tweening;
using UnityEngine;

public class Objectives : MonoBehaviour
{
    public bool homeworkCompleted;

    public Tweener tweener1;
    public Tweener tweener2;

    private void Start()
    {
        tweener1 = transform.DOMove(transform.position + Vector3.up / 2, 1.5f).SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
        tweener2 = transform.DORotate(new Vector3(0, 360, 0), 3, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "MainCharacter")
        {
            homeworkCompleted = true;
            Debug.Log("Homework Obtained");
        }
    }
}