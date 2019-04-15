﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
	public BuildUI buildUI;
	public UIInteractionPanel interactionPanel;

	private Camera _cam;

	private Tile _selectedTile = null;
	private bool _uiBlocked;

	void Start()
	{
		_cam = Camera.main;
		interactionPanel.HidePanel();
		interactionPanel.OnBlur += () => _uiBlocked = false;
		interactionPanel.OnHover += () => _uiBlocked = true;
		interactionPanel.OnUpgradeClick += UpgradeTile;
		interactionPanel.OnDestroyClick += DestroyTile;
	}

	void DestroyTile()
	{
		Map.ActiveMap.RevertTile(_selectedTile as BuildingTile);
		interactionPanel.HidePanel();
	}

	void UpgradeTile()
	{

	}

	void Update()
	{
		if (buildUI.hqMode)
			return;
		var mPos = Input.mousePosition;
		if (Input.GetKeyUp(KeyCode.Escape))
			interactionPanel.HidePanel();
		if (!buildUI.placeMode)
		{
			if (Input.GetKey(KeyCode.Mouse0) && !_uiBlocked && !buildUI.uiBlock)
			{
				var ray = _cam.ScreenPointToRay(mPos);
				var tile = Map.ActiveMap.GetTileFromRay(ray);
				if (tile != null && tile is BuildingTile)
				{
					ShowPanel(tile);
				}
			}
		}
		else
			interactionPanel.HidePanel();

		if(interactionPanel.PanelVisible)
		{
			var uiPos = _cam.WorldToScreenPoint(_selectedTile.SurfacePoint);
			if (uiPos.x < 0)
				uiPos.x = 0;
			if (uiPos.x + interactionPanel.Width > Screen.width)
				uiPos.x = Screen.width - interactionPanel.Width;
			if (uiPos.y < 0)
				uiPos.y = interactionPanel.Height;
			if (uiPos.y > Screen.height)
				uiPos.y = Screen.height;
			interactionPanel.AnchoredPosition = uiPos;
		}

	}

	void ShowPanel(Tile tile)
	{
		_selectedTile = tile;
		switch (tile)
		{
			case HQTile _:
				interactionPanel.ShowPanel(tile.info.name, tile.info.Description, true, false);
				break;
			case SubHQTile _:
				interactionPanel.ShowPanel(tile.info.name, tile.info.Description, true, false);
				break;
			case BuildingTile _:
				interactionPanel.ShowPanel(tile.info.name, tile.info.Description, true, true);
				break;
			default:
				interactionPanel.ShowPanel(tile.info.name, tile.info.Description, false, false);
				break;
		}
	}
}