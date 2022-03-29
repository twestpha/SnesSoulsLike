using System;
using UnityEngine;

class PlayerComponent : MonoBehaviour {
    public static PlayerComponent player;

    private const int IGNORE_CAMERA_LAYER_MASK = 1 << 3;

    private const int IDLE_ANIMATION_INDEX        = 0;
    private const int ROLL_ANIMATION_INDEX        = 1;
    private const int DUCK_ANIMATION_INDEX        = 2;
    private const int UNDUCK_ANIMATION_INDEX      = 3;
    private const int WALK_ANIMATION_INDEX        = 4;
    private const int LIGHTATTACK_ANIMATION_INDEX = 5;

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

    public float staminaRegenRate;

    [Header("Hurt Box and Ducking")]
    public BoxCollider hurtBox;

    public Vector3 originalHurtBoxCenter;
    public Vector3 originalHurtBoxSize;

    public Vector3 duckingHurtBoxCenter;
    public Vector3 duckingHurtBoxSize;

    [Header("Hit Boxes and Attacking")]
    public HitBoxComponent lightAttackHitBox;
    public HitBoxComponent heavyAttackHitBox;

    public float lightAttackTime;
    public float lightAttackDelayTime;
    public float lightAttackDuration;
    public float lightAttackStaminaCost;

    public float heavyAttackTime;
    public float heavyAttackDelayTime;
    public float heavyAttackDuration;
    public float heavyAttackStaminaCost;

    [Header("Camera")]
    public Vector2 cameraDistanceBounds;
    public Transform cameraRaycastOrigin;
    public MeshRenderer skyPlaneMeshRenderer;

    [Header("Sprites and Animation")]
    public Transform playerSpriteTransform;
    public RotatableComponent playerSpriteRotatable;
    public MaterialAnimationComponent playerAnimation;

    public AnimationCurve rollSpeedCurve;

    private float spriteDirection;
    private float spriteDirectionVelocity;

    private Camera playerCamera;
    private CharacterController characterController;

    private bool playerPaused;

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

    public PlayerState playerState;

    private Vector3 cachedRollDirection;
    private Timer rollTimer;

    private float lookAngle;
    private float playerHeight;

    private bool movingAnimation;
    private bool attackDamageStarted;

    private Timer attackTimer = new Timer();
    private Timer attackDelayTimer = new Timer();

    void Start(){
        player = this;

        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();

        playerHeight = transform.position.y;

        rollTimer = new Timer(rollTime);

        // Set aspect ratio of camera
        float targetAspect = 8.0f / 7.0f;
        float windowAspect = (float) Screen.width / (float) Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        float scaleWidth = 1.0f / scaleHeight;

        Rect rect = playerCamera.rect;

        rect.width = scaleWidth;
        rect.height = 1.0f;
        rect.x = (1.0f - scaleWidth) / 2.0f;
        rect.y = 0;

        lookAngle = transform.rotation.eulerAngles.y;

        playerCamera.rect = rect;
    }

    void Update(){
        if(playerPaused){
            return;
        }

        // Stamina regen
        currentStamina = Mathf.Min(currentStamina + (staminaRegenRate * Time.deltaTime), maxStamina);

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

        bool heavyAttack = Input.GetKey(KeyCode.S);

        bool useItem = Input.GetKey(KeyCode.A);
        bool duckRoll = Input.GetKey(KeyCode.X);

        bool lightAttack = Input.GetKey(KeyCode.Z);

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
                } else {
                    playerState = PlayerState.Ducking;

                    playerAnimation.looping = false;
                    playerSpriteRotatable.SetAnimationIndex(DUCK_ANIMATION_INDEX);
                    playerAnimation.ForceUpdate();

                    hurtBox.size = duckingHurtBoxSize;
                    hurtBox.center = duckingHurtBoxCenter;
                }
            } else if(lightAttack || heavyAttack){ // This creates stutter when hitting attack and not having stamina
                if(lightAttack && currentStamina > lightAttackStaminaCost){
                    playerState = PlayerState.LightAttack;

                    currentStamina -= lightAttackStaminaCost;

                    playerAnimation.looping = false;
                    playerSpriteRotatable.SetAnimationIndex(LIGHTATTACK_ANIMATION_INDEX);
                    playerAnimation.ForceUpdate();

                    attackTimer.SetDuration(lightAttackTime);
                    attackDelayTimer.SetDuration(lightAttackDelayTime);
                } else if(heavyAttack && currentStamina > heavyAttackStaminaCost){
                    playerState = PlayerState.HeavyAttack;

                    currentStamina -= heavyAttackStaminaCost;

                    playerAnimation.looping = false;
                    playerSpriteRotatable.SetAnimationIndex(LIGHTATTACK_ANIMATION_INDEX);
                    playerAnimation.ForceUpdate();

                    attackTimer.SetDuration(heavyAttackTime);
                    attackDelayTimer.SetDuration(heavyAttackDelayTime);
                }

                attackTimer.Start();
                attackDelayTimer.Start();
                attackDamageStarted = false;
            } else {
                // Only move if not duck/roll or attack
                characterController.SimpleMove(movementVector.normalized * moveSpeed);

                if(movementInput && !movingAnimation){
                    movingAnimation = true;

                    playerAnimation.looping = true;
                    playerSpriteRotatable.SetAnimationIndex(WALK_ANIMATION_INDEX);
                    playerAnimation.ForceUpdate();
                } else if(!movementInput && movingAnimation){
                    movingAnimation = false;

                    playerAnimation.looping = true;
                    playerSpriteRotatable.SetAnimationIndex(IDLE_ANIMATION_INDEX);
                    playerAnimation.ForceUpdate();
                }
            }
        } else if(playerState == PlayerState.Rolling){
            float t = rollSpeedCurve.Evaluate(rollTimer.Parameterized());
            t = Mathf.Round(t * 6.0f) / 6.0f; // Make it feel chonky

            characterController.SimpleMove(cachedRollDirection.normalized * moveSpeed * t);
            movementVector = cachedRollDirection;

            if(rollTimer.Finished()){
                playerState = PlayerState.None;

                playerAnimation.looping = true;

                if(movementInput){
                    playerSpriteRotatable.SetAnimationIndex(WALK_ANIMATION_INDEX);
                } else {
                    playerSpriteRotatable.SetAnimationIndex(IDLE_ANIMATION_INDEX);
                }

                playerAnimation.ForceUpdate();
            }
        } else if(playerState == PlayerState.Ducking){
            if(!duckRoll){
                // TODO if unducking and moving, just play move animation
                playerState = PlayerState.None;

                playerAnimation.looping = false;
                playerSpriteRotatable.SetAnimationIndex(UNDUCK_ANIMATION_INDEX);
                playerAnimation.ForceUpdate();

                hurtBox.size = originalHurtBoxSize;
                hurtBox.center = originalHurtBoxCenter;
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

                playerAnimation.looping = true;
                playerSpriteRotatable.SetAnimationIndex(movementInput ? WALK_ANIMATION_INDEX : IDLE_ANIMATION_INDEX);
                playerAnimation.ForceUpdate();
            }
        } else if(playerState == PlayerState.HeavyAttack){
            if(!attackDamageStarted && attackDelayTimer.Finished()){
                attackDamageStarted = true;
                heavyAttackHitBox.EnableForTime(heavyAttackDuration);
            }

            if(attackTimer.Finished()){
                playerState = PlayerState.None;

                playerAnimation.looping = true;
                playerSpriteRotatable.SetAnimationIndex(movementInput ? WALK_ANIMATION_INDEX : IDLE_ANIMATION_INDEX);
                playerAnimation.ForceUpdate();
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
        if(Input.GetKey(KeyCode.Q)){ rotateDirection = 1.0f; }
        if(Input.GetKey(KeyCode.W)){ rotateDirection = -1.0f; }

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
    }

    private void Roll(){
        rollTimer.Start();
        playerState = PlayerState.Rolling;

        currentStamina = Mathf.Max(currentStamina - rollStaminaCost, 0.0f);

        // Event though rolling makes the player invincible for a bit, still reset the hurtbox
        hurtBox.size = originalHurtBoxSize;
        hurtBox.center = originalHurtBoxCenter;

        playerAnimation.looping = false;
        playerSpriteRotatable.SetAnimationIndex(ROLL_ANIMATION_INDEX);
        playerAnimation.ForceUpdate();
    }

    public void SetPaused(bool newPaused){
        playerPaused = newPaused;

        playerAnimation.looping = true;
        playerSpriteRotatable.SetAnimationIndex(IDLE_ANIMATION_INDEX);
        playerAnimation.ForceUpdate();

        // Fixes up camera jitter when un pausing
        if(!playerPaused){
            lookAngle = transform.rotation.eulerAngles.y;
        }
    }
}