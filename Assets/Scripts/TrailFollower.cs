using UnityEngine;

public class TrailFollower : MonoBehaviour
{
    public Transform target; // assign the mouse here
    public float followSpeed = 10f;

    void Update()
    {
        if (target == null) return;

        // Smooth follow
        transform.position = Vector3.Lerp(transform.position, target.position+new Vector3(0,0.50f,0), followSpeed * Time.deltaTime);
    }
}

