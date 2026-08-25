using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    public Image iconImage;
    public Button buyButton;
    public TMP_Text buyLabel;
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
        characterPopup.SetActive(false);

        if (iconImage != null)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = item.icon != null;
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
}