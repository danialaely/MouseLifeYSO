using UnityEngine;

public class KeyCollected : MonoBehaviour
{
    private Transform target; // assign the mouse here
    public float followSpeed = 10f;


    private void Start()
    {
        GameObject mouse = GameObject.FindGameObjectWithTag("Player");
        target = mouse.GetComponent<Transform>();
    }

    void Update()
    {
        if (target == null) return;

        // Smooth follow
        transform.position = Vector3.Lerp(transform.position, target.position + new Vector3(0, 1.50f, 0), followSpeed * Time.deltaTime);
    }
}
