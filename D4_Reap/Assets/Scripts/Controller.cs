using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : RaycastController {

	private Animator anim;

	private Sprite attackSprite;

	public Vector3 playerScale;

	//  Magic numbers for how steep climbable slopes are
	public float maxClimbAngle = 80;
	public float maxDescendAngle = 75;

	public float hitDelay = 0.5f;
	float nextHitAllowed = 0f;

	//  Public collisions struct
	public CollisionInfo collisions;

	// Reference to the LevelManager
	public LevelManager manager;

	// Determines if Controller is controlling the player or an enemy
	bool isPlayer = false;

	//  Raw player input, set in the Move function
	[HideInInspector]
	public Vector2 playerInput;

	// ============================================= Collision Info =================================================

	/*
	 * A struct for keeping track of whether or not there is a collide-able object in any direction by using 
	 * ray tracing.  Also contains a function to reset things.
	 */

	public struct CollisionInfo {
		public bool above, below, left, right, climbingSlope, descendingSlope;
		public float slopeAngle, slopeAngleOld;
		public Vector3 velocityOld;

		//  1 means facing right, -1 means facing left
		public int faceDir;

		public void Reset() {
			above = below = left = right = climbingSlope = descendingSlope = false;
			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	}

	//  Overrides the start function in order to initialize our face direction, otherwise uses the inherited start function

	public override void Start() {
		base.Start ();
		collisions.faceDir = 1;
		anim = GetComponent<Animator> ();
		playerScale = transform.localScale;
	}

	public void playerInit(int i)
	{
		print (i == 8);
		isPlayer = (i == 8);
	}

	// ============================================== Attack ==================================================

	/*
	 * Call this function when we make an attack. Probably going to need to be 2 separate functions for each type
	 * of attack in the future.
	 */

	public void Attack(int id)
	{
		if (id == 0)
		{
			anim.SetTrigger ("Attack");
		}
		else
		{
			anim.SetTrigger ("Projectile");
		}
	}

	// ============================================= Movement =================================================

	/*
	 * Takes in a vector for velocity (provided by the Player script) and translates the Player accordingly.  Also
	 * takes in the player's raw input from the controller as passed by the Player script.
	 * 
	 * Checks for collisions and other things.
	 */
		
	public void Move(Vector3 velocity, Vector2 input) {
		UpdateRaycastOrigins();
		collisions.Reset ();
		collisions.velocityOld = velocity;
		playerInput = input;

		if (velocity.x != 0) {
			collisions.faceDir = (int) Mathf.Sign(velocity.x);
		}

		if (velocity.y < 0) {
			DescendSlope (ref velocity);
		}

		if (velocity.x != 0) {
			HorizontalCollisions (ref velocity);
		}
		if (velocity.y != 0) {
			VerticalCollisions (ref velocity);
		}

		transform.Translate (velocity);
		transform.localScale =  new Vector3(playerScale.x * collisions.faceDir, playerScale.y, playerScale.z);
	}

	public void Hurt() {
		if (Time.time > nextHitAllowed) {
			manager.hp--;
			nextHitAllowed = Time.time + hitDelay;
			print ("HP:  " + manager.hp);
		}
	}

	// ============================================= Horizontal Collision =================================================

	/*
	 * This function is called whenever the player is moving horizontally to use raytracing to check for horizontal collisions
	 * in a robust fashion.  Also includes some specialized code for if you're dealing with a slope.
	 */

	void HorizontalCollisions(ref Vector3 velocity) {
		float directionX = collisions.faceDir;
		float rayLength = Mathf.Abs (velocity.x) + skinWidth;

		//  For all rays
		for (int i = 0; i < horizontalRayCount; i++) {

			//  Cast the rays
			Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.botL : raycastOrigins.botR;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

			RaycastHit2D enemy = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, enemyMask);

			Debug.DrawRay (rayOrigin, Vector2.right * directionX * rayLength, Color.blue);

			//  What happens if it's a hit?
			if (hit) {

				//  Determine the slope angle 
				float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);


				if ((i == 0) && slopeAngle <= maxClimbAngle) {

					//  Handles moving from a decending slope to ascending a slope
					if (collisions.descendingSlope) {
						collisions.descendingSlope = false;
						velocity = collisions.velocityOld;
					}

					float distanceToSlopeStart = 0;
					if (slopeAngle != collisions.slopeAngleOld) {
						distanceToSlopeStart = hit.distance - skinWidth;
						velocity.x -= distanceToSlopeStart * directionX;
					}
					ClimbSlope (ref velocity, slopeAngle);
					velocity.x += distanceToSlopeStart * directionX;
				}

				//  What happens if it's not a slope and instead a wall
				if (!collisions.climbingSlope || slopeAngle > maxClimbAngle) {
					velocity.x = (hit.distance - skinWidth) * directionX;
					rayLength = hit.distance;

					if (collisions.climbingSlope) {
						velocity.y = Mathf.Tan (collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs (velocity.x);
					}

					collisions.left = directionX == -1;
					collisions.right = directionX == 1;
				}
			}

			//  What happens if player hits an enemy?
			if (enemy) {
				//print ("I'm a " +  + " and I just got hit!");

				//velocity.x = (hit.distance - skinWidth) * directionX + (directionX * -4);
				//transform.Translate (velocity);

				rayLength = hit.distance;

				//  Set the booleans to determine if we've collided
				collisions.left = directionX == -1;
				collisions.right = directionX == 1;
				Hurt ();
			}

		}
	}

	// ============================================= Vertical Collision =================================================

	/*
	 * This function is called whenever the player is moving vertically to use raytracing to check for vertical collisions
	 * in a robust fashion.  Also includes some specialized code for if you're dealing with a slope.
	 */

	void VerticalCollisions(ref Vector3 velocity) {
		float directionY = Mathf.Sign (velocity.y);
		float rayLength = Mathf.Abs (velocity.y) + skinWidth;

		for (int i = 0; i < verticalRayCount; i++) {
			Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.botL : raycastOrigins.topL;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);

			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
			RaycastHit2D enemy = Physics2D.Raycast (rayOrigin, Vector2.right * directionY, rayLength, enemyMask);

			Debug.DrawRay (rayOrigin, Vector2.up * directionY * rayLength, Color.red);

			if (hit) {
				velocity.y = (hit.distance - skinWidth) * directionY;
				rayLength = hit.distance;

				if (collisions.climbingSlope) {
					velocity.x = velocity.y / Mathf.Tan (collisions.slopeAngle * Mathf.Deg2Rad) * collisions.faceDir;
				}

				collisions.below = directionY == -1;
				collisions.above = directionY == 1;
			}

			//  What happens if it hits an enemy?
			/*
			if (enemy) {
				Hurt ();
			}
			*/
		}

		if (collisions.climbingSlope) {
			float directionX = collisions.faceDir;
			rayLength = Mathf.Abs (velocity.x) + skinWidth;
			Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.botL : raycastOrigins.botR) + Vector2.up * velocity.y;
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

			if (hit) {
				float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);
				if (slopeAngle != collisions.slopeAngle) {
					velocity.x = (hit.distance - skinWidth) * directionX;
					collisions.slopeAngle = slopeAngle;
				}
			}
		}
	}

	// ============================================= Slope Functions =================================================

	/*
	 * If we're being perfectly honest I don't entirely understand all of the math behind this as I wrote it late last night,
	 * but it works and it does improve the movement significantly.
	 */

	//  Smothing and collision detection for ascending slopes.
	void ClimbSlope(ref Vector3 velocity, float slopeAngle) {
		float moveDistance = Mathf.Abs (velocity.x);
		float climbVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;

		if (velocity.y <= climbVelocityY) {
			velocity.y = climbVelocityY;
			velocity.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * collisions.faceDir;
			collisions.below = true;
			collisions.climbingSlope = true;
			collisions.slopeAngle = slopeAngle;
		}
	}

	//  Smoothing and collision detection for downward slopes.
	void DescendSlope(ref Vector3 velocity) {
		float directionX = collisions.faceDir;
		Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.botR : raycastOrigins.botL;
		RaycastHit2D hit = Physics2D.Raycast (rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

		if (hit) {
			float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);
			if ((slopeAngle != 0 && slopeAngle <= maxDescendAngle) && (Mathf.Sign (hit.normal.x) == directionX) && (hit.distance - skinWidth <= Mathf.Tan (slopeAngle * Mathf.Deg2Rad) * Mathf.Abs (velocity.x))) {
				float moveDistance = Mathf.Abs (velocity.x);
				float descendVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;
				velocity.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * collisions.faceDir;
				velocity.y -= descendVelocityY;

				collisions.slopeAngle = slopeAngle;
				collisions.descendingSlope = true;
				collisions.below = true;
			}
		}
	}



}
