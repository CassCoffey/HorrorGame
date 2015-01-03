using UnityEngine;
using System.Collections;

public class WeaponSpawn : MonoBehaviour {

    public GameObject[] weapons;

	// Use this for initialization
	void Start () {
        GameObject weapon = (GameObject)Instantiate(weapons[Random.Range(0, weapons.Length)]);
        weapon.transform.position = transform.position;
	}
}
