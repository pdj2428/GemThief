using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public AudioClip[] backgroundAudioClip;
    public AudioClip buttonSound;
    public AudioClip gemSound;
    public AudioClip sceneLoadSound;

    public AudioSource backgroundAudioSource;
    public AudioSource touchAudioSource;

    private string prevScene = "";

    private void Awake()
    {
        if (SoundManager.instance == null)
            SoundManager.instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "Lobby" && prevScene == "InGame")
        {
            prevScene = SceneManager.GetActiveScene().name;
            backgroundAudioSource.clip = backgroundAudioClip[0];
            backgroundAudioSource.Play();
        }
        else if (SceneManager.GetActiveScene().name == "InGame" && prevScene != SceneManager.GetActiveScene().name)
        {
            prevScene = SceneManager.GetActiveScene().name;
            backgroundAudioSource.clip = backgroundAudioClip[1];
            backgroundAudioSource.Play();
        }

        DontDestroyOnLoad(gameObject);
    }

    public void PlayButtonSound() {
        touchAudioSource.PlayOneShot(buttonSound);
    }

    public void PlayGemSound()
    {
        touchAudioSource.PlayOneShot(gemSound);
    }

    public void SceneLoadSound()
    {
        touchAudioSource.PlayOneShot(sceneLoadSound);
    }
}
