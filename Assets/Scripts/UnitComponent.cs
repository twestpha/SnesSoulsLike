using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
class UnitComponent : MonoBehaviour {

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

    private Vector3 velocity;
    private Vector3 acceleration;

    void Start(){
        characterController = GetComponent<CharacterController>();
    }

    void Update(){
        // Only accelerate when going from stopped to starting, effectively decelerating immediately
        if(moveDirection.magnitude > 0.01f){
            velocity = Vector3.SmoothDamp(velocity, moveDirection.normalized * moveSpeed, ref acceleration, moveSpeedTime);
        } else {
            velocity = Vector3.zero;
        }

        characterController.SimpleMove(velocity);

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