using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingMenu : MonoBehaviour
{

    public GameObject SettingOBJ;
    private RectTransform settingRectObj;
    public bool isActiveMenu = false;

    // Start is called before the first frame update
    void Awake()
    {
        settingRectObj = SettingOBJ.GetComponent<RectTransform>();
    }

    private void Start()
    {
        isActiveMenu = false;
    }

    private void Update()
    {
        if (isActiveMenu && !settingRectObj.gameObject.activeSelf)
        {
            ShowSettingMenu();
        }
        
        if(!isActiveMenu && settingRectObj.gameObject.activeSelf)
        { 
            HideSettingMenu();
        }
    }

    Vector3 hidePos;
    Vector3 visiblePos;
    public void HideSettingMenu()
    {
        settingRectObj.anchoredPosition = hidePos;
        settingRectObj.gameObject.SetActive(false);
        isActiveMenu = false;
    }

    public void ShowSettingMenu()
    {
        settingRectObj.anchoredPosition = visiblePos;
        settingRectObj.gameObject.SetActive(true);
        isActiveMenu = true;
    }

    public void SetHidePosSettingMenu(Vector3 pos)
    {
        hidePos = pos;
    }
    public void SetVisiblePosSettingMenu(Vector3 pos)
    {
        visiblePos = pos;
    }

}
