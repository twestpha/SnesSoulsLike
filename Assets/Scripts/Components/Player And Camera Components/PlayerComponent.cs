using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

class PlayerComponent : MonoBehaviour {

    private const float CAMERA_MOVE_TIME = 0.75f;

    public static PlayerComponent player;

    public UnitComponent[] units;
    public Transform[] cameraTargetTransforms;

    private UnitComponent currentUnit;
    private PlayerUnitAIComponent currentPlayerAI;

    public Transform cameraTransform;

    private int index;

    private Timer cameraMoveTimer = new Timer(CAMERA_MOVE_TIME);

    private Vector3 previousCameraPosition;
    private Quaternion previousCameraRotation;

    void Awake(){
        player = this;
    }

    void Start(){
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentUnit = units[0];
        currentPlayerAI = currentUnit.GetComponent<PlayerUnitAIComponent>();

        currentPlayerAI.SetSelected();

        // Snap camera instantly on frame 1
        previousCameraPosition = cameraTargetTransforms[0].position;
        previousCameraRotation = cameraTargetTransforms[0].rotation;
    }

    void Update(){
        UpdatePlayerInputs();
        UpdateCamera();
    }

    private void UpdateCamera(){
        // TODO if can't draw a environment raycast between them, fade in/out instead, that'll feel much more polished in the future

        // Camera movement
        float cameraT = CustomMath.EaseInOut(cameraMoveTimer.Parameterized());

        Vector3 cameraTargetPosition = cameraTargetTransforms[index].position;
        Quaternion cameraTargetRotation = cameraTargetTransforms[index].rotation;

        cameraTransform.position = Vector3.Lerp(previousCameraPosition, cameraTargetPosition, cameraT);
        cameraTransform.rotation = Quaternion.Slerp(previousCameraRotation, cameraTargetRotation, cameraT);

        // Camera input to pass to current player
        Vector3 tempMouseInput = new Vector3(
            Input.GetAxis("Mouse Y"),
            Input.GetAxis("Mouse X"),
            0.0f
        );

        currentPlayerAI.SetCameraVelocity(tempMouseInput);
    }

    private void UpdatePlayerInputs(){
        if(Input.GetKeyDown(KeyCode.E)){
            ChangeCurrentPlayer(1);
        }

        if(Input.GetKeyDown(KeyCode.Q)){
            ChangeCurrentPlayer(-1);
        }

        // Player Input
        Vector3 inputDirection = Vector3.zero;

        if(Input.GetKey(KeyCode.W)){
            inputDirection += cameraTargetTransforms[index].transform.forward;
        }
        if(Input.GetKey(KeyCode.A)){
            inputDirection -= cameraTargetTransforms[index].transform.right;
        }
        if(Input.GetKey(KeyCode.S)){
            inputDirection -= cameraTargetTransforms[index].transform.forward;
        }
        if(Input.GetKey(KeyCode.D)){
            inputDirection += cameraTargetTransforms[index].transform.right;
        }

        currentUnit.SetInputDirection(inputDirection);

        if(Input.GetKeyDown(KeyCode.Space)){
            currentUnit.UseAbility(0);
        }

        if(Input.GetKeyDown(KeyCode.Mouse0)){
            currentUnit.UseAbility(1);
        }
    }

    private void ChangeCurrentPlayer(int direction){

        currentUnit.SetInputDirection(Vector3.zero);
        currentPlayerAI.SetUnselected();

        index = (index + units.Length + direction) % units.Length;

        currentUnit = units[index];
        currentPlayerAI = currentUnit.GetComponent<PlayerUnitAIComponent>();
        currentPlayerAI.SetSelected();

        cameraMoveTimer.Start();

        previousCameraPosition = cameraTransform.position;
        previousCameraRotation = cameraTransform.rotation;
    }

    public UnitComponent GetCurrentPlayerUnit(){
        return currentUnit;
    }
}