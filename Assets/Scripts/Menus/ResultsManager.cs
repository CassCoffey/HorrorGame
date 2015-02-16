using UnityEngine;
using System.Collections;


public class ResultsManager : MonoBehaviour {

	public GameObject lobbyButton;

	public void GoToLobby()
	{
		Application.LoadLevel ("MainMenu");
		if (Network.isServer) 
		{
			networkView.RPC("EnableLobby",RPCMode.All);
		}
	}

	[RPC] public void EnableLobby()
	{
		lobbyButton.SetActive (true);
	}
}
