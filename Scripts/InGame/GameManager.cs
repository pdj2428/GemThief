using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum GameState
{
    wait,
    move
}

public class GameManager : MonoBehaviour
{
    public GameState currentState = GameState.move;

    public Text roomNumber;

    public Text nicknameText;
    public Text opponentNicknameText;

    public Text opponentMapText;

    public Text scoreText;
    public Text opponentScoreText;

    public Text countTimer;

    public GameObject background;
    public GameObject gemPrefab;

    public GameObject opponentGemPrefab;

    public GameObject opponentBackgruond;

    public Button ExitButton;

    object[] userData;
    string opponentNickname = "";

    private int[,] map = new int[,] { };
    public GameObject[,] allGems;

    public int[,] opponentMap = new int[,] { };
    public GameObject[,] opponentAllGems;
    public bool opponentMapChange = false;

    public GameObject destroyEffect;

    public bool gameStart = false;

    public int myScore = 0;
    public int opponentScore = 0;

    public int offset = 20;

    private int roundTime = 30;
    int timer = 0;

    public bool getTimer =  false;
    public bool timerSet =  false;

    private int destroyGem = 0;
    private int prevDestroyGem = 0;

    void Start()
    {
        
        userData = UserManager.instance.GetUserData();
        nicknameText.text = userData[0].ToString();
        roomNumber.text = UserManager.instance.GetRoomId().ToString() + "번 방";
    }

    void Update()
    {
        opponentNickname = UserManager.instance.GetOpponentUserNickname();

        if(UserManager.instance.getMap == false)
        {
            UserManager.instance.GetShowMap();
        }


        if (opponentNickname == "" || opponentNickname == null)
        {
            opponentNicknameText.text = "";
            opponentMapText.text = "";
            opponentScoreText.text = "";
            StopGame();
        }
        else
        {
            UserManager.instance.GetOpponentScore();
            UserManager.instance.GetOpponentMap();

            //상대방 나가기
            UserManager.instance.OpponentPlayerIsExitRoom();
            //상대방 강제 종료
            UserManager.instance.OpponentPlayerIsLeaveRoom(timerSet);

            opponentNicknameText.text = opponentNickname;
            opponentMapText.text = opponentNickname;

            if (gameStart == false)
            {
                gameStart = true;
                StartCoroutine(StartRquest());
            }

            if (gameStart && getTimer)
            {
                getTimer = false;
                timerSet = true;
                GameTimer();
            }

            scoreText.text = myScore.ToString();
            opponentScoreText.text = opponentScore.ToString();

            if(opponentMapChange)
            {
                OpponentMapSetUp();
                opponentMapChange = false;
            }
        }
    }

    IEnumerator StartRquest()
    {
        UserManager.instance.GameStart();
        
        while(UserManager.instance.getMap == false)
        {
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.1f);
 
        map = UserManager.instance.GetMap();

        destroyGem = 0;
        prevDestroyGem = 0;

        yield return new WaitForSeconds(0.1f);

        SetUp();

        countTimer.text = "5";
        yield return new WaitForSeconds(1.0f);
        countTimer.text = "4";
        yield return new WaitForSeconds(1.0f);
        countTimer.text = "3";
        yield return new WaitForSeconds(1.0f);
        countTimer.text = "2";
        yield return new WaitForSeconds(1.0f);
        countTimer.text = "1";
        yield return new WaitForSeconds(1.0f);
        ExitButton.interactable = false;
        countTimer.text = "";
        getTimer = true;
    }

    //맵에 따른 보석 배치
    private void SetUp()
    {
        opponentScore = 0;
        myScore = 0;
 
        //자신 맵 배치
        allGems = new GameObject[map.GetLength(0)/2, map.GetLength(1)];
        for (int i = 0; i < map.GetLength(0)/2; i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                Vector2 tempPosition = new Vector2(j, i + offset);
                GameObject gem = Instantiate(gemPrefab, tempPosition, Quaternion.identity) as GameObject;
                gem.GetComponent<Gem>().row = i;
                gem.GetComponent<Gem>().colume = j;

                gem.transform.parent = background.transform;
                gem.GetComponent<Gem>().SetGem(map[i, j]);
                gem.name = "( " + j + ", " + i + " )";
                allGems[i, j] = gem;
            }
        }

        //상대방 맵 배치
        opponentAllGems = new GameObject[map.GetLength(0) / 2, map.GetLength(1)];
        for (int i = 0; i < map.GetLength(0) / 2; i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                Vector2 tempPosition = new Vector2(opponentBackgruond.transform.position.x + j, opponentBackgruond.transform.position.y + i);
                GameObject gem = Instantiate(opponentGemPrefab, tempPosition, Quaternion.identity) as GameObject;

                gem.transform.parent = opponentBackgruond.transform;
                gem.GetComponent<OpponentGem>().SetGem(map[i, j]);
                gem.name = "( " + j + ", " + i + " )";
                opponentAllGems[i, j] = gem;
            }
        }
    }

    //상대방 맵 보석 재배치
    private void OpponentMapSetUp()
    {
        for (int i = 0; i < opponentAllGems.GetLength(0); i++)
        {
            for (int j = 0; j < opponentAllGems.GetLength(1); j++)
            {
                opponentAllGems[i, j].GetComponent<OpponentGem>().SetGem(opponentMap[j, i]);
            }
        }
    }

    private void StopGame()
    {
        if(gameStart == true)
        {
            UserManager.instance.getMap = false;
            ExitButton.interactable = true;
            StopAllCoroutines();
            gameStart = false;
            countTimer.text = "상대가 떠났습니다";
            timerSet = false;
            DeleteMap();

        }

    }

    private void GameTimer()
    {
        timer = roundTime;
        StartCoroutine(TimerSet());
    }

    private IEnumerator TimerSet()
    {
            
        while(true)
        {
            countTimer.text = timer.ToString();
            timer--;
            yield return new WaitForSeconds(1.0f);
            if(timer < 0)
            {
                break;
            }
        }

        countTimer.text = "타임 아웃! 결과는....";
        yield return new WaitForSeconds(3.0f);

        UserManager.instance.SendGameResult(opponentNickname, myScore, opponentScore);
        timerSet = false;

        if (myScore > opponentScore)
        {
            countTimer.text = "승리!";
        }
        else if (myScore < opponentScore)
        {
            countTimer.text = "패배...";
        }
        else if(myScore == opponentScore)
        {
            countTimer.text = "무승부!";
        }
        
        ExitButton.interactable = true;

        yield return new WaitForSeconds(5.0f);
               
        DeleteMap();
        gameStart = false;
        UserManager.instance.getMap = false;
    }

    //인게임 관련 

    //매치가 됬는지 판별
    private void DestroyMatchesAt(int colume, int row)
    {
        if(allGems[colume, row].GetComponent<Gem>().isMatched)
        {
            GameObject particle = Instantiate(destroyEffect, allGems[colume, row].transform.position, Quaternion.identity);
            Destroy(particle, 0.5f);
            Destroy(allGems[colume, row]);
            allGems[colume, row] = null;
            destroyGem++;
        }
    }

    //매치가 됬는지 요청 받음
    public void DestroyMatches()
    {
        for(int i = 0; i < allGems.GetLength(0); i++)
        {
            for(int j = 0; j < allGems.GetLength(1); j++)
            {
                if (allGems[i, j] != null)
                {
                    DestroyMatchesAt(i, j);
                }
            }
        }
        StartCoroutine(DecreaseRowCorutine());
    }

    //매치된 보석 파괴
    private IEnumerator DecreaseRowCorutine()
    {
        SoundManager.instance.PlayGemSound();
        int nullCount = 0;
        for (int i = 0; i < allGems.GetLength(0); i++)
        {
            for (int j = 0; j < allGems.GetLength(1); j++)
            {
                if (allGems[i, j] == null)
                {
                    nullCount++;
                }
                else if (nullCount > 0)
                {
                    allGems[i, j].GetComponent<Gem>().row -= nullCount;
                    allGems[i, j] = null;
                }
            }
            nullCount = 0;
        }
        yield return new WaitForSeconds(0.4f);

        StartCoroutine(FillBoardCorutine());
    }

    //보석 채우기
    private void RefillBoard()
    {
        for (int i = 0; i < allGems.GetLength(0); i++)
        {
            for (int j = 0; j < allGems.GetLength(1); j++)
            {
                if (allGems[i, j] == null)
                {
                    Vector2 tempPosition = new Vector2(i, j + offset);
                    GameObject piece = Instantiate(gemPrefab, tempPosition, Quaternion.identity);

                    piece.GetComponent<Gem>().SetGem(map[map.GetLength(0) - i - 1, j]);
                    map[i, j] = map[map.GetLength(0) - i - 1, j];
                    map[map.GetLength(0) - i - 1, j] = 0;

                    allGems[i, j] = piece;
                    piece.GetComponent<Gem>().row = j;
                    piece.GetComponent<Gem>().colume = i;
                    piece.transform.parent = background.transform;
                }
                else
                {
                    map[i, j] = allGems[i, j].GetComponent<Gem>().gemNumber;
                }
            }
        }

        int[] sendmap = new int[128];
        int index = 0;
        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                sendmap[index] = map[i, j];
                index++;
            }
        }
        UserManager.instance.SendMapToServer(sendmap, myScore, prevDestroyGem);
        prevDestroyGem = destroyGem;
    }

    //드랍으로 인해 다시 보석이 매치되었는지 확인
    private bool MatchesOnBoard()
    {
        for (int i = 0; i < allGems.GetLength(0); i++)
        {
            for (int j = 0; j < allGems.GetLength(1); j++)
            {
                if(allGems[i, j] != null)
                {
                    if(allGems[i, j].GetComponent<Gem>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    //매치가 없을 때까지 보석 채우기
    private IEnumerator FillBoardCorutine()
    {
        RefillBoard();

        yield return new WaitForSeconds(0.1f);
        scoreText.text = myScore.ToString();
        map = UserManager.instance.GetMap();

        yield return new WaitForSeconds(0.4f);
        while(MatchesOnBoard())
        {
            yield return new WaitForSeconds(0.5f);
            DestroyMatches();
        }
        yield return new WaitForSeconds(0.5f);
        currentState = GameState.move;
    }

    //맵 파괴
    private void DeleteMap()
    {
        
        Transform[] childList = background.GetComponentsInChildren<Transform>(true);
        if (childList != null)
        {
            for (int i = 1; i < childList.Length; i++)
            {
                if (childList[i] != transform)
                    Destroy(childList[i].gameObject);
            }
        }

        Transform[] opponentChildList = opponentBackgruond.GetComponentsInChildren<Transform>(true);
        if (opponentChildList != null)
        {
            for (int i = 1; i < opponentChildList.Length; i++)
            {
                if (opponentChildList[i] != transform)
                {
                    Destroy(opponentChildList[i].gameObject);
                }
                    
            }
        }
    }

    //백그라운드 실행 시 
    private void OnApplicationPause(bool pause)
    {
        if(pause)
        {
            if(timerSet == false)
            {
                UserManager.instance.ExitRoom();
                SceneManager.LoadScene("Lobby");
            }
            else
            {
                UserManager.instance.ForceQuit();
                SceneManager.LoadScene("Lobby");
            }
        }
    }
}
