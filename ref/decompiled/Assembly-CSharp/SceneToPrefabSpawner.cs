using System;
using System.Collections.Generic;
using UnityEngine;

public class SceneToPrefabSpawner : MonoBehaviour, IServerComponent
{
	[Serializable]
	public class EntitySpawnInfo
	{
		public string PrefabName;

		public Vector3 Position;

		public Quaternion Rotation;

		public Vector3 Scale;

		public string ApartmentRoomNumber;
	}

	public List<EntitySpawnInfo> EntitiesToSpawn = new List<EntitySpawnInfo>();

	[NonSerialized]
	public List<BaseEntity> Entities = new List<BaseEntity>();
}
