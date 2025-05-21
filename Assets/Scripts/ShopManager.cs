using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


public class ShopManager : MonoBehaviour
{
    public List<ShopItem> page1Items; // Skins (Toggles)
    public List<ShopItem> page2Items; // Weapons (Buttons)

    public Button gemBuyButton;
    public Button cheeseBuyButton;

    public int playerLevel;
    public int currentGems;
    public int currentCheese;

    private ShopItem selectedSkin;

    void Start()
    {
        LoadPlayerDataFromPlayFab();
        RefreshShopUI();
    }

    public void RefreshShopUI()
    {
        // Skins (Page1)
        foreach (ShopItem item in page1Items)
        {
            bool unlockedByLevel = playerLevel >= item.unlockLevel;

            if (item.isUnlocked)
            {
                EnableSkinSelection(item);
            }
            else if (unlockedByLevel && item.gemCost > 0)
            {
                item.itemButton.interactable = true;
                EnableBuyButtonsIfNeeded(item);
            }
            else
            {
                item.itemButton.interactable = false;
            }
        }

        // Weapons (Page2)
        foreach (ShopItem item in page2Items)
        {
            bool unlocked = item.isUnlocked;

            item.itemButton.interactable = unlocked || (item.gemCost > 0 || item.cheeseCost > 0);
            EnableBuyButtonsIfNeeded(item);
        }
    }

    void EnableBuyButtonsIfNeeded(ShopItem item)
    {
        if (!item.isUnlocked)
        {
            if (item.gemCost > 0)
                gemBuyButton.interactable = true;
            if (item.cheeseCost > 0)
                cheeseBuyButton.interactable = true;
        }
    }

    void EnableSkinSelection(ShopItem item)
    {
        item.itemButton.interactable = true;

        Toggle toggle = item.itemButton.GetComponent<Toggle>();
        toggle.isOn = item.isSelected;
        toggle.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
                SelectSkin(item);
        });
    }

    void SelectSkin(ShopItem selected)
    {
        foreach (ShopItem item in page1Items)
        {
            if (item != selected && item.isUnlocked)
            {
                item.isSelected = false;
                item.itemButton.GetComponent<Toggle>().isOn = false;
            }
        }

        selected.isSelected = true;
        selectedSkin = selected;
        SaveSelectionToPlayFab(selected);
    }

    public void BuyWithGems()
    {
        TryBuy(item => item.gemCost > 0 && currentGems >= item.gemCost, CurrencyType.Gems);
    }

    public void BuyWithCheese()
    {
        TryBuy(item => item.cheeseCost > 0 && currentCheese >= item.cheeseCost, CurrencyType.Cheese);
    }

    void TryBuy(Func<ShopItem, bool> condition, CurrencyType type)
    {
        List<ShopItem> allItems = page1Items.Concat(page2Items).ToList();

        foreach (ShopItem item in allItems)
        {
            if (!item.isUnlocked && condition(item))
            {
                item.isUnlocked = true;
                DeductCurrency(item, type);
                RefreshShopUI();
                SavePurchaseToPlayFab(item);
                break;
            }
        }
    }

    void DeductCurrency(ShopItem item, CurrencyType type)
    {
        if (type == CurrencyType.Gems)
            currentGems -= item.gemCost;
        else
            currentCheese -= item.cheeseCost;
    }

    // These methods will interact with PlayFab
    void SavePurchaseToPlayFab(ShopItem item) { /* save item unlocked */ }
    void SaveSelectionToPlayFab(ShopItem item) { /* save selected skin */ }
    void LoadPlayerDataFromPlayFab() { /* fetch unlocked items, gems, cheese, level */ }
}

