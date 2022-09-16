using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

class PlayerUnitAIComponent : MonoBehaviour {

    private const int ENVIRONMENT_LAYER_MASK = 1 << 3;

    private const float FOLLOW_LEASH_DISTANCE = 1.0f;
    private const float PLAYER_LEASH_DISTANCE = 4.0f;
    private const float PLAYER_KEEP_DISTANCE = 3.0f;

    private const float MIN_FIDGET_TIME = 1.5f;
    private const float MAX_FIDGET_TIME = 6.8f;
    private const float FIDGET_DURATION = 0.08f;

    private const float CAMERA_SENSITIVITY = 5.0f;

    public DetectorComponent detector;

    public Transform cameraRigTransform;
    public Transform cameraTargetTransform;

    public enum Mode {
        Follow,
        Combat,
        Selected,
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

    private Timer followResetTimer = new Timer();

    // Fidgeting
    private bool fidgetStarted;
    private bool fidgetHappening;

    private Vector3 fidgetDirection;

    private Timer fidgetWaitTimer = new Timer();
    private Timer fidgetDurationTimer = new Timer(FIDGET_DURATION);

    // Camera
    private Vector3 cameraDirection;
    private float cameraDistance;
    private Vector3 cameraVelocity;

    void Start(){
        unit = GetComponent<UnitComponent>();
        followPosition = new Vector3(0.0f, 9999999.0f, 0.0f);

        cameraDirection = cameraTargetTransform.position - cameraRigTransform.position;
        cameraDistance = cameraDirection.magnitude;
        cameraDirection.Normalize();
    }

    void Update(){
        if(mode == Mode.Follow){
            UpdateFollowing();
        } else if(mode == Mode.Combat){
            // TODO
        } else if(mode == Mode.Selected){
            UpdateCamera();
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

        if(!inFollowLeash || !inPlayerLeash){
            if(pickedFollowDirection == FollowDirection.None){
                pickedFollowDirection = (FollowDirection)(UnityEngine.Random.Range(1, 6));
                followPosition = GetFollowPosition();

                float timeToFollowPosition = (followPosition - transform.position).magnitude / unit.moveSpeed;
                followResetTimer.SetDuration(timeToFollowPosition + 2.0f); // Plus a little buffer
                followResetTimer.Start();
            }

            unit.SetInputDirection(toFollowPosition.normalized);

            if(followResetTimer.Finished()){
                // TODO rework to line of sight, and if no line of sight switch to different follow mode to path incrementally
                // Debug.Log("RESET");
                pickedFollowDirection = FollowDirection.None;
                // WaypointComponent.PathfindBetweenPositions(transform.position, new Vector3(72.9f, 1.0f, -6.2f));
            }

            // Always clear fidgeting while moving
            fidgetStarted = false;
            fidgetHappening = false;
        } else {
            pickedFollowDirection = FollowDirection.None;
            unit.SetInputDirection(Vector3.zero);

            if(!fidgetStarted){
                fidgetStarted = true;

                fidgetWaitTimer.SetDuration(UnityEngine.Random.Range(MIN_FIDGET_TIME, MAX_FIDGET_TIME));
                fidgetWaitTimer.Start();
            }
        }

        if(fidgetStarted && !fidgetHappening && fidgetWaitTimer.Finished()){
            // Later, pick between movement fidgets and playing a fidget animation

            fidgetHappening = true;
            fidgetDurationTimer.Start();

            fidgetDirection = new Vector3(
                UnityEngine.Random.Range(-1.0f, 1.0f),
                0.0f,
                UnityEngine.Random.Range(-1.0f, 1.0f)
            );
        } else if(fidgetHappening){
            unit.SetInputDirection(fidgetDirection);

            if(fidgetDurationTimer.Finished()){
                fidgetStarted = false;
                fidgetHappening = false;
                unit.SetInputDirection(Vector3.zero);
            }
        }
    }

    private void UpdateCamera(){
        // Move camera rig around using velocity/ Fuck the pitch axis?
        Vector3 currentEulerAngles = cameraRigTransform.rotation.eulerAngles;
        currentEulerAngles.y += cameraVelocity.y * CAMERA_SENSITIVITY;
        cameraRigTransform.rotation = Quaternion.Euler(currentEulerAngles);

        // Then, raycast backward towards the camera and move the position forward based on the
        // distance returned from that

        RaycastHit hit;
        Vector3 localCameraDirection = cameraRigTransform.TransformDirection(cameraDirection);
        if(Physics.Raycast(cameraRigTransform.position, localCameraDirection, out hit, cameraDistance, ENVIRONMENT_LAYER_MASK, QueryTriggerInteraction.Ignore)){
            cameraTargetTransform.position = hit.point - (localCameraDirection * 0.1f); // Move a little back towards; prevents camera clipping sometimes
        } else {
            cameraTargetTransform.position = cameraRigTransform.position + (localCameraDirection * cameraDistance);
        }
    }

    //##############################################################################################
    // Getters and Setters
    //##############################################################################################

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

    public void SetSelected(){
        mode = Mode.Selected;

        // Snap camera the direction root is facing here, making it so camera always start behind
        // the character. Also; lookup the unit if necessary (if this gets called during start)
        if(unit == null){
            unit = GetComponent<UnitComponent>();
        }

        cameraRigTransform.rotation = unit.rootTransform.rotation;

        // Might have to do a quick raycast here too? So that player update's UpdateCamera can more
        // accurately raycast while this unit pokes it's camera into a wall while Follow and resetting

        // We'll see if this is actually needed
    }

    public void SetUnselected(){
        mode = Mode.Follow;
        cameraVelocity = Vector3.zero;
    }

    public void SetCameraVelocity(Vector3 cameraVelocity_){
        cameraVelocity = cameraVelocity_;
    }
}