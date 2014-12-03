﻿using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {

    private const string typeName = "HorrorGameTest";
    private const string gameName = "TestRoom";
    public GameObject playerPrefab;

    private HostData[] hostList;

	// Use this for initialization
	private void StartServer() 
    {
        Network.InitializeServer(4, 25000, !Network.HavePublicAddress());
        MasterServer.RegisterHost(typeName, gameName);
	}
	
	// Update is called once per frame
	void OnServerInitialized() 
    {
        Debug.Log("Server Initialized");
        SpawnPlayer();
	}

    private void RefreshHostList()
    {
        MasterServer.RequestHostList(typeName);
    }

    void OnMasterServerEvent(MasterServerEvent msEvent)
    {
        if (msEvent == MasterServerEvent.HostListReceived)
            hostList = MasterServer.PollHostList();
    }

    private void JoinServer(HostData hostData)
    {
        Network.Connect(hostData);
    }

    void OnConnectedToServer()
    {
        Debug.Log("Server Joined");
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        GameObject player = (GameObject)Network.Instantiate(playerPrefab, new Vector3(0f, 2f, 0f), Quaternion.identity, 0);
        Camera.main.enabled = false;
        MouseLook lookScript = player.GetComponentInChildren<Camera>().GetComponent<MouseLook>();
        FirstPersonHeadBob headBob = player.GetComponent< FirstPersonHeadBob>();
        headBob.head = player.GetComponentInChildren<Camera>().transform;
        lookScript.enabled = true;
    }

    void OnGUI()
    {
        if (!Network.isClient && !Network.isServer)
        {
            if (GUI.Button(new Rect(100, 100, 250, 100), "Start Server"))
            {
                StartServer();
            }
            if (GUI.Button(new Rect(100, 250, 250, 100), "Refresh Hosts"))
                RefreshHostList();

            if (hostList != null)
            {
                for (int i = 0; i < hostList.Length; i++)
                {
                    if (GUI.Button(new Rect(400, 100 + (110 * i), 300, 100), hostList[i].gameName))
                        JoinServer(hostList[i]);
                }
            }
        }
    }
}
