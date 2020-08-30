using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PluginInit : MonoBehaviour
{
    #region singleton
    public static PluginInit instance;
    private static readonly object padlock = new object();

    public static PluginInit Instance
    {
        get
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    Debug.LogError("PluginInit is null");
                    //instance = new PluginInit();
                }
                return instance;
            }
        }
    }
    #endregion

    private AndroidJavaObject activityContext = null;
    private AndroidJavaClass FCMServiceClass = null;
    private AndroidJavaObject fcmPluginInstance = null;

    bool waitQuit = false;

    public ToggleController toggleReceiveInfo;
    public ToggleController toggleSoundNoti;
    public ToggleController toggleVibNoti;

    const string receiveInfo = "receiveInfo";
    const string soundNoti = "soundNoti";
    const string vibNoti = "vibNoti";

    // Start is called before the first frame update
    public void Awake()
    {
        instance = this;
        Screen.fullScreen = false;
    }

    public void Start()
    {
        AttachAndroid();

        webViewObject = FindObjectOfType<WebViewObject>();
    }

    public void OnEnable()
    {
        if (webViewObject  == null )
            webViewObject = FindObjectOfType<WebViewObject>();

        InitSetting();

        toggleReceiveInfo.ButtonEvent += ToggleReceiveInfo_ButtonEvent;
        toggleSoundNoti.ButtonEvent += ToggleSoundNoti_ButtonEvent;
        toggleVibNoti.ButtonEvent += ToggleVibNoti_ButtonEvent;
    }

    public void OnDisable()
    {
        toggleReceiveInfo.ButtonEvent -= ToggleReceiveInfo_ButtonEvent;
        toggleSoundNoti.ButtonEvent -= ToggleSoundNoti_ButtonEvent;
        toggleVibNoti.ButtonEvent -= ToggleVibNoti_ButtonEvent;
    }

    public void AttachAndroid()
    {
        //Debugging.instance.DebugLog($"run AttachAndroid");
        //plugin의 context를 설정하기 위해 유니티 자체의 UnityPlayerActivity를 가져온다
        using (AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            activityContext = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
            Debugging.instance.DebugLog("get activityContext");

        }

        //클래스 호출 패키지명 + 클래스명
        using (FCMServiceClass = new AndroidJavaClass("com.technoblood.playzWebapp.FCMService"))
        {
            if (FCMServiceClass == null)
            {
                Debugging.instance.DebugLog($"FCMServiceClass is null");
                return;
            }


            try
            {
                //플러그인의 싱글톤 static instance를 불러와줍니다.
                fcmPluginInstance = FCMServiceClass.CallStatic<AndroidJavaObject>("instance");

                if (fcmPluginInstance == null)
                {
                    Debugging.instance.DebugLog("fcmPluginInstance is null");
                    return;

                }
            }
            catch (Exception ex)
            {
                Debugging.instance.DebugLog($"instance is null {ex.Message}");
            }



            try
            {
                //플러그인의 싱글톤 static instance를 불러와줍니다.
                fcmPluginInstance = FCMServiceClass.CallStatic<AndroidJavaObject>("getContext");

                if (fcmPluginInstance == null)
                {
                    Debugging.instance.DebugLog("fcmPluginInstance is null");
                    return;
                }
            }
            catch (Exception ex)
            {
                Debugging.instance.DebugLog($"getContext is null {ex.Message}");
            }


            //Context를 설정해줍니다.
            if (fcmPluginInstance != null)
            {
                fcmPluginInstance.Call("setContext", activityContext);
                Debugging.instance.DebugLog("setContext activityContext");
            }
        }


        Debugging.instance.DebugLog($"end AttachAndroid");
    }

    private void InitSetting()
    {
        Debugging.instance.DebugLog("run InitSetting");

        //receiveInfoSlider.value = Convert.ToSingle(GetPreferenceBool(receiveInfo));
        //soundNotiSlider.value = Convert.ToSingle(GetPreferenceBool(soundNoti));
        //vibNotiSlider.value = Convert.ToSingle(GetPreferenceBool(vibNoti));

        toggleReceiveInfo.isOn = GetPreferenceBool(receiveInfo);
        Debugging.instance.DebugLog($"toggleReceiveInfo : {toggleReceiveInfo.isOn}");

        TokenRegistrationOnInitEnabled(toggleReceiveInfo.isOn);

        toggleSoundNoti.isOn = GetPreferenceBool(soundNoti);
        Debugging.instance.DebugLog($"toggleSoundNoti : {toggleSoundNoti.isOn}");
        toggleVibNoti.isOn = GetPreferenceBool(vibNoti);
        Debugging.instance.DebugLog($"toggleVibNoti : {toggleVibNoti.isOn}");

        Debugging.instance.DebugLog($"end InitSetting");
    }

    /////36개

    public void VibrateAction(int ms = 500)
    {
        Debugging.instance.DebugLog("VibrateAction");
        try
        {
            fcmPluginInstance.Call("vibrateAction", ms, activityContext);
        }
        catch (Exception ex)
        {
            Debugging.instance.DebugLog($"VibrateAction {ex.Message}");
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region ::::::::: button test

    public void CallFCM()
    {
        Debugging.instance.DebugLog("CallFCM");
        ShowToastMessage("wellcom");
    }
    WebViewObject webViewObject;

    public void CallTest1()
    {
        Debugging.instance.DebugLog(" CallTest1");
        webViewObject.EvaluateJS("Unity.call(document.documentElement.innerText.toString());");
        //VibrateAction();
    }

    public void CallTest2()
    {
        Debugging.instance.DebugLog(" CallTest2");
        webViewObject.EvaluateJS("Unity.call(location);");

        //        webViewObject.EvaluateJS(@"
        //+                    (function() {
        //+                           console.log('log test');

        //+                        if (window.Unity != null) {
        //+                            var innerText = document.documentElement.innerText;
        //+                           console.log('innerText  ' + innerText);
        //+                            var jsonText = innerText.substring(innerText.indexOf('{'), innerText.lastIndexOf('}')+1);
        //+                           console.log('jsonText  ' + jsonText);

        //+                                window.Unity.call(jsonText);
        //+                        }
        //+                    });
        //+                    ");


        //VibrateAction(500);
    }

    public void CallTest3()
    {
        Debugging.instance.DebugLog(" CallTest3");
        webViewObject.EvaluateJS("Unity.call(document.body.innertext);");


        //        webViewObject.EvaluateJS(@"
        //+                    function() {
        //+                           var  inti = 1+12345;
        //+                                window.Unity.call('log : '+ inti );
        //+                    };
        //+                    ");

    }


    public void CallTestbtn1()
    {
        Debugging.instance.DebugLog(" CallTestbtn1");
        webViewObject.EvaluateJS(@"Unity.call('ua=' + 'tsetttttt')");

    }

    public void CallTestbtn2()
    {
        Debugging.instance.DebugLog(" CallTestbtn2");
        webViewObject.EvaluateJS(@" 
                   window.Unity = { 
                     call: function(msg) { 
                       window.location = 'unity:' + msg; 
                     } 
                   } 
                 ");

    }

    public void CallTestbtn3()
    {
        Debugging.instance.DebugLog(" CallTestbtn3");
        webViewObject.EvaluateJS(@"Unity.call('anchor');");

    }

    #endregion
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////


    private void ToggleReceiveInfo_ButtonEvent(bool isOn)
    {
        Debugging.instance.DebugLog(" ToggleReceiveInfo");
        SetPreferencBool(receiveInfo, isOn);

        TokenRegistrationOnInitEnabled(isOn);
    }

    private void TokenRegistrationOnInitEnabled(bool ison)
    {
        //todo 구독관리
        Debugging.instance.DebugLog(" TokenRegistrationOnInitEnabled 구독관리");

        //Firebase.Messaging.FirebaseMessaging.TokenRegistrationOnInitEnabled = ison;

        //todo 새로 구독후 토큰값이 바뀌는지 확인후에 바뀐다면 서버로 전송
    }
    private void ToggleVibNoti_ButtonEvent(bool isOn)
    {
        Debugging.instance.DebugLog(" ToggleVibNoti");
        SetPreferencBool(vibNoti, isOn);
    }

    private void ToggleSoundNoti_ButtonEvent(bool isOn)
    {
        Debugging.instance.DebugLog(" ToggleSoundNoti");
        SetPreferencBool(soundNoti, isOn);
    }


    private void SetPreferencBool(string prefKey, bool value)
    {
        if (fcmPluginInstance == null)
        {
            Debugging.instance.DebugLog("fcmPluginInstance is null");
            return;
        }

        fcmPluginInstance.Call("setPreferenceBool", prefKey, value, activityContext);
    }
    private bool GetPreferenceBool(string prefKey)
    {
        if (fcmPluginInstance == null)
        { 
            Debug.LogError("fcmPluginInstance is null");
            return false;
        }

        var preference = fcmPluginInstance.Call<bool>("getPreferenceBool", prefKey, activityContext);
        //유니티저장
        //PlayerPrefs.SetString(DeviceIdKey, preference);
        //PlayerPrefs.Save();
        
        Console.WriteLine($"preference {prefKey} val : {preference}");
        return preference;
    }

    private void SetPreferenceString(string prefKey, string value)
    {
        if (fcmPluginInstance == null)
        {
            Debug.LogError("fcmPluginInstance is null");
            return;
        }

    fcmPluginInstance.Call("setPreferenceString", prefKey, value, activityContext);
    }
    private string GetPreferenceString(string prefKey)
    {
        if (fcmPluginInstance == null)
        { 
            Debug.LogError("fcmPluginInstance is null");
            return null;
        }

        var preference = fcmPluginInstance.Call<string>("getPreferenceString", prefKey, activityContext);
        //유니티저장
        //PlayerPrefs.SetString(DeviceIdKey, savedPreference);
        //PlayerPrefs.Save();

        Console.WriteLine($"preference {prefKey} val : {preference}");
        return preference;
    }


    public void SlectSlider()
    {
        Debugging.instance.DebugLog($"SlectSlider");
    }


    public void SwitchReceiveInfoOption(float option)
    {
        Debugging.instance.DebugLog(" SwitchReceiveInfoOption");

        SetPreferencBool(receiveInfo, Convert.ToBoolean(option));
    }
    public void GetNotiOption()
    {
        Debugging.instance.DebugLog("GetNotiOption");

        var result = GetPreferenceBool(receiveInfo);

        Debugging.instance.DebugLog($"receiveInfo result {result}");
    }
    
    public void SwitchSoundOption(float option)
    {
        Debugging.instance.DebugLog(" SwitchSoundOption");

        SetPreferencBool(soundNoti, Convert.ToBoolean(option));
    }
    public void GetSoundOption()
    {
        Debugging.instance.DebugLog($" GetSoundOption");

        var result = GetPreferenceBool(soundNoti);

        Debugging.instance.DebugLog($"soundNoti result {result}");
    }

    public void SwitchVibrationOption(float option)
    {
        Debugging.instance.DebugLog(" SwitchVibrationOption");

        SetPreferencBool(vibNoti, Convert.ToBoolean(option));
    }
    public void GetVibarationOption()
    {
        Debugging.instance.DebugLog($" GetVibarationOption");

        var result = GetPreferenceBool(vibNoti);

        Debugging.instance.DebugLog($"vibNoti result {result}");
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
     
    public void ShowToastMessage(string msg, int duration = 0)
    {
        //Toast는 안드로이드의 UiThread를 사용하기때문에 
        //UnityPlayerActivity UiThread를 호출하고, 다시 ShowToast를 호출합니다.

        //UiThread에서 호출하지 않으면 Looper.prepare()어쩌고 에러가 뜨는데..
        activityContext.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            if (fcmPluginInstance != null)
            {
                if (duration == 0)
                    fcmPluginInstance.Call("ShowToast", msg, duration);
                else
                    fcmPluginInstance.Call("ShowToast", msg);
            }

        }));
    }

    public void LogUnity(string msg)
    {
        //Toast는 안드로이드의 UiThread를 사용하기때문에 
        //UnityPlayerActivity UiThread를 호출하고, 다시 ShowToast를 호출합니다.

        //UiThread에서 호출하지 않으면 Looper.prepare()어쩌고 에러가 뜨는데..
        //activityContext.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        //{

        if (fcmPluginInstance != null)
        {
            fcmPluginInstance.Call("LogUnity", msg);
        }

        //}));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (waitQuit)
                Application.Quit();
            else
            {
                StartCoroutine(QuitApp());
            }
        }
    }

    IEnumerator QuitApp()
    {
        ShowToastMessage("Quit?", 500);
        waitQuit = true;
        yield return new WaitForSeconds(0.5f);
        waitQuit = false;
    }
}
