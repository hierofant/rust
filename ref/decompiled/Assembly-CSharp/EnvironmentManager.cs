using System.Collections.Generic;
using Development.Attributes;
using Facepunch;
using Unity.Collections;
using UnityEngine;

[ResetStaticFields]
public class EnvironmentManager : SingletonComponent<EnvironmentManager>
{
	private static ListHashSet<EnvironmentVolume> dynamicVolumes = new ListHashSet<EnvironmentVolume>();

	private static Collider[] check_colliderBuffer = new Collider[32768];

	private void Update()
	{
		foreach (EnvironmentVolume dynamicVolume in dynamicVolumes)
		{
			dynamicVolume.UpdateVolumeTransformationAndBounds();
		}
	}

	public void RegisterDynamicVolume(EnvironmentVolume volume)
	{
		dynamicVolumes.Add(volume);
	}

	public void UnregisterDynamicVolume(EnvironmentVolume volume)
	{
		dynamicVolumes.Remove(volume);
	}

	public static EnvironmentType Get(OBB obb)
	{
		EnvironmentType environmentType = (EnvironmentType)0;
		List<EnvironmentVolume> obj = Pool.Get<List<EnvironmentVolume>>();
		GamePhysics.OverlapOBB(obb, obj, 262144, QueryTriggerInteraction.Collide);
		for (int i = 0; i < obj.Count; i++)
		{
			environmentType |= obj[i].Type;
		}
		Pool.FreeUnmanaged(ref obj);
		return environmentType;
	}

	public static EnvironmentType Get(Vector3 pos, ref List<EnvironmentVolume> list, float radius = 0.01f)
	{
		EnvironmentType environmentType = (EnvironmentType)0;
		GamePhysics.OverlapSphere(pos, radius, list, 262144, QueryTriggerInteraction.Collide);
		for (int i = 0; i < list.Count; i++)
		{
			environmentType |= list[i].Type;
		}
		return environmentType;
	}

	public static EnvironmentType Get(Vector3 pos, float radius = 0.01f)
	{
		List<EnvironmentVolume> list = Pool.Get<List<EnvironmentVolume>>();
		EnvironmentType result = Get(pos, ref list, radius);
		Pool.FreeUnmanaged(ref list);
		return result;
	}

	public static bool Check(OBB obb, EnvironmentType type)
	{
		int mask = GamePhysics.HandleIgnoreCollision(obb.position, 262144);
		int num = Physics.OverlapBoxNonAlloc(obb.position, obb.extents, check_colliderBuffer, obb.rotation, mask, QueryTriggerInteraction.Collide);
		for (int i = 0; i < num; i++)
		{
			if (check_colliderBuffer[i].TryGetComponent<EnvironmentVolume>(out var component) && (component.Type & type) != 0)
			{
				return true;
			}
		}
		return false;
	}

	public static bool Check(Vector3 pos, EnvironmentType type, float radius = 0.01f)
	{
		int layerMask = GamePhysics.HandleIgnoreCollision(pos, 262144);
		int num = Physics.OverlapSphereNonAlloc(pos, radius, check_colliderBuffer, layerMask, QueryTriggerInteraction.Collide);
		for (int i = 0; i < num; i++)
		{
			if (check_colliderBuffer[i].TryGetComponent<EnvironmentVolume>(out var component) && (component.Type & type) != 0)
			{
				return true;
			}
		}
		return false;
	}

	public static void Get(NativeArray<Vector3>.ReadOnly positions, NativeArray<float>.ReadOnly radii, NativeArray<int>.ReadOnly layerMasks, NativeArray<EnvironmentType> results, int maxResPerCast, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore, GamePhysics.MasksToValidate validate = GamePhysics.MasksToValidate.All)
	{
		using (TimeWarning.New("GamePhysics.OverlapSpheresEnvironment"))
		{
			NativeArray<ColliderHit> hits = new NativeArray<ColliderHit>(positions.Length * maxResPerCast, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			GamePhysics.OverlapSpheres(positions, radii, layerMasks, hits, maxResPerCast, triggerInteraction, validate).Complete();
			using (TimeWarning.New("FindComponent"))
			{
				for (int i = 0; i < positions.Length; i++)
				{
					results[i] = (EnvironmentType)0;
					int num = i * maxResPerCast;
					for (int j = 0; j < maxResPerCast; j++)
					{
						ColliderHit colliderHit = hits[num + j];
						if (colliderHit.instanceID == 0)
						{
							break;
						}
						if (colliderHit.collider.TryGetComponent<EnvironmentVolume>(out var component))
						{
							results[i] |= component.Type;
						}
					}
				}
				hits.Dispose();
			}
		}
	}
}
