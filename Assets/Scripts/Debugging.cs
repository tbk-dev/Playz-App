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

    public Text logText1;
    public InputField InputField_topic;

    private void Awake()
    {
        instance = this;
        TokenFilePath = Path.Combine(Application.persistentDataPath,"userToken.txt");
    }
    // Start is called before the first frame update
    void Start()
    {
        DebugLog("key S : Subscribe");
        DebugLog("key U : Unsubscribe");
        DebugLog("key T : Toggle Token On Init");
        DebugLog("key L : BottonLogDependencyStatus");

        DebugLog("Origin TokenRegistrationOnInitEnabled = " + FirebaseMessaging.TokenRegistrationOnInitEnabled);

        firebaseMSGSet = FindObjectOfType<FirebaseMSGSet>();
        webViewSift = FindObjectOfType<WebViewSift>();

        if (firebaseMSGSet == null)
            DebugLog("firebaseMSGSet null");

        if (webViewSift == null)
            DebugLog("webViewSift null");

        StartCoroutine(SetMessageDebuging());
    }

    private void Update()
    {
        Order();
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

            DebugLog($"File path : {TokenFilePath} {Environment.NewLine}token = {token}");
        }
        catch (Exception ex)
        {
            DebugLog(ex.Message.ToString());
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
    //public void Send()
    //{
    //    var fmsg = new FirebaseMessage();
    //    fmsg.
    //    FirebaseMessaging.Send()
    //}

    IEnumerator SetMessageDebuging()
    {
        while (true)
        {
            if (firebaseMSGSet.IsFirebaseInitialized)
            {
                DebugLog("SetMessageDebuging");
                Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived_Debugging;
                Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived_Debugging;
                break;
            }
            else
            {
                DebugLog("Waitting firebase init");
            }

            yield return new WaitForSecondsRealtime(0.2f);
        }

    }

    //public void GetToken()
    //{
    //    Firebase.Auth.FirebaseUser user = auth.CurrentUser;
    //    user.TokenAsync(true).ContinueWith(task => {
    //        if (task.IsCanceled)
    //        {
    //            Debug.LogError("TokenAsync was canceled.");
    //            return;
    //        }

    //        if (task.IsFaulted)
    //        {
    //            Debug.LogError("TokenAsync encountered an error: " + task.Exception);
    //            return;
    //        }

    //        string idToken = task.Result;

    //        // Send token to your backend via HTTPS
    //        // ...
    //    });
    //}

    private void OnTokenReceived_Debugging(object sender, TokenReceivedEventArgs token)
    {
        DebugLog("OnTokenReceived_Debugging Registration Token: " + token.Token);
        userToken = token.Token;
        SaveToken(userToken);
    }

    FirebaseMSGSet firebaseMSGSet;
    WebViewSift webViewSift;
    private void OnMessageReceived_Debugging(object sender, MessageReceivedEventArgs e)
    {
        try
        {

            DebugLog("OnMessageReceived_Debugging new message");
            var notification = e.Message.Notification;
            if (notification != null)
            {
                DebugLog("title: " + notification.Title);
                DebugLog("body: " + notification.Body);
                var android = notification.Android;
                if (android != null)
                {
                    DebugLog("android channel_id: " + android.ChannelId);
                }
            }

            if (e.Message.From.Length > 0)
                DebugLog("from: " + e.Message.From);

            if (e.Message.Link != null)
            {
                DebugLog("link: " + e.Message.Link.ToString());
            }

            if (e.Message.Data.Count > 0)
            {
                DebugLog("data:");
                foreach (System.Collections.Generic.KeyValuePair<string, string> iter in e.Message.Data)
                {
                    DebugLog("  " + iter.Key + ": " + iter.Value);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"OnMessageReceived_Debugging = {ex.Message}");
        }
    }


    Firebase.DependencyStatus lastStatus;
    public void Order()
    {
        if (firebaseMSGSet.dependencyStatus != Firebase.DependencyStatus.Available)
        {
            if (lastStatus != Firebase.DependencyStatus.Available && lastStatus != firebaseMSGSet.dependencyStatus)
            {
                DebugLog("One or more Firebase dependencies are not present.");
                DebugLog("Current dependency status: " + firebaseMSGSet.dependencyStatus.ToString());

                lastStatus = firebaseMSGSet.dependencyStatus;
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            BottonSubscribe();
        }
        else if (Input.GetKeyDown(KeyCode.U))
        {
            BottonUnsubscribe();
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            BottonToggleTokenOnInit();
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            BottonLogDependencyStatus();
        }
    }

    const int kMaxLogSize = 16382;
    public ScrollRect scrollRect;

    // Output text to the debug log text field, as well as the console.
    public void DebugLog(string s)
    {
        Debug.Log($"log >>> {s}");

        while (logText1.text.Length > kMaxLogSize)
        {
            int index = logText1.text.IndexOf("\n");
            logText1.text = logText1.text.Substring(index + 1);
        }

        logText1.text += s + "\n";

        scrollRect.verticalNormalizedPosition = 0.0f;
    }

    protected bool LogTaskCompletion(Task task, string operation)
    {
        bool complete = false;
        if (task.IsCanceled)
        {
            DebugLog(operation + " canceled.");
        }
        else if (task.IsFaulted)
        {
            DebugLog(operation + " encounted an error.");
            foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
            {
                string errorCode = "";
                Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                if (firebaseEx != null)
                {
                    errorCode = $"Error.{((Firebase.Messaging.Error)firebaseEx.ErrorCode).ToString()}: ";
                }
                DebugLog(errorCode + exception.ToString());
            }
        }
        else if (task.IsCompleted)
        {
            DebugLog(operation + " completed");
            complete = true;
        }
        return complete;
    }

    public void BottonLogDependencyStatus()
    {
        DebugLog("One or more Firebase dependencies are not present.");
        DebugLog("Current dependency status: " + firebaseMSGSet.dependencyStatus.ToString());
    }

    public void BottonToggleTokenOnInit()
    {
        ToggleTokenOnInit();
    }

    public void BottonSubscribe()
    {
        firebaseMSGSet.SubscribeTopic(InputField_topic.text);
    }

    public void BottonUnsubscribe()
    {
        Firebase.Messaging.FirebaseMessaging.UnsubscribeAsync(InputField_topic.text).ContinueWithOnMainThread(
          task =>
          {
              bool isAction = LogTaskCompletion(task, "UnsubscribeAsync");

              if (isAction)
              {
                  DebugLog("Unsubscribed from " + InputField_topic.text);
              }
              else
              {
                  DebugLog("!!! fail Unsubscribed from " + InputField_topic.text);
              }
          }
        );
    }

    public void ToggleTokenOnInit()
    {
        bool newValue = !Firebase.Messaging.FirebaseMessaging.TokenRegistrationOnInitEnabled;
        Firebase.Messaging.FirebaseMessaging.TokenRegistrationOnInitEnabled = newValue;
        DebugLog("Set TokenRegistrationOnInitEnabled to " + newValue);
    }

    public void Teststart()
    {
        Debug.Log($"log >>> start");
        webViewSift.webViewObject.CallOnStarted("http://www.daum.net");
    }

    public void Testload()
    {
        Debug.Log($"log >>> loaded");
        webViewSift.webViewObject.CallOnLoaded("http://www.naver.com");
    }

    public void CheckAndFixDependencies()
    {
        if (firebaseMSGSet == null)
            firebaseMSGSet = FindObjectOfType<FirebaseMSGSet>();

        firebaseMSGSet.CheckAndFixDependencies();
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

    public GameObject logView;
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

    public void OnDestroy()
    {
        Firebase.Messaging.FirebaseMessaging.MessageReceived -= OnMessageReceived_Debugging;
        Firebase.Messaging.FirebaseMessaging.TokenReceived -= OnTokenReceived_Debugging;
    }

}
