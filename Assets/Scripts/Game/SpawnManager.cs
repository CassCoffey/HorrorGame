using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour {
	
	public GameObject playerPrefab;
	public GameObject monsterPrefab;
    public List<string> firstNames;
    public List<string> lastNames;
	public int playersAlive = -1;
    public Hashtable userNames = new Hashtable();

	private GameObject[] spawnPoints;
	private float spawnRadius = 5f;
	private float radiusCheck = 1.5f;
	private int maxSpawnAttempts = 50;
	private List<NetworkPlayer> playerList = new List<NetworkPlayer>();
	private List<NetworkPlayer> readyList = new List<NetworkPlayer>();
	private Hashtable playerNames = new Hashtable();

    public string randomName;
	public string myRole;
    public GameObject myPlayer;

	/// <summary>
	/// Performs initialization after the network has loaded the level.
	/// </summary>
    void OnNetworkLoadedLevel()
    {
        ChooseRandomName();
        // Begin building hashtable of all networkplayers and their names.
        networkView.RPC("CreateNameHashtable", RPCMode.All, Network.player, randomName, GameObject.Find("NetworkManager").GetComponent<NetworkManager>().playerName);
        if (Network.isServer)
        {
            MasterServer.UnregisterHost();
            Network.maxConnections = 0;
            MasterServer.RegisterHost(NetworkManager.AppId, GameObject.Find("NetworkManager").GetComponent<NetworkManager>().gameName, "Closed");
        }
		if (Network.isClient)
		{
            // Tells the server when the client is ready to spawn.
            Debug.Log("Is client");
			networkView.RPC("ClientReady", RPCMode.Server, Network.player);
		}
		if (Network.isServer && Network.connections.Length == 0)
		{
            Debug.Log("Singleplayer");
            // If playing by yourself, just start the game.
            ChooseWeapons();
			ChooseRole();
		}
	}
	/// <summary>
	/// Chooses a random name for the players when they spawn
	/// </summary>
    private void ChooseRandomName()
    {
        randomName = firstNames[Random.Range(0, firstNames.Count)];
        randomName += " ";
        randomName += lastNames[Random.Range(0, firstNames.Count)];
    }

    /// <summary>
    /// Goes through all weapon spawns and tells them to spawn a weapon.
    /// </summary>
    private void ChooseWeapons()
    {
        foreach (GameObject spawn in GameObject.FindGameObjectsWithTag("WeaponSpawn"))
        {
            spawn.GetComponent<WeaponSpawn>().SpawnWeapon();
        }
    }

    /// <summary>
    /// Handles weighted assignment of roles.
    /// </summary>
	void ChooseRole()
	{
		if(Network.isServer)
        {
			playerList.Add (Network.player);
			for (int i = 0; i < Network.connections.Length; i++) 
			{
				playerList.Add (Network.connections[i]);
			}

			//Spawning the monster
			int monsterIndex = Random.Range (0, playerList.Count);
			NetworkPlayer Monster = playerList[monsterIndex];
			if (playerList [monsterIndex] == Network.player) 
			{
				SpawnPlayer("Monster", Monster);
			}
			else
			{
				networkView.RPC("SpawnPlayer", playerList[monsterIndex], "Monster", Monster);
			}
			playerList.RemoveAt(monsterIndex);

			//Spawning the cultist if their are more than 6 players
			if (playerList.Count >= 6) 
			{
				int cultistIndex = Random.Range(0,playerList.Count);
				if (playerList [cultistIndex] == Network.player) 
				{
					SpawnPlayer("Cultist", Monster);
				}
				else
				{
					networkView.RPC("SpawnPlayer", playerList[cultistIndex], "Cultist", Monster);
				}
				playerList.RemoveAt(cultistIndex);
			}
            playersAlive = playerList.Count;

			//Spawning special roles by taking half of the amount of players left rounded up
			int numOfRoles = Mathf.FloorToInt(playerList.Count / 2);
			for(int i = 0; i < numOfRoles; i++)
			{
				int specialIndex = Random.Range(0,playerList.Count);
				NetworkPlayer specialPlayer = playerList[specialIndex];
				int role;
				//If there are two roles left or more, allow the spawning of paired roles
				if(numOfRoles >= 2)
				{
					role = Random.Range (0,100);
				}
				else
				{
					role = Random.Range (50,100);
				}
				//Paired Roles
				if(role < 25)
				{
					int pairIndex = Random.Range(0,playerList.Count);
					NetworkPlayer pairPlayer = playerList[pairIndex];
					while(pairIndex == specialIndex){
						pairIndex = Random.Range(0,playerList.Count);
					}
					if (specialPlayer == Network.player) 
					{
						SpawnPair("Lover", (string)playerNames[pairPlayer], Monster);
					}
					else
					{
						networkView.RPC("SpawnPair", specialPlayer, "Lover", (string)playerNames[pairPlayer], Monster);
					}
					if(pairPlayer == Network.player)
					{
						SpawnPair("Lover", (string)playerNames[specialPlayer], Monster);
					}
					else
					{
						networkView.RPC("SpawnPair", pairPlayer, "Lover", (string)playerNames[specialPlayer], Monster);
					}
					playerList.Remove(pairPlayer);
				}
				if(role >= 25 && role < 50)
				{
					int pairIndex = Random.Range(0,playerList.Count);
					NetworkPlayer pairPlayer = playerList[pairIndex];
					while(pairIndex == specialIndex){
						pairIndex = Random.Range(0,playerList.Count);
					}
					if (specialPlayer == Network.player) 
					{
						SpawnPair("Thief", (string)playerNames[pairPlayer], Monster);
					}
					else
					{
						networkView.RPC("SpawnPair", specialPlayer, "Thief", (string)playerNames[pairPlayer], Monster);
					}
					if(pairPlayer == Network.player)
					{
						SpawnPair("Thief", (string)playerNames[specialPlayer], Monster);
					}
					else
					{
						networkView.RPC("SpawnPair", pairPlayer, "Thief", (string)playerNames[specialPlayer], Monster);
					}
					playerList.Remove(pairPlayer);
				}
				//Singular Special Roles
				if(role >= 50 && role < 75)
				{
					if (specialPlayer == Network.player) 
					{
						SpawnPlayer("Priest", Monster);
					}
					else
					{
						networkView.RPC("SpawnPlayer", specialPlayer, "Priest", Monster);
					}
				}
				if(role >= 75)
				{
					if (specialPlayer == Network.player) 
					{
						SpawnPlayer("Assassin", Monster);
					}
					else
					{
						networkView.RPC("SpawnPlayer", specialPlayer, "Assassin", Monster);
					}
				}
				playerList.Remove(specialPlayer);

			//Spawns the rest of the normal roles
			}
			while (playerList.Count > 0) 
			{
				int normIndex = Random.Range (0,playerList.Count);
				NetworkPlayer normPlayer = playerList[normIndex];
				int normRole = Random.Range (0,100);
				if(normRole < 50)
				{
					if (normPlayer == Network.player) 
					{
						SpawnPlayer("Peasant", Monster);
					}
					else
					{
						networkView.RPC("SpawnPlayer", normPlayer, "Peasant", Monster);
					}
				}
				if(normRole >= 50)
				{
					if (normPlayer == Network.player) 
					{
						SpawnPlayer("Survivor", Monster);
					}
					else
					{
						networkView.RPC("SpawnPlayer", normPlayer, "Survivor", Monster);
					}
				}
				playerList.Remove(normPlayer);
			}
		}
	}

    /// <summary>
    /// Updates the text of a player's role description.
    /// </summary>
    /// <param name="player">The player whose text is being changed.</param>
    /// <param name="roleName">The name of their role.</param>
    /// <param name="roleDescription">The description of their role.</param>
    public void SetRoleText(GameObject player, string roleName, string roleDescription)
    {
		if (roleName == "Monster") {
			player.GetComponent<MonsterManager> ().Menu.transform.FindChild ("MainPanel").FindChild ("RoleNamePanel").FindChild ("RoleName").GetComponent<Text> ().text = roleName;
			player.GetComponent<MonsterManager> ().Menu.transform.FindChild ("MainPanel").FindChild ("RoleDescriptionPanel").FindChild ("RoleDescription").GetComponent<Text> ().text = roleDescription;
		} 
		else 
		{
			player.GetComponent<Player>().Menu.transform.FindChild("MainPanel").FindChild("RoleNamePanel").FindChild("RoleName").GetComponent<Text>().text = roleName;
			player.GetComponent<Player>().Menu.transform.FindChild("MainPanel").FindChild("RoleDescriptionPanel").FindChild("RoleDescription").GetComponent<Text>().text = roleDescription;
		}
    }

    /// <summary>
    /// Keeps track of the living players.
    /// </summary>
	public void playerDeath()
	{
		playersAlive -= 1;
	}

    /// <summary>
    /// Enables the trails on the specified player.
    /// </summary>
	[RPC] void EnableTrails(NetworkViewID viewID)
	{
		GameObject player = NetworkView.Find(viewID).gameObject;
		player.transform.FindChild ("TrailParticles").particleSystem.Play();
	}

    /// <summary>
    /// Sent to the server when a client is ready.
    /// The server adds the readied player to a readyList and checks to see if the readyList contains all players.
    /// </summary>
    /// <param name="player">The player who is ready.</param>
	[RPC] void ClientReady(NetworkPlayer player)
	{
		readyList.Add(player);
		if (readyList.Count == Network.connections.Length) 
		{
            ChooseWeapons();
			ChooseRole();
		}
	}

    /// <summary>
    /// Create a player object.
    /// </summary>
    /// <param name="role">The role of that player.</param>
	[RPC] void SpawnPlayer(string role, NetworkPlayer Monster)
	{
        myRole = role;
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
			Debug.Log("Spawning Monster");
            randomName = "Monster";
			GameObject player = (GameObject)Network.Instantiate(monsterPrefab, spawn, Quaternion.identity, 0);
            myPlayer = player;
            SetRoleText(player, "Monster", "You're a monster! Kill everyone...");
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
            myPlayer = player;
            Debug.Log("role - " + role + ", myRole - " + myRole);
			switch (role) 
			{
			case "Cultist":
                SetRoleText(player, "Cultist", "You're " + randomName + ", a Cultist");
				break;
			case "Peasant":
                SetRoleText(player, "Peasant", "You're " + randomName + ", a Peasant");
				break;
			case "Survivor":
                SetRoleText(player, "Survivor", "You're " + randomName + ", a Survivor");
				break;
			case "Priest":
                SetRoleText(player, "Priest", "You're " + randomName + ", a Priest");
				break;
            case "Assassin":
                SetRoleText(player, "Assassin", "You're " + randomName + ", an Assassin");
				break;
			}
			networkView.RPC ("EnableTrails", Monster, player.networkView.viewID);
		}
	}

    /// <summary>
    /// Create a player object.
    /// </summary>
    /// <param name="role">The role of that player.</param>
    /// <param name="pair">That player's pair player name.</param>
	[RPC] void SpawnPair(string role, string pair , NetworkPlayer Monster)
	{
        myRole = role;
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
        myPlayer = player;
		switch (role) 
		{
		case "Lover":
            SetRoleText(player, "Lover", "You're " + randomName + ", in love with " + pair);
			break;
		case "Thief":
            SetRoleText(player, "Thief", "You're " + randomName + ", a thief! There is another thief among you.");
			break;
		}
		networkView.RPC ("EnableTrails", Monster, player.networkView.viewID);
	}

    /// <summary>
    /// Creates a hashtable of all the players and their names.
    /// </summary>
	[RPC] void CreateNameHashtable(NetworkPlayer player, string name, string username)
	{
		playerNames.Add(player, name);
        userNames.Add(player, username);
	}
}