using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

class FireComponent : MonoBehaviour {

    public float delayTime;

    public GameObject scaledObject;
    public Vector2 scale;

    public GameObject[] fireObjects;

    private bool lit;

    private void OnTriggerEnter(Collider other){
        if(!lit){
            lit = true;
            StartCoroutine(LightSequence());
        }
    }

    public IEnumerator LightSequence(){
        Timer delayTimer = new Timer(delayTime);
        delayTimer.Start();

        while(!delayTimer.Finished()){
            yield return null;
        }

        foreach(GameObject fireObject in fireObjects){
            fireObject.SetActive(true);
        }

        Timer scaleTimer = new Timer(0.5f);
        scaleTimer.Start();

        scaledObject.SetActive(true);

        while(!scaleTimer.Finished()){
            float t = Mathf.Lerp(scale.x, scale.y, scaleTimer.Parameterized());
            scaledObject.transform.localScale = new Vector3(t, t, t);

            yield return null;
        }

        scaledObject.transform.localScale = new Vector3(scale.y, scale.y, scale.y);
    }
}