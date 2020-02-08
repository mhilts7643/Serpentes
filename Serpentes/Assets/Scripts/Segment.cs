#region Using Statements

using UnityEngine;

#endregion

public class Segment : MonoBehaviour
{
	private Direction direction;
	private Vector2 currentPosition;
	private Vector2 destination;
	private float startTime;
	private float rotSpeed;

	#region Properties

	public Direction Direction
	{
		get { return direction; }
		set { direction = value; }
	}

	public Vector2 Destination
	{
		get { return destination; }
		set { destination = value; }
	}

	public Vector2 Position
	{
		get { return currentPosition; }
		set { currentPosition = value; }
	}

	public Quaternion Rotation
	{
		get { return transform.rotation; }
		set { transform.rotation = value; }
	}

	public float RotationSpeed
	{
		set { rotSpeed = value; }
	}

	#endregion

	#region Public Methods

	public void Move(float fractionOfJourney)
	{
		Rotate();
		transform.position = Vector3.Lerp(currentPosition, destination, fractionOfJourney);
	}

	private void Rotate()
	{
		switch (direction)
		{
			case Direction.Up:
				transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(new Vector3(0f, 0f, 90f)), Time.deltaTime * rotSpeed);
				break;

			case Direction.Down:
				transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(new Vector3(0f, 0f, -90f)), Time.deltaTime * rotSpeed);
				break;

			case Direction.Right:
				transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(new Vector3(0f, 0f, 0f)), Time.deltaTime * rotSpeed);
				break;

			case Direction.Left:
				transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(new Vector3(0f, 0f, 180f)), Time.deltaTime * rotSpeed);
				break;
		}
	}

	#endregion
}
