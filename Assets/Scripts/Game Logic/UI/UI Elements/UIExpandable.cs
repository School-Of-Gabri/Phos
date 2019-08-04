﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIExpandable : UIHover
{
	[Header("Expandable Settings")]
	public bool expandOnHover;
	public Vector2 expandSize;
	public bool overrideBaseSize;
	[ConditionalHide("overrideBaseSize")]
	public Vector2 baseSize;
	public RectTransform expandTarget;
	public float expandSpeed = 1;
	[HideInInspector]
	public bool isExpanded;

	public event Action OnExpand;
	public event Action OnContract;

	protected RectTransform _rTransform;
	private float _animTime;

	protected override void Start()
	{
		base.Start();
		_rTransform = GetComponent<RectTransform>();
		expandTarget = expandTarget ?? _rTransform;
		if(!overrideBaseSize)
			baseSize = expandTarget.rect.size;
		if (expandOnHover)
		{
			OnHover += Expand;
			OnBlur += Contract;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (isExpanded)
			_animTime += Time.deltaTime * expandSpeed;
		else
			_animTime -= Time.deltaTime * expandSpeed;
		_animTime = Mathf.Clamp(_animTime, 0, 1);
		var ease = _animTime.EaseOut(4);
		expandTarget.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(baseSize.x, expandSize.x, ease));
		expandTarget.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Lerp(baseSize.y, expandSize.y, ease));
	}

	public virtual void Expand()
	{
		isExpanded = true;
	}

	public virtual void Contract()
	{
		isExpanded = false;
	}
}