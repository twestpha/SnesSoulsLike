using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

public enum AnimationState {
    Idle,
    Walk,
    Roll,
    LightAttack,
    HeavyAttack,
    Aim,
    Shoot,
    UseItem,
    Flinch,
    Stagger,
    Death,
}

class PlayerComponent : MonoBehaviour {
    public static PlayerComponent player;

    private const int IGNORE_CAMERA_LAYER_MASK = 1 << 3;
    
    private const int MAX_OPTIONS = 4;
    private const float INTERACT_DISTANCE = 0.4f;
    
    [Header("Movement")]
    public float moveSpeed = 1.0f;
    public float rotateSpeed = 1.0f;
    public float spriteRotateTime = 1.0f;

    public float rollTime;
    public float rollStaminaCost;

    [Header("Health and Stamina")]
    public float maxHealth;
    public float currentHealth;

    public float maxStamina;
    public float currentStamina;

    [Header("Hurt Box")]
    public BoxCollider hurtBox;

    [Space(10)]
    public float staggerThreshold = 0.5f;
    public float staggerTime;

    [Header("Camera")]
    public Vector2 cameraDistanceBounds;
    public Transform cameraRaycastOrigin;
    public MeshRenderer skyPlaneMeshRenderer;

    [Header("Sprites and Animation")]
    public Transform playerSpriteTransform;
    public CharacterRenderable characterRenderable;

    public AnimationCurve rollSpeedCurve;

    [Header("Text, Messages, and UI")]
    public Image messageBackground;
    public Text messageText;

    [Space(10)]
    public Image itemImage;
    public Sprite itemFull;
    public Sprite itemEmpty;

    [Space(10)]
    public Text locationText;
    public Text gameOverText;

    [Space(10)]
    public GameObject bossBarParent;
    public Image bossBar;
    
    [Space(10)]
    public GameObject optionParent;
    public RectTransform optionCursor;
    public Text[] optionTexts;

    private float spriteDirection;
    private float spriteDirectionVelocity;

    private Camera playerCamera;
    private CharacterController characterController;
    private InventoryComponent inventory;
    private AbilityComponent ability;

    private bool fightingBoss;
    private CreatureComponent bossCreature;

    private bool playerPaused;
    private bool showingMessage;
    
    private InteractComponent interact;
    private int interactCount;
    private int interactIndex;

    public enum PlayerState {
        None,
        Rolling,
        UsingAbility,
        Staggered,
        Interacting,
        Dead,
    }

    [Header("State")]
    public PlayerState playerState;

    private Vector3 cachedRollDirection;
    private Timer rollTimer;

    private float lookAngle;
    private float playerHeight;

    private bool movingAnimation;
    private bool attackDamageStarted;

    private Timer attackTimer = new Timer();
    private Timer attackDelayTimer = new Timer();

    private Timer itemUseTimer;
    private Timer itemDelayTimer;
    private Timer staggerTimer;

    private GameComponent gameComponent;

    void Start(){
        player = this;

        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        inventory = GetComponent<InventoryComponent>();
        ability = GetComponent<AbilityComponent>();
        
        gameComponent = GameObject.FindObjectOfType<GameComponent>(); // TODO make this an instance, oof

        playerHeight = transform.position.y;

        rollTimer = new Timer(rollTime);
        staggerTimer = new Timer(staggerTime);

        lookAngle = transform.rotation.eulerAngles.y;
    }

    void Update(){
        if(playerPaused){
            return;
        }

        // Movement
        Vector3 movementVector = Vector3.zero;

        bool up = Input.GetKey(KeyCode.UpArrow);
        bool left = Input.GetKey(KeyCode.LeftArrow);
        bool back = Input.GetKey(KeyCode.DownArrow);
        bool right = Input.GetKey(KeyCode.RightArrow);
        bool movementInput = (up || left || back || right);

        //            +----------+
        //            |   ????   |
        // +----------+----------+----------+
        // |   Left   |   Roll   |   Right  |
        // +----------+----------+----------+

        bool topAction = Input.GetKeyDown(KeyCode.W);
        bool leftAction = Input.GetKeyDown(KeyCode.A);
        bool rightAction = Input.GetKeyDown(KeyCode.D);
        bool bottomAction = Input.GetKeyDown(KeyCode.S);

        if(up){ movementVector += transform.forward; }
        if(left){ movementVector -= transform.right; }
        if(back){ movementVector -= transform.forward; }
        if(right){ movementVector += transform.right; }

        movementVector.y = 0.0f;

        if(playerState == PlayerState.None){
            if(movementVector.magnitude > 0.01f){
                cachedRollDirection = movementVector.normalized;
            }

            if(topAction){
                Debug.DrawLine(transform.position, transform.position + (playerSpriteTransform.forward * INTERACT_DISTANCE), Color.blue, 5.0f, false);
                
                RaycastHit[] interactHits = Physics.RaycastAll(transform.position, playerSpriteTransform.forward, INTERACT_DISTANCE);
                for(int i = 0, hitCount = interactHits.Length; i < hitCount; ++i){
                    InteractComponent hitInteract = interactHits[i].collider.gameObject.GetComponentInParent<InteractComponent>();
                    
                    if(hitInteract != null){
                        interactCount = hitInteract.GetInteractCount();
                        
                        if(interactCount > 0){
                            interact = hitInteract;
                            Interact();
                            return;
                        }
                    }
                }
            } else if(leftAction){
                if(inventory.leftHandEquippedItem != null && ability.CanCast(inventory.leftHandEquippedItem.ability)){
                    ability.Cast(inventory.leftHandEquippedItem.ability, inventory.leftHandEquippedItem);
                    playerState = PlayerState.UsingAbility;
                }
            } else if(rightAction){
                if(inventory.rightHandEquippedItem != null && ability.CanCast(inventory.rightHandEquippedItem.ability)){
                    ability.Cast(inventory.rightHandEquippedItem.ability, inventory.rightHandEquippedItem);
                    playerState = PlayerState.UsingAbility;
                }
            } else if(bottomAction){
                if(movementInput && currentStamina > rollStaminaCost){
                    Roll();
                }
            } else {
                // Only move if not doing another action
                characterController.SimpleMove(movementVector.normalized * moveSpeed);

                if(movementInput && !movingAnimation){
                    movingAnimation = true;
                    characterRenderable.PlayAnimation(AnimationState.Walk);
                } else if(!movementInput && movingAnimation){
                    movingAnimation = false;
                    characterRenderable.PlayAnimation(AnimationState.Idle);
                }
            }
        } else if(playerState == PlayerState.Rolling){
            float t = rollSpeedCurve.Evaluate(rollTimer.Parameterized());
            t = Mathf.Round(t * 6.0f) / 6.0f; // Make it feel chonky

            characterController.SimpleMove(cachedRollDirection.normalized * moveSpeed * t);
            movementVector = cachedRollDirection;

            if(rollTimer.Finished()){
                playerState = PlayerState.None;

                if(movementInput){
                    characterRenderable.PlayAnimation(AnimationState.Walk);
                } else {
                    characterRenderable.PlayAnimation(AnimationState.Idle);
                }
            }
        } else if(playerState == PlayerState.Staggered){
            if(staggerTimer.Finished()){
                playerState = PlayerState.None;

                if(movementInput){
                    characterRenderable.PlayAnimation(AnimationState.Walk);
                } else {
                    characterRenderable.PlayAnimation(AnimationState.Idle);
                }
            }
        } else if(playerState == PlayerState.UsingAbility){
            if(ability.CastingAnyAbility()){
                if(inventory.leftHandEquippedItem != null && !Input.GetKey(KeyCode.A)){
                    ability.NotifyOfInput(inventory.leftHandEquippedItem.ability);
                }
                if(inventory.rightHandEquippedItem != null && !Input.GetKey(KeyCode.D)){
                    ability.NotifyOfInput(inventory.rightHandEquippedItem.ability);
                }
            } else {
                if(movementInput){
                    characterRenderable.PlayAnimation(AnimationState.Walk);
                } else {
                    characterRenderable.PlayAnimation(AnimationState.Idle);
                }
                
                playerState = PlayerState.None;
            }
        }

        // Sprite direction
        if(movementInput && (playerState == PlayerState.None || playerState == PlayerState.Rolling)){
            spriteDirection = Mathf.SmoothDampAngle(
                spriteDirection,
                Mathf.Atan2(movementVector.x, movementVector.z) * Mathf.Rad2Deg - 5.0f, // Magic number makes rotations look better *shrug*
                ref spriteDirectionVelocity,
                spriteRotateTime
            );
        }

        playerSpriteTransform.rotation = Quaternion.Euler(
            playerSpriteTransform.rotation.x,
            spriteDirection,
            playerSpriteTransform.rotation.z
        );

        // Camera
        float rotateDirection = 0.0f;
        if(currentHealth > 0.0f){
            if(Input.GetKey(KeyCode.E)){ rotateDirection = 1.0f; }
            if(Input.GetKey(KeyCode.Q)){ rotateDirection = -1.0f; }
        }

        lookAngle = lookAngle + (rotateDirection * rotateSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Euler(
            transform.rotation.eulerAngles.x,
            Mathf.Round(lookAngle / 2.0f) * 2.0f, // Make it feel chonky
            transform.rotation.eulerAngles.z
        );

        RaycastHit hit;
        float distance = 1.0f;
        if(Physics.Raycast(cameraRaycastOrigin.position, cameraRaycastOrigin.forward, out hit, 10.0f, ~IGNORE_CAMERA_LAYER_MASK, QueryTriggerInteraction.Ignore)){
            distance = hit.distance - 0.05f;
        }

        playerCamera.transform.localPosition = new Vector3(
            playerCamera.transform.localPosition.x,
            playerCamera.transform.localPosition.y,
            -Mathf.Clamp(distance, cameraDistanceBounds.x, cameraDistanceBounds.y)
        );

        skyPlaneMeshRenderer.material.SetTextureOffset("_MainTex", new Vector2(transform.rotation.eulerAngles.y / 360.0f, 0.0f));

        // Boss bar
        if(fightingBoss){
            bossBar.fillAmount = bossCreature.currentHealth / bossCreature.maxHealth;

            if(bossCreature.currentHealth <= 0.0f || currentHealth <= 0.0f){
                fightingBoss = false;
                bossCreature = null;

                bossBar.fillAmount = 1.0f;
                bossBarParent.SetActive(false);
            }
        }
    }

    private void Roll(){
        rollTimer.Start();
        playerState = PlayerState.Rolling;
        
        currentStamina = Mathf.Max(currentStamina - rollStaminaCost, 0.0f);
        characterRenderable.PlayAnimation(AnimationState.Roll);
    }
    
    private void Interact(){
        playerState = PlayerState.Interacting;
        
        // show options
        interactIndex = 0;
        SetOptionCursorToIndex();
        
        for(int i = 0; i < MAX_OPTIONS; ++i){
            if(i < interactCount){
                optionTexts[i].text = interact.GetInteractString(i);
            } else {
                optionTexts[i].text = "";
            }
        }
        
        StartCoroutine(InteractCoroutine());
    }
    
    private void SetOptionCursorToIndex(){
        optionCursor.anchoredPosition = new Vector2(
            optionCursor.anchoredPosition.x,
            optionTexts[interactIndex].GetComponent<RectTransform>().anchoredPosition.y
        );
    }
    
    private IEnumerator InteractCoroutine(){
        optionParent.SetActive(true);
        
        while(true){
            if(Input.GetKeyDown(KeyCode.S)){
                interact.ChooseInteractOption(interactIndex);
                break;
            }
            
            if(Input.GetKeyDown(KeyCode.D)){
                interact.ChooseInteractOption(interactCount);
                break;
            }
            
            if(Input.GetKeyDown(KeyCode.UpArrow)){
                interactIndex = (interactIndex + interactCount - 1) % interactCount;
                SetOptionCursorToIndex();
            }
            
            if(Input.GetKeyDown(KeyCode.DownArrow)){
                interactIndex = (interactIndex + 1) % interactCount;
                SetOptionCursorToIndex();
            }
            
            yield return null;
        }
        
        optionParent.SetActive(false);
        playerState = PlayerState.None;
    }

    public void SetPaused(bool newPaused){
        playerPaused = newPaused;

        characterRenderable.PlayAnimation(AnimationState.Idle);

        // Fixes up camera jitter when un pausing
        if(!playerPaused){
            lookAngle = transform.rotation.eulerAngles.y;
            skyPlaneMeshRenderer.material.SetTextureOffset("_MainTex", new Vector2(transform.rotation.eulerAngles.y / 360.0f, 0.0f));
        }
    }

    public void ShowMessage(string message){
        StartCoroutine(ShowMessageCoroutine(message));
    }

    private IEnumerator ShowMessageCoroutine(string message){
        if(showingMessage){
            yield break;
        }

        showingMessage = true;
        messageText.text = message;
        messageBackground.gameObject.SetActive(true);

        // Wait
        Timer waitTimer = new Timer(3.0f);
        waitTimer.Start();

        while(!waitTimer.Finished()){
            yield return null;
        }

        // Hide
        messageBackground.gameObject.SetActive(false);
        showingMessage = false;
    }

    public void ShowLocation(){
        StartCoroutine(ShowLocationCoroutine());
    }

    private IEnumerator ShowLocationCoroutine(){
        locationText.enabled = true;

        Timer showTimer = new Timer(3.5f);
        showTimer.Start();

        while(!showTimer.Finished()){
            yield return null;
        }

        locationText.enabled = false;
    }

    public bool DealDamage(float damage){
        if(currentHealth < 0 || playerState == PlayerState.Rolling){
            return false;
        }

        currentHealth -= damage;

        if(currentHealth < 0){
            playerState = PlayerState.Dead;

            characterRenderable.PlayAnimation(AnimationState.Death);

            StartCoroutine(DieCoroutine());

            return true;
        }

        if(currentStamina <= (staggerThreshold * maxStamina)){ // Staggering based on stamina
            playerState = PlayerState.Staggered;

            staggerTimer.Start();

            characterRenderable.PlayAnimation(AnimationState.Stagger);
        }

        return true;
    }

    public IEnumerator DieCoroutine(){
        // Just wait for a while
        Timer waitTimer = new Timer(3.0f);
        waitTimer.Start();
        while(!waitTimer.Finished()){
            // Enable game over text halfway through
            gameOverText.enabled = waitTimer.Parameterized() > 0.5f;
            yield return null;
        }

        // Fade out
        Timer fadeTimer = new Timer(1.5f);
        fadeTimer.Start();
        while(!fadeTimer.Finished()){
            float t = 1.0f - fadeTimer.Parameterized();
            t = Mathf.Round(t * 10.0f) / 10.0f;

            RenderSettings.ambientLight = new Color(t, t, t, 1.0f);
            yield return null;
        }

        RenderSettings.ambientLight = new Color(0.0f, 0.0f, 0.0f, 1.0f);

        // Reset things
        player.transform.position = gameComponent.respawnTransform.position;

        player.transform.rotation = gameComponent.respawnTransform.rotation;
        lookAngle = transform.rotation.eulerAngles.y;

        playerSpriteTransform.localRotation = Quaternion.identity;
        spriteDirection = 0.0f;

        currentHealth = maxHealth;
        currentStamina = maxStamina;

        itemImage.sprite = itemFull;

        characterRenderable.PlayAnimation(AnimationState.Idle);

        // Hide messages? Reset Paused? Reset boxes?
        gameOverText.enabled = false;

        // Reset the levels after a frame, and give it a frame
        yield return null;
        gameComponent.ResetLevels();
        yield return null;

        // Wait in darkness for a sec
        waitTimer.SetDuration(0.5f);
        waitTimer.Start();
        while(!waitTimer.Finished()){
            yield return null;
        }

        // Fade in
        fadeTimer.Start();
        while(!fadeTimer.Finished()){
            float t = fadeTimer.Parameterized();
            t = Mathf.Round(t * 10.0f) / 10.0f;

            RenderSettings.ambientLight = new Color(t, t, t, 1.0f);
            yield return null;
        }

        RenderSettings.ambientLight = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        // Set state after fade in to unlock movement, etc.
        playerState = PlayerState.None;
    }

    public void ShowBossBar(CreatureComponent bossCreature_){
        fightingBoss = true;
        bossCreature = bossCreature_;

        bossBarParent.SetActive(true);
    }
    
    public void ApplyEffects(EffectData[] effects){        
        for(int i = 0, count = effects.Length; i < count; ++i){
            if(effects[i].effectType == EffectType.ChangeCurrentHp){
                float effectValue = effects[i].GetFinalValue();
                if(effectValue < 0.0f){
                    DealDamage(Mathf.Abs(effectValue));
                } else {
                    currentHealth = Mathf.Clamp(currentHealth + effectValue, 0.0f, maxHealth);
                }
            } else if(effects[i].effectType == EffectType.ChangeCurrentStamina){
                Debug.Log("Not implemented yet!");
            } else if(effects[i].effectType == EffectType.ChangeMaxHp){
                maxHealth += effects[i].GetFinalValue();
                currentHealth = Mathf.Clamp(currentHealth, 0.0f, maxHealth);
            } else if(effects[i].effectType == EffectType.ChangeMaxStamina){
                Debug.Log("Not implemented yet!");
            } else if(effects[i].effectType == EffectType.RegenCurrentHp){
                Debug.Log("Not implemented yet!");
            } else if(effects[i].effectType == EffectType.RegenCurrentStamina){
                Debug.Log("Not implemented yet!");
            } else if(effects[i].effectType == EffectType.GiveState){
                Debug.Log("Not implemented yet!");
            } else if(effects[i].effectType == EffectType.RemoveState){
                Debug.Log("Not implemented yet!");
            }
        }
    }
    
    public void PlayAnimation(AnimationState animationState){
        characterRenderable.PlayAnimation(animationState);
    }
}