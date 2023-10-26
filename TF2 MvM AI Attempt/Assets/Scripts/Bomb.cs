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

    private void Update()
    {
        if(Input.GetKey(KeyCode.E))
        {
            //This function might be called upon the actual destruction of the carrier.
            //This line is for testing purposes only.
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
        //Get a reference to the previous parent and the current position to drop the bomb at before destroying the parent.
        //This somehow causes a bug where it gets rid of two robots at once even though there can be only one parent.
        Vector3 droppedPos = transform.position;
        GameObject oldParent = transform.parent.gameObject;
        transform.localScale = oldSize;
        transform.position = new Vector3(droppedPos.x, 1.5f, droppedPos.z);
        transform.parent = null;
        Destroy(oldParent.gameObject); //This line causes an error afterwards but it probably not a big deal.
        bombBeingCarried = false;
    }
}
