using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cheese : MonoBehaviour
{
    public float attractionRadius = 3f; // Radius in which cheese is attracted
    public float moveSpeed = 2f;  // Initial speed of movement
    public float maxSpeed = 7f;   // Max speed for smooth effect
    public Transform mouse;       // Reference to the Mouse

    private bool isAttracted = false;

    void Start()
    {
        mouse = FindFirstObjectByType<MouseMovement>().transform; // Auto-find the mouse
    }

    void Update()
    {
        if (!isAttracted)
        {
            float distance = Vector3.Distance(transform.position, mouse.position);
            if (distance <= attractionRadius)
            {
                isAttracted = true; // Start moving towards the mouse
            }
        }

        if (isAttracted)
        {
            // Increase speed gradually for a smooth effect
            moveSpeed = Mathf.Lerp(moveSpeed, maxSpeed, Time.deltaTime * 2f);

            // Move towards the mouse
            transform.position = Vector3.MoveTowards(transform.position, mouse.position, moveSpeed * Time.deltaTime);

            // If cheese reaches mouse, collect it
            if (Vector3.Distance(transform.position, mouse.position) < 0.2f)
            {
                CollectCheese();
            }
        }
    }

    void CollectCheese()
    {
        MouseMovement mouseScript = mouse.GetComponent<MouseMovement>();
        if (mouseScript != null)
        {
            mouseScript.cheeseCount++; // Increase cheese count
        }

        Destroy(gameObject); // Remove cheese after collection
    }
}
