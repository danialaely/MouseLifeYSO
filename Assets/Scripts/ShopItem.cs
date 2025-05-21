// File: ShopItem.cs
using UnityEngine;
using UnityEngine.UI;

public enum CurrencyType { Gems, Cheese }

[System.Serializable]
public class ShopItem
{
    public string itemId;
    public string displayName;
    public bool isUnlocked;
    public bool isSelected;
    public int unlockLevel;
    public int gemCost;
    public int cheeseCost;
    public CurrencyType currencyType;

    public Button itemButton; // reference to UI button (or Toggle)
}
