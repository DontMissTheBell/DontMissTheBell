using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public float TimeLimit;
    private float FinishTime;
    // Start is called before the first frame update
    void Start()
    {
        FinishTime = Time.time + TimeLimit;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(Time.time);
        if (Time.time >= FinishTime)
        {
            //SceneManager.LoadScene("LoadScreen");
            Debug.Log("Lose");
        }
    }
}