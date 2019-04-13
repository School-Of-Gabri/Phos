﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Units/Unit")]
public class MobileUnitInfo : MeshEntityRotatable
{
	public override ComponentType[] GetComponents()
	{
		return base.GetComponents().Concat(new ComponentType[] {
			typeof(MoveSpeed),
			typeof(Heading)
		}).ToArray();
	}

	public Entity Instantiate(Vector3 pos, Quaternion rotation)
	{
		var e = Instantiate(pos, Vector3.one, rotation);
		Map.EM.SetComponentData(e, new MoveSpeed { Value = 2 });
		Map.EM.SetComponentData(e, new Heading { Value = Vector3.forward });
		return e;
	}
}
