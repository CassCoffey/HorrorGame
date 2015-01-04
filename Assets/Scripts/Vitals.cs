using UnityEngine;
using System.Collections;

public class Vitals : MonoBehaviour {

    public int health;
    public float stamina;
    public int scent;

    public void TakeDamage(int damage)
    {
        Debug.Log("Taking Damage");
        health -= damage;
        if (health <= 0)
        {
            Debug.Log("You died.");
        }
    }
    public void UseStamina(float staminaUse)
    {
        stamina -= staminaUse;
    }
}
