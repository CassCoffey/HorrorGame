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
		playerPrefabs = GameObject.Find ("SpawnManager").GetComponent<SpawnManager> ().playerGameObjectList;
		int i = 0;
		foreach (GameObject player in playerPrefabs) 
		{
			if(player.tag == "Monster")
			{
				CreatePlayerLable(player.GetComponent<MonsterManager>().Name,player.GetComponent<MonsterManager>().Role, playerPrefabs.Count, i);
			}
			else
			{
				CreatePlayerLable(player.GetComponent<Player>().Name, player.GetComponent<Player>().Role, playerPrefabs.Count, i);
			}
			i++;
		}
	}

	void CreatePlayerLable(string playerName, string playerRole, int num, int i)
	{
		GameObject label = (GameObject)Instantiate (playerLabel);
		label.transform.SetParent(playersList.transform.FindChild("PlayersScrolling"), false);
		label.GetComponentInChildren<Text>().font = font;
		label.transform.FindChild ("PlayerName").GetComponent<Text> ().text = playerName;
		label.transform.FindChild ("PlayerRole").GetComponent<Text> ().text = playerRole;
		label.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1 - ((1.0f / (float)num) * i));
		label.GetComponent<RectTransform>().anchorMin = new Vector2(0, (1 - (1.0f / (float)num)) - ((1.0f / (float)num) * i));
		label.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
		label.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
	}

	public void GoToLobby()
	{
		Application.LoadLevel ("MainMenu");
		if (Network.isServer) 
		{
			networkView.RPC("EnableLobby",RPCMode.All);
		}
	}

	[RPC] public void EnableLobby()
	{
		lobbyButton.SetActive (true);
	}
}
