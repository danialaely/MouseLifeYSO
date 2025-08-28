using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseStatusPanel : MonoBehaviour
{

    public Image bg;
    public TMP_Text heading;
    public Image itemIcon;
    public TMP_Text itemText;

    public Color success;
    public Color failed;

    public Sprite successBg;
    public Sprite failedBg;

    public static PurchaseStatusPanel INSTANCE { get; protected private set; }

    private void Awake()
    {
        INSTANCE = this;
        gameObject.SetActive(false);
    }

    public void Show(Sprite sprite, string text, bool status)
    {
        if (status)
        {
            bg.sprite = successBg;
            heading.color = success;
            heading.text = "Purchase Successful";
        }
        else
        {
            bg.sprite = failedBg;
            heading.color = failed;
            heading.text = "Purchase Failed";
        }

        itemIcon.sprite = sprite;
        itemText.text = text;
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
