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
    public GameObject shopPanel;
    public GameObject currencyPanel;

    private bool gameStarted = false;
   // public Button useButton;
    public GameObject useButton;
    

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
    }

    public void NextLvlBtn()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // Optional: Check if the next scene exists
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
            UIManager.Instance.ShowInitialPanel();
            gameplayPanel.SetActive(false);
            startPanel.SetActive(true);
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
    // Add more methods for showing/hiding other panels
}
