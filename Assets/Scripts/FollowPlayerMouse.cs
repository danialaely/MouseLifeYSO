using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System;
using Unity.VisualScripting;

public class FollowPlayerMouse : MonoBehaviour
{
    public Transform playerMouse;
    public bool shouldFollow = false;

    public float stopDistance = 1.5f; // Distance at which the mouse stops running
    private NavMeshAgent agent;
    private Animator animator;
    public ParticleSystem confetti;
    public GameObject levelCompletedPanel;
    private CatAI[] allCats;
    public GameObject innerPortal;
    public GameObject wall;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        agent.updateRotation = false; // We'll handle rotation manually

        allCats = FindObjectsOfType<CatAI>();
        wall.GetComponent<NavMeshObstacle>().enabled = false;
        //innerPortal = GameObject.FindGameObjectWithTag("inPortal");
    }

    void Update()
    {
        if (shouldFollow && playerMouse != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerMouse.position);

            // Move toward the player mouse
            agent.SetDestination(playerMouse.position);

            // Play run animation if not close
            if (distanceToPlayer > stopDistance)
            {
                animator.SetBool("isRunning", true);
                agent.isStopped = false;
            }
            else
            {
                animator.SetBool("isRunning", false);
                agent.isStopped = true;
            }

            // Smoothly rotate toward the player
            Vector3 lookDirection = playerMouse.position - transform.position;
            lookDirection.y = 0f;
            if (lookDirection.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }
        else
        {
            animator.SetBool("isRunning", false); // Stop animation if not following
            agent.isStopped = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "EndWall") 
        {
            Debug.Log("Win");
            confetti.gameObject.SetActive(true);
            confetti.Play();
            StartCoroutine(levelComp(1.0f));
            AudioManager.instance.PlaySFX("LevelCompleted");
            playerMouse.gameObject.GetComponent<MouseMovement>().dummyJoystick.SetActive(false);
            // Loop through and stop their agents
            if (allCats != null) 
            {
            foreach (CatAI cat in allCats)
            {
                NavMeshAgent agent = cat.GetComponent<NavMeshAgent>();
                if (agent != null)
                {
                    agent.isStopped = true;
                }
            }
            }
        }

        if (other.CompareTag("portal"))
        {
            //+ new Vector3(0, 0, -3)
            transform.position = innerPortal.transform.position + new Vector3(0, 0, -3);
          //  wall.GetComponent<NavMeshObstacle>().enabled = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("portal"))
        {
           StartCoroutine(wallObs(0.3f));
        }
    }

    IEnumerator wallObs(float del) 
    {
        yield return new WaitForSeconds(del);
        wall.GetComponent<NavMeshObstacle>().enabled = true;
    }

        IEnumerator levelComp(float del) 
    {
        yield return new WaitForSeconds(del);
        levelCompletedPanel.SetActive(true);
    }
}
