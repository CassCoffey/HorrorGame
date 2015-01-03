using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RoleManager : MonoBehaviour {

	public GameObject Player;
	private List<NetworkPlayer> playerList = new List<NetworkPlayer>();

	// Use this for initialization
	void Start () {
		playerList.Add (Network.player);
		for (int i = 0; i < Network.connections.Length; i++) 
		{
			playerList.Add (Network.connections[i]);
		}
		Debug.Log (playerList);

		int monsterIndex = Random.Range (0, playerList.Count-1);
		networkView.RPC ("makeMonster", playerList[monsterIndex]);
		playerList.RemoveAt (monsterIndex);

		if (playerList.Count >= 6) 
		{
			int cultistIndex = Random.Range(0,playerList.Count-1);
			networkView.RPC ("makeCultist", playerList[cultistIndex]);
			playerList.RemoveAt(cultistIndex);

		}

		int numOfRoles = Mathf.FloorToInt(playerList.Count / 2);
		while (playerList.Count > numOfRoles) 
		{
			int specialIndex = Random.Range(0,playerList.Count-1);
			int role = Random.Range (0,100);
			if(role < 50){
				networkView.RPC("makePriest", playerList[specialIndex]);
			}
			if(role >= 50){
				networkView.RPC("makeAssassin", playerList[specialIndex]);
			}
			playerList.RemoveAt(specialIndex);
		}
		while (playerList.Count > 0) 
		{
			int normIndex = Random.Range (0,playerList.Count);
			int normRole = Random.Range (0,1);
			if(normRole > 1){
				networkView.RPC("makePeasant", playerList[normIndex]);
			}
			if(normRole <= 1){
				networkView.RPC("makeSurvivor", playerList[normIndex]);
			}
			playerList.RemoveAt(normIndex);
		}
	}
	[RPC] void makeMonster()
	{
		Player.transform.FindChild ("Menu").FindChild ("RoleNamePanel").FindChild ("RoleName").GetComponent<Text>().text = "Monster";
		Player.transform.FindChild ("Menu").FindChild ("RoleDescriptionPanel").FindChild ("RoleDescription").GetComponent<Text>().text = "You're a monster";
	}

	[RPC] void makeCultist()
	{
		Player.transform.FindChild ("Menu").FindChild ("RoleNamePanel").FindChild ("RoleName").GetComponent<Text>().text = "Cultist";
		Player.transform.FindChild ("Menu").FindChild ("RoleDescriptionPanel").FindChild ("RoleDescription").GetComponent<Text>().text = "You're a cultist";
	}

	[RPC] void makePriest() 
	{
		Player.transform.FindChild ("Menu").FindChild ("RoleNamePanel").FindChild ("RoleName").GetComponent<Text>().text = "Priest";
		Player.transform.FindChild ("Menu").FindChild ("RoleDescriptionPanel").FindChild ("RoleDescription").GetComponent<Text>().text = "You're a priest";
	}

	[RPC] void makeAssassin() 
	{
		Player.transform.FindChild ("Menu").FindChild ("RoleNamePanel").FindChild ("RoleName").GetComponent<Text>().text = "Assassin";
		Player.transform.FindChild ("Menu").FindChild ("RoleDescriptionPanel").FindChild ("RoleDescription").GetComponent<Text>().text = "You're an assassin";
	}

	[RPC] void makePeasant() 
	{
		Player.transform.FindChild ("Menu").FindChild ("RoleNamePanel").FindChild ("RoleName").GetComponent<Text>().text = "Peasant";
		Player.transform.FindChild ("Menu").FindChild ("RoleDescriptionPanel").FindChild ("RoleDescription").GetComponent<Text>().text = "You're a peasant";
	}

	[RPC] void makeSurvivor() 
	{
		Player.transform.FindChild ("Menu").FindChild ("RoleNamePanel").FindChild ("RoleName").GetComponent<Text>().text = "Survivor";
		Player.transform.FindChild ("Menu").FindChild ("RoleDescriptionPanel").FindChild ("RoleDescription").GetComponent<Text>().text = "You're a survivor";
	}
}
