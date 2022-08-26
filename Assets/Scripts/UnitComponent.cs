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
        // Damp move speed and then feed that to current vector so turning isn't so binary
        currentMoveSpeed = Mathf.SmoothDamp(currentMoveSpeed, moveDirection.magnitude > 0.01f ? moveSpeed : 0.0f, ref currentMoveAcceleration, moveSpeedTime);
        currentDirection = Vector3.SmoothDamp(currentDirection, moveDirection.normalized, ref currentDirectionAcceleration, DIRECTION_TIME);

        characterController.SimpleMove(currentDirection.normalized * currentMoveSpeed);

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