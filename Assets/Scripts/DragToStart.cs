using UnityEngine;
using UnityEngine.EventSystems;

public class DragToStart : MonoBehaviour
{
    public GameObject gameplayUI;
    public GameObject startPanel;

    private void Start()
    {
        //Time.timeScale = 0.0f;
    }

    public void OnDrag(BaseEventData data)
    {
        bool gameStarted = UIManager.Instance.GetGameState();
        if (!gameStarted)
        {
            Debug.Log("Game Started by Dragging");
            //gameplayUI.SetActive(true);
            //startPanel.SetActive(false);
            UIManager.Instance.whenDragged();
            //gameStarted = true;
            
            //StartGame();
        }
        else 
        {
            Debug.Log("Nahi chal raha ukhar le kuch= GameStarted:"+gameStarted);
        }
    }

    private void Update()
    {
        
    }

    void StartGame() 
    {
        Time.timeScale = 1.0f;
    }
}
