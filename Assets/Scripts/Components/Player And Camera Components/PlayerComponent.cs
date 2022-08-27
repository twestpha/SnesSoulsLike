using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

class PlayerComponent : MonoBehaviour {

    public static PlayerComponent player;

    public UnitComponent[] units;
    public Camera[] cameras;

    private int index;

    void Start(){
        player = this;

        // For now
        cameras[index].gameObject.SetActive(true);
    }

    void Update(){
        if(Input.GetKeyDown(KeyCode.E)){
            units[index].SetMoveDirection(Vector3.zero);
            units[index].GetComponent<PlayerUnitAIComponent>().enabled = true;
            cameras[index].gameObject.SetActive(false);

            index = (index + units.Length + 1) % units.Length;

            cameras[index].gameObject.SetActive(true);
            units[index].GetComponent<PlayerUnitAIComponent>().enabled = false;
        }

        if(Input.GetKeyDown(KeyCode.Q)){
            units[index].SetMoveDirection(Vector3.zero);
            units[index].GetComponent<PlayerUnitAIComponent>().enabled = true;
            cameras[index].gameObject.SetActive(false);

            index = (index + units.Length - 1) % units.Length;

            cameras[index].gameObject.SetActive(true);
            units[index].GetComponent<PlayerUnitAIComponent>().enabled = false;
        }

        // Player Input
        Vector3 inputDirection = Vector3.zero;

        if(Input.GetKey(KeyCode.W)){
            inputDirection += cameras[index].transform.forward;
        }
        if(Input.GetKey(KeyCode.A)){
            inputDirection -= cameras[index].transform.right;
        }
        if(Input.GetKey(KeyCode.S)){
            inputDirection -= cameras[index].transform.forward;
        }
        if(Input.GetKey(KeyCode.D)){
            inputDirection += cameras[index].transform.right;
        }

        units[index].SetMoveDirection(inputDirection);

        if(Input.GetKeyDown(KeyCode.Alpha1)){
            units[index].UseAbility(0);
        }
    }

    public UnitComponent GetCurrentPlayerUnit(){
        return units[index];
    }
}