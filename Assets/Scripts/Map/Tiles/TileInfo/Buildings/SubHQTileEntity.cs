﻿using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile/Sub HQ")]
public class SubHQTileEntity : BuildingTileEntity
{
	public override Tile CreateTile(Map map, HexCoords pos, float height)
	{
		return new SubHQTile(pos, height, map, this);
	}
}