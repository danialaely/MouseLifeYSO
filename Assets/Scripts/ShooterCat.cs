using UnityEngine;
using UnityEngine.AI;

public class ShooterCat : MonoBehaviour
{
    public Transform[] waypoints;
    public Transform mouseTarget; // Assign the mouse object
    public float sightRange = 10f;
    public float fieldOfView = 90f; // how wide the cat's vision cone is

    public float rotationSpeed = 2f;
    public GameObject bulletPrefab;
    public Transform shootPoint; // where bullets are fired from
    public float shootCooldown = 1.5f;

    private float lastShootTime;

    private int currentWaypointIndex = 0;
    private NavMeshAgent agent;

    private Animator shootercatAnim;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        shootercatAnim = GetComponent<Animator>();
        if (waypoints.Length > 0)
            agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    // Update is called once per frame
    void Update()
    {
        if (CanSeeMouse())
        {
            Vector3 lookDir = (mouseTarget.position - transform.position).normalized;
            lookDir.y = 0f;
            Quaternion lookRotation = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
            shootercatAnim.SetBool("SeeMouse",true);
            ShootMouse();
        }
        else
        {
            shootercatAnim.SetBool("SeeMouse",false);
            //LookAround();
            Patrol();
        }
    }

    private bool CanSeeMouse()
    {
        Vector3 directionToMouse = mouseTarget.position - transform.position;
        float distance = directionToMouse.magnitude;

        if (distance > sightRange)
            return false;

        float angle = Vector3.Angle(transform.forward, directionToMouse);

        return angle <= fieldOfView / 2f;
    }

    private void LookAround()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }

    private void ShootMouse()
    {
        if (Time.time - lastShootTime < shootCooldown)
            return;

        lastShootTime = Time.time;

        GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, shootPoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.velocity = shootPoint.forward * 20f;

        // Optional: Add effects or sound 
    }

    private void OnDrawGizmosSelected()
    {
        if (mouseTarget == null) return;

        // Draw forward direction (blue)
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * sightRange);

        // Draw line to mouse (green if in FOV, red otherwise)
        Vector3 directionToMouse = mouseTarget.position - transform.position;
        float angle = Vector3.Angle(transform.forward, directionToMouse);
        Gizmos.color = (angle <= fieldOfView / 2f && directionToMouse.magnitude <= sightRange) ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, mouseTarget.position);

        // Draw FOV boundaries (yellow)
        Gizmos.color = Color.yellow;
        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfView / 2f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfView / 2f, 0) * transform.forward;

        Gizmos.DrawRay(transform.position, leftBoundary * sightRange);
        Gizmos.DrawRay(transform.position, rightBoundary * sightRange);
    }

    void Patrol()
    {
        agent.isStopped = false;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }
}
