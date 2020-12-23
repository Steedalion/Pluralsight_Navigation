using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Our first singleton: Globals!
/// Stores all kinds of constants referenced throughout the code
/// </summary>
public class Globals
	: SingletonBehaviour<Globals>
{
	[SerializeField]
	[DisplayScriptableObjectProperties]
	GameSettings _Settings;

	public GameSettings Settings
	{
		get { return _Settings; }
	}
}
