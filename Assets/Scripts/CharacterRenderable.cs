using System;
using UnityEngine;

class CharacterRenderable : MonoBehaviour {

    public const float CAMERA_ANGLE_COUNT = 8.0f;

    public MeshRenderer characterMesh;
    public float characterSize;

    [Space(10)]
    public bool alwaysRender;
    public float renderDistance;

    [Space(10)]
    public GameObject characterPrefab;

    private CharacterRenderingManager manager;
    private Camera cachedMainCamera;

    private int renderableSlot = -1;
    private RenderTexture characterTexture;
    private GameObject characterInstance;
    private AnimationComponent animationComponent;

    void Start(){
        cachedMainCamera = Camera.main;

        if(alwaysRender){
            RequestSlot();
        }
    }

    public void RequestSlot(){
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
        }
    }

    public void GiveUpSlot(){
        if(renderableSlot != -1){
            manager.FreeCharacterSlot(renderableSlot);
            renderableSlot = -1;

            characterMesh.material.mainTexture = null;
            characterMesh.enabled = false;
        }
    }

    public void Update(){
        float distanceToCamera = (cachedMainCamera.transform.position - transform.position).magnitude;

        if(renderableSlot != -1){
            if(distanceToCamera >= renderDistance && !alwaysRender){
                GiveUpSlot();
                return;
            }

            Vector3 renderableFlat = transform.forward;
            renderableFlat.y = 0.0f;

            Vector3 cameraFlat = cachedMainCamera.transform.forward;
            cameraFlat.y = 0.0f;

            float angle = Vector3.Angle(renderableFlat, cameraFlat);
            angle *= (Vector3.Dot(transform.right, cameraFlat) < 0.0f) ? -1.0f : 1.0f;

            if(angle < 0.0f){
                angle = 360.0f + angle;
            }

            // Snap to angle count
            float degreesPerCount = 360.0f / CAMERA_ANGLE_COUNT;
            float halfDegrees = degreesPerCount / 2.0f;

            angle -= halfDegrees;
            angle = Mathf.Ceil(angle / degreesPerCount) * degreesPerCount;

            // Debug.Log(angle + " " + degreesPerCount);

            manager.UpdateCameraForRenderable(renderableSlot, angle);
        } else {
            if(distanceToCamera <= renderDistance){
                RequestSlot();
                return;
            }
        }
    }

    public void PlayAnimation(AnimationComponent.Sequence seq, bool idleOnFinish = false){
        if(renderableSlot != -1){
            animationComponent.PlayAnimation(seq, idleOnFinish);
        }
    }
}