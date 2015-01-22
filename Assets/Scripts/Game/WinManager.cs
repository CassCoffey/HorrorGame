using UnityEngine;
using System.Collections;

public class WinManager : MonoBehaviour 
{
	public int playersInZone;
	void Start()
	{
		playersInZone = 0;
	}
	void OnTriggerEnter(Collider person)
	{
		if (person.gameObject.tag == "Player") 
		{
			playersInZone += 1;
			if(GameObject.Find("SpawnManager").GetComponent<SpawnManager>().playersAlive == playersInZone)
			{
				Debug.Log ("Player's Win!");
			}
		}
	}
	void OnTriggerExit(Collider person)
	{
		if (person.gameObject.tag == "Player") 
		{
			playersInZone -= 1;
		}
	}
}
