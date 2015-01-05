using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;



public class SpawnManager : MonoBehaviour {

	// The default player prefab.
	public GameObject playerPrefab;

	private GameObject[] spawnPoints;
	private float spawnRadius = 5f;
	private float radiusCheck = 1.5f;
	private int maxSpawnAttempts = 50;
	private List<NetworkPlayer> playerList = new List<NetworkPlayer>();
	private List<NetworkPlayer> readyList = new List<NetworkPlayer>();
	private Hashtable playerNames = new Hashtable();

	// Use this for initialization
    void OnNetworkLoadedLevel()
    {
		networkView.RPC("CreateNameHashtable", RPCMode.All, Network.player, PlayerPrefs.GetString("UserName"));
		if (Network.isClient) 
		{
            Debug.Log("Is client");
			networkView.RPC("ClientReady", RPCMode.Server, Network.player);
		}
		if (Network.isServer && Network.connections.Length == 0)
		{
            ChooseWeapons();
			ChooseRole();
		}
	}

    private void ChooseWeapons()
    {
        foreach (GameObject spawn in GameObject.FindGameObjectsWithTag("WeaponSpawn"))
        {
            spawn.GetComponent<WeaponSpawn>().SpawnWeapon();
        }
    }

	void ChooseRole()
	{
		if(Network.isServer){
			playerList.Add (Network.player);
			for (int i = 0; i < Network.connections.Length; i++) 
			{
				playerList.Add (Network.connections[i]);
			}
			int monsterIndex = Random.Range (0, playerList.Count);
			Debug.Log ("Creating Monster...");
			if (playerList [monsterIndex] == Network.player) 
			{
				SpawnPlayer("Monster");
			}
			else
			{
				networkView.RPC("SpawnPlayer", playerList[monsterIndex], "Monster");
			}
			playerList.RemoveAt(monsterIndex);
			
			if (playerList.Count >= 6) 
			{
				int cultistIndex = Random.Range(0,playerList.Count);
				Debug.Log ("Creating Cultist...");
				if (playerList [cultistIndex] == Network.player) 
				{
					SpawnPlayer("Cultist");
				}
				else
				{
					networkView.RPC("SpawnPlayer", playerList[cultistIndex], "Cultist");
				}
				playerList.RemoveAt(cultistIndex);
			}

			Debug.Log ("Generating Special Roles...");
			int numOfRoles = Mathf.FloorToInt(playerList.Count / 2);
			for(int i = 0; i < numOfRoles; i++)
			{
				int specialIndex = Random.Range(0,playerList.Count);
				NetworkPlayer specialPlayer = playerList[specialIndex];
				int role;
				if(numOfRoles >= 2)
				{
					role = Random.Range (0,100);
				}
				else
				{
					role = Random.Range (50,100);
				}
				if(role < 25)
				{
					Debug.Log ("Creating Lovers...");
					int pairIndex = Random.Range(0,playerList.Count);
					NetworkPlayer pairPlayer = playerList[pairIndex];
					while(pairIndex == specialIndex){
						pairIndex = Random.Range(0,playerList.Count);
					}
					if (specialPlayer == Network.player) 
					{
						SpawnPair("Lover", (string)playerNames[pairPlayer]);
					}
					else
					{
						networkView.RPC("SpawnPair", specialPlayer, "Lover", (string)playerNames[pairPlayer]);
					}
					if(pairPlayer == Network.player)
					{
						SpawnPair("Lover", (string)playerNames[specialPlayer]);
					}
					else
					{
						networkView.RPC("SpawnPair", pairPlayer, "Lover", (string)playerNames[specialPlayer]);
					}
					playerList.Remove(pairPlayer);
				}
				if(role >= 25 && role < 50)
				{
					Debug.Log ("Creating Thieves...");
					int pairIndex = Random.Range(0,playerList.Count);
					NetworkPlayer pairPlayer = playerList[pairIndex];
					while(pairIndex == specialIndex){
						pairIndex = Random.Range(0,playerList.Count);
					}
					if (specialPlayer == Network.player) 
					{
						SpawnPair("Thief", (string)playerNames[pairPlayer]);
					}
					else
					{
						networkView.RPC("SpawnPair", specialPlayer, "Thief", (string)playerNames[pairPlayer]);
					}
					if(pairPlayer == Network.player)
					{
						SpawnPair("Thief", (string)playerNames[specialPlayer]);
					}
					else
					{
						networkView.RPC("SpawnPair", pairPlayer, "Thief", (string)playerNames[specialPlayer]);
					}
					playerList.Remove(pairPlayer);
				}
				if(role >= 50 && role < 75)
				{
					Debug.Log ("Creating Priest...");
					if (specialPlayer == Network.player) 
					{
						SpawnPlayer("Priest");
					}
					else
					{
						networkView.RPC("SpawnPlayer", specialPlayer, "Priest");
					}
				}
				if(role >= 75)
				{
					Debug.Log ("Creating Assassin...");
					if (specialPlayer == Network.player) 
					{
						SpawnPlayer("Assassin");
					}
					else
					{
						networkView.RPC("SpawnPlayer", specialPlayer, "Assassin");
					}
				}
				playerList.Remove(specialPlayer);
			}
			while (playerList.Count > 0) 
			{
				int normIndex = Random.Range (0,playerList.Count);
				NetworkPlayer normPlayer = playerList[normIndex];
				int normRole = Random.Range (0,100);
				if(normRole < 50)
				{
					Debug.Log ("Creating Peasant...");
					if (normPlayer == Network.player) 
					{
						SpawnPlayer("Peasant");
					}
					else
					{
						networkView.RPC("SpawnPlayer", normPlayer, "Peasant");
					}
				}
				if(normRole >= 50)
				{
					Debug.Log ("Creating Survivor...");
					if (normPlayer == Network.player) 
					{
						SpawnPlayer("Survivor");
					}
					else
					{
						networkView.RPC("SpawnPlayer", normPlayer, "Survivor");
					}
				}
				playerList.Remove(normPlayer);
			}
		}
	}

    public void SetRoleText(GameObject player, string roleName, string roleDescription)
    {
        player.GetComponent<Player>().Menu.transform.FindChild("MainPanel").FindChild("RoleNamePanel").FindChild("RoleName").GetComponent<Text>().text = roleName;
        player.GetComponent<Player>().Menu.transform.FindChild("MainPanel").FindChild("RoleDescriptionPanel").FindChild("RoleDescription").GetComponent<Text>().text = roleDescription;
    }

	[RPC] void ClientReady(NetworkPlayer player)
	{
		readyList.Add(player);
		if (readyList.Count == Network.connections.Length) 
		{
            ChooseWeapons();
			ChooseRole();
		}
	}

	// Create a player object.
	[RPC] void SpawnPlayer(string role)
	{
		if (role == "Monster") 
		{
			spawnPoints = GameObject.FindGameObjectsWithTag("MonsterSpawn");
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
            SetRoleText(player, "Monster", "You're a monster!");
		}
		else
		{
			spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
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
			switch (role) 
			{
			case "Cultist":
                SetRoleText(player, "Cultist", "You're a Cultist");
				break;
			case "Peasant":
                SetRoleText(player, "Peasant", "You're a Peasant");
				break;
			case "Survivor":
                SetRoleText(player, "Survivor", "You're a Survivor");
				break;
			case "Priest":
                SetRoleText(player, "Priest", "You're a Priest");
				break;
            case "Assassin":
                SetRoleText(player, "Assassin", "You're an Assassin");
				break;
			}
		}
	}

	[RPC] void SpawnPair(string role, string pair)
	{
		spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
		int index = Random.Range(0, spawnPoints.Length);
		Vector3 spawnPoint = spawnPoints[index].transform.position;
		Vector3 spawn = new Vector3(spawnPoint.x + Random.Range(-spawnRadius, spawnRadius), spawnPoint.y, spawnPoint.z + Random.Range(-spawnRadius, spawnRadius));
		for(int i = 0; i < maxSpawnAttempts; i++)
		{
			if(Physics.CheckSphere(spawn,radiusCheck))
			{
				spawn = new Vector3(spawnPoint.x + Random.Range(-spawnRadius, spawnRadius), spawnPoint.y, spawnPoint.z + Random.Range(-spawnRadius, spawnRadius));
			}
			else
			{
				break;
			}
		}
		Debug.Log("Spawning Pairs");
		GameObject player = (GameObject)Network.Instantiate(playerPrefab, spawn, Quaternion.identity, 0);
		switch (role) 
		{
		case "Lover":
			SetRoleText(player, "Lover", "You're in love with " + pair);
			break;
		case "Thief":
			SetRoleText(player, "Thief", "You're a thief! There is another thief among you.");
			break;
		}
	}

	[RPC] void CreateNameHashtable(NetworkPlayer player, string name)
	{
		playerNames.Add(player, name);
	}
}