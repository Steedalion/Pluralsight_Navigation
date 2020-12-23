using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseGameObject
	: MonoBehaviour
	, IPausable
{

	// Use this for initialization
	void Start ()
	{
		// Register	with the game
		Game.Instance.RegisterPauseable(this);
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	public void Pause()
	{
		var thisAnim = GetComponent<Animator>();
		if (thisAnim != null)
		{
			thisAnim.speed = 0.0f;
		}

		var thisFx = GetComponent<ParticleSystem>();
		if (thisFx != null)
		{
			thisFx.Pause();
		}

		foreach (var animator in GetComponentsInChildren<Animator>())
		{
			animator.speed = 0.0f;
		}

		foreach (var fx in GetComponentsInChildren<ParticleSystem>())
		{
			fx.Pause();
		}
	}

	public void Resume()
	{
		var thisAnim = GetComponent<Animator>();
		if (thisAnim != null)
		{
			thisAnim.speed = 1.0f;
		}

		var thisFx = GetComponent<ParticleSystem>();
		if (thisFx != null)
		{
			thisFx.Play();
		}

		foreach (var animator in GetComponentsInChildren<Animator>())
		{
			animator.speed = 1.0f;
		}

		foreach (var fx in GetComponentsInChildren<ParticleSystem>())
		{
			fx.Play();
		}
	}


	void OnDestroy()
	{
		if (Game.Instance != null)
		{
			Game.Instance.UnregisterPauseable(this);
		}
	}
}
