using UnityEngine;

public class Waypoints : MonoBehaviour
{
    [Range(0f,2f)]
    [SerializeField] float sphereSize = 1f;

    private void OnDrawGizmos()
    {
        foreach(Transform point in transform)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(point.position, sphereSize);
        }

        Gizmos.color = Color.red;
        for (int i = 0; i < transform.childCount - 1; i++)
        {
            Gizmos.DrawLine(transform.GetChild(i).position, transform.GetChild(i + 1).position);
        }

        //To make the first and the last lines connect, use the code below
        //Unfortunately, the thickness of these lines can not be changed.
        //Gizmos.DrawLine(transform.GetChild(transform.childCount - 1).position, transform.GetChild(0).position);
    }

    public Transform GetNextWaypoint(Transform currentWaypoint) 
    {
        if (currentWaypoint == null)
        {
            return transform.GetChild(0);
        }
        if(currentWaypoint.GetSiblingIndex() < transform.childCount - 1)
        {
            return transform.GetChild(currentWaypoint.GetSiblingIndex() + 1);
        }
        if(ReachedLastWaypoint(currentWaypoint))
        {
            Debug.Log("Reached the last waypoint");
            return transform.GetChild(currentWaypoint.GetSiblingIndex());
        }
        else
        {
            return transform.GetChild(0);
        }
    }

    bool ReachedLastWaypoint(Transform currentWaypoint) 
    {
        bool canStop = false;
        if (currentWaypoint.GetSiblingIndex() == transform.childCount - 1) 
        {
            canStop = true;
        }
        return canStop;
    }
}
