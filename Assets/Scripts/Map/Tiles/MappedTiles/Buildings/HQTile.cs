﻿using DataStore.ConduitGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class HQTile : BuildingTile
{
	public readonly HQTileInfo hqInfo;

	public HQTile(HexCoords coords, float height, HQTileInfo tInfo) : base(coords, height, tInfo)
	{
		hqInfo = tInfo;
	}

	public override void OnPlaced()
	{
#if DEBUG
		if (Map.ActiveMap.HQ != null)
			throw new Exception("Second HQ added");
#endif
		Map.ActiveMap.HQ = this;
		Map.ActiveMap.conduitGraph = new ConduitGraph(Coords);
		var info = this.info as HQTileInfo;
		var tilesToReplace = Map.ActiveMap.GetNeighbors(Coords);
		for (int i = 0; i < tilesToReplace.Length; i++)
		{
			Map.ActiveMap.ReplaceTile(tilesToReplace[i], info.subHQTiles[i]);
		}
		GameRegistry.BaseNameUI.panel.Show();
	}

	protected override void OnBuilt()
	{
		var spawnTiles = Map.ActiveMap.HexSelect(Coords, 2);
		for (int i = 0; i < spawnTiles.Count; i++)
		{
			if (!(spawnTiles[i] is BuildingTile))
				Map.ActiveMap.AddUnit(hqInfo.unitInfo, spawnTiles[i]);
		}
		ResourceSystem.AddResources(hqInfo.startingResources);
	}

	public override void OnHeightChanged()
	{
		base.OnHeightChanged();
		var foundation = Map.ActiveMap.HexSelect(Coords, buildingInfo.size);
		for (int i = 0; i < foundation.Count; i++)
		{
			if(foundation[i] != this)
				foundation[i].UpdateHeight(Height);
		}
	}

	
}

public class SubHQTile : PoweredBuildingTile
{
	public SubHQTile(HexCoords coords, float height, SubHQTileInfo tInfo) : base(coords, height, tInfo)
	{
		HasHQConnection = true;	
	}

	public override void OnPlaced()
	{
		base.OnPlaced();
		Build();
	}

	protected override void OnBuilt()
	{
	}

	public override void OnHQConnected()
	{
	}

	public override void OnHQDisconnected()
	{
	}
}
