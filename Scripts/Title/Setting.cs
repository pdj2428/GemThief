using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class Setting : MonoBehaviour
{
    public static Setting instance;

    public GameObject SettingUI;

    public GameObject CheckNetworkUI;

    private int isDisconnectedType = 0;

    public Animator LoadSceneAnimator;

    private void Awake()
    {
        if (Setting.instance == null)
            Setting.instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            CheckNetworkUI.SetActive(true);
            if(SceneManager.GetActiveScene().name == "InGame")
            {
                if(GameObject.Find("GameManager").GetComponent<GameManager>().timerSet)
                {
                    isDisconnectedType = 1;
                }
                else
                {
                    isDisconnectedType = 2;
                }
            }
        }
        else
        {
            if (isDisconnectedType == 1)
            {
                UserManager.instance.ForceQuit();
            }
            else if(isDisconnectedType == 2)
            {
                UserManager.instance.ExitRoom();
            }

            isDisconnectedType = 0;
            CheckNetworkUI.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetUIActive();
        }
    }

    public void SceneLoadAnimation(int sceneNumber)
    {
        StartCoroutine(SceneLoadCorutine(sceneNumber));
    }

    IEnumerator SceneLoadCorutine(int sceneNumber)
    {
        LoadSceneAnimator.SetBool("Load", true);
        SoundManager.instance.SceneLoadSound();

        yield return new WaitForSeconds(1.6f);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneNumber);
        while(!op.isDone)
        {
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.5f);

        LoadSceneAnimator.SetBool("Load", false);
        yield return new WaitForSeconds(1.0f);

    }

    public void SetUIActive()
    {
        SoundManager.instance.PlayButtonSound();
        SettingUI.SetActive(!SettingUI.activeSelf);
    }

    public void ExitGame()
    {
        SoundManager.instance.PlayButtonSound();
        Application.Quit();
    }
}
