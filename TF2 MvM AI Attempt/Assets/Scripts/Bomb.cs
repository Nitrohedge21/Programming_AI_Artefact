using UnityEngine;

public class Bomb : MonoBehaviour
{
    //Collision check stuff can be done here and then referenced to the robots to do their thing.
    public bool bombBeingCarried = false;
    Vector3 oldSize = new Vector3(1f, 1f, 1f);
    Vector3 newSize = new Vector3(0.5f, 0.5f, 0.5f);
    Vector3 newPosition = new Vector3(-0.5f, 0f, 0f);

    private void OnTriggerEnter(Collider other)
    {
        
        if(other.gameObject.CompareTag("Robots") && bombBeingCarried == false) 
        {
            //Robots seem to be ignoring the boolean or something because they can take the bomb from the other robots.
            //Might have a seperate tag to the carrier robot to fix things up.
            AttachToCarrier(other);
            bombBeingCarried = true; 
            Debug.Log("Bomb has been picked up!");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Robots") && bombBeingCarried == true)
        {
            
            bombBeingCarried = false;
            Destroy(other.gameObject);
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            DetachFromCarrier();
        }
    }

    void AttachToCarrier(Collider other)
    {
        transform.parent = other.transform;
        transform.localScale = newSize;
        transform.position = other.transform.position + newPosition;
        other.GetComponent<Robot>().MoveSpeed = 2;
    }

    void DetachFromCarrier()
    {
        Vector3 droppedPos = transform.position;
        
        transform.localScale = oldSize;
        transform.position = droppedPos;
        transform.parent = null;
        //bombBeingCarried = false;
    }
}
