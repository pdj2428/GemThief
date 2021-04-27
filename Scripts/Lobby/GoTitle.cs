using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoTitle : MonoBehaviour
{
    public void GoTotitle()
    {
        SoundManager.instance.PlayButtonSound();

        Setting.instance.SceneLoadAnimation(0);
    }
}
