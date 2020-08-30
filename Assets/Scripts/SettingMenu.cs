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
        settingRectObj.anchoredPosition = new Vector3(settingRectObj.rect.width, 0);
    }


    public void HideSettingMenu()
    {
        settingRectObj.anchoredPosition = new Vector3(settingRectObj.rect.width, 0);
        isActiveMenu = false;
    }

    public void ShowSettingMenu()
    {
        settingRectObj.anchoredPosition = new Vector3(0, 0);
        isActiveMenu = true;
    }

}
