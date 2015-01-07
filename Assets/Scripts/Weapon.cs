using UnityEngine;
using System.Collections.Generic;

public class Weapon : MonoBehaviour {

    public bool isEquipped = false;
    public bool isStuck = false;
    public bool isSharp;
    public GameObject equippedTo;
    public int damage;

    private bool moving = false;
    private float startTime = 0;
    private float totalTime = 0;
    private Vector3 originPos;
    private Vector3 destinationPos;
    private Quaternion originRot;
    private Quaternion destinationRot;

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

    /// <summary>
    /// When the weapon collides with something, if it is moving fast enough it will deal damage.
    /// Also, if it is sharp it will stick.
    /// </summary>
    public void OnCollisionEnter(Collision collision)
    {
        if (!isEquipped && !isStuck && collision.collider.transform.parent != transform && !collision.collider.isTrigger && collision.collider.transform != transform && collision.relativeVelocity.magnitude > 15)
        {
            if (isSharp)
            {
                // Only stick to players and terrain.
                if (collision.collider.tag == "Player" || collision.collider.tag == "Terrain")
                {
                    collider.isTrigger = true;
                    GetComponent<Weapon>().isStuck = true;
                    ContactPoint contact = collision.contacts[0];
                    transform.position = contact.point;
                    transform.localRotation = Quaternion.FromToRotation(Vector3.down, contact.normal);
                    Vector3 tempVector = transform.localPosition;
                    transform.localPosition = new Vector3(tempVector.x, tempVector.y - (renderer.bounds.size.y / 4), tempVector.z);
                    transform.SetParent(collision.collider.transform);
                    rigidbody.velocity = Vector3.zero;
                    rigidbody.angularVelocity = Vector3.zero;
                    rigidbody.isKinematic = true;
                }
            }
            if (collision.collider.GetComponent<Vitals>() != null)
            {
                collision.collider.GetComponent<Vitals>().TakeDamage((int)(damage * (float)(collision.relativeVelocity.magnitude / 20)));
            }
        }
    }

    /// <summary>
    /// When the weapon is swinging, its collider acts as a trigger.
    /// When it is triggered, it deals damage.
    /// </summary>
    public void OnTriggerEnter(Collider other)
    {
        if (isEquipped && other.gameObject != equippedTo && other.GetComponent<Vitals>() != null)
        {
            other.GetComponent<Vitals>().TakeDamage(damage);
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
}
