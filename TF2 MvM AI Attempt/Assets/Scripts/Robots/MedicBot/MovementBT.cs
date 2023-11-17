using UnityEngine;

//   TODO:
// - Get rid of the RobotBB or make a seperate one if necessary.
[RequireComponent(typeof(RobotBB))]
public class MovementBT : MonoBehaviour
{
    #region AI Stuff
    [SerializeField] private GameObject[] teamMembers;
    public GameObject closestMember;
    public Vector3 ClosestMemberPos = Vector3.zero;
    [Range(0.0f, 30.0f)] public float followRange;

    public float MoveSpeed = 10.0f;
    private Vector3 MoveLocation;
    private bool IsMoving = false;
    private BTNode BTRootNode;
    #endregion

    void Start()
    {
        #region Reference Initializations
        teamMembers = GameObject.FindGameObjectsWithTag("Player");
        MoveLocation = transform.position;
        #endregion

        RobotBB bb = GetComponent<RobotBB>();

        Selector rootChild = new Selector(bb);
        BTRootNode = rootChild;

        CompositeNode Follow = new Sequence(bb);
        Follow.AddChild(new MedicFollow(bb, this));
        Follow.AddChild(new MedicStopMovement(bb, this));

        rootChild.AddChild(Follow);

        //Execute our BT every 0.1 seconds
        InvokeRepeating("ExecuteBT", 0.1f, 0.1f);
    }

    #region Functions
    void Update()
    {
        FindClosest();
        ClosestMemberPos = closestMember.transform.position;

        if (IsMoving)
        {
            Vector3 dir = MoveLocation - transform.position;
            transform.position += dir.normalized * MoveSpeed * Time.deltaTime;
        }
    }

    public void RobotMoveTo(Vector3 MoveLocation)
    {
        IsMoving = true;
        transform.LookAt(MoveLocation);
        this.MoveLocation = MoveLocation;
        
        //The line above fixes the issue with Y axis but it also breaks the rotation completely.
        transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, 0, transform.rotation.z));
    }

    public void StopMovement()
    {
        IsMoving = false;
    }

    private GameObject FindClosest()
    {
        closestMember = null;
        
        foreach (GameObject member in teamMembers)
        {
            float distance = (member.transform.position - transform.position).magnitude;
            if (gameObject != member && distance < followRange)
            {
                if(closestMember != null  && Vector3.Distance(closestMember.transform.position, transform.position) > distance)
                {
                    closestMember = member;
                    followRange = 10.0f;
                }
                else if (closestMember == null) { closestMember = member; followRange = 10.0f; }
            }
        }
        if (gameObject.layer == 8 && closestMember != null)
        {
            Debug.Log("closest object is " + closestMember.name);
        }

        return closestMember;
    }

    public void ExecuteBT()
    {
        BTRootNode.Execute();
    }
    #endregion

}

#region Actions
public class MedicFollow : BTNode
{
    private RobotBB zBB;
    private MovementBT robotRef;
    bool FirstRun = true;
    public MedicFollow(Blackboard bb, MovementBT zombay) : base(bb)
    {
        zBB = (RobotBB)bb;
        robotRef = zombay;
    }

    public override BTStatus Execute()
    {
        if (FirstRun)
        {
            FirstRun = false;
            zBB.CurrentTarget = robotRef.closestMember.name;
            Debug.Log("Moving to " + robotRef.closestMember.name);
        }
        BTStatus rv = BTStatus.RUNNING;
        robotRef.transform.LookAt(robotRef.closestMember.transform);
        robotRef.RobotMoveTo(robotRef.ClosestMemberPos);
        if ((robotRef.transform.position - robotRef.ClosestMemberPos).magnitude <= 2.0f)
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
}

public class MedicStopMovement : BTNode
{
    private MovementBT robotRef;
    public MedicStopMovement(Blackboard bb, MovementBT zombay) : base(bb)
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
#endregion

