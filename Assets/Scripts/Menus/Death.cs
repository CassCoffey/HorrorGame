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

	/// <summary>
	/// Death Class for keeping track of any death that happens, whether it is a human or a monster that dies.
	/// </summary>
	/// <param name="time">The time of death.</param>
	/// <param name="player">The networkview person who died.</param>
	/// <param name="Name">The Name of the person who died.</param>
	/// <param name="killer">The killer.</param>
	/// <param name="damage">The amount of damage it did.</param>
	/// <param name="location">The location the player died.</param>
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