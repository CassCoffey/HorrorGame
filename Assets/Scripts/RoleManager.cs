using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoleManager : MonoBehaviour {

	public GameObject RoleNameText;
	public GameObject RoleDescriptionText;
	private List<NetworkPlayer> playerList = new List<NetworkPlayer>();

	// Use this for initialization
	void Start () {
		playerList.Add (Network.player);
		for (int i = 0; i < Network.connections.Length; i++) 
		{
			playerList.Add (Network.connections[i]);
		}
		Debug.Log (playerList);
		while (playerList.Count != 0) 
		{
			int index = Random.Range(0,playerList.Count);

			playerList.RemoveAt(index);
		}
	}
	
	// Update is called once per frame
	void Update () {

	}
}
