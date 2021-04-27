using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoLobby : MonoBehaviour
{
    private bool login;

    public Text loginText;
    public void GotoLobby()
    {
        login = Login.isComputer;
        login = GameObject.Find("Login").GetComponent<Login>().isLogined;
        SoundManager.instance.PlayButtonSound();
        
        if (login)
        {
            loginText.text = "연결중...";
            Setting.instance.SceneLoadAnimation(1);
        }
        else
        {
            loginText.text = "로그인을 해야 게임을 시작할 수 있습니다.";
        }
    }
}
