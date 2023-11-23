using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This is our Robot Character. It requires a Robot Blackboard (ZombieBB) component
/// </summary>
[RequireComponent(typeof(RobotBB))]
public class Robot : MonoBehaviour {
    
    #region General Stuff
    public float MoveSpeed = 10.0f;

    private Vector3 MoveLocation;
    private bool IsMoving = false;
    public bool CanSeparate = true;
    public bool CanWander = false;
    private Vector3 ToOurAgent;
    float Distance;

    private bool IsShooting = false;
    private BTNode BTRootNode;
    #endregion

    #region Waypoint Pathfinding Stuff
    [SerializeField] private float distance = 3f;
    public Transform currentWaypoint;
    [SerializeField] public GameObject[] waypointArray;
    [SerializeField] List<GameObject> Visited = new List<GameObject>();
    private Vector3 directionToWaypoint;
    [SerializeField] private Waypoints waypoints;
    #endregion

    #region Timer Stuff
    float time = 0f;
    float timeDelay = 1f;
    #endregion

    // Use this for initialization
    void Start()
    {
        currentWaypoint = waypoints.GetNextWaypoint(currentWaypoint);
        MoveLocation = transform.position;

        //CREATING OUR Robot BEHAVIOUR TREE

        //Get reference to Robot Blackboard
        RobotBB bb = GetComponent<RobotBB>();

        //Create our root selector
        Selector rootChild = new Selector(bb); // selectors will execute it's children 1 by 1 until one of them succeeds
        BTRootNode = rootChild;

        CompositeNode PickUpBomb = new Sequence(bb);
        BombDroppedDecorator bombDecoRoot = new BombDroppedDecorator(PickUpBomb, bb);
        PickUpBomb.AddChild(new RobotPF(bb, this));
        //PickUpBomb.AddChild(new RobotMoveToBomb(bb, this));
        PickUpBomb.AddChild(new RobotStopMovement(bb, this));

        CompositeNode TargetHatch = new Sequence(bb);
        BombPickedDecorator hatchRoot = new BombPickedDecorator(TargetHatch, bb, this);
        TargetHatch.AddChild(new RobotPF(bb, this));
        //TargetHatch.AddChild(new RobotMoveToHatch(bb, this));
        TargetHatch.AddChild(new RobotStopMovement(bb, this));

        //TODO: Make it move randomly after the bomb has been dropped.
        //This could either be in this sequence or another sequence

        CompositeNode Wander = new Sequence(bb);
        Wander.AddChild(new RobotWander(bb, this));

        //Adding to root selector
        rootChild.AddChild(bombDecoRoot);
        rootChild.AddChild(hatchRoot);
        rootChild.AddChild(Wander);


        //Execute our BT every 0.1 seconds
        InvokeRepeating("ExecuteBT", 0.1f, 0.1f);
    }
    #region Functions
    // Update is called once per frame
    void Update()
    {
        waypointArray = GameObject.FindGameObjectsWithTag("Waypoints");
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
        if (IsMoving)
        {
            Vector3 dir = MoveLocation - transform.position;
            transform.position += dir.normalized * MoveSpeed * Time.deltaTime;
            Seperation(TaggedRobots);
        }
        ShootPlayer();
    }

    public void RobotMoveTo(Vector3 MoveLocation)
    {
        IsMoving = true;
        if(!IsShooting) { transform.LookAt(MoveLocation); }
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

    public void Pathfinding()
    {
        IsMoving = true;
        RobotMoveTo(currentWaypoint.position);

        foreach (GameObject point in waypointArray)
        {
            if(!ReachedLastPoint())
            {
                if (Visited.Contains(point))
                {
                    continue;
                }
                if (point != currentWaypoint && Vector3.Distance(transform.position, currentWaypoint.position) < distance)
                {
                    currentWaypoint = point.transform;
                    RobotMoveTo(point.transform.position);
                    if (Vector3.Distance(currentWaypoint.position, transform.position) < 3f)
                    {
                        Visited.Add(currentWaypoint.gameObject);
                    }
                }
                else if (point != currentWaypoint && Vector3.Distance(transform.position, currentWaypoint.position) > Vector3.Distance(transform.position, point.transform.position))
                {
                    currentWaypoint = point.transform;
                    RobotMoveTo(point.transform.position);
                    if (Vector3.Distance(currentWaypoint.position, transform.position) < 3f)
                    {
                        Visited.Add(currentWaypoint.gameObject);
                    }
                }
            }

        }
    }

    public void ShootPlayer()
    {
        RobotBB rBB = GetComponent<RobotBB>();
        if ((transform.position - rBB.PlayerLocation).magnitude <= 12.0f)
        {
            transform.LookAt(rBB.Player.transform);
            time += 6f * Time.deltaTime;    //Increment the added float value to make it shoot faster.
            if (time >= timeDelay)
            {
                GetComponentInChildren<Minigun>().Shoot();
                time = 0f;
            }
            
            IsShooting = true;
        }
        else { IsShooting = false; }
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

    bool ReachedLastPoint()
    {
        bool rv = false;
        if ((transform.position - waypointArray[waypointArray.Length - 1].transform.position).magnitude <= distance)
        {
            rv = true;
        }
        return rv;
    }
}

#region Actions
public class RobotWaitTillAtLocation : BTNode
{
    private RobotBB zBB;
    private Robot robotRef;

    public RobotWaitTillAtLocation(Blackboard bb, Robot zombay) : base(bb)
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

public class RobotPF : BTNode
{
    private RobotBB zBB;
    private Robot robotRef;
    bool FirstRun = true;
    public RobotPF(Blackboard bb, Robot zombay) : base(bb)
    {
        zBB = (RobotBB)bb;
        robotRef = zombay;
    }

    public override BTStatus Execute()
    {
        bool ReachedLastPoint()
        {
            bool rv = false;
            if((robotRef.transform.position - robotRef.waypointArray[robotRef.waypointArray.Length - 1].transform.position).magnitude <= 1.0f)
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
        robotRef.Pathfinding();
        if (ReachedLastPoint())
        {
            Debug.Log("Reached the target");
            rv = BTStatus.SUCCESS;
            FirstRun = true;
        }
        return rv;
    }
}

public class RobotMoveToPlayer : BTNode
{
    private RobotBB zBB;
    private Robot robotRef;
    bool FirstRun = true;
    public RobotMoveToPlayer(Blackboard bb, Robot zombay) : base(bb)
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
    private Robot robotRef;
    bool FirstRun = true;
    public RobotMoveToBomb(Blackboard bb, Robot zombay) : base(bb)
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
    private Robot robotRef;
    bool FirstRun = true;
    public RobotMoveToHatch(Blackboard bb, Robot zombay) : base(bb)
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
    private Robot robotRef;
    public RobotStopMovement(Blackboard bb, Robot zombay) : base(bb)
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

public class RobotShootPlayer : BTNode
{
    private RobotBB zBB;
    private Robot robotRef;
    bool FirstRun = true;
    public RobotShootPlayer(Blackboard bb, Robot zombay) : base(bb)
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
            Debug.Log("Shooting at player");
        }
        BTStatus rv = BTStatus.RUNNING;
        if ((robotRef.transform.position - zBB.PlayerLocation).magnitude <= 12.0f)
        {
            robotRef.transform.LookAt(zBB.PlayerLocation);
            robotRef.GetComponentInChildren<Minigun>().Shoot();
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

// RobotArriveAtHatch is unused for now. //
public class RobotArriveAtHatch : BTNode
{
    private Robot robotRef;
    public RobotArriveAtHatch(Blackboard bb, Robot zombay) : base(bb)
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
    private Robot robotRef;
    private RobotBB rBB;
    public float WanderRadius = 10f;
    public float WanderDistance = 10f;
    public float WanderJitter = 1f;
    Vector3 WanderTarget = Vector3.zero;
    float WanderAngle = 0.0f;

    public Vector3 Position;
    public Vector3 Heading;

    public RobotWander(Blackboard bb, Robot zombay) : base(bb)
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

/*public class RobotFollowWaypoints : BTNode
{
    private RobotBB zBB;
    private Robot robotRef;
    bool FirstRun = true;
    public RobotFollowWaypoints(Blackboard bb, Robot zombay) : base(bb)
    {
        zBB = (RobotBB)bb;
        robotRef = zombay;
    }

    public override BTStatus Execute()
    {
        if (FirstRun)
        {
            FirstRun = false;
            zBB.CurrentTarget = "Waypoints";
        }
        BTStatus rv = BTStatus.RUNNING;
        robotRef.RobotMoveTo(zBB.WaypointLocation);
        if ((robotRef.transform.position - zBB.WaypointLocation).magnitude <= 1.0f)
        {
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
}*/
#endregion

#region Decorators
public class BombPickedDecorator : ConditionalDecorator
{
    private Robot robotRef;
    RobotBB zBB;
    public BombPickedDecorator(BTNode WrappedNode, Blackboard bb, Robot zombay) : base(WrappedNode, bb)
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