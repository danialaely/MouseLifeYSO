using UnityEngine;
using UnityEngine.EventSystems;

public class DragToStart : MonoBehaviour
{
    public GameObject gameplayUI;
    public GameObject startPanel;
    private bool gameStarted = false;

    private void Start()
    {
        Time.timeScale = 0.0f;
    }

    public void OnDrag(BaseEventData data)
    {
        if (!gameStarted)
        {
            Debug.Log("Game Started by Dragging");
            gameplayUI.SetActive(true);
            startPanel.SetActive(false);
            gameStarted = true;
            StartGame();
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
