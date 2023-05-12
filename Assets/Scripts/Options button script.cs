using System;
using UnityEngine;
using UnityEngine.UI;

public class Optionsbuttonscript : MonoBehaviour
{
    public Toggle fullscreenToggle;
    // Start is called before the first frame update
    void Start()
    {
        
        gameObject.GetComponent<Toggle>().isOn = Globals.Instance.dynamicFOV;
        
    }

    public void SetFullscreen()
    {
        Screen.fullScreen = fullscreenToggle.isOn;
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetDynamicFOV()
    {
        Globals.Instance.dynamicFOV = !Globals.Instance.dynamicFOV; 
        PlayerPrefs.SetInt("dynamic_fov", Convert.ToInt32(Globals.Instance.dynamicFOV));
    }

    //void for volume here when get music
}
