using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Optionsbuttonscript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setFullscreen(bool FullscreenOn )
    {
        Screen.fullScreen = FullscreenOn;
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void setDynamicFOV()
    {
        Globals.Instance.dynamicFOV = !Globals.Instance.dynamicFOV;
        PlayerPrefs.SetInt("dynamic_fov", Convert.ToInt32(Globals.Instance.dynamicFOV));
    }

    //void for volume here when get music
}
