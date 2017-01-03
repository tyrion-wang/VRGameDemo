﻿using UnityEngine;
using UnityEditor;


[InitializeOnLoad]
public class AndriodBuilSetting : Editor
{

    // Use this for initialization
    void Start()
    {
        PlayerSettings.MTRendering = true;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.companyName = "Pico";
        //PlayerSettings.gpuSkinning = true;
        PlayerSettings.mobileMTRendering = false;
        PlayerSettings.productName = "PicoVRSDK";
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Enabled;//Disabled;
        PlayerSettings.defaultIsFullScreen = true;
        EditorUserBuildSettings.activeBuildTargetChanged += OnChangePlatform;
    }

    // Update is called once per frame
    static AndriodBuilSetting()
    {
        EditorUserBuildSettings.activeBuildTargetChanged += OnChangePlatform;
    }
    static void OnChangePlatform()
    {
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
        {
            PlayerSettings.MTRendering = true;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.companyName = "Pico";
            //PlayerSettings.gpuSkinning = true;
            PlayerSettings.mobileMTRendering = false;
            PlayerSettings.productName = "PicoVRSDK";
           
        }
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows)
        {           
            PlayerSettings.companyName = "Pico";
            PlayerSettings.productName = "PicoVRSDK";
            PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Enabled;//Disabled;
            PlayerSettings.defaultIsFullScreen = true;
            //EditorUserBuildSettings.selectedStandaloneTarget = BuildTarget.StandaloneWindows64;
        }


    }

    [MenuItem("PicoVR/APK Setting")]
    static void PerformAndroidAPKBuild()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);
        PlayerSettings.MTRendering = true;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.companyName = "Pico";
        // PlayerSettings.gpuSkinning = true;
        PlayerSettings.mobileMTRendering = false;
        PlayerSettings.productName = "PicoVRSDK";
    }
    [MenuItem("PicoVR/WinPC Setting")]
    static void PerformPCSDKBuild()
    {
        PlayerSettings.companyName = "Pico";
        PlayerSettings.productName = "PicoVRSDK";
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Enabled;
        PlayerSettings.defaultIsFullScreen = false;
        // EditorUserBuildSettings.selectedStandaloneTarget = BuildTarget.StandaloneWindows64;

    }
   
}
