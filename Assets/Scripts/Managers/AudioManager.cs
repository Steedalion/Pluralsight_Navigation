using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Audio;

public class AudioManager
	: SingletonBehaviour<AudioManager>
	, IManager
{
	[Header("Scene References")]
	[SerializeField]
	AudioSource _CameraAudioSource;
	[SerializeField]
	AudioSource _MainMusicAudioSource;
	[SerializeField]
	AudioSource _AuxMusicAudioSource;
	[SerializeField]
	AudioSource _AmbienceAudioSource;

	public void Initialize()
	{
	}

	public void Process()
	{
		// Nothing to process for now!
	}

	public void PlayAcknowledge()
	{
		_CameraAudioSource.PlayOneShot(Globals.Instance.Settings.ValidCommand);
	}

	public void PlayInvalid()
	{
		_CameraAudioSource.PlayOneShot(Globals.Instance.Settings.InvalidCommand);
	}

	public void OnCombatStart()
	{
		// Do something!!!
		_AuxMusicAudioSource.volume = 0.5f; // TEMP!!!
	}

	public void OnCombatEnd()
	{
		// Do something!!!
		_AuxMusicAudioSource.volume = 0.05f; // TEMP!!!
	}

	public void FadeIn()
	{
		// Nothing to do for now!
		Globals.Instance.Settings.MainSnapshot.TransitionTo(0.25f);
	}

	public void FadeOut()
	{
		// Nothing to do for now!
		Globals.Instance.Settings.SilenceSnapshot.TransitionTo(0.25f);
	}
}