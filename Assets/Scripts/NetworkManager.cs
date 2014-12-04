using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour {

    private const string typeName = "HorrorGameTest";
    private const string gameName = "TestRoom";
    public string gameScene = "TestScene";
    public GameObject dummy;
    public Canvas canvas;
    public Font font;

    private HostData[] hostList;
    private List<GameObject> buttons = new List<GameObject>();
    private int lastLevelPrefix = 0;
    private HostData host;

    void Awake()
    {
        // Network level loading is done in a separate channel.
        DontDestroyOnLoad(this);
        networkView.group = 1;
    }

	// Use this for initialization
	public void StartServer() 
    {
        Network.InitializeServer(4, 25000, !Network.HavePublicAddress());
        MasterServer.RegisterHost(typeName, gameName);
	}
	
	// Update is called once per frame
	void OnServerInitialized() 
    {
        Debug.Log("Server Initialized");
	}

    // Refreshes the list of hosts.
    public void RefreshHostList()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            Destroy(buttons[i]);
        }
        buttons.Clear();
        MasterServer.RequestHostList(typeName);
    }

    // Called by the master server.
    void OnMasterServerEvent(MasterServerEvent msEvent)
    {
        if (msEvent == MasterServerEvent.HostListReceived)
        {
            hostList = MasterServer.PollHostList();
            if (hostList != null)
            {
                for (int i = 0; i < hostList.Length; i++)
                {
                    GameObject button = (GameObject)Instantiate(dummy);
                    button.transform.SetParent(canvas.transform, false);
                    UnityEngine.UI.Button buttonScript = button.GetComponent<UnityEngine.UI.Button>();
                    button.GetComponentInChildren<Text>().font = font;
                    button.GetComponentInChildren<Text>().text = hostList[i].gameName;
                    button.GetComponent<RectTransform>().anchoredPosition.Set(10, 10);
                    HostData data = hostList[i];
                    AddListener(buttonScript, data);
                    buttons.Add(button);
                }
            }
        }
    }

    public void AddListener(UnityEngine.UI.Button button, HostData data)
    {
        button.onClick.AddListener(() => setHost(data));
    }

    public void setHost(HostData data)
    {
        host = data;
    }

    // Joins the specified host.
    public void JoinServer()
    {
        Debug.Log("Attempting to join server.");
        Network.Connect(host);
    }

    public void ShutdownServer()
    {
        Network.Disconnect();
        MasterServer.UnregisterHost();
    }

    // On connecting to a server, spawn a player.
    void OnConnectedToServer()
    {
        Debug.Log("Server Joined");
    }

    public void LoadLevel()
    {
        Network.RemoveRPCsInGroup(0);
        Network.RemoveRPCsInGroup(1);
        networkView.RPC("LoadLevelRPC", RPCMode.AllBuffered, gameScene, lastLevelPrefix + 1);
    }

    // Called when disconnected from a server. Will perform cleanup tasks.
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