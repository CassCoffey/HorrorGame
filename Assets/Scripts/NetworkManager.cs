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
    public GameObject playerPrefab;
    public Canvas canvas;
    public Font font;

    private HostData[] hostList;
    private List<GameObject> buttons = new List<GameObject>();

    void Start()
    {
        DontDestroyOnLoad(this);
    }

	// Use this for initialization
	public void StartServer() 
    {
        Network.InitializeServer(4, 25000, !Network.HavePublicAddress());
        MasterServer.RegisterHost(typeName, gameName);
        
	}
	//Loads the level to start the game
	public void LoadLevel()
	{
		Application.LoadLevel(gameScene);
		SpawnPlayer();
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
        button.onClick.AddListener(() => JoinServer(data));
    }

    // Joins the specified host.
    public void JoinServer(HostData hostData)
    {
        Debug.Log("Attempting to join server.");
        Network.Connect(hostData);
    }

    // On connecting to a server, spawn a player.
    void OnConnectedToServer()
    {
        Debug.Log("Server Joined");
        Application.LoadLevel(gameScene);
        SpawnPlayer();
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
        Application.LoadLevel("MainMenu");
    }

    // Create a player object.
    private void SpawnPlayer()
    {
        Debug.Log("Spawning Player");
        GameObject player = (GameObject)Network.Instantiate(playerPrefab, new Vector3(0f, 2f, 0f), Quaternion.identity, 0);
    }
}