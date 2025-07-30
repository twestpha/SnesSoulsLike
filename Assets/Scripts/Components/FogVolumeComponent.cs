using System;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public enum FogThickness {
    Dense,
    Medium,
    Light
}

class FogVolumeComponent : MonoBehaviour {
    
    private const float FADE_TIME = 1.0f;
    
    private static FogVolumeComponent updater;
    
    private static Timer fadeTimer = new Timer(FADE_TIME);
    
    private static Color currentColor;
    private static Vector2 currentThickness;
    
    private static Color targetColor;
    private static Vector2 targetThickness;

    public bool instant;
    public Color color;
    public FogThickness thickness;
    
    private InventoryComponent playerInventory;
    
    void Start(){
        playerInventory = PlayerComponent.player.GetComponent<InventoryComponent>();
    }
    
    void Update(){
        if(updater == null){
            updater = this;
        }
        
        if(updater == this){
            float t = fadeTimer.Parameterized();
            
            RenderSettings.fogColor = Color.Lerp(currentColor, targetColor, t);
            
            // If the player has a torch, the fog can be only as thick as medium
            Vector2 maxThickness = targetThickness;
            if(playerInventory.leftHandEquippedItem != null && playerInventory.leftHandEquippedItem.itemType == ItemType.LightSource){
                Vector2 mediumThickness = GetThicknessRange(FogThickness.Medium);
                maxThickness.x = Mathf.Max(maxThickness.x, mediumThickness.x);
                maxThickness.y = Mathf.Max(maxThickness.y, mediumThickness.y);
            }
            
            RenderSettings.fogStartDistance = Mathf.Lerp(currentThickness.x, maxThickness.x, t);
            RenderSettings.fogEndDistance = Mathf.Lerp(currentThickness.y, maxThickness.y, t);
        }
    }

    void OnTriggerEnter(Collider other){
        PlayerComponent player = other.gameObject.GetComponent<PlayerComponent>();

        if(player != null){
            currentColor = RenderSettings.fogColor;
            currentThickness = new Vector2(RenderSettings.fogStartDistance, RenderSettings.fogEndDistance);
            
            targetColor = color;
            targetThickness = GetThicknessRange(thickness);
            
            if(instant){
                fadeTimer.SetParameterized(1.0f);
            } else {
                fadeTimer.Start();
            }
        }
    }
    
    public Vector2 GetThicknessRange(FogThickness thickness){
        if(thickness == FogThickness.Dense){
            return new Vector2(1.0f, 3.0f);
        } else if(thickness == FogThickness.Medium){
            return new Vector2(1.5f, 6.0f);
        } else {
            return new Vector2(5.0f, 10.0f);
        }
    }
}