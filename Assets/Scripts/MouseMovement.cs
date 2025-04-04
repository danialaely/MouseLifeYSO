using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.AI.Navigation;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MouseMovement : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody rb;
    private Vector2 moveDirection;
    public InputAction playerControls;
    private Animator mouseAnim;

    public GameObject useBtn;

    public GameObject[] weaponPrefabs; // Assign Mine, Bomb, Dynamite, Soccer Boot, Wheel in the Inspector

    
    public GameObject currentWeapon; // Store the currently spawned weapon
    
    private float throwForce = 1.0f; // Adjust the throw force as needed

    public FixedJoystick joystick;
    public float SpeedMove = 5f;
    public float rotationSpeed = 10f;

    public bool hasthrown = false;

    public int cheeseCount = 0;
    public int maxCheese = 4;  // Number of cheese required to spawn the gift
    public GameObject giftPrefab;  // Assign the gift prefab in Inspector
    public NavMeshSurface navMeshSurface; // Assign the baked NavMesh surface in Inspector
    public Rigidbody catRB;

    public GameObject cheesePopUpPanel;
    public GameObject giftPopUpPanel;
    public GameObject Cam;
    public Vector3 camOffset;

    public Slider cageSlider;   // Reference to the UI Slider
    public GameObject cageTxt;
    public float sliderSpeed = 0.5f; // Speed of slider movement
    public GameObject cagePrefab;

    public GameObject cageCol;
    public GameObject cageconvertPE;
    GameObject newWeapon;
    public GameObject bulletPrefab;
    GameObject bullet;
    public ParticleSystem cheeseEffect;

    //TO BE DONE: In this prototype the player has to gather multple items to enable/spawn gift box.
    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Get Rigidbody component
        useBtn.SetActive(false);
        mouseAnim = GetComponent<Animator>();
        cheesePopUpPanel.SetActive(false);
        giftPopUpPanel.SetActive(false);

        if (cageSlider != null) 
        {
            cageSlider.maxValue = maxCheese; // Set slider max value
        }
    }

    void Update()
    {
        Debug.Log("CWWW:"+currentWeapon);

        if (bulletPrefab != null)
        {
            //Rigidbody bulletrb = bulletPrefab.GetComponent<Rigidbody>();
            //bulletrb.linearVelocity = transform.forward * 100;
           // Debug.Log("is it even moving?");
           // bulletPrefab.transform.position += transform.forward * Time.deltaTime * 50;
        }
        Vector3 moveDirection = new Vector3(joystick.Horizontal, 0, joystick.Vertical).normalized;
        cheesePopUpPanel.transform.position = this.transform.position + new Vector3(0,1,1.5f);
        giftPopUpPanel.transform.position = this.transform.position + new Vector3(0,1,1.5f);
        Cam.transform.position = this.transform.position + camOffset; //Vector3(0,20,-5.5f)
        if (moveDirection.magnitude > 0.1f) // Ensure movement input is present
        {
            // Move the player
            transform.position += moveDirection * SpeedMove * Time.deltaTime;
            mouseAnim.SetBool("isWalking", true);
            // Adjust rotation to face movement direction (with 90-degree correction if needed)
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up) * Quaternion.Euler(0, 0, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else 
        {
            mouseAnim.SetBool("isWalking", false);
        }

       
    }

    void FixedUpdate()
    {
        
      //  Vector3 moveVector = new Vector3(moveDirection.x, 0, moveDirection.y) * speed; // Convert to 3D
       // rb.linearVelocity = moveVector; // Apply velocity for movement
      // transform.position += moveVector;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cheese"))
        {
            cheesePopUpPanel.SetActive(true);
            StartCoroutine(DeactiveCheesePopUp(1.0f));

            Destroy(other.gameObject);  // Remove the cheese
            cheeseCount++;
            AudioManager.instance.PlaySFX("PickCheese4");
            PlayCheeseEffect();

            if (cheeseCount == maxCheese)
            {
                SpawnGift();
               // cheeseCount = 0; // Reset cheese count
            }
        }

        if (other.CompareTag("Gift"))
        {
            Debug.Log("Gift collision detected");
            Destroy(other.gameObject); // Destroy the gift
            useBtn.SetActive(true); // Enable the Use button
            AudioManager.instance.PlaySFX("PickGift");
            giftPopUpPanel.SetActive(true);
            StartCoroutine(DeactiveCheesePopUp(1.0f));

            // Check if player has children (should be a hand or weapon holder)
            if (transform.childCount > 0 && weaponPrefabs.Length > 0)
            {
                Transform targetChild = transform.GetChild(2); // Get the first child
                Debug.Log("Target child for weapon: " + targetChild.name);

                int randomIndex = Random.Range(0, weaponPrefabs.Length);

                // Instantiate weapon normally if it's NOT pistol_3
                if (randomIndex != 3)
                {
                    Debug.Log("Random Index: " + randomIndex);
                    newWeapon = Instantiate(weaponPrefabs[randomIndex], targetChild.position, Quaternion.identity);
                }
                else
                {
                    // Special handling for pistol_3 (index 3)
                    Debug.Log("Pistol index: " + randomIndex);
                    newWeapon = Instantiate(weaponPrefabs[randomIndex], Vector3.zero, Quaternion.identity);
                    newWeapon.transform.SetParent(targetChild); // Set as a child first

                    newWeapon.transform.localPosition = new Vector3(1, 1, -5); // Position offset
                    newWeapon.transform.localRotation = Quaternion.Euler(-90.0f, 90.0f, 0.0f); // Correct rotation
                }

                // Parent all weapons to targetChild
                newWeapon.transform.SetParent(targetChild);

                // If it's NOT pistol_3, reset position and rotation
                if (randomIndex != 3)
                {
                    newWeapon.transform.localPosition = Vector3.zero;
                    newWeapon.transform.localRotation = Quaternion.identity;
                }

                currentWeapon = newWeapon;
                Debug.Log("Assigned currentWeapon: " + currentWeapon.name);
            }
            else
            {
                Debug.LogWarning("No child found or weaponPrefabs is empty!");
            }
        }


        if (other.CompareTag("cageCollider"))  // If colliding with the cage
        {
            StartCoroutine(AnimateSliderValue(cageSlider.value, cheeseCount));
            Debug.Log(cheeseCount);
            if (cheeseCount >= 4) 
            {
                Debug.Log("Cage Spawned");
                cageCol = other.gameObject;
                Instantiate(cageconvertPE, other.transform.position, Quaternion.identity);
                AudioManager.instance.PlaySFX("Cage2");
                StartCoroutine(DeactiveCageSlider(1.0f));
            }
        }

        if (other.CompareTag("CrackCollider")) 
        {
            Debug.Log("Creaking Sound");
            AudioManager.instance.PlayCreaking("Creaking");
        }
    }

    public void PlayCheeseEffect()
    {
        if (cheeseEffect != null)
        {
            cheeseEffect.Play();
            // Stop the effect after 1 second
            Invoke(nameof(StopCheeseEffect), 0.5f);
        }
        else
        {
            Debug.LogWarning("Particle effect not assigned.");
        }
    }

    private void StopCheeseEffect()
    {
        if (cheeseEffect.isPlaying)
        {
            cheeseEffect.Stop();
        }
    }

    IEnumerator DeactiveCageSlider(float del) 
    {
        yield return new WaitForSeconds(del);
        Instantiate(cagePrefab, cageCol.transform.position , Quaternion.Euler(-80.0f,0.0f,0.0f));
        cageTxt.gameObject.SetActive(false);
        cageCol.gameObject.SetActive(false);
        cageSlider.gameObject.SetActive(false);
    }

    private IEnumerator AnimateSliderValue(float startValue, float targetValue)
    {
        float elapsedTime = 0;
        while (elapsedTime < sliderSpeed)
        {
            cageSlider.value = Mathf.Lerp(startValue, targetValue, elapsedTime / sliderSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        cageSlider.value = targetValue; // Ensure it reaches the exact target value
    }

    public void UseBtn()
    {
        Debug.Log("UseBtn Pressed, currentWeapon: " + currentWeapon);
        catRB.isKinematic = false;
        catRB.useGravity = true;
        StartCoroutine(ActiveKinem(4.0f));

        if (currentWeapon != null && currentWeapon.name != "Pistol_3(Clone)")
        {
            Debug.Log("Throwing weapon: " + currentWeapon.name);
            currentWeapon.transform.SetParent(null);

            Rigidbody rb = currentWeapon.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = currentWeapon.AddComponent<Rigidbody>(); // Add Rigidbody if missing
                rb.isKinematic = false;
                rb.useGravity = true;
            }

            if (rb.gameObject.name != "water_mine(Clone)")
            {
                //  rb.linearVelocity = transform.forward * throwForce;
                Debug.Log(rb.name);
            }

            hasthrown = true;
            StartCoroutine(thrownfalse(5.0f));
            //catRB.isKinematic = false;

            currentWeapon = null;
            useBtn.SetActive(false);
        }
        else if (currentWeapon != null && currentWeapon.name == "Pistol_3(Clone)") 
        {
            Transform bulletSpawnPoint = currentWeapon.transform.GetChild(0);
            Debug.Log("Pistol hai Pistol");
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
            bullet.transform.SetParent(bulletSpawnPoint);
            AudioManager.instance.PlaySFX("Bullet2");
           // currentWeapon = null;
            useBtn.SetActive(false);
            StartCoroutine(DeactiveGun(2.0f));

            bullet.transform.localPosition = Vector3.zero;
            bullet.transform.localRotation = Quaternion.identity;
            //hasthrown = true;
            // bulletPrefab.transform.SetParent(null);

        }
        else
        {
            Debug.LogError("Weapon is still NULL!");
        }
    }

    IEnumerator ActiveKinem(float del) 
    {
        yield return new WaitForSeconds(del);
        catRB.isKinematic = true;
        catRB.useGravity = false;
    }

    IEnumerator DeactiveGun(float del) 
    {
        yield return new WaitForSeconds(del);
        Destroy(currentWeapon.gameObject);
        currentWeapon = null;
    }

    public bool thrown() 
    {
        return hasthrown;
    }

    IEnumerator thrownfalse(float del) 
    {
        yield return new WaitForSeconds(del);
        hasthrown = false;
    }

    void SpawnGift()
    {
        Vector3 randomPosition = GetRandomPointOnNavMesh();
        Instantiate(giftPrefab, randomPosition, Quaternion.identity);
        giftPrefab.SetActive(true);
    }

    Vector3 GetRandomPointOnNavMesh()
    {
        // Define a random range for spawning within the NavMesh bounds
        Vector3 randomPosition = Vector3.zero;
        NavMeshHit hit;

        for (int i = 0; i < 10; i++) // Try 10 times to find a valid point
        {
            float randomX = Random.Range(-10f, 10f); // Adjust based on your scene
            float randomZ = Random.Range(-10f, 10f);
            Vector3 randomPoint = new Vector3(randomX, 1.0f, randomZ); // Y is slightly above ground

            if (NavMesh.SamplePosition(randomPoint, out hit, 2.0f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        return randomPosition; // Return default position if no valid point found
    }

    IEnumerator DeactiveCheesePopUp(float del) 
    {
        yield return new WaitForSeconds(del);
        cheesePopUpPanel.SetActive(false);
        giftPopUpPanel.SetActive(false);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
