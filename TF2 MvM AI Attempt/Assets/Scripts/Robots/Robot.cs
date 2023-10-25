using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This is our Robot Character. It requires a Robot Blackboard (ZombieBB) component
/// </summary>
[RequireComponent(typeof(RobotBB))]
public class Robot : MonoBehaviour {

    public float MoveSpeed = 10.0f;

    private Vector3 MoveLocation;
    private bool IsMoving = false;

    private BTNode BTRootNode;
    
    // Use this for initialization
    void Start()
    {
        MoveLocation = transform.position;

        //CREATING OUR Robot BEHAVIOUR TREE

        //Get reference to Robot Blackboard
        RobotBB bb = GetComponent<RobotBB>();

        //Create our root selector
        Selector rootChild = new Selector(bb); // selectors will execute it's children 1 by 1 until one of them succeeds
        BTRootNode = rootChild;

        CompositeNode PickUpBomb = new Sequence(bb);
        BombDroppedDecorator bombDecoRoot = new BombDroppedDecorator(PickUpBomb, bb);
        PickUpBomb.AddChild(new RobotMoveToBomb(bb, this));
        PickUpBomb.AddChild(new RobotStopMovement(bb, this));

        CompositeNode TargetPlayer = new Sequence(bb);
        PlayerDistanceDecorator playerDecoRoot = new PlayerDistanceDecorator(TargetPlayer, bb);
        TargetPlayer.AddChild(new RobotMoveToPlayer(bb, this));
        TargetPlayer.AddChild(new RobotStopMovement(bb, this));

        CompositeNode TargetHatch = new Sequence(bb);
        BombPickedDecorator hatchRoot = new BombPickedDecorator(TargetHatch, bb);
        TargetHatch.AddChild(new RobotMoveToHatch(bb, this));
        TargetHatch.AddChild(new RobotStopMovement(bb, this));

        //Adding to root selector
        rootChild.AddChild(playerDecoRoot);
        rootChild.AddChild(bombDecoRoot);
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
        robotRef.RobotMoveTo(zBB.PlayerLocation);
        if ((robotRef.transform.position - zBB.PlayerLocation).magnitude <= 1.0f)
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

        //GameObject[] robots = GameObject.FindGameObjectsWithTag("Robots");
        //foreach (GameObject robot in robots)
        //{
        //    if (zBB.bombBeingCarried == false) {  }
        //}

        //
        // TODO:
        // Might have to use an OnTriggerEnter to set "bombBeingCarried" to true so that not all the robots go for the bomb if it's picked up by another.

        if ((robotRef.transform.position - zBB.BombLocation).magnitude <= 1.0f)
        {
            Debug.Log("Reached the target");
            rv = BTStatus.SUCCESS;
            FirstRun = true;
            zBB.bombBeingCarried = true;
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
            zBB.bombBeingCarried = true;
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
        robotRef.StopMovement();
        return BTStatus.SUCCESS;
    }
}
#endregion

#region Decorators
public class BombPickedDecorator : ConditionalDecorator
{
    RobotBB zBB;
    public BombPickedDecorator(BTNode WrappedNode, Blackboard bb) : base(WrappedNode, bb)
    {
        zBB = (RobotBB)bb;
    }

    public override bool CheckStatus()
    {
        
        return zBB.Bomb.GetComponent<Bomb>().bombBeingCarried;
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
#endregion