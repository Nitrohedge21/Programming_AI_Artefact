using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieBB : Blackboard {

    [HideInInspector] public Vector3 MoveToLocation;      //This is used by zombie script, might replace it later.
    public Vector3 PlayerLocation;
    public Vector3 BombLocation;
    public Vector3 TargetLocation;

    [HideInInspector] public int PlayerHealth = 2;

    public GameObject Player;
    public GameObject Bomb;
    public string CurrentTarget;


    void Update ()
    {
        //This part should probably be a decorator.
        float BombDistance = (Bomb.transform.position - this.transform.position).magnitude;
        float PlayerDistance = (Player.transform.position - this.transform.position).magnitude;
        //For my project, i can just use the location of the red team but there probably is a better way of making it work for multiple objects.
        if ( PlayerDistance < BombDistance)
        { TargetLocation = PlayerLocation; CurrentTarget = "Player"; } else { TargetLocation = BombLocation; CurrentTarget = "Bomb"; }

        //if ()

        PlayerLocation = Player.transform.position;
        BombLocation = Bomb.transform.position;
        //TargetLocation = Target.transform.position;
    }
}
