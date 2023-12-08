using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

[RequireComponent(typeof(RobotBB))]
public class RobotMovementBT : MonoBehaviour
{

    #region General/Required Stuff
    private BTNode BTRootNode;
    [HideInInspector] public NavMeshAgent agent;
    #endregion
    #region Movement Stuff
    public float MoveSpeed = 10.0f;
    private Vector3 MoveLocation;
    private bool IsMoving = false;
    public bool CanSeparate = true;
    public bool CanWander = false;
    private Vector3 ToOurAgent;
    float Distance;
    #endregion
    #region Waypoint Stuff
    [HideInInspector] private float distance = 3f;
    [HideInInspector] public Transform currentWaypoint;
    [HideInInspector] public GameObject[] waypointArray;
    [HideInInspector] List<GameObject> Visited = new List<GameObject>();
    [HideInInspector] private Waypoints waypoints;
    [HideInInspector] public GameObject targetWaypoint;
    #endregion
    #region Wall Avoidance
    [SerializeField] LayerMask layerMask;
    [Range(0f, 10f)] public float LineLength = 10f; // AKA - PenetrationDistance
    Vector3 WallNormal = Vector3.zero;
    #endregion
    #region Timer Stuff (2BeUsed4BombDropping)
    float time = 0f;
    float timeDelay = 1f;
    #endregion

    void Start()
    {
        //currentWaypoint = waypoints.GetNextWaypoint(currentWaypoint);

        agent = GetComponent<NavMeshAgent>();
        MoveLocation = transform.position;

        //CREATING OUR Robot BEHAVIOUR TREE

        //Get reference to Robot Blackboard
        RobotBB bb = GetComponent<RobotBB>();

        //Create our root selector
        Selector rootChild = new Selector(bb); // selectors will execute it's children 1 by 1 until one of them succeeds
        BTRootNode = rootChild;

        CompositeNode PickUpBomb = new Sequence(bb);
        BombDroppedDecorator bombDecoRoot = new BombDroppedDecorator(PickUpBomb, bb);
        PickUpBomb.AddChild(new RobotFindBomb(bb, this));
        PickUpBomb.AddChild(new RobotStopMovement(bb, this));

        CompositeNode TargetHatch = new Sequence(bb);
        BombPickedDecorator hatchRoot = new BombPickedDecorator(TargetHatch, bb, this);
        TargetHatch.AddChild(new RobotFindHatch(bb, this));
        TargetHatch.AddChild(new RobotStopMovement(bb, this));

        CompositeNode Wander = new Sequence(bb);
        Wander.AddChild(new RobotWander(bb, this));

        CompositeNode TestNode = new Sequence(bb);
        TestNode.AddChild(new RobotWaypoint(bb, this));
        TestNode.AddChild(new RobotStopMovement(bb, this));

        //Adding to root selector
        rootChild.AddChild(bombDecoRoot);
        rootChild.AddChild(hatchRoot);
        //rootChild.AddChild(Wander);
        //rootChild.AddChild(TestNode);


        //Execute our BT every 0.1 seconds
        InvokeRepeating("ExecuteBT", 0.1f, 0.1f);
    }
    
    void Update()
    {
        #region Waypoint stuff that were cut
        //waypointArray = GameObject.FindGameObjectsWithTag("Waypoints");
        //targetWaypoint = waypointArray[waypointArray.Length - 1];
        #endregion

        #region Seperation Stuff
        GameObject[] robots = GameObject.FindGameObjectsWithTag("Robots");
        List<GameObject> TaggedRobots = new List<GameObject>();
        TaggedRobots.Clear();
        foreach (GameObject robot in robots)
        {
            if (gameObject != robot && Vector3.Distance(robot.transform.position, transform.position) <= 2.5f)
            {
                TaggedRobots.Add(robot);
            }
        }
        #endregion
        if (IsMoving)
        {
            Vector3 dir = MoveLocation - transform.position;
            transform.position += dir.normalized * MoveSpeed * Time.deltaTime;
            WallAvoidance();
            Seperation(TaggedRobots);
        }
    }

    #region Functions
    public void RobotMoveTo(Vector3 MoveLocation)
    {
        IsMoving = true;
        if (GetComponent<RobotShootingBT>() != null && !GetComponent<RobotShootingBT>().IsShooting) { transform.LookAt(MoveLocation); }
        this.MoveLocation = MoveLocation;
    }

    void Seperation(List<GameObject> _taggedVehicles)
    {
        // TODO: Figure out a proper and efficent way of doing this.
        if (CanSeparate == true)
        {
            foreach (GameObject vehicle in _taggedVehicles)
            {
                ToOurAgent = transform.position - vehicle.transform.position;
                Distance = ToOurAgent.magnitude;
                //The line below is most likely the cause of jittery movement issue.
                RobotMoveTo(transform.position + ToOurAgent.normalized / Distance);
            }
        }
    }

    public void Waypoint()
    {
        IsMoving = true;
        // An attempt at making it go to the final one
        //RobotMoveTo(waypointArray[waypointArray.Length - 1].transform.position);

        foreach (GameObject point in waypointArray)
        {
            if (!ReachedLastPoint())
            {
                // If the agent has already visited a point, it skips to the other one.
                if (Visited.Contains(point))
                {
                    continue;
                }
                if (point != currentWaypoint && Vector3.Distance(transform.position, currentWaypoint.position) < distance)
                {
                    currentWaypoint = point.transform;
                    RobotMoveTo(currentWaypoint.position);
                    if (Vector3.Distance(currentWaypoint.position, transform.position) < 3f)
                    {
                        Visited.Add(currentWaypoint.gameObject);
                    }
                }
                // IF the agent find a point that's closer to the one it was originally going for, it chooses the closest one.
                else if (point != currentWaypoint && Vector3.Distance(transform.position, currentWaypoint.position) > Vector3.Distance(transform.position, point.transform.position))
                {
                    currentWaypoint = point.transform;
                    RobotMoveTo(currentWaypoint.position);
                    if (Vector3.Distance(currentWaypoint.position, transform.position) < 3f)
                    {
                        Visited.Add(currentWaypoint.gameObject);
                    }
                }
            }

        }
    }

    public void Pathfinding(Vector3 targetPos)
    {
        IsMoving = true;
        agent.speed = MoveSpeed;
        agent.SetDestination(targetPos);
        this.MoveLocation = targetPos;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
    }

    public void WallAvoidance()
    {
        IsMoving = true;
        this.MoveLocation = WallNormal * LineLength;
        RaycastHit hit;
        Debug.DrawLine(transform.position, transform.position + transform.forward * LineLength, Color.red);
        Debug.DrawLine(transform.position, transform.position + (transform.forward + transform.right) * LineLength, Color.blue);
        Debug.DrawLine(transform.position, transform.position + (transform.forward - transform.right) * LineLength, Color.green);

        if (Physics.Raycast(transform.position, transform.forward, out hit, LineLength, layerMask))
        {
            WallNormal = hit.normal;
        }
        else if (Physics.Raycast(transform.position, transform.forward + transform.right, out hit, LineLength, layerMask))
        {
            WallNormal = hit.normal;
        }
        else if (Physics.Raycast(transform.position, transform.forward - transform.right, out hit, LineLength, layerMask))
        {
            WallNormal = hit.normal;
        }
        
    }

    public void StopMovement()
    {
        IsMoving = false;
    }

    public void ExecuteBT()
    {
        BTRootNode.Execute();
    }

    bool ReachedLastPoint()
    {
        bool rv = false;
        // The line below was checking the distance between the agent and the last point
        // but I feel like this is a better way of doing it.
        if (currentWaypoint == waypointArray[waypointArray.Length - 1].transform)
        {
            rv = true;
        }
        return rv;
    }
    #endregion
}

#region Actions
public class RobotWaitTillAtLocation : BTNode
{
    private RobotBB zBB;
    private RobotMovementBT robotRef;

    public RobotWaitTillAtLocation(Blackboard bb, RobotMovementBT zombay) : base(bb)
    {
        zBB = (RobotBB)bb;
        robotRef = zombay;
    }

    public override BTStatus Execute()
    {
        BTStatus rv = BTStatus.RUNNING;
        if ((robotRef.transform.position - zBB.MoveToLocation).magnitude <= 1.0f)
        {
            Debug.Log("Reached target");
            rv = BTStatus.SUCCESS;
        }
        return rv;
    }
}

public class RobotWaypoint : BTNode
{
    private RobotBB zBB;
    private RobotMovementBT robotRef;
    bool FirstRun = true;
    public RobotWaypoint(Blackboard bb, RobotMovementBT zombay) : base(bb)
    {
        zBB = (RobotBB)bb;
        robotRef = zombay;
    }

    public override BTStatus Execute()
    {
        bool ReachedLastPoint()
        {
            bool rv = false;
            if ((robotRef.transform.position - robotRef.waypointArray[robotRef.waypointArray.Length - 1].transform.position).magnitude <= 1.0f)
            {
                rv = true;
            }
            return rv;
        };
        if (FirstRun)
        {
            zBB.CurrentTarget = "Bomb";
            FirstRun = false;
            Debug.Log("Moving to target");
        }
        BTStatus rv = BTStatus.RUNNING;
        robotRef.Waypoint();
        if (ReachedLastPoint())
        {
            Debug.Log("Reached the target");
            rv = BTStatus.SUCCESS;
            FirstRun = true;
        }
        return rv;
    }
}

public class RobotFindBomb : BTNode
{
    private RobotBB zBB;
    private RobotMovementBT robotRef;
    bool FirstRun = true;
    public RobotFindBomb(Blackboard bb, RobotMovementBT zombay) : base(bb)
    {
        zBB = (RobotBB)bb;
        robotRef = zombay;
    }

    public override BTStatus Execute()
    {
        
        if (FirstRun)
        {
            zBB.CurrentTarget = "Bomb";
            FirstRun = false;
        }
        BTStatus rv = BTStatus.RUNNING;
        robotRef.Pathfinding(zBB.BombLocation);
        if (Vector3.Distance(zBB.BombLocation, robotRef.transform.position) < 1.0f)
        {
            rv = BTStatus.SUCCESS;
            FirstRun = true;
        }
        return rv;
    }
}

public class RobotFindHatch : BTNode
{
    private RobotBB zBB;
    private RobotMovementBT robotRef;
    bool FirstRun = true;
    public RobotFindHatch(Blackboard bb, RobotMovementBT zombay) : base(bb)
    {
        zBB = (RobotBB)bb;
        robotRef = zombay;
    }

    public override BTStatus Execute()
    {

        if (FirstRun)
        {
            zBB.CurrentTarget = "Hatch";
            FirstRun = false;
        }
        BTStatus rv = BTStatus.RUNNING;
        robotRef.Pathfinding(zBB.HatchLocation);
        if (Vector3.Distance(zBB.HatchLocation, robotRef.transform.position) < 1.0f)
        {
            rv = BTStatus.SUCCESS;
            FirstRun = true;
        }
        return rv;
    }
}

public class RobotMoveToPlayer : BTNode
{
    private RobotBB zBB;
    private RobotMovementBT robotRef;
    bool FirstRun = true;
    public RobotMoveToPlayer(Blackboard bb, RobotMovementBT zombay) : base(bb)
    {
        zBB = (RobotBB)bb;
        robotRef = zombay;
    }

    public override BTStatus Execute()
    {
        if (FirstRun)
        {
            FirstRun = false;
            zBB.CurrentTarget = "Player";
            Debug.Log("Moving to player");
            // perhaps the BTNode should have some "start" function that
            // can be overridden in child classes so we don't have to do this?
        }
        BTStatus rv = BTStatus.RUNNING;
        robotRef.transform.LookAt(zBB.PlayerLocation);
        robotRef.RobotMoveTo(zBB.PlayerLocation);
        if ((robotRef.transform.position - zBB.PlayerLocation).magnitude <= 10.0f)
        {
            Debug.Log("Reached the player");
            rv = BTStatus.SUCCESS;
            FirstRun = true;
        }
        return rv;
    }

    public override void Reset()
    {
        base.Reset();
        FirstRun = true;
    }
}

public class RobotMoveToBomb : BTNode
{
    private RobotBB zBB;
    private RobotMovementBT robotRef;
    bool FirstRun = true;
    public RobotMoveToBomb(Blackboard bb, RobotMovementBT zombay) : base(bb)
    {
        zBB = (RobotBB)bb;
        robotRef = zombay;
    }

    public override BTStatus Execute()
    {
        if (FirstRun)
        {
            zBB.CurrentTarget = "Bomb";
            FirstRun = false;
            Debug.Log("Moving to target");
        }
        BTStatus rv = BTStatus.RUNNING;
        robotRef.RobotMoveTo(zBB.BombLocation);
        if ((robotRef.transform.position - zBB.BombLocation).magnitude <= 1.0f)
        {
            Debug.Log("Reached the target");
            rv = BTStatus.SUCCESS;
            FirstRun = true;
        }
        return rv;
    }
}

public class RobotMoveToHatch : BTNode
{
    private RobotBB zBB;
    private RobotMovementBT robotRef;
    bool FirstRun = true;
    public RobotMoveToHatch(Blackboard bb, RobotMovementBT zombay) : base(bb)
    {
        zBB = (RobotBB)bb;
        robotRef = zombay;
    }

    public override BTStatus Execute()
    {
        if (FirstRun)
        {
            zBB.CurrentTarget = "Hatch";
            FirstRun = false;
            Debug.Log("Moving to target");
        }
        BTStatus rv = BTStatus.RUNNING;
        robotRef.RobotMoveTo(zBB.HatchLocation);
        if ((robotRef.transform.position - zBB.HatchLocation).magnitude <= 1.0f)
        {
            Debug.Log("Reached the target");
            rv = BTStatus.SUCCESS;
            FirstRun = true;
        }
        return rv;
    }
}

public class RobotStopMovement : BTNode
{
    private RobotMovementBT robotRef;
    public RobotStopMovement(Blackboard bb, RobotMovementBT zombay) : base(bb)
    {
        robotRef = zombay;
    }

    public override BTStatus Execute()
    {
        Debug.Log("Stopped Moving");
        robotRef.StopMovement();
        return BTStatus.SUCCESS;
    }
}

// RobotArriveAtHatch is unused for now. //
public class RobotArriveAtHatch : BTNode
{
    private RobotMovementBT robotRef;
    public RobotArriveAtHatch(Blackboard bb, RobotMovementBT zombay) : base(bb)
    {
        robotRef = zombay;
    }

    public override BTStatus Execute()
    {
        float slowingSpeed = 1f;
        robotRef.MoveSpeed = slowingSpeed;
        return BTStatus.SUCCESS;
    }
}

public class RobotWander : BTNode
{
    private RobotMovementBT robotRef;
    private RobotBB rBB;
    public float WanderRadius = 10f;
    public float WanderDistance = 10f;
    public float WanderJitter = 1f;
    Vector3 WanderTarget = Vector3.zero;
    float WanderAngle = 0.0f;

    public Vector3 Position;
    public Vector3 Heading;

    public RobotWander(Blackboard bb, RobotMovementBT zombay) : base(bb)
    {
        rBB = (RobotBB)bb;
        robotRef = zombay;
        //WanderAngle = Mathf.Lerp(WanderAngle, WanderAngle, 0.1f);
    }

    public override BTStatus Execute()
    {
        rBB.CurrentTarget = "Wandering around";
        Position = robotRef.transform.position;
        Heading = robotRef.transform.forward;
        WanderAngle = Random.Range(0.0f, Mathf.PI * 2);
        WanderTarget = new Vector3(Mathf.Cos(WanderAngle), 0, Mathf.Sin(WanderAngle)) * WanderRadius;

        Vector3 targetWorld = Position + WanderTarget;

        targetWorld += Heading * WanderDistance;

        robotRef.RobotMoveTo(targetWorld);
        //Debug.Log("Moving to " + (targetWorld));

        return BTStatus.SUCCESS;
    }
}

#endregion

#region Decorators
public class BombPickedDecorator : ConditionalDecorator
{
    private RobotMovementBT robotRef;
    RobotBB zBB;
    public BombPickedDecorator(BTNode WrappedNode, Blackboard bb, RobotMovementBT zombay) : base(WrappedNode, bb)
    {
        robotRef = zombay;
        zBB = (RobotBB)bb;
    }

    public override bool CheckStatus()
    {

        return zBB.Bomb.GetComponent<Bomb>().bombBeingCarried && !robotRef.CanWander;
    }
}

public class BombDroppedDecorator : ConditionalDecorator
{
    RobotBB zBB;
    public BombDroppedDecorator(BTNode WrappedNode, Blackboard bb) : base(WrappedNode, bb)
    {
        zBB = (RobotBB)bb;
    }

    public override bool CheckStatus()
    {
        return !zBB.Bomb.GetComponent<Bomb>().bombBeingCarried;
    }
}

public class PlayerDistanceDecorator : ConditionalDecorator
{
    RobotBB zBB;
    public PlayerDistanceDecorator(BTNode WrappedNode, Blackboard bb) : base(WrappedNode, bb)
    {
        zBB = (RobotBB)bb;
    }

    public override bool CheckStatus()
    {
        bool PlayerIsClose = false;

        float PlayerDistance = (zBB.Player.transform.position - zBB.transform.position).magnitude;

        if (PlayerDistance < 12f) { PlayerIsClose = true; }
        //Might create a variable for the if statement instead of hard-coding a value.

        return PlayerIsClose;
    }
}

public class HatchDistanceDecorator : ConditionalDecorator
{
    RobotBB zBB;
    float hatchDistance = 2.0f;
    public HatchDistanceDecorator(BTNode WrappedNode, Blackboard bb) : base(WrappedNode, bb)
    {
        zBB = (RobotBB)bb;
    }

    public override bool CheckStatus()
    {
        bool closeToHatch = false;

        float HatchDistance = (zBB.Hatch.transform.position - zBB.transform.position).magnitude;

        if (HatchDistance < hatchDistance) { closeToHatch = true; }
        //Might create a variable for the if statement instead of hard-coding a value.

        return closeToHatch;
    }
}
#endregion
