using UnityEngine;

public class BombHatch : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        other.GetComponent<Robot>().CanSeparate = false;
    }
    private void OnTriggerExit(Collider other)
    {
        other.GetComponent<Robot>().CanSeparate = true;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}