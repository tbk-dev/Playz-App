using DanielLochner.Assets.SimpleSideMenu;
using Lean.Localization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DanielLochner.Assets.SimpleSideMenu.SimpleSideMenu;

public class WebViewSift : MonoBehaviour
{
    public WebViewObject webViewObject { get; private set; }
    SimpleSideMenu simpleSideMenu;


    public SimpleSideMenu right_simpleSideMenu;
    RectTransform rectTransform;

    public SimpleSideMenu left_simpleSideMenu;

    public SettingMenu SettingMenuOBJ;

    void Start()
    {
        webViewObject = FindObjectOfType<WebViewObject>();

        if (right_simpleSideMenu != null)
            rectTransform = right_simpleSideMenu.GetComponent<RectTransform>();

    }

    void Update()
    {

        //if (simpleSideMenu.TargetState == State.Closed)
        //if (rightsimpleSideMenu.TargetState == State.Closed)
        if (SettingMenuOBJ.isActiveMenu)
        {
            if (webViewObject.GetVisibility())
                webViewObject.SetVisibility(false);
        }
        else
        {
            if(webViewObject.GetVisibility() == false)
                webViewObject.SetVisibility(true);
        }
        Debugging.instance.DebugLog("webViewObject : " + webViewObject.transform.position.ToString());
    }


    public void WebViewGoBack()
    {
        webViewObject.GoBack();
    }

    public void WebViewGoForward()
    {
        webViewObject.GoForward();
    }

    public void WebViewReflash()
    {
        webViewObject.EvaluateJS("location.reload(true)");
    }

    public void WebViewGoLogin()
    {
        webViewObject.LoadURL($"{mainhost}/member/login");
    }
    public void WebViewGoLoginjwtp()
    {
        webViewObject.LoadURL($"{mainhost}/member/login?response_type=jwt");
    }




    public string mainhost = "http://dev-playz.virtual-gate.co.kr";
    //public string mainhost = "http://www.kiwooza.com/";
    public void WebViewGoHome()
    {
        CloseSideMenu();

        webViewObject.LoadURL(mainhost);
        //webViewObject.LoadURL(Url.Replace(" ", "%20"));
    }
    public void WebViewGoMap()
    {
        CloseSideMenu();

        //webViewObject.LoadURL($"{mainhost}/planet#direct");
        webViewObject.LoadURL($"{mainhost}/lounge#direct");
    }

    public void WebViewGoWorkShop()
    {
        CloseSideMenu();

        webViewObject.LoadURL($"{mainhost}/party");
        //webViewObject.LoadURL($"{mainhost}/workshop");
    }

    public void WebViewGoNotice()
    {
        CloseSideMenu();

        webViewObject.LoadURL($"{mainhost}/board/notice");
    }



    public void CloseSideMenu()
    {
        //right_simpleSideMenu.Close();
        //left_simpleSideMenu.Open();
    }

    public void OpenSideMenu()
    {
        //right_simpleSideMenu.Open();
        //left_simpleSideMenu.Close();
    }


}

