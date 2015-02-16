using UnityEngine;
using System.Collections;


public class WinManager : MonoBehaviour 
{
	public int playersInZone;
	public GameObject currCamera;
	public GameObject resultsCamera;
	public GameObject lobbyButton;

	/// <summary>
    /// When the winzone spawns, set the amount of players in the winzone to 0
    /// Every two seconds, check if the amount of players alive is the same as amount of players in the win zone
	/// </summary>
	void Start()
	{
		playersInZone = 0;
		InvokeRepeating("CheckForWin", 1, 2);
	}

	/// <summary>
    /// Checks to see if everyone alive is in the winzone
	/// </summary>
	void CheckForWin()
	{
		if(GameObject.Find("SpawnManager").GetComponent<SpawnManager>().playersAlive == playersInZone)
		{
			Debug.Log ("Player's Win!");
			resultsCamera.SetActive(true);
			currCamera.GetComponent<Camera>().enabled = false;
			resultsCamera.GetComponent<Camera>().enabled = true;
			Screen.lockCursor = false;
			Screen.showCursor = true;
			CancelInvoke();
			if(Network.isServer)
			{
				lobbyButton.SetActive(true);
			}
		}
	}

	/// <summary>
    /// When a player enters, add one to the players in zone
	/// </summary>
	void OnTriggerEnter(Collider person)
	{
		if (person.gameObject.tag == "Player") 
		{
			playersInZone += 1;
		}
	}

	/// <summary>
    /// When a player leaves, subtract one from the players in zone
	/// </summary>
	void OnTriggerExit(Collider person)
	{
		if (person.gameObject.tag == "Player") 
		{
			playersInZone -= 1;
		}
	}
}
