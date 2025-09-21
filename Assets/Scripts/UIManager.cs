using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Panels")]
    public GameObject startPanel;
    public GameObject gameplayPanel;
    public GameObject levelCompletedPanel;
    public GameObject levelFailedPanel;
    public GameObject storePanel;
    public GameObject currencyPanel;
    public GameObject WeaponUnlockedPanel;

    [Header("Texts")]
    public TMP_Text lvlTxt;
    public TMP_Text lvlTxt2;
    public TMP_Text storeHeadingTxt;

    private bool gameStarted = false;
   // public Button useButton;
    public GameObject useButton;
    bool activeTouchCanvas = false;
    public RectTransform itemPages; // Assign ItemPages here via Inspector
    public float scrollDuration = 0.3f; // How long the scroll takes (in seconds)

    public Button[] pageButtons; // Assign your 3 buttons in the Inspector
    public Color selectedColor = Color.white;
    public Color normalColor = Color.gray;

    public Button Reward1Button;
    public Button Reward2Button;

    // Reference to HostageMouse (shown in Inspector)
    [Header("References")]
    public GameObject hostageMouse;
    int nextSceneIndex;

    // public ShopManager shopManager;
    public GameObject BananaPrefab;
    public GameObject DynamitePrefab;
    public GameObject PistolPrefab;
    public GameObject MinePrefab;


    public GameObject BananaPreview;
    public GameObject DynamitePreview;
    public GameObject PistolPreview;
    public GameObject MinePreview;

    private Vector2[] positions = new Vector2[]
    {
        new Vector2(0f, 0f),
        new Vector2(-830f, 0f),
        new Vector2(-1660f, 0f)
    };

    public int currentTry = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        if (!PlayerPrefs.HasKey("CurrentLevel"))
        {
            PlayerPrefs.SetInt("CurrentLevel", 2);
        }
    }

    private void Start()
    {
        StartCoroutine(FindHostageMouse());
        SceneManager.LoadScene(PlayerPrefs.GetInt("CurrentLevel"));

    }

    private IEnumerator FindHostageMouse()
    {

        while (hostageMouse == null)
        {
            hostageMouse = GameObject.FindGameObjectWithTag("HostageMouse");
            yield return null; 
        }

        Debug.Log("HostageMouse found and assigned in Inspector: " + hostageMouse.name);

        FollowPlayerMouse fpm = hostageMouse.GetComponent<FollowPlayerMouse>();
        if (fpm != null && fpm.touchCanvas != null)
        {
            fpm.touchCanvas.SetActive(false);
        }
        else
        {
            Debug.LogError("⚠️ FollowPlayerMouse or touchCanvas missing on HostageMouse.");
        }
    }


    public void ShowInitialPanel() //Call in start
    {
        startPanel.SetActive(true);
        gameplayPanel.SetActive(false);
        levelCompletedPanel.SetActive(false);
        levelFailedPanel.SetActive(false);
        //gameStarted = false;
        setGameState(false);
    }

    public void ShowLevelCompleted()   //When CageMouse collides with EndCollider
    {
        levelCompletedPanel.SetActive(true);

        int level = PlayerPrefs.GetInt("CurrentLevel");
        if (level == 2)
        {
            WeaponUnlockedPanel.SetActive(true);
            BananaPreview.SetActive(true);
            GameData.unlockedWeapons.Add(BananaPrefab);
        }
        else if (level == 4)
        {
            WeaponUnlockedPanel.SetActive(true);
            DynamitePreview.SetActive(true);
            GameData.unlockedWeapons.Add(DynamitePrefab);
        }
        else if (level == 5)
        {
            WeaponUnlockedPanel.SetActive(true);
            PistolPreview.SetActive(true);
            GameData.unlockedWeapons.Add(PistolPrefab);
        }
        else if (level == 6)
        {
            WeaponUnlockedPanel.SetActive(true);
            MinePreview.SetActive(true);
            GameData.unlockedWeapons.Add(MinePrefab);
        }

        TryShowAd();
    }

    public void ClaimWeaponBtn() 
    {
        WeaponUnlockedPanel.SetActive(false);
        BananaPreview.SetActive(false);
        DynamitePreview.SetActive(false);
        PistolPreview.SetActive(false);
        MinePreview.SetActive(false);
    }

    public void ShowLevelFailed()   //Cat collides with Player
    {
        levelFailedPanel.SetActive(true);

        TryShowAd();
    }

    public void TryShowAd()
    {
        Reward1Button.interactable = true;
        Reward2Button.interactable = true;

        currentTry++;
        Debug.Log("Current try" + currentTry);
        if (currentTry == 3)
        {
            InterstitialAds.Instance.ShowOrLoadAd();
            currentTry = 0;

        }
    }

    public void whenDragged() 
    {
        //SceneManager.LoadScene(PlayerPrefs.GetInt("CurrentLevel"));
        gameplayPanel.SetActive(true);
        startPanel.SetActive(false);
        setGameState(true);

        if (!activeTouchCanvas) 
        {
            GameObject mouse = GameObject.FindGameObjectWithTag("HostageMouse");
            //Debug.Log("Hello:" + mouse);
            mouse.GetComponent<FollowPlayerMouse>().touchCanvas.SetActive(true);
            activeTouchCanvas = true;
        }
    }

    public bool GetGameState() 
    {
        return gameStarted;
    }

    public void setGameState(bool gs) 
    {
        gameStarted = gs;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        levelFailedPanel.SetActive(false);
        gameplayPanel.SetActive(false);
        startPanel.SetActive(true);
        setGameState(false);
        activeTouchCanvas = false;
    }

    public void NextLvlBtn()
    {
        //int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        //int nextSceneIndex = currentSceneIndex + 1;
        nextSceneIndex=PlayerPrefs.GetInt("CurrentLevel");
        
        if (PlayerPrefs.GetInt("CurrentLevel") <= 10)
        {
        nextSceneIndex = nextSceneIndex + 1;

        }
        PlayerPrefs.SetInt("CurrentLevel", nextSceneIndex);
        
        // Optional: Check if the next scene exists
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            int lvlnumber = PlayerPrefs.GetInt("CurrentLevel")-1;
            ShopManager.Instance.setLevel(lvlnumber);
            ShopManager.Instance.SetupShopItems();

            SceneManager.LoadScene(nextSceneIndex);
            lvlTxt.text = "Level" + lvlnumber;
            lvlTxt2.text = "Level" + lvlnumber;
            UIManager.Instance.ShowInitialPanel();
            gameplayPanel.SetActive(false);
            startPanel.SetActive(true);
            DeactiveWeaponBtn();
            
            activeTouchCanvas = false;
        }
        else
        {
            Debug.Log("No more levels! Looping back to first level or show GameComplete screen.");
            SceneManager.LoadScene(0); // Or load a "GameComplete" scene
        }

        if (PlayerPrefs.GetInt("CurrentLevel") > 10)
        {
            PlayerPrefs.SetInt("CurrentLevel", 2);
        }

    }

    public void ActiveWeaponBtn() 
    {
        useButton.SetActive(true);
        AssignMouseToButton();
    }

    public void DeactiveWeaponBtn()
    {
        useButton.SetActive(false);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
    }

    void AssignMouseToButton()
    {
        // Find the Mouse GameObject in the scene (use tag or name)
        GameObject mouse = GameObject.FindGameObjectWithTag("Player");
        if (mouse != null)
        {
            MouseMovement mm = mouse.GetComponent<MouseMovement>();
            if (mm != null)
            {
                Button weaponBtn = useButton.GetComponent<Button>();
                weaponBtn.onClick.RemoveAllListeners(); // Clear old reference
                weaponBtn.onClick.AddListener(mm.UseBtn);
            }
        }
        else
        {
            Debug.LogWarning("Mouse not found in scene!");
        }
    }

    public void ActiveStorePanel() 
    {
        storePanel.SetActive(true);
    }

    public void DectivateStorePanel()
    {
        storePanel.SetActive(false);
    }

    public void ActiveCurrencyPanel()
    {
       currencyPanel.SetActive(true);
    }

    public void DeactiveCurrencyPanel()
    {
        currencyPanel.SetActive(false);
    }

    public void BtnOne()
    { 
        HighlightButton(0);
        StartCoroutine(ScrollToPosition(positions[0]));
        storeHeadingTxt.text = "Skins";
    }

    public void BtnTwo()
    {
        HighlightButton(1);
        StartCoroutine(ScrollToPosition(positions[1]));
        storeHeadingTxt.text = "Weapons";
    }

    public void BtnThree()
    {
        HighlightButton(2);
        StartCoroutine(ScrollToPosition(positions[2]));
        storeHeadingTxt.text = "Gadgets";
    }

    private IEnumerator ScrollToPosition(Vector2 target)
    {
        Vector2 start = itemPages.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < scrollDuration)
        {
            itemPages.anchoredPosition = Vector2.Lerp(start, target, elapsed / scrollDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Snap to exact position at the end
        itemPages.anchoredPosition = target;
    }

    private void HighlightButton(int index)
    {
        for (int i = 0; i < pageButtons.Length; i++)
        {
            Image btnImage = pageButtons[i].GetComponent<Image>();
            btnImage.color = (i == index) ? selectedColor : normalColor;
        }
    }

    public void ActiveCharacterPanel() { }
    // Add more methods for showing/hiding other panels
}
