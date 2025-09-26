using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class RewardedAds : MonoBehaviour, IUnityAdsShowListener, IUnityAdsLoadListener
{
    [Header("Ad Unit Configuration")]
    [SerializeField] string _androidProdId = "Rewarded_Android";
    [SerializeField] string _iOSProdId = "Rewarded_iOS";
    private const string TEST_REWARDED = "rewardedVideo";
    [Header("Settings")]
    [SerializeField] bool _testMode = true;

    //private ParticleSystem speedBoostPE;
    public GameObject speedBoost;

    string AdUnitId; /*=> Application.platform == RuntimePlatform.IPhonePlayer ? _iOSAdUnitId : _androidAdUnitId;*/
    private bool isAdReady = false;

    public bool OnAdLoaded;

    private void Awake()
    {

    }

    void Start()
    {
#if UNITY_IOS
        AdUnitId = AdsInitializer.INSTANCE._testMode ? TEST_REWARDED : _iOSProdId;
#elif UNITY_ANDROID
        AdUnitId = AdsInitializer.INSTANCE._testMode ? TEST_REWARDED : _androidProdId;
#elif UNITY_EDITOR
        // Use Android path for Editor
       AdUnitId = AdsInitializer.INSTANCE._testMode ? TEST_REWARDED : _androidProdId;
#endif
        LoadAd();
    }


    public void ShowOrLoadAd() => StartCoroutine(IEShowOrLoadAd());


    public IEnumerator IEShowOrLoadAd()
    {

        if (!isAdReady)
        {
            LoadingPanel.INSTANCE.Show();

            OnAdLoaded = false;
            LoadAd();
            yield return new WaitUntil(() => OnAdLoaded == true);   

            if (!isAdReady)
            {
                Debug.LogError("Ad Unable To Load!");
                LoadingPanel.INSTANCE.Hide();
            }
            else
            {
                ShowAd();
                LoadingPanel.INSTANCE.Hide();
            }
        }
        else
        {
            yield return StartCoroutine(LoadingPanel.INSTANCE.ShowLoading(2f));
            ShowAd();
        }

        
    }


    public void LoadAd()
    {
        Debug.Log($"[RewardedAds] Loading ad: {AdUnitId}, Test Mode: {_testMode}, Platform: {Application.platform}");
        Advertisement.Load(AdUnitId, this);
    }

    public void ShowAd()
    {
        Debug.Log("ShowAd() called");

        if (isAdReady)
        {
            Advertisement.Show(AdUnitId, this);
            Debug.Log("Showing Ad");
            GetComponent<Button>().interactable = false;
        }
        else
        {
            Debug.LogWarning("Ad not ready yet");
        }
    }

    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        if (adUnitId.Equals(AdUnitId))
        {
            isAdReady = true;
            Debug.Log("✅ Ad is ready");
        }
        else
        {
            isAdReady = false;
        }

        OnAdLoaded = true;
    }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (adUnitId.Equals(AdUnitId) && showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            Debug.Log("🎁 Ad Completed Successfully — Give Reward Here");
            if (gameObject.name == "rewardBtn")
            {
                var mouse = GameObject.FindGameObjectWithTag("Player");
                if (mouse != null)
                {
                    mouse.GetComponent<MouseMovement>().SpeedMove = 7.5f;
                    Debug.Log("✅ Speed Boost Applied");
                    Instantiate(speedBoost);
                    //Instantiate(cageconvertPE, other.transform.position, Quaternion.identity);
                }
            }
            else if (gameObject.name == "rewardBtn2")
            {
                ShopManager.Instance.AddGem(50);
                Debug.Log("✅ 50 Gems Added");
            }
        }
        LoadAd();
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"❌ Failed to load Ad Unit {adUnitId}: {error} - {message}. Test Mode: {_testMode}, Platform: {Application.platform}");
        isAdReady = false;
        OnAdLoaded = true;
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"❌ Failed to show Ad {adUnitId}: {error} - {message}");
    }

    public void OnUnityAdsShowStart(string adUnitId) { }
    public void OnUnityAdsShowClick(string adUnitId) { }
}

