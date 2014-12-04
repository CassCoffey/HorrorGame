using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

    public GameObject playerPrefab;

	// Use this for initialization
	void Start () {
        SpawnPlayer();
	}

    // Create a player object.
    private void SpawnPlayer()
    {
        Debug.Log("Spawning Player");
        GameObject player = (GameObject)Network.Instantiate(playerPrefab, new Vector3(0f, 2f, 0f), Quaternion.identity, 0);
    }
	
	// Update is called once per frame
	void Update () {
	    
	}
}
