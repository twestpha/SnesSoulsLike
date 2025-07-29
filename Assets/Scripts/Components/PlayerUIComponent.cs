using System;
using UnityEngine;
using UnityEngine.UI;

class PlayerUIComponent : MonoBehaviour {

    public Image healthBar;
    public Image staminaBar;
    public Image magicBar;
    
    [Space(10)]
    public Image leftItem;
    public Image rightItem;

    public PlayerComponent playerComponent;
    private InventoryComponent playerInventory;
    private ItemData cachedLeftHandItem = null;
    private ItemData cachedRightHandItem = null;
        
    private float healthBarWidth;
    private float staminaBarWidth;
    private float magicBarWidth;

    void Start(){
        playerInventory = playerComponent.GetComponent<InventoryComponent>();
        
        healthBarWidth = healthBar.GetComponent<RectTransform>().rect.width;
        staminaBarWidth = staminaBar.GetComponent<RectTransform>().rect.width;
        magicBarWidth = magicBar.GetComponent<RectTransform>().rect.width;
    }

    void Update(){
        float healthPercent = playerComponent.currentHealth / playerComponent.maxHealth;
        healthBar.fillAmount = Mathf.Round(healthPercent * healthBarWidth) / healthBarWidth;

        float staminaPercent = playerComponent.currentStamina / playerComponent.maxStamina;
        staminaBar.fillAmount = Mathf.Round(staminaPercent * staminaBarWidth) / staminaBarWidth;
        
        float magicPercent = playerComponent.currentMagic / playerComponent.maxMagic;
        magicBar.fillAmount = Mathf.Round(magicPercent * magicBarWidth) / magicBarWidth;
        
        // Update icons
        if(playerInventory.leftHandEquippedItem != cachedLeftHandItem){
            cachedLeftHandItem = playerInventory.leftHandEquippedItem;
            
            if(cachedLeftHandItem != null && cachedLeftHandItem.hudSprite != null){
                leftItem.enabled = true;
                leftItem.sprite = cachedLeftHandItem.hudSprite;
            } else {
                leftItem.enabled = false;
            }
        }
        
        if(playerInventory.rightHandEquippedItem != cachedRightHandItem){
            cachedRightHandItem = playerInventory.rightHandEquippedItem;
            
            if(cachedRightHandItem != null && cachedRightHandItem.hudSprite != null){
                rightItem.enabled = true;
                rightItem.sprite = cachedRightHandItem.hudSprite;
            } else {
                rightItem.enabled = false;
            }
        }
    }
}