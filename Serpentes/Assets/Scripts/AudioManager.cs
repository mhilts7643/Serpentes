using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance;

	[Tooltip("The music volume slider.")]
	[SerializeField]
	private Slider musicVolSlider;

	[Tooltip("The soundeffects volume slider.")]
	[SerializeField]
	private Slider soundEffectsVolSlider;

	private bool sfx;

	private AudioSource music;
	private AudioSource clickEffect;
	private AudioSource eatEffect;
	private AudioSource gameOverEffect;

	// Start is called before the first frame update
	void Start()
    {
		if (Instance != null)
		{
			Debug.LogError("Duplicate audiomanager detected, destroying now.");
			Destroy(this);
		}
		Instance = this;

		music = GetComponent<AudioSource>();
		clickEffect = transform.Find("RattleSound").gameObject.GetComponent<AudioSource>();
		eatEffect = transform.Find("ChompSound").gameObject.GetComponent<AudioSource>();
		gameOverEffect = transform.Find("GameOverSound").gameObject.GetComponent<AudioSource>();

		musicVolSlider.value = music.volume;
		soundEffectsVolSlider.value = eatEffect.volume;
		sfx = true;
	}

	public void PlayClickEffect()
	{
		if (sfx)
		{
			clickEffect.Play();
		}
	}

	public void PlayEatEffect()
	{
		if (sfx)
		{
			eatEffect.Play();
		}
	}

	public void PlayGameOverEffect()
	{
		if (sfx)
		{
			gameOverEffect.Play();
		}
	}

	public void MusicToggleClick(bool on)
	{
		if (on)
		{
			music.Play();
			musicVolSlider.interactable = true;
		}
		else
		{
			music.Stop();
			musicVolSlider.interactable = false;
		}
	}

	public void SoundEffectsToggleClick(bool on)
	{
		if (on)
		{
			sfx = true;
			soundEffectsVolSlider.interactable = true;
		}
		else
		{
			sfx = false;
			soundEffectsVolSlider.interactable = false;
		}
	}

	public void MusicVolChanged(float newValue)
	{
		music.volume = newValue;
	}

	public void SoundEffectsVolumeChanged(float newValue)
	{
		clickEffect.volume = newValue;
		eatEffect.volume = newValue;
		gameOverEffect.volume = newValue;
		if (GameManager.Instance.ActiveMenu.name.Equals("OptionsMenu"))
		{
			eatEffect.Play();
		}
	}

}
