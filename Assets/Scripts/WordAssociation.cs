using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.XR;

public class WordAssociation : MonoBehaviour
{
    string[] words ={ "Bored","Content","Excited","Tired","Calm","Curious","Frustrated","Unbothered","Happy",
        "Uninterested","Indifferent","Immersed","Overwhelmed","Distracted","Engaged","Exhausted","Relaxed","Concentrated"};

    TMP_Text[] texts;
    public Transform textParent;
    private System.Random _random = new System.Random();
    int temp = 0;

    private void Start()
    {
        texts = textParent.GetComponentsInChildren<TMP_Text>();

        ShuffleTexts();
    }

    //for shuffle number from array
    void Shuffle(string[] array)
    {
        int p = array.Length;
        for (int n = p - 1; n > 0; --n)
        {
            int r = UnityEngine.Random.Range(0, n);
            string t = array[r];
            array[r] = array[n];
            array[n] = t;
        }
    }
    
    void ShuffleTexts()
    {
        Shuffle(words);

        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].text = words[i];
        }
    }

    private void Update()
    {

        if (Input.GetKeyUp(KeyCode.P))
        {
            ShuffleTexts();
            TakeScreenshot("WordAssociation");
        }
    }

    void TakeScreenshot(string screenshotName)
    {
        ScreenCapture.CaptureScreenshot(Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "screenshot" + screenshotName + temp.ToString() + ".png"));
        temp++;
        Debug.Log("Screenshot Captured");
    }

}
