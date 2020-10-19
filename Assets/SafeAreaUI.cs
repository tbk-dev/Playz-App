using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeAreaUI : MonoBehaviour
{
    void Awake()
    {
        Panel = GetComponent<RectTransform>();
        Refresh();
    }

    RectTransform Panel;
    Rect LastSafeArea = new Rect(0, 0, 0, 0);
    
    void Update()
    {
        Refresh();
    }

    void Refresh()
    {
        Rect safeArea = GetSafeArea();

        if (safeArea != LastSafeArea)
            ApplySafeArea(safeArea);
    }

    // 세이프 에어리어 영역값 가져오기. 노치가 없으면, new Rect(0, 0, Screen.Width, Screen.Height) 값과 동일하다.
    Rect GetSafeArea()
    {
        return Screen.safeArea;
    }

    void ApplySafeArea(Rect r)
    {
        LastSafeArea = r;

        // Convert safe area rectangle from absolute pixels to normalised anchor coordinates
        // 이 게이오브젝트의 RectTransform 앵커최대최소값을 다시 설정해서 세이프 에어리어의 영역만큼 잡히도록 한다.
        Vector2 anchorMin = r.position;
        Vector2 anchorMax = r.position + r.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;

        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;
        Panel.anchorMin = anchorMin;
        Panel.anchorMax = anchorMax;

        //Debug.LogFormat($"New safe area applied to {name}: x={r.x}, y={r.y}, w={r.width}, h={r.height} on full extents w={Screen.width}, h={Screen.height}");
    }
}
