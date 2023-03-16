using DG.Tweening;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    [SerializeField] private GameObject particleObject;

    public ParticleSystem particles;

    // Determines the type of Powerup
    // 1 = Speed boost
    // 2 = Jump boost
    public int variant;


    public Tweener powerupLoop1;
    public Tweener powerupLoop2;


    // Start is called before the first frame update
    private void Start()
    {
        particles = particleObject.GetComponent<ParticleSystem>();
        powerupLoop1 = transform.DOMove(transform.position + Vector3.up / 2, 1.5f).SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
        powerupLoop2 = transform.DORotate(new Vector3(0, 360, 0), 3, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
    }

    // Update is called once per frame
    private void Update()
    {
    }
}