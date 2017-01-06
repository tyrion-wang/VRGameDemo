using UnityEngine;
using System.Collections;

public class move : MonoBehaviour {

	// Use this for initialization
	private Vector3 offset = new Vector3(0,0,0);
	private float speed = 0;
	private float speed_front = 0;
	private float speed_back = 0;
	public float maxSpeed = 2.0f;
	public float maxBackSpeed = 0.5f;
	
	public float acceleration = 0.1f;
	public float brake = 0.3f;
	private bool isMove = false;
	private bool isBrake = false;
	private bool isBack = false;
	public void TestName()
	{
	//Given
	
	//When
	
	//Then
	}
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
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
			speed_front += acceleration;
			if(speed_front >= maxSpeed){
				speed_front = maxSpeed;
			}
		}else{
			speed_front -= acceleration;
			if(speed_front <= 0.0f){
				speed_front = 0.0f;
			}
		}

		if(isBack){
			speed_back += acceleration;
			if(speed_back <= maxBackSpeed){
				speed_back = maxBackSpeed;
			}
		}else{
			speed_back -= acceleration;
			if(speed_back <= 0.0f){
				speed_back = 0.0f;
			}
		}

		if(isBrake){
			speed_front -= brake;
			if(speed_front <= 0.0f){
				speed_front = 0.0f;
			}

			speed_back -= brake;
			if(speed_back <= 0.0f){
				speed_back = 0.0f;
			}
		}

		speed = speed_front - speed_back;

		
		offset.z = speed;

		transform.position = transform.position + offset;
	}
}
