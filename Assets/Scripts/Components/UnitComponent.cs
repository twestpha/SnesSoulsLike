using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
class UnitComponent : MonoBehaviour {

    private const float DIRECTION_TIME = 0.1f;
    private const float ABILITY_DIRECTION_TIME = 0.05f;

    public float moveSpeed;
    public float moveSpeedTime;
    public float turnSpeed;

    public enum Team {
        Player,
        Enemy,
    }

    public enum UnitState {
        Idle,
        PerformingAbility,
        Dead,
    }

    public Team team;

    public Transform rootTransform;

    public AnimationComponent anim;

    public UnitAbilityData[] equippedAbilities;

    private UnitState unitState;

    private float originalCharacterHeight;
    private float currentMoveSpeed;
    private float currentMoveAcceleration;

    private Vector3 inputDirection;
    private Vector3 previousNonZeroInputDirection;


    private Vector3 abilityMoveDirection;
    private Vector3 currentMoveDirection;
    private Vector3 previousMoveDirection;
    private Vector3 currentMoveDirectionAcceleration;

    private HealthComponent health;
    private CharacterController characterController;
    private UnitAbilityData currentlyPerformingAbility;

    private Timer abilityTimer = new Timer();
    private Timer damageTimer = new Timer();

    void Start(){
        characterController = GetComponent<CharacterController>();
        originalCharacterHeight = characterController.height;

        health = GetComponent<HealthComponent>();
        health.RegisterOnDamagedDelegate(OnDamaged);
        health.RegisterOnKilledDelegate(OnKilled);
    }

    void Update(){
        if(unitState == UnitState.Idle){
            UpdateMovement();
        } else if(unitState == UnitState.PerformingAbility){
            UpdateAbility();
        } else if(unitState == UnitState.Dead){
            // Nothing for now
        }
    }

    private void UpdateMovement(){
        previousMoveDirection = currentMoveDirection;

        // Damp move speed and then feed that to current vector so turning isn't so binary
        currentMoveSpeed = Mathf.SmoothDamp(currentMoveSpeed, inputDirection.magnitude > 0.01f ? moveSpeed : 0.0f, ref currentMoveAcceleration, moveSpeedTime);
        currentMoveDirection = Vector3.SmoothDamp(currentMoveDirection, inputDirection, ref currentMoveDirectionAcceleration, DIRECTION_TIME);

        characterController.SimpleMove(currentMoveDirection.normalized * currentMoveSpeed);

        // Flatten move direction and drive the look direction based on that
        Vector3 flatMoveDirection = currentMoveDirection;
        flatMoveDirection.y = 0.0f;

        if(flatMoveDirection.sqrMagnitude > 0.1f){
            rootTransform.rotation = Quaternion.RotateTowards(
                rootTransform.rotation,
                Quaternion.LookRotation(flatMoveDirection),
                turnSpeed * Time.deltaTime
            );
        }

        // Play walk/idle animation based on previous and current movement magnitudes
        if(currentMoveDirection.magnitude > 0.01f && previousMoveDirection.magnitude <= 0.01f){
            if(anim != null && !anim.IsPlayingAnimation("walk")){
                anim.PlayAnimation("walk");
            }
        } else if(previousMoveDirection.magnitude <= 0.01f && currentMoveDirection.magnitude < 0.01f){
            if(anim != null && !anim.IsPlayingAnimation("idle")){
                anim.PlayAnimation("idle");
            }
        }
    }

    private void UpdateAbility(){
        float abilityT = abilityTimer.Parameterized();

        // Update movement from ability
        if(abilityT <= currentlyPerformingAbility.movePercentage){
            float abilityMoveSpeed =
              currentlyPerformingAbility.maxMoveSpeed *
              currentlyPerformingAbility.moveSpeedCurve.Evaluate(
                abilityT / currentlyPerformingAbility.movePercentage
            );

            characterController.SimpleMove(abilityMoveDirection.normalized * abilityMoveSpeed);
        } else {
            abilityMoveDirection = Vector3.zero;
            currentMoveDirection = Vector3.zero;
        }

        // Update collision height/center from ability
        if(currentlyPerformingAbility.collisionResizeRangePercent.x <= abilityT
           && abilityT <= currentlyPerformingAbility.collisionResizeRangePercent.y
           && currentlyPerformingAbility.collisionResizeRangePercent.y > 0.0f){
            characterController.center = new Vector3(
                characterController.center.x,
                currentlyPerformingAbility.resizeHeight / 2.0f,
                characterController.center.z
            );

            characterController.height = currentlyPerformingAbility.resizeHeight;
        } else {
            characterController.center = new Vector3(
                characterController.center.x,
                originalCharacterHeight / 2.0f,
                characterController.center.z
            );

            characterController.height = originalCharacterHeight;
        }

        // Drive move direction towards ability move direction over time to make character's turning smoother
        currentMoveDirection = Vector3.SmoothDamp(currentMoveDirection, abilityMoveDirection.normalized, ref currentMoveDirectionAcceleration, ABILITY_DIRECTION_TIME);

        // Flatten move direction and drive the look direction based on that
        Vector3 flatMoveDirection = currentMoveDirection;
        flatMoveDirection.y = 0.0f;

        if(flatMoveDirection.sqrMagnitude > 0.1f){
            rootTransform.rotation = Quaternion.RotateTowards(
                rootTransform.rotation,
                Quaternion.LookRotation(flatMoveDirection),
                turnSpeed * Time.deltaTime
            );
        }

        // All done
        if(abilityTimer.Finished()){
            // Clear ability
            unitState = UnitState.Idle;
            currentlyPerformingAbility = null;

            // Clear velocity and reset animation to idle
            abilityMoveDirection = Vector3.zero;
            currentMoveDirection = Vector3.zero;

            if(anim != null && !anim.IsPlayingAnimation("idle")){
                anim.PlayAnimation("idle");
            }

            // Reset character controller
            characterController.center = new Vector3(
                characterController.center.x,
                originalCharacterHeight / 2.0f,
                characterController.center.z
            );

            characterController.height = originalCharacterHeight;
        }
    }

    public void SetInputDirection(Vector3 inputDirection_){
        inputDirection = inputDirection_;

        if(inputDirection.sqrMagnitude > 0.01f){
            previousNonZeroInputDirection = inputDirection;
        }
    }

    public void UseAbility(int index){
        if(index < 0 || index >= equippedAbilities.Length || equippedAbilities[index] == null){
            return;
        }

        // If already performing ability, check if interruptable
        if(unitState == UnitState.PerformingAbility && abilityTimer.Parameterized() < currentlyPerformingAbility.interruptPercent){
            return;
        }

        // TODO stamina costs?

        unitState = UnitState.PerformingAbility;
        currentlyPerformingAbility = equippedAbilities[index];

        abilityTimer.SetDuration(currentlyPerformingAbility.abilityDuration);
        abilityTimer.Start();

        if(!string.IsNullOrEmpty(currentlyPerformingAbility.animationName)){
            if(anim != null){
                anim.PlayAnimation(currentlyPerformingAbility.animationName);
            }
        }

        // Get ability in terms of last non zero input direction, but don't commit to the
        // rotation yet; let that happen passively.
        Vector3 inputDirection = previousNonZeroInputDirection;
        inputDirection.y = 0.0f;

        Quaternion originalRotation = rootTransform.rotation;
        rootTransform.rotation = Quaternion.LookRotation(inputDirection);

        abilityMoveDirection = rootTransform.TransformDirection(currentlyPerformingAbility.moveDirection);
        rootTransform.rotation = originalRotation;
    }

    void OnDamaged(HealthComponent health){

    }

    void OnKilled(HealthComponent health){
        unitState = UnitState.Dead;
    }

    public bool IsDead(){
        return unitState == UnitState.Dead;
    }
}