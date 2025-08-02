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
    
    private static Color currentColor;
    private static Color colorVelocity;
    
    private static Vector2 currentThickness;
    private static Vector2 thicknessVelocity;
    
    private static Color targetColor;
    private static Vector2 targetThickness;

    public bool instant;
    public Color color;
    public FogThickness thickness;
    public MeshRenderer skyboxRenderer;
    
    [Space(10)]
    public bool primary;
    
    private InventoryComponent playerInventory;
    private Camera mainCamera;
    
    void Start(){
        playerInventory = PlayerComponent.player.GetComponent<InventoryComponent>();
        mainCamera = Camera.main;
    }
    
    void Update(){
        if(primary){
            currentColor = ColorSmoothDamp(currentColor, targetColor, ref colorVelocity, FADE_TIME);
            
            RenderSettings.fogColor = currentColor;
            mainCamera.backgroundColor = currentColor;
            skyboxRenderer.material.SetColor("_FadeColor", currentColor);
            
            // If the player has a torch, the fog can be only as thick as medium
            Vector2 maxThickness = targetThickness;
            if(playerInventory.leftHandEquippedItem != null && playerInventory.leftHandEquippedItem.itemType == ItemType.LightSource){
                Vector2 mediumThickness = GetThicknessRange(FogThickness.Medium);
                maxThickness.x = Mathf.Max(maxThickness.x, mediumThickness.x);
                maxThickness.y = Mathf.Max(maxThickness.y, mediumThickness.y);
            }
            
            currentThickness = Vector2.SmoothDamp(currentThickness, maxThickness, ref thicknessVelocity, FADE_TIME);
            
            RenderSettings.fogStartDistance = currentThickness.x;
            RenderSettings.fogEndDistance = currentThickness.y;
        }
    }
    
    private static Color ColorSmoothDamp(Color current, Color target, ref Color velocity, float smoothTime){
        current.r = Mathf.SmoothDamp(current.r, target.r, ref velocity.r, smoothTime);
        current.g = Mathf.SmoothDamp(current.g, target.g, ref velocity.g, smoothTime);
        current.b = Mathf.SmoothDamp(current.b, target.b, ref velocity.b, smoothTime);
        current.a = Mathf.SmoothDamp(current.a, target.a, ref velocity.a, smoothTime);
        
        return current;
    }

    void OnTriggerEnter(Collider other){
        PlayerComponent player = other.gameObject.GetComponent<PlayerComponent>();

        if(player != null){
            targetColor = color;
            targetThickness = GetThicknessRange(thickness);
            
            if(instant){
                currentColor = color;
                currentThickness = GetThicknessRange(thickness);
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