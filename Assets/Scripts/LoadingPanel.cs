using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class LoadingPanel : MonoBehaviour
{
    public static LoadingPanel INSTANCE {  get; private protected set; }

    public float loadingTime;

    private GameObject panel;

    private void Awake()
    {
        INSTANCE = this;
        panel = transform.GetChild(0).gameObject;
    }

    private void Start()
    {
        Show(loadingTime);
    }

    public void Show(float t = float.MaxValue) => StartCoroutine(ShowLoading(t));

    public IEnumerator ShowLoading(float time = float.MaxValue)
    {
        panel.SetActive(true);

        if (time != float.MaxValue)
        {
            yield return new WaitForSeconds(time);
            panel.SetActive(false);
        }
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}
