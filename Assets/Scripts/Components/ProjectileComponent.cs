using System.Collections.Generic;
using UnityEngine;

public class ProjectileComponent : MonoBehaviour {
    
    public const int NO_PROJECTILE_COLLIDE_LAYER = 1 << 11;

    public const float PROJECTILE_EFFECTS_OFFSET = 0.1f;
    public const float PROJECTILE_DECAL_OFFSET = 0.001f;
    
    public const float SPHERECAST_RADIUS = 0.1f;

    [HeaderAttribute("Projectile Component")]
    public float maxDistance = 100.0f;
    public float gravityModifier = 0.0f;
    public GameObject optionalImpactEffects;
    // public SoundClip impactSound;

    public GameObject optionalDecalObject;

    private bool fired;
    private int collisionsRemaining;
    private bool shouldKill;

    private AbilityData ability;

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
            Destroy(gameObject);
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
        // Do both a ray and sphere cast, using the former for environment and the latter for damage
        RaycastHit rayHit;
        Physics.Raycast(transform.position, move, out rayHit, moveDist, ~NO_PROJECTILE_COLLIDE_LAYER, QueryTriggerInteraction.Ignore);
        
        RaycastHit sphereHit;
        Debug.DrawLine(transform.position, transform.position + (transform.up * SPHERECAST_RADIUS), Color.blue, 5.0f, false);
        Debug.DrawLine(transform.position, transform.position + (-transform.up * SPHERECAST_RADIUS), Color.blue, 5.0f, false);
        Debug.DrawLine(transform.position, transform.position + (transform.right * SPHERECAST_RADIUS), Color.blue, 5.0f, false);
        Debug.DrawLine(transform.position, transform.position + (-transform.right * SPHERECAST_RADIUS), Color.blue, 5.0f, false);
        
        Physics.SphereCast(transform.position, SPHERECAST_RADIUS, move, out sphereHit, moveDist, ~NO_PROJECTILE_COLLIDE_LAYER, QueryTriggerInteraction.Ignore);
        
        if(rayHit.collider != null || sphereHit.collider != null){
            PlayerComponent player = sphereHit.collider?.gameObject.GetComponentInParent<PlayerComponent>() ?? null;
            CreatureComponent creature = sphereHit.collider?.gameObject.GetComponentInParent<CreatureComponent>() ?? null;
            
            if(player == null){ player = rayHit.collider?.gameObject.GetComponentInParent<PlayerComponent>() ?? null; }
            if(creature == null){ creature = rayHit.collider?.gameObject.GetComponentInParent<CreatureComponent>() ?? null; }
            
            if(player != null || creature != null){
                transform.position = sphereHit.point;

                if(player != null){
                    player.ApplyEffects(ability.effects);
                } else if(creature != null){
                    creature.ApplyEffects(ability.effects);
                }
            } else {
                // Spawn FX maybe?
            }
            
            shouldKill = true;
        } else {
            transform.position += move;
        }
    }

    //##############################################################################################
    // Notify the bullet it's been fired, with the provided characteristics
    //##############################################################################################
    public void Fire(AbilityData ability_, Vector3 velocity_, GameObject firer_){
        fired = true;
        shouldKill = false;

        ability = ability_;
        velocity = velocity_;
        firer = firer_;

        startPosition = transform.position;
    }
}
