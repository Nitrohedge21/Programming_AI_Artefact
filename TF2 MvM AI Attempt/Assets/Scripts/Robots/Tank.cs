using UnityEngine;

[RequireComponent(typeof(TankBB))]
public class Tank : MonoBehaviour {

    public float MoveSpeed = 10.0f;

    private Vector3 MoveLocation;
    private bool IsMoving = false;

    private BTNode BTRootNode;
    
    void Start()
    {
        MoveLocation = transform.position;

        //CREATING OUR Robot BEHAVIOUR TREE

        //Get reference to Robot Blackboard
        TankBB bb = GetComponent<TankBB>();

        //Create our root selector
        Selector rootChild = new Selector(bb); // selectors will execute it's children 1 by 1 until one of them succeeds
        BTRootNode = rootChild;

        CompositeNode hatchRoot = new Sequence(bb);
        hatchRoot.AddChild(new TankMoveToHatch(bb, this));

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
