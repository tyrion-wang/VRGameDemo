
using UnityEngine;
using System;
using System.IO;
using System.Runtime.InteropServices;

/// <summary>
/// Matches the events in the native plugin.
/// </summary>
public enum RenderEventType
{
	// Android
	InitRenderThread = 0,
	Pause = 1,
	Resume = 2,
	LeftEyeEndFrame = 3,
	RightEyeEndFrame = 4,
	TimeWarp = 5,
	ResetVrModeParms = 6,
	ShutdownRenderThread = 7,
}

public struct quat
{
    public float x;
    public float y;
    public float z;
    public float w;
};

public struct RT
{

    public quat orientation;
    public IntPtr Ltext;
    public IntPtr Rtext;
}

/// <summary>
/// Communicates with native plugin functions that run on the rendering thread.
/// </summary>
public static class PVRPluginEvent
{
	/// <summary>
	/// Immediately issues the given event.
	/// </summary>
	public static void Issue(RenderEventType eventType)
	{
		GL.IssuePluginEvent(EncodeType((int)eventType));
	}

    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="eventData"></param>
    public static void IssueWithData(RenderEventType eventType, int eventData)
	{
		// Encode and send-two-bytes of data
		GL.IssuePluginEvent(EncodeData((int)eventType, eventData, 0));

		// Encode and send remaining two-bytes of data
		GL.IssuePluginEvent(EncodeData((int)eventType, eventData, 1));

		// Explicit event that uses the data
		GL.IssuePluginEvent(EncodeType((int)eventType));
	}


	private const UInt32 IS_DATA_FLAG = 0x80000000;
	private const UInt32 DATA_POS_MASK = 0x40000000;
	private const int DATA_POS_SHIFT = 30;
	private const UInt32 EVENT_TYPE_MASK = 0x3E000000;
	private const int EVENT_TYPE_SHIFT = 25;
	private const UInt32 PAYLOAD_MASK = 0x0000FFFF;
	private const int PAYLOAD_SHIFT = 16;

	private static int EncodeType(int eventType)
	{
		return (int)((UInt32)eventType & ~IS_DATA_FLAG); // make sure upper bit is not set
	}
    /// <param name="eventId"></param>
    /// <param name="eventData"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
	private static int EncodeData(int eventId, int eventData, int pos)
	{
		UInt32 data = 0;
		data |= IS_DATA_FLAG;
		data |= (((UInt32)pos << DATA_POS_SHIFT) & DATA_POS_MASK);
		data |= (((UInt32)eventId << EVENT_TYPE_SHIFT) & EVENT_TYPE_MASK);
		data |= (((UInt32)eventData >> (pos * PAYLOAD_SHIFT)) & PAYLOAD_MASK);

		return (int)data;
	}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="eventData"></param>
    /// <returns></returns>
	private static int DecodeData(int eventData)
	{
//		bool hasData   = (((UInt32)eventData & IS_DATA_FLAG) != 0);
		UInt32 pos     = (((UInt32)eventData & DATA_POS_MASK) >> DATA_POS_SHIFT);
//		UInt32 eventId = (((UInt32)eventData & EVENT_TYPE_MASK) >> EVENT_TYPE_SHIFT);
		UInt32 payload = (((UInt32)eventData & PAYLOAD_MASK) << (PAYLOAD_SHIFT * (int)pos));

		return (int)payload;
	}
    public const string dllFileName = "PicoPlugin";
    [DllImport(dllFileName)]
    private static extern void SetUnityStreamingAssetsPath([MarshalAs(UnmanagedType.LPStr)] string path);

    [DllImport(dllFileName)]

    private static extern void SetTextureFromUnity(System.IntPtr Ltexture, System.IntPtr Rtexture);

    [DllImport(dllFileName)]

    private static extern void SetTimeFromUnity(float t);
    [DllImport(dllFileName)]
    private static extern IntPtr GetRenderEventFunc();

    [DllImport(dllFileName)]
    public static extern void PVR_RenderEditorInitializeLegacy();
    [DllImport(dllFileName)]
    public static extern void PVR_RenderEditorShutDownLegacy();
    [DllImport(dllFileName)]
    public static extern void PVR_RenderEditorInitialize();
    [DllImport(dllFileName)]
    public static extern void PVR_RenderEditorShutDown();
    [DllImport(dllFileName)]
    public static extern bool PVR_Shutdown();
    [DllImport(dllFileName)]
    private static extern void SetTextureFromUnityATW(RT rt);

    public static void CallbackCoroutine()
    {

        SetTimeFromUnity(Time.timeSinceLevelLoad);
        // GL.IssuePluginEvent(GetRenderEventFunc(), 1);
#if (UNITY_5_0 || UNITY_5_1 || UNITY_4_6 || UNITY_4_7)
        GL.IssuePluginEvent(1);
#else
        GL.IssuePluginEvent(GetRenderEventFunc(), 1);
#endif

    }
    public static void IssueWithDatapc(System.IntPtr Ltexture, System.IntPtr Rtexture)
    {
        //SetTextureFromUnity(Ltexture, Rtexture);
        RT temp = new RT();
        temp.orientation.x = PicoVRManager.SDK.headPose.Orientation.x;
        temp.orientation.y = PicoVRManager.SDK.headPose.Orientation.y;
        temp.orientation.z = PicoVRManager.SDK.headPose.Orientation.z;
        temp.orientation.w = PicoVRManager.SDK.headPose.Orientation.w;

        temp.Ltext = Ltexture;
        temp.Rtext = Rtexture;

        //Debug.LogError("3 rot : " + temp.orientation.x + " , " + temp.orientation.y + " , " + temp.orientation.z + " , " + temp.orientation.w);
        SetTextureFromUnityATW(temp);
      
    }
    public static void IssueWithAssetsPath()
    {
        SetUnityStreamingAssetsPath(Application.streamingAssetsPath);

    }
   
   
}
