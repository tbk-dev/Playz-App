
using UnityEngine;

//using for DllImport
using System.Runtime.InteropServices;

public class iOSManager : MonoBehaviour
{
    static iOSManager _instance;
    public static string strLog = "WestWoodForever's Unity3D iOS Plugin Sample";

    ////private AndroidJavaObject curActivity;
    //[DllImport("__Internal")]
    //private static extern void iOSPluginHelloWorld(string strMessage);

    public static iOSManager GetInstance()
    {
        if (_instance == null)
        {
            _instance = new GameObject("iOSManager").AddComponent<iOSManager>();
        }
        return _instance;
    }

    void Awake()
    {
        /*
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        curActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
        */
    }

    public void CalliOSFunc(string strMessage)
    {
        //curActivity.Call("AndroidFunc", strMsg);
        Debug.Log("UnityLog1");
        //iOSPluginHelloWorld(strMessage);
        Debug.Log("UnityLog2");
    }

    public void SetLog(string _strLog)
    {
        Debug.Log("Set UnityLog");
        strLog = _strLog;
    }

    void OnGUI()
    {
        float fYpos = 0;
        GUI.Label(new Rect(0, fYpos, 400, 50), iOSManager.strLog);

        fYpos += 50;
        if (GUI.Button(new Rect(0, fYpos, 100, 50), "Call iOS") == true)
        {
            iOSManager.GetInstance().CalliOSFunc("Unity3D iOS Plugin Test Ok");
        }
    }

}
