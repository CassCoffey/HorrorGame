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

	void OnServerInitialized()
	{
		playerList.Add (Network.player);
		UpdatePlayerLabels (playerList);
	}

	[RPC] void ClearPlayerLabels()	
	{
		for (int i = 0; i < playerLabels.Count; i++)
		{
			Destroy(playerLabels[i]);
		}
			playerLabels.Clear();
	}

	public void ClearPlayerList()
	{
		playerList.Clear ();
	}

	void RefreshList()
	{
		playerList.Clear ();
		playerList.Add (Network.player);
		for (int i = 0; i < Network.connections.Length; i++) 
		{
			playerList.Add (Network.connections[i]);
		}
	}
	

	void UpdatePlayerLabels(List<NetworkPlayer> serverPlayerList)
	{
		networkView.RPC ("ClearPlayerLabels", RPCMode.All);
		for(int i = 0; i < serverPlayerList.Count; i++)
		{
			networkView.RPC("CreatePlayerLabel", RPCMode.All, serverPlayerList[i].ipAddress, serverPlayerList.Count, i);
		}
	}

	[RPC] void CreatePlayerLabel(string playerText, int numOfPlayers, int i){
		GameObject label = (GameObject)Instantiate(labelPrefab);
		label.transform.SetParent(playerListPanel.transform.FindChild("PlayersScrolling"), false);
		UnityEngine.UI.Button labelScript = label.GetComponent<UnityEngine.UI.Button>();
		label.GetComponentInChildren<Text>().font = font;
		label.GetComponentInChildren<Text>().text = playerText;
		label.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1 - ((1.0f / (float)numOfPlayers) * i));
		label.GetComponent<RectTransform>().anchorMin = new Vector2(0, (1 - (1.0f / (float)numOfPlayers)) - ((1.0f / (float)numOfPlayers) * i));
		label.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
		label.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
		playerLabels.Add(label);
	 }

	void OnPlayerConnected(NetworkPlayer player) {
		Debug.Log ("Player Connected!" + player.ipAddress);
		if (Network.isServer) 
		{
			RefreshList();
			UpdatePlayerLabels (playerList);
		}
	}

	void OnPlayerDisconnected(NetworkPlayer player) {
		Debug.Log ("Player Disconnected!" + player.ipAddress);
		if (Network.isServer) 
		{
			playerList.Remove (player);
			UpdatePlayerLabels (playerList);
		}
	}
}