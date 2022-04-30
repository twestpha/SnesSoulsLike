using UnityEngine;
using System;

class EnemyComponent : MonoBehaviour {

    private const float MOVE_THRESHOLD_RANGE = 0.1f;

    private const int IDLE_ANIMATION_INDEX         = 0;
    private const int WALK_ANIMATION_INDEX         = 1;
    private const int LIGHT_ATTACK_ANIMATION_INDEX = 2;
    private const int HEAVY_ATTACK_ANIMATION_INDEX = 3;

    [Header("Health and Damage")]
    public float maxHealth;
    public float currentHealth;

    [Space(10)]
    public float damageChance;
    public float damageTime;

    [Space(10)]
    public float deathTime;

    [Header("Movement")]
    public float moveSpeed = 1.0f;
    public float spriteRotateTime = 1.0f;

    [Space(10)]
    public float engageRange;
    public float leashRange;

    [Header("Attacking")]
    public float attackRange;
    public float lightAttackChance;

    [Space(10)]
    public HitBoxComponent lightAttackHitBox;
    public HitBoxComponent heavyAttackHitBox;

    [Space(10)]
    public float lightAttackTime;
    public float lightAttackDelayTime;
    public float lightAttackDuration;
    public float lightCooldownTime;

    [Space(10)]
    public float heavyAttackTime;
    public float heavyAttackDelayTime;
    public float heavyAttackDuration;
    public float heavyCooldownTime;

    public enum EnemyState {
        Inactive,
        Idle,
        Moving,
        LightAttack,
        HeavyAttack,
        Damaged,
        Dead,
    }

    [Header("State")]
    public EnemyState enemyState;

    [Header("Animation")]
    public RotatableComponent spriteRotatable;
    public MaterialAnimationComponent materialAnimation;

    private PlayerComponent player;
    private CharacterController characterController;

    private Vector3 startPosition;
    private Vector3 moveTargetPosition;

    private bool attackDamageStarted;

    private Timer attackTimer = new Timer();
    private Timer attackDelayTimer = new Timer();
    private Timer damageTimer;
    private Timer deathTimer;
    private Timer cooldownTimer = new Timer();

    void Start(){
        player = PlayerComponent.player;
        startPosition = transform.position;

        damageTimer = new Timer(damageTime);
        deathTimer = new Timer(deathTime);

        characterController = GetComponent<CharacterController>();

        materialAnimation.looping = true;
        spriteRotatable.SetAnimationIndex(IDLE_ANIMATION_INDEX);
        materialAnimation.ForceUpdate();
    }

    void Update(){
        if(enemyState != EnemyState.Inactive){
            Vector3 toPlayer = player.transform.position - transform.position;
            float playerDistance = toPlayer.magnitude;
            float selfDistance = (startPosition - transform.position).magnitude;

            if(enemyState == EnemyState.Idle){
                if(playerDistance < engageRange){
                    enemyState = EnemyState.Moving;

                    materialAnimation.looping = true;
                    spriteRotatable.SetAnimationIndex(WALK_ANIMATION_INDEX);
                    materialAnimation.ForceUpdate();

                    moveTargetPosition = player.transform.position;
                }

                if(selfDistance > leashRange){
                    enemyState = EnemyState.Moving;

                    materialAnimation.looping = true;
                    spriteRotatable.SetAnimationIndex(WALK_ANIMATION_INDEX);
                    materialAnimation.ForceUpdate();

                    moveTargetPosition = startPosition;
                }
            } else if(enemyState == EnemyState.Moving){
                Vector3 toTarget = moveTargetPosition - transform.position;
                float toTargetDistance = toTarget.magnitude;

                if(playerDistance <= attackRange && cooldownTimer.Finished()){
                    // face player
                    transform.rotation = Quaternion.Euler(
                        transform.rotation.x,
                        Mathf.Atan2(toPlayer.x, toPlayer.z) * Mathf.Rad2Deg - 5.0f, // Magic number makes rotations look better *shrug*
                        transform.rotation.z
                    );

                    bool lightAttack = UnityEngine.Random.value <= lightAttackChance;
                    attackDamageStarted = false;

                    attackTimer.SetDuration(lightAttack ? lightAttackTime : heavyAttackTime);
                    attackTimer.Start();

                    attackDelayTimer.SetDuration(lightAttack ? lightAttackDelayTime : heavyAttackDelayTime);
                    attackDelayTimer.Start();

                    materialAnimation.looping = false;
                    spriteRotatable.SetAnimationIndex(lightAttack ? LIGHT_ATTACK_ANIMATION_INDEX : HEAVY_ATTACK_ANIMATION_INDEX);
                    materialAnimation.ForceUpdate();

                    enemyState = lightAttack ? EnemyState.LightAttack : EnemyState.HeavyAttack;
                }

                if(toTargetDistance < MOVE_THRESHOLD_RANGE){
                    enemyState = EnemyState.Idle;
                } else {
                    characterController.SimpleMove(toTarget.normalized * moveSpeed);

                    // face move direction, snapping instantly so player can read directionality better/faster
                    transform.rotation = Quaternion.Euler(
                        transform.rotation.x,
                        Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg - 5.0f, // Magic number makes rotations look better *shrug*
                        transform.rotation.z
                    );

                }
            } else if(enemyState == EnemyState.LightAttack){
                // wait for anim, trigger hitbox
                if(!attackDamageStarted && attackDelayTimer.Finished()){
                    attackDamageStarted = true;
                    lightAttackHitBox.EnableForTime(lightAttackDuration);
                }

                if(attackTimer.Finished()){
                    enemyState = EnemyState.Idle;

                    materialAnimation.looping = true;
                    spriteRotatable.SetAnimationIndex(IDLE_ANIMATION_INDEX);
                    materialAnimation.ForceUpdate();

                    cooldownTimer.SetDuration(lightCooldownTime);
                    cooldownTimer.Start();
                }
            } else if(enemyState == EnemyState.HeavyAttack){
                // wait for anim, trigger hitbox
                if(!attackDamageStarted && attackDelayTimer.Finished()){
                    attackDamageStarted = true;
                    heavyAttackHitBox.EnableForTime(heavyAttackDuration);
                }

                if(attackTimer.Finished()){
                    enemyState = EnemyState.Idle;

                    materialAnimation.looping = true;
                    spriteRotatable.SetAnimationIndex(IDLE_ANIMATION_INDEX);
                    materialAnimation.ForceUpdate();

                    cooldownTimer.SetDuration(heavyCooldownTime);
                    cooldownTimer.Start();
                }
            } else if(enemyState == EnemyState.Damaged){
                if(damageTimer.Finished()){
                    enemyState = EnemyState.Idle;
                    // anim
                }
            } else if(enemyState == EnemyState.Dead){
                // play anim, disable lots of things
            }
        }
    }

    public void DealDamage(float damage){
        currentHealth -= damage; // for now. Later set death state + animation

        if(currentHealth < 0){
            enemyState = EnemyState.Dead;
            // anim
            return;
        }

        if(UnityEngine.Random.value < damageChance){
            enemyState = EnemyState.Damaged;
            // anim
            damageTimer.Start();
        }
    }

    void OnTriggerEnter(Collider other){
        if(player != null && other.gameObject == player.gameObject && enemyState == EnemyState.Inactive){
            enemyState = EnemyState.Idle;
            // play idle animation
        }
    }
}