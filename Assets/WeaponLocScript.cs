using UnityEngine;
using System.Collections;

public class WeaponLocScript : MonoBehaviour {

	public void TriggerWeapon()
    {
        transform.GetChild(0).collider.isTrigger = true;
        transform.GetChild(0).collider.enabled = true;
    }

    public void RestWeapon()
    {
        transform.GetChild(0).collider.enabled = false;
        transform.GetChild(0).collider.isTrigger = false;
    }
}