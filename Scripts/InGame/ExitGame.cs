using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitGame : MonoBehaviour
{
    public void ExitPlayRoom()
    {
        SoundManager.instance.PlayButtonSound();
        UserManager.instance.ExitRoom();
        Setting.instance.SceneLoadAnimation(1);
    }
}
