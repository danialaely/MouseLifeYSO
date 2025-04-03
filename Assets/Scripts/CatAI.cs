using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class CatAI : MonoBehaviour
{
    public Transform[] patrolPoints; // Assign Waypoint1 and Waypoint2 in Inspector
    public Transform mouse; // Assign Mouse in Inspector
    public float sightRange = 5f; // Cat's vision range
    public float chaseSpeed = 5f; // Speed when chasing Mouse
    public float patrolSpeed = 2f; // Speed when patrolling
    private int currentPatrolIndex = 0;
    private NavMeshAgent agent;

    public GameObject retryPanel;
    public GameObject BoomParticleEffect;
    public GameObject HittingWallPE;

    private bool isChasing = false;

    private Light spotLight;   // Reference to the spotlight
    private Color originalColor; // Store the original colo
    //private float lastDistance = 0f;
    Animator catAnimator;
    private Rigidbody rb;

    public float hearingRange = 8.0f; // Distance the cat can hear
    public LayerMask soundLayer; // Layer for sound-emitting objects
    public Animator mouseAnim;

    public GameObject questionMarkPanel;

    private float lingerTime = 2.0f; // Time to linger after the sound stops
    private float lingerTimer = 0.0f;
    private Vector3 lastSoundPosition;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;
        GoToNextPatrolPoint();
        retryPanel.SetActive(false);
        BoomParticleEffect.SetActive(false);

        rb = GetComponent<Rigidbody>();
        catAnimator = GetComponent<Animator>();
        //rb.linearVelocity = Vector3.zero;
        // Get the 4th child (index 3) and its Light component
        spotLight = transform.GetChild(3).GetComponent<Light>();

        if (spotLight != null)
        {
            originalColor = spotLight.color; // Store initial color
        }

        rb.isKinematic = true;
        rb.useGravity = false;
        Debug.Log("Kinematic enabled, Gravity disabled.");
        questionMarkPanel.SetActive(false);
       // mouseAnim = mouse.GetComponent<Animator>();
    }

    void Update()
    {
        questionMarkPanel.transform.position = this.transform.position + new Vector3(0, 3, 1.5f);

        if (CanSeeMouse())
        {
            isChasing = true;
            ChaseMouse();
            ChangeSpotlightColor(Color.red); // Set light to red
            questionMarkPanel.SetActive(false);
            lingerTimer = 0.0f; // Reset linger timer when seeing the mouse
            return;
        }

        if (CanHearSound(out Vector3 soundPosition))
        {
            Debug.Log("Heard Sound");
            isChasing = true;
            lastSoundPosition = soundPosition;
            lingerTimer = lingerTime; // Reset linger timer when hearing sound
            ChaseSound(soundPosition);
            questionMarkPanel.SetActive(true);
            return;
        }

        // Linger after sound stops
        if (lingerTimer > 0)
        {
            lingerTimer -= Time.deltaTime;
            isChasing = true;
            ChaseSound(lastSoundPosition);
            questionMarkPanel.SetActive(true);
            return;
        }

        // If not chasing anymore
        if (isChasing)
        {
            isChasing = false;
            FindNearestPatrolPoint();
        }

        if (!agent.pathPending && agent.remainingDistance <= 0.5f)
        {
            GoToNextPatrolPoint();
        }
        ChangeSpotlightColor(originalColor);
        questionMarkPanel.SetActive(false);

        // Rotate the cat in the direction it's moving
        if (agent.velocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        agent.destination = patrolPoints[currentPatrolIndex].position;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    void ChangeSpotlightColor(Color newColor)
    {
        if (spotLight != null)
        {
            spotLight.color = newColor;
        }
    }

    bool CanHearSound(out Vector3 soundPosition)
    {
        soundPosition = Vector3.zero;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, hearingRange, soundLayer);

        foreach (Collider collider in hitColliders)
        {
            if (collider.CompareTag("SoundSource"))
            {
                AudioSource audioSource = collider.GetComponent<AudioSource>();
                if (audioSource != null && audioSource.isPlaying) // Check if sound is playing
                {
                    soundPosition = collider.transform.position;
                    Debug.Log("Heard a sound at: " + soundPosition);
                    return true;
                }
            }
        }
        return false;
    }

    void ChaseSound(Vector3 soundPosition)
    {
        isChasing = true;
        agent.speed = chaseSpeed;
        agent.destination = soundPosition;
        Debug.Log("Chasing the sound!");

        float distance = Vector3.Distance(transform.position, mouse.position);
        if (distance < sightRange)
        {
            //Debug.Log("Distance:" + distance);
            Vector3 directionToMouse = (mouse.position - transform.position).normalized;
            if (Vector3.Dot(transform.forward, directionToMouse) > 0.5f) // Check if Mouse is in front
            {
                ChangeSpotlightColor(Color.red);
               // mouseAnim.SetBool("isChased", true);
            }
        }
        else 
        {
            ChangeSpotlightColor(Color.yellow);
           // mouseAnim.SetBool("isChased", false);
        }
    }

    bool CanSeeMouse()
    {
        float distance = Vector3.Distance(transform.position, mouse.position);
        if (distance < sightRange)
        {
            Debug.Log("Distance: " + distance);
            Vector3 directionToMouse = (mouse.position - transform.position).normalized;

            // Check if the mouse is in front of the cat
            if (Vector3.Dot(transform.forward, directionToMouse) > 0.5f)
            {
                Debug.Log("Mouse is in front");
                RaycastHit hit;

                // Perform the raycast to detect obstacles between the cat and the mouse
                if (Physics.Raycast(transform.position, directionToMouse, out hit, sightRange))
                {
                    Debug.DrawRay(transform.position, directionToMouse * hit.distance, Color.green);

                    // Check if the raycast hit the mouse and not an obstacle
                    if (hit.collider.name == "MouseNew")
                    {
                        Debug.Log("Can See Mouse");
                    }
                    else
                    {
                        Debug.Log("Obstacle detected: " + hit.collider.name);
                    }
                       // AudioManager.instance.PlaySFX("Chase");
                       // StartCoroutine(StopSfXChase(1.0f));
                        return true;
                }
                else
                {
                    Debug.DrawRay(transform.position, directionToMouse * sightRange, Color.red);
                }
            }
        }
        return false;
    }


    void ChaseMouse()
    {
        isChasing = true;
        agent.speed = chaseSpeed;
        agent.destination = mouse.position;
    }

    IEnumerator StopSfXChase(float del) 
    {
        yield return new WaitForSeconds(del);
        AudioManager.instance.StopSFX();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Level Failed!");
            mouseAnim.SetBool("isCaught", true);
            //AudioManager.instance.PlaySFX(AudioManager.instance.levelFailedSFX);
            BoomParticleEffect.SetActive(true);
            AudioManager.instance.PlaySFX("LevelFailed");
            StartCoroutine(ActiveRetryPanel(1.2f));
        }
        if (collision.gameObject.CompareTag("Wall")) 
        {
           // Instantiate(HittingWallPE, this.transform.position, Quaternion.identity);
        }
        if (collision.gameObject.CompareTag("Bullet")) 
        {
            catAnimator.SetBool("isBomb",true);
            StartCoroutine(BackToRunning(2.0f));
        }
    }

    IEnumerator BackToRunning(float del) 
    {
        yield return new WaitForSeconds(del);
        catAnimator.SetBool("isBomb",false);
    }

    IEnumerator ActiveRetryPanel(float del) 
    {
        yield return new WaitForSeconds(del);
        retryPanel.SetActive(true);
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Reloads the current scene
    }

    void FindNearestPatrolPoint()
    {
        float shortestDistance = Mathf.Infinity;
        int closestIndex = 0;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            float distance = Vector3.Distance(transform.position, patrolPoints[i].position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closestIndex = i;
            }
        }

        currentPatrolIndex = closestIndex;
        agent.destination = patrolPoints[currentPatrolIndex].position;
    }
}
