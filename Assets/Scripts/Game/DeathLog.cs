using UnityEngine;
using System.Collections.Generic;

public class DeathLog : MonoBehaviour {

    private List<Death> deaths = new List<Death>();

    /// <summary>
    /// Called whenever a player dies.
    /// </summary>
    /// <param name="time">The time of death.</param>
    /// <param name="player">The dead player.</param>
    /// <param name="killer">The killer's name.</param>
    /// <param name="damage">The damage taken from the killing blow.</param>
    /// <param name="location">The location of death.</param>
    public void LogDeath(float time, NetworkViewID player, string killer, int damage, Vector3 location)
    {
        Death death = new Death(time, player, killer, damage, location);
        deaths.Add(death);
    }


    /// <summary>
    /// A class for keeping track of death information.
    /// </summary>
    private class Death
    {
        public float DeathTime { get; private set; }
        public NetworkViewID Player { get; private set; }
        public string Killer { get; private set; }
        public int Damage { get; private set; }
        public Vector3 Location { get; private set; }

        public Death(float time, NetworkViewID player, string killer, int damage, Vector3 location)
        {
            DeathTime = time;
            Player = player;
            Killer = killer;
            Damage = damage;
            Location = location;
        }
    }
}