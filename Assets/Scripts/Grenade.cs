using System.Collections;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public float delay = 3f;
    public float radius = 5f;
    public float force = 700000f;

    float countdown;
    bool hasExploded = false;

    public MouseMovement mouse;
    public GameObject explosionEffect;
    Rigidbody rb;
    Rigidbody catRB;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        countdown = delay;
        mouse = FindFirstObjectByType<MouseMovement>();
        explosionEffect = Resources.Load<GameObject>("Prefabs/ExplosionEffect Variant");
    }

    // Update is called once per frame
    void Update()
    {
        if (mouse.thrown()) 
        {
        countdown -= Time.deltaTime;
        if (countdown <= 0 && !hasExploded) 
        {
            Explode();
        hasExploded = true;
        }
        }
    }

    public void Explode() 
    {
        Debug.Log("Boom");
        GameObject explosive = Instantiate(explosionEffect, transform.position, transform.rotation);
        
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider nearbyObjects in colliders) 
        {
            Debug.Log("Got Colliders");
            rb = nearbyObjects.GetComponent<Rigidbody>();
            if (rb != null && rb.gameObject.name == "cat") 
            {
                Debug.Log("Applied force");
                //this.gameObject.SetActive(false);
                catRB = rb;
                rb.AddExplosionForce(force, transform.position, radius);
                StartCoroutine(activeCatkinematic(2.0f));
            }
            
            
        }

        StartCoroutine(DestroyExplosionAfterDelay(explosive, 3f));
    }

    IEnumerator DestroyExplosionAfterDelay(GameObject explosion, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
        Destroy(explosion);
    }

    IEnumerator activeCatkinematic(float del) 
    {
        yield return new WaitForSeconds(del);
        catRB.isKinematic = true;
        yield return new WaitForSeconds(3.0f);
        catRB.isKinematic = false;
    }

}
