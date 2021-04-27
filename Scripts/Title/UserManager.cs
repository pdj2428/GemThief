using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using BestHTTP.SocketIO;


public class UserManager : MonoBehaviour
{
    public static UserManager instance;

    private const string address = "http://3.35.230.80:3000/socket.io/";
    private SocketManager socketManager;

    private string userId = "";
    private string nickname = "";

    private int win = 0;
    private int lose = 0;
    private int draw = 0;

    private Dictionary<string, object>[] userTotal = new Dictionary<string, object>[] { };
    private Dictionary<string, object>[] roomList = new Dictionary<string, object>[] { };
    private string opponentUserNickName;
    private Dictionary<string, object> roomJoinRequestData = new Dictionary<string, object>();

    private int nowRoomId;

    private int[,] map = new int[,] { };

    public bool getMap = false;

    // �ٸ� Ŭ�������� ���� �����͸� ������ �� ���

    private void Awake()
    {
        if (UserManager.instance == null)
            UserManager.instance = this;
        else
            Destroy(gameObject);
        socketManager =  new SocketManager(new Uri(address));
    }

    private void Update()
    {
        DontDestroyOnLoad(gameObject);
    }

    
    //���� ���� �� �� ����
    //�α��ν� ���� ������ �������� �ѱ�
    public void SendUserGoogleDataToServer(string _userId, string _nickname)
    {
        Dictionary<string, object> userData = new Dictionary<string, object>();

        userId = _userId;
        nickname = _nickname;

        userData["userId"] = userId;
        userData["nickname"] = nickname;

        socketManager.Socket.Emit("login", userData);
    }

    // ���� ������ ��ȯ
    public object[] GetUserData()
    {
        TakeUserDataFromServer();
        object[] data = { nickname, win, lose, draw } ;
        return data;
    }

    //�����κ��� ���� ���� ����
    public void TakeUserDataFromServer()
    {
        socketManager.Socket.Emit("getInfo", userId);
        socketManager.Socket.On("info", (socket, packet, args) => 
        {
            Dictionary<string, object> userData = new Dictionary<string, object>();
            userData = args[0] as Dictionary<string, object>;
            userId = userData["userId"].ToString();
            nickname = userData["nickname"].ToString();
            win = Convert.ToInt32(userData["win"]);
            lose = Convert.ToInt32(userData["lose"]);
            draw = Convert.ToInt32(userData["draw"]);
        });
    }

    //���� �ֱ� ���� ��ȯ
    public Dictionary<string, object>[] GetUserTotal()
    {
        return userTotal;
    }

    //�����κ��� ������ �ֱ� ���� ����
    public void TakeUserTotalFromServer()
    {
        int length = 0;
        socketManager.Socket.Emit("getLength", userId);
        socketManager.Socket.On("showLength", (socket, packet, args) =>
        {
            length = Convert.ToInt32(args[0]);
        });
        socketManager.Socket.Emit("getTotal", userId);
        socketManager.Socket.On("showTotal", (socket, packet, args) => 
        {
            userTotal = ConvertStringToDictionaryArray(packet.RemoveEventName(true), length, "");                        
        });
    }

    //�� ����Ʈ ��ȯ
    public Dictionary<string, object>[] GetRoomList()
    {
        return roomList;
    }

    //�����κ��� �� ����Ʈ�� ������
    public void TakeRoomListFromServer(int length)
    {
        socketManager.Socket.Emit("getRoom");
        socketManager.Socket.On("showRoomList", (socket, packet, args) =>
        {
            roomList = ConvertStringToDictionaryArray(packet.RemoveEventName(true), length, 1);
        });
    }

    //�濡 ����
    public void JoinRoom(int roomId, int memberCount)
    {
        roomJoinRequestData.Clear();
        roomJoinRequestData.Add("userId", userId);
        roomJoinRequestData.Add("roomId", roomId);

        socketManager.Socket.Emit("joinRoom", roomJoinRequestData);

        //������ �濡 ���� ���
        if(memberCount == 0)
        {
            StartCoroutine(GetUserNickname());
        }
        //������ �濡 �̹� �������
        else if (memberCount == 1)
        {
            socketManager.Socket.On("getOpponentNickname", (socket, packet, args) =>
            {
                opponentUserNickName = args[0].ToString();
            });
        }
        nowRoomId = roomId;
    }

    IEnumerator GetUserNickname()
    {
        while(opponentUserNickName == "" || opponentUserNickName == null)
        {
            socketManager.Socket.On("getNickname", (socket, packet, args) =>
            {
                
                opponentUserNickName = args[0].ToString();
            });
            yield return new WaitForSeconds(0.02f);
        }
    }

    //���� �г��� �ޱ�
    public string GetOpponentUserNickname()
    {
        return opponentUserNickName;
    }
    
    //�� ��ȣ �ޱ�
    public int GetRoomId()
    {
        return nowRoomId;
    }

    //���� ����
    public void GameStart()
    {
        Dictionary<string, object> roomData = new Dictionary<string, object>();
        roomData.Add("roomId", nowRoomId);
        roomData.Add("userId", userId);
        socketManager.Socket.Emit("startGame", roomData);
    }
    //�� ����

    //�����κ��� �� �ޱ�
    public void GetShowMap()
    {
        socketManager.Socket.On("showMap", (socket, packet, args) =>
        {
            map = ConvertStringToIntArray(packet.RemoveEventName(true));
            getMap = true;
        });
    }

    //�� �ޱ�
    public int[,] GetMap()
    {
        return map;
    }

    //���� ���� �� ����
    public void ExitRoom()
    {
        map = null;
        socketManager.Socket.Emit("leaveRoom", roomJoinRequestData);
        opponentUserNickName = "";
        nowRoomId = 0;
    }

    //���� �÷��̾ �������� �Ǻ�
    public void OpponentPlayerIsExitRoom()
    {
        socketManager.Socket.On("leaveOpponent", (socket, packet, args) =>
        {
            opponentUserNickName = "";
        });
    }

    //���� �÷��̾� ���� ���� �Ǻ�
    public void OpponentPlayerIsLeaveRoom(bool isStart)
    {
        socketManager.Socket.On("forceQuit", (socket, packet, args) =>
        {
            opponentUserNickName = "";
        });
        
    }


    //������ ���� ����
    public void SendMapToServer(int[] _map, int score, int destroyBlock)
    {
        Dictionary<string, object> sendMapData = new Dictionary<string, object>();
        sendMapData.Add("map", _map);
        sendMapData.Add("roomId", nowRoomId);
        sendMapData.Add("score", score);
        sendMapData.Add("destroyBlock", destroyBlock);
        
        socketManager.Socket.Emit("destroy", sendMapData);
        socketManager.Socket.On("plusScore", (socket, packet, args) =>
        {
            GameObject.Find("GameManager").GetComponent<GameManager>().myScore = Convert.ToInt32(args[0]);
        });

        socketManager.Socket.On("updateMap", (socket, packet, args) =>
        {
            map = ConvertStringToIntArray(packet.RemoveEventName(true));
        });
    }

    //���� ���� Ȯ��
    public void GetOpponentScore()
    {
        socketManager.Socket.On("getOpponentScore", (socket, packet, args) =>
        {
            GameObject.Find("GameManager").GetComponent<GameManager>().opponentScore = Convert.ToInt32(args[0]);
        });
    }

    public void GetOpponentMap()
    {
        socketManager.Socket.On("getOpponentMap", (socket, packet, args) =>
        {
            GameObject.Find("GameManager").GetComponent<GameManager>().opponentMap = ConvertStringToIntArray(packet.RemoveEventName(true));
            GameObject.Find("GameManager").GetComponent<GameManager>().opponentMapChange = true;
        });
    }

    //string -> Dictionary<string, object>[]
    public Dictionary<string, object>[] ConvertStringToDictionaryArray(string dataString, int length, object type)
    {
        Dictionary<string, object>[] arrayDictionary = new Dictionary<string, object>[length];

        string key = "";
        string value = "";
        int saveIndex = 0;

        //value�� string�϶�
        if (type is string)
        {
            for (int index = 0; index < length; index++)
            {
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                //
                for (int i = saveIndex; i < dataString.Length; i++)
                {
                    if (dataString[i] == '"' && key == "")
                    {
                        for (int j = i + 1; dataString[j] != '"'; j++)
                        {
                            key += dataString[j];
                        }
                        i += key.Length + 1;
                        value = "";
                    }
                    else if (dataString[i] == '"' && key != "")
                    {
                        for (int j = i + 1; dataString[j] != '"'; j++)
                        {
                            value += dataString[j];
                        }
                        dictionary.Add(key, value);


                        i += value.Length + 1;
                        key = "";
                    }
                    else if (dataString[i] == '}')
                    {
                        saveIndex = i + 1;
                        break;
                    }
                    arrayDictionary[index] = dictionary;
                }
            
            }
        }
        //value�� int�϶�
        else if (type is int)
        {
            for (int index = 0; index < length; index++)
            {
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                //
                for (int i = saveIndex; i < dataString.Length; i++)
                {
                    if (dataString[i] == '"' && key == "")
                    {
                        for (int j = i + 1; dataString[j] != '"'; j++)
                        {
                            key += dataString[j];
                        }
                        i += key.Length + 1;
                        value = "";
                    }
                    else if (dataString[i] == ':' && key != "")
                    {
                        for (int j = i + 1; dataString[j] != ',' && dataString[j] != '}'; j++)
                        {
                            value += dataString[j];
                        }
                        dictionary.Add(key, value);

                        i += value.Length - 1;
                        key = "";
                    }
                    else if (dataString[i] == '}')
                    {
                        saveIndex = i + 1;
                        break;
                    }
                    arrayDictionary[index] = dictionary;
                }

            }
        }
        return arrayDictionary;
    }

    //string -> intarr
    public int[,] ConvertStringToIntArray(string dataString)
    {
        int[,] dataArray = new int[16, 8];
        int width = 0;
        for (int i = 1; i < dataString.Length; i++)
        {
            if (dataString[i] == '[')
            {
                int height = 0;
                int plusLength = 0;
                for (int j = i+1; dataString[j] != ']'; j++)
                {
                    if (dataString[j] != ',' && dataString[j] != '"')
                    {
                        dataArray[width, height++] = char.ToUpper(dataString[j]) - 64;
                    }
                    plusLength++;
                }
                width++;
                i += plusLength + 1;
            }
        }
        return dataArray;
    }

    //���� ��� ������ ����
    public void SendGameResult(string opponentNickname, int myScore, int opponentScore)
    {
        Dictionary<string, object> sendResult = new Dictionary<string, object>();
        sendResult.Add("userId", userId);
        sendResult.Add("opponentNickname", opponentNickname);
        sendResult.Add("myScore", myScore);
        sendResult.Add("opponentScore", opponentScore);
        sendResult.Add("roomId", nowRoomId);
        socketManager.Socket.Emit("endGame", sendResult);
    }

    //���� ���� �� ����
    private void OnApplicationQuit()
    {
        if (nowRoomId != 0)
        {
            if(GameObject.Find("GameManager").GetComponent<GameManager>().timerSet)
            {
                ForceQuit();
            }
            else
            {
                ExitRoom();
            }
        }
    }

    //���� ������ ���� �� ����
    public void ForceQuit()
    {
        map = null;
        Dictionary<string, object> roomData = new Dictionary<string, object>();
        roomData.Add("userId", userId);
        roomData.Add("roomId", nowRoomId);
        roomData.Add("opponentNickname", opponentUserNickName);     
        socketManager.Socket.Emit("leaveGame", roomData);
        opponentUserNickName = "";
        nowRoomId = 0;
    }

    //�α׾ƿ��� ����� ���� ������ ����
    public void DeleteUserData()
    {
        userId = "";
        nickname = "";
        win = 0;
        lose = 0;
    }
}
