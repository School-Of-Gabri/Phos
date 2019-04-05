﻿using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Tile Decorators/Ice")]
public class IceDecorator : TileDecorator
{
	public float height;
	public override int GetDecorEntityCount(Tile tile)
	{
		return 0;
	}

	public override Entity[] Render(Tile tile, Entity parent)
	{
		var e = new Entity[0];
		//e[0] = meshEntity.Instantiate(tile.SurfacePoint, new Vector3(1, Mathf.Abs(Map.ActiveMap.SeaLevel - tile.Height) + height, 1));
		Map.EM.SetComponentData(parent, new NonUniformScale { Value = new Vector3(1, Map.ActiveMap.SeaLevel + height, 1) });
		return e;
	}

	public override void UpdateHeight(NativeSlice<Entity> decor, Tile tile, Entity parent)
	{
		Map.EM.SetComponentData(parent, new NonUniformScale { Value = new Vector3(1, Map.ActiveMap.SeaLevel + height, 1) });
	}
}
