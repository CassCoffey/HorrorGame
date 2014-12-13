using UnityEngine;
using System.Collections;

public class BasicConnection : MonoBehaviour {
	
	public string IP = "127.0.0.1";
	public int Port = 25001;
	
	void Start () {
	
	}
	
	void Update () {
	
	}
	
	void OnGUI()
	{
		if(Network.peerType == NetworkPeerType.Disconnected)
		{
			if(GUI.Button(new Rect(100,100,100,25), "Start Client"))
			{
				//Network.natFacilitatorIP = IP;
				//Network.natFacilitatorPort = Port;
			   	Network.Connect(IP, Port);
			}
			if(GUI.Button(new Rect(100,125,100,25), "Start Server"))
			{
				//Network.natFacilitatorIP = IP;
				//Network.natFacilitatorPort = Port;
			   	Network.InitializeServer(32, Port, !Network.HavePublicAddress());
			}
		}
		else
		{
			if(Network.peerType == NetworkPeerType.Client)
			{
				GUI.Label(new Rect(100,100,100,25), "Client");
				if(GUI.Button(new Rect(100, 125, 100, 25), "Logout"))
				{
					Network.Disconnect(250);
				}
			}
			if(Network.peerType == NetworkPeerType.Server)
			{
				GUI.Label(new Rect(100,100,100,25), "Server: " + Network.HavePublicAddress());
				GUI.Label(new Rect(100,125,100,25), "Connections " + Network.connections.Length);
				if(GUI.Button(new Rect(100, 150, 100, 25), "Logout"))
				{
					Network.Disconnect(250);
				}
			}
		}
	}
}
