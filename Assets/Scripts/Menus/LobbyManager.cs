using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour {

	public GameObject labelPrefab;
	public GameObject startGameButton;
	public GameObject playerListPanel;
	public GameObject serverNamePanel;
	public GameObject networkManager;
	public string serverName;
	public Font font;
	
	private List<NetworkPlayer> playerList = new List<NetworkPlayer>();
    private List<string> playerNameList = new List<string>();
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
        playerNameList.Add(networkManager.GetComponent<NetworkManager>().playerName);
		UpdatePlayerLabels (playerList);
		serverName = networkManager.GetComponent<NetworkManager> ().gameName;
		networkView.RPC ("UpdateServerName",RPCMode.AllBuffered, serverName);
	}

	[RPC] void UpdateServerName(string name)
    {
		serverNamePanel.transform.FindChild ("ServerName").GetComponent<Text> ().text = name;
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
		playerList.Clear();
        playerNameList.Clear();
	}

    void OnConnectedToServer()
    {
        networkView.RPC("AddPlayerName", RPCMode.Server, networkManager.GetComponent<NetworkManager>().playerName);
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
		networkView.RPC ("ResizeScrollingBox", RPCMode.All,serverPlayerList.Count);
		for(int i = 0; i < serverPlayerList.Count; i++)
		{
			networkView.RPC("CreatePlayerLabel", RPCMode.All, playerNameList[i], serverPlayerList.Count, i);
		}
	}

	[RPC] void ResizeScrollingBox(int numOfPlayers)
	{
		float panelHeight = labelPrefab.GetComponent<RectTransform>().rect.height * numOfPlayers;
		float currentHeight = playerListPanel.GetComponent<RectTransform>().rect.height;
		playerListPanel.GetComponent<ScrollRect>().enabled = (panelHeight > currentHeight);
		playerListPanel.transform.FindChild("PlayersScrolling").GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
		playerListPanel.transform.FindChild("PlayersScrolling").GetComponent<RectTransform>().offsetMin = new Vector2(0, currentHeight - panelHeight);
	}

	[RPC] void CreatePlayerLabel(string playerText, int numOfPlayers, int i)
    {
		GameObject label = (GameObject)Instantiate(labelPrefab);
		label.transform.SetParent(playerListPanel.transform.FindChild("PlayersScrolling"), false);
		UnityEngine.UI.Text labelScript = label.GetComponent<UnityEngine.UI.Text>();
		label.GetComponentInChildren<Text>().font = font;
		label.GetComponentInChildren<Text>().text = playerText;
		label.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1 - ((1.0f / (float)numOfPlayers) * i));
		label.GetComponent<RectTransform>().anchorMin = new Vector2(0, (1 - (1.0f / (float)numOfPlayers)) - ((1.0f / (float)numOfPlayers) * i));
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
		playerLabels.Add(label);
	}

    [RPC] void AddPlayerName(string playerName)
    {
        playerNameList.Add(playerName);
    }

	void OnPlayerConnected(NetworkPlayer player) 
    {
		Debug.Log ("Player Connected!" + player.ipAddress);
		if (Network.isServer) 
		{
			RefreshList();
			UpdatePlayerLabels (playerList);
		}
	}

	void OnPlayerDisconnected(NetworkPlayer player) 
    {
		Debug.Log ("Player Disconnected!" + player.ipAddress);
		if (Network.isServer) 
		{
            playerNameList.RemoveAt(playerList.IndexOf(player));
			playerList.Remove (player);
			UpdatePlayerLabels (playerList);
		}
	}
}