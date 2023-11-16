using UnityEngine;

public class RobotBB : Blackboard {

    [HideInInspector] public Vector3 MoveToLocation;      //This is used by Robot script, might replace it later.
    public Vector3 PlayerLocation;
    public Vector3 BombLocation;
    public Vector3 HatchLocation;
    //public Vector3 WaypointLocation;    
    //Use this to store the waypoint locations and then use zombieMoveTo.

    [HideInInspector] public int PlayerHealth = 2;

    public GameObject Player;
    public GameObject Bomb;
    public GameObject Hatch;
    public GameObject WaypointObject;
    public GameObject BombCarrier;
    public string CurrentTarget;


    void Update ()
    {
        BombCarrier = Bomb.GetComponent<Bomb>().carrier;
        //WaypointLocation = WaypointObject.transform.position;
        PlayerLocation = Player.transform.position;
        BombLocation = Bomb.transform.position;
        HatchLocation = Hatch.transform.position;
    }
}
