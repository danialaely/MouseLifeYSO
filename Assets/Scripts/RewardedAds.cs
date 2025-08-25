using UnityEngine;
using UnityEngine.Advertisements;

public class RewardedAds : MonoBehaviour, IUnityAdsShowListener, IUnityAdsLoadListener
{
    [SerializeField] string _adUnitId = "Rewarded_Android";
    private bool isAdReady = false;

    void Start()
    {
        LoadAd();
    }

    public void LoadAd()
    {
        Advertisement.Load(_adUnitId, this);
    }

    public void ShowAd()
    {
        Debug.Log("ShowAd() called");

        if (isAdReady)
        {
            Advertisement.Show(_adUnitId, this);
            Debug.Log("Showing Ad");
        }
        else
        {
            Debug.LogWarning("Ad not ready yet");
        }
    }

    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        if (adUnitId.Equals(_adUnitId))
        {
            isAdReady = true;
            Debug.Log("✅ Ad is ready");
        }
    }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (adUnitId.Equals(_adUnitId) && showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            Debug.Log("🎁 Ad Completed Successfully — Give Reward Here");
            if (gameObject.name == "rewardBtn")
            {
                var mouse = GameObject.FindGameObjectWithTag("Player");
                if (mouse != null)
                {
                    mouse.GetComponent<MouseMovement>().SpeedMove = 7.5f;
                    Debug.Log("✅ Speed Boost Applied");
                }
            }
            else if (gameObject.name == "rewardBtn2")
            {
                ShopManager.Instance.AddGem(50);
                Debug.Log("✅ 50 Gems Added");
            }
        }
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"❌ Failed to load Ad Unit {adUnitId}: {error} - {message}");
        isAdReady = false;
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"❌ Failed to show Ad {adUnitId}: {error} - {message}");
    }

    public void OnUnityAdsShowStart(string adUnitId) { }
    public void OnUnityAdsShowClick(string adUnitId) { }
}

