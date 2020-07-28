using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PluginInit : MonoBehaviour
{
    private AndroidJavaObject activityContext = null;
    private AndroidJavaClass javaClass = null;

    // Start is called before the first frame update
    void Start()
    {
        //일단 아까 plugin의 context를 설정해주기 위해
        //유니티 자체의 UnityPlayerActivity를 가져옵시다.
        using (AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            activityContext = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
        }

        //클래스를 불러와줍니다.
        //패키지명 + 클래스명입니다.
        using (javaClass = new AndroidJavaClass("com.technoblood.playzWebapp.FCMService"))
        {
            if (javaClass != null)
            {
                //Context를 설정해줍니다.
                javaClass.Call("setContext", activityContext);
            }
        }
    }


    public void SwitchVibrationOption(float option)
    {
        Debug.Log(">>> SwitchVibrationOption");
        javaClass.Call("switchVibrationOption", Convert.ToBoolean(option));
    }
    public float GetVibarationOption()
    {
        Debug.Log(">>> GetVibarationOption");
        var result = javaClass.Call<bool>("getVibarationOption", true);
        return Convert.ToSingle(result);
    }


    public void SwitchSoundOption(float option)
    {
        Debug.Log(">>> SwitchSoundOption");
        javaClass.Call("switchSoundOption", Convert.ToBoolean(option));
    }
    public float GetSoundOption()
    {
        Debug.Log(">>> GetSoundOption");
        var result = javaClass.Call<bool>("getSoundOption", true);
        return Convert.ToSingle(result);
    }


    public void SwitchNotiOption(float option)
    {
        Debug.Log(">>> SwitchNotiOption");
        javaClass.Call("switchNotiOption", Convert.ToBoolean(option));
    }
    public float GetNotiOption()
    {
        Debug.Log(">>> GetNotiOption");
        var result = javaClass.Call<bool>("getNotiOption", true);
        return Convert.ToSingle(result);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
