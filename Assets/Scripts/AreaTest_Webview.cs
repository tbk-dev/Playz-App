using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaTest_Webview : MonoBehaviour
{

    RectTransform rect;
    public bool selfRect = false;
    public int left;
    public int top;
    public int right;
    public int bottom;
    // Start is called before the first frame update
    void Start()
    {
        rect = GetComponent<RectTransform>();

    }
    public float 비율 = 0.9f;
    // Update is called once per frame
    void Update()
    {
        //Vector2 newVector2;
        //    newVector2.x = Screen.safeArea.size.x;

        //    var he = Screen.safeArea.yMax* 비율;
        //    newVector2.y = he;

        //Debug.Log($">>> newVector2 {newVector2}");
        //rect.sizeDelta = newVector2;

        Debug.Log($">>> AreaTest_Webview.rect.height {rect.rect.height}");


        //safeArea wid = 1125
        //safeArea hei = 2202

        //rect.position = new Vector3(Screen.safeArea.x, Screen.safeArea.y);
        //MarginsEdit();
    }

    public void MarginsEdit()
    {
        if (!selfRect)
        { 
        SetMargins(left, top, right, bottom);
            //SetMargins(rect, left, top, right, bottom);
        }
        else
        {
            left = (int)rect.offsetMin.x;
            bottom = (int)rect.offsetMin.y;
            right = (int)rect.offsetMax.x;
            top = (int)rect.offsetMax.y;
        }
    }

    public static void SetMargins(RectTransform trs, int left, int top, int right, int bottom)
    {
        trs.offsetMin = new Vector2(left, bottom);
        trs.offsetMax = new Vector2(right, -top);
    }

    int mMarginLeft;
    int mMarginTop;
    int mMarginRight;
    int mMarginBottom;

    public void SetMargins(int left, int top, int right, int bottom, bool relative = false)
    {
        mMarginLeft = left;
        mMarginTop = top;
        mMarginRight = right;
        mMarginBottom = bottom;

        float w = (float)Screen.width;
        float h = (float)Screen.height;
        int iw = Screen.currentResolution.width;
        int ih = Screen.currentResolution.height;


        var newLeft = (int)(left / w * iw);
        var newTop = (int)(top / h * ih);
        var newRight = (int)(right / w * iw);
        var newBottom = (int)(bottom / h * ih);


        rect.offsetMin = new Vector2(newLeft, newBottom);
        rect.offsetMax = new Vector2(newRight, -newTop);
    }
}

