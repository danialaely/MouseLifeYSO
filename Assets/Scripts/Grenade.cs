using System.Collections;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public float delay = 3f;
    public float radius = 5f;
    private float force = 2000f;

    float countdown;
    bool hasExploded = false;

    public MouseMovement mouse;
    public GameObject explosionEffect;
    Rigidbody rb;
    Rigidbody catRB;
    private Animator catanim;
    private bool isSlipping = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        countdown = delay;
        mouse = FindFirstObjectByType<MouseMovement>();
        //explosionEffect = Resources.Load<GameObject>("Prefabs/ExplosionEffect Variant");
        explosionEffect = Resources.Load<GameObject>("Prefabs/CFXR Explosion 1 + Text");
    }

    // Update is called once per frame
    void Update()
    {
        if (mouse.thrown()) 
        {
        countdown -= Time.deltaTime;
        if (countdown <= 0 && !hasExploded && this.gameObject.name != "banana(Clone)" && this.gameObject.name != "Pistol_3(Clone)") 
        {
            Explode();
            hasExploded = true;
        }
        }
    }

    public void Explode() 
    {
        Debug.Log("Boom");
        AudioManager.instance.PlaySFX("Explosion");
        GameObject explosive = Instantiate(explosionEffect, transform.position, transform.rotation);
        
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider nearbyObjects in colliders) 
        {
            Debug.Log("Got Colliders");
            rb = nearbyObjects.GetComponent<Rigidbody>();
            if (rb != null && rb.gameObject.name == "CatNew") 
            {
                Debug.Log("Applied force");
                catanim = rb.gameObject.GetComponent<Animator>();
                catanim.SetBool("isDynamite", true);
                //this.gameObject.SetActive(false);
                catRB = rb;
                rb.AddExplosionForce(force, transform.position, radius);
               // StartCoroutine(activeCatkinematic(2.0f));
            }
            
            
        }
        StartCoroutine(DeactiveCatAnim(2.0f));
        StartCoroutine(DestroyExplosionAfterDelay(explosive, 3f));
    }

    IEnumerator DeactiveCatAnim(float del) 
    {
        yield return new WaitForSeconds(del);
        catanim.SetBool("isDynamite", false);
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "CatNew") 
        {
            if (this.gameObject.name == "banana(Clone)")
            {
                Debug.Log("Slip");
                catanim = collision.gameObject.GetComponent<Animator>();
                catRB = collision.gameObject.GetComponent<Rigidbody>();
                AudioManager.instance.PlaySFX("Banana2");
                Slip();
            }
            else if (this.gameObject.name == "Pistol(Clone)") 
            {
                Debug.Log("Shoot");
            }
            else
            {
                Explode();
                hasExploded = true;
            }
        }
    }


    public void Slip()
    {
        if (!isSlipping)
        {
            StartCoroutine(SlipCoroutine());
        }
    }

    private IEnumerator SlipCoroutine()
    {
        isSlipping = true;

        // Disable movement or apply force to simulate slipping
        catRB.linearVelocity = new Vector3(3f, 0, 3f); // Example slipping force
        catanim.SetBool("isSliding",true);
        yield return new WaitForSeconds(1f); // Slip for 1 second

        catRB.linearVelocity = Vector3.zero; // Stop slipping
        isSlipping = false;
        catanim.SetBool("isSliding",false);
        Destroy(this.gameObject);
    }

    IEnumerator DeactiveBanana(float del) 
    {
        yield return new WaitForSeconds(del);
        Destroy(this.gameObject);
    }

}
