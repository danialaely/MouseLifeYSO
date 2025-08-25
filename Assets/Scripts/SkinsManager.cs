using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
using System.IO;
using UnityEngine.Purchasing.Extension;

public class SkinsManager : MonoBehaviour
{

    public static SkinsManager INSTANCE { get; private protected set; }

    [SerializeField] Material PlayerMaterial;
    [SerializeField] string selectedSkinId;
    [SerializeField] List<Skin> Skins = new List<Skin>();

    private string saveFilePath;

    [System.Serializable]
    public class SkinSaveData
    {
        public string selectedSkinId;
        public List<string> purchasedSkinIds = new List<string>();
    }

    private void Awake()
    {
        if (INSTANCE != null)
        {
            Debug.Log("[SkinsManager] Duplicate instance destroyed");
            Destroy(gameObject);
            return;
        }

        INSTANCE = this;
        DontDestroyOnLoad(gameObject);
        
        saveFilePath = Path.Combine(Application.persistentDataPath, "skindata.json");
        Debug.Log($"[SkinsManager] Save file path: {saveFilePath}");
        
        LoadSkinData();
        Debug.Log($"[SkinsManager] Initialized with {Skins.Count} skins");
    }

    private Skin GetSkin(string id) => Skins.FirstOrDefault(s => s.id == id);


    public void OnIAPSkinPurchased(Product product)
    {
        Debug.Log($"[SkinsManager] IAP Purchase Successful For: {product.definition.id}");
        
        string id = product.definition.payout.data;
        Debug.Log($"[SkinsManager] Extracted skin ID from payout data: '{id}'");
        
        // Check if skin exists before attempting purchase
        Skin targetSkin = GetSkin(id);
        if (targetSkin == null)
        {
            Debug.LogError($"[SkinsManager] IAP skin with ID '{id}' not found in Skins list! Available skins:");
            foreach (Skin skin in Skins)
            {
                Debug.LogError($"[SkinsManager] Available skin: '{skin.id}' - {skin.name}");
            }
            return;
        }
        
        Debug.Log($"[SkinsManager] Found target skin: '{targetSkin.id}' - {targetSkin.name}");
        PurchaseSkin(id);
    }

    public void OnIAPSkinPurchaseFailed(Product product, PurchaseFailureDescription description)
    {
        Debug.Log($"[SkinsManager] IAP Purchase Failed For: {description.productId} With Reason: {description.message}");
    }


    public void PurchaseSkin(string id)
    {
        Debug.Log($"[SkinsManager] Attempting to purchase skin: {id}");
        
        Skin skin = GetSkin(id);
        if (skin == null) 
        {
            Debug.LogError($"[SkinsManager] Skin {id} not found");
            return;
        }
        if (skin.isPurchased == true) 
        {
            Debug.Log($"[SkinsManager] Skin {id} already purchased");
            return;
        }

        var shop = ShopManager.Instance;
        Debug.Log($"[SkinsManager] Skin {id} costs {skin.price} {skin.payment}");

        switch (skin.payment)
        {
            case Payment.Free:
                Debug.Log($"[SkinsManager] Free skin {id} unlocked");
                break;
            case Payment.Gems when shop.currentGems < skin.price:
                Debug.LogWarning($"[SkinsManager] Not enough gems. Need: {skin.price}, Have: {shop.currentGems}");
                return;
            case Payment.Cheese when shop.currentCheese < skin.price:
                Debug.LogWarning($"[SkinsManager] Not enough cheese. Need: {skin.price}, Have: {shop.currentCheese}");
                return;
            case Payment.Gems:
                shop.DeductGems(skin.price);
                Debug.Log($"[SkinsManager] Purchased {id} for {skin.price} gems");
                break;
            case Payment.Cheese:
                shop.DeductCheese(skin.price);
                Debug.Log($"[SkinsManager] Purchased {id} for {skin.price} cheese");
                break;
            case Payment.IAP:
                Debug.Log($"[SkinsManager] Purchased {id} for {skin.price} IAP");
                break;
            default:
                Debug.LogError($"[SkinsManager] Invalid payment type for skin {id}");
                return;
        }

        skin.isPurchased = true;
        Debug.Log($"[SkinsManager] Skin {id} purchase completed successfully");
        SaveSkinData();
    }

    public void SelectSkin(string id)
    {
        Debug.Log($"[SkinsManager] Attempting to select skin: {id}");
        
        Skin skin = GetSkin(id);
        if (skin == null) 
        {
            Debug.LogError($"[SkinsManager] Cannot select skin {id} - not found");
            return;
        }
        if (!skin.isPurchased) 
        {
            Debug.LogWarning($"[SkinsManager] Cannot select skin {id} - not purchased");
            return;
        }

        selectedSkinId = skin.id;
        OnSkinSelected(skin);
        Debug.Log($"[SkinsManager] Skin {id} ({skin.name}) selected and applied");
        SaveSkinData();
    }

    public void OnSkinSelected(Skin skin)
    {
        PlayerMaterial.mainTexture = skin.texture;
        Debug.Log($"[SkinsManager] Applied texture for skin: {skin.name}");
    }

    public void BuyOrSelectSkin(string id)
    {
        Debug.Log($"[SkinsManager] BuyOrSelectSkin called for: {id}");
        
        Skin skin = GetSkin(id);
        if (skin == null) 
        {
            Debug.LogError($"[SkinsManager] BuyOrSelectSkin - Skin {id} not found");
            return;
        }
        
        if (!skin.isPurchased)
        {
            Debug.Log($"[SkinsManager] Skin {id} not owned, attempting purchase");
            PurchaseSkin(skin.id);
        }
        else
        {
            Debug.Log($"[SkinsManager] Skin {id} already owned, selecting");
            SelectSkin(id);
        }
    }

    private void SaveSkinData()
    {
        try
        {
            SkinSaveData saveData = new SkinSaveData();
            saveData.selectedSkinId = selectedSkinId;
            
            foreach (Skin skin in Skins)
            {
                if (skin.isPurchased)
                {
                    saveData.purchasedSkinIds.Add(skin.id);
                }
            }

            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(saveFilePath, json);
            Debug.Log($"[SkinsManager] Skin data saved successfully. Selected: {selectedSkinId}, Purchased: {saveData.purchasedSkinIds.Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SkinsManager] Failed to save skin data: {e.Message}");
        }
    }

    private void LoadSkinData()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                string json = File.ReadAllText(saveFilePath);
                SkinSaveData saveData = JsonUtility.FromJson<SkinSaveData>(json);

                // Apply purchased skins
                foreach (string purchasedId in saveData.purchasedSkinIds)
                {
                    Skin skin = GetSkin(purchasedId);
                    if (skin != null)
                    {
                        skin.isPurchased = true;
                    }
                }

                // Apply selected skin
                if (!string.IsNullOrEmpty(saveData.selectedSkinId))
                {
                    Skin selectedSkin = GetSkin(saveData.selectedSkinId);
                    if (selectedSkin != null && selectedSkin.isPurchased)
                    {
                        selectedSkinId = saveData.selectedSkinId;
                        OnSkinSelected(selectedSkin);
                        Debug.Log($"[SkinsManager] Loaded and applied selected skin: {selectedSkinId}");
                    }
                }

                Debug.Log($"[SkinsManager] Skin data loaded successfully. Selected: {selectedSkinId}, Purchased: {saveData.purchasedSkinIds.Count}");
            }
            else
            {
                Debug.Log("[SkinsManager] No save file found, starting with default data");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SkinsManager] Failed to load skin data: {e.Message}");
        }
    }
}

[Serializable]
public enum Payment
{
    IAP,
    Cheese,
    Gems,
    Free,
}

[Serializable]
public class Skin
{
    public string id;
    public string name;
    public bool isPurchased;
    public Texture texture;
    public Payment payment;
    public int price;
}
