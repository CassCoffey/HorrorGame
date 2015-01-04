using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class Vitals : MonoBehaviour {

    public int health;
    public float stamina;
    public int scent;
    public GameObject vitalsPanel;

    public void TakeDamage(int damage)
    {
        Debug.Log("Taking Damage");
        health -= damage;
        if (networkView.isMine)
        {
            vitalsPanel.transform.FindChild("HealthSlider").GetComponent<Slider>().value = health;
        }
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
