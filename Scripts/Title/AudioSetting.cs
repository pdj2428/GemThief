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
        //������ ����� ���� ������ �����ͼ� ����
        volume = PlayerPrefs.GetFloat("volume");
        audioSlider.value = volume;
        backgroundAudioSource.volume = audioSlider.value;
        touchAudioSource.volume = audioSlider.value;
    }

    void Update()
    {
        //�ǽð����� ���� ������ �����ϰ� ����
        backgroundAudioSource.volume = audioSlider.value;
        touchAudioSource.volume = audioSlider.value;
        soundVolumeText.text = "���� : " + Mathf.Floor(audioSlider.value * 100).ToString();

        volume = audioSlider.value;
        PlayerPrefs.SetFloat("volume", volume);
    }
}
