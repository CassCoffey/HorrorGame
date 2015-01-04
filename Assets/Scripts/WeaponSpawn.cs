using UnityEngine;
using System.Collections;

public class WeaponSpawn : MonoBehaviour {

    public GameObject[] weapons;

    public void SpawnWeapon()
    {
        Network.Instantiate(weapons[Random.Range(0, weapons.Length)], transform.position, transform.rotation, 0);
    }
}
