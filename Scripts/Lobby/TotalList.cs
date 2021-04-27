using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TotalList : MonoBehaviour
{
    public Text date;
    public Text nickname;
    public Text record;
    public void ShowTotalText(string _data, string _nickname, string _record)
    {
        date.text = _data;
        nickname.text = _nickname;
        record.text = _record;
    }
}
