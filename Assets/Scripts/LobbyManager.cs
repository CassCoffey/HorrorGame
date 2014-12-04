using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class LobbyManager : MonoBehaviour {

    public GameObject startGameButton;
	
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
}
