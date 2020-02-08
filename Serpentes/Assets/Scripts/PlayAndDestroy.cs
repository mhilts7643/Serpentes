using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAndDestroy : MonoBehaviour
{
	private ParticleSystem ps;

	// Start is called before the first frame update
	void Start()
	{
		ps = GetComponent<ParticleSystem>();
		ps.Play();
		Invoke("DelayedDestroy", ps.main.duration);
	}

	private void DelayedDestroy()
	{
		Destroy(gameObject);
	}
}
