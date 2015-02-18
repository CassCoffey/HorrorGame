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
		ResizeScrollingBox (playerPrefabs.Count);
		int i = 0;
		foreach (GameObject player in playerPrefabs) 
		{
			if(player.tag == "Monster")
			{
				CreatePlayerLable(GameObject.Find("NetworkManager").GetComponent<NetworkManager>().playerName, player.GetComponent<MonsterManager>().Name,player.GetComponent<MonsterManager>().Role, playerPrefabs.Count, i);
			}
			else
			{
				CreatePlayerLable(GameObject.Find("NetworkManager").GetComponent<NetworkManager>().playerName, player.GetComponent<Player>().Name, player.GetComponent<Player>().Role, playerPrefabs.Count, i);
			}
			i++;
		}
	}
	
	void CreatePlayerLable(string userName, string playerName, string playerRole, int num, int i)
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
		if (Network.isServer) 
		{
			networkView.RPC("EnableLobby",RPCMode.All);
		}
		Application.LoadLevel ("MainMenu");
	}
	
	[RPC] public void EnableLobby()
	{
		lobbyButton.SetActive (true);
	}
}
