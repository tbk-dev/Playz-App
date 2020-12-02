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


[Serializable]
public class Token
{
    public string token_type { get; set; }
    public string access_token { get; set; }
    public int expires_in { get; set; }
    public string refresh_token { get; set; }
}

// LoginAuth myDeserializedClass = JsonConvert.DeserializeObject<LoginAuth>(myJsonResponse); 
[Serializable]
public class LoginAuth
{
    public int member_no { get; set; }
    public Token token { get; set; }
    public string redirect { get; set; }
}

[Serializable]
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
    private void Awake()
    {
        LoadLoginAuth();
    }

    // cf. https://answers.unity.com/questions/1013011/convert-recttransform-rect-to-screen-space.html?childToView=1628573#answer-1628573
    public static Bounds GetRectTransformBounds(RectTransform transform)
    {
        var corners = new Vector3[4];
        transform.GetWorldCorners(corners);
        var bounds = new Bounds(corners[0], Vector3.zero);
        for (var i = 1; i < 4; i++)
        {
            bounds.Encapsulate(corners[i]);
        }
        return bounds;
    }

    IEnumerator Start()
    {
        webViewObject.Init(
            cb: (msg) =>
            {
                //Debugging.instance.DebugLog(string.Format("CallFromJS [ {0} ]", msg));
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

                            SaveLoginAuth(loginAuth);
                            STATE = LOGINSTATE.complete;
                            webViewObject.EvaluateJS(@"window.location.replace('"+Url+"');");
                            Debugging.instance.DebugLog("location.replace");

                            StartCoroutine(SendToken(REQUEST_TYPE.Post, loginAuth));
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
                    StartCoroutine(SendToken(REQUEST_TYPE.Delete, loginAuth));
                }
            },
            hooked: (msg) =>
            {
                Debugging.instance.DebugLog(string.Format("CallOnHooked[{0}]", msg));
            },
            ld: (msg) =>
            {
                Debugging.instance.DebugLog(string.Format("CallOnLoaded[{0}]", msg));

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
                
                //ua: "custom user agent string",
                //webViewObject.EvaluateJS(@"Unity.call('ua1 = ' + navigator.userAgent)");
            },
            enableWKWebView: true);


#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        webViewObject.bitmapRefreshCycle = 1;
#endif
        //webViewObject.SetAlertDialogEnabled(false);
        //webViewObject.SetURLPattern("", "^https://.*youtube.com", "^https://.*google.com");

        if (Screen.safeArea.width < Screen.safeArea.height)
        {
            var settingPanelHeight = Screen.safeArea.height * 0.9f;

            var buttomPanelHeight = Screen.safeArea.height - settingPanelHeight;
            var buttomPanelCenter = buttomPanelHeight * 0.5f;

            Rect newWebviewArea = new Rect
            {
                height = settingPanelHeight,
                width = Screen.safeArea.width - 300,
                y = buttomPanelCenter
            };

            webViewObject.SetCenterPositionWithScale(new Vector2(0, buttomPanelCenter), new Vector2(Screen.safeArea.width, settingPanelHeight));

            //webViewObject.SetMargins(0, 0, 0, (int)((Screen.safeArea.y + Screen.safeArea.height) * 0.1));
        }
        else
        {
            //가로길이가 더 큰경우
            //webViewObject.SetMargins(0, 0, 0, (int)((Screen.safeArea.y +Screen.safeArea.height) - 192));
            Debug.Log($"");

        }

        webViewObject.SetVisibility(true);
            Debug.Log($"webViewObject.SetVisibility(true)  webViewObject.SetVisibility(true)  webViewObject.SetVisibility(true)  ");

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
        STATE = LOGINSTATE.receivewaitjson;
        webViewObject.EvaluateJS("Unity.call(document.documentElement.innerText.toString());");
    }


    public enum REQUEST_TYPE
    {
        Post,
        Delete,
        Get
    }



    //http://dev-api.playz.virtual-gate.co.kr/member/100059/fcm/token
    public IEnumerator SendToken(REQUEST_TYPE _TYPE, LoginAuth loginAuth, System.Action<bool> callback = null)
    {
        if(loginAuth == null)
        {
            Debugging.instance.DebugLog($"::: SendToken break");
            yield break;
        }

        var token = GetUserToken();

        string adress = $"{host}/member/{loginAuth.member_no}/fcm/token?t={DateTime.Now.Millisecond}";
        Debugging.instance.DebugLog($"adress {adress}");
        Debugging.instance.DebugLog($"access_token : {loginAuth.token.access_token}");


        UnityWebRequest www = new UnityWebRequest();
        if (_TYPE == REQUEST_TYPE.Post)
        {
            Debugging.instance.DebugLog($"REQUEST_TYPE.Post");
            WWWForm form = new WWWForm();
            form.AddField("token", token);


            www = UnityWebRequest.Post(adress, form);
        }
        else if (_TYPE == REQUEST_TYPE.Delete)
        {
            Debugging.instance.DebugLog($"REQUEST_TYPE.Delete");
            www = UnityWebRequest.Delete(adress);

            //www.useHttpContinue = useHttpContinue;
            var sendToken = new SendToken();
            sendToken.token = token;
            var jsonstring = JsonConvert.SerializeObject(sendToken);


            if (!string.IsNullOrEmpty(jsonstring))
            {
                byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonstring);
                www.uploadHandler = new UploadHandlerRaw(jsonToSend);
                www.SetRequestHeader("Content-Type", "Application/json");
            }
            www.downloadHandler = new DownloadHandlerBuffer();
        }

        var authtoken = $"{loginAuth.token.token_type} {loginAuth.token.access_token}";
        www.SetRequestHeader("Authorization", authtoken);
        //www.SetRequestHeader("Authorization", token);

        //Send the request then wait here until it returns
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log($"::: Request Error : {www.error}");
            callback?.Invoke(false);

            yield return null;
        }
        else
        {
            //todo 전송 성공시 상태알림 (버튼 on off처리)
            callback?.Invoke(true);

            // Or retrieve results as binary data
            Debugging.instance.DebugLog($"downloadHandler {www.downloadHandler.text}");

            // Show results as text
            //byte[] results = www.downloadHandler.data;
            //string str = Encoding.Default.GetString(results);

            //json 오브젝트 변환
            //var JsonObject = ParsingJson2TeamInfoList(str);
            //ActiveRequestEvent(JsonObject);

            //파일로 저장
            //    using (FileStream file = new FileStream($"{Application.dataPath}\\result.txt", FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            //    {
            //        // 생성된 파일에 바이트배열로 저장한 컨텐츠 파일을 씀
            //        file.Write(results, 0, results.Length);
            //        Debug.LogError("!!! : " + results);
            //    }
            yield return null;
        }
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
    public void SaveLoginAuth(LoginAuth loginAuthdata)
    {
        BinaryFormatter bf = new BinaryFormatter();

        var filePath = $"{Application.persistentDataPath}/playerInfo.dat";
        FileStream file = File.Create(filePath);

        LoginAuth authData = loginAuthdata;
        bf.Serialize(file, authData);
        file.Close();
        Debugging.instance.DebugLog($"SaveLoginAuth :: {filePath}");
    }

    public LoginAuth LoadLoginAuth()
    {
        var filePath = $"{Application.persistentDataPath}/playerInfo.dat";

        if (File.Exists(filePath))
        {
            using (FileStream file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.Open))
            {
                BinaryFormatter bf = new BinaryFormatter();
                LoginAuth data = (LoginAuth)bf.Deserialize(file);
                file.Close();
                loginAuth = data;
                Debugging.instance.DebugLog($"LoadLoginAuth >> {loginAuth}");
            }
            return loginAuth;
        }

        return null;
    }


    public string GetUserToken()
    {
        string token = FindObjectOfType<FirebaseMSGSet>()?.userToken;
        if (string.IsNullOrEmpty(token))
        {
            Debugging.instance.DebugLog($"::: IsNullOrEmpty(FirebaseMSGSet token)");
            return null;
        }

        Debugging.instance.DebugLog($"GetUserToken : {token}");

        return token;
    }


}
