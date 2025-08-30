using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
using System.IO;
using UnityEngine.Purchasing.Extension;
using UnityEngine.SceneManagement;

public class SkinsManager : MonoBehaviour
{

    public static SkinsManager INSTANCE { get; private protected set; }

    [SerializeField] Material PlayerMaterial;
    [SerializeField] public string selectedSkinId;
    [SerializeField] List<Skin> Skins = new List<Skin>();
    [SerializeField] List<SkinButton> SkinButtons = new List<SkinButton>();

    public Sprite cashIcon;
    public Sprite cheeseIcon;
    public Sprite gemsIcon;

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
        
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        LoadSkinData();
        Debug.Log($"[SkinsManager] Initialized with {Skins.Count} skins");
    }

    public Skin GetSkin(string id) => Skins.FirstOrDefault(s => s.id == id);

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[SkinsManager] Scene loaded: {scene.name}, updating extraObjects references");
        StartCoroutine(UpdateExtraObjectReferencesDelayed());
    }


    private IEnumerator UpdateExtraObjectReferencesDelayed()
    {
        yield return new WaitForEndOfFrame();

        Scene targetScene = SceneManager.GetActiveScene();
        UpdateExtraObjectReferences(targetScene);

        if (!string.IsNullOrEmpty(selectedSkinId))
        {
            Skin selectedSkin = GetSkin(selectedSkinId);
            if (selectedSkin != null && selectedSkin.isPurchased)
            {
                OnSkinSelected(selectedSkin);
            }
        }
    }


    private void UpdateExtraObjectReferences(Scene targetScene)
    {
        Debug.Log($"[SkinsManager] Updating extraObject references for {Skins.Count} skins in scene: {targetScene.name}");
        
        foreach (Skin skin in Skins)
        {
            if (skin.extraObjectNames != null && skin.extraObjectNames.Count > 0)
            {
                if (skin.extraObjects == null)
                {
                    skin.extraObjects = new List<GameObject>();
                }
                else
                {
                    skin.extraObjects.Clear();
                }

                Debug.Log($"[SkinsManager] Searching for {skin.extraObjectNames.Count} objects for skin {skin.id}");
                
                foreach (string objectName in skin.extraObjectNames)
                {
                    GameObject foundObj = FindGameObjectInScene(targetScene, objectName);
                    
                    if (foundObj != null)
                    {
                        skin.extraObjects.Add(foundObj);
                        Debug.Log($"[SkinsManager] Found and added {objectName} to skin {skin.id} (scene: {foundObj.scene.name})");
                    }
                    else
                    {
                        Debug.LogWarning($"[SkinsManager] Could not find GameObject with name: {objectName} for skin {skin.id} in scene {targetScene.name}");
                    }
                }
                
                Debug.Log($"[SkinsManager] Skin {skin.id} now has {skin.extraObjects.Count} extraObjects");
            }
        }
    }

    private GameObject FindGameObjectInScene(Scene scene, string objectName)
    {
        GameObject[] rootObjects = scene.GetRootGameObjects();
        
        foreach (GameObject rootObj in rootObjects)
        {
            if (rootObj.name == objectName)
            {
                return rootObj;
            }
            
            GameObject foundChild = FindGameObjectInChildren(rootObj.transform, objectName);
            if (foundChild != null)
            {
                return foundChild;
            }
        }
        
        return null;
    }

    private GameObject FindGameObjectInChildren(Transform parent, string objectName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == objectName)
            {
                return child.gameObject;
            }
            
            GameObject foundInChild = FindGameObjectInChildren(child, objectName);
            if (foundInChild != null)
            {
                return foundInChild;
            }
        }
        
        return null;
    }


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


        string id = product.definition.payout.data;
        Skin targetSkin = GetSkin(id);
        PurchaseStatusPanel.INSTANCE.Show(targetSkin.icon, targetSkin.name, false);
    }


    public void PurchaseSkin(string id)
    {
        Debug.Log($"[SkinsManager] Attempting to purchase skin: {id}");
        
        Skin skin = GetSkin(id);
        if (skin == null) 
        {
            Debug.LogError($"[SkinsManager] Skin {id} not found");
            PurchaseStatusPanel.INSTANCE.Show(skin.icon, skin.name, false);
            return;
        }
        if (skin.isPurchased == true) 
        {
            Debug.Log($"[SkinsManager] Skin {id} already purchased");
            PurchaseStatusPanel.INSTANCE.Show(skin.icon, skin.name, false);
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
                PurchaseStatusPanel.INSTANCE.Show(skin.icon, skin.name, false);
                return;
            case Payment.Cheese when shop.currentCheese < skin.price:
                Debug.LogWarning($"[SkinsManager] Not enough cheese. Need: {skin.price}, Have: {shop.currentCheese}");
                PurchaseStatusPanel.INSTANCE.Show(skin.icon, skin.name, false);
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
                PurchaseStatusPanel.INSTANCE.Show(skin.icon, skin.name, false);
                return;
        }

        skin.isPurchased = true;
        Debug.Log($"[SkinsManager] Skin {id} purchase completed successfully");
        PurchaseStatusPanel.INSTANCE.Show(skin.icon, skin.name, true);
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

    private void ManageExtraObjects(Skin selectedSkin)
    {
        foreach (Skin skin in Skins)
        {
            DisableExtraObjectsForSkin(skin);
        }

        EnableExtraObjectsForSkin(selectedSkin);
        Debug.Log($"[SkinsManager] Extra objects managed for skin: {selectedSkin.name}");
    }

    private void DisableExtraObjectsForSkin(Skin skin)
    {
        if (skin.extraObjects != null)
        {
            foreach (GameObject extraObj in skin.extraObjects)
            {
                if (extraObj != null)
                {
                    extraObj.SetActive(false);
                }
            }
        }
    }

    private void EnableExtraObjectsForSkin(Skin skin)
    {
        if (skin.extraObjects != null)
        {
            foreach (GameObject extraObj in skin.extraObjects)
            {
                if (extraObj != null)
                {
                    extraObj.SetActive(true);
                }
            }
        }
    }

    public void OnSkinSelected(Skin skin)
    {
        PlayerMaterial.mainTexture = skin.texture;
        ManageExtraObjects(skin);
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

            // Delay the button setup to avoid collection modification issues during IAP processing
            StartCoroutine(DelayedButtonSetup());

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

                // Delay the button setup to avoid collection modification issues during IAP processing
                StartCoroutine(DelayedButtonSetup());

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

    private IEnumerator DelayedButtonSetup()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        SkinButtons.ForEach((s) => s.SetUp());
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
    public Sprite icon;
    public Payment payment;
    public int price;
    public string iapID;
    public List<GameObject> extraObjects;
    public List<string> extraObjectNames;
}
