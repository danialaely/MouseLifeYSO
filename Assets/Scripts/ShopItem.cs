using UnityEngine;
using UnityEngine.UI;

public enum CurrencyType
{
    Gems,
    Cheese
}


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

    public Toggle toggleUI; // for skins
    public Button buttonUI; // for weapons

    public GameObject buyPanel; // Assigned from the inspector
}