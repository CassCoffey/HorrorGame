using UnityEngine;
using System.Collections;

public class TrailSyncing : MonoBehaviour {

	public int trailTime;
	public float trailGreen;
	public float trailRed;
	public GameObject player;

	//Syncs up the trailtime between players and makes the trailrenderer time = the trailtime
	//Syncs up color as well
	private void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		if (stream.isWriting) 
		{
			stream.Serialize(ref trailTime);
			stream.Serialize(ref trailGreen);
			stream.Serialize(ref trailRed);
			Debug.Log ("Writing");
		}
		else
		{
			stream.Serialize(ref trailTime);
			stream.Serialize(ref trailGreen);
			stream.Serialize(ref trailRed);
			Debug.Log("Time" + trailTime);
			Debug.Log ("Red" + trailRed);
			Debug.Log ("Green" + trailGreen);
			player.transform.FindChild("TrailParticles").particleSystem.startLifetime = trailTime;
			player.transform.FindChild("TrailParticles").particleSystem.startColor = new Color(trailRed,trailGreen, 0, 1);
		}
	}

	//The equation for the trailtime and color based on health
	public void SyncTrails(int health)
	{
		trailTime = 20 + (30 / ((health/10) + 1));
		trailRed = 1f/(((float)health / 10f) + 1f);
		trailGreen = ((float)health/100f);
	}
}
