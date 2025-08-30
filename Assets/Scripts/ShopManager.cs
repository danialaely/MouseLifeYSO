using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing;

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

    public List<TMP_Text> cheeseTextObjects = new List<TMP_Text>();
    public List<TMP_Text> gemTextObjects = new List<TMP_Text>();

    public GameObject shieldPrefab;
    public GameObject nukePrefab;

    public Sprite cheeseIcon; 
    public Sprite gemsIcon; 

    private string currencySaveFilePath;

    [System.Serializable]
    public class CurrencyData
    {
        public int gems;
        public int cheese;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            currencySaveFilePath = Path.Combine(Application.persistentDataPath, "currencydata.json");
        }
        else
        {
            Destroy(gameObject); // Avoid duplicates on scene load
        }
        PlayerPrefs.SetInt("CurrentLevel", 2);

    }

    void Start()
    {
       // LoadPlayerDataFromPlayFab();
        LoadCurrencyData();
        SetupShopItems();
        setLevel(1);
        UpdateAllCurrencyUI();
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
                    UpdateGemUI();
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
                    UpdateCheeseUI();
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
                    UpdateGemUI();
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
                    UpdateCheeseUI();
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
        UpdateAllCurrencyUI();
        SaveCurrencyData();
        Debug.Log("Total Cheese: " + currentCheese);
    }

    public void AddGem(int amount)
    {
        currentGems += amount;
        UpdateAllCurrencyUI();
        SaveCurrencyData();
        Debug.Log("Total Gems: " + currentGems);
    }

    public void DeductCheese(int amount)
    {
        currentCheese -= amount;
        UpdateAllCurrencyUI();
        SaveCurrencyData();
        Debug.Log("Total Cheese: " + currentCheese);
    }

    public void DeductGems(int amount)
    {
        currentGems -= amount;
        UpdateAllCurrencyUI();
        SaveCurrencyData();
        Debug.Log("Total Gems: " + currentGems);
    }

    public void DeactiveGemPanel() 
    {
        noGemsPanel.SetActive(false);
    }

    public void IAPBtn1() 
    {
        AddGem(500);
    }

    public void IAPBtn2()
    {
        AddGem(2500);
    }

    public void IAPBtn3()
    {
        AddCheese(3000);
    }

    public void IAPBtn4()
    {
        AddGem(500);
    }

    // Dummy methods for PlayFab
    void SavePurchaseToPlayFab(ShopItem item) { }
    void SaveSelectionToPlayFab(ShopItem item) { }
    void LoadPlayerDataFromPlayFab() { }

    private void UpdateAllCurrencyUI()
    {
        UpdateCheeseUI();
        UpdateGemUI();
    }

    private void UpdateCheeseUI()
    {
        foreach (TMP_Text cheeseText in cheeseTextObjects)
        {
            if (cheeseText != null)
                cheeseText.text = currentCheese.ToString();
        }
    }

    private void UpdateGemUI()
    {
        foreach (TMP_Text gemText in gemTextObjects)
        {
            if (gemText != null)
                gemText.text = currentGems.ToString();
        }
    }

    private void SaveCurrencyData()
    {
        try
        {
            CurrencyData currencyData = new CurrencyData
            {
                gems = currentGems,
                cheese = currentCheese
            };

            string json = JsonUtility.ToJson(currencyData, true);
            File.WriteAllText(currencySaveFilePath, json);
            Debug.Log($"[ShopManager] Currency saved - Gems: {currentGems}, Cheese: {currentCheese}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ShopManager] Failed to save currency data: {e.Message}");
        }
    }

    private void LoadCurrencyData()
    {
        try
        {
            if (File.Exists(currencySaveFilePath))
            {
                string json = File.ReadAllText(currencySaveFilePath);
                CurrencyData currencyData = JsonUtility.FromJson<CurrencyData>(json);

                currentGems = currencyData.gems;
                currentCheese = currencyData.cheese;

                Debug.Log($"[ShopManager] Currency loaded - Gems: {currentGems}, Cheese: {currentCheese}");
            }
            else
            {
                Debug.Log("[ShopManager] No currency save file found, starting with default values");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ShopManager] Failed to load currency data: {e.Message}");
        }
    }

    public void OnCurrencyIAPSuccessful(Product product)
    {

        bool cheeseOrGems = false;
        foreach (var p in product.definition.payouts)
        {
            if (p.subtype == "cheese")
            {
                AddCheese((int)p.quantity);
                cheeseOrGems = false;
            }
            else if (p.subtype == "gems")
            {
                AddGem((int)p.quantity);
                cheeseOrGems = true;
            }
        }

        PurchaseStatusPanel.INSTANCE.Show(cheeseOrGems == true ? gemsIcon : cheeseIcon, product.metadata.localizedTitle, true);
    }

    public void OnCurrencyIAPFailed(Product product, PurchaseFailureDescription description)
    {
        PurchaseStatusPanel.INSTANCE.Show(product.definition.payout.subtype == "gems" ? gemsIcon : cheeseIcon, product.metadata.localizedTitle, false);
    }

    public void BuyCheeseFromGems(int cheese)
    {
        if (currentGems < cheese)
        {
            PurchaseStatusPanel.INSTANCE.Show(cheeseIcon, cheese.ToString() + " Cheese", false);
            return;
        }

        if (cheese == 250)
        {
            DeductGems(50);
            AddCheese(cheese);

            PurchaseStatusPanel.INSTANCE.Show(cheeseIcon, "250 Cheese", true);
        }
    }
}
