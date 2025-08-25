using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    public TMP_Text coinCurrencyTxt;
    public TMP_Text coinCurrencyTxt2;
    public TMP_Text coinStoreCurrencyTxt;

    public TMP_Text gemCurrencyTxt;
    public TMP_Text gemStoreCurrencyTxt;
    //public TMP_Text gemCurrencyTxt2;

    public GameObject shieldPrefab;
    public GameObject nukePrefab;

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

            // Locked skins are NOT directly interactable
            item.toggleUI.interactable = item.isUnlocked;

            // Use a separate invisible Button overlay to handle "Buy" clicks
            Button clickCatcher = item.toggleUI.GetComponentInChildren<Button>(true);
            if (clickCatcher != null)
            {
                clickCatcher.onClick.RemoveAllListeners();
                clickCatcher.onClick.AddListener(() =>
                {
                    if (item.isUnlocked)
                    {
                        OnItemClicked(item, true); // Normal selection
                    }
                    else
                    {
                        TryBuyItem(item);
                    }
                });
            }
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
                    gemCurrencyTxt.text = currentGems.ToString();
                    gemStoreCurrencyTxt.text = currentGems.ToString();
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
                    coinCurrencyTxt.text = currentCheese.ToString();
                    coinCurrencyTxt2.text = currentCheese.ToString();
                    coinStoreCurrencyTxt.text = currentCheese.ToString();
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
            {
                Debug.Log("Bought:"+item.displayName);
                // Dynamically find the current mouse in the scene
                GameObject mouse = GameObject.FindGameObjectWithTag("Player");

                // if item.displayName == "Shield" {  add shieldPrefab to weaponPrefab } else if item.displayName == "Nuke" { add nukePrefab to weaponPrefab}
                if (mouse != null)
                {
                    MouseMovement mm = mouse.GetComponent<MouseMovement>();

                    if (mm != null)
                    {
                        if (item.displayName == "Shield" && !mm.weaponPrefabs.Contains(shieldPrefab))
                        {
                            mm.weaponPrefabs.Add(shieldPrefab);

                            if (!GameData.unlockedWeapons.Contains(shieldPrefab))
                                GameData.unlockedWeapons.Add(shieldPrefab);
                        }
                        else if (item.displayName == "Nuke" && !mm.weaponPrefabs.Contains(nukePrefab))
                        {
                            mm.weaponPrefabs.Add(nukePrefab);

                            if (!GameData.unlockedWeapons.Contains(nukePrefab))
                                GameData.unlockedWeapons.Add(nukePrefab);
                        }
                    }
                }

                item.buyPanel.SetActive(false);
            }

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

    void TryBuyItem(ShopItem item)
    {
        bool canBuy = false;

        switch (item.currencyType)
        {
            case CurrencyType.Gems:
                if (currentGems >= item.gemCost)
                {
                    currentGems -= item.gemCost;
                    gemCurrencyTxt.text = currentGems.ToString();
                    gemStoreCurrencyTxt.text = currentGems.ToString();
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
                    coinCurrencyTxt.text = currentCheese.ToString();
                    coinCurrencyTxt2.text = currentCheese.ToString();
                    coinStoreCurrencyTxt.text = currentCheese.ToString();
                    canBuy = true;
                }
                else
                {
                    noGemsPanel.SetActive(true); // or noCheesePanel
                }
                break;
        }

        if (canBuy)
        {
            item.isUnlocked = true;
            item.toggleUI.interactable = true;
            OnItemClicked(item, true);
            if (item.buyPanel != null)
                item.buyPanel.SetActive(false);
        }
    }


    // Call this to increase cheese
    public void AddCheese(int amount)
    {
        currentCheese += amount;
        coinCurrencyTxt.text = currentCheese.ToString();
        coinCurrencyTxt2.text = currentCheese.ToString();
        coinStoreCurrencyTxt.text = currentCheese.ToString();
        Debug.Log("Total Cheese: " + currentCheese);
    }

    public void AddGems(int amount)
    {
        currentGems += amount;
        gemCurrencyTxt.text = currentGems.ToString();
        gemStoreCurrencyTxt.text = currentGems.ToString();
        Debug.Log("Total Gems: " + currentGems);
    }

    // Call this to increase cheese
    public void DeductCheese(int amount)
    {
        currentCheese -= amount;
        coinCurrencyTxt.text = currentCheese.ToString();
        coinCurrencyTxt2.text = currentCheese.ToString();
        coinStoreCurrencyTxt.text = currentCheese.ToString();
        Debug.Log("Total Cheese: " + currentCheese);
    }

    public void DeductGems(int amount)
    {
        currentGems -= amount;
        gemCurrencyTxt.text = currentGems.ToString();
        gemStoreCurrencyTxt.text = currentGems.ToString();
        Debug.Log("Total Gems: " + currentGems);
    }

    public void DeactiveGemPanel() 
    {
        noGemsPanel.SetActive(false);
    }

    public void IAPBtn1() 
    {
        AddGems(500);
    }

    public void IAPBtn2()
    {
        AddGems(2500);
    }

    public void IAPBtn3()
    {
        AddCheese(3000);
    }

    public void IAPBtn4()
    {
        AddGems(500);
    }

    // Dummy methods for PlayFab
    void SavePurchaseToPlayFab(ShopItem item) { }
    void SaveSelectionToPlayFab(ShopItem item) { }
    void LoadPlayerDataFromPlayFab() { }
}
