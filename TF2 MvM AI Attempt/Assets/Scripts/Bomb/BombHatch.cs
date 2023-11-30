using UnityEngine;

public class BombHatch : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        other.GetComponent<RobotMovementBT>().CanSeparate = false;
        //Might create a point in the hatch so that it doesn't set it to true immediately.
        other.GetComponent<RobotMovementBT>().CanWander = true;
    }
    private void OnTriggerExit(Collider other)
    {
        other.GetComponent<RobotMovementBT>().CanSeparate = true;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}