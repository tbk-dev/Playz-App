using Firebase.Extensions;
using Firebase.Messaging;
using Lean.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class Debugging : MonoBehaviour
{
    #region singleton
    public static Debugging instance ;
    private static readonly object padlock = new object();

    public static Debugging Instance
    {
        get
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    Debug.LogError("debugging is null");
                    //instance = new Debugging();
                }
                return instance;
            }
        }
    }
    #endregion

    [SerializeField]
    private Text logText1 = null;
    [SerializeField]
    private InputField InputField_topic = null;

    public Text UrlText = null;

    FirebaseMSGSet firebaseMSGSet;

    private void Awake()
    {
        instance = this;
        TokenFilePath = Path.Combine(Application.persistentDataPath, "userToken.txt");

        StartCoroutine(ShowLateLog());
    }

    WebViewSift webViewSift;

    // Start is called before the first frame update
    void Start()
    {
        firebaseMSGSet = FindObjectOfType<FirebaseMSGSet>();
        webViewSift = FindObjectOfType<WebViewSift>();

        if (firebaseMSGSet == null)
            DebugLog("firebaseMSGSet null");

        if (webViewSift == null)
            DebugLog("webViewSift null");

        //StartCoroutine(BottonLogDependencyStatus());

    }

    private void Update()
    {
    }


    Firebase.DependencyStatus lastStatus;

    //public IEnumerator BtnDependencyStatus()
    public void BtnDependencyStatus()
    {
        DebugLog("dependency status: " + firebaseMSGSet.dependencyStatus.ToString());
        //while (true)
        {
            if (lastStatus != firebaseMSGSet.dependencyStatus)
            {
                //DebugLog("One or more Firebase dependencies are not present.");
                //DebugLog("Current dependency status: " + firebaseMSGSet.dependencyStatus.ToString());

                lastStatus = firebaseMSGSet.dependencyStatus;
                DebugLog("change dependency status: " + firebaseMSGSet.dependencyStatus.ToString());
            }
            //yield return new WaitForSeconds(1);
        }
    }

    public void BtnToggleTokenOnInit()
    {
        Debug.Log($"BottonToggleTokenOnInit");
        firebaseMSGSet.ToggleTokenOnInit();

    }

    public void BtnSubscribe()
    {
        Debug.Log($"BottonSubscribe");
        firebaseMSGSet.SubscribeTopic(InputField_topic.text);
    }

    public void BtnUnsubscribe()
    {
        Debug.Log($"BottonUnsubscribe");
        firebaseMSGSet.UnSubscribeTopic(InputField_topic.text);
    }

    public void BtnTeststart()
    {
        Debug.Log($"Teststart");
        webViewSift.webViewObject.CallOnStarted("http://dev-playz.virtual-gate.co.kr/member/login?response_type=jwt");
    }

    public void BtnTestload()
    {
        Debug.Log($"Testload");
        webViewSift.webViewObject.CallOnLoaded("http://dev-playz.virtual-gate.co.kr/member/login?response_type=jwt");
    }

    public void BtnCheckAndFixDependencies()
    {
        firebaseMSGSet.CheckAndFixDependencies();
    }


    string userToken;
    string[] userTokenArrya;
    string TokenFilePath;

    public void SaveToken(string token)
    {
        try
        {
            FileStream f = new FileStream(TokenFilePath, FileMode.Append, FileAccess.Write);
            StreamWriter writer = new StreamWriter(f, System.Text.Encoding.Unicode);
            writer.Write(token);
            writer.Close();

            DebugLog($"File path : {TokenFilePath}{Environment.NewLine}token = {token}");
        }
        catch (Exception ex)
        {
            DebugLog($">>> SaveToken ERROR : {ex.Message.ToString()}");
        }
    }

    public void ReadToken()
    {
        DebugLog("ReadToken");
        try
        {
            var read = File.ReadAllText(TokenFilePath);
            StringReader sr = new StringReader(read);

            // 먼저 한줄을 읽는다. 
            string source = sr.ReadLine();

            while (source != null)
            {
                userTokenArrya = source.Split('\n');  // 쉼표로 구분한다. 저장시에 쉼표로 구분하여 저장하였다.
                if (userTokenArrya.Length == 0)
                {
                    sr.Close();
                    return;
                }
                source = sr.ReadLine();    // 한줄 읽는다.
                DebugLog($"ReadToken = {source}");
            }

        }
        catch (Exception ex)
        {
            DebugLog($"ReadToken ex {ex.Message.ToString()}");
        }
    }


    public void LangBtn()
    {
        //var lean = FindObjectOfType<LeanLocalization>();

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

    [SerializeField]
    private GameObject logView = null;
    private string debugmsg = " ";
    bool ActiveLog = true;
    public void ShowLogView()
    {
        if (ActiveLog)
        {
            //logView.SetActive(false);
            logView.transform.localScale = new Vector3(0, 0, 0);
            ActiveLog = false;
        }
        else
        {
            logView.transform.localScale = new Vector3(1, 1, 1);
            //logView.SetActive(true);
            ActiveLog = true;
        }
    }

    public void ClearLog()
    {
        debugmsg = "";
        logText1.text = debugmsg;
        scrollRect.verticalNormalizedPosition = 0.0f;
    }

    const int kMaxLogSize = 16382;

    [SerializeField]
    private ScrollRect scrollRect = null;
    // Output text to the debug log text field, as well as the console.
    public void DebugLog(string s)
    {
#if UNITY_EDITOR
        Debug.Log($"Ulog >>> {s}");
#elif UNITY_ANDROID
        if ( PluginInit.instance == null )
            return;
        PluginInit.instance.LogUnity(s);
#else
        Console.WriteLine($"Ulog >>> {s}");
#endif
        if (logText1 == null)
        {
            return;
        }

        while (debugmsg.Length > kMaxLogSize)
        {
            int index = debugmsg.IndexOf("\n");
            debugmsg = debugmsg.Substring(index + 1);
        }

        debugmsg += s + "\n";
        logText1.text = debugmsg;


        scrollRect.verticalNormalizedPosition = 0.0f;
    }
    LoginAuth loginAuth = null;
    public void GetloginAuth()
    {
        loginAuth = FindObjectOfType<SampleWebView>().loginAuth;
        DebugLog($"access_token : {loginAuth.token.access_token}");
        DebugLog($"loginAuth : {loginAuth.ToString()}");
    }
        

    string lateLog;

    public void GetCookies()
    {
        webViewSift.webViewObject.CallFromJS("Unity.call(document.cookie);");
    }

    public void Loglate(string s)
    {
        lateLog += s + "\n";
    }

    IEnumerator ShowLateLog()
    {
        while(true)
        {
            //while (lateLog.Length > kMaxLogSize)
            //{
            //    int index = lateLog.IndexOf("\n");
            //    lateLog = lateLog.Substring(index + 1);
            //}
            if (!string.IsNullOrEmpty(lateLog))
            {
                string[] logArr = lateLog.Split('\n');  // 쉼표로 구분한다. 저장시에 쉼표로 구분하여 저장하였다.
                lateLog = null;
                for (int i = 0; i < logArr.Length; i++)
                {
                    if (!string.IsNullOrEmpty(logArr[i]))
                        DebugLog($"latelog >> {logArr[i]}");
                }
            }

            yield return new WaitForEndOfFrame();
        }

    }













}













