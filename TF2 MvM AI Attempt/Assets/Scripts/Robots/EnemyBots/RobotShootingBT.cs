using UnityEngine;

/// <summary>
/// This is our Robot Character. It requires a Robot Blackboard (ZombieBB) component
/// </summary>
[RequireComponent(typeof(RobotBB))]
public class RobotShootingBT : MonoBehaviour {
    
    #region General Stuff
    public bool IsShooting = false;
    private BTNode BTRootNode;
    #endregion
    #region Timer Stuff
    float time = 0f;
    float timeDelay = 1f;
    #endregion

    void Start()
    {
        //CREATING OUR Robot BEHAVIOUR TREE

        //Get reference to Robot Blackboard
        RobotBB bb = GetComponent<RobotBB>();

        //Create our root selector
        Selector rootChild = new Selector(bb); // selectors will execute it's children 1 by 1 until one of them succeeds
        BTRootNode = rootChild;

        CompositeNode Shoot = new Sequence(bb);
        Shoot.AddChild(new RobotShootPlayer(bb, this));

        rootChild.AddChild(Shoot);

        //Execute our BT every 0.1 seconds
        InvokeRepeating("ExecuteBT", 0.1f, 0.1f);
    }
    #region Functions

    void Update()
    {

    }

    public void ShootPlayer()
    {
        GetComponentInChildren<Minigun>().BulletDamage = Random.Range(1, 3);
        RobotBB rBB = GetComponent<RobotBB>();
        if ((transform.position - rBB.PlayerLocation).magnitude <= 12.0f)
        {
            transform.LookAt(rBB.Player.transform);
            time += 50f * Time.deltaTime;    //Increment the added float value to make it shoot faster.
            if (time >= timeDelay)
            {
                GetComponentInChildren<Minigun>().Shoot();
                time = 0f;
            }
        }
    }

    public void ExecuteBT()
    {
        BTRootNode.Execute();
    }
    #endregion

}

#region Actions

public class RobotShootPlayer : BTNode
{
    private RobotBB zBB;
    private RobotShootingBT robotRef;
    bool FirstRun = true;
    public RobotShootPlayer(Blackboard bb, RobotShootingBT zombay) : base(bb)
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
        }
        BTStatus rv = BTStatus.RUNNING;
        if ((robotRef.transform.position - zBB.PlayerLocation).magnitude <= 12.0f)
        {
            robotRef.transform.LookAt(zBB.PlayerLocation);
            robotRef.ShootPlayer();
            robotRef.IsShooting = true;
            FirstRun = true;
        }
        else { robotRef.IsShooting = false; rv = BTStatus.SUCCESS; }
        return rv;
    }

    public override void Reset()
    {
        base.Reset();
        FirstRun = true;
    }
}

#endregion

#region Decorators

// No decorators required. I think.

#endregion