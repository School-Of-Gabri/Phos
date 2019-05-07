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
	private int _id = -1;

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
		var t = _selectedTile as BuildingTile;
		Map.ActiveMap.RevertTile(t);
		ResourceSystem.AddResources(t.buildingInfo.cost, .5f);
		interactionPanel.HidePanel();
	}

	void UpgradeTile()
	{

	}

	void LateUpdate()
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
				if(tile != null)
				{
					if (tile.IsOccupied)
					{
						Debug.Log($"Unit Selected {_id = tile.GetUnit()}");
					}else if(tile is BuildingTile)
					{
						ShowPanel(tile);
					}
				}
			}
			if(Input.GetKey(KeyCode.Mouse1) && _id != -1)
			{
				var ray = _cam.ScreenPointToRay(mPos);
				var tile = Map.ActiveMap.GetTileFromRay(ray);
				if(tile != null)
				{
					Map.ActiveMap.units[_id].MoveTo(tile.SurfacePoint);
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
				interactionPanel.ShowPanel(tile.info.name, tile.info.description, showDestroyBtn: false);
				break;
			case SubHQTile _:
				interactionPanel.ShowPanel(tile.info.name, tile.info.description, showDestroyBtn: false);
				break;
			case PoweredBuildingTile p:
				interactionPanel.ShowPanel(tile.info.name, $"{tile.info.description}\n\n<b>HQ Connection: {p.HasHQConnection}</b>");
				break;
			case BuildingTile _:
				interactionPanel.ShowPanel(tile.info.name, tile.info.description);
				break;
			default:
				interactionPanel.ShowPanel(tile.info.name, tile.info.description, false, false);
				break;
		}
	}
}
