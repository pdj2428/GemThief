using UnityEngine;
using UnityEngine.UI;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

public class Login : MonoBehaviour
{
    public Text LoginText;
    private bool bWaitingForAuth = false;

    public bool isLogined = false;

    public static bool isComputer = true;

    public static bool GetIsLogined()
    {
        return Social.localUser.authenticated;

    }

    private void Awake()
    {
        //���� ���� Ȱ��ȭ
        PlayGamesPlatform.InitializeInstance(new PlayGamesClientConfiguration.Builder().Build());
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
    }

    private void Start()
    { 
        //DoAutoLogin();
       
    }

    public void DoAutoLogin()
    {
        if (bWaitingForAuth)
            return;

        //�α��� ��Ȱ��ȭ ���½�
        if (!Social.localUser.authenticated)
        {
            LoginText.text = "������...";
            

            //���� �α��� ���� ó������
            Social.localUser.Authenticate(AuthenticateCallback);
        }
        else
        {
            bWaitingForAuth = true;
            isLogined = true;
            LoginText.text = "�α��� ����!";
        }
    }

    public void DoManualLogin()
    {
        SoundManager.instance.PlayButtonSound();
        if (Social.localUser.authenticated)
        {
            LoginText.text = "�̹� �α��� �Ǿ����ϴ�.";
        }
        else
            Social.localUser.Authenticate((bool success) =>
            {
                if (success)
                {
                    LoginText.text = "�α��� ����!";
                    isLogined = true;
                    SendUserDataToServer();
                }
                else
                {
                    isLogined = false;
                    LoginText.text = "�α��� ����!";                   
                }
            });
    }

    public void DoManualLogout()
    {
        SoundManager.instance.PlayButtonSound();
        ((PlayGamesPlatform)Social.Active).SignOut();
        LoginText.text = "�α׾ƿ�";
        DeleteUserData();
    }

    void AuthenticateCallback(bool success)
    {
        LoginText.text = "�ε���...";
        if(success)
        {
            isLogined = true;
            LoginText.text = "�α��� ����!";
            SendUserDataToServer();
        }
        else
        {
            isLogined = false;
            LoginText.text = "�α��� ����!";
        }
    }

    void SendUserDataToServer()
    {
        UserManager.instance.SendUserGoogleDataToServer(((PlayGamesLocalUser)Social.localUser).id, Social.localUser.userName);
    }

    void DeleteUserData()
    {
        UserManager.instance.GetComponent<UserManager>().DeleteUserData();
    }
}