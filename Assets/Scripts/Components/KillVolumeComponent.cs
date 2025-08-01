using System;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

class KillVolumeComponent : MonoBehaviour {
    void OnTriggerEnter(Collider other){
        PlayerComponent player = other.gameObject.GetComponent<PlayerComponent>();

        if(player != null){
            EffectData effect = new EffectData();
            effect.effectType = EffectType.ChangeCurrentHp;
            effect.valueRange = new Vector2(-9999999.0f, -9999999.0f);
            
            player.ApplyEffects(new EffectData[]{ effect });
        }
    }
}