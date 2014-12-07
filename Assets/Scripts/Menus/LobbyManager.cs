using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour {

	public GameObject labelPrefab;
	public GameObject startGameButton;
	public GameObject playerListPanel;
	public Font font;
	
	private List<GameObject> playerList = new List<GameObject>();

	// Update is called once per frame
	void Update () {
	    if (!Network.isServer)
        {
            startGameButton.GetComponent<UnityEngine.UI.Button>().enabled = false;
            startGameButton.GetComponent<UnityEngine.UI.Image>().enabled = false;
            startGameButton.GetComponentInChildren<UnityEngine.UI.Text>().enabled = false;
        }
        else
        {
            startGameButton.GetComponent<UnityEngine.UI.Button>().enabled = true;
            startGameButton.GetComponent<UnityEngine.UI.Image>().enabled = true;
            startGameButton.GetComponentInChildren<UnityEngine.UI.Text>().enabled = true;
        }
	}

	void OnPlayerConnected(NetworkPlayer player) {
		Debug.Log ("Player Connected!" + player.ipAddress);
		for (int i = 0; i < playerList.Count; i++)
		{
			Destroy(playerList[i]);
		}
		playerList.Clear();
		for (int i = 0; i < Network.connections.Length; i++)
		{
			GameObject label = (GameObject)Instantiate(labelPrefab);
			label.transform.SetParent(playerListPanel.transform, false);
			UnityEngine.UI.Button labelScript = label.GetComponent<UnityEngine.UI.Button>();
			label.GetComponentInChildren<Text>().font = font;
			label.GetComponentInChildren<Text>().text = (player.ipAddress);
			label.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1 - ((1.0f / (float)Network.connections.Length) * i));
			label.GetComponent<RectTransform>().anchorMin = new Vector2(0, (1 - (1.0f / (float)Network.connections.Length)) - ((1.0f / (float)Network.connections.Length) * i));
			label.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
			label.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
			playerList.Add(label);
		}
	}

	void OnPlayerDisconnected(NetworkPlayer player) {
		Debug.Log ("Player Disconnected!" + player.ipAddress);
		for (int i = 0; i < playerList.Count; i++) 
		{
			Destroy (playerList [i]);
		}
		playerList.Clear ();
		for (int i = 0; i < Network.connections.Length; i++) 
		{
			GameObject label = (GameObject)Instantiate (labelPrefab);
			label.transform.SetParent (playerListPanel.transform, false);
			UnityEngine.UI.Button labelScript = label.GetComponent<UnityEngine.UI.Button> ();
			label.GetComponentInChildren<Text> ().font = font;
			label.GetComponentInChildren<Text> ().text = (player.ipAddress);
			label.GetComponent<RectTransform> ().anchorMax = new Vector2 (1, 1 - ((1.0f / (float)Network.connections.Length) * i));
			label.GetComponent<RectTransform> ().anchorMin = new Vector2 (0, (1 - (1.0f / (float)Network.connections.Length)) - ((1.0f / (float)Network.connections.Length) * i));
			label.GetComponent<RectTransform> ().offsetMax = new Vector2 (0, 0);
			label.GetComponent<RectTransform> ().offsetMin = new Vector2 (0, 0);
			playerList.Add (label);
		}
	}
}
