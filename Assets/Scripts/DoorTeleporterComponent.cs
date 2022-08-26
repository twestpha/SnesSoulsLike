using System;
using UnityEngine;
using System.Collections;

class DoorTeleporterComponent : MonoBehaviour {

    public Transform teleportTarget;

    void OnTriggerEnter(Collider other){
        PlayerComponent player = other.gameObject.GetComponent<PlayerComponent>();

        if(player != null){
            StartCoroutine(Teleport(player));
        }
    }

    public IEnumerator Teleport(PlayerComponent player){
        // Pause
        // player.SetPaused(true);

        // Fade out
        Timer fadeTimer = new Timer(1.4f);
        fadeTimer.Start();
        while(!fadeTimer.Finished()){
            float t = 1.0f - fadeTimer.Parameterized();
            t = Mathf.Round(t * 10.0f) / 10.0f;

            RenderSettings.ambientLight = new Color(t, t, t, 1.0f);
            yield return null;
        }

        RenderSettings.ambientLight = new Color(0.0f, 0.0f, 0.0f, 1.0f);

        // Do the actual teleport
        player.transform.position = teleportTarget.position;
        player.transform.rotation = teleportTarget.rotation;

        // Unpause
        // player.SetPaused(false);

        // Fade in
        fadeTimer.Start();
        while(!fadeTimer.Finished()){
            float t = fadeTimer.Parameterized();
            t = Mathf.Round(t * 10.0f) / 10.0f;

            RenderSettings.ambientLight = new Color(t, t, t, 1.0f);
            yield return null;
        }

        RenderSettings.ambientLight = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    }
}