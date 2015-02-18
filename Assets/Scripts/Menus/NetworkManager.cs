using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour {

    // AppID for identifying the game on the master server.
    private static string appId = "285263d0-0de6-43a5-b2df-33bc4f463a63";
    public static string AppId 
    {
        get
        {
            return appId;
        }
        private set
        {
            appId = value;
        }
    }
    // Version number management
    private static string version = "v0.02";
    public static string Version
    {
        get
        {
            return version;
        }
        private set
        {
            version = value;
        }
    }
    // Fields for defining the server.
    public string gameScene = "TestScene";
    public string gameName = "ServerName";
    public string password = "";
    public int maxPlayers = 4;
    public int port = 25000;
    public string playerName = "Player Name";

    public Text versionText;

    // Defines the prefix of the previous level.
    private int lastLevelPrefix = 0;

    /// <summary>
    /// When the script wakes up, change the group of this network view.
    /// </summary>
    void Awake()
    {
        // Network level loading is done in a separate channel.
        networkView.group = 1;
    }

    /// <summary>
    /// On start, update the player's username and version text.
    /// </summary>
    void Start()
    {
        if (PlayerPrefs.HasKey("UserName"))
        {
            playerName = PlayerPrefs.GetString("UserName");
        }
        versionText.text = Version;
        DontDestroyOnLoad(this);
    }

    /// <summary>
    /// Sets the player's username.
    /// </summary>
    /// <param name="newName">The new username.</param>
    public void SetPlayerName(string newName)
    {
        playerName = newName;
        PlayerPrefs.SetString("UserName", newName);
        PlayerPrefs.Save();
    }

	/// <summary>
    /// When creating the server, set a password and register it with the master server.
	/// </summary>
	public void StartServer() 
    {
        Network.incomingPassword = password;
        Network.InitializeServer(maxPlayers - 1, port, !Network.HavePublicAddress());
        MasterServer.RegisterHost(appId, gameName, Version);
	}

    /// <summary>
    /// Called whenever a input field is changed that has to do with server creation.
    /// </summary>
    /// <param name="field">TThe field that has been changed.</param>
    public void SetOption(InputField field)
    {
        switch(field.name)
        {
            case "NameInputField":
                gameName = field.text;
                if (gameName.Equals(""))
                {
                    gameName = "ServerName";
                }
                break;
            case "PortInputField":
                int portNum;
                if (int.TryParse(field.text, out portNum))
                {
                    port = portNum;
                }
                else
                {
                    port = 25000;
                }
                break;
            case "PlayerInputField":
                int maxPlayersNum;
                if (int.TryParse(field.text, out maxPlayersNum))
                {
                    maxPlayers = maxPlayersNum;
                }
                else
                {
                    maxPlayers = 4;
                }
                break;
            case "PasswordInputField":
                password = field.text;
                break;
            case "TimeInputField":
                PlayerPrefs.SetString("BoatTime", field.text);
                break;
        }
    }
	
	/// <summary>
    /// Debug info that server has started.
	/// </summary>
	void OnServerInitialized() 
    {
        Debug.Log("Server Initialized");
	}

    /// <summary>
    /// Disconnects the server and removes it from the master server list.
    /// </summary>
    public void ShutdownServer()
    {
        Network.Disconnect();
        MasterServer.UnregisterHost();
    }

    /// <summary>
    /// On connecting to a server
    /// </summary>
    void OnConnectedToServer()
    {
        Debug.Log("Server Joined");
    }

    /// <summary>
    /// Calls an RPC to load the level for all clients.
    /// </summary>
    public void LoadLevel()
    {
        Network.RemoveRPCsInGroup(0);
        Network.RemoveRPCsInGroup(1);
        networkView.RPC("LoadLevelRPC", RPCMode.AllBuffered, gameScene, lastLevelPrefix + 1);
    }

    /// <summary>
    /// Called when disconnected from a server.
    /// </summary>
    /// <param name="info">Information about the disconnect.</param>
    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        if (Network.isServer)
        {
            Debug.Log("Successfully shut down server.");
        }
        else
        {
            if (info == NetworkDisconnection.Disconnected)
            {
                Debug.Log("Successfully terminated connection to server.");
            }
            else
            {
                Debug.Log("Lost connection to host.");
            }
        }
    }

    public void ReturnToLobby()
    {
        Debug.Log("Returning to Lobby");
        Network.RemoveRPCsInGroup(0);
        Network.RemoveRPCsInGroup(1);
        StartCoroutine(ReturnedToLobby());
    }

    [RPC] IEnumerator ReturnedToLobby()
    {
        Network.SetLevelPrefix(0);
        Application.LoadLevel("MainMenu");

        yield return null;
        yield return null;

        // Allow receiving data again
        Network.isMessageQueueRunning = true;
        // Now the level has been loaded and we can start sending out data to clients
        Network.SetSendingEnabled(0, true);

        MasterServer.UnregisterHost();
        Network.maxConnections = maxPlayers;
        MasterServer.RegisterHost(NetworkManager.AppId, gameName, version);

        GameObject.Find("ScreenManager").GetComponent<ScreenManager>().MoveCameraTo(GameObject.Find("Lobby"));

        Destroy(gameObject);
    }

    /// <summary>
    /// The RPC that handles loading a level for all clients.
    /// http://docs.unity3d.com/Manual/net-NetworkLevelLoad.html
    /// </summary>
    /// <param name="level">The level to load.</param>
    /// <param name="levelPrefix">The prefix of that level.</param>
    /// <returns></returns>
    [RPC] IEnumerator LoadLevelRPC(string level, int levelPrefix)
    {
        lastLevelPrefix = levelPrefix;

        // There is no reason to send any more data over the network on the default channel,
        // because we are about to load the level, thus all those objects will get deleted anyway
        Network.SetSendingEnabled(0, false);    

        // We need to stop receiving because first the level must be loaded first.
        // Once the level is loaded, rpc's and other state update attached to objects in the level are allowed to fire
        Network.isMessageQueueRunning = false;
        
        // All network views loaded from a level will get a prefix into their NetworkViewID.
        // This will prevent old updates from clients leaking into a newly created scene.
        Network.SetLevelPrefix(levelPrefix);
        Application.LoadLevel(level);
        yield return null;
        yield return null;

        // Allow receiving data again
        Network.isMessageQueueRunning = true;
        // Now the level has been loaded and we can start sending out data to clients
        Network.SetSendingEnabled(0, true);

        foreach (var go in FindObjectsOfType<GameObject>())
        {
            go.SendMessage("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver); 
        }     
    }
}