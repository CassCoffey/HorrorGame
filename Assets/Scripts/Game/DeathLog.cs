using UnityEngine;
using System.Collections.Generic;

public class DeathLog : MonoBehaviour {

    public List<Death> deaths = new List<Death>();

    /// <summary>
    /// Called whenever a player dies.
    /// </summary>
    /// <param name="time">The time of death.</param>
    /// <param name="player">The dead player.</param>
    /// <param name="killer">The killer's name.</param>
    /// <param name="damage">The damage taken from the killing blow.</param>
    /// <param name="location">The location of death.</param>
    public void LogDeath(float time, NetworkViewID player, string Name, string killer, int damage, Vector3 location)
    {
        Death death = new Death(time, player, Name, killer, damage, location);
        deaths.Add(death);
    }
}