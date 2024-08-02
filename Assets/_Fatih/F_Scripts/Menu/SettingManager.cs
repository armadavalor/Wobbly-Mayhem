using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{

    [SerializeField] Slider sensivitySlider;
    [SerializeField] TextMeshProUGUI sensivityValueText;
    [SerializeField] PlayerController2 playerController;

    [SerializeField] Slider musicSlider;
    [SerializeField] TextMeshProUGUI musicVolumeText;
    [SerializeField] AudioSource[] gameMusic;

    [SerializeField] Slider sfxSlider;
    [SerializeField] TextMeshProUGUI sfxVolumeText;
    [SerializeField] AudioSource[] gameSfxs;


    void Start()
    {
        if (PlayerPrefs.HasKey("Sensivity"))
        {
            sensivitySlider.value = PlayerPrefs.GetFloat("Sensivity");
        }
        else
        {
            SetSensivityValue();
        }

        if (PlayerPrefs.HasKey("Music"))
        {
            musicSlider.value = PlayerPrefs.GetFloat("Music");
            SetMusicVolume();
        }
        else
        {
            SetMusicVolume();
        }

        if (PlayerPrefs.HasKey("SFX"))
        {
            sfxSlider.value = PlayerPrefs.GetFloat("SFX");
        }
        else
        {
            SetSFXVolume();
        }
    }

    public void SetSensivityValue()
    {
        float value = sensivitySlider.value;
        sensivityValueText.text = value.ToString("N2");
        PlayerPrefs.SetFloat("Sensivity", value);

        playerController.rotationSpeed = value;
    }

    public void SetMusicVolume()
    {
        float value = musicSlider.value;
        musicVolumeText.text = (value * 100).ToString("N0");
        PlayerPrefs.SetFloat("Music", value);

        foreach (var music in gameMusic) { music.volume = value; }
    }

    public void SetSFXVolume()
    {
        float value = sfxSlider.value;
        sfxVolumeText.text = (value * 100).ToString("N0");
        PlayerPrefs.SetFloat("SFX", value);

        foreach (var sfx in gameSfxs) { sfx.volume = value; }
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
