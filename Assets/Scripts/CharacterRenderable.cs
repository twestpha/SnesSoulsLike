using System;
using UnityEngine;

class CharacterRenderable : MonoBehaviour {

    public MeshRenderer characterMesh;
    public float characterSize;
    public GameObject characterPrefab;

    private CharacterRenderingManager manager;

    private int renderableSlot = -1;
    private RenderTexture characterTexture;
    private GameObject characterInstance;
    private AnimationComponent animationComponent;

    void Start(){
        Setup();
    }

    public void Setup(){
        manager = CharacterRenderingManager.instance;

        if(manager.RequestCharacterSlot(this, out renderableSlot)){
            characterTexture = manager.GetRenderTextureAtSlot(renderableSlot);
            characterInstance = manager.GetCharacterInstanceAtSlot(renderableSlot);

            animationComponent = characterInstance.GetComponentInChildren<AnimationComponent>();

            characterMesh.material.mainTexture = characterTexture;
            characterMesh.enabled = true;

            // characterSize ???
        } else {
            Debug.LogError("Not enough character slots!");
            enabled = false;
        }
    }

    public void Update(){
        if(renderableSlot != -1){
            float angle = 1.0f;
            // manager.UpdateCameraForRenderable(renderableSlot, angle);
        }
    }

    public void PlayAnimation(AnimationComponent.Sequence seq, bool idleOnFinish = false){
        if(renderableSlot != -1){
            animationComponent.PlayAnimation(seq, idleOnFinish);
        }
    }

    void OnDisable(){
        if(renderableSlot != -1){
            manager.FreeCharacterSlot(renderableSlot);
            renderableSlot = -1;

            characterMesh.material.mainTexture = null;
            characterMesh.enabled = false;
        }
    }
}