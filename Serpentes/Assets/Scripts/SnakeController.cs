#region Using Statements

using System.Collections.Generic;
using UnityEngine;

#endregion

#region Enums

public enum Direction
{
	Start,
	Up,
	Down,
	Left,
	Right
}

#endregion

public class SnakeController : MonoBehaviour
{
	[Tooltip("The speed the snake will move.")]
	[SerializeField]
	private float startSpeed = 5f;

	[Tooltip("The speed the snake will rotate when turning directions.")]
	[SerializeField]
	private float rotationSpeed = 5f;

	[Tooltip("The prefab for segments to be added after eating fruit.")]
	[SerializeField]
	private GameObject segmentPrefab = null;

	private bool addOnArrival;
	private List<Segment> segments;

	private Direction currentDirection;
	private Vector2 currentPosition;
	private Vector2 destination;
	private float startTime;

	#region Properties

	public Direction CurrentDirection
	{
		get { return currentDirection; }
	}

	#endregion

	#region MonoBehaviour Callbacks

	// Start is called before the first frame update
	void Start()
    {
		transform.position = Vector3.zero;
		currentDirection = Direction.Start;
		currentPosition = transform.position;
		destination = transform.position;
		startTime = 0f;
		addOnArrival = false;

		segments = new List<Segment>();
    }

    // Update is called once per frame
    void Update()
    {
		if (!GameManager.Instance.IsPaused)
		{
			GetInput();

			if (currentDirection != Direction.Start)
			{
				AdjustDestination();
				Rotate();
				Move();
			}
		}
    }

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Fruit"))
		{
			EatFruit(other.gameObject);
		}
		else if (other.gameObject.CompareTag("Wall") || other.gameObject.CompareTag("Snake"))
		{
			if (segments.Count > 1 || other.gameObject.CompareTag("Wall"))
			{
				GameManager.Instance.GameOver();
			}
		}
	}

	#endregion

	#region Private Methods

	private void EatFruit(GameObject fruitGO)
	{
		Fruit triggerFruit = fruitGO.gameObject.GetComponent<Fruit>();

		if (triggerFruit.AddSegment)
		{
			addOnArrival = true;
		}

		GameManager.Instance.EatFruit(triggerFruit);
	}

	private void AddSegment()
	{
		Vector2 segmentPos = GetSegmentPosition();
		GameObject segmentGO = Instantiate(segmentPrefab, segmentPos, segmentPrefab.transform.rotation) as GameObject;
		Segment segment = segmentGO.GetComponent<Segment>();

		if (segments.Count == 0)
		{
			segment.Destination = currentPosition;
			segment.Direction = currentDirection;
		}
		else
		{
			segment.Destination = segments[segments.Count - 1].Position;
		}

		segment.Position = segmentPos;
		segment.RotationSpeed = rotationSpeed;
		segments.Add(segment);
		if (segments.Count >= 2)
		{
			segments[segments.Count - 1].Rotation = segments[segments.Count - 2].Rotation;
		}
		else if (segments.Count == 1)
		{
			segments[segments.Count - 1].Rotation = transform.rotation;
		}
	}

	private Vector2 GetSegmentPosition()
	{
		Vector2 tailPosition;

		if (segments.Count == 0)
		{
			tailPosition = currentPosition;
		}
		else
		{
			tailPosition = segments[segments.Count - 1].Position;
		}

		return tailPosition;
	}

	private void GetInput()
	{
		float horizontal = Input.GetAxis("Horizontal");
		float vertical = Input.GetAxis("Vertical");
		Direction startingDirection = currentDirection;

		if (horizontal > 0f)
		{
			currentDirection = Direction.Right;
		}
		else if (horizontal < 0f)
		{
			currentDirection = Direction.Left;
		}

		if (vertical > 0f)
		{
			currentDirection = Direction.Up;
		}
		else if (vertical < 0f)
		{
			currentDirection = Direction.Down;
		}

		if (startingDirection == Direction.Start && startingDirection != currentDirection)
		{
			GameObject.FindGameObjectWithTag("Spawner").GetComponent<FruitSpawner>().Spawning = true;
			GameManager.Instance.CloseInstructionsText();
		}
	}

	private void Move()
	{
		float distCovered = (Time.time - startTime) * (startSpeed + GameManager.Instance.Level);
		float journeyLength = Vector2.Distance(currentPosition, destination);
		float fractionOfJourney = distCovered / journeyLength;

		transform.position = Vector3.Lerp(currentPosition, destination, fractionOfJourney);

		foreach (Segment current in segments)
		{
			current.Move(fractionOfJourney);
		}

		if (Vector2.Distance(transform.position, destination) < 0.01f)
		{
			currentPosition = destination;
			if (addOnArrival)
			{
				addOnArrival = false;
				AddSegment();
			}
		}
	}

	private void Rotate()
	{
		switch(currentDirection)
		{
			case Direction.Up:
				transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(new Vector3(0f, 0f, 90f)), Time.deltaTime * rotationSpeed);
				break;

			case Direction.Down:
				transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(new Vector3(0f, 0f, -90f)), Time.deltaTime * rotationSpeed);
				break;

			case Direction.Right:
				transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(new Vector3(0f, 0f, 0f)), Time.deltaTime * rotationSpeed);
				break;

			case Direction.Left:
				transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(new Vector3(0f, 0f, 180f)), Time.deltaTime * rotationSpeed);
				break;
		}
	}

	private void AdjustDestination()
	{
		if (currentPosition == destination)
		{
			Vector2 newDest = destination;

			switch (currentDirection)
			{
				case Direction.Up:
					newDest = new Vector2(newDest.x, newDest.y + 1);
					break;

				case Direction.Down:
					newDest = new Vector2(newDest.x, newDest.y - 1);
					break;

				case Direction.Right:
					newDest = new Vector2(newDest.x + 1, newDest.y);
					break;

				case Direction.Left:
					newDest = new Vector2(newDest.x - 1, newDest.y);
					break;
			}

			Vector2 prevPos = currentPosition;
			Direction prevDirection = currentDirection;
			for (int i = 0; i < segments.Count; i++)
			{
				Direction tmp = segments[i].Direction;

				segments[i].Position = segments[i].Destination;
				segments[i].Destination = prevPos;
				segments[i].Direction = prevDirection;

				prevDirection = tmp;
				prevPos = segments[i].Position;
			}

			destination = newDest;
			startTime = Time.time;
		}
	}

	#endregion

	#region Public Methods

	public void Restart()
	{
		for (int i = 0; i < segments.Count; i++)
		{
			Destroy(segments[i].gameObject);
		}
		segments.Clear();
		transform.rotation = Quaternion.identity;
		Start();
	}

	#endregion
}
