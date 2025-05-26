using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;
    public List<ShopItem> page1Items; // Skins
    public List<ShopItem> page2Items; // Weapons

    public int currentGems;
    public int currentCheese;
    public int playerLevel;

    public GameObject noGemsPanel;
    //public GameObject noCheesePanel;

    private ShopItem selectedSkin;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // Avoid duplicates on scene load
        }
    }

    void Start()
    {
       // LoadPlayerDataFromPlayFab();
        SetupShopItems();
        setLevel(1);
    }

    public void setLevel(int level) 
    {
        playerLevel = level;
    }

    public int getLevel() 
    {
        return playerLevel;
    }

    public void SetupShopItems()
    {
        foreach (ShopItem item in page1Items)
            SetupItem(item, true);

        foreach (ShopItem item in page2Items)
            SetupItem(item, false);
    }

    void SetupItem(ShopItem item, bool isSkin)
    {
        bool canUnlockByLevel = playerLevel >= item.unlockLevel;

        if (isSkin && item.toggleUI != null)
        {
            item.toggleUI.isOn = item.isSelected;
            item.toggleUI.interactable = item.isUnlocked || canUnlockByLevel;

            item.toggleUI.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                    OnItemClicked(item, true);
            });
        }
        else if (!isSkin && item.buttonUI != null)
        {
            item.buttonUI.interactable = item.isUnlocked || canUnlockByLevel;

            item.buttonUI.onClick.AddListener(() =>
            {
                OnItemClicked(item, false);
            });
        }

        // Hide buy panel if already unlocked
        if (item.buyPanel != null)
            item.buyPanel.SetActive(!item.isUnlocked);
    }

    void OnItemClicked(ShopItem item, bool isSkin)
    {
        if (item.isUnlocked)
        {
            if (isSkin)
                SelectSkin(item);
            return;
        }

        bool canBuy = false;

        switch (item.currencyType)
        {
            case CurrencyType.Gems:
                if (currentGems >= item.gemCost)
                {
                    currentGems -= item.gemCost;
                    canBuy = true;
                }
                else
                {
                    noGemsPanel.SetActive(true);
                }
                break;

            case CurrencyType.Cheese:
                if (currentCheese >= item.cheeseCost)
                {
                    currentCheese -= item.cheeseCost;
                    canBuy = true;
                }
                else
                {
                    noGemsPanel.SetActive(true);
                }
                break;
        }

        if (canBuy)
        {
            item.isUnlocked = true;

            if (isSkin)
                SelectSkin(item);

            if (item.buyPanel != null)
                item.buyPanel.SetActive(false);

            SavePurchaseToPlayFab(item);
        }
    }

    void SelectSkin(ShopItem selected)
    {
        foreach (ShopItem item in page1Items)
        {
            if (item != selected)
            {
                item.isSelected = false;
                if (item.toggleUI != null)
                    item.toggleUI.isOn = false;
            }
        }

        selected.isSelected = true;
        selectedSkin = selected;
        SaveSelectionToPlayFab(selected);
    }

    // Call this to increase cheese
    public void AddCheese(int amount)
    {
        currentCheese += amount;
        Debug.Log("Total Cheese: " + currentCheese);
    }

    public void DeactiveGemPanel() 
    {
        noGemsPanel.SetActive(false);
    }

    // Dummy methods for PlayFab
    void SavePurchaseToPlayFab(ShopItem item) { }
    void SaveSelectionToPlayFab(ShopItem item) { }
    void LoadPlayerDataFromPlayFab() { }
}
