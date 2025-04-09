using UnityEngine;
using UnityEngine.UI;

public class SliderAutoFill : MonoBehaviour
{
    public Slider slider;         // Assign in inspector or via GetComponent
    public float fillDuration = 2f;

    private void OnEnable()
    {
        slider.value = 0f;
        StartCoroutine(FillSlider());
    }

    private System.Collections.IEnumerator FillSlider()
    {
        float elapsed = 0f;

        while (elapsed < fillDuration)
        {
            elapsed += Time.deltaTime;
            slider.value = Mathf.Clamp01(elapsed / fillDuration);
            yield return null;
        }

        // Optional: destroy after fill or trigger something
        Debug.Log("Slider fill complete!");

        // Destroy(gameObject); // If you want to remove it after filling
    }
}
