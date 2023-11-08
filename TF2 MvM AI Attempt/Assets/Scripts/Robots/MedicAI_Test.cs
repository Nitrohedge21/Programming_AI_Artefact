using UnityEngine;

public class MedicAI_Test : MonoBehaviour
{
    [SerializeField] private GameObject[] teamMembers;
    private GameObject beamObject;
    private LineRenderer mediBeam;
    private GameObject closestMember;
    [Range(0.0f, 10.0f)] public float distanceRadius;

    private void Start()
    {
        teamMembers = GameObject.FindGameObjectsWithTag("Player");
        beamObject = GameObject.Find("MediBeam");
        mediBeam = beamObject.GetComponent<LineRenderer>();
    }

    private void Update()
    {
        FindClosest();
        if(ClosestExists())
        {
            Health closestHealthComp = closestMember.GetComponent<Health>();
            mediBeam.enabled = true;
            mediBeam.SetPosition(0, transform.position);
            mediBeam.SetPosition(1, closestMember.transform.position);

            if (closestHealthComp.currentHealth <= closestHealthComp.maxHealth)
            {
                //TODO: Figure out how to delay this.
                closestMember.GetComponent<Health>().Heal(10);
            }
        }
        else { mediBeam.enabled = false; }
    }

    #region Custom Functions
    private GameObject FindClosest() 
    {
        float closestDistance = Mathf.Infinity;
        closestMember = null;

        foreach (GameObject member in teamMembers)
        {
            float distance = (member.transform.position - transform.position).magnitude;
            if(gameObject != member && distance < distanceRadius)
            {
                closestDistance = distance;
                closestMember = member;
            }
        }
        if(gameObject.layer == 8 && closestMember != null)
        {
            Debug.Log("closest object is " + closestMember.name);
        }
        
        return closestMember;
    }

    private bool ClosestExists() 
    {
        bool rv = false;
        if(gameObject.layer == 8 && closestMember != null)
        {
            rv = true;
        }
        return rv;
    }

    #endregion
}
