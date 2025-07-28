using UnityEngine;
using System;

enum Attitude {
    Friendly,
    Scared,
    Hostile,
}

[Serializable]
class LootOption {
    // TODO overhaul this into loot groups but this is a good start
    public string optionLocText;
    public ItemType requiredItem;
    public ItemData lootItem;
}

class CreatureComponent : MonoBehaviour {

    private const float MOVE_THRESHOLD_RANGE = 0.05f;
    private const float FLEE_STAMINA_COST = 5.0f;

    [Header("Attitude")]
    public Attitude attitude;
    
    [Header("Health and Damage")]
    public float maxHealth;
    public float currentHealth;
    
    [Space(10)]
    public float maxStamina;
    public float currentStamina;
    public float staminaRegenRate;

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
    
    [Header("Looting")]
    public LootOption[] lootOptions;

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
        
        currentStamina = maxStamina;
    }

    void Update(){
        if(attitude == Attitude.Friendly){
            UpdateFriendly();
        } else if(attitude == Attitude.Scared){
            UpdateScared();
        } else if(attitude == Attitude.Hostile){
            UpdateHostile();
        }
        
        currentStamina = Mathf.Clamp(currentStamina + staminaRegenRate * Time.deltaTime, 0.0f, maxStamina);
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
                if(playerDistance < engageRange && currentStamina >= FLEE_STAMINA_COST){
                    currentStamina -= FLEE_STAMINA_COST;
                    
                    creatureState = CreatureState.Moving;
                    characterRenderable.PlayAnimation(AnimationState.Walk);
                    
                    moveTargetPosition = transform.position + (-toPlayer.normalized * 3.0f);
                }
            } else if(creatureState == CreatureState.Moving){
                Debug.DrawLine(transform.position, moveTargetPosition, Color.red, 0.0f, false);
                Vector3 toTarget = moveTargetPosition - transform.position;
                toTarget.y = 0.0f;
                
                float toTargetDistance = toTarget.magnitude;
                
                if(toTargetDistance <= MOVE_THRESHOLD_RANGE){
                    // This is buggy and sometimes fails... I think it's due to low framerate? fuck it
                    creatureState = CreatureState.Idle;
                    characterRenderable.PlayAnimation(AnimationState.Idle);
                } else {
                    characterController.SimpleMove(toTarget.normalized * moveSpeed);
                    
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

                // if(playerDistance <= attackRange && cooldownTimer.Finished()){
                //     // face player
                //     transform.rotation = Quaternion.Euler(
                //         transform.rotation.x,
                //         Mathf.Atan2(toPlayer.x, toPlayer.z) * Mathf.Rad2Deg - 5.0f, // Magic number makes rotations look better *shrug*
                //         transform.rotation.z
                //     );
                // 
                //     // TODO switch these to ability-based things
                //     characterRenderable.PlayAnimation(lightAttack ? AnimationState.LightAttack : AnimationState.HeavyAttack);
                // 
                //     creatureState = lightAttack ? CreatureState.LightAttack : CreatureState.HeavyAttack;
                // }

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
                // TODO switch to ability
            } else if(creatureState == CreatureState.HeavyAttack){
                // TODO switch to ability
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
                // Not implemented for creatures
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