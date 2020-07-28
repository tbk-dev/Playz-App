using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Extensions;

public class FCMpluginTest : MonoBehaviour
{
    private AndroidJavaObject activityContext = null;
    private AndroidJavaClass javaClass = null;
    private AndroidJavaObject javaClassInstance = null;
    const string packageName = "com.technoblood.playzWebappA";

    void Awake()
    {
        //일단 아까 plugin의 context를 설정해주기 위해
        //유니티 자체의 UnityPlayerActivity를 가져옵시다.
        using (AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            activityContext = activityClass.GetStatic<AndroidJavaObject>("currentActivity");

            if (activityContext != null)
            {
                Debug.Log($">>> currentActivity : {activityContext}");
            }
            else
            {
                Debug.LogError($">>> Not Find currentActivity");
            }
        }

        //클래스를 불러와줍니다.
        //패키지명 + 클래스명입니다.
        using (javaClass = new AndroidJavaClass($"{packageName}.MyFirebaseInstanceIDService"))
        {
            if (javaClass != null)
            {
                Debug.Log($">>> javaClass : {javaClass}");
                ////아까 싱글톤으로 사용하자고 만들었던 static instance를 불러와줍니다.
                //javaClassInstance = javaClass.CallStatic<AndroidJavaObject>("instance");
                ////Context를 설정해줍니다.
                //javaClassInstance.Call("setContext", activityContext);
            }
            else
            {
                Debug.LogError($">>> Not Find javaClass");
            }
        }

    }
    public void Update()
    {
        
    }

    public void CallShowToast()
    {
        //Toast는 안드로이드의 UiThread를 사용하기때문에 
        //UnityPlayerActivity UiThread를 호출하고, 다시 ShowToast를 호출합니다.

        //UiThread에서 호출하지 않으면 Looper.prepare()어쩌고 에러가 뜨는데..
        //제대로 이해하지 못했습니다.. 누가 설명좀해줘요.
        activityContext.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            javaClassInstance.Call("ShowToast", "Hello world!!");
        }));
    }


}
