﻿using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;

namespace Effects.Lines
{
	public static class LineFactory
	{
		public static Entity CreateLine(MeshEntityRotatable line, Vector3 a, Vector3 b)
		{
			var e = line.Instantiate(a, Vector3.one);
			var em = World.DefaultGameObjectInjectionWorld.EntityManager;
			em.AddComponentData(e, new LineSegment
			{
				Start = a,
				End = b,
			});
			return e;
		}

		public static Entity CreateStaticLine(MeshEntityRotatable line, Vector3 a, Vector3 b, float thiccness = 0.1f)
		{
			var (t, s, r) = PrepareLine(a, b, thiccness);
			return line.Instantiate(t, s, r);
		}

		public static void UpdateStaticLine(Entity e, Vector3 a, Vector3 b, float thiccness = 0.1f)
		{
			var (t, s, r) = PrepareLine(a, b, thiccness);
			var cmb = World.DefaultGameObjectInjectionWorld.EntityManager;
			cmb.SetComponentData(e, new Translation { Value = t });
			cmb.SetComponentData(e, new NonUniformScale { Value = s });
			cmb.SetComponentData(e, new Rotation { Value = r });
		}

		public static void UpdateStaticLine(EntityCommandBuffer cmb, Entity e, Vector3 a, Vector3 b, float thiccness = 0.1f)
		{
			var (t, s, r) = PrepareLine(a, b, thiccness);
			cmb.SetComponent(e, new Translation { Value = t });
			cmb.SetComponent(e, new NonUniformScale { Value = s });
			cmb.SetComponent(e, new Rotation { Value = r });
		}

		public static void UpdateStaticLine(EntityCommandBuffer.ParallelWriter cmb, int index, Entity e, Vector3 a, Vector3 b, float thiccness = 0.1f)
		{
			var (t, s, r) = PrepareLine(a, b, thiccness);
			cmb.SetComponent(index, e, new Translation { Value = t });
			cmb.SetComponent(index, e, new NonUniformScale { Value = s });
			cmb.SetComponent(index, e, new Rotation { Value = r });
		}

		public static (float3 translation, float3 scale, quaternion rotation) PrepareLine(float3 a, float3 b, float thiccness)
		{
			var dir = b - a;
			if(dir.x == 0 && dir.z == 0)
			{
				if(dir.y < 0)
					return (a, new float3(thiccness, thiccness, math.abs(math.length(dir))), quaternion.RotateX(math.radians(90)));
				else
					return (a, new float3(thiccness, thiccness, math.abs(math.length(dir))), quaternion.RotateX(math.radians(-90)));
			}
			return (a, new float3(thiccness, thiccness, math.abs(math.length(dir))), quaternion.LookRotationSafe(dir, math.up()));
		}
	}
}