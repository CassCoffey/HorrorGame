using UnityEngine;
using System.Collections;

public class TrailSyncing : MonoBehaviour {

	public int trailTime;
	public GameObject player;

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
	public void SyncTrails(int health)
	{
		trailTime = 15 + (20 / ((health/10) + 1));
	}
}
