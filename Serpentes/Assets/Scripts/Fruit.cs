using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fruit : MonoBehaviour
{
	[Tooltip("How many points this fruit is worth.")]
	[SerializeField]
	private bool addSegment = true;

	[Tooltip("How many points this fruit is worth.")]
	[SerializeField]
	private int points = 10;

	[Tooltip("How quickly the fruit rotates.")]
	[SerializeField]
	private float rotationSpeed = 50f;

	private Vector2 location = Vector2.zero;

	public bool AddSegment { get { return addSegment; } }
	public int Points { get { return points; } }
	public Vector2 Location { get { return location; } }

	private void Update()
	{
		transform.Rotate(Vector3.forward, Time.deltaTime * rotationSpeed);
	}
}
