using System;
using UnityEngine;
using UnityEngine.UI;

class PlayerUIComponent : MonoBehaviour {

    public Image[] selectionPips;
    public Image[] healthBars;

    public PlayerComponent playerComponent;

    void Start(){
        playerComponent = GetComponentInParent<PlayerComponent>();
    }

    void Update(){
        UnitComponent currentUnit = playerComponent.GetCurrentPlayerUnit();

        for(int i = 0, count = playerComponent.units.Length; i < count; ++i){
            selectionPips[i].enabled = playerComponent.units[i] == currentUnit;
            healthBars[i].fillAmount = playerComponent.units[i].GetComponent<HealthComponent>().GetCurrentHealthPercentage();
        }
    }
}