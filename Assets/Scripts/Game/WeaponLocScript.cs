using UnityEngine;
using System.Collections;

public class WeaponLocScript : MonoBehaviour {

    /// <summary>
    /// Activates the weapon's trigger.
    /// </summary>
	public void TriggerWeapon()
    {
        transform.GetChild(0).collider.isTrigger = true;
        transform.GetChild(0).collider.enabled = true;
    }

    /// <summary>
    /// Puts the weapon back in rest state.
    /// </summary>
    public void RestWeapon()
    {
        transform.GetChild(0).collider.enabled = false;
        transform.GetChild(0).collider.isTrigger = false;
    }
}