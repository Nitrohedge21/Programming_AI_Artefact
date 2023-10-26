using UnityEngine;

public class WaypointMovement : MonoBehaviour
{
    [SerializeField] private Waypoints waypoints;

    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float rotateSpeed = 4.0f;

    [SerializeField] private float distance = 0.1f;

    public Transform currentWaypoint;
    private Quaternion targetRotation;
    private Vector3 directionToWaypoint;
    [SerializeField] private Transform realPlayer;

    void Start()
    {
        currentWaypoint = waypoints.GetNextWaypoint(currentWaypoint);
        transform.LookAt(currentWaypoint);
    }

    void Update()
    {
        if(playerIsClose() == true)
        {
            transform.position = Vector3.MoveTowards(transform.position, realPlayer.position, moveSpeed * Time.deltaTime);
        }
        else { MoveTowardsWaypoint(); RotateTowardsWaypoint(); }
        
    }

    #region Functions
    void MoveTowardsWaypoint()
    {
        transform.position = Vector3.MoveTowards(transform.position, currentWaypoint.position, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, currentWaypoint.position) < distance)
        {
            currentWaypoint = waypoints.GetNextWaypoint(currentWaypoint);
        }
    }

    void RotateTowardsWaypoint()
    {
        directionToWaypoint = (currentWaypoint.position - transform.position).normalized;
        targetRotation = Quaternion.LookRotation(directionToWaypoint);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }

    bool playerIsClose() 
    {
        float detectionRange = 5.0f;
        bool canFollowPlayer = false;
        if(Vector3.Distance(realPlayer.position,transform.position) < detectionRange)
        {
            canFollowPlayer = true;
        }
        return canFollowPlayer;
    }
    #endregion
}
