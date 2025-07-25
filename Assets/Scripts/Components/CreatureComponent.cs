using UnityEngine;
using System;

enum Attitude {
    Friendly,
    Scared,
    Hostile,
}

class CreatureComponent : MonoBehaviour {

    private const float MOVE_THRESHOLD_RANGE = 0.05f;

    [Header("Attitude")]
    public Attitude attitude;
    
    [Header("Health and Damage")]
    public float maxHealth;
    public float currentHealth;

    [Space(10)]
    public float flinchChance;
    public float flinchTime;

    [Space(10)]
    public float deathTime;

    [Header("Movement")]
    public float moveSpeed = 1.0f;
    public float rotateSpeed = 45.0f;

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

    public enum CreatureState {
        Inactive,
        Idle,
        Moving,
        LightAttack,
        HeavyAttack,
        Flinching,
        Dead,
    }

    [Header("State")]
    public CreatureState creatureState;

    [Header("Boss")]
    public bool isBoss;
    public string bossNameLoc;

    private PlayerComponent player;
    private CharacterController characterController;
    private CharacterRenderable characterRenderable;
    
    private Vector3 startPosition;
    private Vector3 moveTargetPosition;

    private bool attackDamageStarted;

    private Timer attackTimer = new Timer();
    private Timer attackDelayTimer = new Timer();
    private Timer flinchTimer;
    private Timer deathTimer;
    private Timer cooldownTimer = new Timer();

    void Start(){
        player = PlayerComponent.player;
        startPosition = transform.position;

        flinchTimer = new Timer(flinchTime);
        deathTimer = new Timer(deathTime);

        characterController = GetComponent<CharacterController>();
        
        characterRenderable = GetComponent<CharacterRenderable>();
        characterRenderable.onRenderingStart.Register(OnRenderingStart);
        characterRenderable.onRenderingEnd.Register(OnRenderingEnd);
    }

    void Update(){
        if(attitude == Attitude.Friendly){
            UpdateFriendly();
        } else if(attitude == Attitude.Scared){
            UpdateScared();
        } else if(attitude == Attitude.Hostile){
            UpdateHostile();
        }
    }
    
    private void UpdateFriendly(){
        // Honestly, not sure if we really need to do anything?
        // Maybe face player if they wander past, or just patrol some random points?
        // and then transition to hostile/scared if attacked?
    }
    
    private void UpdateScared(){
        // kinda the inverse of hostile, run away from the player
        if(creatureState != CreatureState.Inactive){
            Vector3 toPlayer = player.transform.position - transform.position;
            float playerDistance = toPlayer.magnitude;
            
            if(creatureState == CreatureState.Idle){
                if(playerDistance < engageRange){
                    creatureState = CreatureState.Moving;
                    characterRenderable.PlayAnimation(AnimationState.Walk);
                    
                    moveTargetPosition = transform.position + (-toPlayer.normalized * 3.0f);
                }
            } else if(creatureState == CreatureState.Moving){
                Vector3 toTarget = moveTargetPosition - transform.position;
                float toTargetDistance = toTarget.magnitude;
                
                if(toTargetDistance <= MOVE_THRESHOLD_RANGE){
                    // This is buggy and sometimes fails... I think it's due to low framerate? fuck it
                    creatureState = CreatureState.Idle;
                } else {
                    characterController.SimpleMove(toTarget.normalized * moveSpeed);
                    toTarget.y = 0.0f;

                    // face move direction, snapping instantly so player can read directionality faster
                    if(toTarget.magnitude > MOVE_THRESHOLD_RANGE){
                        transform.rotation = Quaternion.LookRotation(toTarget);
                    }
                }
            }
        }
    }
    
    private void UpdateHostile(){
        if(creatureState != CreatureState.Inactive){
            Vector3 toPlayer = player.transform.position - transform.position;
            float playerDistance = toPlayer.magnitude;
            float selfDistance = (startPosition - transform.position).magnitude;

            if(creatureState == CreatureState.Idle){
                if(playerDistance < engageRange){
                    creatureState = CreatureState.Moving;
                    characterRenderable.PlayAnimation(AnimationState.Walk);
                
                    
                    moveTargetPosition = player.transform.position;
                }

                if(selfDistance > leashRange){
                    creatureState = CreatureState.Moving;
                    characterRenderable.PlayAnimation(AnimationState.Walk);

                    moveTargetPosition = startPosition;
                }
            } else if(creatureState == CreatureState.Moving){
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

                    // TODO switch these to ability-based things
                    characterRenderable.PlayAnimation(lightAttack ? AnimationState.LightAttack : AnimationState.HeavyAttack);

                    creatureState = lightAttack ? CreatureState.LightAttack : CreatureState.HeavyAttack;
                }

                if(toTargetDistance < MOVE_THRESHOLD_RANGE){
                    creatureState = CreatureState.Idle;
                } else {
                    characterController.SimpleMove(toTarget.normalized * moveSpeed);

                    // face move direction, snapping instantly so player can read directionality better/faster
                    transform.rotation = Quaternion.Euler(
                        transform.rotation.x,
                        Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg - 5.0f, // Magic number makes rotations look better *shrug*
                        transform.rotation.z
                    );

                }
            } else if(creatureState == CreatureState.LightAttack){
                // wait for anim, trigger hitbox
                if(!attackDamageStarted && attackDelayTimer.Finished()){
                    attackDamageStarted = true;
                    lightAttackHitBox.EnableForTime(lightAttackDuration);
                }

                if(attackTimer.Finished()){
                    creatureState = CreatureState.Idle;
                    characterRenderable.PlayAnimation(AnimationState.Idle);

                    cooldownTimer.SetDuration(lightCooldownTime);
                    cooldownTimer.Start();
                }
            } else if(creatureState == CreatureState.HeavyAttack){
                // wait for anim, trigger hitbox
                if(!attackDamageStarted && attackDelayTimer.Finished()){
                    attackDamageStarted = true;
                    heavyAttackHitBox.EnableForTime(heavyAttackDuration);
                }

                if(attackTimer.Finished()){
                    creatureState = CreatureState.Idle;
                    characterRenderable.PlayAnimation(AnimationState.Idle);

                    cooldownTimer.SetDuration(heavyCooldownTime);
                    cooldownTimer.Start();
                }
            } else if(creatureState == CreatureState.Flinching){
                if(flinchTimer.Finished()){
                    creatureState = CreatureState.Idle;
                    characterRenderable.PlayAnimation(AnimationState.Idle);
                }
            } else if(creatureState == CreatureState.Dead){
                // Do anything?
            }
        }
    }

    public void DealDamage(float damage){
        if(creatureState == CreatureState.Dead){
            return;
        }

        currentHealth -= damage;

        if(currentHealth <= 0.0f){
            creatureState = CreatureState.Dead;
            characterRenderable.PlayAnimation(AnimationState.Death);

            return;
        }

        if(UnityEngine.Random.value < flinchChance){
            creatureState = CreatureState.Flinching;
            characterRenderable.PlayAnimation(AnimationState.Flinch);

            flinchTimer.Start();
        }
    }

    void OnRenderingStart(CharacterRenderable renderable){
        // TODO kick inventory to reup item visuals
        creatureState = CreatureState.Idle;
        
        characterRenderable.PlayAnimation(AnimationState.Idle);

        if(isBoss){
            PlayerComponent.player.ShowBossBar(this);
        }
    }
    
    void OnRenderingEnd(CharacterRenderable renderable){
        creatureState = CreatureState.Inactive;
    }
}