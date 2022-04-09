using System;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

class GameComponent : MonoBehaviour {

    public static bool locationFound;
    public static bool gateOpened;

    [Header("Game Connections")]
    public GameObject player;
    public Transform playerStartTransform;

    [Space(10)]
    public GameObject exteriorPrefab;

    [Space(10)]
    public GameObject interiorPrefab;

    [Space(10)]
    public Transform respawnTransform;

    private GameObject levelPrefab;

    private int graphicsSettings = 0;
    private Camera playerCamera;
    private PixellateAndPalette pixellateEffect;
    private Antialiasing antialiasingEffect;

    [Header("Debug")]
    public bool debug;

    void Start(){
        locationFound = false;
        gateOpened = false;

        // Emulate probable SNES framerate and graphics options by default
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 15;

        playerCamera = player.GetComponentInChildren<Camera>();
        pixellateEffect = playerCamera.GetComponent<PixellateAndPalette>();
        antialiasingEffect = playerCamera.GetComponent<Antialiasing>();

        // Cut off if we're just debug playing
        if(debug){
            enabled = false;
            return;
        }

        // Setup first level
        levelPrefab = GameObject.Instantiate(exteriorPrefab);
        levelPrefab.transform.position = Vector3.zero;

        player.transform.position = playerStartTransform.position;
        player.transform.rotation = playerStartTransform.rotation;
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
}