using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class GetRoomList : MonoBehaviour
{
    private const int ROOM_LENGTH = 20;

    public Text[] connectedCount;
    public GameObject checkJoinRoom;
    public Text checkJoinRoomText;
    public Button acceptJoinRoomButton;

    private Dictionary<string, object>[] roomList = new Dictionary<string, object>[] { };

    private int roomId = -1;

    public void OnEnable()
    {
        SearchRoom();
    }

    public void SearchRoom()
    {
        StartCoroutine(SetRoomlList());
    }

    IEnumerator SetRoomlList()
    {
        UserManager.instance.TakeRoomListFromServer(ROOM_LENGTH);
        yield return new WaitForSeconds(0.1f);
        roomList = UserManager.instance.GetRoomList();

        for (int i = 0; i < ROOM_LENGTH; i++)
        {
            connectedCount[i].text = roomList[i]["count"].ToString() + "/2";
        }
    }

    public void JoinRoom(int _roomId)
    {
        SoundManager.instance.PlayButtonSound();
        SearchRoom();
        roomId = _roomId - 1;
        checkJoinRoom.SetActive(true);


        if(Convert.ToInt32(roomList[roomId]["count"]) == 2)
        {
            checkJoinRoom.SetActive(true);
            acceptJoinRoomButton.interactable = false;
            checkJoinRoomText.text = "�̹� ���� ���� ã���ϴ�.";
                   
        }
        else if(Convert.ToInt32(roomList[roomId]["count"]) < 2)
        {
            checkJoinRoom.SetActive(true);
            acceptJoinRoomButton.interactable = true;
            checkJoinRoomText.text = "�����Ͻðڽ��ϱ�?";
        }
    }

    public void CheckJoinRoom()
    {
        SoundManager.instance.PlayButtonSound();
        SearchRoom();
        checkJoinRoomText.text = "�ε���...";
        

        if (Convert.ToInt32(roomList[roomId]["count"]) == 2)
        {
            checkJoinRoom.SetActive(true);
            acceptJoinRoomButton.interactable = false;
            checkJoinRoomText.text = "�̹� ���� ���� ã���ϴ�.";                 
        }
        else if(Convert.ToInt32(roomList[roomId]["count"]) < 2)
        {
            checkJoinRoomText.text = "�ε���...";
            UserManager.instance.JoinRoom(Convert.ToInt32(roomList[roomId]["id"]), Convert.ToInt32(roomList[roomId]["count"]));
            Setting.instance.SceneLoadAnimation(2);
        }

    }

    public void ExitCheckJoinRoom()
    {
        roomId = -1;
        checkJoinRoom.SetActive(false);
    }
}
