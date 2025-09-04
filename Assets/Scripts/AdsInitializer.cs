using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.SceneManagement;

public class AdsInitializer : MonoBehaviour, IUnityAdsInitializationListener
{

    public static AdsInitializer INSTANCE {  get; private protected set; }

    [SerializeField] string _androidGameId = "1401815";
    [SerializeField] string _iOSGameId = "1401814";
    public bool _testMode = true;
    private string _gameId;
    [SerializeField] RewardedAds _rewardedAds;
    [SerializeField] RewardedAds _rewardedAds2;
    public GameObject loadingPanel;

    void Awake()
    {
        if (INSTANCE != null)
        {
            Debug.Log("[AdsInitializer] Duplicate instance destroyed");
            Destroy(gameObject);
            return;
        }

        INSTANCE = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (_testMode)
        {
            _androidGameId = "14851";
         _iOSGameId = "14850";
        }
        InitializeAds();
    }

    public void InitializeAds()
    {
#if UNITY_IOS
        _gameId = _iOSGameId;
#elif UNITY_ANDROID
        _gameId = _androidGameId;
#elif UNITY_EDITOR
        _gameId = _androidGameId; // For testing in Editor
#endif
        if (!Advertisement.isInitialized && Advertisement.isSupported)
        {
            Debug.Log($"[AdsInitializer] Initializing Unity Ads with Game ID: {_gameId}, Test Mode: {_testMode}");
            Advertisement.Initialize(_gameId, _testMode, this);
        }
        else if (Advertisement.isInitialized)
        {
            // Ads may already be initialized when switching scenes
            Debug.Log("[AdsInitializer] Unity Ads already initialized.");
            OnInitializationComplete();
        }
        else
        {
            Debug.LogWarning("[AdsInitializer] Unity Ads not supported or already in progress.");
            // Proceed to load scene anyway to avoid app freeze
            loadingPanel.SetActive(false);
            //SceneManager.LoadScene("Level2");
        }
    }

    public void OnInitializationComplete()
    {
        Debug.Log("[AdsInitializer] Unity Ads initialization complete.");
        // Pre-load your rewarded ads
        _rewardedAds?.LoadAd();
        _rewardedAds2?.LoadAd();
        //loadingPanel.SetActive(false);
        //SceneManager.LoadScene("Level2");
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogError($"[AdsInitializer] Unity Ads Initialization Failed: {error} - {message}. Proceeding without ads.");
        // Proceed to load scene to prevent app from being stuck
        //loadingPanel.SetActive(false);
        //SceneManager.LoadScene("Level2");
    }
}