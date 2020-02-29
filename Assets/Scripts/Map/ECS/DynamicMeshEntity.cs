﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using SphereCollider = Unity.Physics.SphereCollider;

[CreateAssetMenu(menuName = "ECS/Dynamic Entity")]
public class DynamicMeshEntity : MeshEntityRotatable
{
	[Header("Collision")]
	public float colliderRadius;

	public override IEnumerable<ComponentType> GetComponents()
	{
		return base.GetComponents().Concat(new ComponentType[]
		{
			typeof(PhysicsCollider),
			typeof(PhysicsVelocity)
		});

	}

	public virtual Entity Instantiate(float3 position, quaternion rotation, float3 velocity = default, float3 angularVelocity = default)
	{
		var e = Instantiate(position, 1, rotation);
		var em = World.DefaultGameObjectInjectionWorld.EntityManager;
		em.SetComponentData(e, new PhysicsCollider
		{
			Value = SphereCollider.Create(new SphereGeometry
			{
				Radius = colliderRadius,
			}, CollisionFilter.Default, new Unity.Physics.Material
			{
				Flags = Unity.Physics.Material.MaterialFlags.EnableCollisionEvents
			})
		});
		em.SetComponentData(e, new PhysicsVelocity
		{
			Linear = velocity,
			Angular = angularVelocity
		});
		return e;
	}

	public Entity BufferedInstantiate(EntityCommandBuffer commandBuffer, Vector3 position, Quaternion rotation, float3 velocity = default, float3 angularVelocity = default)
	{
		var e = BufferedInstantiate(commandBuffer, position, 1, rotation);
		var col = SphereCollider.Create(new SphereGeometry
		{
			Radius = colliderRadius
		});
		commandBuffer.SetComponent(e, new PhysicsCollider
		{
			Value = col
		});
		commandBuffer.SetComponent(e, new PhysicsVelocity
		{
			Linear = velocity,
			Angular = angularVelocity
		});
		return e;
	}
}
