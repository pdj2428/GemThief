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
            checkJoinRoomText.text = "이미 방이 가득 찾습니다.";
                   
        }
        else if(Convert.ToInt32(roomList[roomId]["count"]) < 2)
        {
            checkJoinRoom.SetActive(true);
            acceptJoinRoomButton.interactable = true;
            checkJoinRoomText.text = "입장하시겠습니까?";
        }
    }

    public void CheckJoinRoom()
    {
        SoundManager.instance.PlayButtonSound();
        SearchRoom();
        checkJoinRoomText.text = "로딩중...";
        

        if (Convert.ToInt32(roomList[roomId]["count"]) == 2)
        {
            checkJoinRoom.SetActive(true);
            acceptJoinRoomButton.interactable = false;
            checkJoinRoomText.text = "이미 방이 가득 찾습니다.";                 
        }
        else if(Convert.ToInt32(roomList[roomId]["count"]) < 2)
        {
            checkJoinRoomText.text = "로딩중...";
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
