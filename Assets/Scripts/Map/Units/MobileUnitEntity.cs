﻿using System.Collections.Generic;
using System.Linq;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

using UnityEngine;

[CreateAssetMenu(menuName = "Map Asset/Units/Unit")]
public partial class MobileUnitEntity : MeshEntityRotatable
{
	[Header("Stats")]
	public float moveSpeed = 1;
	public float attackSpeed = 1;
	public float maxHealth;
	public int size;
	[Header("Classification")]
	public UnitDomain unitDomain;
	public UnitClass unitClass;
	[Header("Misc")]
	public float3 centerOfMassOffset;
	public MeshEntityRotatable head;
	public MeshEntityRotatable projectile;
	public MeshEntityRotatable healthBar;
	public MeshEntityRotatable healthBarFill;

	public override IEnumerable<ComponentType> GetComponents()
	{
		return base.GetComponents().Concat(new ComponentType[] {
			typeof(MoveSpeed),
			typeof(Heading),
			typeof(UnitId),
			typeof(Projectile),
			typeof(AttackSpeed),
			typeof(Health),
			typeof(FactionId),
			typeof(UnitClass),
			typeof(UnitDomain),
			typeof(PhysicsCollider),
			typeof(PhysicsMass),
			typeof(CenterOfMassOffset),
			typeof(CenterOfMass)
		});
	}

	public override void PrepareDefaultComponentData(Entity entity)
	{
		base.PrepareDefaultComponentData(entity);
		Map.EM.SetComponentData(entity, new MoveSpeed { Value = moveSpeed });
		Map.EM.SetComponentData(entity, new Heading { Value = Vector3.forward });
		Map.EM.SetComponentData(entity, new Projectile { Value = projectile.GetEntity() });
		Map.EM.SetComponentData(entity, new AttackSpeed { Value = 1f / attackSpeed });
		Map.EM.SetComponentData(entity, new Health { maxHealth = maxHealth, Value = maxHealth });
		Map.EM.SetComponentData(entity, new UnitDomain { Value = unitDomain.Value });
		Map.EM.SetComponentData(entity, new UnitClass { Value = unitClass.Value });
		Map.EM.SetComponentData(entity, PhysicsMass.CreateKinematic(MassProperties.UnitSphere));
		Map.EM.SetComponentData(entity, new CenterOfMassOffset { Value = centerOfMassOffset });
	}

	public Entity Instantiate(float3 pos, Quaternion rotation, int id, Faction faction = Faction.None)
	{
		var e = Instantiate(pos, Vector3.one, rotation);
		Map.EM.SetComponentData(e, new UnitId { Value = id });
		Map.EM.SetComponentData(e, new FactionId { Value = faction });
		Map.EM.SetComponentData(e, new CenterOfMass { Value = pos + centerOfMassOffset });
		var collisionFilter = new CollisionFilter
		{
			CollidesWith = ~0u,
			BelongsTo = (1u << (int)faction) | (1u << (int)Faction.Unit),
			GroupIndex = 0
		};

		var physMat = Unity.Physics.Material.Default;
		physMat.Flags |= Unity.Physics.Material.MaterialFlags.EnableCollisionEvents;

		Map.EM.SetComponentData(e, new PhysicsCollider
		{
			Value = Unity.Physics.BoxCollider.Create(new BoxGeometry
			{
				Center = new float3(),
				Size = new float3(1, 1, 1),
				Orientation = quaternion.identity,
				BevelRadius = 0
			}, collisionFilter, physMat)
		});

		var bar = healthBar.Instantiate(pos);
		Map.EM.AddComponentData(bar, new HealthBar { target = e });
		var fill = healthBarFill.Instantiate(pos);
		Map.EM.AddComponentData(fill, new HealthBar { target = e }); ;
		Map.EM.AddComponent<HealthBarFillTag>(fill);
		return e;
	}
}