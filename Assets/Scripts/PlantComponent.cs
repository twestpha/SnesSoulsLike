using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantComponent : MonoBehaviour {
    
    public bool randomFlipX;
    public bool randomScale;

    void Start(){
        if(randomScale){
            Vector3 defaultScale = transform.localScale;
            Vector3 originalPosition = transform.localPosition;
            
            float randomScale = Random.Range(0.8f, 1.2f);

            transform.localScale = randomScale * defaultScale;
            transform.localPosition = new Vector3(
                originalPosition.x,
                originalPosition.y - ((defaultScale.y - transform.localScale.y) / 2.0f),
                originalPosition.z
            );
        }
        
        if(randomFlipX){
            bool flip = Random.value < 0.5f;

            transform.localScale = new Vector3(
                transform.localScale.x * (flip ? -1.0f : 1.0f),
                transform.localScale.y,
                transform.localScale.z
            );
        }
    }
}
