using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface for all game managers (like the camera manager, input, etc...)
/// </summary>
public interface IManager
{
	void Initialize();
	void Process();
}
