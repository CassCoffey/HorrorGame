using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour {

	public GameObject labelPrefab;
	public GameObject startGameButton;
	public GameObject playerListPanel;
	public Font font;
	
	private List<NetworkPlayer> playerList = new List<NetworkPlayer>();
	private List<GameObject> playerLabels = new List<GameObject>();

	// Update is called once per frame
	void Update () {
	    if (!Network.isServer)
        {
            startGameButton.GetComponent<UnityEngine.UI.Button>().enabled = false;
            startGameButton.GetComponent<UnityEngine.UI.Image>().enabled = false;
            startGameButton.GetComponentInChildren<UnityEngine.UI.Text>().enabled = false;
        }
        else
        {
            startGameButton.GetComponent<UnityEngine.UI.Button>().enabled = true;
            startGameButton.GetComponent<UnityEngine.UI.Image>().enabled = true;
            startGameButton.GetComponentInChildren<UnityEngine.UI.Text>().enabled = true;
        }
	}

//	[RPC] void UpdatePlayerList(NetworkPlayer player) {
//
//		for (int i = 0; i < Network.connections.Length; i++)
//		{
//			Debug.Log (Network.connections[i].ipAddress);
//			GameObject label = (GameObject)Instantiate(labelPrefab);
//			label.transform.SetParent(playerListPanel.transform, false);
//			UnityEngine.UI.Button labelScript = label.GetComponent<UnityEngine.UI.Button>();
//			label.GetComponentInChildren<Text>().font = font;
//			label.GetComponentInChildren<Text>().text = (player.ipAddress);
//			label.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1 - ((1.0f / (float)Network.connections.Length) * i));
//			label.GetComponent<RectTransform>().anchorMin = new Vector2(0, (1 - (1.0f / (float)Network.connections.Length)) - ((1.0f / (float)Network.connections.Length) * i));
//			label.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
//			label.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
//			playerList.Add(label);
//		}
//	}

//	[RPC] void RemovePlayerFromList(NetworkPlayer player) 
//	{
//		for (int i = 0; i< playerList.Count; i++) 
//		{
//			if(player.ipAddress == playerList[i].GetComponentInChildren<Text>().text)
//			{
//				Destroy(playerList[i]);
//				playerList.Remove (playerList[i]);
//			}
//		}
//	}
//
	[RPC] public void ClearPlayerLabels()	{
		for (int i = 0; i < playerLabels.Count; i++)
		{
			Destroy(playerLabels[i]);
		}
			playerLabels.Clear();
	}
	[RPC] void AddPlayerToList(NetworkPlayer player)
	{
		playerList.Add (player);
	}

	[RPC] void RemovePlayerFromList(NetworkPlayer player)
	{
		playerList.Remove (player);
	}

	[RPC] void UpdatePlayerList()
	{
		ClearPlayerLabels();
		for(int i = 0; i < playerList.Count; i++)
		{
			GameObject label = (GameObject)Instantiate(labelPrefab);
			label.transform.SetParent(playerListPanel.transform, false);
			UnityEngine.UI.Button labelScript = label.GetComponent<UnityEngine.UI.Button>();
			label.GetComponentInChildren<Text>().font = font;
			label.GetComponentInChildren<Text>().text = (playerList[i].ipAddress);
			label.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1 - ((1.0f / (float)Network.connections.Length) * i));
			label.GetComponent<RectTransform>().anchorMin = new Vector2(0, (1 - (1.0f / (float)Network.connections.Length)) - ((1.0f / (float)Network.connections.Length) * i));
			label.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
			label.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
			playerLabels.Add(label);
		}
	}

	void OnPlayerConnected(NetworkPlayer player) {
		Debug.Log ("Player Connected!" + player.ipAddress);
		networkView.RPC ("AddPlayerToList", RPCMode.All,player);
		networkView.RPC ("UpdatePlayerList", RPCMode.All);
	}

	void OnPlayerDisconnected(NetworkPlayer player) {
		Debug.Log ("Player Disconnected!" + player.ipAddress);
		networkView.RPC ("RemovePlayerFromList", RPCMode.All,player);
		networkView.RPC ("UpdatePlayerList", RPCMode.All);
	}
}
