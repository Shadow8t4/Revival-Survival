using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (BoxCollider2D))]
public class RaycastController : MonoBehaviour {

	//  collisionMask is for terrain
	public LayerMask collisionMask;
	//  enemyMask is for enemies
	public LayerMask enemyMask;

	public const float skinWidth = 0.015f;
	public int horizontalRayCount = 4;
	public int verticalRayCount = 4;

	[HideInInspector]
	public float horizontalRaySpacing;
	[HideInInspector]
	public float verticalRaySpacing;

	[HideInInspector]
	public BoxCollider2D collider;
	public RaycastOrigins raycastOrigins;

	public struct RaycastOrigins {
		public Vector2 topL, topR, botL, botR;
	}

	public virtual void Awake () {
		collider = GetComponent<BoxCollider2D> ();
	}

	public virtual void Start() {
		CalculateRaySpacing ();
	}

	public void UpdateRaycastOrigins () {
		Bounds bounds = collider.bounds;
		bounds.Expand (skinWidth * -2);

		raycastOrigins.botL = new Vector2 (bounds.min.x, bounds.min.y);
		raycastOrigins.botR = new Vector2 (bounds.max.x, bounds.min.y);
		raycastOrigins.topL = new Vector2 (bounds.min.x, bounds.max.y);
		raycastOrigins.topR = new Vector2 (bounds.max.x, bounds.max.y);
	}

	public void CalculateRaySpacing () {
		//  Getting the bounds again
		Bounds bounds = collider.bounds;
		bounds.Expand (skinWidth * -2);

		horizontalRayCount = Mathf.Clamp (horizontalRayCount, 2, int.MaxValue);
		verticalRayCount = Mathf.Clamp (verticalRayCount, 2, int.MaxValue);

		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
	}
}
