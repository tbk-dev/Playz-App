using Lean.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
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
        if (webViewObject == null)
            webViewObject = FindObjectOfType<WebViewObject>();
    }


    public void OnEnable()
    {
#if UNITY_ANDROID
        AttachAndroid();
#elif UNITY_IOS
#endif
        InitSetting();

        toggleReceiveInfo.ButtonEvent += ToggleReceiveInfo_ButtonEvent;
        toggleSoundNoti.ButtonEvent += ToggleSoundNoti_ButtonEvent;
        toggleVibNoti.ButtonEvent += ToggleVibNoti_ButtonEvent;

    }

    public void GetUnityActivity()
    {
        //일단 아까 plugin의 context를 설정해주기 위해
        //유니티 자체의 UnityPlayerActivity를 가져옵시다.
        using (AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            activityContext = activityClass.GetStatic<AndroidJavaObject>("currentActivity");

            if (activityContext != null)
            {
                Debug.Log("get activityContext");
            }
            else
            {
                Debug.LogError("failed get activityContext");
            }
        }

    }

    public void AttachAndroid()
    {
        Debugging.instance.DebugLog($"run AttachAndroid");

        GetUnityActivity();

        //string packageName = "com.technoblood.playzWebapp.PlayzUtill";
        string packageName = "com.technoblood.playzWebapp.FCMService";

        //var FCMServiceClass = GetAndroidJavaClass(packageName); //using 사용후으로 해제되면서 class에서 instance를 가져오지 못하는 문제가 있는것으로 추정
        using (var FCMServiceClass = new AndroidJavaClass(packageName))
        {
            if (FCMServiceClass != null)
            {
                fcmPluginInstance = JavaClassCallStaticObject(FCMServiceClass, "instance");
                Debug.Log($"get FCMService instance");
            }
            else
            {
                Debug.Log($"FCMServiceClass is null");
            }
        }

        //unityContext를 설정한다
        if (fcmPluginInstance != null)
        {
            fcmPluginInstance.Call("setContext", activityContext);
            Debugging.instance.DebugLog("fcmPluginInstance setContext activityContext");
        }
        else
        {
            Debugging.instance.DebugLog("not fcmPluginInstance setContext activityContext");
        }

        Debugging.instance.DebugLog($"end AttachAndroid");
    }




    public void LangBtn()
    {

        //var LeanLocalization = FindObjectOfType<LeanLocalization>();

        if (LeanLocalization.CurrentLanguage == "Korean")
        {
            LeanLocalization.CurrentLanguage = "VietNam";
        }
        else if (LeanLocalization.CurrentLanguage == "VietNam")
        {
            LeanLocalization.CurrentLanguage = "English";
        }
        else
        {
            LeanLocalization.CurrentLanguage = "Korean";
        }

        LeanLocalization.UpdateTranslations();
    }


    private void InitSetting()
    {
        Debugging.instance.DebugLog("run InitSetting");
        //receiveInfoSlider.value = Convert.ToSingle(GetPreferenceBool(receiveInfo));
        //soundNotiSlider.value = Convert.ToSingle(GetPreferenceBool(soundNoti));
        //vibNotiSlider.value = Convert.ToSingle(GetPreferenceBool(vibNoti));


        //toggleReceiveInfo.isOn = GetPreferenceBool(receiveInfo);
        toggleReceiveInfo.isOn = Firebase.Messaging.FirebaseMessaging.TokenRegistrationOnInitEnabled;        //GetPreferenceBool(receiveInfo);
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

#if UNITY_ANDROID
        Debugging.instance.DebugLog("run VibrateAction");
        try
        {
            fcmPluginInstance.Call("vibrateAction", ms, activityContext);
        }
        catch (Exception ex)
        {
            Debugging.instance.DebugLog($"VibrateAction {ex.Message}");
        }
#elif UNITY_IOS
        Debugging.instance.DebugLog($"VibrateAction call IOS");
#endif
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
        Debugging.instance.DebugLog($" ToggleReceiveInfo  {isOn}");
        SetPreferencBool(receiveInfo, isOn);

        TokenRegistrationOnInitEnabled(isOn);
    }

    private void TokenRegistrationOnInitEnabled(bool isOn)
    {
        //todo 구독관리1
        Debugging.instance.DebugLog($"run TokenRegistrationOnInitEnabled");
        //Firebase.Messaging.FirebaseMessaging.TokenRegistrationOnInitEnabled = ison;
        FindObjectOfType<FirebaseMSGSet>().ToggleTokenOnInit();

#if UNITY_ANDROID
        if (fcmPluginInstance == null)
        {
            Debugging.instance.DebugLog("TokenRegistrationOnInitEnabled Plugin is null");
            return;
        }

        fcmPluginInstance.Call("SetActiveFCM", isOn);
        //fcmPluginInstance.Call("SetActiveFCM", ison, true); //알림 수신 설정시에 토큰 삭제가 필요 한 경우 인자추가
        Debugging.instance.DebugLog(" end ANDROID SetActiveFCM");


#elif UNITY_IOS
        Debugging.instance.DebugLog($"TokenRegistrationOnInitEnabled call IOS");
#endif


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
    /*
     
        
#if UNITY_ANDROID
#elif UNITY_IOS
        Debugging.instance.DebugLog($"preference call IOS");
#endif


    */

    private void SetPreferencBool(string prefKey, bool value)
    {
        PlayerPrefs.SetInt(prefKey, Convert.ToInt32(value));
        PlayerPrefs.Save();

#if UNITY_ANDROID
        if (fcmPluginInstance == null)
        {
            Debugging.instance.DebugLog("SetPreferencBool Plugin is null");
            return;
        }

        fcmPluginInstance.Call("setPreferenceBool", prefKey, value, activityContext);
#elif UNITY_IOS
        Debugging.instance.DebugLog($"SetPreferencBool call IOS");
#endif
    }
    private bool GetPreferenceBool(string prefKey)
    {
        var playerPref = Convert.ToBoolean(PlayerPrefs.GetInt(prefKey));

        bool preference = false;
#if UNITY_ANDROID
        if (fcmPluginInstance == null)
        {
            Debugging.instance.DebugLog("GetPreferenceBool Plugin is null 1");
            return false;
        }

        preference = fcmPluginInstance.Call<bool>("getPreferenceBool", prefKey, activityContext);
        //유니티저장
        //PlayerPrefs.SetString(DeviceIdKey, preference);
        //PlayerPrefs.Save();

#elif UNITY_IOS
        Debugging.instance.DebugLog($"GetPreferenceBool call IOS");
#endif
        Debugging.instance.DebugLog($"preference {prefKey} val : {preference}");
        return preference;
    }

    private void SetPreferenceString(string prefKey, string value)
    {
        PlayerPrefs.SetString(prefKey, value);
        PlayerPrefs.Save();
#if UNITY_ANDROID
        if (fcmPluginInstance == null)
        {
            Debugging.instance.DebugLog("SetPreferenceString Plugin is null");
            return;
        }

        fcmPluginInstance.Call("setPreferenceString", prefKey, value, activityContext);
#elif UNITY_IOS
        Debugging.instance.DebugLog($"preference call IOS");
#endif
    }
    private string GetPreferenceString(string prefKey)
    {
        var playerPref = PlayerPrefs.GetString(prefKey);
        string preference = "";
#if UNITY_ANDROID
        if (fcmPluginInstance == null)
        {
            Debugging.instance.DebugLog("GetPreferenceString Plugin is null");
            return null;
        }

        preference = fcmPluginInstance.Call<string>("getPreferenceString", prefKey, activityContext);
        //유니티저장
        //PlayerPrefs.SetString(DeviceIdKey, savedPreference);
        //PlayerPrefs.Save();

#elif UNITY_IOS
        Debugging.instance.DebugLog($"preference call IOS");
#endif
        Debugging.instance.DebugLog($"preference {prefKey} val : {preference}");
        return preference;
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
#if UNITY_ANDROID
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
#elif UNITY_IOS
        Debugging.instance.DebugLog($"ShowToastMessage call IOS");
#endif
    }


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

    public void LogUnity(string msg)
    {
        if (fcmPluginInstance != null)
        {
            fcmPluginInstance.Call("LogUnity", msg);
        }
        else
        {
            Debug.LogError($"notWorkPluginLog >>> {msg}");
        }
    }

    public void OnDisable()
    {
        toggleReceiveInfo.ButtonEvent -= ToggleReceiveInfo_ButtonEvent;
        toggleSoundNoti.ButtonEvent -= ToggleSoundNoti_ButtonEvent;
        toggleVibNoti.ButtonEvent -= ToggleVibNoti_ButtonEvent;
    }

    #region android plugin

    public AndroidJavaClass GetAndroidJavaClass(string javaClass)
    {
        try
        {
            using (var androidJavaClass = new AndroidJavaClass(javaClass))
            {
                if (androidJavaClass != null)
                {
                    Debugging.instance.DebugLog($"{javaClass} Android javaClass is loaded");
                    return androidJavaClass;
                }
            }
        }
        catch
        {
            Debugging.instance.DebugLog($"{javaClass} could not be loaded");
        }

        return null;
    }

    public AndroidJavaObject GetAndroidJavaObject(string javaObject)
    {
        try
        {
            using (var androidJavaObject = new AndroidJavaObject(javaObject))
            {
                Debugging.instance.DebugLog($"{javaObject} Android javaObject is loaded");
                return androidJavaObject;
            }
        }
        catch
        {
            Debugging.instance.DebugLog($"{javaObject} could not be loaded");
        }
        return null;
    }

    private AndroidJavaObject JavaObjectCallStaticObject(AndroidJavaObject androidJavaObject, string objectName)
    {
        try
        {
            var result = androidJavaObject.CallStatic<AndroidJavaObject>(objectName);
            Debugging.instance.DebugLog("### call JavaObjectCallStaticObject");
            return result;
        }
        catch (Exception)
        {
            Debugging.instance.DebugLog($"### JavaObjectCallStaticObject is null");
        }
        return null;
    }

    private AndroidJavaObject JavaObjectCallObject(AndroidJavaObject androidJavaObject, string objectName)
    {
        try
        {

            var result = androidJavaObject.Call<AndroidJavaObject>(objectName);
            Debugging.instance.DebugLog("### call JavaObjectCallObject");
            return result;
        }
        catch (Exception)
        {
            Debugging.instance.DebugLog($"### JavaObjectCallObject is null");
        }
        return null;
    }

    private AndroidJavaObject JavaClassCallStaticObject(AndroidJavaClass androidJavaClass, string objectName)
    {
        try
        {
            var result = androidJavaClass.CallStatic<AndroidJavaObject>(objectName);
            Debugging.instance.DebugLog("### call JavaClassCallStaticObject");
            return result;
        }
        catch (Exception)
        {
            Debugging.instance.DebugLog($"### JavaClassCallStaticObject is null");
        }
        return null;
    }

    private AndroidJavaObject JavaClassCallObject(AndroidJavaClass androidJavaClass, string objectName)
    {
        try
        {

            var result = androidJavaClass.Call<AndroidJavaObject>(objectName);
            Debugging.instance.DebugLog("### call JavaClassCallObject");
            return result;
        }
        catch (Exception)
        {
            Debugging.instance.DebugLog($"### JavaClassCallObject is null");
        }
        return null;
    }


    private AndroidJavaClass JavaObjectCallStaticClass(AndroidJavaObject androidJavaObject, string className)
    {
        try
        {
            var result = androidJavaObject.CallStatic<AndroidJavaClass>(className);
            Debugging.instance.DebugLog("### call JavaObjectCallStaticClass");
            return result;

        }
        catch (Exception)
        {
            Debugging.instance.DebugLog($"### JavaObjectCallStaticClass is null");
        }
        return null;
    }

    private AndroidJavaClass JavaObjectCallClass(AndroidJavaObject androidJavaObject, string className)
    {
        try
        {

            var result = androidJavaObject.Call<AndroidJavaClass>(className);
            Debugging.instance.DebugLog("### call JavaObjectCallClass");
            return result;
        }
        catch (Exception)
        {
            Debugging.instance.DebugLog($"### JavaObjectCallClass is null");
        }
        return null;
    }

    private AndroidJavaClass JavaClassCallStaticClass(AndroidJavaClass androidJavaClass, string className)
    {
        try
        {
            var result = androidJavaClass.CallStatic<AndroidJavaClass>(className);
            Debugging.instance.DebugLog("### call JavaClassCallStaticClass");
            return result;
        }
        catch (Exception)
        {
            Debugging.instance.DebugLog($"### JavaClassCallStaticClass is null");
        }
        return null;
    }

    private AndroidJavaClass JavaClassCallClass(AndroidJavaClass androidJavaClass, string className)
    {
        try
        {
            var result = androidJavaClass.Call<AndroidJavaClass>(className);
            Debugging.instance.DebugLog("### call JavaClassCallClass");
            return result;
        }
        catch (Exception)
        {
            Debugging.instance.DebugLog($"### JavaClassCallClass is null");
        }
        return null;
    }

    #endregion

    IEnumerator QuitApp()
    {
        try
        {
            ShowToastMessage("종료하시겠습니까?\n뒤로가기 버튼을 다시 한 번 누르면 종료됩니다", 500);
        }
        catch (Exception ex)
        {
            Debugging.instance.DebugLog($"QuitApp {ex.Message}");
        }
        waitQuit = true;
        yield return new WaitForSeconds(2f);
        waitQuit = false;
    }
}
