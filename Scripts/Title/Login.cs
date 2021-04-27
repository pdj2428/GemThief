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
        //구글 서비스 활성화
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

        //로그인 비활성화 상태시
        if (!Social.localUser.authenticated)
        {
            LoginText.text = "연결중...";
            

            //구글 로그인 인증 처리과정
            Social.localUser.Authenticate(AuthenticateCallback);
        }
        else
        {
            bWaitingForAuth = true;
            isLogined = true;
            LoginText.text = "로그인 성공!";
        }
    }

    public void DoManualLogin()
    {
        SoundManager.instance.PlayButtonSound();
        if (Social.localUser.authenticated)
        {
            LoginText.text = "이미 로그인 되었습니다.";
        }
        else
            Social.localUser.Authenticate((bool success) =>
            {
                if (success)
                {
                    LoginText.text = "로그인 성공!";
                    isLogined = true;
                    SendUserDataToServer();
                }
                else
                {
                    isLogined = false;
                    LoginText.text = "로그인 실패!";                   
                }
            });
    }

    public void DoManualLogout()
    {
        SoundManager.instance.PlayButtonSound();
        ((PlayGamesPlatform)Social.Active).SignOut();
        LoginText.text = "로그아웃";
        DeleteUserData();
    }

    void AuthenticateCallback(bool success)
    {
        LoginText.text = "로딩중...";
        if(success)
        {
            isLogined = true;
            LoginText.text = "로그인 성공!";
            SendUserDataToServer();
        }
        else
        {
            isLogined = false;
            LoginText.text = "로그인 실패!";
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