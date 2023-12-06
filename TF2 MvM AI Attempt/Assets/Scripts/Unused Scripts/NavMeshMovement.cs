using UnityEngine;
using UnityEngine.AI;

public class NavMeshMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    public Transform target;
    [SerializeField] private float speed = 3f;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        agent.speed = speed;
        agent.SetDestination(target.position);
    }


}
