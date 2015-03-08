using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour {

	public GameObject labelPrefab;
    public GameObject chatPrefab;
    public GameObject chatPanel;
    public InputField chatInput;
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
    private float maxChatHeight;
    private float currentChatHeight = 0;

	/// <summary>
	/// If you're the server, update ping for the players.
    /// If enter is being pressed and text has been input, send a chat message.
	/// </summary>
	void Update () 
    {
	    if (Network.isServer)
        {
            if (pingTime > 1)
            {
                pingTime = 0;
                for (int i = 0; i < playerList.Count; i++)
                {
                    if (playerList[i] == Network.player)
                    {
                        networkView.RPC("UpdatePing", RPCMode.All, i, 0);
                    }
                    else
                    {
                        networkView.RPC("UpdatePing", RPCMode.All, i, Network.GetLastPing(playerList[i]));
                    }
                }
            }
            pingTime += Time.deltaTime;
        }
        if (Input.GetButtonDown("Chat") && chatInput.text != "" && EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.name == chatInput.name)
        {
            SendChatMessage(chatInput.text);
            chatInput.text = "";
        }
	}

    /// <summary>
    /// Called when the camera arrives to this menu.
    /// </summary>
    public void OnCameraArrive(Object canvas)
    {
        if ((GameObject)canvas == gameObject)
        {
            /// <summary>
            /// Refreshes the chat, enables settings and start game button for host, adds the host to the list of players and labels
            /// and sets the server name.
            /// </summary>
            if (Network.isServer)
            {
                RefreshChat();
                settingsBlocker.SetActive(false);
                staticServerInfo.SetActive(false);
                dynamicServerInfo.SetActive(true);
                startGameButton.GetComponent<UnityEngine.UI.Button>().enabled = true;
                startGameButton.GetComponent<UnityEngine.UI.Image>().enabled = true;
                startGameButton.GetComponentInChildren<UnityEngine.UI.Text>().enabled = true;
                playerList.Add(Network.player);
                playerNameList.Add(networkManager.GetComponent<NetworkManager>().playerName);
                serverName = networkManager.GetComponent<NetworkManager>().gameName;
                UpdateServerName(serverName);
                ClearPlayerLabels();
                ResizeScrollingBox(1);
                CreatePlayerLabel(playerNameList[0], "0", 1, 0);
            }
            /// <summary>
            /// Refreshes the chat, blocks access to settings and server info, removes the start game button, and adds the player that connected
            /// </summary>
            if (Network.isClient)
            {
                RefreshChat();
                settingsBlocker.SetActive(true);
                staticServerInfo.SetActive(true);
                dynamicServerInfo.SetActive(false);
                startGameButton.GetComponent<UnityEngine.UI.Button>().enabled = false;
                startGameButton.GetComponent<UnityEngine.UI.Image>().enabled = false;
                startGameButton.GetComponentInChildren<UnityEngine.UI.Text>().enabled = false;
                networkView.RPC("AddPlayerName", RPCMode.Server, networkManager.GetComponent<NetworkManager>().playerName);
            }
        }
    }

	/// <summary>
    /// Removes all chat messages and resizes the chat panel
	/// </summary>
    void RefreshChat()
    {
        for (int i = 0; i < chatMessages.Count; i++)
        {
            Destroy(chatMessages[i]);
        }
        chatMessages.Clear();
        float panelHeight = chatPrefab.GetComponent<RectTransform>().rect.height * 20;
        maxChatHeight = panelHeight;
        currentChatHeight = 0;
        chatPanel.transform.FindChild("ChatScrolling").GetComponent<RectTransform>().offsetMax = new Vector2(0, panelHeight);
        chatPanel.transform.FindChild("ChatScrolling").GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
    }

    /// <summary>
    /// Clears the list of players and names.
    /// </summary>
	public void ClearPlayerList()
	{
		playerList.Clear();
        playerNameList.Clear();
	}

	/// <summary>
    /// Clears the list of players, adds the player that connected, then goes through each connection and adds them to the list
	/// </summary>
	void RefreshList()
	{
		playerList.Clear ();
		playerList.Add (Network.player);
		for (int i = 0; i < Network.connections.Length; i++) 
		{
			playerList.Add (Network.connections[i]);
		}
	}

	/// <summary>
    /// Removes the player from the game and updates the player labels
	/// </summary>
    void OnPlayerDisconnected(NetworkPlayer player)
    {
        if (Network.isServer)
        {
            networkView.RPC("RetrieveChatMessage", RPCMode.All, "Server", playerNameList[playerList.IndexOf(player)] + " has disconnected.", Color.green.r, Color.green.g, Color.green.b);
            playerNameList.RemoveAt(playerList.IndexOf(player));
            playerList.Remove(player);
            UpdatePlayerLabels(playerList);
        }
    }

	/// <summary>
    /// Clears all of the labels, resizes the scrolling box, and adds a player label for each person connected
	/// </summary>
	void UpdatePlayerLabels(List<NetworkPlayer> serverPlayerList)
	{
		networkView.RPC ("ClearPlayerLabels", RPCMode.All);
		networkView.RPC ("ResizeScrollingBox", RPCMode.All,serverPlayerList.Count);
		for(int i = 0; i < serverPlayerList.Count; i++)
		{
			networkView.RPC("CreatePlayerLabel", RPCMode.All, playerNameList[i], Network.GetLastPing(serverPlayerList[i]).ToString(), serverPlayerList.Count, i);
		}
	}

    /// <summary>
    /// Updatess the server info on clients.
    /// </summary>
    public void UpdateServerInfo(string info)
    {
        networkView.RPC("RetrieveServerInfo", RPCMode.OthersBuffered, info);
    }

    /// <summary>
    /// Updatess the server options on clients.
    /// </summary>
    public void UpdateServerOptions(GameObject option)
    {
        networkView.RPC("RetrieveServerOptions", RPCMode.OthersBuffered, option.name, option.transform.FindChild("Text").GetComponent<Text>().text);
    }

    /// <summary>
    /// Sends a chat message to everyone else.
    /// </summary>
    public void SendChatMessage(string message)
    {
        networkView.RPC("RetrieveChatMessage", RPCMode.All, networkManager.GetComponent<NetworkManager>().playerName, message, 0.2f, 0.2f, 0.2f);
    }

	/// <summary>
    /// Adds the chat message to the chat message array and moves all chat messages up in the scrolling box.
    /// Destroys any chat messages past the maximum chat height
    /// Allows changing color of messages.
    /// </summary>
    [RPC] void RetrieveChatMessage(string player, string message, float r, float g, float b)
    {
        GameObject chat = (GameObject)Instantiate(chatPrefab);
        chat.transform.SetParent(chatPanel.transform.FindChild("ChatScrolling"), false);
        chat.GetComponentInChildren<Text>().font = font;
        chat.transform.FindChild("NameText").GetComponent<Text>().text = player;
        chat.transform.FindChild("ChatText").GetComponent<Text>().color = new Color(r, g, b);
        chat.transform.FindChild("ChatText").GetComponent<Text>().text = message;
        chat.GetComponent<RectTransform>().anchorMax = new Vector2(1, chat.transform.FindChild("ChatText").GetComponent<Text>().preferredHeight / maxChatHeight);
        chat.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
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
        for (int i = chatMessages.Count - 2; i >= 0; i--)
        {
            chatMessages[i].GetComponent<RectTransform>().anchorMax = new Vector2(1, chatMessages[i+1].GetComponent<RectTransform>().anchorMax.y + (chatMessages[i].transform.FindChild("ChatText").GetComponent<Text>().preferredHeight / maxChatHeight));
            chatMessages[i].GetComponent<RectTransform>().anchorMin = new Vector2(0, chatMessages[i+1].GetComponent<RectTransform>().anchorMax.y);
        }
        currentChatHeight += chat.transform.FindChild("ChatText").GetComponent<Text>().preferredHeight;
        float panelHeight = chatPanel.GetComponent<RectTransform>().rect.height;
        chatPanel.GetComponent<ScrollRect>().enabled = (currentChatHeight > panelHeight);
        while (currentChatHeight > maxChatHeight)
        {
            currentChatHeight -= chatMessages[0].transform.FindChild("ChatText").GetComponent<Text>().preferredHeight;
            Destroy(chatMessages[0]);
            chatMessages.RemoveAt(0);
        }
    }


    /// <summary>
    /// RPCs for retrieving server info and options.
    /// </summary>
    
    [RPC] void RetrieveServerInfo(string info)
    {
        staticServerInfo.GetComponent<Text>().text = info;
    }

    [RPC] void RetrieveServerOptions(string option, string value)
    {
        settingsPanel.transform.FindChild(option).FindChild("Text").GetComponent<Text>().text = value;
        settingsPanel.transform.FindChild(option).FindChild("Placeholder").GetComponent<Text>().text = "";
    }


	/// <summary>
    /// Resizes the player label scrolling box to the number of players connected
	/// </summary>
	[RPC] void ResizeScrollingBox(int numOfPlayers)
	{
		float panelHeight = labelPrefab.GetComponent<RectTransform>().rect.height * numOfPlayers;
		float currentHeight = playerListPanel.GetComponent<RectTransform>().rect.height;
		playerListPanel.GetComponent<ScrollRect>().enabled = (panelHeight > currentHeight);
		playerListPanel.transform.FindChild("PlayersScrolling").GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
		playerListPanel.transform.FindChild("PlayersScrolling").GetComponent<RectTransform>().offsetMin = new Vector2(0, currentHeight - panelHeight);
	}

	/// <summary>
    /// Adds a player label to the list of labels
	/// </summary>
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

	/// <summary>
    /// Adds the player that connected to the list and updates the labels
	/// </summary>
    [RPC] void AddPlayerName(string playerName)
    {
        RefreshList();
        playerNameList.Add(playerName);
		UpdatePlayerLabels (playerList);
        networkView.RPC("RetrieveChatMessage", RPCMode.All, "Server", playerName + " has joined.", Color.green.r, Color.green.g, Color.green.b);
    }

    /// <summary>
    /// Updates the ping of that index.
    /// </summary>
    [RPC] void UpdatePing(int index, int ping)
    {
        if (playerLabels.Count > index)
        {
            playerLabels[index].transform.FindChild("PlayerPing").GetComponent<Text>().text = ping.ToString();
        }
    }

    /// <summary>
    /// Updates the server name.
    /// </summary>
    [RPC] void UpdateServerName(string name)
    {
        serverNamePanel.transform.FindChild("ServerName").GetComponent<Text>().text = name;
    }

	/// <summary>
    /// Removes all player labels
	/// </summary>
    [RPC] void ClearPlayerLabels()
    {
        for (int i = 0; i < playerLabels.Count; i++)
        {
            Destroy(playerLabels[i]);
        }
        playerLabels.Clear();
    }
}