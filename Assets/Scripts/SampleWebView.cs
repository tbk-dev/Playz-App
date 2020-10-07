/*
 * Copyright (C) 2012 GREE, Inc.
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty.  In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 3. This notice may not be removed or altered from any source distribution.
 */

using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
#if UNITY_2018_4_OR_NEWER
using UnityEngine.Networking;
#endif
using UnityEngine.UI;
// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
public class Token
{
    public string token_type { get; set; }
    public string access_token { get; set; }
    public int expires_in { get; set; }
    public string refresh_token { get; set; }
}

public class LoginAuth
{
    public int member_no { get; set; }
    public Token token { get; set; }
    public string redirect { get; set; }
}

// Root myDeserializedClass = JsonConvert.DeserializeObject<SendToken>(myJsonResponse); 
public class SendToken
{
    public string token { get; set; }
}

public class SampleWebView : MonoBehaviour
{

    enum LOGINSTATE
    {
        siteconnect,
        login,
        receivewaitjson,
        complete,
    }
    //public string Url; = //http://www.kiwooza.com/
    public string Url = "http://dev-playz.virtual-gate.co.kr";
    string host = "http://dev-api.playz.virtual-gate.co.kr";

    public Text status;
    public WebViewObject webViewObject;

    public LoginAuth loginAuth;

    LOGINSTATE STATE;

    IEnumerator Start()
    {
        //webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
        webViewObject.Init(
            cb: (msg) =>
            {
                Debugging.instance.DebugLog(string.Format("CallFromJS [ {0} ]", msg));
                status.text = msg;
                status.GetComponent<Animation>().Play();
                if (STATE == LOGINSTATE.receivewaitjson)
                {
                    if(msg.Contains("token"))
                    {
                        try
                        {
                            Debugging.instance.DebugLog($"jsonSting : {msg}");
                            loginAuth = JsonConvert.DeserializeObject<LoginAuth>(msg);
                            STATE = LOGINSTATE.complete;
                            webViewObject.EvaluateJS(@"window.location.replace('"+Url+"');");
                            Debugging.instance.DebugLog("location.replace");

                            StartCoroutine(SendToken(REQUEST_TYPE.Post, loginAuth.member_no));
                        }
                        catch (Exception ex)
                        {
                            Debugging.instance.DebugLog($"jsonConvert file : {ex.Message}");

                        }

                        //window.location.assign('http://www.example.com');
                    }
                }
            },
            err: (msg) =>
            {
                Debugging.instance.DebugLog(string.Format("CallOnError[ {0} ]", msg));
                status.text = msg;
                status.GetComponent<Animation>().Play();

            },
            started: (msg) =>
            {
                Debugging.instance.DebugLog(string.Format("CallOnStarted[ {0} ]", msg));

                if (msg.Contains(@"member/login"))
                {
                    if (!msg.Contains("response_type=jwt"))
                    {
                        Debugging.instance.DebugLog("page redirect");
                        webViewObject.LoadURL($"{Url}/member/login?response_type=jwt");
                    }
                } else if (msg.Contains(@"/member/logout"))
                {
                    StartCoroutine(SendToken(REQUEST_TYPE.Delete, loginAuth.member_no));
                }
            },
            hooked: (msg) =>
            {
                Debugging.instance.DebugLog(string.Format("CallOnHooked[{0}]", msg));
            },
            ld: (msg) =>
            {
                Debugging.instance.DebugLog(string.Format("CallOnLoaded[{0}]", msg));

                if (Debugging.instance.UrlText != null)
                    Debugging.instance.UrlText.text = msg;

                if (msg.Contains(@"response_type=jwt"))
                {
                    CallInnerText();
                }
                else if (msg.Contains(@"member/login"))
                {
                    var cookies = webViewObject.GetCookies(Url);
                    Debugging.instance.DebugLog($"cookies :: {cookies}");
                }
                else
                {
                    //outher
                }


#if UNITY_EDITOR_OSX || !UNITY_ANDROID
                // NOTE: depending on the situation, you might prefer
                // the 'iframe' approach.
                // cf. https://github.com/gree/unity-webview/issues/189
#if true
                webViewObject.EvaluateJS(@"
                  if (window && window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.unityControl) {
                    window.Unity = {
                      call: function(msg) {
                        window.webkit.messageHandlers.unityControl.postMessage(msg);
                      }
                    }
                  } else {
                    window.Unity = {
                      call: function(msg) {
                        window.location = 'unity:' + msg;
                      }
                    }
                  }
                ");
#else
                webViewObject.EvaluateJS(@"
                  if (window && window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.unityControl) {
                    window.Unity = {
                      call: function(msg) {
                        window.webkit.messageHandlers.unityControl.postMessage(msg);
                      }
                    }
                  } else {
                    window.Unity = {
                      call: function(msg) {
                        var iframe = document.createElement('IFRAME');
                        iframe.setAttribute('src', 'unity:' + msg);
                        document.documentElement.appendChild(iframe);
                        iframe.parentNode.removeChild(iframe);
                        iframe = null;
                      }
                    }
                  }
                ");
#endif
#endif

                //ua: "custom user agent string",
                //webViewObject.EvaluateJS(@"Unity.call('ua1 = ' + navigator.userAgent)");

            },
            enableWKWebView: true);


#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        webViewObject.bitmapRefreshCycle = 1;
#endif
        //webViewObject.SetAlertDialogEnabled(false);
        //webViewObject.SetURLPattern("", "^https://.*youtube.com", "^https://.*google.com");

        if (Screen.width < Screen.height)
            webViewObject.SetMargins(0, 0, 0, (int)(Screen.height * 0.1));
        else
            webViewObject.SetMargins(0, 0, 0, (int)(Screen.height - 192));

        //Debug.Log($"log >>> : height : {Screen.height} , 0.1 : {(int)(Screen.height * 0.1)}  차이 {Screen.height- (int)(Screen.height * 0.1)}  ");
        //Debug.Log($"log >>> : width : {Screen.width} , 0.1 : {(int)(Screen.width * 0.1)}  차이 {Screen.width - (int)(Screen.width * 0.1)}  ");
        webViewObject.SetVisibility(true);
        
#if !UNITY_WEBPLAYER && !UNITY_WEBGL
        if (Url.StartsWith("http"))
        {
            webViewObject.LoadURL(Url.Replace(" ", "%20"));
        }
        else
        {
            var exts = new string[]{
                ".jpg",
                ".js",
                ".html"  // should be last
            };
            foreach (var ext in exts)
            {
                var url = Url.Replace(".html", ext);
                var src = System.IO.Path.Combine(Application.streamingAssetsPath, url);
                var dst = System.IO.Path.Combine(Application.persistentDataPath, url);
                byte[] result = null;
                if (src.Contains("://"))
                {  // for Android
#if UNITY_2018_4_OR_NEWER
                    // NOTE: a more complete code that utilizes UnityWebRequest can be found in https://github.com/gree/unity-webview/commit/2a07e82f760a8495aa3a77a23453f384869caba7#diff-4379160fa4c2a287f414c07eb10ee36d
                    var unityWebRequest = UnityWebRequest.Get(src);
                    yield return unityWebRequest.SendWebRequest();
                    result = unityWebRequest.downloadHandler.data;
#else
                    var www = new WWW(src);
                    yield return www;
                    result = www.bytes;
#endif
                }
                else
                {
                    result = System.IO.File.ReadAllBytes(src);
                }
                System.IO.File.WriteAllBytes(dst, result);
                if (ext == ".html")
                {
                    webViewObject.LoadURL("file://" + dst.Replace(" ", "%20"));
                    break;
                }
            }
        }
#else
        if (Url.StartsWith("http")) {
            webViewObject.LoadURL(Url.Replace(" ", "%20"));
        } else {
            webViewObject.LoadURL("StreamingAssets/" + Url.Replace(" ", "%20"));
        }
        webViewObject.EvaluateJS(
            "parent.$(function() {" +
            "   window.Unity = {" +
            "       call:function(msg) {" +
            "           parent.unityWebView.sendMessage('WebViewObject', msg)" +
            "       }" +
            "   };" +
            "});");
#endif
        yield break;
    }

    public void CallInnerText()
    {
        //webViewObject.EvaluateJS("Unity.call('"+tag+">>> '+ document.documentElement.innerText.toString());");
        webViewObject.EvaluateJS("Unity.call(document.documentElement.innerText.toString());");
        STATE = LOGINSTATE.receivewaitjson;
    }


    public enum REQUEST_TYPE
    {
        Post,
        Delete,
        Get
    }


    //http://dev-api.playz.virtual-gate.co.kr/member/100059/fcm/token
    IEnumerator SendToken(REQUEST_TYPE _TYPE, int member_no)
    {
        string token = FindObjectOfType<FirebaseMSGSet>().userToken;
        if(string.IsNullOrEmpty(token))
        {
            Debugging.instance.DebugLog($"::: IsNullOrEmpty(FCM_token)");
            yield return null;
        }


        string adress = $"{host}/member/{member_no}/fcm/token?t={DateTime.Now.Millisecond}";
        Debugging.instance.DebugLog("::: adress " + adress);
        Debugging.instance.DebugLog("::: access_token : " + loginAuth.token.access_token);
        Debugging.instance.DebugLog($"::: FCM_token : {token}");


        UnityWebRequest www = new UnityWebRequest();
        if (_TYPE == REQUEST_TYPE.Post)
        {
            WWWForm form = new WWWForm();
            form.AddField("token", token);


            www = UnityWebRequest.Post(adress, form);
        }
        else if (_TYPE == REQUEST_TYPE.Delete)
        {
            www = UnityWebRequest.Delete(adress);

            var sendToken = new SendToken();
            sendToken.token = token;
            var jsonstring = JsonConvert.SerializeObject(sendToken);


            if (!string.IsNullOrEmpty(jsonstring))
            {
                byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonstring);
                www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
                www.SetRequestHeader("Content-Type", "Application/json");
            }
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        }

        www.SetRequestHeader("Authorization", $"{loginAuth.token.token_type} {loginAuth.token.access_token}");


        //Send the request then wait here until it returns
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            yield return null;
        }

        // Show results as text
        //Debug.Log(www.downloadHandler.text);

        // Or retrieve results as binary data
        Debugging.instance.DebugLog("::: downloadHandler " + www.downloadHandler.text);
        
        byte[] results = www.downloadHandler.data;
        ////string str = Encoding.Default.GetString(results);

        //Debug.Log($"dataStr {str}");
        //var JsonObject = ParsingJson2TeamInfoList(str);
        //ActiveRequestEvent(JsonObject);

        //    using (FileStream file = new FileStream(Application.dataPath + "\\urls33333333333333.txt", FileMode.Create, FileAccess.ReadWrite, FileShare.Read))   // 지정된 경로에 파일을 생성 
        //    {
        //        // 생성된 파일에 바이트배열로 저장한 컨텐츠 파일을 씀
        //        file.Write(results, 0, results.Length);
        //        Debug.LogError("!!! : " + results);
        //    }
        yield return null;
    }


    IEnumerator GetTimeTable(string suburl)
    {

        string adress = $"{suburl}?t={DateTime.Now.Millisecond}";

        UnityWebRequest www = UnityWebRequest.Get(adress);
        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            yield return null;
        }

        // Show results as text
        //resultData = www.downloadHandler.text;

        // Or retrieve results as binary data
        var resultData = www.downloadHandler.data;
        //string str = Encoding.UTF8.GetString(resultData);
        //timeTableData = str.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

        yield return null;
    }


//클래스 저장
    public void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/playerInfo.dat");
        LoginAuth data = new LoginAuth();
        data = loginAuth;
        bf.Serialize(file, data);
        file.Close();
    }
    public void Load()
    {
        if (File.Exists(Application.persistentDataPath + "/ playerInfo.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/ playerInfo.dat", FileMode.Open);
            LoginAuth data = (LoginAuth)bf.Deserialize(file);
            file.Close();
            loginAuth = data;
        }
    }

    //void OnGUI()
    //{
    //    GUI.enabled = webViewObject.CanGoBack();
    //    if (GUI.Button(new Rect(10, 10, 80, 80), "<")) {
    //        webViewObject.GoBack();
    //    }
    //    GUI.enabled = true;

    //    GUI.enabled = webViewObject.CanGoForward();
    //    if (GUI.Button(new Rect(100, 10, 80, 80), ">")) {
    //        webViewObject.GoForward();
    //    }
    //    GUI.enabled = true;

    //    GUI.TextField(new Rect(200, 10, 300, 80), "" + webViewObject.Progress());

    //    if (GUI.Button(new Rect(600, 10, 80, 80), "*")) {
    //        var g = GameObject.Find("WebViewObject");
    //        if (g != null) {
    //            Destroy(g);
    //        } else {
    //            StartCoroutine(Start());
    //        }
    //    }
    //    GUI.enabled = true;

    //    if (GUI.Button(new Rect(700, 10, 80, 80), "c")) {
    //        Debug.Log(webViewObject.GetCookies(Url));
    //    }
    //    GUI.enabled = true;
    //}

}
