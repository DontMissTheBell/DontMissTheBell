using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Volume : MonoBehaviour

{
    public Slider soundSlider;
    public AudioMixer masterMixer;
    // Start is called before the first frame update
    private void Start()
    {
        SetVolume(PlayerPrefs.GetFloat("SavedMasterVolume", 30));
    }

    public void SetVolume(float VolumeValue)
    {
        if (VolumeValue < 1)
        {
            VolumeValue = 0.001f;
        }

        RefreshSlider(VolumeValue);
        PlayerPrefs.SetFloat("SavedMasterVolume", VolumeValue);
        masterMixer.SetFloat("MasterVolume", Mathf.Log10(VolumeValue / 100f) * 20f);
    }

    public void SetVolumeFromSlider()
    {
        SetVolume(soundSlider.value);
    }

    public void RefreshSlider(float VolumeValue)
    {
        soundSlider.value = VolumeValue;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
