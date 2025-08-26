using System;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class InterstitialAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [Header("Ad Unit IDs (set per-platform)")]
    [SerializeField] string _androidAdUnitId = "Interstitial_Android";
    [SerializeField] string _iOSAdUnitId = "Interstitial_iOS";

    [Header("Behaviour")]
    [SerializeField] bool _autoLoadOnStart = true;
    [SerializeField] float _loadTimeoutSeconds = 10f;
    [SerializeField] bool _showOnLoadIfRequested = false; // set true if you want ShowAd() to auto-show when loaded
    [SerializeField] bool _testMode = true; // toggle for testing

    [Header("Events")]
    public UnityEvent OnAdLoaded;
    public UnityEvent OnAdFailedToLoad;
    public UnityEvent OnAdShown;
    public UnityEvent OnAdFailedToShow;
    public UnityEvent OnAdClosed; // called after show completes or is skipped

    // singleton for convenience
    public static InterstitialAds Instance { get; private set; }

    string AdUnitId => Application.platform == RuntimePlatform.IPhonePlayer ? _iOSAdUnitId : _androidAdUnitId;

    bool _isLoaded;
    bool _isLoading;
    float _loadStartTime;
    Action _onClosedCallback; // optional callback caller can provide to resume gameplay

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Initialize Unity Ads if not already
        if (!Advertisement.isInitialized)
        {
            // Unity Ads auto-initializes in many setups. If you need to pass a gameId here,
            // prefer to initialize in a separate bootstrapper with your gameId and proper testMode.
            Debug.Log($"[InterstitialAds] Advertisement not initialized. You should call Advertisement.Initialize(gameId, testMode) elsewhere with your real game id.");
        }

        if (_autoLoadOnStart) LoadAd();
    }

    void Update()
    {
        // simple timeout watchdog for load attempts
        if (_isLoading && Time.unscaledTime - _loadStartTime > _loadTimeoutSeconds)
        {
            Debug.LogWarning("[InterstitialAds] Load timed out.");
            _isLoading = false;
            _isLoaded = false;
            OnUnityAdsFailedToLoad(AdUnitId, UnityAdsLoadError.NO_FILL, "Load timeout");
        }
    }

    /// <summary>
    /// Public: start loading an interstitial ad.
    /// </summary>
    public void LoadAd()
    {
        if (_isLoaded || _isLoading) return;

        _isLoading = true;
        _loadStartTime = Time.unscaledTime;
        Debug.Log($"[InterstitialAds] Loading Ad Unit: {AdUnitId}");
        Advertisement.Load(AdUnitId, this);
    }

    /// <summary>
    /// Public: show ad. Optional callback invoked after ad closed (or skipped/failure).
    /// If ad isn't ready, it will attempt to load (and optionally show on load if configured).
    /// </summary>
    public void ShowAd(Action onClosed = null)
    {
        _onClosedCallback = onClosed;

        if (_isLoaded)
        {
            Debug.Log("[InterstitialAds] Showing ad.");
            Advertisement.Show(AdUnitId, this);
        }
        else
        {
            Debug.LogWarning("[InterstitialAds] Ad not ready. Attempting to load.");
            if (!_isLoading) LoadAd();

            if (_showOnLoadIfRequested)
            {
                // When loaded, OnAdLoaded will call ShowAd again because _isLoaded will be true then.
                Debug.Log("[InterstitialAds] Will show automatically when loaded.");
            }
            else
            {
                // if caller expects immediate show flow, you can still choose to wait or call back
                OnAdFailedToShow?.Invoke(); // let the game know ad couldn't be shown immediately
                _onClosedCallback?.Invoke();
            }
        }
    }

    #region IUnityAdsLoadListener
    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        if (!adUnitId.Equals(AdUnitId)) return;

        _isLoading = false;
        _isLoaded = true;
        Debug.Log("[InterstitialAds] ✅ Ad loaded: " + adUnitId);
        OnAdLoaded?.Invoke();

        if (_showOnLoadIfRequested)
        {
            ShowAd(_onClosedCallback);
        }
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        if (!adUnitId.Equals(AdUnitId)) return;

        _isLoading = false;
        _isLoaded = false;
        Debug.LogError($"[InterstitialAds] ❌ Failed to load Ad Unit {adUnitId}: {error} - {message}");
        OnAdFailedToLoad?.Invoke();

        // auto-retry with exponential/backoff could be placed here
        // for simplicity, schedule a reload after a short delay
        Invoke(nameof(LoadAd), 5f);
    }
    #endregion

    #region IUnityAdsShowListener
    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        if (!adUnitId.Equals(AdUnitId)) return;

        Debug.LogError($"[InterstitialAds] ❌ Failed to show Ad {adUnitId}: {error} - {message}");
        OnAdFailedToShow?.Invoke();

        // ensure we attempt to reload for next time
        _isLoaded = false;
        _isLoading = false;
        Invoke(nameof(LoadAd), 2f);

        // call closure so gameplay can continue
        _onClosedCallback?.Invoke();
        _onClosedCallback = null;
    }

    public void OnUnityAdsShowStart(string adUnitId)
    {
        if (!adUnitId.Equals(AdUnitId)) return;
        Debug.Log("[InterstitialAds] Ad show started.");
        OnAdShown?.Invoke();
    }

    public void OnUnityAdsShowClick(string adUnitId)
    {
        // optional: track clicks, analytics, etc.
        if (!adUnitId.Equals(AdUnitId)) return;
        Debug.Log("[InterstitialAds] Ad clicked.");
    }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (!adUnitId.Equals(AdUnitId)) return;

        // For interstitials we do NOT give rewards. Treat COMPLETED or SKIPPED as closed.
        Debug.Log($"[InterstitialAds] Ad show complete: {showCompletionState}");

        // reset and prepare next ad
        _isLoaded = false;
        _isLoading = false;
        Invoke(nameof(LoadAd), 1f); // preload next ad quickly

        // event & callback to resume game/flow
        OnAdClosed?.Invoke();
        _onClosedCallback?.Invoke();
        _onClosedCallback = null;
    }
    #endregion

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
