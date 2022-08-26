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

    private GameObject exteriorInstance;
    private GameObject interiorInstance;

    private int graphicsSettings = 0;
    private Camera playerCamera;
    private PixellateAndPalette pixellateEffect;
    private Antialiasing antialiasingEffect;

    void Start(){
        locationFound = false;
        gateOpened = false;

        respawnTransform = playerStartTransform; // until overridden by bonfire

        // Emulate probable SNES framerate and graphics options by default
        // QualitySettings.vSyncCount = 0;
        // Application.targetFrameRate = 15;

        playerCamera = player.GetComponentInChildren<Camera>();
        pixellateEffect = playerCamera.GetComponent<PixellateAndPalette>();
        antialiasingEffect = playerCamera.GetComponent<Antialiasing>();

        // Setup first level
        exteriorInstance = GameObject.Instantiate(exteriorPrefab);
        interiorInstance = GameObject.Instantiate(interiorPrefab);

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

    public void ResetLevels(){
        if(exteriorInstance != null){
            Destroy(exteriorInstance);
        }

        if(interiorInstance != null){
            Destroy(interiorInstance);
        }

        exteriorInstance = GameObject.Instantiate(exteriorPrefab);
        interiorInstance = GameObject.Instantiate(interiorPrefab);
    }
}