﻿using Unity.Mathematics;
using UnityEngine;

public class WindTurbineTileEntity : BuildingTileEntity
{
	[Header("Wind Turbine")]
	public float maxSpinSpeed = 10;
	public float2 efficencyRange;

	public override Tile CreateTile(Map map, HexCoords pos, float height)
	{
		return new WindTurbileTile(pos, height, map, this);
	}
}