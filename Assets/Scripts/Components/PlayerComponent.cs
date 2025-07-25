using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

public enum AnimationState {
    Idle,
    Walk,
    Roll,
    Duck,
    UnDuck,
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

    [Header("Hurt Box and Ducking")]
    public BoxCollider hurtBox;

    [Space(10)]
    public Vector3 originalHurtBoxCenter;
    public Vector3 originalHurtBoxSize;

    [Space(10)]
    public Vector3 duckingHurtBoxCenter;
    public Vector3 duckingHurtBoxSize;

    [Space(10)]
    public Vector2 originalRadiusAndHeight;
    public Vector3 originalCharacterControllerCenter;

    [Space(10)]
    public Vector2 duckingRadiusAndHeight;
    public Vector3 duckingCharacterControllerCenter;

    [Header("Hit Boxes and Attacking")]
    public HitBoxComponent lightAttackHitBox;
    public HitBoxComponent heavyAttackHitBox;

    [Space(10)]
    public float lightAttackTime;
    public float lightAttackDelayTime;
    public float lightAttackDuration;
    public float lightAttackStaminaCost;

    [Space(10)]
    public float heavyAttackTime;
    public float heavyAttackDelayTime;
    public float heavyAttackDuration;
    public float heavyAttackStaminaCost;

    [Space(10)]
    public float itemUseTime;
    public float itemDelayTime;
    public float itemUseHealAmount;

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

    public Image itemImage;
    public Sprite itemFull;
    public Sprite itemEmpty;

    public Text locationText;
    public Text gameOverText;

    public GameObject bossBarParent;
    public Image bossBar;

    private float spriteDirection;
    private float spriteDirectionVelocity;

    private Camera playerCamera;
    private CharacterController characterController;

    private bool fightingBoss;
    private CreatureComponent bossCreature;

    private bool playerPaused;
    private bool showingMessage;
    private bool hasItem = true;

    public enum PlayerState {
        None,
        Ducking,
        Rolling,
        LightAttack,
        HeavyAttack,
        Staggered,
        UsingItem,
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

        gameComponent = GameObject.FindObjectOfType<GameComponent>();

        playerHeight = transform.position.y;

        rollTimer = new Timer(rollTime);
        itemUseTimer = new Timer(itemUseTime);
        itemDelayTimer = new Timer(itemDelayTime);
        staggerTimer = new Timer(staggerTime);

        // Set aspect ratio of camera
        // float targetAspect = 8.0f / 7.0f;
        // float windowAspect = (float) Screen.width / (float) Screen.height;
        // float scaleHeight = windowAspect / targetAspect;
        // 
        // float scaleWidth = 1.0f / scaleHeight;
        // 
        // Rect rect = playerCamera.rect;
        // 
        // rect.width = scaleWidth;
        // rect.height = 1.0f;
        // rect.x = (1.0f - scaleWidth) / 2.0f;
        // rect.y = 0;

        lookAngle = transform.rotation.eulerAngles.y;

        // playerCamera.rect = rect;
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

        //         + ----------+
        //         |   Heavy   |
        // +----------+--------+
        // |   Item   | +-----------+
        // +----------+ |    Roll   |
        //     +--------+----------+
        //     |   Light   |
        //     + ----------+

        bool heavyAttack = Input.GetKeyDown(KeyCode.S);

        bool useItem = Input.GetKeyDown(KeyCode.A);
        bool duckRoll = Input.GetKeyDown(KeyCode.X);

        bool lightAttack = Input.GetKeyDown(KeyCode.Z);

        if(up){ movementVector += transform.forward; }
        if(left){ movementVector -= transform.right; }
        if(back){ movementVector -= transform.forward; }
        if(right){ movementVector += transform.right; }

        movementVector.y = 0.0f;

        if(playerState == PlayerState.None){
            if(movementVector.magnitude > 0.01f){
                cachedRollDirection = movementVector.normalized;
            }

            if(duckRoll){
                if(movementInput && currentStamina > rollStaminaCost){
                    Roll();
                }
            } else if((lightAttack && currentStamina > lightAttackStaminaCost) || (heavyAttack && currentStamina > heavyAttackStaminaCost)){
                if(lightAttack && currentStamina > lightAttackStaminaCost){
                    playerState = PlayerState.LightAttack;

                    currentStamina -= lightAttackStaminaCost;

                    characterRenderable.PlayAnimation(AnimationState.LightAttack);

                    attackTimer.SetDuration(lightAttackTime);
                    attackDelayTimer.SetDuration(lightAttackDelayTime);
                } else if(heavyAttack && currentStamina > heavyAttackStaminaCost){
                    playerState = PlayerState.HeavyAttack;

                    currentStamina -= heavyAttackStaminaCost;

                    characterRenderable.PlayAnimation(AnimationState.HeavyAttack);

                    attackTimer.SetDuration(heavyAttackTime);
                    attackDelayTimer.SetDuration(heavyAttackDelayTime);
                }

                attackTimer.Start();
                attackDelayTimer.Start();
                attackDamageStarted = false;
            } else if(useItem && hasItem){
                playerState = PlayerState.UsingItem;

                characterRenderable.PlayAnimation(AnimationState.UseItem);

                itemUseTimer.Start();
                itemDelayTimer.Start();
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
        } else if(playerState == PlayerState.Ducking){
            if(!duckRoll){
                playerState = PlayerState.None;

                if(movementInput){
                    characterRenderable.PlayAnimation(AnimationState.Walk);
                } else {
                    characterRenderable.PlayAnimation(AnimationState.Idle);
                }

                hurtBox.size = originalHurtBoxSize;
                hurtBox.center = originalHurtBoxCenter;

                characterController.radius = originalRadiusAndHeight.x;
                characterController.height = originalRadiusAndHeight.y;
                characterController.center = originalCharacterControllerCenter;
            }

            if(movementInput && currentStamina > rollStaminaCost){
                cachedRollDirection = movementVector.normalized;
                Roll();
            }
        } else if(playerState == PlayerState.LightAttack){
            if(!attackDamageStarted && attackDelayTimer.Finished()){
                attackDamageStarted = true;
                lightAttackHitBox.EnableForTime(lightAttackDuration);
            }

            if(attackTimer.Finished()){
                playerState = PlayerState.None;

                if(movementInput){
                    characterRenderable.PlayAnimation(AnimationState.Walk);
                } else {
                    characterRenderable.PlayAnimation(AnimationState.Idle);
                }
            }
        } else if(playerState == PlayerState.HeavyAttack){
            if(!attackDamageStarted && attackDelayTimer.Finished()){
                attackDamageStarted = true;
                heavyAttackHitBox.EnableForTime(heavyAttackDuration);
            }

            if(attackTimer.Finished()){
                playerState = PlayerState.None;

                if(movementInput){
                    characterRenderable.PlayAnimation(AnimationState.Walk);
                } else {
                    characterRenderable.PlayAnimation(AnimationState.Idle);
                }
            }
        } else if(playerState == PlayerState.UsingItem){
            if(itemDelayTimer.Finished() && hasItem){
                hasItem = false;
                itemImage.sprite = itemEmpty;
                currentHealth = Mathf.Min(currentHealth + itemUseHealAmount, maxHealth);
            }

            if(itemUseTimer.Finished()){
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
            if(Input.GetKey(KeyCode.W)){ rotateDirection = 1.0f; }
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

        // Event though rolling makes the player invincible for a bit, still reset the hurtbox
        hurtBox.size = originalHurtBoxSize;
        hurtBox.center = originalHurtBoxCenter;

        characterController.radius = originalRadiusAndHeight.x;
        characterController.height = originalRadiusAndHeight.y;
        characterController.center = originalCharacterControllerCenter;

        characterRenderable.PlayAnimation(AnimationState.Roll);
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

        // Show text box
        Timer showTimer = new Timer(0.5f);
        showTimer.Start();

        messageBackground.enabled = true;
        messageBackground.transform.localScale = new Vector3(1.0f, 0.0f, 1.0f);

        while(!showTimer.Finished()){
            float t = Mathf.Round(showTimer.Parameterized() * 4.0f) / 4.0f;
            messageBackground.transform.localScale = new Vector3(1.0f, t, 1.0f);

            yield return null;
        }

        messageBackground.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        messageText.enabled = true;

        // Wait
        Timer waitTimer = new Timer(3.0f);
        waitTimer.Start();

        while(!waitTimer.Finished()){
            yield return null;
        }

        // Hide
        messageText.enabled = false;
        showTimer.Start();

        while(!showTimer.Finished()){
            float t = Mathf.Round(showTimer.Parameterized() * 4.0f) / 4.0f;
            messageBackground.transform.localScale = new Vector3(1.0f, 1.0f - t, 1.0f);

            yield return null;
        }

        messageBackground.transform.localScale = new Vector3(1.0f, 0.0f, 1.0f);
        messageBackground.enabled = true;

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

        hasItem = true; // Reset "estus"
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
}