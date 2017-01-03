﻿#if !UNITY_EDITOR
#if UNITY_ANDROID
#define ANDROID_DEVICE
#elif UNITY_IPHONE
#define IOS_DEVICE
#elif UNITY_STANDALONE_WIN
#define WIN_DEVICE
#endif
#endif

using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

#if UNITY_ANDROID



public sealed class PicoVRAndroidDevice : PicoVRBaseDevice
{
    #region Properties
    private bool canConnecttoActivity = false;
    public override bool CanConnecttoActivity
    {
        get { return canConnecttoActivity; }
        set
        {
            if (value != canConnecttoActivity)
                canConnecttoActivity = value;
        }
    }
    private UnityEngine.AndroidJavaObject activity;
    private static UnityEngine.AndroidJavaClass javaVrActivityClass;


    private static UnityEngine.AndroidJavaClass batteryjavaVrActivityClass;
    private AndroidJavaObject vrActivityObj;
    private static UnityEngine.AndroidJavaClass volumejavaVrActivityClass;

    private int Headweartype = (int)PicoVRConfigProfile.DeviceTypes.PICOVR_I;
    private bool PupillaryPoint = false;
    private Quaternion rot;
    private Vector3 pos;
    private bool usePhoneSensor = true;
    private int inittime = 0;
    private bool useHMD = false;
    private bool isFalcon = false;
    private static readonly Matrix4x4 flipZ = Matrix4x4.Scale(new Vector3(1, 1, -1));
    private const long NO_DOWNTIME = -1;
    private float w = 0, x = 0, y = 0, z = 0, fov = 90f;
    private int timewarpid = 0;
    private bool isInitrenderThread = false;
    private string UnityVersion = "0.7.1.0";
    private static string model;
    #endregion Properties

    public PicoVRAndroidDevice()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        Application.runInBackground = false;
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
        rot = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);
        pos = rot * new Vector3(0.0f, 0.0f, 0.0f);
        currEyeTextureIdx = 0;
        nextEyeTextureIdx = 1;
        inittime = 0;


        if (!canConnecttoActivity)
        {
            ConnectToAndriod();
        }
        for (int i = 0; i < eyeTextureCount; i++)
        {
            if (null == eyeTextures[i])
            {
                try
                {
                    ConfigureEyeTexture(i);
                }
                catch (Exception)
                {
                    Debug.LogError("ERROR");
                    throw;
                }
            }
        }

    }

    /// <summary>
    /// 连接安卓
    /// </summary>
    public void ConnectToAndriod()
    {
        try
        {
            UnityEngine.AndroidJavaClass unityPlayer = new UnityEngine.AndroidJavaClass("com.unity3d.player.UnityPlayer");
            activity = unityPlayer.GetStatic<UnityEngine.AndroidJavaObject>("currentActivity");
            javaVrActivityClass = new UnityEngine.AndroidJavaClass("com.picovr.picovrlib.VrActivity");
            SetInitActivity(activity.GetRawObject(), javaVrActivityClass.GetRawClass());
            canConnecttoActivity = true;
            CanConnecttoActivity = canConnecttoActivity;
            PicoVRManager.SDK.inPicovr = true;
            Headweartype = (int)PicoVRManager.SDK.DeviceType;
            SetPupillaryPoint(PupillaryPoint);
            model = javaVrActivityClass.CallStatic<string>("getBuildModel");
            Debug.Log("model = " + model);
            if (model == "Pico Neo DK")
            {
                model = "Falcon";
            }
            Debug.Log("SDK Version = " + GetSDKVersion() + ",UnityVersion=" + UnityVersion);
            Async = GetAsyncFlag();
            useHMD = GetSensorExternal();
            if (useHMD)
            {
                usePhoneSensor = false;
            }
            Debug.Log("ConnectToAndroid: useHMD = " + useHMD + ", usePhoneSensor = " + usePhoneSensor);

            if (model == "Falcon")
            {
                usePhoneSensor = false;
                useHMD = true;
                isFalcon = true;
                Debug.Log("ConnectToAndroid: useHMD = " + useHMD + ", usePhoneSensor = " + usePhoneSensor + ", isFalcon = " + isFalcon);
                Headweartype = (int)PicoVRConfigProfile.DeviceTypes.PicoNeo;
                Debug.Log("Falcon : " + Headweartype.ToString());
                CallStaticMethod(javaVrActivityClass, "initFalconDevice", activity);
            }
            else
            {
                int deviceType = 0;
                CallStaticMethod<int>(ref deviceType, javaVrActivityClass, "readDeviceTypeFromWing", activity);
                Debug.Log("wingDeviceType = " + deviceType);
                if (deviceType != 0)
                {
                    Headweartype = deviceType;
                }
            }
            double[] parameters = new double[5];
            CallStaticMethod(ref parameters, javaVrActivityClass, "getDPIParameters", activity);
            ModifyScreenParameters(model, (int)parameters[0], (int)parameters[1], parameters[2], parameters[3], parameters[4]);
            ChangeHeadwear(Headweartype);
            SetAsyncModel(Async);
            Debug.Log("Async:" + Async.ToString() + "  Headweartype: " + Headweartype.ToString());
        }
        catch (AndroidJavaException e)
        {
            Debug.LogError("ConnectToAndriod------------------------catch" + e.Message);
        }
    }

    /// <summary>
    /// 调用静态方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="result"></param>
    /// <param name="jclass"></param>
    /// <param name="name"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public bool CallStaticMethod<T>(ref T result, UnityEngine.AndroidJavaClass jclass, string name, params object[] args)
    {
        try
        {
            result = jclass.CallStatic<T>(name, args);
            return true;
        }
        catch (AndroidJavaException e)
        {
            Debug.LogError("Exception calling static method " + name + ": " + e);
            return false;
        }
    }

    public bool CallStaticMethod(UnityEngine.AndroidJavaObject jobj, string name, params object[] args)
    {
        try
        {
            jobj.CallStatic(name, args);
            return true;
        }
        catch (AndroidJavaException e)
        {
            Debug.LogError("CallStaticMethod  Exception calling activity method " + name + ": " + e);
            return false;
        }
    }


    /// <summary>
    /// 调用方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="result"></param>
    /// <param name="jobj"></param>
    /// <param name="name"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public bool CallMethod<T>(ref T result, UnityEngine.AndroidJavaObject jobj, string name, params object[] args)
    {
        try
        {
            result = jobj.Call<T>(name, args);
            return true;
        }
        catch (AndroidJavaException e)
        {
            Debug.LogError("Exception calling activity method " + name + ": " + e);
            return false;
        }
    }

    /// <summary>
    /// 调用方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="result"></param>
    /// <param name="jobj"></param>
    /// <param name="name"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public bool CallMethod(UnityEngine.AndroidJavaObject jobj, string name, params object[] args)
    {
        try
        {
            jobj.Call(name, args);
            return true;
        }
        catch (AndroidJavaException e)
        {
            Debug.LogError(" Exception calling activity method " + name + ": " + e);
            return false;
        }
    }

    public override void startHidService()
    {
        Debug.LogError("colin startHidService ");
        CallStaticMethod(javaVrActivityClass, "startPeriodService", activity);
    }

    public override void stopHidService()
    {
        Debug.LogError("colin stopHidService ");
        CallStaticMethod(javaVrActivityClass, "stopPeriodService", activity);
    }

    //shine add
    public void startLarkConnectService()
    {
        Debug.LogError("shine startLarkConnectService ");
        CallStaticMethod(javaVrActivityClass, "startLarkConnectService", activity, PicoVRManager.SDK.gameObject.name);
    }

    public void stopLarkConnectService()
    {
        Debug.LogError("shine stopLarkConnectService ");
        CallStaticMethod(javaVrActivityClass, "stopLarkConnectService", activity);
    }
    //shine add end

    /// <summary>
    /// 
    /// </summary>
    public void UpdateFrameParamsFromActivity()
    {
        float[] frameInfo = null;
        frameInfo = UpdateRenderParams(1.0f, 1000.0f);
        if (frameInfo == null)
        {
            return;
        }
        int j = 0;
        for (int i = 0; i < 16; ++i, ++j)
        {
            PicoVRManager.SDK.headView[i] = frameInfo[j];
        }
        PicoVRManager.SDK.headView = flipZ * PicoVRManager.SDK.headView.inverse * flipZ;

    }
    public void UpdateBOXFrameParamsFromActivity()
    {
        float[] frameInfo = null;
        frameInfo = UpdateRenderParamsBox(1.0f, 1000.0f);
        if (frameInfo == null)
        {
            return;
        }
        int j = 0;
        for (int i = 0; i < 16; ++i, ++j)
        {
            PicoVRManager.SDK.boxView[i] = frameInfo[j];
        }
        PicoVRManager.SDK.boxView = flipZ * PicoVRManager.SDK.boxView.inverse * flipZ;

    }
    public float[] UpdateRenderParamsBox(float zNear, float zFar)
    {
        float ratation = 90.0f;

        float[] frameInfo = new float[16];
        Native_UpdateRenderParamsBox(frameInfo, ratation, zNear, zFar);
        return frameInfo;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>

    public static float[] UpdateRenderParams(float zNear, float zFar)
    {
        float ratation = 90.0f;
        if ("Falcon".Equals(model))
        {
            ratation = 360.0f;
        }
        float[] frameInfo = new float[16];
        Native_UpdateRenderParams(frameInfo, ratation, zNear, zFar);
        return frameInfo;
    }
    /// <summary>
    /// 
    /// </summary>
    public override void UpdateTextures()
    {
        if (PicoVRManager.SDK.onResume && inittime < 2401 && !Async)
        {
            for (int i = 0; i < 6; i++)
            {
                if (null == eyeTextures[i])
                {
                    try
                    {
                        ConfigureEyeTexture(i);
                    }
                    catch (Exception)
                    {
                        Debug.LogError("ERROR");
                        throw;
                    }
                }
                if (!eyeTextures[i].IsCreated())
                {
                    eyeTextures[i].Create();
                    eyeTextureIds[i] = eyeTextures[i].GetNativeTexturePtr().ToInt32();
                }
                eyeTextureIds[i] = eyeTextures[i].GetNativeTexturePtr().ToInt32();
            }
            inittime++;
            if (inittime == 2401)
            {
                PicoVRManager.SDK.onResume = false;
                inittime = 1;
            }
        }
        for (int i = 0; i < 6; i++)
        {
            if (null == eyeTextures[i])
            {
                try
                {
                    ConfigureEyeTexture(i);
                }
                catch (Exception)
                {
                    Debug.LogError("ERROR");
                    throw;
                }
            }
            if (!eyeTextures[i].IsCreated())
            {
                eyeTextures[i].Create();
                eyeTextureIds[i] = eyeTextures[i].GetNativeTexturePtr().ToInt32();
            }
        }
        currEyeTextureIdx = nextEyeTextureIdx;
        nextEyeTextureIdx = (nextEyeTextureIdx + 1) % 3;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="eyeTextureIndex"></param>
    private void ConfigureEyeTexture(int eyeTextureIndex)
    {
        Vector2 renderTexSize = GetStereoScreenSize();
        int x = (int)renderTexSize.x;
        int y = (int)renderTexSize.y;
        eyeTextures[eyeTextureIndex] = new RenderTexture(x, y, (int)PicoVRManager.SDK.RtBitDepth, PicoVRManager.SDK.RtFormat);
        eyeTextures[eyeTextureIndex].anisoLevel = 0;
        eyeTextures[eyeTextureIndex].antiAliasing = Mathf.Max(QualitySettings.antiAliasing, (int)PicoVRManager.SDK.RtAntiAlising);

        eyeTextures[eyeTextureIndex].Create();
        if (eyeTextures[eyeTextureIndex].IsCreated())
        {
            eyeTextureIds[eyeTextureIndex] = eyeTextures[eyeTextureIndex].GetNativeTexturePtr().ToInt32();
            Debug.Log("eyeTextureIndex : " + eyeTextureIndex.ToString());
        }

    }

    #region Inheritance
    public override void InitForEye(ref Material mat) { mat = null; }

    public override float GetSeparation()
    {
        return PicoVRManager.SDK.picoVRProfile.device.devLenses.separation;
    }

    public override void Init()
    {
        if (!isInitrenderThread && PicoVRManager.SDK.VRModeEnabled)
        {
            PVRPluginEvent.Issue(RenderEventType.InitRenderThread);
            isInitrenderThread = true;
        }
    }

    public override void SetVRModeEnabled(bool enabled) { }

    public override void SetDistortionCorrectionEnabled(bool enabled) { }

    public override Vector2 GetStereoScreenSize()
    {
        return new Vector2(1280, 1280);
    }

    public override void SetStereoScreen(RenderTexture stereoScreen) { }

    public override void resetFalconBoxSensor()
    {
        Native_ResetBoxTrack();
    }
    public override void ResetFalconBoxSensor()
    {
        Native_ResetBoxTrack();
    }

    public override void SetAutoDriftCorrectionEnabled(bool enabled) { }

    public override void UpdateState()
    {
        if (inittime == 0)
        {
            inittime++;
            if (isFalcon)
            {
                SetUsePredictedMatrix(true);
                StartHeadTrack();
                Debug.Log("model = " + model + ", StartHeadTrack--->");
            }
            else if (useHMD)
            {
                SetUsePredictedMatrix(true);
                Debug.Log("use HDM sensor.");
            }
            else if (usePhoneSensor)
            {
                StartHeadTrack();
                Debug.Log("use Phone sensor,  StartHeadTrack--->");
            }
            Debug.Log("inittime = " + inittime.ToString() + "          useHMD = " + useHMD.ToString());
            //Debug.Log ("colin before setDeviceProp default = " + getDeviceProp (4));
            //setDeviceProp (4, "-1");
            setDeviceCpuFreqDefault();
            //Debug.Log ("colin after setDeviceProp default = " + getDeviceProp (4));
        }

        if (useHMD && !usePhoneSensor)
        {
            GetSensorState(
                false,
                ref w,
                ref x,
                ref y,
                ref z,
                ref fov,
                ref timewarpid);
            PicoVRManager.SDK.timewarpID = timewarpid;
            PicoVRManager.SDK.eyeFov = fov;
            rot = new Quaternion(-x, -y, z, w);
            pos = rot * new Vector3(0.0f, 0.0f, 0.0f);
        }



        if (!useHMD && usePhoneSensor)
        {
            UpdateFrameParamsFromActivity();
            GetFOV(ref fov);
            rot = Quaternion.LookRotation(PicoVRManager.SDK.headView.GetColumn(2), PicoVRManager.SDK.headView.GetColumn(1));
            pos = PicoVRManager.SDK.headView.GetColumn(3);
            PicoVRManager.SDK.eyeFov = fov;
        }


        if (isFalcon && PicoVRManager.SDK.UseFalconBoxSensor)
        {
            UpdateBOXFrameParamsFromActivity();
            PicoVRManager.SDK.boxQuaternion = Quaternion.LookRotation(PicoVRManager.SDK.boxView.GetColumn(1), -PicoVRManager.SDK.boxView.GetColumn(2));
        }

        if (PicoVRManager.SDK.PVRNeck)
        {
            pos = (rot * new Vector3(0f, PicoVRManager.NECK_Y, PicoVRManager.NECK_Z) - PicoVRManager.NECK_Y * Vector3.up) * 1.0f;
        }
        else
            pos = rot * new Vector3(0.0f, 0.0f, 0.0f);
        PicoVRManager.SDK.headPose.Set(pos, rot);

    }

    public override void UpdateScreenData()
    {
        ComputeEyesFromProfile();
    }

    public override void Destroy()
    {
        try
        {
            base.Destroy();
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    public override void ResetHeadTrack()
    {
        if (PicoVRManager.SDK.reStartHead)
        {
            PicoVRManager.SDK.reStartHead = false;
            if (usePhoneSensor)
            {
                //headTrack.Call("resetTracker");
                Native_ResetHeadTrack();
            }
            else if (isFalcon)
            {
                PVR_ResetSensor();
            }
        }
    }

    public override void CloseHMDSensor()
    {
        PVR_CloseSensor();
    }

    public override void OpenHMDSensor()
    {
        PVR_OpenSensor();
    }

    public override void IsFocus(bool state)
    {
        PVR_SetFocus(state);
        //return true;
    }

    public override void StartHeadTrack()
    {
        Debug.Log("Jay StartHeadTrack,  isFalcon=" + isFalcon + ",UseFalconBoxSensor=" + PicoVRManager.SDK.UseFalconBoxSensor);
        if (usePhoneSensor || (isFalcon && PicoVRManager.SDK.UseFalconBoxSensor))
        {
            Native_StartHeadTrack();
        }
        //        if (usePhoneSensor||isFalcon)
        //        {
        //            headTrack.Call("startTracking");
        //        }
    }

    public override void StopHeadTrack()
    {
        if (usePhoneSensor || (isFalcon && PicoVRManager.SDK.UseFalconBoxSensor))
        {
            Native_StopHeadTrack();
        }
        //        if (usePhoneSensor||isFalcon)
        //        {
        //            headTrack.Call("stopTracking");
        //        }
    }

    public override void ChangeHeadwear(int headwear)
    {
        PVR_ChangeHeadwear(headwear);
    }

    public override Vector3 GetBoxSensorAcc()
    {
        Vector3 boxSensorDataAcc = new Vector3(0.0f, 0.0f, 0.0f);

        if (model == "Falcon")
        {
            float[] temp = new float[3];
            //temp = headTrack.Call<float[]> ("getBoxSensorDataAcc");//jar
            Native_GetBoxSensorAcc(temp);//so
            boxSensorDataAcc.x = temp[0];
            boxSensorDataAcc.y = temp[1];
            boxSensorDataAcc.z = temp[2];
        }
        else
        {
            Debug.LogError("GetBoxSensorAcc: Device model is " + model + ", not Falcon!");
        }

        return boxSensorDataAcc;
    }

    public override Vector3 GetBoxSensorGyr()
    {
        Vector3 boxSensorDataGyr = new Vector3(0.0f, 0.0f, 0.0f);

        if (model == "Falcon")
        {
            float[] temp = new float[3];
            //temp = headTrack.Call<float[]> ("getBoxSensorDataGyr");//jar
            Native_GetBoxSensorGyro(temp);//so
            boxSensorDataGyr.x = temp[0];
            boxSensorDataGyr.y = temp[1];
            boxSensorDataGyr.z = temp[2];
        }
        else
        {
            Debug.LogError("GetBoxSensorGyr: Device model is " + model + ", not Falcon!");
        }

        return boxSensorDataGyr;
    }


    /*****************************音量亮度*************************************/
    public override bool initBatteryVolClass()
    {
        try
        {
            if (javaVrActivityClass != null && activity != null)
            {
                batteryjavaVrActivityClass = new UnityEngine.AndroidJavaClass("com.picovr.picovrlib.BatteryReceiver");
                volumejavaVrActivityClass = new AndroidJavaClass("com.picovr.picovrlib.AudioReceiver");
                CallStaticMethod(javaVrActivityClass, "initAudioDevice", activity);
                CallStaticMethod<AndroidJavaObject>(ref vrActivityObj, javaVrActivityClass, "InitVRActivity");
                Debug.Log("INIT");
                return true;
            }
            else
                return false;
        }
        catch (Exception e)
        {
            Debug.LogError("startReceiver Error :" + e.ToString());
            return false;
        }
    }
    public override bool InitBatteryVolClass()
    {
        try
        {
            if (javaVrActivityClass != null && activity != null)
            {
                batteryjavaVrActivityClass = new UnityEngine.AndroidJavaClass("com.picovr.picovrlib.BatteryReceiver");
                volumejavaVrActivityClass = new AndroidJavaClass("com.picovr.picovrlib.AudioReceiver");
                CallStaticMethod(javaVrActivityClass, "initAudioDevice", activity);
                //CallStaticMethod<AndroidJavaObject>(ref vrActivityObj, javaVrActivityClass, "InitVRActivity");
                Debug.Log("INIT");
                return true;
            }
            else
                return false;
        }
        catch (Exception e)
        {
            Debug.LogError("startReceiver Error :" + e.ToString());
            return false;
        }
    }
    public override bool startAudioReceiver()
    {
        try
        {
            string startreceivre = PicoVRManager.SDK.gameObject.name;
            CallStaticMethod(volumejavaVrActivityClass, "startReceiver", activity, startreceivre);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("startReceiver Error :" + e.ToString());
            return false;
        }
    }
    public override bool StartAudioReceiver()
    {
        try
        {
            string startreceivre = PicoVRManager.SDK.gameObject.name;
            CallStaticMethod(volumejavaVrActivityClass, "startReceiver", activity, startreceivre);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("startReceiver Error :" + e.ToString());
            return false;
        }
    }

    public override bool startBatteryReceiver()
    {
        try
        {
            string startreceivre = PicoVRManager.SDK.gameObject.name;
            CallStaticMethod(batteryjavaVrActivityClass, "startReceiver", activity, startreceivre);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("startReceiver Error :" + e.ToString());
            return false;
        }
    }
    public override bool StartBatteryReceiver()
    {
        try
        {
            string startreceivre = PicoVRManager.SDK.gameObject.name;
            CallStaticMethod(batteryjavaVrActivityClass, "startReceiver", activity, startreceivre);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("startReceiver Error :" + e.ToString());
            return false;
        }
    }

    public override bool stopAudioReceiver()
    {
        try
        {
            CallStaticMethod(volumejavaVrActivityClass, "stopReceiver", activity);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("startReceiver Error :" + e.ToString());
            return false;
        }

    }
    public override bool StopAudioReceiver()
    {
        try
        {
            CallStaticMethod(volumejavaVrActivityClass, "stopReceiver", activity);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("startReceiver Error :" + e.ToString());
            return false;
        }

    }

    public override bool stopBatteryReceiver()
    {
        try
        {
            CallStaticMethod(batteryjavaVrActivityClass, "stopReceiver", activity);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("startReceiver Error :" + e.ToString());
            return false;
        }
    }
    public override bool StopBatteryReceiver()
    {
        try
        {
            CallStaticMethod(batteryjavaVrActivityClass, "stopReceiver", activity);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("startReceiver Error :" + e.ToString());
            return false;
        }
    }
    public override bool setDevicePropForUser(PicoVRConfigProfile.DeviceCommand deviceid, string number)
    {
        bool istrue = false;
        CallStaticMethod<bool>(ref istrue, javaVrActivityClass, "setDevicePropForUser", (int)deviceid, number);
        return istrue;
    }
    public override bool SetDevicePropForUser(PicoVRConfigProfile.DeviceCommand deviceid, string number)
    {
        bool istrue = false;
        CallStaticMethod<bool>(ref istrue, javaVrActivityClass, "setDevicePropForUser", (int)deviceid, number);
        return istrue;
    }
    public override string getDevicePropForUser(PicoVRConfigProfile.DeviceCommand deviceid)
    {
        string istrue = "0";
        CallStaticMethod<string>(ref istrue, javaVrActivityClass, "getDevicePropForUser", (int)deviceid);
        return istrue;
    }
    public override string GetDevicePropForUser(PicoVRConfigProfile.DeviceCommand deviceid)
    {
        string istrue = "0";
        CallStaticMethod<string>(ref istrue, javaVrActivityClass, "getDevicePropForUser", (int)deviceid);
        return istrue;
    }
    public override string getDeviceModelName()
    {
        if (null != model)
        {
            return model;
        }
        else
        {
            return "unknow";
        }
    }
    public override string GetDeviceModelName()
    {
        if (null != model)
        {
            return model;
        }
        else
        {
            return "unknow";
        }
    }
    public override int getMaxVolumeNumber()
    {
        int maxvolm = 0;
        try
        {
            CallStaticMethod<int>(ref maxvolm, javaVrActivityClass, "getMaxAudionumber");
        }
        catch (Exception e)
        {
            Debug.LogError(" Error :" + e.ToString());
        }
        return maxvolm;
    }
    public override int GetMaxVolumeNumber()
    {
        int maxvolm = 0;
        try
        {
            CallStaticMethod<int>(ref maxvolm, javaVrActivityClass, "getMaxAudionumber");
        }
        catch (Exception e)
        {
            Debug.LogError(" Error :" + e.ToString());
        }
        return maxvolm;
    }
    public override int getCurrentVolumeNumber()
    {
        int currentvolm = 0;
        try
        {
            CallStaticMethod<int>(ref currentvolm, javaVrActivityClass, "getAudionumber");
        }
        catch (Exception e)
        {
            Debug.LogError(" Error :" + e.ToString());
        }
        return currentvolm;
    }
    public override int GetCurrentVolumeNumber()
    {
        int currentvolm = 0;
        try
        {
            CallStaticMethod<int>(ref currentvolm, javaVrActivityClass, "getAudionumber");
        }
        catch (Exception e)
        {
            Debug.LogError(" Error :" + e.ToString());
        }
        return currentvolm;
    }

    public override bool volumeUp()
    {
        try
        {
            CallStaticMethod(javaVrActivityClass, "UpAudio");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(" Error :" + e.ToString());
            return false;
        }
    }
    public override bool VolumeUp()
    {
        try
        {
            CallStaticMethod(javaVrActivityClass, "UpAudio");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(" Error :" + e.ToString());
            return false;
        }
    }

    public override bool volumeDown()
    {
        try
        {
            CallStaticMethod(javaVrActivityClass, "DownAudio");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(" Error :" + e.ToString());
            return false;
        }
    }
    public override bool VolumeDown()
    {
        try
        {
            CallStaticMethod(javaVrActivityClass, "DownAudio");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(" Error :" + e.ToString());
            return false;
        }
    }

    public override bool setVolumeNum(int volume)
    {
        try
        {
            CallStaticMethod(javaVrActivityClass, "changeAudio", volume);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(" Error :" + e.ToString());
            return false;
        }
    }
    public override bool SetVolumeNum(int volume)
    {
        try
        {
            CallStaticMethod(javaVrActivityClass, "changeAudio", volume);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(" Error :" + e.ToString());
            return false;
        }
    }

    public override bool setBrightness(int brightness)
    {
        try
        {
            CallMethod(vrActivityObj, "setScreen_Brightness", brightness, activity);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(" Error :" + e.ToString());
            return false;
        }
    }
    public override bool SetBrightness(int brightness)
    {
        try
        {
            CallMethod(vrActivityObj, "setScreen_Brightness", brightness, activity);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(" Error :" + e.ToString());
            return false;
        }
    }

    public override int getCurrentBrightness()
    {
        int currentlight = 0;
        try
        {
            CallMethod<int>(ref currentlight, vrActivityObj, "getScreen_Brightness", activity);
        }
        catch (Exception e)
        {
            Debug.LogError(" Error :" + e.ToString());
        }
        return currentlight;
    }
    public override int GetCurrentBrightness()
    {
        int currentlight = 0;
        try
        {
            CallMethod<int>(ref currentlight, vrActivityObj, "getScreen_Brightness", activity);
        }
        catch (Exception e)
        {
            Debug.LogError(" Error :" + e.ToString());
        }
        return currentlight;
    }
    /*******************************音量亮度****************************************/

    /*********************************************************************************/
    #endregion Inheritance

    #region Unity interface
    /*************************** unity interface *************************************/
	public override string GetSDKVersion()
    {
        IntPtr ptr = PVR_GetSDKVersion();
        if (ptr != IntPtr.Zero)
        {
            return Marshal.PtrToStringAnsi(ptr);
        }
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="model"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="xppi"></param>
    /// <param name="yppi"></param>
    /// <param name="densityDpi"></param>
    public void ModifyScreenParameters(string model, int width, int height,
          double xppi, double yppi, double densityDpi)
    {
        PVR_ChangeScreenParameters(model, width, height, xppi, yppi, densityDpi);
    }

    /// <summary>
    /// 更改同步异步类型
    /// </summary>
    /// <param name="async"></param> 
    private void SetAsyncModel(bool async)
    {
        PVR_SetAsyncTimeWarp(async);
    }

    /// <summary>
    /// 初始化activity
    /// </summary>
    private void SetInitActivity(IntPtr activity, IntPtr vrActivityClass)
    {
        PVR_SetInitActivity(activity, vrActivityClass);
    }

    /// <summary>
    /// 是否需开启Pupillary point
    /// </summary>
    private void SetPupillaryPoint(bool enable)
    {
        PVR_SetPupillaryPoint(enable);
    }

    private bool GetAsyncFlag()
    {
        bool asycflag = false;
        asycflag = PVR_GetAsyncFlag();
        return asycflag;
    }

    private bool GetSensorExternal()
    {
        bool sensorexternal = false;
        sensorexternal = PVR_GetSensorExternal();
        return sensorexternal;
    }

    private void SetUsePredictedMatrix(bool enable)
    {
        PVR_SetUsePredictedMatrix(enable);
    }

    private void GetSensorState(bool monoscopic, ref float w, ref float x, ref float y, ref float z, ref float fov, ref int timewarpid)
    {
        PVR_GetSensorState(
                monoscopic,
                ref w,
                ref x,
                ref y,
                ref z,
                ref fov,
                ref timewarpid);
    }

    private void GetFOV(ref float fov)
    {
        PVR_GetFOV(ref fov);
    }
    private int GetPsensorState()
    {
        int state = PVR_GetPsensorState();
        return state;
    }
    private bool GetUsePredictedMatrix()
    {
        bool premat = false;
        premat = PVR_GetUsePredictedMatrix();
        return premat;
    }

    private float GetBatteryLevel()
    {
        float batterylevel = 100.0f;
        batterylevel = PVR_GetBatteryLevel();
        return batterylevel;
    }

    /*************************** unity interface *************************************/
    #endregion unity interface

    #region Haptics

    /***************************Haptics*************************************/
    public static int HAPTICS_LEFT = 0x01;
    public static int HAPTICS_RIGHT = 0x02;
    public static int HAPTICS_ALL = 0x03;
    public static int HAPTICS_HAPTICTHEME_SIP = 1;
    public static int HAPTICS_HAPTICTHEME_DIALPAD = 2;
    public static int HAPTICS_HAPTICTHEME_LAUNCHER = 3;
    public static int HAPTICS_HAPTICTHEME_LONGPRESS = 4;
    public static int HAPTICS_HAPTICTHEME_VIRTUALKEY = 5;
    public static int HAPTICS_HAPTICTHEME_ROTATE = 7;
    public static int HAPTICS_HAPTICTHEME_GALLERY = 8;
    public static int HAPTICS_HAPTICTHEME_LOCKSCREEN = 9;
    public static int HAPTICS_HAPTICTHEME_TRY_UNLOCK = 10;
    public static int HAPTICS_HAPTICTHEME_MULTITOUCH = 11;
    public static int HAPTICS_HAPTICTHEME_SCROLLING = 12;
    public static String DATA_HAPTICTHEME_VIRTUALKEY = "data_haptictheme_virtualkey";
    public static String DATA_HAPTICTHEME_LONGPRESS = "data_haptictheme_longpress";
    public static String DATA_HAPTICTHEME_LAUNCHER = "data_haptictheme_launcher";
    public static String DATA_HAPTICTHEME_DIALPAD = "data_haptictheme_dialpad";
    public static String DATA_HAPTICTHEME_SIP = "data_haptictheme_SIP";
    public static String DATA_HAPTICTHEME_ROTATE = "data_haptictheme_rotate";
    public static String DATA_HAPTICTHEME_GALLERY = "data_haptictheme_gallery";
    public static String DATA_HAPTICTHEME_SCROLL = "data_haptictheme_scroll";
    public static String DATA_HAPTICTHEME_MULTI_TOUCH = "data_haptictheme_multi_touch";
    public static String DATA_HAPTIC_VIBRATE = "haptic_vibrate_data";
    public static String DATA_HAPTIC_A2H = "haptic_A2H_data";

    public override void playeffect(int effectID, int whichHaptic)
    {
        // Log.e("berton", "========playeffectandroid================");
        CallStaticMethod(javaVrActivityClass, "playeffect", effectID, whichHaptic);
    }
    public override void PlayEffect(int effectID, int whichHaptic)
    {
        // Log.e("berton", "========playeffectandroid================");
        CallStaticMethod(javaVrActivityClass, "playeffect", effectID, whichHaptic);
    }

    public override void playEffectSequence(string sequence, int whichHaptic)
    {
        CallStaticMethod(javaVrActivityClass, "playEffectSequence", sequence, whichHaptic);
    }
    public override void PlayEffectSequence(string sequence, int whichHaptic)
    {
        CallStaticMethod(javaVrActivityClass, "playEffectSequence", sequence, whichHaptic);
    }

    public override void setAudioHapticEnabled(bool enable, int whichHaptic)
    {
        CallStaticMethod(javaVrActivityClass, "setAudioHapticEnabled", enable, whichHaptic);
    }
    public override void SetAudioHapticEnabled(bool enable, int whichHaptic)
    {
        CallStaticMethod(javaVrActivityClass, "setAudioHapticEnabled", enable, whichHaptic);
    }

    public override void stopPlayingEffect(int whichHaptic)
    {
        CallStaticMethod(javaVrActivityClass, "stopPlayingEffect", whichHaptic);
    }
    public override void StopPlayingEffect(int whichHaptic)
    {
        CallStaticMethod(javaVrActivityClass, "stopPlayingEffect", whichHaptic);
    }
    public override void playeffectforce(int effectID, int whichHaptic)
    {
        CallStaticMethod(javaVrActivityClass, "playeffectforce", effectID, whichHaptic);
    }
    public override void Playeffectforce(int effectID, int whichHaptic)
    {
        CallStaticMethod(javaVrActivityClass, "playeffectforce", effectID, whichHaptic);
    }
    public override void playTimedEffect(int effectDuration, int whichHaptic)
    {
        CallStaticMethod(javaVrActivityClass, "playTimedEffect", effectDuration, whichHaptic);
    }
    public override void PlayTimedEffect(int effectDuration, int whichHaptic)
    {
        CallStaticMethod(javaVrActivityClass, "playTimedEffect", effectDuration, whichHaptic);
    }
    public override void playPatternRTP(float vibrationDuration, float vibrationStrength, int whichHaptic, bool large, bool small, int repeat_times, float silienceDuration, float HapticsDuration)
    {
        CallStaticMethod(javaVrActivityClass, "playPatternRTP", vibrationDuration, vibrationStrength, whichHaptic, large, small, repeat_times, silienceDuration, HapticsDuration);
    }
    public override void PlayPatternRTP(float vibrationDuration, float vibrationStrength, int whichHaptic, bool large, bool small, int repeat_times, float silienceDuration, float HapticsDuration)
    {
        CallStaticMethod(javaVrActivityClass, "playPatternRTP", vibrationDuration, vibrationStrength, whichHaptic, large, small, repeat_times, silienceDuration, HapticsDuration);
    }
    public override void playEffectSeqBuff(byte[] Sequence, int buffSize, int whichHaptic)
    {
        CallStaticMethod(javaVrActivityClass, "playEffectSeqBuff", Sequence, buffSize, whichHaptic);
    }
    public override void PlayEffectSeqBuff(byte[] Sequence, int buffSize, int whichHaptic)
    {
        CallStaticMethod(javaVrActivityClass, "playEffectSeqBuff", Sequence, buffSize, whichHaptic);
    }
    public override void playRTPSequence(String sequence, int whichHaptic)
    {
        CallStaticMethod(javaVrActivityClass, "playRTPSequence", sequence, whichHaptic);
    }
    public override void PlayRTPSequence(String sequence, int whichHaptic)
    {
        CallStaticMethod(javaVrActivityClass, "playRTPSequence", sequence, whichHaptic);
    }
    public override void playRTPSeqBuff(byte[] Sequence, int buffSize, int whichHaptic)
    {
        CallStaticMethod(javaVrActivityClass, "playRTPSeqBuff", Sequence, buffSize, whichHaptic);
    }
    public override void PlayRTPSeqBuff(byte[] Sequence, int buffSize, int whichHaptic)
    {
        CallStaticMethod(javaVrActivityClass, "playRTPSeqBuff", Sequence, buffSize, whichHaptic);
    }
    public override void playRingHaptics(int index, int whichHaptic)
    {
        CallStaticMethod(javaVrActivityClass, "playRingHaptics", index, whichHaptic);
    }
    public override void PlayRingHaptics(int index, int whichHaptic)
    {
        CallStaticMethod(javaVrActivityClass, "playRingHaptics", index, whichHaptic);
    }
    public override void playRingSeq(int index, int whichHaptic)
    {
        CallStaticMethod(javaVrActivityClass, "playRingSeq", index, whichHaptic);
    }
    public override void PlayRingSeq(int index, int whichHaptic)
    {
        CallStaticMethod(javaVrActivityClass, "playRingSeq", index, whichHaptic);
    }
    public override string getRingHapticsName()
    {
        string value = null;
        CallStaticMethod<string>(ref value, javaVrActivityClass, "getRingHapticsName");
        return value;
    }
    public override string GetRingHapticsName()
    {
        string value = null;
        CallStaticMethod<string>(ref value, javaVrActivityClass, "getRingHapticsName");
        return value;
    }
    public override string getRingHapticsValues()
    {
        string value = null;
        CallStaticMethod<string>(ref value, javaVrActivityClass, "getRingHapticsValues");
        return value;
    }
    public override string GetRingHapticsValues()
    {
        string value = null;
        CallStaticMethod<string>(ref value, javaVrActivityClass, "getRingHapticsValues");
        return value;
    }
    public override string getRingHapticsValue(int index)
    {
        string value = null;
        CallStaticMethod<string>(ref value, javaVrActivityClass, "getRingHapticsValue", index);
        return value;
    }
    public override string GetRingHapticsValue(int index)
    {
        string value = null;
        CallStaticMethod<string>(ref value, javaVrActivityClass, "getRingHapticsValue", index);
        return value;
    }
    /***************************Haptics*************************************/

    /****************************AM3d*******************************************/
    public override void openEffects()
    {
        CallStaticMethod(javaVrActivityClass, "openEffects");
    }
    public override void OpenEffects()
    {
        CallStaticMethod(javaVrActivityClass, "openEffects");
    }
    public override void closeEffects()
    {
        CallStaticMethod(javaVrActivityClass, "closeEffects");
    }
    public override void CloseEffects()
    {
        CallStaticMethod(javaVrActivityClass, "closeEffects");
    }
    public override void setSurroundroomType(int type)
    {
        CallStaticMethod(javaVrActivityClass, "setSurroundroomType", type);
    }
    public override void SetSurroundroomType(int type)
    {
        CallStaticMethod(javaVrActivityClass, "setSurroundroomType", type);
    }
    public override void openRoomcharacteristics()
    {
        CallStaticMethod(javaVrActivityClass, "openRoomcharacteristics");
    }
    public override void OpenRoomcharacteristics()
    {
        CallStaticMethod(javaVrActivityClass, "openRoomcharacteristics");
    }
    public override void closeRoomcharacteristics()
    {
        CallStaticMethod(javaVrActivityClass, "closeRoomcharacteristics");
    }
    public override void CloseRoomcharacteristics()
    {
        CallStaticMethod(javaVrActivityClass, "closeRoomcharacteristics");
    }
    public override void EnableSurround()
    {
        CallStaticMethod(javaVrActivityClass, "EnableSurround");
    }
    public override void EnableReverb()
    {
        CallStaticMethod(javaVrActivityClass, "EnableReverb");
    }
    public override void startAudioEffect(String audioFile, bool isSdcard)
    {
        Debug.Log("AiLi  CallStaticMethod startAudioEffect    + path  " + audioFile.ToString());

        CallStaticMethod(javaVrActivityClass, "startAudioEffect", activity, audioFile, isSdcard);
    }
    public override void StartAudioEffect(String audioFile, bool isSdcard)
    {
        Debug.Log("AiLi  CallStaticMethod startAudioEffect    + path  " + audioFile.ToString());

        CallStaticMethod(javaVrActivityClass, "startAudioEffect", activity, audioFile, isSdcard);
    }
    public override void stopAudioEffect()
    {
        CallStaticMethod(javaVrActivityClass, "stopAudioEffect");
    }
    public override void StopAudioEffect()
    {
        CallStaticMethod(javaVrActivityClass, "stopAudioEffect");
    }
    public override void ReleaseAudioEffect()
    {
        CallStaticMethod(javaVrActivityClass, "releaseAudio");
    }
    /****************************AM3d*******************************************/

    /***************************Touch*************************************/
    public override void enableTouchPad(bool enable)
    {
        CallStaticMethod(javaVrActivityClass, "enableTouchPad", enable);
    }
    public override void EnableTouchPad(bool enable)
    {
        CallStaticMethod(javaVrActivityClass, "enableTouchPad", enable);
    }

    public override void switchTouchType(int device)
    {
        CallStaticMethod(javaVrActivityClass, "switchTouchType", device);
    }
    public override void SwitchTouchType(int device)
    {
        CallStaticMethod(javaVrActivityClass, "switchTouchType", device);
    }

    public override int getTouchPadStatus()
    {
        int i = 0;
        CallStaticMethod<int>(ref i, javaVrActivityClass, "getTouchPadStatus");
        return i;
    }
    public override int GetTouchPadStatus()
    {
        int i = 0;
        CallStaticMethod<int>(ref i, javaVrActivityClass, "getTouchPadStatus");
        return i;
    }

    public bool setDeviceCpuFreqDefault()
    {
        bool istrue = false;
        CallStaticMethod<bool>(ref istrue, javaVrActivityClass, "setDeviceCpuFreqDefault");
        return istrue;
    }
    public bool SetDeviceCpuFreqDefault()
    {
        bool istrue = false;
        CallStaticMethod<bool>(ref istrue, javaVrActivityClass, "setDeviceCpuFreqDefault");
        return istrue;
    }

    public override bool setDeviceProp(int device_id, string value)
    {
        bool istrue = false;
        CallStaticMethod<bool>(ref istrue, javaVrActivityClass, "setDeviceProp", device_id, value);
        return istrue;
    }
    public override bool SetDeviceProp(int device_id, string value)
    {
        bool istrue = false;
        CallStaticMethod<bool>(ref istrue, javaVrActivityClass, "setDeviceProp", device_id, value);
        return istrue;
    }

    public override string getDeviceProp(int device_id)
    {
        string str = "";
        CallStaticMethod<string>(ref str, javaVrActivityClass, "getDeviceProp", device_id);
        return str;
    }
    public override string GetDeviceProp(int device_id)
    {
        string str = "";
        CallStaticMethod<string>(ref str, javaVrActivityClass, "getDeviceProp", device_id);
        return str;
    }

    public override bool requestHidSensor(int user)
    {
        bool istrue = false;
        CallStaticMethod<bool>(ref istrue, javaVrActivityClass, "requestHidSensor", user);
        return istrue;
    }
    public override bool RequestHidSensor(int user)
    {
        bool istrue = false;
        CallStaticMethod<bool>(ref istrue, javaVrActivityClass, "requestHidSensor", user);
        return istrue;
    }

    public override int getHidSensorUser()
    {
        int i = 0;
        CallStaticMethod<int>(ref i, javaVrActivityClass, "getHidSensorUser");
        return i;
    }
    public override int GetHidSensorUser()
    {
        int i = 0;
        CallStaticMethod<int>(ref i, javaVrActivityClass, "getHidSensorUser");
        return i;
    }

    public override bool setThreadRunCore(int pid, int core_id)
    {
        bool istrue = false;
        CallStaticMethod<bool>(ref istrue, javaVrActivityClass, "setThreadRunCore", pid, core_id);
        return istrue;

    }
    public override bool SetThreadRunCore(int pid, int core_id)
    {
        bool istrue = false;
        CallStaticMethod<bool>(ref istrue, javaVrActivityClass, "setThreadRunCore", pid, core_id);
        return istrue;

    }

    public override int getThreadRunCore(int pid)
    {
        int i = 0;
        CallStaticMethod<int>(ref i, javaVrActivityClass, "getThreadRunCore", pid);
        return i;
    }
    public override int GetThreadRunCore(int pid)
    {
        int i = 0;
        CallStaticMethod<int>(ref i, javaVrActivityClass, "getThreadRunCore", pid);
        return i;
    }
    public override bool setSystemRunLevel(int device_id, int level)
    {
        bool istrue = false;
        CallStaticMethod<bool>(ref istrue, javaVrActivityClass, "setSystemRunLevel", device_id, level);
        return istrue;
    }
    public override bool SetSystemRunLevel(int device_id, int level)
    {
        bool istrue = false;
        CallStaticMethod<bool>(ref istrue, javaVrActivityClass, "setSystemRunLevel", device_id, level);
        return istrue;
    }

    public override int getSystemRunLevel(int device_id)
    {
        int i = 0;
        CallStaticMethod<int>(ref i, javaVrActivityClass, "getSystemRunLevel", device_id);
        return i;
    }
    public override int GetSystemRunLevel(int device_id)
    {
        int i = 0;
        CallStaticMethod<int>(ref i, javaVrActivityClass, "getSystemRunLevel", device_id);
        return i;
    }
    /***************************Touch*************************************/
    #endregion Haptics

    #region PicoPlugin
    /***************************PicoPlugin*************************************/
    public const string LibFileName = "PicoPlugin";
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void PVR_SetInitActivity(IntPtr activity, IntPtr vrActivityClass);
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void PVR_SetUsePredictedMatrix(bool use);
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool PVR_GetUsePredictedMatrix();
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float PVR_GetBatteryLevel();
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    private static extern float PVR_GetBatteryTemperature();
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void PVR_SetMinimumVsyncs(int minimumVsyncs);
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool PVR_IsPowerSaved();
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void PVR_ChangeHeadwear(int type);
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void PVR_SetPupillaryPoint(bool enable);
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void PVR_ChangeScreenParameters([MarshalAs(UnmanagedType.LPStr)]string model, int width, int height, double xppi, double yppi, double densityDpi);
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool PVR_GetSensorState(
        bool monoscopic,
        ref float w,
        ref float x,
        ref float y,
        ref float z,
        ref float fov,
        ref int viewNumber);
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool PVR_GetSensorExternal();
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void PVR_CloseSensor();
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void PVR_OpenSensor();
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool PVR_SetFocus(bool state);
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool PVR_ResetSensor();
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void PVR_SetHMDBrightness(int value);
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int PVR_GetHMDBrightness();
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr PVR_GetSDKVersion();
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int PVR_GetBatteryStatus();
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void PVR_SetAllowPowerSave(bool allow);
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void PVR_EnableChromaticAberration(bool enable);
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void PVR_SetAsyncTimeWarp(bool enable);
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void PVR_SetVsyncFrameRate(double rate);
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool PVR_GetAsyncFlag();
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void PVR_GetFOV(ref float fov);
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void Native_StartHeadTrack();
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void Native_StopHeadTrack();
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void Native_ResetHeadTrack();
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool Native_UpdateRenderParams(float[] renderParams, float rotation, float zNear, float zFar);
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool Native_UpdateRenderParamsBox(float[] renderParams, float rotation, float zNear, float zFar);
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]

    private static extern void Native_GetBoxSensorAcc(float[] boxAcc);
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void Native_GetBoxSensorGyro(float[] boxGyro);
    //add interface : getPsensorState
    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int PVR_GetPsensorState();

    [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void Native_ResetBoxTrack();
    #endregion PicoPlugin
}
#endif