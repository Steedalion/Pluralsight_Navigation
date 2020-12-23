using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIManager
	: SingletonBehaviour<UIManager>
	, IManager
{
	[Header("Scene References")]
	[SerializeField]
	Canvas _UIRoot;

	[SerializeField]
	RectTransform _CharacterUIRoot;

	[SerializeField]
	RectTransform _HeroUIAnchor;

	[SerializeField]
	Animator _FaderAnimator;

	[SerializeField]
	Animator _StartGameAnimator;

	[SerializeField]
	Button _StartGameRestartButton;

	[SerializeField]
	Animator _GameOverAnimator;

	[SerializeField]
	Button _GameOverRestartButton;

	[SerializeField]
	Animator _PauseAnimator;

	[SerializeField]
	Button _PauseMenuResumeGameButton;

	[SerializeField]
	Button _PauseMenuQuitGameButton;

	[SerializeField]
	Button _PauseMenuRestartGameButton;

	[SerializeField]
	Text _CounterText;

	Dictionary<Character, GameObject> _CharacterUIs;
	InputManager.ILayer _BlockInputLayer;

	public void Initialize()
	{
		_CharacterUIs = new Dictionary<Character, GameObject>();
		_GameOverRestartButton.onClick.AddListener(() => Game.Instance.PushMessage(Messages.RestartGame.Create()));
		_PauseMenuQuitGameButton.onClick.AddListener(() => Game.Instance.PushMessage(Messages.QuitGame.Create()));
		_PauseMenuRestartGameButton.onClick.AddListener(() => Game.Instance.PushMessage(Messages.RestartGame.Create()));
		_PauseMenuResumeGameButton.onClick.AddListener(() => Game.Instance.PushMessage(Messages.UnpauseGame.Create()));
	}

	public void Process()
	{
		// Nothing to process for now!
	}

	public bool WorldPointToLocalPoint(Vector3 worldPoint, out Vector2 localPoint)
	{
		Vector2 screenPoint = CameraManager.Instance.Camera.WorldToScreenPoint(worldPoint);
		return ScreenPointToLocalPoint(screenPoint, out localPoint);
	}

	public bool ScreenPointToLocalPoint(Vector2 screenPoint, out Vector2 localPoint)
	{
		return RectTransformUtility.ScreenPointToLocalPointInRectangle(_CharacterUIRoot, screenPoint, null, out localPoint);
	}

	public MonsterUI CreateMonsterUI(Monster monster)
	{
		var ret = GameObject.Instantiate<MonsterUI>(Globals.Instance.Settings.MonsterUIPrefab, _CharacterUIRoot);
		ret.Initialize(monster);

		// Make the ui follow the anchor!
		var followTransform = ret.GetComponent<UIFollowTransform>();
		followTransform.Target = monster.UIAnchor;

		_CharacterUIs.Add(monster, ret.gameObject);

		return ret;
	}

	public void DestroyMonsterUI(Monster monster)
	{
		DestroyUI(monster);
	}

	public HeroUI CreateHeroUI()
	{
		var ret = GameObject.Instantiate<HeroUI>(Globals.Instance.Settings.HeroUIPrefab, _HeroUIAnchor, false);
		//ret.transform.localPosition = Vector3.zero;
		ret.Initialize();
		_CharacterUIs.Add(Game.Instance.Hero, ret.gameObject);
		return ret;
	}

	public void DestroyHeroUI()
	{
		DestroyUI(Game.Instance.Hero);
	}

	public void ShowGameOver()
	{
		BlockInput();
		_GameOverAnimator.SetBool("Visible", true);
	}

	public void HideGameOver()
	{
		_GameOverAnimator.SetBool("Visible", false);
		UnblockInput();
	}

	public void ShowStartGame()
	{
		BlockInput();
		_StartGameAnimator.SetBool("Visible", true);
	}

	public void HideStartGame()
	{
		_StartGameAnimator.SetBool("Visible", false);
		UnblockInput();
	}

	public void ShowFader()
	{
		_FaderAnimator.SetBool("Visible", true);
	}

	public void HideFader()
	{
		_FaderAnimator.SetBool("Visible", false);
	}

	public void ShowGamePaused()
	{
		BlockInput();
		_PauseAnimator.SetBool("Visible", true);
	}

	public void HideGamePaused()
	{
		_PauseAnimator.SetBool("Visible", false);
		UnblockInput();
	}

	public void UpdateKillCount()
	{
		_CounterText.text = "Kills: " + CombatManager.Instance.KillCount;
	}

	void DestroyUI(Character character)
	{
		GameObject ui = null;
		if (_CharacterUIs.TryGetValue(character, out ui))
		{
			ui.transform.SetParent(null);
			GameObject.Destroy(ui);
			_CharacterUIs.Remove(character);
		}
	}
	
	void BlockInput()
	{
		_BlockInputLayer = InputManager.Instance.SetLayer(
			InputManager.Layers.ScreenUI,
			null,
			BlockKeyCodeHandler,
			BlockMouseButtonHandler,
			BlockMouseButtonHandler,
			blockMouseMoveHandler);
	}

	void UnblockInput()
	{
		if (_BlockInputLayer != null)
		{
			InputManager.Instance.PopLayer(_BlockInputLayer);
		}
	}

	bool BlockKeyCodeHandler(KeyCode key, bool down, bool up)
	{
		return true;
	}
	bool BlockMouseButtonHandler(Vector2 position, bool down, bool up)
	{
		return true;
	}
	bool blockMouseMoveHandler(Vector2 position, bool hasPriority)
	{
		return hasPriority;
	}
}