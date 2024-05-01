using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class ScreenShot : MonoBehaviour
{

    // Update is called once per frame
    public void TakeScreenshot(string screenshotName)
    { 
            ScreenCapture.CaptureScreenshot(Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), screenshotName + ".png"), 8);
            Debug.Log("Screenshot Captured");
    }
}