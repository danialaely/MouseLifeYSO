using UnityEngine;
using TMPro; // if you are using TextMeshPro

public class LoadingText : MonoBehaviour
{
    public TMP_Text loadingText;  // Drag your text here in inspector
    public float dotSpeed = 0.5f; // how fast the dots change

    private string baseText = "Loading";
    private int dotCount = 0;

    private void OnEnable()
    {
        InvokeRepeating(nameof(UpdateLoadingText), 0f, dotSpeed);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(UpdateLoadingText));
    }

    private void UpdateLoadingText()
    {
        dotCount = (dotCount + 1) % 4; // cycles 0,1,2,3
        loadingText.text = baseText + new string('.', dotCount);
    }
}
