using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

class LightFlickerComponent : MonoBehaviour {

    public float flickerSpeed;
    public AnimationCurve brightness;

    private float maxIntensity;
    private Light flickerLight;

    void Start(){
        flickerLight = GetComponent<Light>();
        maxIntensity = flickerLight.intensity;
    }

    void Update(){
        flickerLight.intensity = brightness.Evaluate((Time.time * flickerSpeed) % 1.0f) * maxIntensity;
    }
}