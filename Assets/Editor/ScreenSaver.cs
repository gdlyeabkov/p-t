using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using UnityEditor.Sprites;

public class ScreenSaver : ScriptableWizard
{

    [MenuItem("Custom Plugins/Take screenshot")]
    static void TakeScreenshot()
    {
        string fullPath = @"C:\unity_projects\PiratesAndTreasures\Assets\screenshot.png"; 
        ScreenCapture.CaptureScreenshot(fullPath);
    }

}
