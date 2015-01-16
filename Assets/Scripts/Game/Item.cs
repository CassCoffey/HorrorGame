using UnityEngine;
using System.Collections;

public class Item : MonoBehaviour {

    public Weapon weapon = null;
    public bool isEquipped = false;
    public GameObject equippedTo;

    private bool moving = false;
    private float startTime = 0;
    private float totalTime = 0;
    private Vector3 originPos;
    private Vector3 destinationPos;
    private Quaternion originRot;
    private Quaternion destinationRot;

    public bool CanSwitch()
    {
        if (weapon != null)
        {
            return !weapon.charging;
        }
        else return true;
    }

    public void PickUp(GameObject player)
    {
        networkView.RPC("SyncPickup", RPCMode.OthersBuffered, player.networkView.viewID);
        isEquipped = true;
        equippedTo = player;
        collider.enabled = false;
        collider.isTrigger = false;
        rigidbody.isKinematic = true;
        if (weapon != null)
        {
            weapon.isStuck = false;
        }
    }

    public void Drop()
    {
        networkView.RPC("SyncDrop", RPCMode.OthersBuffered);
        transform.localPosition = Vector3.zero;
        if (weapon != null)
        {
            weapon.charging = false;
        }
        EndLerp();
        isEquipped = false;
        equippedTo = null;
        transform.parent = null;
        collider.enabled = true;
        collider.isTrigger = false;
        rigidbody.isKinematic = false;
    }

    public void Use()
    {
        if (weapon != null)
        {
            networkView.RPC("SyncSwing", RPCMode.OthersBuffered);
            weapon.charging = false;
            transform.parent.GetComponent<Animator>().SetTrigger("Attack");
        }
        
    }

    public void AltUse()
    {
        if (weapon != null)
        {
            if (!weapon.charging)
            {
                weapon.Charge();
            }
            else
            {
                Debug.Log("Throwing");
                float throwForce = equippedTo.GetComponent<Player>().throwForce;
                equippedTo.GetComponent<Player>().DropItem();
                weapon.Throw(throwForce);
            }
        }
    }

    /// <summary>
    /// Moves the weapon smoothly to a location. 
    /// *Will eventually be deprecated with actual animations.*
    /// </summary>
    public void LerpTo(Vector3 location, Quaternion rotation, float time)
    {
        moving = true;
        startTime = Time.time;
        totalTime = time;
        originPos = transform.localPosition;
        destinationPos = location;
        originRot = transform.localRotation;
        destinationRot = rotation;
    }

    /// <summary>
    /// Ends the lerp early.
    /// </summary>
    public void EndLerp()
    {
        if (moving)
        {
            transform.localPosition = destinationPos;
            transform.localRotation = destinationRot;
            moving = false;
        }
    }

    /// <summary>
    /// If the weapon is being moved to a new position, handle Slerping.
    /// </summary>
    public void FixedUpdate()
    {
        if (moving)
        {
            float time = Time.time - startTime;
            float percentage = time / totalTime;
            transform.localPosition = Vector3.Slerp(originPos, destinationPos, percentage);
            transform.localRotation = Quaternion.Slerp(originRot, destinationRot, percentage);
            if (percentage >= 1.0f)
            {
                moving = false;
                transform.localRotation = destinationRot;
                transform.localPosition = destinationPos;
            }
        }
    }

    [RPC] void SyncPickup(NetworkViewID viewID)
    {
        GameObject player = NetworkView.Find(viewID).gameObject;
        isEquipped = true;
        equippedTo = player;
        collider.enabled = false;
        collider.isTrigger = false;
        rigidbody.isKinematic = true;
        if (weapon != null)
        {
            weapon.isStuck = false;
        }
    }

    [RPC] void SyncDrop()
    {
        EndLerp();
        isEquipped = false;
        equippedTo = null;
        transform.parent = null;
        collider.enabled = true;
        collider.isTrigger = false;
        rigidbody.isKinematic = false;
    }

    [RPC] void SyncSwing()
    {
        weapon.charging = false;
        transform.parent.GetComponent<Animator>().SetTrigger("Attack");
    }
}
