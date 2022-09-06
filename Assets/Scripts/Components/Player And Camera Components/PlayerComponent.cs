using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

class PlayerComponent : MonoBehaviour {

    private const float CAMERA_MOVE_TIME = 0.75f;

    public static PlayerComponent player;

    public UnitComponent[] units;
    public Transform[] cameraTargetTransforms;

    public Transform cameraTransform;

    private int index;

    private Timer cameraMoveTimer = new Timer(CAMERA_MOVE_TIME);

    private Vector3 previousCameraPosition;
    private Vector3 previousCameraForward;

    void Start(){
        player = this;
    }

    void Update(){
        if(Input.GetKeyDown(KeyCode.E)){
            units[index].SetInputDirection(Vector3.zero);
            units[index].GetComponent<PlayerUnitAIComponent>().enabled = true;

            index = (index + units.Length + 1) % units.Length;

            units[index].GetComponent<PlayerUnitAIComponent>().enabled = false;

            cameraMoveTimer.Start();

            previousCameraPosition = cameraTransform.position;
            previousCameraForward = cameraTransform.forward;
        }

        if(Input.GetKeyDown(KeyCode.Q)){
            units[index].SetInputDirection(Vector3.zero);
            units[index].GetComponent<PlayerUnitAIComponent>().enabled = true;

            index = (index + units.Length - 1) % units.Length;

            units[index].GetComponent<PlayerUnitAIComponent>().enabled = false;

            cameraMoveTimer.Start();

            previousCameraPosition = cameraTransform.position;
            previousCameraForward = cameraTransform.forward;
        }

        // Camera movement
        float cameraT = CustomMath.EaseInOut(cameraMoveTimer.Parameterized());

        Vector3 cameraTargetPosition = cameraTargetTransforms[index].position;
        Vector3 cameraTargetOrientation = cameraTargetTransforms[index].forward;

        cameraTransform.position = Vector3.Lerp(previousCameraPosition, cameraTargetPosition, cameraT);
        cameraTransform.rotation = Quaternion.LookRotation(Vector3.Lerp(previousCameraForward, cameraTargetOrientation, cameraT));

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

        units[index].SetInputDirection(inputDirection);

        if(Input.GetKeyDown(KeyCode.Space)){
            units[index].UseAbility(0);
        }

        if(Input.GetKeyDown(KeyCode.Mouse0)){
            units[index].UseAbility(1);
        }
    }

    public UnitComponent GetCurrentPlayerUnit(){
        return units[index];
    }
}