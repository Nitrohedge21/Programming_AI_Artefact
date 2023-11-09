using UnityEngine;

[RequireComponent(typeof(TankBB))]
public class Tank : MonoBehaviour {

    public float MoveSpeed = 10.0f;

    private Vector3 MoveLocation;
    private bool IsMoving = false;

    [SerializeField] private Waypoints waypoints;

    [SerializeField] private float rotateSpeed = 4.0f;

    [SerializeField] private float distance = 0.1f;

    public Transform currentWaypoint;
    private Quaternion targetRotation;
    private Vector3 directionToWaypoint;

    private BTNode BTRootNode;
    
    void Start()
    {
        currentWaypoint = waypoints.GetNextWaypoint(currentWaypoint);
        transform.LookAt(currentWaypoint);

        //Get reference to Robot Blackboard
        TankBB bb = GetComponent<TankBB>();

        //Create our root selector
        Selector rootChild = new Selector(bb); // selectors will execute it's children 1 by 1 until one of them succeeds
        BTRootNode = rootChild;

        CompositeNode hatchRoot = new Sequence(bb);
        hatchRoot.AddChild(new TankFollowWaypoints(bb, this));

        //Adding to root selector
        rootChild.AddChild(hatchRoot);

        //Execute our BT every 0.1 seconds
        InvokeRepeating("ExecuteBT", 0.1f, 0.1f);
    }
    #region Functions
    // Update is called once per frame
    void Update()
    {
        if (IsMoving)
        {
            Vector3 dir = MoveLocation - transform.position;
            transform.position += dir.normalized * MoveSpeed * Time.deltaTime;
        }
    }

    public void RobotMoveTo(Vector3 MoveLocation)
    {
        IsMoving = true;
        this.MoveLocation = MoveLocation;
    }

    public void MoveTowardsWaypoint()
    {
        IsMoving = true;
        this.MoveLocation = currentWaypoint.position;
        if (Vector3.Distance(transform.position, currentWaypoint.position) < distance)
        {
            currentWaypoint = waypoints.GetNextWaypoint(currentWaypoint);
        }
    }
    public void RotateTowardsWaypoint()
    {
        directionToWaypoint = (currentWaypoint.position - transform.position).normalized;
        targetRotation = Quaternion.LookRotation(directionToWaypoint);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }

    public void StopMovement()
    {
        IsMoving = false;
    }

    public void ExecuteBT()
    {
        BTRootNode.Execute();
    }
    #endregion
}

public class TankMoveToHatch : BTNode
{
    private TankBB tBB;
    private Tank tankRef;
    bool FirstRun = true;
    public TankMoveToHatch(Blackboard bb, Tank _tank) : base(bb)
    {
        tBB = (TankBB)bb;
        tankRef = _tank;
    }

    public override BTStatus Execute()
    {
        if (FirstRun)
        {
            FirstRun = false;
        }
        BTStatus rv = BTStatus.RUNNING;
        tankRef.RobotMoveTo(tBB.HatchLocation);
        if ((tankRef.transform.position - tBB.HatchLocation).magnitude <= 1.0f)
        {
            Debug.Log("Reached the target");
            rv = BTStatus.SUCCESS;
            FirstRun = true;
        }
        return rv;
    }

}

public class TankFollowWaypoints : BTNode
{
    private TankBB tBB;
    private Tank tankRef;
    bool FirstRun = true;

    public TankFollowWaypoints(Blackboard bb, Tank _tank) : base(bb)
    {
        tankRef = _tank;
        tBB = (TankBB)bb;
    }
    public override BTStatus Execute()
    {
        if (FirstRun)
        {
            FirstRun = false;
        }
        BTStatus rv = BTStatus.RUNNING;
        tankRef.MoveTowardsWaypoint();
        tankRef.RotateTowardsWaypoint();
        if ((tankRef.transform.position - tBB.HatchLocation).magnitude <= 1.0f)
        {
            rv = BTStatus.SUCCESS;
            FirstRun = true;
        }
        return rv;
    }
}
