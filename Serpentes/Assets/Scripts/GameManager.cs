#region Using Statements

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

#endregion

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;

	[Tooltip("The rotation speed for the camera.")]
	[SerializeField]
	private float camRotationSpeed = 100f;

	[Tooltip("The rotation speed for the camera.")]
	[SerializeField]
	private ParticleSystem wind = null;

	[Tooltip("The CanvasGroup component of the UICanvas GameObject.")]
	[SerializeField]
	private CanvasGroup canvasGroup = null;

	[Tooltip("The SnakeController component of the snake GameObject.")]
	[SerializeField]
	private SnakeController snake = null;

	[Tooltip("The FruitSpawner component of the Floor.")]
	[SerializeField]
	private FruitSpawner fruitSpawner = null;

	[Tooltip("The Start menu game object.")]
	[SerializeField]
	private GameObject startMenu = null;

	[Tooltip("The UI menu game object.")]
	[SerializeField]
	private GameObject uiMenu = null;

	[Tooltip("The pause menu game object.")]
	[SerializeField]
	private GameObject pauseMenu = null;

	[Tooltip("The results menu game object.")]
	[SerializeField]
	private GameObject resultsMenu = null;

	[Tooltip("The highscores menu game object.")]
	[SerializeField]
	private GameObject highscoresMenu = null;

	[Tooltip("The highscores menu game object.")]
	[SerializeField]
	private GameObject controlsMenu = null;

	[Tooltip("The options menu game object.")]
	[SerializeField]
	private GameObject optionsMenu = null;

	[Tooltip("The main camera transform.")]
	[SerializeField]
	private new Transform camera = null;

	[Tooltip("The text which displays the time.")]
	[SerializeField]
	private TextMeshProUGUI timeText = null;

	[Tooltip("The text which displays the score.")]
	[SerializeField]
	private TextMeshProUGUI scoreText = null;

	[Tooltip("The input box for high score name entry.")]
	[SerializeField]
	private TMP_InputField nameInput = null;

	[Tooltip("The the error text for name entry.")]
	[SerializeField]
	private GameObject nameEntryErrorText = null;

	[Tooltip("The the color selection transform in the options menu.")]
	[SerializeField]
	private Transform colorSelection = null;

	[Tooltip("The prefab for eating effect.")]
	[SerializeField]
	private GameObject eatEffectPrefab = null;

	public int Level
	{
		get { return score / 50; }
	}

	private int[] highScores;
	private string[] names;

	private List<Fruit> fruitList;
	private int score;
	private bool isPaused;
	private bool rotating;
	private bool gameOver;
	private GameObject activeMenu;
	private float timer;
	private int rank;

	private float startingAlpha;
	private float rotStartTime;
	private Vector3 rotationStart;
	private Vector3 rotationDest;

	private Color startColor;

	#region Properties

	public List<Fruit> FruitList
	{
		get { return fruitList; }
	}

	public int FruitCount
	{
		get { return fruitList.Count; }
	}

	public bool IsPaused
	{
		get { return isPaused; }
		set { isPaused = value; }
	}

	public GameObject ActiveMenu
	{
		get { return activeMenu; }
	}

	#endregion

	#region MonoBehaviour CallBacks

	// Start is called before the first frame update
	void Start()
    {
        if (Instance != null)
		{
			Debug.LogError("Duplicate GameManager detected. Destroying duplicate.");
			Destroy(this);
		}
		Instance = this;

		fruitList = new List<Fruit>();
		IsPaused = true;
		activeMenu = startMenu;
		timer = 0f;
		highScores = (highScores == null) ? GetHighScores() : highScores;
		names = (names == null) ? GetNames() : names;
		startColor = snake.Color;
	}

	// Update is called once per frame
	void Update()
	{
		if (!isPaused && snake.CurrentDirection != Direction.Start)
		{
			timer += Time.deltaTime;
			timeText.text = FormatTime(timer);
		}

		if (rotating)
		{
			RotateCamera();
		}
		else
		{
			if (wind.isPaused)
			{
				wind.Play();
			}

			if (!startMenu.activeSelf && !resultsMenu.activeSelf)
			{
				CheckInput();
			}
		}
	}

	private void OnApplicationQuit()
	{
		SaveHighScores();
	}

	#endregion

	#region Private Methods

	private int[] GetHighScores()
	{
		int[] retArray = new int[10];

		for (int i = 0; i < retArray.Length; i++)
		{
			if (PlayerPrefs.HasKey("score" + i.ToString()))
			{
				retArray[i] = PlayerPrefs.GetInt("score" + i.ToString());
			}
			else
			{
				PlayerPrefs.SetInt("score" + i.ToString(), 0);
				retArray[i] = PlayerPrefs.GetInt("score" + i.ToString());
			}
		}

		return retArray;
	}

	private string[] GetNames()
	{
		string[] retArray = new string[10];

		for (int i = 0; i < retArray.Length; i++)
		{
			if (PlayerPrefs.HasKey("name" + i.ToString()))
			{
				retArray[i] = PlayerPrefs.GetString("name" + i.ToString());
			}
			else
			{
				PlayerPrefs.SetInt("name" + i.ToString(), 0);
				retArray[i] = PlayerPrefs.GetString("name" + i.ToString());
			}
		}

		return retArray;
	}

	private void SaveHighScores()
	{
		for (int i = 0; i < highScores.Length; i++)
		{
			PlayerPrefs.SetInt("score" + i.ToString(), highScores[i]);
			PlayerPrefs.SetString("name" + i.ToString(), names[i]);
		}
	}

	private string FormatTime(float time)
	{
		TimeSpan t = TimeSpan.FromSeconds(time);
		return string.Format("{0,1:0}:{1,2:00}", t.Minutes, t.Seconds);
	}

	private void CheckInput()
	{
		if (gameOver && Input.anyKeyDown)
		{
			GameObject instructions = uiMenu.transform.Find("InstructionsText").gameObject;
			instructions.transform.GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text =
				"Move to begin the game.";
			uiMenu.SetActive(false);

			PostGameOver();
		}
		else if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
		{
			TogglePause();
		}
	}

	private void StartRotation()
	{
		rotating = true;
		rotationStart = camera.transform.eulerAngles;
		rotationDest = new Vector3(rotationStart.x, rotationStart.y + 180, rotationStart.z);
		rotStartTime = Time.time;
	}

	private void RotateCamera()
	{
		if (wind.isPlaying)
		{
			wind.Pause();
		}

		float distCovered = (Time.time - rotStartTime) * camRotationSpeed;
		float journeyLength = Vector3.Distance(rotationStart, rotationDest);
		float fractionOfJourney = distCovered / journeyLength;
		camera.transform.eulerAngles = Vector3.Lerp(rotationStart, rotationDest, fractionOfJourney);

		if (startingAlpha >= 1f)
		{
			FadeOut(fractionOfJourney);
		}
		else if (startingAlpha <= 0f || activeMenu == resultsMenu)
		{
			FadeIn(fractionOfJourney);
		}

		if (fractionOfJourney >= 1)
		{
			camera.transform.eulerAngles = rotationDest;
			rotating = false;
		}
	}

	private void FadeIn(float alpha)
	{
		if (!activeMenu.activeSelf)
		{
			activeMenu.SetActive(true);
		}
		
		alpha = Mathf.Clamp01(alpha);
		canvasGroup.alpha = alpha;
	}

	private void FadeOut(float alpha)
	{
		alpha = 1 - alpha;
		alpha = Mathf.Clamp01(alpha);
		canvasGroup.alpha = alpha;

		if (alpha <= 0.01f)
		{
			if (activeMenu == startMenu || activeMenu == resultsMenu)
			{
				activeMenu.gameObject.SetActive(false);
				activeMenu = pauseMenu;
			}
			IsPaused = false;
			uiMenu.SetActive(true);
		}
	}

	private int GetRank()
	{
		int rank = 0;

		for (int i = 0; i < highScores.Length; i++)
		{
			if (score > highScores[i])
			{
				rank = i + 1;
				AdjustHighScores(i);
				break;
			}
		}

		return rank;
	}

	private void AdjustHighScores(int index)
	{
		int prev = 0;
		int tmp = 0;
		for (int i = index; i < highScores.Length; i++)
		{
			if (i == index)
			{

				prev = highScores[i];
				highScores[i] = score;
			}
			else
			{
				tmp = highScores[i];
				highScores[i] = prev;
				prev = tmp;
			}
		}
	}

	private void PopulateHighScores()
	{
		TextMeshProUGUI nameText = highscoresMenu.transform.Find("NamesText").gameObject.GetComponent<TextMeshProUGUI>();
		TextMeshProUGUI highScoresText = highscoresMenu.transform.Find("HighScoresText").gameObject.GetComponent<TextMeshProUGUI>();

		nameText.text = string.Empty;
		highScoresText.text = string.Empty;

		for (int i = 0; i < highScores.Length; i++)
		{
			nameText.text += (i + 1).ToString() + ".";

			if (i == highScores.Length - 1)
			{
				nameText.text += " ";
			}
			else
			{
				nameText.text += "   ";
			}

			nameText.text += names[i];
			highScoresText.text += highScores[i].ToString("00000");

			if (i != highScores.Length - 1)
			{
				nameText.text += "\n";
				highScoresText.text += "\n";
			}
		}
	}

	private void AddName(string name)
	{
		string prev = "";
		string tmp = "";
		for (int i = rank - 1; i < names.Length; i++)
		{
			if (i == rank - 1)
			{

				prev = names[i];
				names[i] = name;
			}
			else
			{
				tmp = names[i];
				names[i] = prev;
				prev = tmp;
			}
		}
	}

	#endregion

	#region Public Methods

	public void TogglePause()
	{
		if (!rotating)
		{
			startingAlpha = canvasGroup.alpha;
			if (!IsPaused)
			{
				IsPaused = true;
				uiMenu.SetActive(false);
			}
			StartRotation();		
		}
	}

	public bool IsLocationOccupied(Vector2 location)
	{
		bool found = false;

		foreach (Fruit current in fruitList)
		{
			if (current.Location.Equals(location))
			{
				found = true;
			}
		}

		return found;
	}

	public void AddFruit(Fruit fruit)
	{
		fruitList.Add(fruit);
	}

	public void EatFruit(Fruit fruit)
	{
		AudioManager.Instance.PlayEatEffect();
		score += fruit.Points;
		scoreText.text = "Score: " + score.ToString("00000");
		fruitList.Remove(fruit);
		Instantiate(eatEffectPrefab, fruit.transform.position, Quaternion.identity, null);
		Destroy(fruit.gameObject);
	}

	public void GameOver()
	{
		gameOver = true;
		IsPaused = true;
		GameObject instructions = uiMenu.transform.Find("InstructionsText").gameObject;
		AudioManager.Instance.PlayGameOverEffect();
		instructions.SetActive(true);
		pauseMenu.SetActive(false);
		nameEntryErrorText.SetActive(false);
		instructions.transform.GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text =
			"Game Over. Press any key to continue.";
	}

	public void PostGameOver()
	{
		activeMenu = resultsMenu;
		rank = GetRank();
		TextMeshProUGUI infoText = resultsMenu.transform.Find("InfoText").gameObject.GetComponent<TextMeshProUGUI>();
		if (rank > 0 && rank < 11)
		{
			infoText.text = "Congratulations! You got " + rank.ToString();

			switch (rank)
			{
				case 1:
					infoText.text += "st";
					break;
				case 2:
					infoText.text += "nd";
					break;
				case 3:
					infoText.text += "rd";
					break;
				default:
					infoText.text += "th";
					break;
			}

			infoText.text += " place!";

			nameInput.gameObject.SetActive(true);
		}
		else
		{
			nameInput.gameObject.SetActive(false);
			infoText.text = "You Lose!";
		}
		TogglePause();
	}

	public void CloseInstructionsText()
	{
		uiMenu.transform.Find("InstructionsText").gameObject.SetActive(false);
	}

	public void SetInstructionsText(string str)
	{
		GameObject instructions = uiMenu.transform.Find("InstructionsText").gameObject;
		instructions.SetActive(true);
		instructions.transform.GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = str;
	}

	#endregion

	#region Button Click Handlers

	public void NewGameClick()
	{
		AudioManager.Instance.PlayClickEffect();
		RestartClick();
	}

	public void MainMenuClick()
	{
		AudioManager.Instance.PlayClickEffect();

		if (activeMenu == resultsMenu && nameInput.gameObject.activeSelf)
		{
			if (string.IsNullOrEmpty(nameInput.text))
			{
				nameEntryErrorText.SetActive(true);
				return;
			}
			else
			{
				AddName(nameInput.text);
			}
		}

		activeMenu.SetActive(false);
		activeMenu = startMenu;
		gameOver = false;
		activeMenu.SetActive(true);
	}

	public void ControlsClick()
	{
		AudioManager.Instance.PlayClickEffect();

		if (activeMenu == startMenu)
		{
			controlsMenu.transform.Find("MainMenuButton").gameObject.SetActive(true);
			controlsMenu.transform.Find("PauseMenuButton").gameObject.SetActive(false);
		}
		else if (activeMenu == pauseMenu)
		{
			controlsMenu.transform.Find("MainMenuButton").gameObject.SetActive(false);
			controlsMenu.transform.Find("PauseMenuButton").gameObject.SetActive(true);
		}

		activeMenu.SetActive(false);
		activeMenu = controlsMenu;
		activeMenu.SetActive(true);
	}

	public void OptionsClick()
	{
		AudioManager.Instance.PlayClickEffect();

		if (activeMenu == startMenu)
		{
			optionsMenu.transform.Find("MainMenuButton").gameObject.SetActive(true);
			optionsMenu.transform.Find("PauseMenuButton").gameObject.SetActive(false);
		}
		else if (activeMenu == pauseMenu)
		{
			optionsMenu.transform.Find("MainMenuButton").gameObject.SetActive(false);
			optionsMenu.transform.Find("PauseMenuButton").gameObject.SetActive(true);
		}

		activeMenu.SetActive(false);
		activeMenu = optionsMenu;
		activeMenu.SetActive(true);
	}

	public void ReturnToPauseClick()
	{
		AudioManager.Instance.PlayClickEffect();

		activeMenu.SetActive(false);
		activeMenu = pauseMenu;
		activeMenu.SetActive(true);
	}

	public void HighScoresClick()
	{
		AudioManager.Instance.PlayClickEffect();

		activeMenu.SetActive(false);
		activeMenu = highscoresMenu;
		activeMenu.SetActive(true);
		PopulateHighScores();
	}

	public void RestartClick()
	{
		AudioManager.Instance.PlayClickEffect();

		if (activeMenu == resultsMenu && nameInput.gameObject.activeSelf)
		{
			if (string.IsNullOrEmpty(nameInput.text))
			{
				nameEntryErrorText.SetActive(true);
				return;
			}
			else
			{
				AddName(nameInput.text);
			}
		}

		score = 0;
		scoreText.text = "Score: " + score.ToString("00000");
		timer = 0;
		timeText.text = FormatTime(timer);
		snake.Restart();
		fruitSpawner.Spawning = false;
		gameOver = false;

		for (int i = 0; i < fruitList.Count; i++)
		{
			Destroy(fruitList[i].gameObject);
		}

		fruitList.Clear();

		TogglePause();
	}

	public void QuitClick()
	{
		AudioManager.Instance.PlayClickEffect();

		#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
		#else
			Application.Quit();
		#endif
	}

	public void RedClick()
	{
		snake.Color = Color.red;
		Vector3 pos = colorSelection.transform.Find("SelectedColor").position;
		pos.x = colorSelection.transform.Find("RedButton").position.x;
		colorSelection.transform.Find("SelectedColor").position = pos;
	}

	public void WhiteClick()
	{
		snake.Color = Color.white;
		Vector3 pos = colorSelection.transform.Find("SelectedColor").position;
		pos.x = colorSelection.transform.Find("WhiteButton").position.x;
		colorSelection.transform.Find("SelectedColor").position = pos;
	}

	public void YellowClick()
	{
		snake.Color = Color.yellow;
		Vector3 pos = colorSelection.transform.Find("SelectedColor").position;
		pos.x = colorSelection.transform.Find("YellowButton").position.x;
		colorSelection.transform.Find("SelectedColor").position = pos;
	}

	public void GreenClick()
	{
		snake.Color = Color.green;
		Vector3 pos = colorSelection.transform.Find("SelectedColor").position;
		pos.x = colorSelection.transform.Find("GreenButton").position.x;
		colorSelection.transform.Find("SelectedColor").position = pos;
	}

	public void BlueClick()
	{
		snake.Color = Color.blue;
		Vector3 pos = colorSelection.transform.Find("SelectedColor").position;
		pos.x = colorSelection.transform.Find("BlueButton").position.x;
		colorSelection.transform.Find("SelectedColor").position = pos;
	}

	public void PurpleClick()
	{
		snake.Color = startColor;
		Vector3 pos = colorSelection.transform.Find("SelectedColor").position;
		pos.x = colorSelection.transform.Find("PurpleButton").position.x;
		colorSelection.transform.Find("SelectedColor").position = pos;
	}
	#endregion
}
