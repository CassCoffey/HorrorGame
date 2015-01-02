using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

    // The default player prefab.
    public GameObject playerPrefab;

    private GameObject[] spawnPoints;
	private float spawnRadius = 5f;
	private float radiusCheck = 1.5f;
	private int maxSpawnAttempts = 50;

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
        Vector3 spawn = new Vector3(spawnPoint.x + Random.Range(-spawnRadius, spawnRadius), spawnPoint.y, spawnPoint.z + Random.Range(-spawnRadius, spawnRadius));
		for(int i = 0; i < maxSpawnAttempts; i++){
			if(Physics.CheckSphere(spawn,radiusCheck))
			{
				spawn = new Vector3(spawnPoint.x + Random.Range(-spawnRadius, spawnRadius), spawnPoint.y, spawnPoint.z + Random.Range(-spawnRadius, spawnRadius));
			}
			else
			{
				break;
			}
		}
        Debug.Log("Spawning Player");
        GameObject player = (GameObject)Network.Instantiate(playerPrefab, spawn, Quaternion.identity, 0);
    }
}
