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

    private Rigidbody rb;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;
        GoToNextPatrolPoint();
        retryPanel.SetActive(false);
        BoomParticleEffect.SetActive(false);

        rb = GetComponent<Rigidbody>();
        //rb.linearVelocity = Vector3.zero;
        // Get the 4th child (index 3) and its Light component
        spotLight = transform.GetChild(3).GetComponent<Light>();

        if (spotLight != null)
        {
            originalColor = spotLight.color; // Store initial color
        }
    }

    void Update()
    {
        if (CanSeeMouse())
        {
            isChasing = true;  // Start chasing
            ChaseMouse();
            ChangeSpotlightColor(Color.red); // Set light to red
        }
        else
        {
            if (isChasing) // If the agent was chasing but lost sight
            {
                isChasing = false; // Stop chasing
                FindNearestPatrolPoint(); // Reassign a patrol target
            }

            if (!agent.pathPending && agent.remainingDistance <= 0.5f)
            {
                GoToNextPatrolPoint();
            }
            ChangeSpotlightColor(originalColor); // Revert light to original color

        }

        // Rotate the cat in the direction it's moving
        if (agent.velocity.magnitude > 0.1f) // Only rotate if moving
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


    bool CanSeeMouse()
    {
        float distance = Vector3.Distance(transform.position, mouse.position);
        if (distance < sightRange)
        {
            Vector3 directionToMouse = (mouse.position - transform.position).normalized;
            if (Vector3.Dot(transform.forward, directionToMouse) > 0.5f) // Check if Mouse is in front
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, directionToMouse, out hit, sightRange))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        Debug.Log("Can see");
                        return true;
                    }
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Level Failed!");
            //AudioManager.instance.PlaySFX(AudioManager.instance.levelFailedSFX);
            BoomParticleEffect.SetActive(true);
            StartCoroutine(ActiveRetryPanel(1.2f));
        }
        if (collision.gameObject.CompareTag("Wall")) 
        {
           // Instantiate(HittingWallPE, this.transform.position, Quaternion.identity);
        }
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
