using UnityEngine;


//   TODO:
// - Get rid of the RobotBB or make a seperate one if necessary.
// - Figure out why distanceRadius isn't working as it was in the previous one.
[RequireComponent(typeof(RobotBB))]
public class MedicAI : MonoBehaviour
{
    #region AI Stuff
    [SerializeField] private GameObject[] teamMembers;
    private GameObject beamObject;
    private LineRenderer mediBeam;
    public GameObject closestMember;
    public Vector3 ClosestMemberPos = Vector3.zero;
    [Range(0.0f, 5.0f)] public float distanceRadius;

    public float MoveSpeed = 10.0f;
    private Vector3 MoveLocation;
    private bool IsMoving = false;
    private BTNode BTRootNode;
    #endregion

    #region Timer Stuff
    float time = 0f;
    float timeDelay = 1f;
    #endregion

    void Start()
    {
        #region Reference Initializations
        teamMembers = GameObject.FindGameObjectsWithTag("Player");
        beamObject = GameObject.Find("MediBeam");
        mediBeam = beamObject.GetComponent<LineRenderer>();
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
        HealClosest();
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
        float closestDistance = Mathf.Infinity;
        closestMember = null;

        foreach (GameObject member in teamMembers)
        {
            float distance = (member.transform.position - transform.position).magnitude;
            if (gameObject != member && distance < distanceRadius)
            {
                closestDistance = distance;
                closestMember = member;
            }
            //The line above isn't what I want but it makes it follow when it get's outta it's radius as well.
            else if (gameObject != member) { closestMember = member; }
        }
        if (gameObject.layer == 8 && closestMember != null)
        {
            Debug.Log("closest object is " + closestMember.name);
        }

        return closestMember;
    }

    private bool ClosestExists()
    {
        bool rv = false;
        if (gameObject.layer == 8 && closestMember != null)
        {
            rv = true;
        }
        return rv;
    }

    private void HealClosest()
    {
        if (ClosestExists() && (Vector3.Distance(transform.position, ClosestMemberPos) < distanceRadius))
        {
            Health closestHealthComp = closestMember.GetComponent<Health>();
            mediBeam.enabled = true;
            mediBeam.SetPosition(0, transform.position);
            mediBeam.SetPosition(1, closestMember.transform.position);

            if (closestHealthComp.currentHealth < closestHealthComp.maxHealth)
            {
                time += 1f * Time.deltaTime;
                //TODO: Figure out how to delay this.
                if (time >= timeDelay)
                {
                    closestMember.GetComponent<Health>().Heal(10);
                    MoveSpeed = closestMember.GetComponent<Movement>().Speed - 1f;
                    time = 0f;
                }
            }
        }
        else { mediBeam.enabled = false; MoveSpeed = 4f; }
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
    private MedicAI robotRef;
    bool FirstRun = true;
    public MedicFollow(Blackboard bb, MedicAI zombay) : base(bb)
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
    private MedicAI robotRef;
    public MedicStopMovement(Blackboard bb, MedicAI zombay) : base(bb)
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

