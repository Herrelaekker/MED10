using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public Sprite crackedSprite;
    public Sprite goldenSprite;

    SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void ChangeSprite(MotorImageryEvent classification)
    {
        if (classification == MotorImageryEvent.Rest)
            sr.sprite = crackedSprite;
        else if (classification == MotorImageryEvent.GoldenMotorImagery)
        {
           sr.sprite = goldenSprite;
        }
    }
}
