using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;

public class SkinsManager : MonoBehaviour
{

    public static SkinsManager INSTANCE { get; private protected set; }

    [SerializeField] Material PlayerMaterial;
    [SerializeField] string selectedSkinId;
    [SerializeField] List<Skin> Skins = new List<Skin>();

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
        Debug.Log($"[SkinsManager] Initialized with {Skins.Count} skins");
    }

    private Skin GetSkin(string id) => Skins.FirstOrDefault(s => s.id == id);

    public void PurchaseSkin(string id) => TryPurchaseSkin(id);

    public bool TryPurchaseSkin(string id)
    {
        Debug.Log($"[SkinsManager] Attempting to purchase skin: {id}");
        
        Skin skin = GetSkin(id);
        if (skin == null) 
        {
            Debug.LogError($"[SkinsManager] Skin {id} not found");
            return false;
        }
        if (skin.isPurchased == true) 
        {
            Debug.Log($"[SkinsManager] Skin {id} already purchased");
            return false;
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
                return false;
            case Payment.Cheese when shop.currentCheese < skin.price:
                Debug.LogWarning($"[SkinsManager] Not enough cheese. Need: {skin.price}, Have: {shop.currentCheese}");
                return false;
            case Payment.Gems:
                shop.DeductGems(skin.price);
                Debug.Log($"[SkinsManager] Purchased {id} for {skin.price} gems");
                break;
            case Payment.Cheese:
                shop.DeductCheese(skin.price);
                Debug.Log($"[SkinsManager] Purchased {id} for {skin.price} cheese");
                break;
            case Payment.IAP:
                Debug.LogWarning($"[SkinsManager] IAP for skin {id} not implemented");
                return false;
            default:
                Debug.LogError($"[SkinsManager] Invalid payment type for skin {id}");
                return false;
        }

        skin.isPurchased = true;
        Debug.Log($"[SkinsManager] Skin {id} purchase completed successfully");
        return true;
    }

    public void SelectSkin(string id) => TrySelectSkin(id);

    public bool TrySelectSkin(string id)
    {
        Debug.Log($"[SkinsManager] Attempting to select skin: {id}");
        
        Skin skin = GetSkin(id);
        if (skin == null) 
        {
            Debug.LogError($"[SkinsManager] Cannot select skin {id} - not found");
            return false;
        }
        if (!skin.isPurchased) 
        {
            Debug.LogWarning($"[SkinsManager] Cannot select skin {id} - not purchased");
            return false;
        }

        selectedSkinId = skin.id;
        OnSkinSelected(skin);
        Debug.Log($"[SkinsManager] Skin {id} ({skin.name}) selected and applied");

        return true;
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
            bool success = TryPurchaseSkin(skin.id);
            Debug.Log($"[SkinsManager] Purchase result for {id}: {success}");
        }
        else
        {
            Debug.Log($"[SkinsManager] Skin {id} already owned, selecting");
            bool success = TrySelectSkin(id);
            Debug.Log($"[SkinsManager] Selection result for {id}: {success}");
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
