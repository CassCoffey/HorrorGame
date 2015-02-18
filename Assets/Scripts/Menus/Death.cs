using UnityEngine;
using System.Collections;

public class Death
{
	public float DeathTime { get; private set; }
	public NetworkViewID Player { get; private set; }
	public string PlayerName { get; private set; }
	public string Killer { get; private set; }
	public int Damage { get; private set; }
	public Vector3 Location { get; private set; }
	
	public Death(float time, NetworkViewID player, string Name, string killer, int damage, Vector3 location)
	{
		DeathTime = time;
		Player = player;
		PlayerName = Name;
		Killer = killer;
		Damage = damage;
		Location = location;
	}
}