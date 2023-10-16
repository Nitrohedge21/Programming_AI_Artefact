using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This is our Zombie Character. It requires a Zombie Blackboard (ZombieBB) component
/// </summary>
[RequireComponent(typeof(ZombieBB))]
public class Zombie : MonoBehaviour {

    public float MoveSpeed = 10.0f;

    private Vector3 MoveLocation;
    private bool IsMoving = false;

    private BTNode BTRootNode;
    // Use this for initialization
    void Start()
    {
        MoveLocation = transform.position;

        //CREATING OUR ZOMBIE BEHAVIOUR TREE

        //Get reference to Zombie Blackboard
        ZombieBB bb = GetComponent<ZombieBB>();

        //Create our root selector
        Selector rootChild = new Selector(bb); // selectors will execute it's children 1 by 1 until one of them succeeds
        BTRootNode = rootChild;

        CompositeNode CustomSequence = new Sequence(bb);
        CustomDecorator customRoot = new CustomDecorator(CustomSequence, bb,this);
        //Decorators are optional, they do not require to be used to make the sequence happen
        CustomSequence.AddChild(new ZombieMoveToTarget(bb, this));
        CustomSequence.AddChild(new ZombieStopMovement(bb, this));
        CustomSequence.AddChild(new ZombiePickUpBomb(bb, this));

        //Adding to root selector
        rootChild.AddChild(customRoot);

        
        //Execute our BT every 0.1 seconds
        InvokeRepeating("ExecuteBT", 0.1f, 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        if (IsMoving)
        {
            Vector3 dir = MoveLocation - transform.position;
            transform.position += dir.normalized * MoveSpeed * Time.deltaTime;
        }
    }

    public void ZombieMoveTo(Vector3 MoveLocation)
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
}

public class ZombieWaitTillAtLocation : BTNode
{
    private ZombieBB zBB;
    private Zombie zombieRef;

    public ZombieWaitTillAtLocation(Blackboard bb, Zombie zombay) : base(bb)
    {
        zBB = (ZombieBB)bb;
        zombieRef = zombay;
    }

    public override BTStatus Execute()
    {
        BTStatus rv = BTStatus.RUNNING;
        if ((zombieRef.transform.position - zBB.MoveToLocation).magnitude <= 1.0f)
        {
            Debug.Log("Reached target");
            rv = BTStatus.SUCCESS;
        }
        return rv;
    }
}

public class ZombieMoveToTarget : BTNode
{
    private ZombieBB zBB;
    private Zombie zombieRef;
    bool FirstRun = true;
    public ZombieMoveToTarget(Blackboard bb, Zombie zombay) : base(bb)
    {
        zBB = (ZombieBB)bb;
        zombieRef = zombay;
    }

    public override BTStatus Execute()
    {
        if (FirstRun)
        {
            FirstRun = false;
            Debug.Log("Moving to target");
        }
        BTStatus rv = BTStatus.RUNNING;
        zombieRef.ZombieMoveTo(zBB.TargetLocation);
        if ((zombieRef.transform.position - zBB.TargetLocation).magnitude <= 1.0f)
        {
            Debug.Log("Reached the target");
            rv = BTStatus.SUCCESS;
            FirstRun = true;
        }
        //Might make it so that if it fails the task, it goes to a random location or a third set location.
        //The zombie stops moving if none of the targets are around.
        return rv;
    }
}

public class ZombiePickUpBomb : BTNode
{
    private ZombieBB zBB;
    private Zombie zombieRef;
    private bool bombBeingCarried;
    public ZombiePickUpBomb(Blackboard bb, Zombie zombay) : base(bb)
    {
        zBB = (ZombieBB)bb;
        zombieRef = zombay;
    }
    BTStatus rv = BTStatus.RUNNING;
    public override BTStatus Execute()
    {
        if ((zombieRef.transform.position - zBB.BombLocation).magnitude <= 1.0f)
        {
            Debug.Log("Picked up the bomb");
            bombBeingCarried = true;
            rv = BTStatus.SUCCESS;
        }
        //Make it actually pick up the bomb here

        return rv;
    }
}

public class CustomDecorator : ConditionalDecorator
{
    ZombieBB zBB;
    private Zombie zombieRef;
    public CustomDecorator(BTNode WrappedNode, Blackboard bb, Zombie zombay) : base(WrappedNode, bb)
    {
        zBB = (ZombieBB)bb;
        zombieRef = zombay;
    }

    public override bool CheckStatus()
    {
        
        float TargetDistance = (zombieRef.transform.position - zBB.TargetLocation).magnitude;
        float PlayerDistance = (zombieRef.transform.position - zBB.PlayerLocation).magnitude;
        bool PlayerTooFar = false;
        if(TargetDistance < PlayerDistance) { PlayerTooFar = true;  }

        return PlayerTooFar;
    }
}

public class ZombieStopMovement : BTNode
{
    private Zombie zombieRef;
    public ZombieStopMovement(Blackboard bb, Zombie zombay) : base(bb)
    {
        zombieRef = zombay;
    }

    public override BTStatus Execute()
    {
        zombieRef.StopMovement();
        return BTStatus.SUCCESS;
    }
}
