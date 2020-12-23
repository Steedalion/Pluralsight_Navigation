using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonsterUI
	: MonoBehaviour
{
	[SerializeField]
	Image _HPBarBackground;

	[SerializeField]
	Image _HPBarForeground;

	// Cached value
	Monster _Monster;
	Animator _Animator;
	int _VisibleID;
	WorldMouseManager.IMouseHandler _MouseHandler;

	float _MaxHP;
	float _MinHealthWidth;
	float _MaxHealthWidth;

	float _CurrentHP;

	void Awake()
	{
		_Animator = GetComponent<Animator>();
		_VisibleID = Animator.StringToHash("Visible");

		// Compute min and max for display
		_MinHealthWidth = 0.0f;
		_MaxHealthWidth = _HPBarBackground.rectTransform.rect.width;
	}


	// Update is called once per frame
	void Update ()
	{
		// For now we poll!
		if (_Monster.Stats.HP != _CurrentHP)
		{
			// Update stored value
			_CurrentHP = _Monster.Stats.HP;

			// Compute new image size and apply
			float width = Mathf.Lerp(_MinHealthWidth, _MaxHealthWidth, _CurrentHP / _MaxHP);
			_HPBarForeground.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
		}
	}

	void OnDestroy()
	{
		if (_MouseHandler != null && WorldMouseManager.Instance != null)
		{
			WorldMouseManager.Instance.UnregisterHandler(_MouseHandler);
		}
	}

	public void Initialize(Monster monster)
	{
		// Register for mouse hover events!
		_Monster = monster;
		var onlyThis = new List<WorldMouseManager.ITarget>();
		onlyThis.Add(monster);
		_MouseHandler = WorldMouseManager.Instance.RegisterTargetHandlers(
			monsters: onlyThis,
			monsterOnHover: mon => Show(),
			monsterOffHover: mon => Hide());

		// Initialize HP bar!
		_MaxHP = _Monster.Stats.HP;
		_CurrentHP = _MaxHP; // For polling!
		_HPBarForeground.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _MaxHealthWidth);
	}

	public void Show()
	{
		_Animator.SetBool(_VisibleID, true);
	}

	public void Hide()
	{
		_Animator.SetBool(_VisibleID, false);
	}
}
