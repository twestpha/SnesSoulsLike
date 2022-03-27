using System;
using UnityEngine;
using UnityEngine.UI;

class PlayerUIComponent : MonoBehaviour {

    public Image healthBar;
    public Image staminaBar;

    public PlayerComponent playerComponent;

    private float healthBarWidth;
    private float staminaBarWidth;

    void Start(){
        healthBarWidth = healthBar.GetComponent<RectTransform>().rect.width;
        staminaBarWidth = staminaBar.GetComponent<RectTransform>().rect.width;
    }

    void Update(){
        float healthPercent = playerComponent.currentHealth / playerComponent.maxHealth;
        healthBar.fillAmount = Mathf.Round(healthPercent * healthBarWidth) / healthBarWidth;

        float staminaPercent = playerComponent.currentStamina / playerComponent.maxStamina;
        staminaBar.fillAmount = Mathf.Round(staminaPercent * staminaBarWidth) / staminaBarWidth;
    }
}