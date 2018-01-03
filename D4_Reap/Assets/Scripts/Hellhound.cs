using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Controller))]
public class Hellhound : MonoBehaviour {

	//  Horizontal move speed
	public float moveSpeed = 6;
	[HideInInspector]
	public float accelerationTimeGrounded = .05f;

	//  AI behavior
	Vector2 input;

	//  Hellhound velocity at any given moment
	Vector3 velocity;

	//  Used to give a sense of momentum to the player
	float velocityXSmoothing;

	Controller controller;

	// Use this for initialization
	void Start () {
		controller = GetComponent<Controller>();
		input.x = 1;
		input.y = 0;
	}

	// Update is called once per frame
	void Update () {

		if (controller.collisions.above || controller.collisions.below) {
			velocity.y = 0;
		}

		if (controller.collisions.left) {
			input.x = 1;
		} else if (controller.collisions.right) {
			input.x = -1;
		}

		float targetVelocityX = input.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, accelerationTimeGrounded);
		velocity.y = 0;

		controller.Move (velocity * Time.deltaTime, input);
	}
}
