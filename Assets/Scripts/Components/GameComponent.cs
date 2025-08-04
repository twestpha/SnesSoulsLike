using System;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public enum DungeonName : int {
    OpenWorld = -1,
    StartingCastle,
    Dungeon1,
    Dungeon2,
    Dungeon3,
    Dungeon4,
    Dungeon5,
    Dungeon6,
}

[Serializable]
class Dungeon {
    public GameObject dungeonParent;
    public Transform[] entrances;
    public Transform[] exits;
}

class GameComponent : MonoBehaviour {
    
    public static GameComponent instance;

    [Header("Game Connections")]
    public GameObject player;

    [Space(10)]
    public GameObject openWorldParent;
    
    public Dungeon[] dungeons;
    private DungeonName currentDungeon;

    private int graphicsSettings = 0;
    private Camera playerCamera;
    private PixellateAndPalette pixellateEffect;
    private Antialiasing antialiasingEffect;
    
    void Awake(){
        instance = this;
    }

    void Start(){
        // Emulate probable SNES framerate and graphics options by default
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 15; // for now?

        playerCamera = player.GetComponentInChildren<Camera>();
        pixellateEffect = playerCamera.GetComponent<PixellateAndPalette>();
        antialiasingEffect = playerCamera.GetComponent<Antialiasing>();
        
        // Editor fixup because I'm dippy and I leave dungeons open a lot
        #if UNITY_EDITOR
            for(int i = 0, count = dungeons.Length; i < count; ++i){
                dungeons[i].dungeonParent.SetActive(false);
            }
            
            openWorldParent.SetActive(true);
        #endif
        
        currentDungeon = DungeonName.OpenWorld;
    }

    void Update(){
        if(Input.GetKeyDown(KeyCode.Equals)){
            graphicsSettings = Mathf.Min(graphicsSettings + 1, 3);
            UpdateGraphicsSettings();
        } else if(Input.GetKeyDown(KeyCode.Minus)){
            graphicsSettings = Mathf.Max(graphicsSettings - 1, 0);
            UpdateGraphicsSettings();
        }
    }

    void UpdateGraphicsSettings(){
        if(graphicsSettings == 0){
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 15;

            antialiasingEffect.enabled = false;
            pixellateEffect.enabled = true;

            RenderSettings.fogMode = FogMode.Linear; // Actually stepped

        } else if(graphicsSettings == 1){
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 30;

            antialiasingEffect.enabled = false;
            pixellateEffect.enabled = true;

            RenderSettings.fogMode = FogMode.Linear; // Actually stepped

        } else if(graphicsSettings == 2){
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = 60;

            antialiasingEffect.enabled = false;
            pixellateEffect.enabled = true;

            RenderSettings.fogMode = FogMode.Linear; // Actually stepped

        } else if(graphicsSettings == 3){
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = 60;

            antialiasingEffect.enabled = true;
            pixellateEffect.enabled = false;

            RenderSettings.fogMode = FogMode.Exponential; // Actually linear
        }
    }
    
    public Transform SetCurrentDungeon(DungeonName newDungeon, int dungeonIndex){
        if(currentDungeon != newDungeon){
            // Disable previous level
            if(currentDungeon == DungeonName.OpenWorld){
                openWorldParent.SetActive(false);
            } else {
                dungeons[(int) currentDungeon].dungeonParent.SetActive(false);
            }
            
            // Enable new level
            if(newDungeon == DungeonName.OpenWorld){
                openWorldParent.SetActive(true);
            } else {
                dungeons[(int) newDungeon].dungeonParent.SetActive(true);
            }

            // Now we're in the dungeon!
            DungeonName previousDungeon = currentDungeon;
            currentDungeon = newDungeon;
            
            if(currentDungeon == DungeonName.OpenWorld){
                return dungeons[(int) previousDungeon].exits[dungeonIndex];
            } else {
                return dungeons[(int) currentDungeon].entrances[dungeonIndex];
            }
        }
        
        return null;
    }
    
    public static string GetDungeonName(DungeonName name){
        return "dungeon_" + (name.ToString().ToLower());
    }
}