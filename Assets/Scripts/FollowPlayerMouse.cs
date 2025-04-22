using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class FollowPlayerMouse : MonoBehaviour
{
    public Transform playerMouse;
    public bool shouldFollow = false;

    public float stopDistance = 1.5f; // Distance at which the mouse stops running
    private NavMeshAgent agent;
    private Animator animator;
    public ParticleSystem confetti;
    public GameObject levelCompletedPanel;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        agent.updateRotation = false; // We'll handle rotation manually
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
        }
    }

    IEnumerator levelComp(float del) 
    {
        yield return new WaitForSeconds(del);
        levelCompletedPanel.SetActive(true);
    }
}
