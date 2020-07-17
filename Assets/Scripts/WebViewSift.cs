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
    RectTransform rectTransform;

    Vector2 closedPosition, openPosition, startPosition, releaseVelocity, menuSize;

    void Start()
    {
        startPosition = transform.position;
        webViewObject = FindObjectOfType<WebViewObject>();

        simpleSideMenu = FindObjectOfType<SimpleSideMenu>();
        rectTransform = simpleSideMenu.GetComponent<RectTransform>();

        closedPosition = new Vector2(640, rectTransform.localPosition.y);
        openPosition = new Vector2(rectTransform.rect.width, rectTransform.localPosition.y);
    }

    void Update()
    {
        //Vector2 targetPosition = (simpleSideMenu.TargetState == State.Closed) ? closedPosition : openPosition;

        if (simpleSideMenu.TargetState == State.Closed)
        {
            webViewObject.SetVisibility(false);
        }
        else
        {
            webViewObject.SetVisibility(true);
        }

    }

    public void WebViewGoBack()
    {
        webViewObject.GoBack();
    }

    public void WebViewGoForward()
    {
        webViewObject.GoForward();
    }

    public void WebViewGoHome()
    {
        webViewObject.LoadURL("http://www.kiwooza.com/");
        //webViewObject.LoadURL(Url.Replace(" ", "%20"));
    }
    public void WebViewGoMap()
    {
        webViewObject.LoadURL("http://www.kiwooza.com/planet#direct");
    }

    public void WebViewGoWorkShop()
    {
        webViewObject.LoadURL("http://www.kiwooza.com/workshop");
    }

    public void WebViewGoNotice()
    {
        webViewObject.LoadURL("http://www.kiwooza.com/board/notice");
    }

    public void WebViewReflash()
    {
        webViewObject.EvaluateJS("location.reload(true)");
    }

}
