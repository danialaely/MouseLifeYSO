using System.Collections.Generic;
using TMPro;
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
                            Debug.Log("Shield prefab added to weaponPrefabs list.");
                        }
                        else if (item.displayName == "Nuke" && !mm.weaponPrefabs.Contains(nukePrefab))
                        {
                            mm.weaponPrefabs.Add(nukePrefab);
                            Debug.Log("Nuke prefab added to weaponPrefabs list.");
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

    // Call this to increase cheese
    public void AddCheese(int amount)
    {
        currentCheese += amount;
        coinCurrencyTxt.text = currentCheese.ToString();
        coinCurrencyTxt2.text = currentCheese.ToString();
        coinStoreCurrencyTxt.text = currentCheese.ToString();
        Debug.Log("Total Cheese: " + currentCheese);
    }

    public void AddGem(int amount)
    {
        currentGems += amount;
        gemCurrencyTxt.text = currentGems.ToString();
        gemStoreCurrencyTxt.text = currentGems.ToString();
        Debug.Log("Total Cheese: " + currentGems);
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
