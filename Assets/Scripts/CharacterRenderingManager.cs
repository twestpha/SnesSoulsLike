using System;
using UnityEngine;

class CharacterRenderingManager : MonoBehaviour {

    public const int MAX_CHARACTER_SLOTS = 8;
    public const float RENDER_TIME = 1.0f / 8.0f; // 8 fps

    public const int DEFAULT_RESOLUTION = 128; // pixels
    public const int ANTI_ALIASING = 8;

    public static CharacterRenderingManager instance;

    private struct CharacterSlot {
        public bool inUse;
        public int resolution;
        public Timer renderTimer;
        public RenderTexture renderTexture;
        public CharacterSlotComponent slotComponent;
        public GameObject characterPrefabInstance;
    }

    public GameObject characterSlotPrototype;
    [SerializeField]
    private CharacterSlot[] characterSlots;

    void Awake(){
        instance = this;

        characterSlots = new CharacterSlot[MAX_CHARACTER_SLOTS];

        for(int i = 0; i < MAX_CHARACTER_SLOTS; ++i){
            characterSlots[i].inUse = false;
            characterSlots[i].resolution = DEFAULT_RESOLUTION;
            characterSlots[i].renderTimer = new Timer(RENDER_TIME);

            characterSlots[i].renderTexture = RenderTexture.GetTemporary(
                DEFAULT_RESOLUTION,
                DEFAULT_RESOLUTION,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Default,
                ANTI_ALIASING
            );
            characterSlots[i].renderTexture.filterMode = FilterMode.Point;

            GameObject slotInstance = GameObject.Instantiate(characterSlotPrototype);
            slotInstance.transform.parent = transform;
            slotInstance.transform.localPosition = new Vector3(((float) i) * 30.0f, 0.0f, 0.0f);

            slotInstance.SetActive(true);

            characterSlots[i].slotComponent = slotInstance.GetComponentInChildren<CharacterSlotComponent>();
            characterSlots[i].slotComponent.slotCamera.targetTexture = characterSlots[i].renderTexture;
        }
    }
    
    public bool RequestCharacterSlot(CharacterRenderable characterRenderable, int resolution, out int slotIndex){
        for(int i = 0; i < MAX_CHARACTER_SLOTS; ++i){
            if(!characterSlots[i].inUse){
                characterSlots[i].inUse = true;
                
                if(characterSlots[i].resolution != resolution){
                    characterSlots[i].renderTexture.Release();
                    
                    characterSlots[i].renderTexture = RenderTexture.GetTemporary(
                        resolution,
                        resolution,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Default,
                        ANTI_ALIASING
                    );
                    
                    characterSlots[i].renderTexture.filterMode = FilterMode.Point;
                    characterSlots[i].slotComponent.slotCamera.targetTexture = characterSlots[i].renderTexture;
                }

                GameObject characterPrefabInstance = GameObject.Instantiate(characterRenderable.characterPrefab);
                characterPrefabInstance.transform.parent = characterSlots[i].slotComponent.characterOrigin.transform;
                characterPrefabInstance.transform.localPosition = Vector3.zero;
                characterPrefabInstance.transform.localRotation = Quaternion.identity;

                characterSlots[i].characterPrefabInstance = characterPrefabInstance;
                
                characterSlots[i].renderTimer.Start();

                characterSlots[i].slotComponent.slotCamera.enabled = false;
                characterSlots[i].slotComponent.slotCamera.Render();

                slotIndex = i;
                return true;
            }
        }

        slotIndex = -1;
        return false;
    }
    
    void Update(){
        for(int i = 0; i < MAX_CHARACTER_SLOTS; ++i){
            if(characterSlots[i].inUse){
                if(characterSlots[i].renderTimer.Finished()){
                    characterSlots[i].renderTimer.Start();
                    characterSlots[i].slotComponent.slotCamera.Render();
                }
            }
        }
    }

    public RenderTexture GetRenderTextureAtSlot(int slotIndex){
        Debug.Assert(characterSlots[slotIndex].inUse);

        return characterSlots[slotIndex].renderTexture;
    }

    public GameObject GetCharacterInstanceAtSlot(int slotIndex){
        Debug.Assert(characterSlots[slotIndex].inUse);

        return characterSlots[slotIndex].characterPrefabInstance;
    }

    public void FreeCharacterSlot(int slotIndex){
        Debug.Assert(characterSlots[slotIndex].inUse);

        Destroy(characterSlots[slotIndex].characterPrefabInstance);
        characterSlots[slotIndex].slotComponent.slotCamera.enabled = false;
        characterSlots[slotIndex].inUse = false;
    }

    public void UpdateCameraForRenderable(int slotIndex, float cameraAngle){
        Debug.Assert(characterSlots[slotIndex].inUse);

        characterSlots[slotIndex].slotComponent.cameraOrigin.transform.localRotation = Quaternion.Euler(0.0f, cameraAngle, 0.0f);
    }
}