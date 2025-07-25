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
    private static Material currentBackground;
    
    private static Color targetColor;
    private static Vector2 targetThickness;
    private static Material targetBackground;

    public bool instant;
    public Color color;
    public FogThickness thickness;
    public Material background;
    
    void Update(){
        if(updater == null){
            updater = this;
        }
        
        if(updater == this){
            float t = fadeTimer.Parameterized();
            
            RenderSettings.fogColor = Color.Lerp(currentColor, targetColor, t);
            
            RenderSettings.fogStartDistance = Mathf.Lerp(currentThickness.x, targetThickness.x, t);
            RenderSettings.fogEndDistance = Mathf.Lerp(currentThickness.y, targetThickness.y, t);
        }
    }

    void OnTriggerEnter(Collider other){
        PlayerComponent player = other.gameObject.GetComponent<PlayerComponent>();

        if(player != null){
            currentColor = RenderSettings.fogColor;
            currentThickness = new Vector2(RenderSettings.fogStartDistance, RenderSettings.fogEndDistance);
            // currentBackground
            
            targetColor = color;
            targetThickness = GetThicknessRange(thickness);
            targetBackground = background;
            
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