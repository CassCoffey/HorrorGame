using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour {

	public GameObject labelPrefab;
    public GameObject chatPrefab;
    public GameObject chatPanel;
	public GameObject startGameButton;
    public GameObject settingsBlocker;
    public GameObject settingsPanel;
    public GameObject dynamicServerInfo;
    public GameObject staticServerInfo;
	public GameObject playerListPanel;
	public GameObject serverNamePanel;
	public GameObject networkManager;
	public string serverName;
	public Font font;
	private float pingTime;
	
	private List<NetworkPlayer> playerList = new List<NetworkPlayer>();
    private List<string> playerNameList = new List<string>();
	private List<GameObject> playerLabels = new List<GameObject>();
    private List<GameObject> chatMessages = new List<GameObject>();

	// Update is called once per frame
	void Update () 
    {
	    if (Network.isServer)
        {
            if (pingTime > 1)
            {
                pingTime = 0;
                for (int i = 0; i < playerList.Count; i++)
                {
                    Debug.Log("Looping through players" + playerList[i].ipAddress + " " + Network.GetLastPing(playerList[i]));
                    networkView.RPC("UpdatePing", RPCMode.All, i, Network.GetLastPing(playerList[i]));
                }
            }
            pingTime += Time.deltaTime;
        }
	}	

	public void ClearPlayerList()
	{
		playerList.Clear();
        playerNameList.Clear();
	}

    void OnConnectedToServer()
    {
        settingsBlocker.SetActive(true);
        staticServerInfo.SetActive(true);
        dynamicServerInfo.SetActive(false);
        startGameButton.GetComponent<UnityEngine.UI.Button>().enabled = false;
        startGameButton.GetComponent<UnityEngine.UI.Image>().enabled = false;
        startGameButton.GetComponentInChildren<UnityEngine.UI.Text>().enabled = false;
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

    void OnPlayerConnected(NetworkPlayer player)
    {
        Debug.Log("Player Connected!" + player.ipAddress);
        if (Network.isServer)
        {
            RefreshList();
        }
    }

    void OnPlayerDisconnected(NetworkPlayer player)
    {
        Debug.Log("Player Disconnected!" + player.ipAddress);
        if (Network.isServer)
        {
            playerNameList.RemoveAt(playerList.IndexOf(player));
            playerList.Remove(player);
            UpdatePlayerLabels(playerList);
        }
    }

	void UpdatePlayerLabels(List<NetworkPlayer> serverPlayerList)
	{
		networkView.RPC ("ClearPlayerLabels", RPCMode.All);
		networkView.RPC ("ResizeScrollingBox", RPCMode.All,serverPlayerList.Count);
		for(int i = 0; i < serverPlayerList.Count; i++)
		{
			networkView.RPC("CreatePlayerLabel", RPCMode.All, playerNameList[i], Network.GetLastPing(serverPlayerList[i]).ToString(), serverPlayerList.Count, i);
		}
	}

    void OnServerInitialized()
    {
        settingsBlocker.SetActive(false);
        staticServerInfo.SetActive(false);
        dynamicServerInfo.SetActive(true);
        startGameButton.GetComponent<UnityEngine.UI.Button>().enabled = true;
        startGameButton.GetComponent<UnityEngine.UI.Image>().enabled = true;
        startGameButton.GetComponentInChildren<UnityEngine.UI.Text>().enabled = true;
        playerList.Add(Network.player);
        playerNameList.Add(networkManager.GetComponent<NetworkManager>().playerName);
        UpdatePlayerLabels(playerList);
        serverName = networkManager.GetComponent<NetworkManager>().gameName;
        networkView.RPC("UpdateServerName", RPCMode.AllBuffered, serverName);
    }

    public void UpdateServerInfo(string info)
    {
        networkView.RPC("RetrieveServerInfo", RPCMode.OthersBuffered, info);
    }

    public void UpdateServerOptions(GameObject option)
    {
        networkView.RPC("RetrieveServerOptions", RPCMode.OthersBuffered, option.name, option.transform.FindChild("Text").GetComponent<Text>().text);
    }

    public void SendChatMessage(string message)
    {
        networkView.RPC("RetrieveChatMessage", RPCMode.All, networkManager.name, message);
    }

    [RPC] void RetrieveChatMessage(string player, string message)
    {
        float panelHeight = chatPrefab.GetComponent<RectTransform>().rect.height * (chatMessages.Count + 1);
        float currentHeight = chatPanel.GetComponent<RectTransform>().rect.height;
        chatPanel.GetComponent<ScrollRect>().enabled = (panelHeight > currentHeight);
        chatPanel.transform.FindChild("ChatScrolling").GetComponent<RectTransform>().offsetMax = new Vector2(0, panelHeight);
        chatPanel.transform.FindChild("ChatScrolling").GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
        GameObject chat = (GameObject)Instantiate(chatPrefab);
        chat.transform.SetParent(chatPanel.transform.FindChild("ChatScrolling"), false);
        chat.GetComponentInChildren<Text>().font = font;
        chat.transform.FindChild("NameText").GetComponent<Text>().text = player;
        chat.transform.FindChild("ChatText").GetComponent<Text>().text = message;
        chat.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1 - ((1.0f / (float)chatMessages.Count) * (chatMessages.Count + 1)));
        chat.GetComponent<RectTransform>().anchorMin = new Vector2(0, (1 - (1.0f / (float)chatMessages.Count)) - ((1.0f / (float)chatMessages.Count) * (chatMessages.Count + 1)));
        chat.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
        chat.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
        if ((chatMessages.Count + 1) % 2 == 0)
        {
            chat.GetComponent<Image>().color = new UnityEngine.Color(0.4f, 0.4f, 0.4f, 0.7f);
        }
        else
        {
            chat.GetComponent<Image>().color = new UnityEngine.Color(0.6f, 0.6f, 0.6f, 0.7f);
        }
        chatMessages.Add(chat);
    }

    [RPC] void RetrieveServerInfo(string info)
    {
        staticServerInfo.GetComponent<Text>().text = info;
    }

    [RPC]
    void RetrieveServerOptions(string option, string value)
    {
        settingsPanel.transform.FindChild(option).FindChild("Text").GetComponent<Text>().text = value;
    }

	[RPC] void ResizeScrollingBox(int numOfPlayers)
	{
		float panelHeight = labelPrefab.GetComponent<RectTransform>().rect.height * numOfPlayers;
		float currentHeight = playerListPanel.GetComponent<RectTransform>().rect.height;
		playerListPanel.GetComponent<ScrollRect>().enabled = (panelHeight > currentHeight);
		playerListPanel.transform.FindChild("PlayersScrolling").GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
		playerListPanel.transform.FindChild("PlayersScrolling").GetComponent<RectTransform>().offsetMin = new Vector2(0, currentHeight - panelHeight);
	}

	[RPC] void CreatePlayerLabel(string playerText,string playerPing, int numOfPlayers, int i)
    {
		GameObject label = (GameObject)Instantiate(labelPrefab);
		label.transform.SetParent(playerListPanel.transform.FindChild("PlayersScrolling"), false);
		label.GetComponentInChildren<Text>().font = font;
		label.transform.FindChild("PlayerName").GetComponent<Text>().text = playerText;
		label.transform.FindChild("PlayerPing").GetComponent<Text>().text = playerPing;
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
		UpdatePlayerLabels (playerList);
		Debug.Log (playerName);
    }

    [RPC]
    void UpdatePing(int index, int ping)
    {
        playerLabels[index].transform.FindChild("PlayerPing").GetComponent<Text>().text = ping.ToString();
    }

    [RPC]
    void UpdateServerName(string name)
    {
        serverNamePanel.transform.FindChild("ServerName").GetComponent<Text>().text = name;
    }

    [RPC]
    void ClearPlayerLabels()
    {
        for (int i = 0; i < playerLabels.Count; i++)
        {
            Destroy(playerLabels[i]);
        }
        playerLabels.Clear();
    }
}