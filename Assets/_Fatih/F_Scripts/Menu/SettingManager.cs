using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{

    [SerializeField] Slider sensivitySlider;
    [SerializeField] TextMeshProUGUI sensivityValueText;

    [SerializeField] Slider musicSlider;
    [SerializeField] TextMeshProUGUI musicVolumeText;
    [SerializeField] AudioSource gameMusic;

    [SerializeField] Slider sfxSlider;
    [SerializeField] TextMeshProUGUI sfxVolumeText;
    [SerializeField] AudioSource gameSfxs;

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
    }

    public void SetMusicVolume()
    {
        float value = musicSlider.value;
        musicVolumeText.text = (value * 100).ToString("N0");
        PlayerPrefs.SetFloat("Music", value);
        gameMusic.volume = value;
    }

    public void SetSFXVolume()
    {
        float value = sfxSlider.value;
        sfxVolumeText.text = (value * 100).ToString("N0");
        PlayerPrefs.SetFloat("SFX", value);
    }
}
