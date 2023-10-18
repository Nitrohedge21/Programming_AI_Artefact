using UnityEngine;

public class NormalMovement : MonoBehaviour
{
    private CharacterController CharacterController;
    protected Transform Camera;
    protected Rigidbody Rb;
    protected CapsuleCollider Collider;
    [SerializeField] protected float Speed = 6f;
    protected float RotationRate = 10f;
    protected float JumpForce;
    protected Vector3 Velocity;
    protected bool GroundedPlayer;
    protected float JumpHeight = 1.0f;
    protected float GravityValue = -9.81f;

    void Start()
    {
        CharacterController = GetComponent<CharacterController>();
        Rb = GetComponent<Rigidbody>();
        Collider = GetComponent<CapsuleCollider>();
        Camera = GameObject.Find("Main Camera").transform;
    }

    void Update()
    {
        GroundedPlayer = CharacterController.isGrounded;
        if (GroundedPlayer && Velocity.y < 0)
        {
            Velocity.y = 0f;
        }

        Vector3 Move = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));    //These could be switched to GetAxis to make them more slippery.
        Vector3 Direction = Move.normalized;

        CharacterController.Move(Direction * Time.deltaTime * Speed);

        if (Direction != Vector3.zero)
        {
            Quaternion DesiredRotation = Quaternion.LookRotation(Direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, DesiredRotation, RotationRate * Time.deltaTime);
        }

        if (Input.GetButtonDown("Jump") && GroundedPlayer)
        {
            Velocity.y += Mathf.Sqrt(JumpHeight * -3.0f * GravityValue);
        }

        Velocity.y += GravityValue * Time.deltaTime;
        CharacterController.Move(Velocity * Time.deltaTime);
    }

}