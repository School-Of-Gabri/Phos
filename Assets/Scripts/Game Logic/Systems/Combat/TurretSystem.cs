﻿using Amatsugu.Phos.Tiles;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Amatsugu.Phos.ECS
{
	[UpdateAfter(typeof(StepPhysicsWorld))]
	public class TurretSystem : ComponentSystem
	{

		private bool _isReady = false;
		private ProjectileMeshEntity _bullet;
		private BuildPhysicsWorld _physicsWorld;
		private StepPhysicsWorld _simWorld;
		private NativeList<int> _castHits;
		private CollisionFilter _playerTargetingFilter;
		private CollisionFilter _phosTargetingFilter;

		protected override void OnCreate()
		{
			base.OnCreate();
			var op = Addressables.LoadAssetAsync<ProjectileMeshEntity>("PlayerLaser");
			op.Completed += e =>
			{
				if (e.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
				{
					_bullet = e.Result;
					_isReady = true;
				}
			};
			_physicsWorld = World.GetExistingSystem<BuildPhysicsWorld>();
			_simWorld = World.GetExistingSystem<StepPhysicsWorld>();
			_castHits = new NativeList<int>(Allocator.Persistent);
			_playerTargetingFilter = new CollisionFilter
			{
				BelongsTo = (1u << (int)Faction.Player),
				CollidesWith = (1u << (int)Faction.Phos)
			};
			_phosTargetingFilter = new CollisionFilter
			{
				BelongsTo = (1u << (int)Faction.Phos),
				CollidesWith = (1u << (int)Faction.Player)
			};
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			_castHits.Dispose();
		}

		protected override void OnUpdate()
		{
			if (!_isReady)
				return;

			var world = _physicsWorld.PhysicsWorld;
			_castHits.Clear();

			
			//Select Target
			Entities.WithNone<AttackTarget, BuildingOffTag, BuildingDisabledTag>().WithAll<Turret>().ForEach((Entity e, ref Translation pos, ref AttackRange range, ref FactionId faction, ref AttackSpeed speed) =>
			{
				if (Time.ElapsedTime < speed.NextAttackTime)
					return;
				
				_physicsWorld.AABBCast(pos.Value, range.MaxRange, faction.Value == Faction.Player ? _playerTargetingFilter : _phosTargetingFilter, ref _castHits);
				int closest = -1;
				float closestDist = range.MaxRange;
				//Find the closest target
				for (int i = 0; i < _castHits.Length; i++)
				{
					var tgtE = _physicsWorld.PhysicsWorld.Bodies[_castHits[i]].Entity;
					var tgtPos = EntityManager.GetComponentData<CenterOfMass>(tgtE).Value;
					var dist = math.length(pos.Value - tgtPos);
					if(dist < closestDist && dist >= range.MinRange)
					{
						closest = i;
						closestDist = dist;
					}
				}
				if(closest != -1)
					PostUpdateCommands.AddComponent(e, new AttackTarget { Value = _physicsWorld.PhysicsWorld.Bodies[_castHits[closest]].Entity });
			});

			//Aim Turret
			Entities.WithNone<BuildingOffTag, BuildingDisabledTag > ().WithAll<UnitClass.Turret>().ForEach((Entity e, ref Turret t, ref Translation pos, ref AttackTarget attackTarget) =>
			{
				if (!EntityManager.Exists(attackTarget.Value))
					return;
				var tgtPos = EntityManager.GetComponentData<CenterOfMass>(attackTarget.Value).Value;
				var dir = pos.Value - tgtPos;
				//Barrel
				if (EntityManager.Exists(t.Barrel))
				{
					var bR = math.normalize(EntityManager.GetComponentData<Rotation>(t.Barrel).Value);
					var barrelR = Quaternion.LookRotation(dir, math.up());
					barrelR = Quaternion.RotateTowards(bR, barrelR, 360 * Time.DeltaTime);
					EntityManager.SetComponentData(t.Barrel, new Rotation
					{
						Value = barrelR
					});
				}
				//Head
				dir.y = 0;
				var hR = math.normalize(EntityManager.GetComponentData<Rotation>(t.Head).Value);
				var headR = Quaternion.LookRotation(dir, math.up());
				headR = Quaternion.RotateTowards(hR, headR, 360 * Time.DeltaTime);
				EntityManager.SetComponentData(t.Head, new Rotation
				{
					Value = headR
				});
			});

			//Shoot Turret
			Entities.WithAll<UnitClass.Turret>().ForEach((Entity e, ref Turret t, ref Translation pos, ref AttackSpeed speed, ref AttackTarget attackTarget) =>
			{
				if (Time.ElapsedTime < speed.NextAttackTime)
					return;
				if (!EntityManager.Exists(attackTarget.Value))
					return;
				bool hasBarrel = EntityManager.Exists(t.Barrel);
				var r = EntityManager.GetComponentData<Rotation>(hasBarrel ? t.Barrel : t.Head).Value;
				var tgtPos = EntityManager.GetComponentData<CenterOfMass>(attackTarget.Value).Value;
				var dir = pos.Value - tgtPos;
				var flatDir = dir;
				if (!hasBarrel)
					flatDir.y = 0;
				var desR = quaternion.LookRotation(flatDir, math.up());
				//if (r.value.Equals(desR.value))
				//	return;
				var barrelPos = EntityManager.GetComponentData<Translation>(hasBarrel ? t.Barrel : t.Head).Value;
				var shotPos = barrelPos + math.rotate(r, t.shotOffset);
				DebugUtilz.DrawCrosshair(shotPos, .1f, Color.magenta, 1);
				var b = _bullet.BufferedInstantiate(PostUpdateCommands, shotPos, .2f, -math.normalize(dir) * 15f);
				if (_bullet.nonUniformScale)
					PostUpdateCommands.SetComponent(b, new NonUniformScale { Value = new float3(0.2f, 0.2f, .6f) });
				PostUpdateCommands.AddComponent(b, new DeathTime { Value = Time.ElapsedTime + 5 });
			});


			//Aim Artillery
			Entities.WithNone<BuildingOffTag, BuildingDisabledTag>().WithAll<UnitClass.Artillery>().ForEach((Entity e, ref Turret t, ref Translation pos, ref AttackTarget attackTarget) =>
			{
				if (!EntityManager.Exists(attackTarget.Value))
					return;
				var tgtPos = EntityManager.GetComponentData<CenterOfMass>(attackTarget.Value).Value;
				//Barrel
				var aim = PhysicsUtilz.CalculateProjectileShotVector(pos.Value, tgtPos);
				var bR = math.normalize(EntityManager.GetComponentData<Rotation>(t.Barrel).Value);
				var barrelR = Quaternion.LookRotation(-aim, math.up());
				barrelR = Quaternion.RotateTowards(bR, barrelR, 180 * Time.DeltaTime);
				EntityManager.SetComponentData(t.Barrel, new Rotation
				{
					Value = barrelR
				});
				//Head
				var dir = pos.Value - tgtPos;
				dir.y = 0;
				var hR = math.normalize(EntityManager.GetComponentData<Rotation>(t.Head).Value);
				var headR = Quaternion.LookRotation(dir, math.up());
				headR = Quaternion.RotateTowards(hR, headR, 180 * Time.DeltaTime);
				EntityManager.SetComponentData(t.Head, new Rotation
				{
					Value = headR
				});
			});

			//Shoot Artillery 
			Entities.WithAll<UnitClass.Artillery>().ForEach((Entity e, ref Turret t, ref Translation pos, ref AttackSpeed speed, ref AttackTarget attackTarget) =>
			{
				if (Time.ElapsedTime < speed.NextAttackTime)
					return;
				if (!EntityManager.Exists(attackTarget.Value))
					return;
				var r = EntityManager.GetComponentData<Rotation>(t.Barrel).Value;
				var barrelPos = EntityManager.GetComponentData<Translation>(t.Barrel).Value;
				DebugUtilz.DrawCrosshair(barrelPos + t.shotOffset, .2f, Color.cyan, .5f);
				var shotPos = barrelPos + math.rotate(r, t.shotOffset);
				DebugUtilz.DrawCrosshair(shotPos, .2f, Color.magenta, .5f);
				var v = PhysicsUtilz.CalculateProjectileShotVector(shotPos, EntityManager.GetComponentData<CenterOfMass>(attackTarget.Value).Value);
				Debug.Log(shotPos);
				var b = _bullet.BufferedInstantiate(PostUpdateCommands, shotPos, .5f, v);
				PostUpdateCommands.AddComponent(b, new DeathTime { Value = Time.ElapsedTime + 8 });
			});

			//Idle Anim
			Entities.WithNone<AttackTarget, BuildingOffTag, BuildingDisabledTag>().ForEach((ref Turret t) =>
			{
				var r = EntityManager.GetComponentData<Rotation>(t.Head).Value;
				r = math.mul(math.normalizesafe(r), quaternion.AxisAngle(math.up(), math.radians(10) * Time.DeltaTime));
				PostUpdateCommands.SetComponent(t.Head, new Rotation { Value = r });
				if (EntityManager.Exists(t.Barrel))
					PostUpdateCommands.SetComponent(t.Barrel, new Rotation { Value = r });
			});

			//Timing
			Entities.ForEach((ref AttackSpeed speed) =>
			{
				if (Time.ElapsedTime < speed.NextAttackTime)
					return;
				speed.NextAttackTime = Time.ElapsedTime + speed.Value;
			});

			//Verify Target
			Entities.WithAll<Turret>().ForEach((Entity e, ref Translation t, ref AttackTarget target, ref AttackRange range) =>
			{
				if (!EntityManager.Exists(target.Value))
				{
					PostUpdateCommands.RemoveComponent<AttackTarget>(e);
					return;
				}
				var tgtPos = EntityManager.GetComponentData<CenterOfMass>(target.Value);
				if (!range.IsInRange(t.Value, tgtPos.Value))
					PostUpdateCommands.RemoveComponent<AttackTarget>(e);
			});
		}
	}

	public struct Turret : IComponentData
	{
		public Entity Head;
		public Entity Barrel;
		public float3 shotOffset;
	}
}
