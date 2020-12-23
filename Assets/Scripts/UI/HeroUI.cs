using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class HeroUI
	: MonoBehaviour
	, IPointerEnterHandler
	, IPointerExitHandler
{
	[SerializeField]
	Image _HPBarBackground;

	[SerializeField]
	Image _HPBarForeground;

	// Cached value
	Hero _Hero;
	Animator _Animator;
	int _VisibleID;

	float _MaxHP;
	float _MinHealthWidth;
	float _MaxHealthWidth;

	float _CurrentHP;
	bool _CurrentlyHovered;

	void Awake()
	{
		_Animator = GetComponent<Animator>();
		_VisibleID = Animator.StringToHash("Visible");

		// Compute min and max for display
		_MinHealthWidth = 0.0f;
		_MaxHealthWidth = _HPBarBackground.rectTransform.rect.width;
	}

	bool Visible
	{
		get { return _CurrentlyHovered || _CurrentHP != _MaxHP; }
	}

	// Update is called once per frame
	void Update()
	{
		// For now we poll!
		if (_Hero.Stats.HP != _CurrentHP)
		{
			// Update stored value
			_CurrentHP = _Hero.Stats.HP;

			// Compute new image size and apply
			float width = Mathf.Lerp(_MinHealthWidth, _MaxHealthWidth, _CurrentHP / _MaxHP);
			_HPBarForeground.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
			_Animator.SetBool(_VisibleID, Visible);
		}
	}

	void OnDestroy()
	{
	}

	public void Initialize()
	{
		// Register for mouse hover events!
		_Hero = Game.Instance.Hero;

		// Initialize HP bar!
		_MaxHP = _Hero.Stats.HP;
		_CurrentHP = _MaxHP; // For polling!
		_HPBarForeground.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _MaxHealthWidth);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_CurrentlyHovered = true;
		_Animator.SetBool(_VisibleID, Visible);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_CurrentlyHovered = false;
		_Animator.SetBool(_VisibleID, Visible);
	}
}
