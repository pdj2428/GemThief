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
            loginText.text = "������...";
            Setting.instance.SceneLoadAnimation(1);
        }
        else
        {
            loginText.text = "�α����� �ؾ� ������ ������ �� �ֽ��ϴ�.";
        }
    }
}
