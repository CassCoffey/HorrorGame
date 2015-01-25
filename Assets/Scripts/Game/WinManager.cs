using UnityEngine;
using System.Collections;

public class WinManager : MonoBehaviour 
{
	public int playersInZone;
	//When the winzone spawns, set the amount of players in the winzone to 0
	//Every two seconds, check if the amount of players alive is the same as amount of players in the win zone
	void Start()
	{
		playersInZone = 0;
		InvokeRepeating("CheckForWin", 0, 2);
	}
	//Checks to see if everyone alive is in the winzone
	void CheckForWin()
	{
		if(GameObject.Find("SpawnManager").GetComponent<SpawnManager>().playersAlive == playersInZone)
		{
			Debug.Log ("Player's Win!");
		}
	}
	//When a player enters, add one to the players in zone
	void OnTriggerEnter(Collider person)
	{
		if (person.gameObject.tag == "Player") 
		{
			playersInZone += 1;
		}
	}
	//When a player leaves, subtract one from the players in zone
	void OnTriggerExit(Collider person)
	{
		if (person.gameObject.tag == "Player") 
		{
			playersInZone -= 1;
		}
	}
}
