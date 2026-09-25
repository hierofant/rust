using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Benchmark;

[JsonModel]
public class Building
{
	[JsonModel]
	public class Entity
	{
		public ulong NetId;

		public string ResPath;

		public ulong ParentNetId;

		public Vector3 Pos;

		public Vector3 RotEuler;

		public ulong SkinID;

		public virtual void FromProto(ProtoBuf.Entity ent)
		{
			NetId = ent.baseNetworkable.uid.Value;
			ResPath = StringPool.Get(ent.baseNetworkable.prefabID);
			ParentNetId = ent.parent?.uid.Value ?? 0;
			Pos = ent.baseEntity.pos;
			RotEuler = ent.baseEntity.rot;
			SkinID = ent.baseEntity.skinid;
		}
	}

	[JsonModel]
	public class BuildingEntity : Entity
	{
		public BuildingGrade.Enum Grade;

		public ulong Model;

		public override void FromProto(ProtoBuf.Entity ent)
		{
			base.FromProto(ent);
			if (ent.buildingBlock != null)
			{
				Grade = (BuildingGrade.Enum)ent.buildingBlock.grade;
				Model = ent.buildingBlock.model;
			}
			else
			{
				Grade = BuildingGrade.Enum.None;
				Model = 0uL;
			}
		}
	}

	[JsonModel]
	public class SpawnMarker
	{
		[Flags]
		public enum SpawnType
		{
			LocalPlayer = 1,
			RemotePlayer = 2
		}

		public Vector3 Pos;

		public Vector3 RotEuler;

		public SpawnType Type;
	}

	public const uint InvalidBuildingId = 0u;

	public uint Id;

	public List<BuildingEntity> BuildingEntities;

	public List<SpawnMarker> SpawnMarkers;

	public Building(uint id)
	{
		Id = id;
		BuildingEntities = new List<BuildingEntity>();
		SpawnMarkers = new List<SpawnMarker>();
	}

	public override string ToString()
	{
		return $"{Id}: {BuildingEntities.Count} entities";
	}
}
