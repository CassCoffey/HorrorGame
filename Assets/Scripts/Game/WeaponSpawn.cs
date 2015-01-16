using UnityEngine;
using System.Collections;

public class WeaponSpawn : MonoBehaviour {

    // A list of all the possible weapons that can spawn here.
    public GameObject[] weapons;

    /// <summary>
    /// Spawns a weapon from the weapon list.
    /// </summary>
    public void SpawnWeapon()
    {
        Network.Instantiate(weapons[Random.Range(0, weapons.Length)], transform.position, transform.rotation, 0);
    }
}
