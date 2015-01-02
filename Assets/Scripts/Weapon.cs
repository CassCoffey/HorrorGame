using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {

    public bool isEquipped = false;
    public bool isStuck = false;
    public bool isSharp;

    private bool moving = false;
    private float startTime = 0;
    private float totalTime = 0;
    private Vector3 originPos;
    private Vector3 destinationPos;
    private Quaternion originRot;
    private Quaternion destinationRot;

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

    public void OnCollisionEnter(Collision collision)
    {
        if (!isEquipped && isSharp && !isStuck && collision.collider.transform.parent != transform && !collision.collider.isTrigger && collision.collider.transform != transform && collision.relativeVelocity.magnitude > 15)
        {
            if (collision.collider.tag == "Player" || collision.collider.tag == "Terrain")
            {
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
                Physics.IgnoreCollision(collider, collision.collider);
            }
        }
    }

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
