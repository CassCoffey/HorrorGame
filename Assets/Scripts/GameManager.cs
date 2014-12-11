using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

    // The default player prefab.
    public GameObject playerPrefab;

    private GameObject[] spawnPoints;

	// Used when the player enters the map to spawn a player object.
	void Start () 
    {
        spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
        SpawnPlayer();
	}

    // Create a player object.
    private void SpawnPlayer()
    {
        int index = Random.Range(0, spawnPoints.Length);
        Vector3 spawnPoint = spawnPoints[index].transform.position;
        Vector3 spawn = new Vector3(spawnPoint.x + Random.Range(-5, 5), spawnPoint.y, spawnPoint.z + Random.Range(-5, 5));
        Debug.Log("Spawning Player");
        GameObject player = (GameObject)Network.Instantiate(playerPrefab, spawn, Quaternion.identity, 0);
    }
}
