﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AxleInfo {
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor;
    public bool steering;
}
    
public class SimpleCarController : MonoBehaviour {
    public List<AxleInfo> axleInfos; 
    public float maxMotorTorque;
    public float maxBrakeTorque = 1000.0f;
    public float maxSteeringAngle;
    private bool isMove = false;
    private float motor = 0.0f;
    public float acceleration = 1.0f;
    private bool isBrake = false;
    private bool isBack = false;
    void Start()
    {
        // 车辆之心提前，靠近发动机
        transform.GetComponent<Rigidbody>().centerOfMass += new Vector3(0, -0.3f, 0.8f);

        AxleInfo frontAxle = new AxleInfo();
        frontAxle.leftWheel = transform.FindChild("wheels").FindChild("frontLeft").GetComponent<WheelCollider>();
        frontAxle.rightWheel = transform.FindChild("wheels").FindChild("frontRight").GetComponent<WheelCollider>();
        frontAxle.motor = false;
        frontAxle.steering = true;

        AxleInfo rearAxle = new AxleInfo();
        rearAxle.leftWheel = transform.FindChild("wheels").FindChild("rearLeft").GetComponent<WheelCollider>();
        rearAxle.rightWheel = transform.FindChild("wheels").FindChild("rearRight").GetComponent<WheelCollider>();
        rearAxle.motor = true;
        rearAxle.steering = false;

        axleInfos.Add(frontAxle);
        axleInfos.Add(rearAxle);
    }

    // finds the corresponding visual wheel
    // correctly applies the transform
    public void ApplyLocalPositionToVisuals(WheelCollider collider)
    {
        if (collider.transform.childCount == 0) {
            return;
        }
    
        Transform visualWheel = collider.transform.GetChild(0);
    
        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);
    
        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation;
    }
    
    public void FixedUpdate()
    {
        if(Input.GetButtonDown(InputUtil.buttonA)){
			isMove = true;
		}

		if(Input.GetButtonUp(InputUtil.buttonA)){
			isMove = false;
		}

        if(Input.GetButtonDown(InputUtil.buttonX)){
			isBrake = true;
		}

		if(Input.GetButtonUp(InputUtil.buttonX)){
			isBrake = false;
		}

        if(Input.GetButtonDown(InputUtil.buttonY)){
			isBack = true;
		}

		if(Input.GetButtonUp(InputUtil.buttonY)){
			isBack = false;
		}

        if(isMove){
			motor += acceleration;
			if(motor >= maxMotorTorque){
				motor = maxMotorTorque;
			}
		}else if(isBack){
            motor -= acceleration;
			if(motor <= -maxMotorTorque){
				motor = -maxMotorTorque;
			}
        }else{
			motor = 0.0f;
		}

        // float motor = maxMotorTorque * Input.GetAxis("Vertical");
        float steering = maxSteeringAngle * Input.GetAxis("Horizontal");

        foreach (AxleInfo axleInfo in axleInfos) {
            if (axleInfo.steering) {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            if (axleInfo.motor) {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }

            if(isBrake){
                axleInfo.leftWheel.brakeTorque = maxBrakeTorque;
                axleInfo.rightWheel.brakeTorque = maxBrakeTorque;
            }else{
                axleInfo.leftWheel.brakeTorque = 0.0f;
                axleInfo.rightWheel.brakeTorque = 0.0f;
            }
            
            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
        }
    }
}