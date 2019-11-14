using UnityEngine;
using UnityEngine.UI;

public class ContentSizeControl : MonoBehaviour
{
    private void Start()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        VerticalLayoutGroup vlg = null;
        HorizontalLayoutGroup hlg = null;
        GridLayoutGroup glg = null;

        if (HasComponent<VerticalLayoutGroup>(gameObject)) vlg = GetComponent<VerticalLayoutGroup>();
        else if (HasComponent<HorizontalLayoutGroup>(gameObject)) hlg = GetComponent<HorizontalLayoutGroup>();
        else if (HasComponent<GridLayoutGroup>(gameObject)) glg = GetComponent<GridLayoutGroup>();
        else
        {
            MyDebug.LogError($"Gameobject {name} does not have any required component");
            return;
        }

        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 0);

        float spacing = 0;
        float paddingTop = 0;
        float paddingBottom = 0;
        float paddingLeft = 0;
        float paddingRight = 0;
        int rowCount = 1;
        
        if(vlg != null)
        {
            spacing = vlg.spacing;
            paddingTop = vlg.padding.top;
            paddingBottom = vlg.padding.bottom;
        }
        else if (hlg != null)
        {
            spacing = hlg.spacing;
            paddingLeft = hlg.padding.left;
            paddingRight = hlg.padding.right;
        }
        else if (glg != null)
        {
            spacing = glg.spacing.y;
            paddingTop = glg.padding.top;
            paddingBottom = glg.padding.bottom;
            
            for (rowCount = 1; rowCount < transform.childCount; rowCount++)
            {
                if (transform.GetChild(rowCount).position.y != transform.GetChild(rowCount - 1).position.y)
                {
                    break;
                }
            }
        }

        bool isHorizontal = hlg != null;

        for (int i = 0; i < Mathf.CeilToInt((float)transform.childCount / rowCount); i++)
        {
            if (transform.GetChild(i).gameObject.activeInHierarchy && transform.GetChild(i).tag != "NotContent")
            {
                var rect = transform.GetChild(i).GetComponent<RectTransform>();
                if(!isHorizontal) rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y + rect.sizeDelta.y + spacing);
                else rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x + rect.sizeDelta.x + spacing, rectTransform.sizeDelta.y);
            }
        }
        if(!isHorizontal) rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y + paddingBottom + paddingTop - spacing);
        else rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x + paddingLeft + paddingRight - spacing, rectTransform.sizeDelta.y);

        bool HasComponent<T>(GameObject obj) => obj.GetComponent<T>() != null;
    }
}
