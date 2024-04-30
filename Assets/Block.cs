using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public Sprite crackedSprite;
    public GameObject goldenEffect;

    SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        goldenEffect.SetActive(false);
    }

    public void ChangeSprite(MotorImageryEvent classification)
    {
        if (classification == MotorImageryEvent.Rest)
            sr.sprite = crackedSprite;
        else if (classification == MotorImageryEvent.GoldenMotorImagery)
        {
            goldenEffect.SetActive(true);
            //goldenEffect.transform.localPosition = Vector3.zero;
        }
    }
}
