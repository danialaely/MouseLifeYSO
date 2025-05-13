using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.AI.Navigation;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;

public class MouseMovement : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody rb;
    private Vector2 moveDirection;
    public InputAction playerControls;
    private Animator mouseAnim;
    public Animator cagemouseAnim;

    public GameObject useBtn;

    public GameObject[] weaponPrefabs; // Assign Mine, Bomb, Dynamite, Soccer Boot, Wheel in the Inspector


    public GameObject currentWeapon; // Store the currently spawned weapon

    // private float throwForce = 1.0f; // Adjust the throw force as needed

    public RectTransform knob;

    public float SpeedMove = 5f;
    public float rotationSpeed = 10f;

    public bool hasthrown = false;

    public int cheeseCount = 0;
    public int giftCheeseInterval = 4;  // Number of cheese required to spawn the gift
    public int maxCheese = 4;  // Number of cheese required to spawn the gift
    private bool cageSpawned = false;

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
    public GameObject CreakingAudio;
    public GameObject sliderPrefab;
    public Transform uiCanvas3d; // Drag your Canvas here in the inspector

    public bool mousetrapped;
    GameObject sliderInstance;
    GameObject mTrap;
    public GameObject saveObj;
    public GameObject cageObj;
    public ParticleSystem mHouseEffect;

    public Transform[] giftSpawnPositions;

    public GameObject innerPortal;
    public GameObject wallToRotate;
    public GameObject portalPrefab;
    public bool wallrotation;


    [SerializeField]
    private Vector2 JoystickSize = new Vector2(225, 225);
    [SerializeField]
    private CustomJoystick Joystick;
    
    public GameObject dummyJoystick;
    //[SerializeField]
    //private NavMeshAgent Player;

    private Finger MovementFinger;
    private Vector2 MovementAmount;

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable(); // starting with Unity 2022 this does not work! You need to attach a TouchSimulation.cs script to your player
        ETouch.Touch.onFingerDown += HandleFingerDown;
        ETouch.Touch.onFingerUp += HandleLoseFinger;
        ETouch.Touch.onFingerMove += HandleFingerMove;
    }

    private void OnDisable()
    {
        ETouch.Touch.onFingerDown -= HandleFingerDown;
        ETouch.Touch.onFingerUp -= HandleLoseFinger;
        ETouch.Touch.onFingerMove -= HandleFingerMove;
        EnhancedTouchSupport.Disable(); // You need to attach a TouchSimulation.cs script to your player
    }

    private void HandleFingerMove(Finger MovedFinger)
    {
        if (MovedFinger == MovementFinger)
        {
            Vector2 knobPosition;
            float maxMovement = JoystickSize.x / 2f;
            ETouch.Touch currentTouch = MovedFinger.currentTouch;

            if (Vector2.Distance(currentTouch.screenPosition,Joystick.RectTransform.anchoredPosition) > maxMovement)
            {
                knobPosition = (currentTouch.screenPosition - Joystick.RectTransform.anchoredPosition).normalized * maxMovement;
            }
            else
            {
                knobPosition = currentTouch.screenPosition - Joystick.RectTransform.anchoredPosition;
            }

            Joystick.Knob.anchoredPosition = knobPosition;
            MovementAmount = knobPosition / maxMovement;
        }
    }

    private void HandleLoseFinger(Finger LostFinger)
    {
        if (LostFinger == MovementFinger)
        {
            MovementFinger = null;
            Joystick.Knob.anchoredPosition = Vector2.zero;
            Joystick.gameObject.SetActive(false);
            dummyJoystick.SetActive(true);
            MovementAmount = Vector2.zero;
        }
    }

    private void HandleFingerDown(Finger TouchedFinger)
    {
        if (MovementFinger == null && TouchedFinger.screenPosition.x <= Screen.width / 2f)
        {
            MovementFinger = TouchedFinger;
            MovementAmount = Vector2.zero;
            Joystick.gameObject.SetActive(true);
            dummyJoystick.SetActive(false);
            Joystick.RectTransform.sizeDelta = JoystickSize;
            Joystick.RectTransform.anchoredPosition = ClampStartPosition(TouchedFinger.screenPosition);
        }
    }

    private Vector2 ClampStartPosition(Vector2 StartPosition)
    {
        if (StartPosition.x < JoystickSize.x / 2)
        {
            StartPosition.x = JoystickSize.x / 2;
        }

        if (StartPosition.y < JoystickSize.y / 2)
        {
            StartPosition.y = JoystickSize.y / 2;
        }
        else if (StartPosition.y > Screen.height - JoystickSize.y / 2)
        {
            StartPosition.y = Screen.height - JoystickSize.y / 2;
        }

        return StartPosition;
    }

    private void Awake()
    {
        if (CreakingAudio == null)
        {
            Debug.Log("is this working?");
            CreakingAudio = GameObject.FindGameObjectWithTag("SoundSource");
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Get Rigidbody component
        useBtn.SetActive(false);
        mouseAnim = GetComponent<Animator>();
        cheesePopUpPanel.SetActive(false);
        giftPopUpPanel.SetActive(false);
        mousetrapped = false;
        cagemouseAnim.enabled = false;
        wallrotation = false;

        if (cageSlider != null)
        {
            cageSlider.maxValue = maxCheese + 1; // Or just set to 12 manually
            cageSlider.value = 0;
        }

    }

    void Update()
    {
        Debug.Log("CWWW:" + currentWeapon);
        CreakingAudio.transform.position = transform.position;

        if (bulletPrefab != null)
        {
            //Rigidbody bulletrb = bulletPrefab.GetComponent<Rigidbody>();
            //bulletrb.linearVelocity = transform.forward * 100;
            // Debug.Log("is it even moving?");
            // bulletPrefab.transform.position += transform.forward * Time.deltaTime * 50;
        }


            cheesePopUpPanel.transform.position = this.transform.position + new Vector3(0, 1, 1.5f);
        giftPopUpPanel.transform.position = this.transform.position + new Vector3(0, 1, 1.5f);
        Cam.transform.position = this.transform.position + camOffset; //Vector3(0,20,-5.5f)

        //Vector3 scaledMovement = speed*Time.deltaTime*new Vector3(MovementAmount.x,0, MovementAmount.y);
        //this.transform.LookAt(this.transform.position + scaledMovement, Vector3.up);

        // Joystick movement (based on FloatingJoystick)
        Vector3 moveDirection = new Vector3(MovementAmount.x, 0, MovementAmount.y).normalized;

        //Vector3 moveDirection = new Vector3(joystick.Horizontal, 0, joystick.Vertical).normalized;

        if (moveDirection.magnitude > 0.1f && !mousetrapped)
        {
            transform.position += moveDirection * SpeedMove * Time.deltaTime;
            mouseAnim.SetBool("isWalking", true);

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
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
            Debug.Log("Name of hidden Cheese item:" + other.gameObject.name);
            cheesePopUpPanel.SetActive(true);
            StartCoroutine(DeactiveCheesePopUp(1.0f));

            Destroy(other.gameObject);  // Remove the cheese
            cheeseCount++;
            AudioManager.instance.PlaySFX("PickCheese4");
            PlayCheeseEffect();

            Debug.Log("Cheese Collected: " + cheeseCount);

            // Spawn gift on reaching milestones: 4, 8, 12, 16...
            if (cheeseCount % maxCheese == 0)
            {
                SpawnGift();
            }
        }

        if (other.CompareTag("Gift"))
        {
            Transform targetChild = transform.GetChild(2); // Get the first child
            Debug.Log("Target child for weapon: " + targetChild.name);

            if (targetChild.childCount == 0)
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
        }


        if (other.CompareTag("cageCollider"))  // If colliding with the cage
        {
            // Animate slider to current cheese count
            //StartCoroutine(AnimateSliderValue(cageSlider.value, cheeseCount));
            Debug.Log("Cheese Count at Cage Collider: " + cheeseCount);
            Instantiate(cageconvertPE, other.transform.position, Quaternion.identity);
            AudioManager.instance.PlaySFX("Cage2");
            mHouseEffect.gameObject.SetActive(true);
            mHouseEffect.Play();

            // Enable hostage mouse to follow player
            GameObject hostageMouse = GameObject.FindGameObjectWithTag("HostageMouse");
            if (hostageMouse != null)
            {
                FollowPlayerMouse follower = hostageMouse.GetComponent<FollowPlayerMouse>();
                if (follower != null)
                {
                    cagemouseAnim.enabled = true;
                    follower.playerMouse = this.transform;
                    follower.shouldFollow = true;
                }
            }

            Destroy(other.gameObject);
            Destroy(saveObj);
            Destroy(cageObj);
            // Spawn cage only if cheeseCount >= 12 and not already spawned
            /*if (cheeseCount == 12 && !cageSpawned)
            {
                Debug.Log("Spawning Cage...");
                cageCol = other.gameObject;

                //Instantiate(cagePrefab, other.transform.position, Quaternion.identity);
                Instantiate(cageconvertPE, other.transform.position, Quaternion.identity);

                AudioManager.instance.PlaySFX("Cage2");
                StartCoroutine(DeactiveCageSlider(1.0f));

                cageSpawned = true;
            }*/
        }

        if (other.CompareTag("SliderCol"))
        {
            Debug.Log("Cheese Count at Cage Collider: " + cheeseCount);
            StartCoroutine(AnimateSliderValue(cageSlider.value, cheeseCount));
            //Instantiate(cageconvertPE, other.transform.position, Quaternion.identity);

            if (cheeseCount >= 5 && !cageSpawned)
            {
                Debug.Log("Spawning Portal...");
                cageCol = other.gameObject;

                //Instantiate(cagePrefab, other.transform.position, Quaternion.identity);
                //Instantiate(cageconvertPE, other.transform.position, Quaternion.identity);

                //AudioManager.instance.PlaySFX("Cage2");
                StartCoroutine(DeactiveCageSlider(1.0f));

                cageSpawned = true;
            }
        }

        if (other.CompareTag("CrackCollider"))
        {
            Debug.Log("Creaking Sound");
            AudioManager.instance.PlayCreaking("Creaking");
        }

        if (other.CompareTag("mouseTrap"))
        {
            //mousetrapped = true;
            Animator trapAnim = other.GetComponent<Animator>();
            AudioManager.instance.PlaySFX("mouseTrap");
            trapAnim.SetBool("mouseTrapped", true);
            StartCoroutine(mouseTrap(4.0f));
            //Destroy(other.gameObject);
            // Instantiate the slider and make it a child of the canvas (for organization)
            sliderInstance = Instantiate(sliderPrefab, other.transform.position, Quaternion.identity, uiCanvas3d);
            mTrap = other.gameObject;
            // Optional: make the slider face the camera
            //sliderInstance.transform.LookAt(Camera.main.transform);
            sliderInstance.transform.Rotate(90, 0, 0); // flip if it looks backward
        }

        if (other.CompareTag("portal"))
        {
            // transform.position = innerPortal.transform.position + new Vector3(0,0,-3);
            StartCoroutine(Teleport(0.3f));
            //wallToRotate.transform.GetChild(0).GetComponent<NavMeshObstacle>().enabled = false;
        }

        if (other.CompareTag("inPortal"))
        {
            transform.position = portalPrefab.transform.position + new Vector3(0, 0, -3);
            //StartCoroutine(Teleport(0.3f));
            //wallToRotate.transform.GetChild(0).GetComponent<NavMeshObstacle>().enabled = false;
        }

        if (other.CompareTag("rotatorBtn"))
        {
            other.transform.position += new Vector3(0, -0.3f, 0);


            if (!wallrotation)
            {
                wallToRotate.transform.rotation = Quaternion.Euler(0, 90, 0);
                wallrotation = true;
            }
            else
            {
                wallToRotate.transform.rotation = Quaternion.Euler(0, 0, 0);
                wallrotation = false;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("rotatorBtn"))
        {
            other.transform.position += new Vector3(0, 0.3f, 0);
        }

        if (other.CompareTag("portal"))
        {
            //wallToRotate.transform.GetChild(0).GetComponent<NavMeshObstacle>().enabled = true;
        }
    }

    IEnumerator Teleport(float del)
    {
        yield return new WaitForSeconds(del);
        transform.position = innerPortal.transform.position + new Vector3(0, 0, -3);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Debug.Log("Got Hit By Bullet");
        }
    }
    IEnumerator mouseTrap(float del)
    {
        yield return new WaitForSeconds(0.1f);
        mousetrapped = true;
        yield return new WaitForSeconds(del);
        mousetrapped = false;
        Destroy(sliderInstance);
        Destroy(mTrap);
    }
    //IEnumerator DeactiveSlider() { }

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
        Instantiate(portalPrefab, cageCol.transform.position, Quaternion.Euler(0.0f, 0.0f, 0.0f));
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
            //bullet.transform.SetParent(null);
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
        if (giftSpawnPositions == null || giftSpawnPositions.Length == 0)
        {
            Debug.LogWarning("No gift spawn positions assigned!");
            return;
        }

        // Find the closest spawn position to the player
        Transform closestSpawn = giftSpawnPositions[0];
        float closestDistance = Vector3.Distance(transform.position, closestSpawn.position);

        for (int i = 1; i < giftSpawnPositions.Length; i++)
        {
            float distance = Vector3.Distance(transform.position, giftSpawnPositions[i].position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestSpawn = giftSpawnPositions[i];
            }
        }

        // Instantiate the gift at the closest spawn point
        GameObject gift = Instantiate(giftPrefab, closestSpawn.position, Quaternion.identity);
        gift.SetActive(true);
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

    public void NextLvlBtn()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // Optional: Check if the next scene exists
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("No more levels! Looping back to first level or show GameComplete screen.");
            SceneManager.LoadScene(0); // Or load a "GameComplete" scene
        }
    }
}
