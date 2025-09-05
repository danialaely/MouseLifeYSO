using System.Collections;
using UnityEngine;
using UnityEngine.AI;

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
    public NavMeshObstacle obstacle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        countdown = delay;
        mouse = FindFirstObjectByType<MouseMovement>();
        //explosionEffect = Resources.Load<GameObject>("Prefabs/ExplosionEffect Variant");
        explosionEffect = Resources.Load<GameObject>("Prefabs/CFXR Explosion 1 + Text");
        //obstacle = GetComponent<NavMeshObstacle>();
         EnableObstacle();
        //DisableObstacle();
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
            if (rb != null && rb.gameObject.tag == "Cat") 
            {
                Debug.Log("Applied force");
                catanim = rb.gameObject.GetComponent<Animator>();
                catanim.SetBool("isDynamite", true);

               CatAI catScript =  rb.gameObject.GetComponent<CatAI>();
                int newCatHealth = catScript.catHealth -= 1;
                catScript.SetCatHealth(newCatHealth);
                catScript.catHealthSlider.value = newCatHealth;
                Debug.Log("Cat's Health:"+newCatHealth);

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
        Debug.Log("takraya bhai takraya");
        Debug.Log("Weapon Name:"+this.gameObject.name);
        Debug.Log("Collided obj Name:"+collision.gameObject.name);
        if (collision.gameObject.CompareTag("Cat")) 
        {
            if (this.gameObject.CompareTag("banana"))
            {
                Debug.Log("Slip");
                catanim = collision.gameObject.GetComponent<Animator>();
                catRB = collision.gameObject.GetComponent<Rigidbody>();
                AudioManager.instance.PlaySFX("Banana2");
                Slip();
                //Destroy(this.gameObject);
            }
            else if (this.gameObject.CompareTag("Pistol")) 
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
            DisableObstacle();
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

    IEnumerator DeactiveWeapon(float del) 
    {
        yield return new WaitForSeconds(del);
        Destroy(this.gameObject);
    }

    public void EnableObstacle()
    {
        obstacle.enabled = true;
    }

    public void DisableObstacle()
    {
        obstacle.enabled = false;
    }
}
