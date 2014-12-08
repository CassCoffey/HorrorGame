using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

    // The default player prefab.
    public GameObject playerPrefab;

	// Used when the player enters the map to spawn a player object.
	void Start () {
        SpawnPlayer();
	}

    // Create a player object.
    private void SpawnPlayer()
    {
        Debug.Log("Spawning Player");
        GameObject player = (GameObject)Network.Instantiate(playerPrefab, new Vector3(0f, 2f, 0f), Quaternion.identity, 0);
    }
}
