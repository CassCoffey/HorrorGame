using UnityEngine;
using System.Collections.Generic;

public class Weapon : MonoBehaviour {

    public bool isStuck = false;
    public bool isSharp;
    public int damage;

    public bool charging = false;
    private float percentCharge;
    private const float goalCharge = 0.15f;
    private float startCharge;
    private Vector3 previousVelocity;

    /// <summary>
    /// Manage charge levels.
    /// </summary>
    public void Update()
    {
        if (charging && percentCharge != 1.0f && transform.parent != null && transform.parent.GetComponent<Animator>() != null && transform.parent.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Default"))
        {
            float time = Time.time - startCharge;
            percentCharge = time / goalCharge;
            transform.localPosition = Vector3.Lerp(Vector3.zero, Vector3.back * 0.06f, percentCharge);
            if (percentCharge >= 1.0f)
            {
                percentCharge = 1.0f;
                //GetComponent<Item>().AltUse();
            }
        }
        else if (transform.parent != null && transform.parent.GetComponent<Animator>() != null && !transform.parent.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Default"))
        {
            charging = false;
        }
    }

    /// <summary>
    /// Keep track of the previous velocity in order to check collision speeds.
    /// </summary>
    public void FixedUpdate()
    {
        previousVelocity = rigidbody.velocity;
    }

    /// <summary>
    /// Begins charging the weapon.
    /// </summary>
    public void Charge()
    {
        charging = true;
        startCharge = Time.time;
    }

    /// <summary>
    /// Throws the weapon.
    /// </summary>
    /// <param name="throwForce">The force with which to throw.</param>
    public void Throw(float throwForce)
    {
        networkView.RPC("SyncThrow", RPCMode.OthersBuffered, percentCharge, throwForce);
        charging = false;
        // Makes sure the charge is at least 15%.
        if (percentCharge <= 0.15f)
        {
            percentCharge = 0.15f;
        }
        rigidbody.AddRelativeForce(Vector3.forward * throwForce * percentCharge, ForceMode.Force);
        rigidbody.maxAngularVelocity = 35;
        rigidbody.AddRelativeTorque(150 * percentCharge, 0, 0, ForceMode.Force);
        percentCharge = 0.0f;
    }

    /// <summary>
    /// When the weapon collides with something, if it is moving fast enough it will deal damage.
    /// Also, if it is sharp it will stick.
    /// </summary>
    public void OnCollisionEnter(Collision collision)
    {
        if (!isStuck && collision.collider.transform.parent != transform && !collision.collider.isTrigger && collision.collider.transform != transform && collision.relativeVelocity.magnitude > (10/rigidbody.mass) && previousVelocity.magnitude > 5)
        {
            if (isSharp)
            {
                // Only stick to players and terrain.
                if (collision.collider.tag == "Player" || collision.collider.tag == "Terrain" || collision.collider.tag == "Monster")
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
            // If you collided with something that has vitals, hurt said vitals.
            if (collision.collider.GetComponent<Vitals>() != null)
            {
                collision.collider.GetComponent<Vitals>().TakeDamage((int)(damage * (float)(collision.relativeVelocity.magnitude / 20)) * (int)rigidbody.mass, "Thrown Weapon");
            }
        }
    }

    /// <summary>
    /// Removes the weapon from whatever it is currently stuck to.
    /// </summary>
    public void Unstick()
    {
        collider.isTrigger = false;
        GetComponent<Weapon>().isStuck = false;
        transform.SetParent(null);
        rigidbody.isKinematic = false;
    }

    /// <summary>
    /// When the weapon is swinging, its collider acts as a trigger.
    /// When it is triggered, it deals damage.
    /// </summary>
    public void OnTriggerEnter(Collider other)
    {
        if (GetComponent<Item>().isEquipped && other.gameObject != GetComponent<Item>().equippedTo && other.GetComponent<Vitals>() != null)
        {
            other.GetComponent<Vitals>().TakeDamage(damage, GetComponent<Item>().equippedTo.GetComponent<Player>().Name);
        }
    }

    /// <summary>
    /// Synchronizes the throw for all instances of the weapon.
    /// </summary>
    /// <param name="percent">The chrge percent.</param>
    /// <param name="throwForce">The throw force.</param>
    [RPC] void SyncThrow(float percent, float throwForce)
    {
        charging = false;
        if (percent <= 0.15f)
        {
            percent = 0.15f;
        }
        rigidbody.AddRelativeForce(Vector3.forward * throwForce * percent);
        rigidbody.maxAngularVelocity = 30;
        rigidbody.AddRelativeTorque(40 * percent, 0, 0, ForceMode.Impulse);
    }
}
