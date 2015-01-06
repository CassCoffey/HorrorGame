using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class Vitals : MonoBehaviour {

    public float maxStamina;
    public float jumpStamina;
    public float staminaRegen;
    public GameObject vitalsPanel;
    public int maxHealth;

    private int health;
    private float stamina;
    private int scent;
    private float currentRegen = 0;
    private bool regen = false;

    /// <summary>
    /// On start initialize all sliders.
    /// </summary>
    public void Start()
    {
        if (networkView.isMine)
        {
            stamina = maxStamina;
            health = maxHealth;
            vitalsPanel.transform.FindChild("StaminaSlider").GetComponent<Slider>().maxValue = maxStamina;
            vitalsPanel.transform.FindChild("StaminaSlider").GetComponent<Slider>().value = maxStamina;
            vitalsPanel.transform.FindChild("HealthSlider").GetComponent<Slider>().maxValue = maxHealth;
            vitalsPanel.transform.FindChild("HealthSlider").GetComponent<Slider>().value = maxHealth;
        }
    }

    /// <summary>
    /// Regens stamina every update.
    /// </summary>
    public void Update()
    {
        if (regen)
        {
            currentRegen += Time.deltaTime;
            if (currentRegen >= staminaRegen)
            {
                regen = false;
                currentRegen = 0;
            }
        }
        if (!regen && stamina < maxStamina)
        {
            stamina += Time.deltaTime / 2f;
        }
        if (stamina > maxStamina)
        {
            stamina = maxStamina;
        }
        if (networkView.isMine)
        {
            vitalsPanel.transform.FindChild("StaminaSlider").GetComponent<Slider>().value = stamina;
        }
    }

    /// <summary>
    /// Checks if the player can run.
    /// </summary>
    public bool CanRun()
    {
        return !regen && stamina > 0;
    }

    /// <summary>
    /// Checks if the player can jump.
    /// </summary>
    public bool CanJump()
    {
        return stamina >= jumpStamina;
    }

    /// <summary>
    /// Handles taking damage.
    /// </summary>
    /// <param name="damage">The amount of health to lose.</param>
    public void TakeDamage(int damage)
    {
        Debug.Log("Taking Damage");
        health -= damage;
        if (health <= 0)
        {
            health = 0;
            Debug.Log("You died.");
        }
        if (networkView.isMine)
        {
            vitalsPanel.transform.FindChild("HealthSlider").GetComponent<Slider>().value = health;
        }
    }

    /// <summary>
    /// Handles using stamina.
    /// </summary>
    /// <param name="staminaUse">The amount of stamina to use.</param>
    public void UseStamina(float staminaUse)
    {
        stamina -= staminaUse;
        if (stamina <= 0)
        {
            stamina = 0;
            regen = true;
        }
        if (networkView.isMine)
        {
            vitalsPanel.transform.FindChild("StaminaSlider").GetComponent<Slider>().value = stamina;
        }
    }
}
