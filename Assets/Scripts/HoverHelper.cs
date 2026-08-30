using UnityEngine;
using UnityEngine.EventSystems;

public class HoverHelper : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ShopItemUI itemUI;

    public void OnPointerEnter(PointerEventData eventData) => itemUI.OnPointerEnter(eventData);
    public void OnPointerExit(PointerEventData eventData) => itemUI.OnPointerExit(eventData);
}