using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetUserDataUI : MonoBehaviour
{
    public Text userNickname;
    public Text userRecord;
    private object[] userData;
    void Start()
    {
        StartCoroutine(GetPlayerData());
    }

    private IEnumerator GetPlayerData()
    {
        UserManager.instance.TakeUserDataFromServer();
        yield return new WaitForSeconds(0.1f);
        userData = UserManager.instance.GetUserData();
        userNickname.text = userData[0].ToString();
        userRecord.text = userData[1].ToString() + "½Â " + userData[2].ToString() + "ÆÐ " + userData[3].ToString() + "¹« ";
    }
}
