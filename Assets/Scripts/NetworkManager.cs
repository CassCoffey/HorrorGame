﻿using UnityEngine;
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
    // Fields for defining the server.
    public string gameScene = "TestScene";
    public string gameName = "ServerName";
    public string password = "";
    public int maxPlayers = 4;
    public int port = 25000;

    // Defines the prefix of the previous level.
    private int lastLevelPrefix = 0;

    void Awake()
    {
        // Network level loading is done in a separate channel.
        DontDestroyOnLoad(this);
        networkView.group = 1;
    }

	// When creating the server, set a password and register it with the master server.
	public void StartServer() 
    {
        Network.incomingPassword = password;
        Network.InitializeServer(maxPlayers - 1, port, !Network.HavePublicAddress());
        MasterServer.RegisterHost(appId, gameName);
	}

    // Called whenever a input field is changed that has to do with server creation.
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
        }
    }
	
	// Debug info that server has started.
	void OnServerInitialized() 
    {
        Debug.Log("Server Initialized");
	}

    // Disconnects the server and removes it from the master server list.
    public void ShutdownServer()
    {
        Network.Disconnect();
        MasterServer.UnregisterHost();
    }

    // On connecting to a server
    void OnConnectedToServer()
    {
        Debug.Log("Server Joined");
    }

    // Calls an RPC to load the level for all clients.
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

    // The RPC that handles loading a level for all clients.
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