using System;
using UnityEngine;

enum CharacterRenderableSize {
    Normal,
    Large,
}

class CharacterRenderable : MonoBehaviour {

    public const float CAMERA_ANGLE_COUNT = 8.0f;

    public MeshRenderer characterMesh;
    public CharacterRenderableSize characterSize;

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
    private Animator animator;
    
    public delegate void OnRenderingStartDelegate(CharacterRenderable renderable);
    public delegate void OnRenderingEndDelegate(CharacterRenderable renderable);
    
    [NonSerialized] public DelegateList<OnRenderingStartDelegate> onRenderingStart = new();
    [NonSerialized] public DelegateList<OnRenderingEndDelegate> onRenderingEnd = new();

    void Start(){
        cachedMainCamera = Camera.main;

        if(alwaysRender){
            RequestSlot();
        }
    }

    public void RequestSlot(){
        manager = CharacterRenderingManager.instance;

        if(manager.RequestCharacterSlot(this, GetResolutionForCharacterSize(characterSize), out renderableSlot)){
            characterTexture = manager.GetRenderTextureAtSlot(renderableSlot);
            characterInstance = manager.GetCharacterInstanceAtSlot(renderableSlot);

            animator = characterInstance.GetComponentInChildren<Animator>();

            characterMesh.material.mainTexture = characterTexture;
            
            // Wait a frame to enable the characterMesh, let the character get rendered
            
            UpdateCameraForRenderable();

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
                onRenderingEnd.Invoke(this);
                return;
            }
            
            characterMesh.enabled = true;
            UpdateCameraForRenderable();
        } else {
            if(distanceToCamera <= renderDistance){
                RequestSlot();
                onRenderingStart.Invoke(this);
                return;
            }
        }
    }
    
    public void UpdateCameraForRenderable(){
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
    }
    
    public static int GetResolutionForCharacterSize(CharacterRenderableSize size){
        if(size == CharacterRenderableSize.Normal){
            return 128;
        } else if(size == CharacterRenderableSize.Large){
            return 256;
        }
        
        return 128;
    }

    public void PlayAnimation(AnimationState state){
        if(renderableSlot != -1){
            animator.SetBool("walking", state == AnimationState.Walk);
            
            if(state != AnimationState.Idle && state != AnimationState.Walk){
                animator.SetTrigger(state.ToString().ToLower());
            }
        }
    }
    
    public GameObject FindNamedObjectInCharacter(string name){
        if(renderableSlot != -1){
            return FindInChildren(characterInstance, name);
        }
        
        return null;
    }
    
    public static GameObject FindInChildren(GameObject parent, string name){
        if(parent.name == name){
            return parent;
        }
        
        for(int i = 0, count = parent.transform.childCount; i < count; ++i){
            GameObject result = FindInChildren(parent.transform.GetChild(i).gameObject, name);
            
            if(result != null){
                return result;
            }
        }
        
        return null;
    }
}