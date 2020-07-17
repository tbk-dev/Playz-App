using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FCMplugin : MonoBehaviour
{

    private AndroidJavaObject UnityActivity;
    private AndroidJavaObject UnityInstance;

    // Start is called before the first frame update
    void Start()
    {
        AndroidJavaClass ajs = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        UnityActivity = ajs.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaClass ajc2 = new AndroidJavaClass("com.technoblood.unityfcm");
        UnityInstance = ajc2.CallStatic<AndroidJavaObject>("Instance");

        UnityInstance.Call("SetContext", UnityActivity);
    }

    public void ToastButton()
    {
        ShowToast(">>> run unity", false);
    }

    public void ShowToast(string msg, bool isLong)
    {
        UnityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            if (isLong)
            {
                UnityInstance.Call("ShowToast", msg, 1);
            }
            else
            {
                UnityInstance.Call("ShowToast", msg, 0);
            }
        }));
    }
}
