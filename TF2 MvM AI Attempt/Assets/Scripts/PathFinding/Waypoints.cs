using UnityEngine;

public class Waypoints : MonoBehaviour
{
    [Range(0f,2f)]
    [SerializeField] float sphereSize = 1f;
    [SerializeField] Color sphereColour = Color.blue;

    private void OnDrawGizmos()
    {
        //  Draws a sphere on each waypoint's position  //
        foreach(Transform point in transform)
        {
            Gizmos.color = sphereColour;
            Gizmos.DrawWireSphere(point.position, sphereSize);
        }

        //  Draws a line between the waypoints  //
        Gizmos.color = Color.red;
        for (int i = 0; i < transform.childCount - 1; i++)
        {
            //Gizmos.DrawLine(transform.GetChild(i).position, transform.GetChild(i + 1).position);
        }

        //Unfortunately, the thickness of these lines can not be changed.
        //To make the first and the last lines connect, use the code below
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
            //Debug.Log("Reached the last waypoint");
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
