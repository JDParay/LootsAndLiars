using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    public Image iconImage;
    public Image sparkleIconImage;
    public Image PriceIcon;

    public Button buyButton;
    public TMP_Text buyLabel;
    public TMP_Text itemNameLabel;
    public GameObject characterPopup;
    public Button[] characterButtons;
    public TMP_Text[] characterLabels;

    private ShopItemData itemData;
    private ShopMngr shopManager;

    public void Setup(ShopItemData item, ShopMngr manager)
    {
        itemData = item;
        shopManager = manager;

        buyLabel.text = $"Buy it for...";
        buyButton.interactable = true;
        itemNameLabel.text = item.itemName;
        characterPopup.SetActive(false);

        if (iconImage != null)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = item.icon != null;
        }

        if (sparkleIconImage != null)
        {
            sparkleIconImage.sprite = item.sparkleIcon;
            sparkleIconImage.enabled = item.sparkleIcon != null;
        }

        if (PriceIcon != null)
        {
            PriceIcon.sprite = item.PriceIcon;
            PriceIcon.enabled = item.PriceIcon != null;
        }

        buyButton.onClick.AddListener(ToggleCharacterPopup);

        for (int i = 0; i < characterButtons.Length; i++)
        {
            var teammate = shopManager.teammates[i];
            characterLabels[i].text = teammate.characterName;

            int index = i;
            characterButtons[i].onClick.AddListener(() => ConfirmPurchase(shopManager.teammates[index]));
        }
    }

    void ToggleCharacterPopup()
    {
        bool willOpen = !characterPopup.activeSelf;

        if (willOpen)
        {
            shopManager.CloseAllPopupsExcept(this);
        }

        characterPopup.SetActive(willOpen);
    }

    public void ClosePopup()
    {
        characterPopup.SetActive(false);
    }

    void ConfirmPurchase(CharacterData target)
    {
        bool success = shopManager.TryPurchase(itemData, target);
        if (success)
        {
            characterPopup.SetActive(false);
            buyButton.interactable = false;
            buyLabel.text = "Purchased";
        }
    }

    public void RefreshPurchaseDisplay()
    {
        bool hasAnyPurchased = shopManager.IsPurchasedByAnyCharacter(itemData);
        buyButton.interactable = !hasAnyPurchased;
        buyLabel.text = hasAnyPurchased ? "Purchased" : "Buy it for...";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Pointer entered");
        shopManager.ShowDescription(itemData.description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        shopManager.ClearDescription();
    }
}