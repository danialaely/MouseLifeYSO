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

    [Header("Texts")]
    public TMP_Text lvlTxt;
    public TMP_Text lvlTxt2;

    private bool gameStarted = false;
   // public Button useButton;
    public GameObject useButton;
    bool activeTouchCanvas = false;
    public RectTransform itemPages; // Assign ItemPages here via Inspector
    public float scrollDuration = 0.3f; // How long the scroll takes (in seconds)

    public Button[] pageButtons; // Assign your 3 buttons in the Inspector
    public Color selectedColor = Color.white;
    public Color normalColor = Color.gray;

   // public ShopManager shopManager;

    private Vector2[] positions = new Vector2[]
    {
        new Vector2(0f, 0f),
        new Vector2(-830f, 0f),
        new Vector2(-1660f, 0f)
    };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // Avoid duplicates on scene load
        }
    }

    private void Start()
    {
        GameObject mouse = GameObject.FindGameObjectWithTag("HostageMouse");
        //Debug.Log("Hello:" + mouse);
        mouse.GetComponent<FollowPlayerMouse>().touchCanvas.SetActive(false);
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
    }

    public void ShowLevelFailed()   //Cat collides with Player
    {
        levelFailedPanel.SetActive(true);
    }

    public void whenDragged() 
    {
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
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // Optional: Check if the next scene exists
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            int lvlnumber = nextSceneIndex + 1;
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
    }

    public void BtnTwo()
    {
        HighlightButton(1);
        StartCoroutine(ScrollToPosition(positions[1]));
    }

    public void BtnThree()
    {
        HighlightButton(2);
        StartCoroutine(ScrollToPosition(positions[2]));
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
