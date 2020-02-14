﻿using AnimationSystem.Animations;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset / Building Decorators / Wind Turbine")]
public class TurbineHeadDecorator : TileDecorator
{
	public Vector3 bladeOffset = Vector3.zero;

	public override int GetDecorEntityCount(Tile tile) => 1;

	public override Entity[] Render(Tile tile)
	{
		var entities = new Entity[1];
		entities[0] = meshEntity.Instantiate(tile.SurfacePoint);
		Map.EM.AddComponentData(entities[0], new RotateAxis { Value = Vector3.up });
		Map.EM.AddComponentData(entities[0], new RotateSpeed { Value = -4 });
		return entities;
	}

	public override void UpdateHeight(NativeSlice<Entity> decor, Tile tile)
	{
		Map.EM.SetComponentData(decor[0], new Translation { Value = tile.SurfacePoint + bladeOffset });
	}
}