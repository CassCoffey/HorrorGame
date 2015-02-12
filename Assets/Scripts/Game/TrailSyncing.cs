using UnityEngine;
using System.Collections;

public class TrailSyncing : MonoBehaviour {

	public int trailTime;
	public float trailGreen;
	public float trailRed;
	public GameObject player;

	/// <summary>
    /// Syncs up the trailtime between players and makes the trailrenderer time = the trailtime
    /// Syncs up color as well
	/// </summary>
	private void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		if (stream.isWriting) 
		{
			stream.Serialize(ref trailTime);
			stream.Serialize(ref trailGreen);
			stream.Serialize(ref trailRed);
		}
		else
		{
			stream.Serialize(ref trailTime);
			stream.Serialize(ref trailGreen);
			stream.Serialize(ref trailRed);
			player.transform.FindChild("TrailParticles").particleSystem.startLifetime = trailTime;
			player.transform.FindChild("TrailParticles").particleSystem.startColor = new Color(trailRed,trailGreen, 0, 1);
		}
	}

    /// <summary>
    /// The equation for the trailtime and color based on health
    /// </summary>
	public void SyncTrails(int health)
	{
		trailTime = 20 + (30 / ((health/10) + 1));
		trailRed = 1f/(((float)health / 10f) + 1f);
		trailGreen = ((float)health/100f);
	}
}
