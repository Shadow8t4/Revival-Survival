using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Controller))]
public class Player : MonoBehaviour {

	public Animator anim;

	//  Values used to calculate jump velocity and gravity
	public float jumpHeight = 4;
	public float timeToJumpApex = .4f;
	//  Horizontal move speed
	public float moveSpeed = 6;

	//  Used for jump time
	[HideInInspector]
	public float accelerationTimeAirborne = .1f;
	[HideInInspector]
	public float accelerationTimeGrounded = .05f;

	//  Gravity and jumpVelocity are derived from provided values
	float gravity;
	float jumpVelocity;

	//  Player velocity at any given moment
	Vector3 velocity;

	//  Used to give a sense of momentum to the player
	float velocityXSmoothing;

	Controller controller;

	// Use this for initialization
	void Start () {
		controller = GetComponent<Controller>();
		controller.playerInit (gameObject.layer);

		gravity = -(2 * jumpHeight) / (Mathf.Pow(timeToJumpApex,2));
		jumpVelocity = Mathf.Abs(gravity * timeToJumpApex);

		print ("Gravity: " + gravity + " Jump Velocity: " + jumpVelocity);
		anim = GetComponent<Animator> ();
		
	}
	
	// Update is called once per frame
	void Update () {

		if (controller.collisions.above || controller.collisions.below) {
			velocity.y = 0;
		}

		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));

		if (Input.GetButtonDown ("Jump") && controller.collisions.below) {
			velocity.y = jumpVelocity;
		}

		if(Input.GetButtonDown("Fire1"))
		{
			controller.Attack (0);
		}

		if(Input.GetButtonDown("Fire2"))
		{
			controller.Attack (1);
		}

		float targetVelocityX = input.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below)?accelerationTimeGrounded:accelerationTimeAirborne);

		velocity.y += gravity * Time.deltaTime;

		anim.SetFloat ("Moving", Mathf.Abs(velocity.x));
		anim.SetBool ("Jumping", !controller.collisions.below);

		controller.Move (velocity * Time.deltaTime, input);
	}
}
