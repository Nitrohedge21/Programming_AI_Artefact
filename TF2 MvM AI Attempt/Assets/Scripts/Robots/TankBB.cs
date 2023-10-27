using UnityEngine;

public class TankBB : Blackboard {

    public Vector3 HatchLocation;
    public Vector3 MoveLocation;
    //public Vector3 WaypointLocation;    
    //Use this to store the waypoint locations and then use zombieMoveTo.

    public GameObject Hatch;
    public string CurrentTarget;


    void Update ()
    {
        HatchLocation = Hatch.transform.position;
    }
}
