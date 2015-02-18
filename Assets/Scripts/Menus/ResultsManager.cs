using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class ResultsManager : MonoBehaviour {
	
	public GameObject lobbyButton;
	public GameObject playersList;
	public GameObject playerLabel;
	public GameObject deathsLog;
	public GameObject deathLabel;
	public Font font;
	public List<NetworkPlayer> players = new List<NetworkPlayer>();
	public List<Death> deathList;

	private DeathLog log;
	
	void Start()
	{
        Camera.SetupCurrent(GetComponent<Camera>());
        Camera.main.gameObject.SetActive(false);
        GetComponent<AudioListener>().enabled = true;
        GetComponent<Camera>().enabled = true;
        Screen.lockCursor = false;
        Screen.showCursor = true;
        if (Network.isServer)
        {
            players.Add(Network.player);
            foreach (NetworkPlayer player in Network.connections)
            {
                players.Add(player);
            }
            networkView.RPC("CreateList", RPCMode.AllBuffered, players.ToArray());
        }
	}
	
	void CreatePlayerLabel(string userName, string playerName, string playerRole, int i)
	{
		int num = players.Count;
		GameObject label = (GameObject)Instantiate (playerLabel);
		label.transform.SetParent(playersList.transform.FindChild("PlayersScrolling"), false);
		label.GetComponentInChildren<Text>().font = font;
		label.transform.FindChild ("UserName").GetComponent<Text> ().text = userName;
        if (playerRole == "Monster")
        {
            label.transform.FindChild("PlayerName").GetComponent<Text>().text = "Monster";
        }
		else
        {
            label.transform.FindChild("PlayerName").GetComponent<Text>().text = playerName;
        }
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

	void CreateDeathLabel(float time, string player, string killer, int i)
	{
		int num = deathList.Count;
		GameObject label = (GameObject)Instantiate (deathLabel);
		label.transform.SetParent(deathsLog.transform.FindChild("DeathsScrolling"), false);
		label.GetComponentInChildren<Text>().font = font;
		label.transform.FindChild ("DeathDescription").GetComponent<Text> ().text = "Time: " + time + " Player: " + player + " Killer: " + killer;
		label.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1 - ((1.0f / (float)num) * i));
		label.GetComponent<RectTransform>().anchorMin = new Vector2(0, (1 - (1.0f / (float)num)) - ((1.0f / (float)num) * i));
		label.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
		label.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
	}
	
	void ResizeScrollingBox(int numOfPlayers)
	{
		float panelHeight = playerLabel.GetComponent<RectTransform>().rect.height * numOfPlayers;
		float currentHeight = playersList.GetComponent<RectTransform>().rect.height;
		playersList.GetComponent<ScrollRect>().enabled = (panelHeight > currentHeight);
		playersList.transform.FindChild("PlayersScrolling").GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
		playersList.transform.FindChild("PlayersScrolling").GetComponent<RectTransform>().offsetMin = new Vector2(0, currentHeight - panelHeight);
	}

	void ResizeDeathsScrollingBox(int numOfDeaths)
	{
		float panelHeight = deathLabel.GetComponent<RectTransform>().rect.height * numOfDeaths;
		float currentHeight = deathsLog.GetComponent<RectTransform>().rect.height;
		deathsLog.GetComponent<ScrollRect>().enabled = (panelHeight > currentHeight);
		deathsLog.transform.FindChild("DeathsScrolling").GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
		deathsLog.transform.FindChild("DeathsScrolling").GetComponent<RectTransform>().offsetMin = new Vector2(0, currentHeight - panelHeight);
	}

	public void GoToLobby()
	{
        if (Network.isServer)
        {
            networkView.RPC("EnableLobby", RPCMode.All);
        }
        GameObject.Find("NetworkManager").GetComponent<NetworkManager>().ReturnToLobby();
	}

    [RPC] void CreateList(NetworkPlayer[] networkPlayers)
    {
        players = new List<NetworkPlayer>();
        foreach (NetworkPlayer player in networkPlayers)
        {
            players.Add(player);
        }
        if (Network.isServer)
        {
            lobbyButton.SetActive(true);
        }
        log = GameObject.Find("GameManager").GetComponent<DeathLog>();
        deathList = log.deaths;
        ResizeScrollingBox(players.Count);
        ResizeDeathsScrollingBox(deathList.Count);
        int i = 0;
        int j = 0;
        foreach (NetworkPlayer player in players)
        {
            CreatePlayerLabel((string)GameObject.Find("SpawnManager").GetComponent<SpawnManager>().userNames[player], (string)GameObject.Find("SpawnManager").GetComponent<SpawnManager>().playerNames[player], (string)GameObject.Find("SpawnManager").GetComponent<SpawnManager>().roles[player], i);
            i++;
        }
        foreach (Death death in deathList)
        {
            CreateDeathLabel(death.DeathTime, death.PlayerName, death.Killer, j);
            j++;
        }
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
