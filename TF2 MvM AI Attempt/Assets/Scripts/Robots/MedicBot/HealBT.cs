using UnityEngine;

//   TODO:
// - Get rid of the RobotBB or make a seperate one if necessary.
[RequireComponent(typeof(RobotBB))]
public class HealBT : MonoBehaviour
{
    #region AI Stuff
    [SerializeField] private GameObject[] teamMembers;
    private GameObject beamObject;
    private LineRenderer mediBeam;
    public GameObject closestMember;
    public Vector3 ClosestMemberPos = Vector3.zero;
    [Range(0.0f, 10.0f)] public float detectionRange;
    [Range(0.0f, 5.0f)] public float healRange;

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

        CompositeNode Heal = new Sequence(bb);
        Heal.AddChild(new MedicHeal(bb, this));

        rootChild.AddChild(Heal);

        //Execute our BT every 0.1 seconds
        InvokeRepeating("ExecuteBT", 0.1f, 0.1f);
    }

    #region Functions
    void Update()
    {
        FindClosest();
        ClosestMemberPos = closestMember.transform.position;
        if (mediBeam.enabled) 
        {
            mediBeam.SetPosition(0, transform.position);
            mediBeam.SetPosition(1, closestMember.transform.position);
        }
        

        if (IsMoving)
        {
            Vector3 dir = MoveLocation - transform.position;
            transform.position += dir.normalized * MoveSpeed * Time.deltaTime;
        }
    }

    private GameObject FindClosest()
    {
        closestMember = null;
        
        foreach (GameObject member in teamMembers)
        {
            float distance = (member.transform.position - transform.position).magnitude;
            if (gameObject != member && distance < detectionRange)
            {
                if(closestMember != null  && Vector3.Distance(closestMember.transform.position, transform.position) > distance)
                {
                    closestMember = member;
                    detectionRange = 10.0f;
                }
                else if (closestMember == null) { closestMember = member; detectionRange = 10.0f; }
            }
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

    public void HealClosest()
    {
        if (ClosestExists() && (Vector3.Distance(transform.position, ClosestMemberPos) < healRange))
        {
            Health closestHealthComp = closestMember.GetComponent<Health>();
            mediBeam.enabled = true;
            

            if (closestHealthComp.currentHealth < closestHealthComp.maxHealth)
            {
                time += 10f * Time.deltaTime;
                //The value added to the time was changed to 10 to make the healing faster.
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
public class MedicHeal : BTNode
{
    private RobotBB zBB;
    private HealBT robotRef;
    bool FirstRun = true;
    public MedicHeal(Blackboard bb, HealBT zombay) : base(bb)
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
        }
        BTStatus rv = BTStatus.RUNNING;
        float oldHP = robotRef.GetComponent<Health>().currentHealth;
        robotRef.HealClosest();
        if(robotRef.GetComponent<Health>().currentHealth > oldHP)
        {
            rv = BTStatus.SUCCESS;
        }
        return rv;
    }

    public override void Reset()
    {
        base.Reset();
        FirstRun = true;
    }
}

#endregion

