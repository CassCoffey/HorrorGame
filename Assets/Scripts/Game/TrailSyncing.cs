using UnityEngine;
using System.Collections;

public class TrailSyncing : MonoBehaviour {

	public int trailTime;
	public GameObject player;

	//Syncs up the trailtime between players and makes the trailrenderer time = the trailtime
	private void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		if (stream.isWriting) 
		{
			stream.Serialize(ref trailTime);
		}
		else
		{
			stream.Serialize(ref trailTime);
			player.transform.FindChild("TrailRenderer").GetComponent<TrailRenderer>().time = trailTime;
		}
	}

	//The equation for the trailtime based on health
	public void SyncTrails(int health)
	{
		trailTime = 15 + (30 / ((health/10) + 1));
	}
}
