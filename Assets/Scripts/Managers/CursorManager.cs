using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager
	: SingletonBehaviour<CursorManager>
	, IManager
{
	Texture2D _CurrentCursor;

	public void OnNavMeshHover()
	{
		if (_CurrentCursor != Globals.Instance.Settings.MoveCursor)
		{
			_CurrentCursor = Globals.Instance.Settings.MoveCursor;
			Cursor.SetCursor(Globals.Instance.Settings.MoveCursor, Globals.Instance.Settings.MoveCursorHotspot, CursorMode.Auto);
		}
	}

	public void OnNothingHover()
	{
		if (_CurrentCursor != Globals.Instance.Settings.DefaultCursor)
		{
			_CurrentCursor = Globals.Instance.Settings.DefaultCursor;
			Cursor.SetCursor(Globals.Instance.Settings.DefaultCursor, Globals.Instance.Settings.DefaultCursorHotspot, CursorMode.Auto);
		}
	}

	public void OnMonsterHover()
	{
		if (_CurrentCursor != Globals.Instance.Settings.AttackCursor)
		{
			_CurrentCursor = Globals.Instance.Settings.AttackCursor;
			Cursor.SetCursor(Globals.Instance.Settings.AttackCursor, Globals.Instance.Settings.AttackCursorHotspot, CursorMode.Auto);
		}
	}

	public void OnDoorwayHover()
	{
		if (_CurrentCursor != Globals.Instance.Settings.DoorwayCursor)
		{
			_CurrentCursor = Globals.Instance.Settings.DoorwayCursor;
			Cursor.SetCursor(Globals.Instance.Settings.DoorwayCursor, Globals.Instance.Settings.DoorwayCursorHotspot, CursorMode.Auto);
		}
	}

	public void Initialize()
	{
		// Initialize cursor!
		OnNothingHover();

		// 
	}

	public void Process()
	{
		
	}
}
