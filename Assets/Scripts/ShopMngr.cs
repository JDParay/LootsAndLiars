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
    private int currentGold;

    public TMP_Text goldLabel;
    public List<ShopItemData> availableItems;
    public Transform itemListParent;
    public GameObject itemButtonPrefab;

    public List<CharacterData> teammates;

    [Header("Cross-references")]
    public CharacterChecker characterChecker;

    private List<ShopItemUI> spawnedItems = new List<ShopItemUI>();

    void Start()
    {
        currentGold = startingGold;

        foreach (var teammate in teammates)
        {
            teammate.ResetRuntimeStats();
        }

        RefreshGoldLabel();
        PopulateShop();

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
        SceneManager.LoadScene(gameplaySceneName);
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

    public bool TryPurchase(ShopItemData item, CharacterData target)
    {
        if (currentGold < item.cost)
        {
            Debug.Log("Not enough gold");
            return false;
        }

        currentGold -= item.cost;
        foreach (var stat in item.statsAffected)
        {
            target.runtimeStats.Add(stat, item.amountPerStat);
        }

        RefreshGoldLabel();

        if (characterChecker != null)
        {
            characterChecker.RefreshDisplay();
        }

        return true;
    }

    void RefreshGoldLabel()
    {
        goldLabel.text = $"Gold: {currentGold}";
    }

    public int GetRemainingGold() => currentGold;

    public void ShowDescription(string description)
    {
        itemDescriptionText.text = description;
    }

    public void ClearDescription()
    {
        itemDescriptionText.text = "Hover an item to see its effect!";
    }
}