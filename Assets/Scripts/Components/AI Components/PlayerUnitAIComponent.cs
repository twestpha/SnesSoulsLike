using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

class PlayerUnitAIComponent : MonoBehaviour {

    private const float FOLLOW_LEASH_DISTANCE = 1.0f;
    private const float PLAYER_LEASH_DISTANCE = 3.0f;
    private const float PLAYER_KEEP_DISTANCE = 1.5f;

    private const float MIN_FIDGET_TIME = 1.5f;
    private const float MAX_FIDGET_TIME = 6.8f;
    private const float FIDGET_DURATION = 0.08f;

    public DetectorComponent detector;

    public enum Mode {
        Follow,
        Combat,
    }

    public enum FollowDirection {
        None,
        Back,
        BackRight,
        BackLeft,
        Right,
        Left,
    }

    private Mode mode;
    private UnitComponent unit;

    // Following
    private FollowDirection pickedFollowDirection;
    private Vector3 followPosition;

    // Fidgeting
    private bool fidgetStarted;
    private bool fidgetHappening;

    private Vector3 fidgetDirection;

    private Timer fidgetWaitTimer = new Timer();
    private Timer fidgetDurationTimer = new Timer(FIDGET_DURATION);

    void Start(){
        unit = GetComponent<UnitComponent>();
        followPosition = new Vector3(0.0f, 9999999.0f, 0.0f);
    }

    void Update(){
        if(mode == Mode.Follow){
            UpdateFollowing();
        }
    }

    private void UpdateFollowing(){
        Vector3 toFollowPosition = followPosition - transform.position;
        Vector3 toPlayerPosition = PlayerComponent.player.GetCurrentPlayerUnit().rootTransform.position - transform.position;

        bool inFollowLeash = toFollowPosition.magnitude < FOLLOW_LEASH_DISTANCE;
        bool inPlayerLeash = toPlayerPosition.magnitude < PLAYER_LEASH_DISTANCE;

        if(inFollowLeash && !inPlayerLeash){
            pickedFollowDirection = FollowDirection.None;
        }

        // TODO reset if can't reach leash in time? Just jump-ahead?

        if(!inFollowLeash || !inPlayerLeash){
            if(pickedFollowDirection == FollowDirection.None){
                pickedFollowDirection = (FollowDirection)(UnityEngine.Random.Range(1, 6));
                followPosition = GetFollowPosition();
            }

            unit.SetMoveDirection(toFollowPosition.normalized);

            // Always clear fidgeting while moving
            fidgetStarted = false;
            fidgetHappening = false;
        } else {
            pickedFollowDirection = FollowDirection.None;
            unit.SetMoveDirection(Vector3.zero);

            if(!fidgetStarted){
                fidgetStarted = true;

                fidgetWaitTimer.SetDuration(UnityEngine.Random.Range(MIN_FIDGET_TIME, MAX_FIDGET_TIME));
                fidgetWaitTimer.Start();
            }
        }

        if(fidgetStarted && !fidgetHappening && fidgetWaitTimer.Finished()){
            fidgetHappening = true;
            fidgetDurationTimer.Start();

            fidgetDirection = new Vector3(
                UnityEngine.Random.Range(-1.0f, 1.0f),
                0.0f,
                UnityEngine.Random.Range(-1.0f, 1.0f)
            );
        } else if(fidgetHappening){
            unit.SetMoveDirection(fidgetDirection);

            if(fidgetDurationTimer.Finished()){
                fidgetStarted = false;
                fidgetHappening = false;
                unit.SetMoveDirection(Vector3.zero);
            }
        }
    }

    private Vector3 GetFollowPosition(){
        Transform playerTransform = PlayerComponent.player.GetCurrentPlayerUnit().rootTransform;

        if(pickedFollowDirection == FollowDirection.Back){
            return playerTransform.position + (playerTransform.forward * -PLAYER_KEEP_DISTANCE);
        } else if(pickedFollowDirection == FollowDirection.BackRight){
            Vector3 backRight = (-playerTransform.forward + playerTransform.right).normalized * PLAYER_KEEP_DISTANCE;
            return playerTransform.position + backRight;
        } else if(pickedFollowDirection == FollowDirection.BackLeft){
            Vector3 backLeft = (-playerTransform.forward + -playerTransform.right).normalized * PLAYER_KEEP_DISTANCE;
            return playerTransform.position + backLeft;
        } else if(pickedFollowDirection == FollowDirection.Right){
            return playerTransform.position + (playerTransform.right * PLAYER_KEEP_DISTANCE);
        } else if(pickedFollowDirection == FollowDirection.Left){
            return playerTransform.position + (-playerTransform.right * PLAYER_KEEP_DISTANCE);
        }

        return playerTransform.position;
    }
}