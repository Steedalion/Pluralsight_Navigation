using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IResetable
	: System.IDisposable
{
	void Init();
}
