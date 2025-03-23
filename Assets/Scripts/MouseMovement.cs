using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.AI.Navigation;
using UnityEngine.AI;
using UnityEngine.UI;

public class MouseMovement : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody rb;
    private Vector2 moveDirection;
    public InputAction playerControls;
    private Animator mouseAnim;

    public GameObject useBtn;

    public GameObject[] weaponPrefabs; // Assign Mine, Bomb, Dynamite, Soccer Boot, Wheel in the Inspector
    private GameObject currentWeapon; // Store the currently spawned weapon
    public float throwForce = 1.0f; // Adjust the throw force as needed

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

    public Slider cageSlider;   // Reference to the UI Slider
    public float sliderSpeed = 0.5f; // Speed of slider movement

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
        Vector3 moveDirection = new Vector3(joystick.Horizontal, 0, joystick.Vertical).normalized;
        cheesePopUpPanel.transform.position = this.transform.position + new Vector3(0,1,1.5f);
        giftPopUpPanel.transform.position = this.transform.position + new Vector3(0,1,1.5f);
        //Cam.transform.position = this.transform.position + new Vector3(0,20,-5.5f);
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
            if (cheeseCount > maxCheese)
            {
                SpawnGift();
                cheeseCount = 0; // Reset cheese count
            }
        }

        if (other.CompareTag("Gift")) 
        {
            Destroy(other.gameObject); // Destroy the gift
            useBtn.SetActive(true); // Enable the Use button
            AudioManager.instance.PlaySFX("PickGift");
            giftPopUpPanel.SetActive(true);
            StartCoroutine(DeactiveCheesePopUp(1.0f));

            // Check if this object has at least one child
            if (transform.childCount > 0 && weaponPrefabs.Length > 0)
            {
                Transform targetChild = transform.GetChild(2); // Get the first child

                int randomIndex = Random.Range(0, weaponPrefabs.Length);
                currentWeapon = Instantiate(weaponPrefabs[randomIndex], targetChild.position, Quaternion.identity);
                currentWeapon.transform.SetParent(targetChild); // Set as a child of the first child
                currentWeapon.transform.localPosition = Vector3.zero; // Keep centered in the child
                currentWeapon.transform.localRotation = Quaternion.identity; // Reset rotation
            }
        }

        if (other.CompareTag("cageCollider"))  // If colliding with the cage
        {
            StartCoroutine(AnimateSliderValue(cageSlider.value, cheeseCount));
        }
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
        if (currentWeapon != null)
        {
            // Detach from any parent
            currentWeapon.transform.SetParent(null);

            // Add Rigidbody if not already present
            Rigidbody rb = currentWeapon.GetComponent<Rigidbody>();
            Debug.Log("Weapom Name:"+rb.gameObject.name);
            if (rb == null)
            {
                rb = currentWeapon.AddComponent<Rigidbody>(); // Add Rigidbody dynamically
            }

            // Apply force to throw it forward
            if (rb.gameObject.name != "water_mine(Clone)") 
            {
                rb.linearVelocity = transform.forward * throwForce;
            }
            hasthrown = true;
            StartCoroutine(thrownfalse(5.0f));
            catRB.isKinematic = false;
            // Clear reference
            currentWeapon = null;

            // Hide the button since we used the weapon
            useBtn.SetActive(false);
        }
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
    }

    Vector3 GetRandomPointOnNavMesh()
    {
        // Define a random range for spawning within the NavMesh bounds
        Vector3 randomPosition = Vector3.zero;
        NavMeshHit hit;

        for (int i = 0; i < 10; i++) // Try 10 times to find a valid point
        {
            float randomX = Random.Range(-5f, 5f); // Adjust based on your scene
            float randomZ = Random.Range(-5f, 5f);
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

}
