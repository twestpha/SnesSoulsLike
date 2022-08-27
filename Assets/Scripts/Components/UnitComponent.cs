using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
class UnitComponent : MonoBehaviour {

    private const float DIRECTION_TIME = 0.1f;

    public float moveSpeed;
    public float moveSpeedTime;
    public float turnSpeed;

    public enum Team {
        Player,
        Enemy,
    }

    public Team team;

    public Transform rootTransform;

    public AnimationComponent anim;

    public UnitAbilityData[] equippedAbilities;

    private bool performingAbility;
    private UnitAbilityData currentlyPerformingAbility;
    private Timer abilityTimer = new Timer();

    // Move attributes
    private Vector3 moveDirection;
    private CharacterController characterController;

    private float currentMoveSpeed;
    private float currentMoveAcceleration;

    private Vector3 currentDirection;
    private Vector3 currentDirectionAcceleration;

    void Start(){
        characterController = GetComponent<CharacterController>();
    }

    void Update(){
        if(performingAbility){
            UpdateAbility();
        } else {
            UpdateMovement();
        }

        UpdateDirection();
    }

    private void UpdateAbility(){
        // Update movement from ability
        if(abilityTimer.Parameterized() <= currentlyPerformingAbility.movePercentage){
            float abilityMoveSpeed =
              currentlyPerformingAbility.maxMoveSpeed *
              currentlyPerformingAbility.moveSpeedCurve.Evaluate(
                abilityTimer.Parameterized() / currentlyPerformingAbility.movePercentage
            );

            characterController.SimpleMove(moveDirection.normalized * abilityMoveSpeed);
        } else {
            moveDirection = Vector3.zero;
        }

        // Once finished, clear ability, velocity, and return to idle
        if(abilityTimer.Finished()){
            performingAbility = false;
            currentlyPerformingAbility = null;

            moveDirection = Vector3.zero;

            if(anim != null && !anim.IsPlayingAnimation("idle")){
                anim.PlayAnimation("idle");
            }
        }
    }

    private void UpdateMovement(){
        // Damp move speed and then feed that to current vector so turning isn't so binary
        currentMoveSpeed = Mathf.SmoothDamp(currentMoveSpeed, moveDirection.magnitude > 0.01f ? moveSpeed : 0.0f, ref currentMoveAcceleration, moveSpeedTime);
        currentDirection = Vector3.SmoothDamp(currentDirection, moveDirection.normalized, ref currentDirectionAcceleration, DIRECTION_TIME);

        characterController.SimpleMove(currentDirection.normalized * currentMoveSpeed);
    }

    private void UpdateDirection(){
        // Flatten move direction and drive the look direction based on that
        Vector3 flatMoveDirection = moveDirection;
        flatMoveDirection.y = 0.0f;

        if(flatMoveDirection.sqrMagnitude > 0.1f){
            rootTransform.rotation = Quaternion.RotateTowards(
                rootTransform.rotation,
                Quaternion.LookRotation(flatMoveDirection),
                turnSpeed * Time.deltaTime
            );
        }
    }

    public void SetMoveDirection(Vector3 moveDirection_){
        // Move direction can only be externally set when not performing and ability
        if(!performingAbility){
            if(moveDirection_.magnitude > 0.01f && moveDirection.magnitude <= 0.01f){
                if(anim != null && !anim.IsPlayingAnimation("walk")){
                    anim.PlayAnimation("walk");
                }
            } else if(moveDirection_.magnitude <= 0.01f && moveDirection.magnitude < 0.01f){
                if(anim != null && !anim.IsPlayingAnimation("idle")){
                    anim.PlayAnimation("idle");
                }
            }

            moveDirection = moveDirection_;
        }
    }

    public void UseAbility(int index){
        if(index < 0 || index >= equippedAbilities.Length || equippedAbilities[index] == null){
            return;
        }

        // If already performing ability, wait until interruptable
        if(performingAbility && abilityTimer.Parameterized() < currentlyPerformingAbility.interruptPercent){
            return;
        }

        // TODO stamina costs?

        performingAbility = true;
        currentlyPerformingAbility = equippedAbilities[index];

        abilityTimer.SetDuration(currentlyPerformingAbility.abilityDuration);
        abilityTimer.Start();

        if(!string.IsNullOrEmpty(currentlyPerformingAbility.animationName)){
            if(anim != null){
                anim.PlayAnimation(currentlyPerformingAbility.animationName);
            }
        }

        // Transform movement direction into root transform space
        moveDirection = rootTransform.TransformDirection(currentlyPerformingAbility.moveDirection);
    }
}