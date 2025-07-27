using System.Collections.Generic;
using UnityEngine;

public enum DamageType {
    NotSureYet,
}

public class ProjectileComponent : MonoBehaviour {
    
    public const int NO_PROJECTILE_COLLIDE_LAYER = 1 << 11;

    public const float PROJECTILE_EFFECTS_OFFSET = 0.1f;
    public const float PROJECTILE_DECAL_OFFSET = 0.001f;

    [HeaderAttribute("Projectile Component")]
    public float maxDistance = 100.0f;
    public float gravityModifier = 0.0f;
    public int collisions = 1;
    public enum CollisionMode {
        Ricochet, Pierce
    }
    public CollisionMode collisionMode;
    public float collisionModeChance = 0.0f;
    public GameObject optionalImpactEffects;
    // public SoundClip impactSound;

    public GameObject optionalDecalObject;

    private bool fired;
    private int collisionsRemaining;
    private bool shouldKill;
    private string poolIdentifier; // Register as 'free' with this if we're a pooled bullet

    private float damage;
    private DamageType type;

    private Vector3 startPosition;
    private Vector3 velocity;

    private GameObject firer;

    //##############################################################################################
    // Update the bullet, moving it along it's velocity, unless it collides with something.
    // If that something is a damageable, deal the damage to it. Either way, mark the bullet
    // for destruction next update.
    // This is done in Fixed Update so that the physics is properly synchronized for the bullet.
    //##############################################################################################
    void FixedUpdate(){
        // Prevents updating before firing information has been provided, since fixed update is
        // disjoint from regular unity update
        if(!fired){
            return;
        }

        // The destruction is done 1 frame after being marked for kill so the bullet and effects
        // appear in the correct position visually for that last frame, before bullet is destroyed.
        if(shouldKill){
            // Destroy if not pooled, otherwise mark this bullet as freed
            if(poolIdentifier == null){
                Destroy(gameObject);
            } else {
                // PooledGameObjectManager.FreeInstanceToPool(poolIdentifier, gameObject);
            }

            return;
        }

        velocity += Physics.gravity * gravityModifier * Time.deltaTime * Time.deltaTime;
        Vector3 move = velocity * Time.deltaTime;
        float moveDist = move.magnitude;

        // Kill the bullet if it's gone too far
        if((transform.position - startPosition).sqrMagnitude >= maxDistance * maxDistance){
            shouldKill = true;
        }

        // See if move would hit anything, ignoring the 'no bullet collide' layer and triggers
        RaycastHit hit;
        if(Physics.Raycast(transform.position, move, out hit, moveDist, ~NO_PROJECTILE_COLLIDE_LAYER, QueryTriggerInteraction.Ignore)){
            if(hit.collider.gameObject != firer){
                transform.position = hit.point;

                CreatureComponent hitCreature = hit.collider.gameObject.GetComponent<CreatureComponent>();
                PlayerComponent hitPlayer = hit.collider.gameObject.GetComponent<PlayerComponent>();

                // Never ricochet off a damageable
                if(hitCreature != null){
                    collisionsRemaining = 0;
                    // hitCreature.DealDamage(damage, type, startPosition, firer);
                } else if(hitPlayer != null){
                    collisionsRemaining = 0;
                    // hitPlayer.DealDamage(damage, type, startPosition, firer);
                } else {
                    // Don't spawn decals when hitting damageable
                    // if(optionalDecalObject != null && damageable == null){
                    //     GameObject decalInstance = GameObject.Instantiate(optionalDecalObject);
                    // 
                    //     // Add random offset to prevent z-fighting
                    //     float randomOffset = (Random.value * PROJECTILE_EFFECTS_OFFSET);
                    // 
                    //     decalInstance.transform.position = hit.point + (hit.normal * (PROJECTILE_DECAL_OFFSET + randomOffset));
                    //     decalInstance.transform.rotation = Quaternion.LookRotation(hit.normal);
                    // }

                    // TODO add impact effects lookup system for hit object
                    if(optionalImpactEffects != null){
                        GameObject fx = GameObject.Instantiate(optionalImpactEffects);

                        // Scoot fx back away from collision a little
                        fx.transform.position = transform.position + (-move).normalized * PROJECTILE_EFFECTS_OFFSET;
                    }
                }

                // Play impact sound if needed
                // if(impactSound != null){
                //     SoundManagerComponent.PlaySound(impactSound, gameObject);
                // }

                if(collisionsRemaining > 0){
                    collisionsRemaining--;

                    if(Random.value <= collisionModeChance){
                        if(collisionMode == CollisionMode.Ricochet){
                            float velocityMagnitude = velocity.magnitude;
                            velocity = Vector3.Reflect(velocity.normalized, hit.normal).normalized * velocityMagnitude;
                            transform.position = hit.point + (velocity * 0.01f);
                        } else if(collisionMode == CollisionMode.Pierce){
                            transform.position += move;
                        }
                    } else {
                        collisionsRemaining = 0;
                    }
                }

                if(collisionsRemaining <= 0){
                    shouldKill = true;
                }
            }
        } else {
            transform.position += move;
        }
    }

    //##############################################################################################
    // Notify the bullet it's been fired, with the provided characteristics
    //##############################################################################################
    public void Fire(float damage_, DamageType type_, Vector3 velocity_, GameObject firer_){
        fired = true;
        shouldKill = false;
        collisionsRemaining = collisions;

        damage = damage_;
        type = type_;
        velocity = velocity_;
        firer = firer_;

        startPosition = transform.position;
    }

    //##############################################################################################
    // Mark the bullet as being a pooled bullet instance, and give it the identifier for pool
    // operations
    //##############################################################################################
    public void SetAsPooled(string identifier){
        poolIdentifier = identifier;
    }
}
