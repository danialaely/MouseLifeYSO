using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.SceneManagement;

public class AdsInitializer : MonoBehaviour, IUnityAdsInitializationListener
{
    [SerializeField] string _androidGameId;
    [SerializeField] string _iOSGameId;
    [SerializeField] bool _testMode = true;
    private string _gameId;

    [SerializeField] RewardedAds _rewardedAds;
    [SerializeField] RewardedAds _rewardedAds2;

    public GameObject loadingPanel;

    void Awake()
    {
        // Prevent duplicates and persist this object
        if (FindObjectsOfType<AdsInitializer>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
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
            Advertisement.Initialize(_gameId, _testMode, this);
        }
        else if (Advertisement.isInitialized)
        {
            // Ads may already be initialized when switching scenes
            OnInitializationComplete();
        }
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");

        // Pre-load your rewarded ads
        _rewardedAds?.LoadAd();
        _rewardedAds2?.LoadAd();

        SceneManager.LoadScene("Level2");
        loadingPanel?.SetActive(false);
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error} - {message}");
    }
}
