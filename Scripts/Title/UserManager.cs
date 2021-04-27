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

    // 다른 클래스에서 유저 데이터를 가져갈 때 사용

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

    
    //유저 정보 및 방 접속
    //로그인시 구글 정보를 서버에게 넘김
    public void SendUserGoogleDataToServer(string _userId, string _nickname)
    {
        Dictionary<string, object> userData = new Dictionary<string, object>();

        userId = _userId;
        nickname = _nickname;

        userData["userId"] = userId;
        userData["nickname"] = nickname;

        socketManager.Socket.Emit("login", userData);
    }

    // 유저 정보를 반환
    public object[] GetUserData()
    {
        TakeUserDataFromServer();
        object[] data = { nickname, win, lose, draw } ;
        return data;
    }

    //서버로부터 유저 정보 받음
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

    //유저 최근 전적 반환
    public Dictionary<string, object>[] GetUserTotal()
    {
        return userTotal;
    }

    //서버로부터 유저의 최근 전적 받음
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

    //방 리스트 반환
    public Dictionary<string, object>[] GetRoomList()
    {
        return roomList;
    }

    //서버로부터 방 리스트를 가져옴
    public void TakeRoomListFromServer(int length)
    {
        socketManager.Socket.Emit("getRoom");
        socketManager.Socket.On("showRoomList", (socket, packet, args) =>
        {
            roomList = ConvertStringToDictionaryArray(packet.RemoveEventName(true), length, 1);
        });
    }

    //방에 접속
    public void JoinRoom(int roomId, int memberCount)
    {
        roomJoinRequestData.Clear();
        roomJoinRequestData.Add("userId", userId);
        roomJoinRequestData.Add("roomId", roomId);

        socketManager.Socket.Emit("joinRoom", roomJoinRequestData);

        //상대방이 방에 없을 경우
        if(memberCount == 0)
        {
            StartCoroutine(GetUserNickname());
        }
        //상대방이 방에 이미 있을경우
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

    //상대방 닉네임 받기
    public string GetOpponentUserNickname()
    {
        return opponentUserNickName;
    }
    
    //룸 번호 받기
    public int GetRoomId()
    {
        return nowRoomId;
    }

    //게임 시작
    public void GameStart()
    {
        Dictionary<string, object> roomData = new Dictionary<string, object>();
        roomData.Add("roomId", nowRoomId);
        roomData.Add("userId", userId);
        socketManager.Socket.Emit("startGame", roomData);
    }
    //맵 생성

    //서버로부터 맵 받기
    public void GetShowMap()
    {
        socketManager.Socket.On("showMap", (socket, packet, args) =>
        {
            map = ConvertStringToIntArray(packet.RemoveEventName(true));
            getMap = true;
        });
    }

    //맵 받기
    public int[,] GetMap()
    {
        return map;
    }

    //게임 퇴장 시 실행
    public void ExitRoom()
    {
        map = null;
        socketManager.Socket.Emit("leaveRoom", roomJoinRequestData);
        opponentUserNickName = "";
        nowRoomId = 0;
    }

    //상대방 플레이어가 나갔는지 판별
    public void OpponentPlayerIsExitRoom()
    {
        socketManager.Socket.On("leaveOpponent", (socket, packet, args) =>
        {
            opponentUserNickName = "";
        });
    }

    //상대방 플레이어 강제 종료 판별
    public void OpponentPlayerIsLeaveRoom(bool isStart)
    {
        socketManager.Socket.On("forceQuit", (socket, packet, args) =>
        {
            opponentUserNickName = "";
        });
        
    }


    //서버로 맵을 보냄
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

    //상대방 점수 확인
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

        //value가 string일때
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
        //value가 int일때
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

    //게임 결과 서버로 저장
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

    //게임 종료 시 실행
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

    //게임 강제로 나갈 시 실행
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

    //로그아웃시 저장된 유저 데이터 삭제
    public void DeleteUserData()
    {
        userId = "";
        nickname = "";
        win = 0;
        lose = 0;
    }
}
