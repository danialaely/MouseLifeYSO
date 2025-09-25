using UnityEngine;
using UnityEngine.EventSystems;

public class DragToStart : MonoBehaviour, IPointerDownHandler
{
    public GameObject gameplayUI;
    public GameObject startPanel;

    private void Start()
    {
        //Time.timeScale = 0.0f;
    }

    public void OnPointerDown(PointerEventData eventData)
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


    public void OnDrag(BaseEventData data)
    {
    }

    private void Update()
    {
        
    }

    void StartGame() 
    {
        Time.timeScale = 1.0f;
    }
}
