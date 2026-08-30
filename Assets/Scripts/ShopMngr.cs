using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ShopMngr : MonoBehaviour
{
    public StatAllocator statAllocator;
    public Button nextSceneButton;
    public string gameplaySceneName = "Gameplay";
    public TMP_Text itemDescriptionText;
    public int startingGold = 2000;

    public TMP_Text goldLabel;
    public Button refundButton;
    public List<ShopItemData> availableItems;
    public Transform itemListParent;
    public GameObject itemButtonPrefab;

    public List<CharacterData> teammates;

    [Header("Cross-references")]
    public CharacterChecker characterChecker;

    private List<ShopItemUI> spawnedItems = new List<ShopItemUI>();
    private readonly Dictionary<ShopItemData, HashSet<CharacterData>> purchasedItemsByCharacter = new Dictionary<ShopItemData, HashSet<CharacterData>>();

    void Start()
    {
        GameMngr.Instance.SetStartingGold(startingGold);

        foreach (var teammate in teammates)
        {
            teammate.ResetRuntimeStats();
        }

        RefreshGoldLabel();
        PopulateShop();

        if (refundButton != null)
        {
            refundButton.onClick.AddListener(OnRefundAllClicked);
            refundButton.interactable = false;
        }

        if (characterChecker != null)
        {
            characterChecker.RefreshDisplay();
        }
    }

    void Update()
    {
        nextSceneButton.interactable = statAllocator.AllPelletsSpent();
    }

    public void OnNextSceneButton()
    {
        if (!statAllocator.AllPelletsSpent()) return;

        GameMngr.Instance.InitFromShop(teammates, statAllocator.GetFinalStats());
        SceneFader.Instance.FadeToScene(gameplaySceneName);
    }

    void PopulateShop()
    {
        foreach (var item in availableItems)
        {
            var go = Instantiate(itemButtonPrefab, itemListParent);
            var itemUI = go.GetComponent<ShopItemUI>();
            itemUI.Setup(item, this);
            spawnedItems.Add(itemUI);
        }
    }

    public void CloseAllPopupsExcept(ShopItemUI keepOpen)
    {
        foreach (var item in spawnedItems)
        {
            if (item != keepOpen)
            {
                item.ClosePopup();
            }
        }
    }

    public bool HasPurchased(ShopItemData item, CharacterData target)
    {
        if (item == null || target == null)
        {
            return false;
        }

        if (!purchasedItemsByCharacter.TryGetValue(item, out var purchasedTargets))
        {
            return false;
        }

        return purchasedTargets.Contains(target);
    }

    public bool IsPurchasedByAnyCharacter(ShopItemData item)
    {
        if (item == null)
        {
            return false;
        }

        return purchasedItemsByCharacter.TryGetValue(item, out var purchasedTargets) && purchasedTargets.Count > 0;
    }

    public bool TryPurchase(ShopItemData item, CharacterData target)
    {
        if (target == null)
        {
            Debug.Log("No target selected");
            return false;
        }

        if (HasPurchased(item, target))
        {
            Debug.Log("This character already owns this item");
            return false;
        }

        if (GameMngr.Instance.gold < item.cost)   // changed from: currentGold < item.cost
        {
            Debug.Log("Not enough gold");
            return false;
        }

        GameMngr.Instance.AddGold(-item.cost);   // changed from: currentGold -= item.cost;
        foreach (var stat in item.statsAffected)
        {
            target.runtimeStats.Add(stat, item.amountPerStat);
        }

        if (!purchasedItemsByCharacter.TryGetValue(item, out var purchasedTargets))
        {
            purchasedTargets = new HashSet<CharacterData>();
            purchasedItemsByCharacter[item] = purchasedTargets;
        }

        purchasedTargets.Add(target);

        RefreshGoldLabel();
        UpdateRefundButtonState();

        if (characterChecker != null)
        {
            characterChecker.RefreshDisplay();
        }

        foreach (var spawnedItem in spawnedItems)
        {
            spawnedItem.RefreshPurchaseDisplay();
        }

        return true;
    }

    public void OnRefundAllClicked()
    {
        int refundTotal = 0;
        foreach (var pair in purchasedItemsByCharacter)
        {
            refundTotal += pair.Key.cost * pair.Value.Count;
        }

        foreach (var teammate in teammates)
        {
            if (teammate != null)
            {
                teammate.ResetRuntimeStats();
            }
        }

        GameMngr.Instance.AddGold(refundTotal); 
        purchasedItemsByCharacter.Clear();

        RefreshGoldLabel();
        UpdateRefundButtonState();

        if (characterChecker != null)
        {
            characterChecker.RefreshDisplay();
        }

        foreach (var spawnedItem in spawnedItems)
        {
            spawnedItem.RefreshPurchaseDisplay();
        }
    }

    void UpdateRefundButtonState()
    {
        if (refundButton == null)
        {
            return;
        }

        refundButton.interactable = purchasedItemsByCharacter.Count > 0;
    }

    void RefreshGoldLabel()
    {
        goldLabel.text = $"{GameMngr.Instance.gold} GOLD";
    }
    public int GetRemainingGold() => GameMngr.Instance.gold;

    public void ShowDescription(string description)
    {
        itemDescriptionText.text = description;
    }

    public void ClearDescription()
    {
        itemDescriptionText.text = "Hover an item to see its effect!";
    }

    public void OnBackClicked()
    {
        SceneManager.LoadScene("Menu");
    }
}