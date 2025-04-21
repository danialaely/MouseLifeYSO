using UnityEngine;
using UnityEngine.AI;

public class FollowPlayerMouse : MonoBehaviour
{
    public Transform playerMouse;
    public bool shouldFollow = false;

    public float stopDistance = 1.5f; // Distance at which the mouse stops running
    private NavMeshAgent agent;
    private Animator animator;

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
            }
            else
            {
                animator.SetBool("isRunning", false);
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
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "EndWall") 
        {
            Debug.Log("Win");
        }
    }
}
