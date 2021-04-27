using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioSetting : MonoBehaviour
{
    public Slider audioSlider;
    public AudioSource backgroundAudioSource;
    public AudioSource touchAudioSource;
    public Text soundVolumeText;

    private float volume = 1.0f;

    private void Start()
    {
        //이전에 저장된 사운드 볼륨을 가져와서 적용
        volume = PlayerPrefs.GetFloat("volume");
        audioSlider.value = volume;
        backgroundAudioSource.volume = audioSlider.value;
        touchAudioSource.volume = audioSlider.value;
    }

    void Update()
    {
        //실시간으로 사운드 볼륨을 저장하고 적용
        backgroundAudioSource.volume = audioSlider.value;
        touchAudioSource.volume = audioSlider.value;
        soundVolumeText.text = "사운드 : " + Mathf.Floor(audioSlider.value * 100).ToString();

        volume = audioSlider.value;
        PlayerPrefs.SetFloat("volume", volume);
    }
}
