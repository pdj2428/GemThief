using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpponentGem : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite[] sprite;

    public void SetGem(int gem)
    {
        spriteRenderer.sprite = sprite[gem - 1];
    }
}
