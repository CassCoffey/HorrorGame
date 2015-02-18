using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class ResultsManager : MonoBehaviour {
	
	public GameObject lobbyButton;
	public GameObject playersList;
	public GameObject playerLabel;
	public Font font;
	public List<GameObject> playerPrefabs;
	
	void Start()
	{
        Camera.main.GetComponent<AudioListener>().enabled = false;
        Camera.main.GetComponent<Camera>().enabled = false;
        camera.enabled = true;
        Screen.lockCursor = false;
        Screen.showCursor = true;
        if (Network.isServer)
        {
            lobbyButton.SetActive(true);
        }
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            playerPrefabs.Add(player);
        }
		playerPrefabs.Add(GameObject.FindGameObjectWithTag("Monster"));
		ResizeScrollingBox (playerPrefabs.Count);
		int i = 0;
		foreach (GameObject player in playerPrefabs) 
		{
			if(player.tag == "Monster")
			{
				CreatePlayerLabel((string)GameObject.Find("SpawnManager").GetComponent<SpawnManager>().userNames[player.networkView.owner], player.GetComponent<MonsterManager>().Name, player.GetComponent<MonsterManager>().Role, playerPrefabs.Count, i);
			}
			else
			{
                CreatePlayerLabel((string)GameObject.Find("SpawnManager").GetComponent<SpawnManager>().userNames[player.networkView.owner], player.GetComponent<Player>().Name, player.GetComponent<Player>().Role, playerPrefabs.Count, i);
			}
			i++;
		}
	}
	
	void CreatePlayerLabel(string userName, string playerName, string playerRole, int num, int i)
	{
		GameObject label = (GameObject)Instantiate (playerLabel);
		label.transform.SetParent(playersList.transform.FindChild("PlayersScrolling"), false);
		label.GetComponentInChildren<Text>().font = font;
		label.transform.FindChild ("UserName").GetComponent<Text> ().text = userName;
		label.transform.FindChild ("PlayerName").GetComponent<Text> ().text = playerName;
		label.transform.FindChild ("PlayerRole").GetComponent<Text> ().text = playerRole;
		label.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1 - ((1.0f / (float)num) * i));
		label.GetComponent<RectTransform>().anchorMin = new Vector2(0, (1 - (1.0f / (float)num)) - ((1.0f / (float)num) * i));
		label.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
		label.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
		if (i % 2 == 0)
		{
			label.GetComponent<Image>().color = new UnityEngine.Color(0.4f, 0.4f, 0.4f, 0.7f);
		}
		else
		{
			label.GetComponent<Image>().color = new UnityEngine.Color(0.6f, 0.6f, 0.6f, 0.7f);
		}
	}
	
	void ResizeScrollingBox(int numOfPlayers)
	{
		float panelHeight = playerLabel.GetComponent<RectTransform>().rect.height * numOfPlayers;
		float currentHeight = playersList.GetComponent<RectTransform>().rect.height;
		playersList.GetComponent<ScrollRect>().enabled = (panelHeight > currentHeight);
		playersList.transform.FindChild("PlayersScrolling").GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
		playersList.transform.FindChild("PlayersScrolling").GetComponent<RectTransform>().offsetMin = new Vector2(0, currentHeight - panelHeight);
	}
	
	public void GoToLobby()
	{
        Debug.Log("Pressed go to lobby.");
        if (Network.isServer)
        {
            networkView.RPC("EnableLobby", RPCMode.All);
        }
        GameObject.Find("NetworkManager").GetComponent<NetworkManager>().ReturnToLobby();
	}
	
	[RPC] public void EnableLobby()
	{
        // There is no reason to send any more data over the network on the default channel,
        // because we are about to load the level, thus all those objects will get deleted anyway
        Network.SetSendingEnabled(0, false);

        // We need to stop receiving because first the level must be loaded first.
        // Once the level is loaded, rpc's and other state update attached to objects in the level are allowed to fire
        Network.isMessageQueueRunning = false;

		lobbyButton.SetActive (true);
	}
}
