using UnityEngine;
using UnityEngine.AI;

public class NavMeshMovement : MonoBehaviour
{
    public Transform target;
    private NavMeshPath path;
    private int currentCornerIndex = 0;
    public float speed = 0f;

    private void Start()
    {
        path = new NavMeshPath();
        InvokeRepeating("CalculatePath", 0.1f, 0.3f);
    }

    private void Update()
    {
        // Move the object along the calculated path
        MoveAlongPath();
    }

    void CalculatePath()
    {
        if (target != null)
        {
            NavMesh.CalculatePath(transform.position, target.position, NavMesh.AllAreas, path);
            currentCornerIndex = 0;
        }
    }

    void MoveAlongPath()
    {
        //if (path.corners.Length < 2) return;

        if (currentCornerIndex < path.corners.Length)
        {
            Vector3 nextPosition = path.corners[currentCornerIndex];
            Vector3 direction = (nextPosition - transform.position).normalized;

            transform.position += direction * speed * Time.deltaTime;

            // Check if we're close enough to the current corner before moving to the next one
            if (Vector3.Distance(transform.position, nextPosition) < 0.1f)
            {
                currentCornerIndex++;
            }
        }
    }
}
