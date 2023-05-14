using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Optionsbuttonscript : MonoBehaviour
{
    public Toggle fullscreenToggle;
    public Toggle dynamicFOVToggle;

    public TMP_Dropdown qualityDropdown;

    // Start is called before the first frame update
    void Start()
    {
        dynamicFOVToggle.isOn = Globals.Instance.dynamicFOV;
    }

    public void SetFullscreen()
    {
        Screen.fullScreen = fullscreenToggle.isOn;
    }

    public void SetQuality()
    {
        QualitySettings.SetQualityLevel(qualityDropdown.value);
    }

    public void SetDynamicFOV()
    {
        Globals.Instance.dynamicFOV = !Globals.Instance.dynamicFOV; 
        PlayerPrefs.SetInt("dynamic_fov", Convert.ToInt32(Globals.Instance.dynamicFOV));
    }

    //void for volume here when get music
}
