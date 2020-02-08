#region Using Statements

using UnityEngine;

#endregion

public class FruitSpawner : MonoBehaviour
{
	[Tooltip("The time between spawning a fruit.")]
	[SerializeField]
	private float spawnRate = 3f;

	[Tooltip("The maximum number of fruit that can exist at once.")]
	[SerializeField]
	private int maxFruit = 7;

	[Tooltip("The fruit prefab to be spawned.")]
	[SerializeField]
	private GameObject fruitPrefab = null;

	private bool spawning;
	private float timer;

	private int maxX;
	private int maxY;

	#region Properties

	public bool Spawning
	{
		get { return spawning; }
		set { spawning = value; }
	}

	#endregion

	#region MonoBehaviour CallBacks

	// Start is called before the first frame update
	void Start()
    {
		timer = spawnRate;
		maxX = (int)GetComponent<BoxCollider>().bounds.max.x;
		maxY = (int)GetComponent<BoxCollider>().bounds.max.y;

		spawning = false;
		SpawnFruit();
	}

    // Update is called once per frame
    void Update()
    {
		if (!GameManager.Instance.IsPaused && spawning)
		{
			timer -= Time.deltaTime;

			if ((timer <= 0f) && (GameManager.Instance.FruitCount < maxFruit))
			{
				SpawnFruit();
			}
		}
    }

	#endregion

	#region Private Methods

	private void SpawnFruit()
	{
		int newX = Random.Range(-maxX, maxX + 1);
		int newY = Random.Range(-maxY, maxY + 1);
		Vector3 spawnPos = new Vector3(newX, newY, 0f);

		if (!GameManager.Instance.IsLocationOccupied(spawnPos))
		{
			GameObject fruitGO = Instantiate(fruitPrefab, spawnPos, fruitPrefab.transform.rotation, transform.parent.Find("Fruit")) as GameObject;
			Fruit fruit = fruitGO.GetComponent<Fruit>();
			GameManager.Instance.AddFruit(fruit);

			timer = spawnRate;
		}
		else
		{
			SpawnFruit();
		}

	}

	#endregion
}
