using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SkinButton : MonoBehaviour
{
    [Header("Data")]
    public string skinId;

    // Cached refs (found via hard-coded child indices like your original)
    private GameObject selectedIndicator;
    private GameObject priceParent;
    private Image priceIcon;
    private TMP_Text priceText;

    private Button button;
    private CodelessIAPButton iapButton;

    // Managers & data
    private SkinsManager skinManager;
    private ShopManager shopManager;
    private Skin skin;

    // Child index constants — update these if prefab layout changes
    private const int SELECTED_PARENT_INDEX = 0;      // transform.GetChild(0).GetChild(0)
    private const int PRICE_PARENT_INDEX = 2;         // transform.GetChild(2)
    private const int PRICE_ICON_INDEX = 0;           // priceParent.transform.GetChild(0)
    private const int PRICE_TEXT_INDEX = 1;           // priceParent.transform.GetChild(1)

    #region Unity lifecycle & init

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        Init();
    }

    // optional public initializer
    public void Init()
    {
        CacheReferences();
        SetUp();
    }

    private void CacheReferences()
    {
        // Managers (cache once)
        if (skinManager == null) skinManager = SkinsManager.INSTANCE;
        if (shopManager == null) shopManager = ShopManager.Instance;

        // Components (cache)
        if (button == null) button = GetComponent<Button>();
        if (iapButton == null) TryGetComponent(out iapButton);

        // Hard-coded lookups (fall back safely if hierarchy differs)
        if (selectedIndicator == null)
        {
            var parent0 = TryGetChild(transform, SELECTED_PARENT_INDEX);
            selectedIndicator = parent0 != null && parent0.childCount > 0 ? parent0.GetChild(0).gameObject : null;
        }

        if (priceParent == null)
        {
            var p = TryGetChild(transform, PRICE_PARENT_INDEX);
            priceParent = p != null ? p.gameObject : null;
        }

        if (priceParent != null)
        {
            var pTrans = priceParent.transform;
            if (priceIcon == null)
            {
                var iconTrans = TryGetChild(pTrans, PRICE_ICON_INDEX);
                if (iconTrans != null) priceIcon = iconTrans.GetComponent<Image>();
            }

            if (priceText == null)
            {
                var textTrans = TryGetChild(pTrans, PRICE_TEXT_INDEX);
                if (textTrans != null) priceText = textTrans.GetComponent<TMP_Text>();
            }
        }
    }

    private static Transform TryGetChild(Transform parent, int index)
    {
        return (parent != null && parent.childCount > index) ? parent.GetChild(index) : null;
    }

    private void OnDestroy()
    {
        ResetAll();
    }

    #endregion

    #region Setup / Reset (idempotent)

    // Safe to call many times; each call fully resets then re-applies desired state.
    public void SetUp()
    {
        CacheReferences();
        ResetAll(); // deterministic baseline

        // Get skin data
        if (skinManager == null)
        {
            Debug.LogError($"SkinButton: SkinsManager not found on '{name}'.");
            return;
        }

        skin = skinManager.GetSkin(skinId);
        if (skin == null)
        {
            Debug.LogError($"SkinButton: invalid skinId '{skinId}' on '{name}'");
            return;
        }

        // Always update selected indicator
        bool isSelected = skinManager.selectedSkinId == skin.id;
        if (selectedIndicator != null) selectedIndicator.SetActive(isSelected);

        if (isSelected) return;

        if (skin.isPurchased)
        {
            ConfigureSelect();
            return;
        }

        // Choose behaviour based on payment type
        switch (skin.payment)
        {
            case Payment.IAP:
                ConfigureIAP(skin);
                break;

            case Payment.Gems:
                ConfigureCurrency(skin, Currency.Gems);
                break;

            case Payment.Cheese:
                ConfigureCurrency(skin, Currency.Cheese);
                break;

            case Payment.Free:
                ConfigureFree(skin);
                break;

            default:
                Debug.LogError($"SkinButton: unsupported payment type '{skin.payment}' for skin '{skinId}'");
                break;
        }
    }

    // Reset to known clean state so SetUp is idempotent.
    public void ResetAll()
    {
        // IAP cleanup
        if (iapButton != null)
        {
            iapButton.onPurchaseComplete.RemoveAllListeners();
            iapButton.onPurchaseFailed.RemoveAllListeners();
            iapButton.button = null;
            iapButton.productId = null;
            iapButton.enabled = false;
        }

        // Button cleanup
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.interactable = false;
        }

        // Price UI baseline
        if (priceParent != null) priceParent.SetActive(false);
        if (priceText != null) priceText.text = string.Empty;
        if (priceIcon != null) priceIcon.sprite = null;
    }

    #endregion

    #region Configuration helpers (icons pulled from SkinsManager)

    private void ConfigureSelect()
    {
        if (button != null)
        {
            button.interactable = true;
            button.onClick.AddListener(() => skinManager.SelectSkin(skin.id));
        }
        if (priceParent != null) priceParent.SetActive(false);
    }

    private void ConfigureIAP(Skin s)
    {
        // Resolve icons from SkinManager
        var (cashIcon, _, _) = GetIconsFromManager();

        if (iapButton == null)
        {
            ConfigurePrice($"{s.price}$", cashIcon);
            if (button != null) button.interactable = false;
            Debug.LogWarning($"SkinButton: IAP requested for '{s.id}' but CodelessIAPButton missing on '{name}'.");
            return;
        }

        // clear then attach
        iapButton.onPurchaseComplete.RemoveAllListeners();
        iapButton.onPurchaseFailed.RemoveAllListeners();

        iapButton.enabled = true;
        iapButton.productId = s.iapID;
        iapButton.button = button;

        iapButton.onPurchaseComplete.AddListener((p) => skinManager.OnIAPSkinPurchased(p));
        iapButton.onPurchaseFailed.AddListener((p, d) => skinManager.OnIAPSkinPurchaseFailed(p, d));

        ConfigurePrice($"{s.price}$", cashIcon);

        if (button != null) button.interactable = true;
    }

    private void ConfigureCurrency(Skin s, Currency currency)
    {
        var (_, cheeseIcon, gemsIcon) = GetIconsFromManager();

        bool canAfford = false;
        Sprite iconToUse = currency == Currency.Gems ? gemsIcon : cheeseIcon;

        if (shopManager != null)
        {
            canAfford = currency == Currency.Gems ? (shopManager.currentGems >= s.price) : (shopManager.currentCheese >= s.price);
        }

        if (!canAfford)
        {
            if (button != null) button.interactable = false;
            ConfigurePrice(s.price.ToString(), iconToUse);
            return;
        }

        // can afford -> enable purchase
        if (button != null)
        {
            button.interactable = true;
            button.onClick.AddListener(() => skinManager.PurchaseSkin(s.id));
        }
        ConfigurePrice(s.price.ToString(), iconToUse);
    }

    private void ConfigureFree(Skin s)
    {
        var (cashIcon, _, _) = GetIconsFromManager();
        if (button != null)
        {
            button.interactable = true;
            button.onClick.AddListener(() => skinManager.PurchaseSkin(s.id));
        }
        ConfigurePrice("FREE", cashIcon);
    }

    private void ConfigurePrice(string text, Sprite sprite)
    {
        if (priceParent != null) priceParent.SetActive(true);
        if (priceText != null) priceText.text = text;
        if (priceIcon != null) priceIcon.sprite = sprite;
    }

    // Attempt to read icons from the SkinsManager instance.
    // Assumes SkinsManager exposes public Sprite fields/properties named exactly: cashIcon, cheeseIcon, gemsIcon.
    // Returns a tuple (cash, cheese, gems) where items may be null if not found.
    private (Sprite cash, Sprite cheese, Sprite gems) GetIconsFromManager()
    {
        if (skinManager == null)
            return (null, null, null);

        // Try direct public fields/properties (common naming). If your SkinsManager uses different names,
        // change these accesses accordingly.
        Sprite cash = null, cheese = null, gems = null;

        // Attempt to access public fields/properties (compile-time access; adjust names if needed)
        // NOTE: the code below expects these members to exist on SkinsManager:
        //   public Sprite cashIcon;
        //   public Sprite cheeseIcon;
        //   public Sprite gemsIcon;
        // If your SkinManager uses different names, edit them here.
        try
        {
            // direct field/property access
            cash = skinManager.cashIcon;
            cheese = skinManager.cheeseIcon;
            gems = skinManager.gemsIcon;
        }
        catch (System.Exception)
        {
            // If fields don't exist, warn once (but don't throw).
            // You can safely ignore this if your SkinsManager already exposes those fields.
            Debug.LogWarning("SkinButton: Could not read icons from SkinsManager. Make sure it exposes public Sprite fields: cashIcon, cheeseIcon, gemsIcon.");
        }

        return (cash, cheese, gems);
    }

    #endregion

    private enum Currency { Gems, Cheese }
}
