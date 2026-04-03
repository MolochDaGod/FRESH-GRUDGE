#if _SQLITE
using UnityEngine;
using UnityEngine.EventSystems;

public class UIShowItemTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public string text;

    public void OnPointerEnter(PointerEventData data)
    {
        //Debug.Log(text);
        UIItemTooltip.singleton.Show(gameObject, text);
    }

    public void OnPointerExit(PointerEventData data)
    {
        UIItemTooltip.singleton.Hide();
    }
}

#endif
