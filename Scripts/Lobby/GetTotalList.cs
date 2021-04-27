using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetTotalList : MonoBehaviour
{
    public GameObject totalList;
    public GameObject totalContent;
    public GameObject total;

    public void ShowTotalList()
    {
        SoundManager.instance.PlayButtonSound();
        totalList.SetActive(true);

        StartCoroutine(SetTotalList());    
    }

    //전적 확인
    IEnumerator SetTotalList()
    {
        UserManager.instance.TakeUserTotalFromServer();
        yield return new WaitForSeconds(0.5f);
        Dictionary<string, object>[] userTotal = UserManager.instance.GetUserTotal();
        //리스트 생성
        for (int i = 0; i < userTotal.Length; i++)
        {
            GameObject totalBlock = Instantiate(total);
            totalBlock.transform.parent = totalContent.transform;
            totalBlock.transform.localScale = new Vector3(1, 1, 1);
            totalBlock.GetComponent<TotalList>().ShowTotalText(userTotal[i]["date"].ToString(), userTotal[i]["nickname"].ToString(), userTotal[i]["win"].ToString());
        }
    }

    //전적 확인 UI 닫기
    public void ExitTotalList()
    {
        SoundManager.instance.PlayButtonSound();
        Transform[] childList = totalContent.GetComponentsInChildren<Transform>(true);
        if (childList != null)
        {
            for (int i = 1; i < childList.Length; i++)
            {
                if (childList[i] != transform)
                    Destroy(childList[i].gameObject);
            }
        }

        totalList.SetActive(false);
    }
}
