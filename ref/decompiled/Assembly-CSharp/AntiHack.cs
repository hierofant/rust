#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using AntiHackJobs;
using BasePlayerJobs;
using ConVar;
using Epic.OnlineServices.Reports;
using Facepunch;
using Facepunch.Rust;
using Oxide.Core;
using Rust;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UtilityJobs;

public static class AntiHack
{
	public struct Batch
	{
		public int PlayerIndex;

		public int Count;

		public bool Force;

		public bool CastVehicleLayer;

		public bool SkipVehicleLayer;
	}

	public struct FlyingBatch
	{
		public int PlayerIndex;

		public int Count;
	}

	private class GroupedLog : Facepunch.Pool.IPooled
	{
		public float firstLogTime;

		public string playerName;

		public AntiHackType antiHackType;

		public string message;

		public Vector3 averagePos;

		public int num;

		public GroupedLog()
		{
		}

		public GroupedLog(string playerName, AntiHackType antiHackType, string message, Vector3 pos)
		{
			SetInitial(playerName, antiHackType, message, pos);
		}

		public void EnterPool()
		{
			firstLogTime = 0f;
			playerName = string.Empty;
			antiHackType = AntiHackType.None;
			averagePos = Vector3.zero;
			num = 0;
		}

		public void LeavePool()
		{
		}

		public void SetInitial(string playerName, AntiHackType antiHackType, string message, Vector3 pos)
		{
			firstLogTime = UnityEngine.Time.unscaledTime;
			this.playerName = playerName;
			this.antiHackType = antiHackType;
			this.message = message;
			averagePos = pos;
			num = 1;
		}

		public bool TryGroup(string playerName, AntiHackType antiHackType, string message, Vector3 pos, float maxDistance)
		{
			if (antiHackType != this.antiHackType || playerName != this.playerName || message != this.message)
			{
				return false;
			}
			if (Vector3.SqrMagnitude(averagePos - pos) > maxDistance * maxDistance)
			{
				return false;
			}
			Vector3 vector = averagePos * num;
			averagePos = (vector + pos) / (num + 1);
			num++;
			return true;
		}
	}

	public struct PlayerState
	{
		public float ViolationLevel;

		public float LastViolationTime;

		public float LastMovementViolationTime;

		public float LastAdminCheatTime;

		public float TickDistancePausetime;

		public float UnparentTime;

		public AntiHackType LastViolationType;
	}

	public struct PlayerNoclipState
	{
		public float VehiclePauseTime;

		public float ForceCastTime;
	}

	public struct PlayerSpeedhackState
	{
		public float PauseTime;

		public float ExtraSpeedTime;

		public float Distance;

		public float ExtraSpeed;
	}

	public struct PlayerFlyhackState
	{
		public Vector3 LastGroundedPosition;

		public float PauseTime;

		public float VerticalDistance;

		public float HorizontalDistance;

		public float LastInAirTime;

		public bool IsInAir;

		public bool IsOnPlayer;
	}

	private const int movement_mask = 1503731969;

	private const int vehicle_mask = 134225920;

	private const int grounded_mask = 1503764737;

	private const int player_mask = 131072;

	private static Collider[] buffer = new Collider[4];

	private static Dictionary<ulong, int> kicks = new Dictionary<ulong, int>();

	private static Dictionary<ulong, int> bans = new Dictionary<ulong, int>();

	private const float LOG_GROUP_SECONDS = 60f;

	private static Queue<GroupedLog> groupedLogs = new Queue<GroupedLog>();

	private static NativeArray<bool> FindIndexWorkBuffer;

	private static NativeList<int> ValidIndexAccum1;

	private static NativeList<int> ValidIndexAccum2;

	private static NativeList<int> InvalidIndices;

	private static NativeList<Vector3> From;

	private static NativeList<Vector3> To;

	private static NativeList<Batch> Batches;

	private static NativeList<int> LayerMasks;

	private static NativeArray<float> PlayerRadii;

	private static NativeList<int> ToOverlapIndices;

	private static NativeList<Matrix4x4> Matrices;

	private static NativeList<Vector3> ToOverlapFrom;

	private static NativeList<Vector3> ToOverlapTo;

	private static NativeList<int> ToOverlapLayerMasks;

	private static NativeList<int> RaycastIndices;

	private static NativeList<RaycastCommand> RaycastRays;

	private static NativeList<SpherecastCommand> RaycastSpheres;

	private static NativeList<int> TraceIndices;

	private static NativeList<RaycastCommand> TraceRays;

	private static NativeList<SpherecastCommand> TraceSpheres;

	private static NativeArray<RaycastHit> RaycastHits;

	private static NativeArray<ColliderHit> ColliderHits;

	private static NativeArray<bool> TerrainIgnoreVolumeHits;

	private static NativeArray<int> QueryToBatchMap;

	private static BufferList<Collider> Colliders;

	public static NativeArray<PlayerState> PlayerStates;

	public static NativeArray<PlayerNoclipState> PlayerNoclipStates;

	public static NativeArray<PlayerSpeedhackState> PlayerSpeedhackStates;

	public static NativeArray<PlayerFlyhackState> PlayerFlyhackStates;

	public static RaycastHit isInsideRayHit;

	private static RaycastHit[] isInsideMeshRaycastHits = new RaycastHit[64];

	public static bool TestNoClipping(BasePlayer ply, Vector3 oldPos, Vector3 newPos, float radius, float backtracking, out Collider col, bool overlapVehicleLayer = false, BaseEntity ignoreEntity = null, bool forceCast = false, bool ignoreChildrenOfIgnoreEntity = false, bool skipVehicles = false)
	{
		int num = 1503731969;
		if (skipVehicles)
		{
			num &= -134225921;
		}
		Vector3 normalized = (newPos - oldPos).normalized;
		Vector3 vector = oldPos - normalized * backtracking;
		float magnitude = (newPos - vector).magnitude;
		Ray ray = new Ray(vector, normalized);
		if (GamePhysics.CheckCapsule(oldPos, newPos, radius, num, QueryTriggerInteraction.Ignore))
		{
			List<Collider> obj = Facepunch.Pool.Get<List<Collider>>();
			GamePhysics.OverlapCapsule(oldPos, newPos, radius, obj, num);
			bool recheck = false;
			bool recheckTerrain = false;
			for (int i = 0; i < obj.Count; i++)
			{
				Collider collider = obj[i];
				if (IsColliderBlocking(collider, ply, forceCast, !overlapVehicleLayer, ignoreEntity, ignoreChildrenOfIgnoreEntity, ref recheck, ref recheckTerrain))
				{
					col = collider;
					Facepunch.Pool.FreeUnmanaged(ref obj);
					return true;
				}
			}
			Facepunch.Pool.FreeUnmanaged(ref obj);
			if (recheck || recheckTerrain)
			{
				if (!recheckTerrain && ignoreEntity == null)
				{
					RaycastHit hitInfo;
					bool result = UnityEngine.Physics.Raycast(ray, out hitInfo, magnitude + radius, num, QueryTriggerInteraction.Ignore) || UnityEngine.Physics.SphereCast(ray, radius, out hitInfo, magnitude, num, QueryTriggerInteraction.Ignore);
					col = hitInfo.collider;
					return result;
				}
				RaycastHit hitInfo2;
				bool result2 = GamePhysics.Trace(ray, 0f, out hitInfo2, magnitude + radius, num, QueryTriggerInteraction.Ignore, ignoreEntity) || GamePhysics.Trace(ray, radius, out hitInfo2, magnitude, num, QueryTriggerInteraction.Ignore, ignoreEntity);
				col = hitInfo2.collider;
				return result2;
			}
		}
		col = null;
		return false;
	}

	private static bool IsColliderBlocking(Collider collider, BasePlayer ply, bool forceCast, bool forceCastVehicles, BaseEntity ignoreEntity, bool ignoreChildrenOfIgnoreEntity, ref bool recheck, ref bool recheckTerrain)
	{
		if (collider is TerrainCollider)
		{
			recheckTerrain = true;
			return false;
		}
		if (((int)collider.excludeLayers & 0x1000) == 4096)
		{
			return false;
		}
		if (forceCastVehicles && ((1 << collider.gameObject.layer) & 0x8002000) > 0)
		{
			recheck = true;
			return false;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(collider);
		if (((1 << collider.gameObject.layer) & 0x2000) > 0)
		{
			if (baseEntity is HotAirBalloon && ply.RecentlyUnparented(5f))
			{
				return false;
			}
			recheck = true;
			return false;
		}
		if (GamePhysics.CompareEntity(baseEntity, ignoreEntity))
		{
			return false;
		}
		if (ignoreChildrenOfIgnoreEntity && (bool)baseEntity && !baseEntity.ShouldAlwaysBlockNoClipChecks() && GamePhysics.CompareEntity(baseEntity.GetRootParentEntity(), ignoreEntity))
		{
			return false;
		}
		if (forceCast)
		{
			recheck = true;
			return false;
		}
		if (baseEntity != null && baseEntity.ShouldUseCastNoClipChecks())
		{
			recheck = true;
			return false;
		}
		if (ply.GetParentEntity() is ElevatorLift)
		{
			recheck = true;
			return false;
		}
		return true;
	}

	public static void TestAreNoClipping(in BasePlayer.PlayerServerStates.ReadOnly playerStates, NativeArray<Vector3>.ReadOnly fromPos, NativeArray<Vector3>.ReadOnly toPos, NativeArray<Batch>.ReadOnly batches, NativeList<int> foundIndices, Span<Collider> foundColls)
	{
		using (TimeWarning.New("TestAreNoClipping"))
		{
			float noclip_backtracking = ConVar.AntiHack.noclip_backtracking;
			float num = BasePlayer.NoClipRadius(ConVar.AntiHack.noclip_margin);
			NativeListEx.Expand(ref LayerMasks, fromPos.Length, copyContents: false);
			BuildLayerMasksJob buildLayerMasksJob = default(BuildLayerMasksJob);
			buildLayerMasksJob.LayerMasks = LayerMasks;
			buildLayerMasksJob.Batches = batches;
			buildLayerMasksJob.DefaultMask = 1503731969;
			buildLayerMasksJob.NoVehicleMask = 1369506049;
			BuildLayerMasksJob jobData = buildLayerMasksJob;
			IJobExtensions.RunByRef(ref jobData);
			NativeArrayEx.Expand(ref PlayerRadii, fromPos.Length, NativeArrayOptions.UninitializedMemory, copyContents: false);
			FillJob<float> fillJob = default(FillJob<float>);
			fillJob.Values = PlayerRadii;
			fillJob.Value = num;
			FillJob<float> jobData2 = fillJob;
			IJobExtensions.RunByRef(ref jobData2);
			NativeArrayEx.Expand(ref TerrainIgnoreVolumeHits, fromPos.Length, NativeArrayOptions.UninitializedMemory, copyContents: false);
			GamePhysics.CheckCapsules(fromPos, toPos, PlayerRadii.AsReadOnly(), LayerMasks.AsReadOnly(), TerrainIgnoreVolumeHits, QueryTriggerInteraction.Ignore, GamePhysics.MasksToValidate.Terrain).Complete();
			NativeListEx.Expand(ref ToOverlapIndices, fromPos.Length, copyContents: false);
			GatherHitIndicesJob gatherHitIndicesJob = default(GatherHitIndicesJob);
			gatherHitIndicesJob.Results = ToOverlapIndices;
			gatherHitIndicesJob.Hits = TerrainIgnoreVolumeHits.GetSubArray(0, fromPos.Length).AsReadOnly();
			GatherHitIndicesJob jobData3 = gatherHitIndicesJob;
			IJobExtensions.RunByRef(ref jobData3);
			if (ToOverlapIndices.IsEmpty)
			{
				return;
			}
			NativeArrayEx.Expand(ref QueryToBatchMap, fromPos.Length, NativeArrayOptions.UninitializedMemory, copyContents: false);
			BuildBatchLookupMapJob buildBatchLookupMapJob = default(BuildBatchLookupMapJob);
			buildBatchLookupMapJob.Lookup = QueryToBatchMap;
			buildBatchLookupMapJob.Batches = batches;
			BuildBatchLookupMapJob jobData4 = buildBatchLookupMapJob;
			IJobExtensions.RunByRef(ref jobData4);
			ToOverlapFrom.Resize(ToOverlapIndices.Length, NativeArrayOptions.UninitializedMemory);
			GatherJob<Vector3> gatherJob = default(GatherJob<Vector3>);
			gatherJob.Results = ToOverlapFrom.AsArray();
			gatherJob.Source = fromPos;
			gatherJob.Indices = ToOverlapIndices.AsReadOnly();
			GatherJob<Vector3> jobData5 = gatherJob;
			IJobExtensions.RunByRef(ref jobData5);
			ToOverlapTo.Resize(ToOverlapIndices.Length, NativeArrayOptions.UninitializedMemory);
			gatherJob = default(GatherJob<Vector3>);
			gatherJob.Results = ToOverlapTo.AsArray();
			gatherJob.Source = toPos;
			gatherJob.Indices = ToOverlapIndices.AsReadOnly();
			GatherJob<Vector3> jobData6 = gatherJob;
			IJobExtensions.RunByRef(ref jobData6);
			ToOverlapLayerMasks.Resize(ToOverlapIndices.Length, NativeArrayOptions.UninitializedMemory);
			GatherJob<int> gatherJob2 = default(GatherJob<int>);
			gatherJob2.Results = ToOverlapLayerMasks.AsArray();
			gatherJob2.Source = LayerMasks.AsReadOnly();
			gatherJob2.Indices = ToOverlapIndices.AsReadOnly();
			GatherJob<int> jobData7 = gatherJob2;
			IJobExtensions.RunByRef(ref jobData7);
			int defaultMaxResultsPerQuery = GamePhysics.DefaultMaxResultsPerQuery;
			NativeArrayEx.Expand(ref ColliderHits, ToOverlapLayerMasks.Length * defaultMaxResultsPerQuery, NativeArrayOptions.UninitializedMemory, copyContents: false);
			GamePhysics.OverlapCapsules(ToOverlapFrom.AsReadOnly(), ToOverlapTo.AsReadOnly(), PlayerRadii.GetSubArray(0, ToOverlapLayerMasks.Length).AsReadOnly(), ToOverlapLayerMasks.AsReadOnly(), ColliderHits, defaultMaxResultsPerQuery, QueryTriggerInteraction.Ignore, GamePhysics.MasksToValidate.Terrain).Complete();
			NativeListEx.Expand(ref TraceIndices, ToOverlapIndices.Length, copyContents: false);
			NativeListEx.Expand(ref TraceRays, ToOverlapIndices.Length, copyContents: false);
			NativeListEx.Expand(ref RaycastIndices, ToOverlapIndices.Length, copyContents: false);
			NativeListEx.Expand(ref RaycastRays, ToOverlapIndices.Length, copyContents: false);
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			using (TimeWarning.New("FilterOverlapResults"))
			{
				bool flag = false;
				for (int i = 0; i < ToOverlapIndices.Length; i++)
				{
					int num2 = ToOverlapIndices[i];
					int index = QueryToBatchMap[num2];
					Batch batch = batches[index];
					if (flag)
					{
						if ((object)foundColls[batch.PlayerIndex] != null)
						{
							continue;
						}
						flag = false;
					}
					BasePlayer ply = objects[batch.PlayerIndex];
					bool force = batch.Force;
					bool castVehicleLayer = batch.CastVehicleLayer;
					bool recheck = false;
					bool recheckTerrain = false;
					Collider collider = null;
					for (int j = 0; j < defaultMaxResultsPerQuery; j++)
					{
						int index2 = i * defaultMaxResultsPerQuery + j;
						ColliderHit colliderHit = ColliderHits[index2];
						if (colliderHit.instanceID == 0)
						{
							break;
						}
						Collider collider2 = colliderHit.collider;
						if (IsColliderBlocking(collider2, ply, force, castVehicleLayer, null, ignoreChildrenOfIgnoreEntity: false, ref recheck, ref recheckTerrain))
						{
							flag = true;
							collider = collider2;
							break;
						}
					}
					int value = batch.PlayerIndex;
					if (flag)
					{
						foundIndices.Add(in value);
						foundColls[value] = collider;
					}
					else if (recheck || recheckTerrain)
					{
						Vector3 vector = ToOverlapFrom[i];
						Vector3 vector2 = ToOverlapTo[i];
						Vector3 normalized = (vector2 - vector).normalized;
						Vector3 vector3 = vector - normalized * noclip_backtracking;
						float magnitude = (vector2 - vector3).magnitude;
						new Ray(vector3, normalized);
						int layerMask = ToOverlapLayerMasks[i];
						QueryParameters queryParameters = new QueryParameters(layerMask, hitMultipleFaces: false, QueryTriggerInteraction.Ignore);
						RaycastCommand value2 = new RaycastCommand(vector3, normalized, queryParameters, magnitude + num);
						if (recheckTerrain)
						{
							TraceIndices.AddNoResize(num2);
							TraceRays.AddNoResize(value2);
						}
						else
						{
							RaycastIndices.AddNoResize(num2);
							RaycastRays.AddNoResize(value2);
						}
					}
				}
			}
			if (!TraceIndices.IsEmpty)
			{
				NativeArrayEx.Expand(ref RaycastHits, TraceIndices.Length * defaultMaxResultsPerQuery, NativeArrayOptions.UninitializedMemory, copyContents: false);
				GamePhysics.TraceRays(TraceRays.AsArray(), RaycastHits, defaultMaxResultsPerQuery, traceWater: false);
				using (TimeWarning.New("GatherRays"))
				{
					int length = 0;
					NativeListEx.Expand(ref TraceSpheres, TraceIndices.Length, copyContents: false);
					for (int k = 0; k < TraceIndices.Length; k++)
					{
						if (!RecordNoclip(RaycastHits[k * defaultMaxResultsPerQuery], TraceIndices[k], batches, foundIndices, foundColls))
						{
							TraceIndices[length++] = TraceIndices[k];
							RaycastCommand raycastCommand = TraceRays[k];
							SpherecastCommand value3 = new SpherecastCommand(raycastCommand.from, num, raycastCommand.direction, raycastCommand.queryParameters, raycastCommand.distance - num);
							TraceSpheres.Add(in value3);
						}
					}
					TraceIndices.Resize(length, NativeArrayOptions.UninitializedMemory);
				}
				if (!TraceIndices.IsEmpty)
				{
					GamePhysics.TraceSpheres(TraceSpheres.AsArray(), RaycastHits, defaultMaxResultsPerQuery, traceWater: false);
					using (TimeWarning.New("GatherSpheres"))
					{
						for (int l = 0; l < TraceIndices.Length; l++)
						{
							RecordNoclip(RaycastHits[l * defaultMaxResultsPerQuery], TraceIndices[l], batches, foundIndices, foundColls);
						}
					}
				}
			}
			if (RaycastIndices.IsEmpty)
			{
				return;
			}
			if (!TraceIndices.IsEmpty)
			{
				using (TimeWarning.New("SkipDupeRaycasts"))
				{
					int length2 = 0;
					for (int m = 0; m < RaycastIndices.Length; m++)
					{
						int num3 = RaycastIndices[m];
						int index3 = QueryToBatchMap[num3];
						if ((object)foundColls[batches[index3].PlayerIndex] == null)
						{
							int index4 = length2++;
							RaycastIndices[index4] = num3;
							RaycastRays[index4] = RaycastRays[m];
						}
					}
					RaycastIndices.Resize(length2, NativeArrayOptions.UninitializedMemory);
					RaycastRays.Resize(length2, NativeArrayOptions.UninitializedMemory);
				}
			}
			if (!RaycastIndices.IsEmpty)
			{
				using (TimeWarning.New("RayCasts"))
				{
					NativeArrayEx.Expand(ref RaycastHits, RaycastIndices.Length, NativeArrayOptions.UninitializedMemory, copyContents: false);
					RaycastCommand.ScheduleBatch(RaycastRays.AsArray(), RaycastHits, 1).Complete();
					int length3 = 0;
					NativeListEx.Expand(ref RaycastSpheres, RaycastIndices.Length, copyContents: false);
					for (int n = 0; n < RaycastIndices.Length; n++)
					{
						if (!RecordNoclip(RaycastHits[n], RaycastIndices[n], batches, foundIndices, foundColls))
						{
							RaycastIndices[length3++] = RaycastIndices[n];
							RaycastCommand raycastCommand2 = RaycastRays[n];
							SpherecastCommand value4 = new SpherecastCommand(raycastCommand2.from, num, raycastCommand2.direction, raycastCommand2.queryParameters, raycastCommand2.distance - num);
							RaycastSpheres.AddNoResize(value4);
						}
					}
					RaycastIndices.Resize(length3, NativeArrayOptions.UninitializedMemory);
				}
			}
			if (RaycastIndices.IsEmpty)
			{
				return;
			}
			using (TimeWarning.New("SphereCasts"))
			{
				SpherecastCommand.ScheduleBatch(RaycastSpheres.AsArray(), RaycastHits, 1).Complete();
				for (int num4 = 0; num4 < RaycastIndices.Length; num4++)
				{
					RecordNoclip(RaycastHits[num4], RaycastIndices[num4], batches, foundIndices, foundColls);
				}
			}
		}
		static bool RecordNoclip(RaycastHit hit, int queryIndex, NativeArray<Batch>.ReadOnly batches, NativeList<int> foundIndices, Span<Collider> foundColls)
		{
			bool num5 = hit.colliderInstanceID != 0;
			if (num5)
			{
				int index5 = QueryToBatchMap[queryIndex];
				int value5 = batches[index5].PlayerIndex;
				if ((object)foundColls[value5] == null)
				{
					foundIndices.Add(in value5);
					foundColls[value5] = hit.collider;
				}
			}
			return num5;
		}
	}

	public static void Cycle()
	{
		float num = UnityEngine.Time.unscaledTime - 60f;
		if (groupedLogs.Count <= 0)
		{
			return;
		}
		GroupedLog groupedLog = groupedLogs.Peek();
		while (groupedLog.firstLogTime <= num)
		{
			GroupedLog obj = groupedLogs.Dequeue();
			LogToConsole(obj.playerName, obj.antiHackType, $"{obj.message} (x{obj.num})", obj.averagePos);
			Facepunch.Pool.Free(ref obj);
			if (groupedLogs.Count != 0)
			{
				groupedLog = groupedLogs.Peek();
				continue;
			}
			break;
		}
	}

	public static void ResetTimer(BasePlayer ply)
	{
		ref PlayerState reference = ref ((Span<PlayerState>)PlayerStates)[ply.ActivePlayerInd];
		reference.LastViolationTime = UnityEngine.Time.realtimeSinceStartup;
		reference.LastMovementViolationTime = UnityEngine.Time.realtimeSinceStartup;
	}

	public static bool ShouldIgnore(BasePlayer ply)
	{
		using (TimeWarning.New("AntiHack.ShouldIgnore"))
		{
			ref PlayerState reference = ref ((Span<PlayerState>)PlayerStates)[ply.ActivePlayerInd];
			if (ply.IsFlying)
			{
				reference.LastAdminCheatTime = UnityEngine.Time.realtimeSinceStartup;
			}
			else if ((ply.IsAdmin || ply.IsDeveloper) && reference.LastAdminCheatTime == 0f)
			{
				reference.LastAdminCheatTime = UnityEngine.Time.realtimeSinceStartup;
			}
			if (ply.IsAdmin)
			{
				if (ConVar.AntiHack.userlevel < 1)
				{
					return true;
				}
				if (ConVar.AntiHack.admincheat && ply.UsedAdminCheat())
				{
					return true;
				}
			}
			if (ply.IsDeveloper)
			{
				if (ConVar.AntiHack.userlevel < 2)
				{
					return true;
				}
				if (ConVar.AntiHack.admincheat && ply.UsedAdminCheat())
				{
					return true;
				}
			}
			if (ply.IsSpectating())
			{
				return true;
			}
			if (ply.isInvisible)
			{
				return true;
			}
			return false;
		}
	}

	public static void InitInternalState(int initCap)
	{
		DisposeInternalState();
		FindIndexWorkBuffer = new NativeArray<bool>(initCap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		ValidIndexAccum1 = new NativeList<int>(initCap, Allocator.Persistent);
		ValidIndexAccum2 = new NativeList<int>(initCap, Allocator.Persistent);
		InvalidIndices = new NativeList<int>(initCap, Allocator.Persistent);
		From = new NativeList<Vector3>(initCap, Allocator.Persistent);
		To = new NativeList<Vector3>(initCap, Allocator.Persistent);
		Batches = new NativeList<Batch>(initCap, Allocator.Persistent);
		LayerMasks = new NativeList<int>(initCap, Allocator.Persistent);
		PlayerRadii = new NativeArray<float>(initCap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		ToOverlapIndices = new NativeList<int>(initCap, Allocator.Persistent);
		Matrices = new NativeList<Matrix4x4>(initCap, Allocator.Persistent);
		ToOverlapFrom = new NativeList<Vector3>(initCap, Allocator.Persistent);
		ToOverlapTo = new NativeList<Vector3>(initCap, Allocator.Persistent);
		ToOverlapLayerMasks = new NativeList<int>(initCap, Allocator.Persistent);
		RaycastIndices = new NativeList<int>(initCap, Allocator.Persistent);
		RaycastRays = new NativeList<RaycastCommand>(initCap, Allocator.Persistent);
		RaycastSpheres = new NativeList<SpherecastCommand>(initCap, Allocator.Persistent);
		TraceIndices = new NativeList<int>(initCap, Allocator.Persistent);
		TraceRays = new NativeList<RaycastCommand>(initCap, Allocator.Persistent);
		TraceSpheres = new NativeList<SpherecastCommand>(initCap, Allocator.Persistent);
		RaycastHits = new NativeArray<RaycastHit>(initCap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		ColliderHits = new NativeArray<ColliderHit>(initCap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		TerrainIgnoreVolumeHits = new NativeArray<bool>(initCap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		QueryToBatchMap = new NativeArray<int>(initCap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Colliders = new BufferList<Collider>(initCap);
		PlayerStates = new NativeArray<PlayerState>(initCap, Allocator.Persistent);
		PlayerNoclipStates = new NativeArray<PlayerNoclipState>(initCap, Allocator.Persistent);
		PlayerSpeedhackStates = new NativeArray<PlayerSpeedhackState>(initCap, Allocator.Persistent);
		PlayerFlyhackStates = new NativeArray<PlayerFlyhackState>(initCap, Allocator.Persistent);
	}

	public static void DisposeInternalState()
	{
		NativeArrayEx.SafeDispose(ref FindIndexWorkBuffer);
		NativeListEx.SafeDispose(ref ValidIndexAccum1);
		NativeListEx.SafeDispose(ref ValidIndexAccum2);
		NativeListEx.SafeDispose(ref InvalidIndices);
		NativeListEx.SafeDispose(ref From);
		NativeListEx.SafeDispose(ref To);
		NativeListEx.SafeDispose(ref Batches);
		NativeListEx.SafeDispose(ref LayerMasks);
		NativeArrayEx.SafeDispose(ref PlayerRadii);
		NativeListEx.SafeDispose(ref ToOverlapIndices);
		NativeListEx.SafeDispose(ref Matrices);
		NativeListEx.SafeDispose(ref ToOverlapFrom);
		NativeListEx.SafeDispose(ref ToOverlapTo);
		NativeListEx.SafeDispose(ref ToOverlapLayerMasks);
		NativeListEx.SafeDispose(ref RaycastIndices);
		NativeListEx.SafeDispose(ref RaycastRays);
		NativeListEx.SafeDispose(ref RaycastSpheres);
		NativeListEx.SafeDispose(ref TraceIndices);
		NativeListEx.SafeDispose(ref TraceRays);
		NativeListEx.SafeDispose(ref TraceSpheres);
		NativeArrayEx.SafeDispose(ref RaycastHits);
		NativeArrayEx.SafeDispose(ref ColliderHits);
		NativeArrayEx.SafeDispose(ref TerrainIgnoreVolumeHits);
		NativeArrayEx.SafeDispose(ref QueryToBatchMap);
		Colliders = null;
		NativeArrayEx.SafeDispose(ref PlayerStates);
		NativeArrayEx.SafeDispose(ref PlayerNoclipStates);
		NativeArrayEx.SafeDispose(ref PlayerSpeedhackStates);
		NativeArrayEx.SafeDispose(ref PlayerFlyhackStates);
	}

	public static void OnPlayerAddedToCache(BasePlayer player, StableObjectArray<BasePlayer> cache, int index)
	{
		NativeArrayEx.Expand(ref PlayerStates, cache.Capacity);
		PlayerStates[index] = default(PlayerState);
		NativeArrayEx.Expand(ref PlayerNoclipStates, cache.Capacity);
		PlayerNoclipStates[index] = default(PlayerNoclipState);
		NativeArrayEx.Expand(ref PlayerSpeedhackStates, cache.Capacity);
		PlayerSpeedhackStates[index] = default(PlayerSpeedhackState);
		NativeArrayEx.Expand(ref PlayerFlyhackStates, cache.Capacity);
		ref PlayerFlyhackState reference = ref ((Span<PlayerFlyhackState>)PlayerFlyhackStates)[index];
		reference = default(PlayerFlyhackState);
		reference.LastGroundedPosition = player.transform.position;
	}

	public static void OnPlayerRemovedFromCache(BasePlayer player, int movedFrom, int movedTo)
	{
		if (movedTo != movedFrom)
		{
			Debug.Assert(movedTo < movedFrom, "Unexpected swap indices, expecting to swap from end to earlier in range!");
			PlayerStates[movedTo] = PlayerStates[movedFrom];
			PlayerNoclipStates[movedTo] = PlayerNoclipStates[movedFrom];
			PlayerSpeedhackStates[movedTo] = PlayerSpeedhackStates[movedFrom];
			PlayerFlyhackStates[movedTo] = PlayerFlyhackStates[movedFrom];
		}
		BasePlayer.ResetAntiHack(player, PlayerStates, PlayerNoclipStates, PlayerSpeedhackStates, PlayerFlyhackStates);
	}

	internal static void ValidateMoves(in BasePlayer.PlayerServerStates.ReadOnly playerStates, NativeArray<int>.ReadOnly indices, NativeArray<BasePlayer.PositionChange> results)
	{
		using (TimeWarning.New("AntiHack.ValidateMoves"))
		{
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			NativeListEx.Expand(ref ValidIndexAccum1, indices.Length, copyContents: false);
			NativeListEx.Expand(ref ValidIndexAccum2, indices.Length, copyContents: false);
			ValidIndexAccum1.Clear();
			ValidIndexAccum2.Clear();
			using (TimeWarning.New("ShouldIgnore"))
			{
				foreach (int item in indices)
				{
					int value = item;
					if (ShouldIgnore(objects[value]))
					{
						results[value] = BasePlayer.PositionChange.Valid;
					}
					else
					{
						ValidIndexAccum1.Add(in value);
					}
				}
			}
			NativeListEx.Expand(ref InvalidIndices, ValidIndexAccum1.Length, copyContents: false);
			InvalidIndices.Clear();
			if (Colliders.Capacity < objects.Length)
			{
				Colliders.Resize(objects.Length);
			}
			AreNoClipping(in playerStates, PlayerNoclipStates, ValidIndexAccum1.AsReadOnly(), InvalidIndices, Colliders.Buffer);
			NativeArrayEx.Expand(ref FindIndexWorkBuffer, objects.Length, NativeArrayOptions.UninitializedMemory, copyContents: false);
			FindValidIndicesJob findValidIndicesJob = default(FindValidIndicesJob);
			findValidIndicesJob.ValidIndices = ValidIndexAccum2;
			findValidIndicesJob.WorkBuffer = FindIndexWorkBuffer;
			findValidIndicesJob.InvalidIndices = InvalidIndices.AsReadOnly();
			findValidIndicesJob.AllIndices = ValidIndexAccum1.AsReadOnly();
			FindValidIndicesJob jobData = findValidIndicesJob;
			IJobExtensions.RunByRef(ref jobData);
			TickInterpolatorCache.ReadOnlyState tickCache = playerStates.TickCache;
			using (TimeWarning.New("NoClipRejections"))
			{
				foreach (int invalidIndex in InvalidIndices)
				{
					if (playerStates.TickDeltaTime[invalidIndex] > ConVar.AntiHack.maxdeltatime)
					{
						results[invalidIndex] = BasePlayer.PositionChange.Invalid;
						continue;
					}
					BasePlayer obj = objects[invalidIndex];
					Vector3 startPoint = TickInterpolatorCache.GetStartPoint(tickCache, invalidIndex);
					Vector3 endPoint = TickInterpolatorCache.GetEndPoint(tickCache, invalidIndex);
					TickInterpolatorCache.PlayerInfo playerInfo = tickCache.Infos[invalidIndex];
					Facepunch.Rust.Analytics.Azure.OnNoclipViolation(obj, startPoint, endPoint, playerInfo.Count, Colliders[invalidIndex]);
					AddViolation(obj, AntiHackType.NoClip, ConVar.AntiHack.noclip_penalty * playerInfo.Length, Colliders[invalidIndex].gameObject);
					if (ConVar.AntiHack.noclip_reject)
					{
						results[invalidIndex] = BasePlayer.PositionChange.Invalid;
					}
				}
			}
			Array.Clear(Colliders.Buffer, 0, Colliders.Capacity);
			NativeList<int> validIndexAccum = ValidIndexAccum2;
			NativeList<int> validIndexAccum2 = ValidIndexAccum1;
			ValidIndexAccum1 = validIndexAccum;
			ValidIndexAccum2 = validIndexAccum2;
			ValidIndexAccum2.Clear();
			InvalidIndices.Clear();
			if (ValidIndexAccum1.Length > 0)
			{
				using NativeArray<bool> results2 = new NativeArray<bool>(ValidIndexAccum1.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				AreSpeeding(in playerStates, PlayerSpeedhackStates, ValidIndexAccum1.AsReadOnly(), results2);
				for (int i = 0; i < ValidIndexAccum1.Length; i++)
				{
					int value2 = ValidIndexAccum1[i];
					if (results2[i])
					{
						InvalidIndices.Add(in value2);
					}
					else
					{
						ValidIndexAccum2.Add(in value2);
					}
				}
			}
			using (TimeWarning.New("IsSpeedingRejections"))
			{
				foreach (int invalidIndex2 in InvalidIndices)
				{
					if (playerStates.TickDeltaTime[invalidIndex2] > ConVar.AntiHack.maxdeltatime)
					{
						results[invalidIndex2] = BasePlayer.PositionChange.Invalid;
						continue;
					}
					BasePlayer obj2 = objects[invalidIndex2];
					Vector3 startPoint2 = TickInterpolatorCache.GetStartPoint(tickCache, invalidIndex2);
					Vector3 endPoint2 = TickInterpolatorCache.GetEndPoint(tickCache, invalidIndex2);
					TickInterpolatorCache.PlayerInfo playerInfo2 = tickCache.Infos[invalidIndex2];
					Facepunch.Rust.Analytics.Azure.OnSpeedhackViolation(obj2, startPoint2, endPoint2, playerInfo2.Count);
					AddViolation(obj2, AntiHackType.SpeedHack, ConVar.AntiHack.speedhack_penalty * playerInfo2.Length);
					if (ConVar.AntiHack.speedhack_reject)
					{
						results[invalidIndex2] = BasePlayer.PositionChange.Invalid;
					}
				}
			}
			NativeList<int> validIndexAccum3 = ValidIndexAccum2;
			validIndexAccum2 = ValidIndexAccum1;
			ValidIndexAccum1 = validIndexAccum3;
			ValidIndexAccum2 = validIndexAccum2;
			ValidIndexAccum2.Clear();
			InvalidIndices.Clear();
			if (ValidIndexAccum1.Length > 0)
			{
				using NativeArray<bool> results3 = new NativeArray<bool>(ValidIndexAccum1.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				AreFlying(in playerStates, PlayerStates.AsReadOnly(), PlayerFlyhackStates, ValidIndexAccum1.AsReadOnly(), results3);
				for (int j = 0; j < ValidIndexAccum1.Length; j++)
				{
					int value3 = ValidIndexAccum1[j];
					if (results3[j])
					{
						InvalidIndices.Add(in value3);
					}
					else
					{
						ValidIndexAccum2.Add(in value3);
					}
				}
			}
			using (TimeWarning.New("IsFlyingRejections"))
			{
				foreach (int invalidIndex3 in InvalidIndices)
				{
					int value4 = invalidIndex3;
					if (playerStates.TickDeltaTime[value4] > ConVar.AntiHack.maxdeltatime)
					{
						results[value4] = BasePlayer.PositionChange.Invalid;
						continue;
					}
					BasePlayer basePlayer = objects[value4];
					Vector3 startPoint3 = TickInterpolatorCache.GetStartPoint(tickCache, value4);
					Vector3 endPoint3 = TickInterpolatorCache.GetEndPoint(tickCache, value4);
					TickInterpolatorCache.PlayerInfo playerInfo3 = tickCache.Infos[value4];
					Facepunch.Rust.Analytics.Azure.OnFlyhackViolation(basePlayer, startPoint3, endPoint3, playerInfo3.Count);
					AddViolation(basePlayer, AntiHackType.FlyHack, ConVar.AntiHack.flyhack_penalty * playerInfo3.Length);
					if (!ConVar.AntiHack.flyhack_reject)
					{
						continue;
					}
					results[value4] = BasePlayer.PositionChange.Invalid;
					Vector3 lastGroundedPosition = PlayerFlyhackStates[value4].LastGroundedPosition;
					if (lastGroundedPosition == default(Vector3) && basePlayer.IsConnected)
					{
						ValidIndexAccum2.Add(in value4);
					}
					else if (Vector3.Distance(lastGroundedPosition, basePlayer.transform.position) <= 10f)
					{
						Collider col;
						bool num = TestNoClipping(basePlayer, basePlayer.transform.position, lastGroundedPosition, BasePlayer.NoClipRadius(ConVar.AntiHack.noclip_margin), ConVar.AntiHack.noclip_backtracking, out col);
						Vector3 start = lastGroundedPosition + new Vector3(0f, BasePlayer.GetRadius(), 0f);
						Vector3 end = lastGroundedPosition + new Vector3(0f, basePlayer.GetHeight() - BasePlayer.GetRadius(), 0f);
						if (!num && !UnityEngine.Physics.CheckCapsule(start, end, BasePlayer.GetRadius(), 1537286401))
						{
							basePlayer.MovePosition(lastGroundedPosition);
							basePlayer.ClientRPC(RpcTarget.Player("ForcePositionTo", basePlayer), basePlayer.transform.position);
							((Span<PlayerState>)PlayerStates)[basePlayer.ActivePlayerInd].ViolationLevel = 0f;
						}
					}
				}
			}
			NativeList<int> validIndexAccum4 = ValidIndexAccum2;
			validIndexAccum2 = ValidIndexAccum1;
			ValidIndexAccum1 = validIndexAccum4;
			ValidIndexAccum2 = validIndexAccum2;
			ValidIndexAccum2.Clear();
			InvalidIndices.Clear();
			using (TimeWarning.New("TickOverflowValidation"))
			{
				foreach (int item2 in ValidIndexAccum1)
				{
					int value5 = item2;
					BasePlayer basePlayer2 = objects[value5];
					if (playerStates.TickDeltaTime[value5] < ConVar.AntiHack.tick_buffer_server_lag_threshold && ConVar.AntiHack.tick_buffer_preventions && (float)basePlayer2.rawTickCount >= ConVar.AntiHack.tick_buffer_reject_threshold * (float)Player.tickrate_cl)
					{
						Log(basePlayer2, AntiHackType.Ticks, $"Player had too many ticks buffered ({basePlayer2.rawTickCount})", logToAnalytics: false);
						Vector3 startPoint4 = TickInterpolatorCache.GetStartPoint(tickCache, value5);
						Vector3 endPoint4 = TickInterpolatorCache.GetEndPoint(tickCache, value5);
						Facepunch.Rust.Analytics.Azure.OnTickViolation(basePlayer2, startPoint4, endPoint4, tickCache.Infos[value5].Count);
						results[value5] = BasePlayer.PositionChange.Invalid;
					}
					else
					{
						ValidIndexAccum2.Add(in value5);
					}
				}
			}
			NativeList<int> validIndexAccum5 = ValidIndexAccum2;
			validIndexAccum2 = ValidIndexAccum1;
			ValidIndexAccum1 = validIndexAccum5;
			ValidIndexAccum2 = validIndexAccum2;
			ValidIndexAccum2.Clear();
			using (TimeWarning.New("MarkPositionsValid"))
			{
				foreach (int item3 in ValidIndexAccum1)
				{
					results[item3] = BasePlayer.PositionChange.Valid;
				}
			}
		}
	}

	public static void ValidateAgainstTerrain(in BasePlayer.PlayerServerStates.ReadOnly playerStates)
	{
		using (TimeWarning.New("ValidateAgainstTerrain"))
		{
			int num = UnityEngine.Time.frameCount % ConVar.AntiHack.terrain_timeslice;
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			for (int i = 0; i < objects.Length; i++)
			{
				BasePlayer basePlayer = objects[i];
				int num2 = (int)(basePlayer.net.ID.Value % (ulong)ConVar.AntiHack.terrain_timeslice);
				if (num == num2 && !ShouldIgnore(basePlayer))
				{
					bool flag = false;
					if (IsInsideTerrain(basePlayer))
					{
						flag = true;
						AddViolation(basePlayer, AntiHackType.InsideTerrain, ConVar.AntiHack.terrain_penalty);
					}
					else if (ConVar.AntiHack.terrain_check_geometry && IsInsideMesh(basePlayer.eyes.position))
					{
						flag = true;
						AddViolation(basePlayer, AntiHackType.InsideGeometry, ConVar.AntiHack.terrain_penalty);
						Log(basePlayer, AntiHackType.InsideGeometry, "Seems to be clipped inside " + isInsideRayHit.collider.name);
					}
					if (flag && ConVar.AntiHack.terrain_kill)
					{
						Facepunch.Rust.Analytics.Azure.OnTerrainHackViolation(basePlayer);
						basePlayer.Hurt(1000f, DamageType.Suicide, basePlayer, useProtection: false);
					}
				}
			}
		}
	}

	public static void ValidateEyeHistory(BasePlayer ply)
	{
		using (TimeWarning.New("AntiHack.ValidateEyeHistory"))
		{
			for (int i = 0; i < ply.eyeHistory.Count; i++)
			{
				Vector3 vector = ply.eyeHistory[i];
				if (ply.tickHistory.Distance(ply, vector) > ConVar.AntiHack.eye_history_forgiveness)
				{
					AddViolation(ply, AntiHackType.EyeHack, ConVar.AntiHack.eye_history_penalty);
					Facepunch.Rust.Analytics.Azure.OnEyehackViolation(ply, vector);
				}
			}
			ply.eyeHistory.Clear();
		}
	}

	public static bool IsInsideTerrain(BasePlayer ply)
	{
		if (ply.IsInTutorial || DeepSeaManager.IsInsideDeepSea(ply))
		{
			return false;
		}
		return TestInsideTerrain(ply.transform.position);
	}

	public static bool TestInsideTerrain(Vector3 pos)
	{
		using (TimeWarning.New("AntiHack.TestInsideTerrain"))
		{
			if (!TerrainMeta.TerrainRenderer)
			{
				return false;
			}
			if (!TerrainMeta.HeightMap)
			{
				return false;
			}
			if (!TerrainMeta.Collision)
			{
				return false;
			}
			float terrain_padding = ConVar.AntiHack.terrain_padding;
			float height = TerrainMeta.HeightMap.GetHeight(pos);
			if (pos.y > height - terrain_padding)
			{
				return false;
			}
			float num = TerrainMeta.SampleTerrainMeshHeight(pos);
			if (pos.y > num - terrain_padding)
			{
				return false;
			}
			if (TerrainMeta.Collision.GetIgnore(pos))
			{
				return false;
			}
			return true;
		}
	}

	public static void TestInsideTerrain(NativeArray<Vector3>.ReadOnly posi, NativeArray<bool> results)
	{
		using (TimeWarning.New("AntiHack.TestInsideTerrain"))
		{
			if (!TerrainMeta.TerrainRenderer || !TerrainMeta.HeightMap || !TerrainMeta.Collision)
			{
				for (int i = 0; i < results.Length; i++)
				{
					results[i] = false;
				}
				return;
			}
			NativeArray<float> results2 = new NativeArray<float>(posi.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			JobHandle heights = TerrainMeta.HeightMap.GetHeights(posi, results2);
			JobHandle.ScheduleBatchedJobs();
			NativeArray<float> heights2 = new NativeArray<float>(posi.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			TerrainMeta.SampleTerrainMeshHeights(posi, heights2);
			heights.Complete();
			NativeList<int> indicesToCheck = new NativeList<int>(posi.Length, Allocator.TempJob);
			NativeArray<Vector3> posiToCheck = new NativeArray<Vector3>(posi.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			NativeArray<float> radiiToCheck = new NativeArray<float>(posi.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			InsideTerrainHeightsChecksJob insideTerrainHeightsChecksJob = default(InsideTerrainHeightsChecksJob);
			insideTerrainHeightsChecksJob.Results = results;
			insideTerrainHeightsChecksJob.IndicesToCheck = indicesToCheck;
			insideTerrainHeightsChecksJob.PosiToCheck = posiToCheck;
			insideTerrainHeightsChecksJob.RadiiToCheck = radiiToCheck;
			insideTerrainHeightsChecksJob.Posi = posi;
			insideTerrainHeightsChecksJob.HeightMapHeights = results2.AsReadOnly();
			insideTerrainHeightsChecksJob.TerrainHeights = heights2.AsReadOnly();
			insideTerrainHeightsChecksJob.TerrainPadding = ConVar.AntiHack.terrain_padding;
			insideTerrainHeightsChecksJob.RadiusToCheck = 0.01f;
			InsideTerrainHeightsChecksJob jobData = insideTerrainHeightsChecksJob;
			IJobExtensions.RunByRef(ref jobData);
			results2.Dispose();
			heights2.Dispose();
			if (!indicesToCheck.IsEmpty)
			{
				NativeArray<Vector3> subArray = posiToCheck.GetSubArray(0, indicesToCheck.Length);
				NativeArray<float> subArray2 = radiiToCheck.GetSubArray(0, indicesToCheck.Length);
				NativeArray<bool> results3 = new NativeArray<bool>(indicesToCheck.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				TerrainMeta.Collision.GetIgnore(subArray.AsReadOnly(), subArray2.AsReadOnly(), results3);
				ScatterInvertedBool scatterInvertedBool = default(ScatterInvertedBool);
				scatterInvertedBool.To = results;
				scatterInvertedBool.From = results3.AsReadOnly();
				scatterInvertedBool.Indices = indicesToCheck.AsReadOnly();
				ScatterInvertedBool jobData2 = scatterInvertedBool;
				IJobExtensions.RunByRef(ref jobData2);
				results3.Dispose();
			}
			radiiToCheck.Dispose();
			posiToCheck.Dispose();
			indicesToCheck.Dispose();
		}
	}

	public static bool IsInsideMesh(Vector3 pos)
	{
		if (ConVar.AntiHack.mesh_inside_check_distance <= 0f)
		{
			return false;
		}
		bool queriesHitBackfaces = UnityEngine.Physics.queriesHitBackfaces;
		if (ConVar.AntiHack.use_legacy_mesh_inside_check)
		{
			UnityEngine.Physics.queriesHitBackfaces = true;
			if (UnityEngine.Physics.Raycast(pos, Vector3.up, out isInsideRayHit, ConVar.AntiHack.mesh_inside_check_distance, 65536))
			{
				UnityEngine.Physics.queriesHitBackfaces = queriesHitBackfaces;
				return Vector3.Dot(Vector3.up, isInsideRayHit.normal) > 0f;
			}
			UnityEngine.Physics.queriesHitBackfaces = queriesHitBackfaces;
			return false;
		}
		UnityEngine.Physics.queriesHitBackfaces = true;
		int num = UnityEngine.Physics.RaycastNonAlloc(pos, Vector3.up, isInsideMeshRaycastHits, ConVar.AntiHack.mesh_inside_check_distance, 65536);
		UnityEngine.Physics.queriesHitBackfaces = queriesHitBackfaces;
		SortHitsByDistance(isInsideMeshRaycastHits, num);
		Collider collider = null;
		for (int i = 0; i < num; i++)
		{
			RaycastHit raycastHit = isInsideMeshRaycastHits[i];
			if (raycastHit.collider.TryGetComponent<ColliderInfo>(out var component) && component.HasFlag(ColliderInfo.Flags.AllowBuildInsideMesh))
			{
				continue;
			}
			if (Vector3.Dot(Vector3.up, raycastHit.normal) > 0f)
			{
				if (collider != raycastHit.collider)
				{
					isInsideRayHit = raycastHit;
					return true;
				}
			}
			else
			{
				collider = raycastHit.collider;
			}
		}
		return false;
	}

	public static void AreInsideMesh(NativeArray<Vector3>.ReadOnly posi, NativeArray<bool> results)
	{
		if (ConVar.AntiHack.mesh_inside_check_distance <= 0f)
		{
			for (int i = 0; i < results.Length; i++)
			{
				results[i] = false;
			}
			return;
		}
		NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(posi.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		GenerateInsideMeshCommandsJob generateInsideMeshCommandsJob = default(GenerateInsideMeshCommandsJob);
		generateInsideMeshCommandsJob.Commands = commands;
		generateInsideMeshCommandsJob.Posi = posi;
		generateInsideMeshCommandsJob.Distance = ConVar.AntiHack.mesh_inside_check_distance;
		GenerateInsideMeshCommandsJob jobData = generateInsideMeshCommandsJob;
		int batchSize = ThreadUtils.GetBatchSize(posi.Length);
		IJobForExtensions.ScheduleParallel(jobData, posi.Length, batchSize, default(JobHandle)).Complete();
		NativeArray<RaycastHit> results2 = new NativeArray<RaycastHit>(posi.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		int batchSize2 = ThreadUtils.GetBatchSize(commands.Length);
		RaycastCommand.ScheduleBatch(commands, results2, batchSize2).Complete();
		commands.Dispose();
		CheckInsideMeshHitsJob jobData2 = default(CheckInsideMeshHitsJob);
		jobData2.Results = results;
		jobData2.Hits = results2.AsReadOnly();
		IJobForExtensions.ScheduleParallel(jobData2, posi.Length, batchSize, default(JobHandle)).Complete();
		results2.Dispose();
	}

	public static void AreInsideMesh(NativeArray<Vector3>.ReadOnly posi, NativeArray<RaycastHit> hits)
	{
		if (ConVar.AntiHack.mesh_inside_check_distance <= 0f)
		{
			for (int i = 0; i < hits.Length; i++)
			{
				hits[i] = default(RaycastHit);
			}
			return;
		}
		NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(posi.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		GenerateInsideMeshCommandsJob generateInsideMeshCommandsJob = default(GenerateInsideMeshCommandsJob);
		generateInsideMeshCommandsJob.Commands = commands;
		generateInsideMeshCommandsJob.Posi = posi;
		generateInsideMeshCommandsJob.Distance = ConVar.AntiHack.mesh_inside_check_distance;
		GenerateInsideMeshCommandsJob jobData = generateInsideMeshCommandsJob;
		int batchSize = ThreadUtils.GetBatchSize(posi.Length);
		IJobForExtensions.ScheduleParallel(jobData, posi.Length, batchSize, default(JobHandle)).Complete();
		int batchSize2 = ThreadUtils.GetBatchSize(commands.Length);
		RaycastCommand.ScheduleBatch(commands, hits, batchSize2).Complete();
		commands.Dispose();
		FilterInsideMeshHitsJob jobData2 = default(FilterInsideMeshHitsJob);
		jobData2.Hits = hits;
		IJobForExtensions.ScheduleParallel(jobData2, posi.Length, batchSize, default(JobHandle)).Complete();
	}

	private static void SortHitsByDistance(RaycastHit[] hits, int maxLength)
	{
		for (int i = 0; i < maxLength - 1; i++)
		{
			int num = i;
			for (int j = i + 1; j < maxLength; j++)
			{
				if (hits[j].distance < hits[num].distance)
				{
					num = j;
				}
			}
			if (num != i)
			{
				RaycastHit raycastHit = hits[i];
				hits[i] = hits[num];
				hits[num] = raycastHit;
			}
		}
	}

	public static void AreNoClipping(in BasePlayer.PlayerServerStates.ReadOnly playerStates, NativeArray<PlayerNoclipState> noclipStates, NativeArray<int>.ReadOnly indices, NativeList<int> foundIndices, Span<Collider> colliders)
	{
		using (TimeWarning.New("AntiHack.AreNoClipping"))
		{
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			Span<PlayerNoclipState> span = noclipStates;
			foreach (int item in indices)
			{
				float num = playerStates.TickDeltaTime[item];
				ref PlayerNoclipState reference = ref span[item];
				reference.VehiclePauseTime = Mathf.Max(0f, reference.VehiclePauseTime - num);
				reference.ForceCastTime = Mathf.Max(0f, reference.ForceCastTime - num);
			}
			if (ConVar.AntiHack.noclip_protection <= 0)
			{
				return;
			}
			int num2 = Mathf.Max(ConVar.AntiHack.noclip_maxsteps, 1);
			using (TimeWarning.New("GatherBatches"))
			{
				NativeListEx.Expand(ref ToOverlapIndices, indices.Length, copyContents: false);
				GatherPlayersWithTicksJob gatherPlayersWithTicksJob = default(GatherPlayersWithTicksJob);
				gatherPlayersWithTicksJob.ValidIndices = ToOverlapIndices;
				gatherPlayersWithTicksJob.TickCache = playerStates.TickCache;
				gatherPlayersWithTicksJob.Indices = indices;
				GatherPlayersWithTicksJob jobData = gatherPlayersWithTicksJob;
				IJobExtensions.RunByRef(ref jobData);
				using (TimeWarning.New("GatherPlayerInfo"))
				{
					NativeListEx.Expand(ref Batches, ToOverlapIndices.Length, copyContents: false);
					NativeListEx.Expand(ref Matrices, ToOverlapIndices.Length, copyContents: false);
					foreach (int toOverlapIndex in ToOverlapIndices)
					{
						BasePlayer basePlayer = objects[toOverlapIndex];
						Transform parent = basePlayer.transform.parent;
						Matrix4x4 value = ((parent == null) ? Matrix4x4.zero : parent.localToWorldMatrix);
						Matrices.AddNoResize(value);
						PlayerNoclipState playerNoclipState = noclipStates[toOverlapIndex];
						bool flag = playerNoclipState.VehiclePauseTime <= 0f && !basePlayer.isMounted;
						bool force = playerNoclipState.ForceCastTime > 0f;
						bool skipVehicleLayer = false;
						Batches.AddNoResize(new Batch
						{
							PlayerIndex = toOverlapIndex,
							Count = (int)basePlayer.rawTickCount,
							Force = force,
							CastVehicleLayer = !flag,
							SkipVehicleLayer = skipVehicleLayer
						});
					}
				}
				NativeListEx.Expand(ref From, ToOverlapIndices.Length * num2, copyContents: false);
				NativeListEx.Expand(ref To, ToOverlapIndices.Length * num2, copyContents: false);
				GatherNoClipBatchesJob gatherNoClipBatchesJob = default(GatherNoClipBatchesJob);
				gatherNoClipBatchesJob.From = From;
				gatherNoClipBatchesJob.To = To;
				gatherNoClipBatchesJob.Batches = Batches.AsArray();
				gatherNoClipBatchesJob.TickCache = playerStates.TickCache;
				gatherNoClipBatchesJob.Indices = ToOverlapIndices.AsReadOnly();
				gatherNoClipBatchesJob.Matrices = Matrices.AsReadOnly();
				gatherNoClipBatchesJob.DeltaTimes = playerStates.TickDeltaTime;
				gatherNoClipBatchesJob.MaxSteps = num2;
				gatherNoClipBatchesJob.DefaultStepSize = Mathf.Max(ConVar.AntiHack.noclip_stepsize, 0.1f);
				gatherNoClipBatchesJob.DefaultProtection = ConVar.AntiHack.noclip_protection;
				gatherNoClipBatchesJob.MaxTickCount = ConVar.AntiHack.tick_buffer_noclip_threshold * (float)Player.tickrate_cl;
				gatherNoClipBatchesJob.LagThreshold = ConVar.AntiHack.tick_buffer_server_lag_threshold;
				gatherNoClipBatchesJob.TickBufferPrevention = ConVar.AntiHack.tick_buffer_preventions;
				GatherNoClipBatchesJob jobData2 = gatherNoClipBatchesJob;
				IJobExtensions.RunByRef(ref jobData2);
			}
			foundIndices.Clear();
			if (!Batches.IsEmpty)
			{
				TestAreNoClipping(in playerStates, From.AsReadOnly(), To.AsReadOnly(), Batches.AsReadOnly(), foundIndices, colliders);
			}
		}
	}

	public static void AreSpeeding(in BasePlayer.PlayerServerStates.ReadOnly playerStates, NativeArray<PlayerSpeedhackState> speedStateCache, NativeArray<int>.ReadOnly indices, NativeArray<bool> results)
	{
		using (TimeWarning.New("AntiHack.AreSpeeding"))
		{
			ProgressSpeedingStatesJob progressSpeedingStatesJob = default(ProgressSpeedingStatesJob);
			progressSpeedingStatesJob.SpeedStates = speedStateCache;
			progressSpeedingStatesJob.DeltaTime = playerStates.TickDeltaTime;
			progressSpeedingStatesJob.Indices = indices;
			ProgressSpeedingStatesJob jobData = progressSpeedingStatesJob;
			JobHandle jobHandle = IJobForExtensions.ScheduleByRef(ref jobData, indices.Length, default(JobHandle));
			if (ConVar.AntiHack.speedhack_protection == 0)
			{
				FillJob<bool> fillJob = default(FillJob<bool>);
				fillJob.Value = false;
				fillJob.Values = results;
				FillJob<bool> jobData2 = fillJob;
				IJobExtensions.ScheduleByRef(ref jobData2, jobHandle).Complete();
				return;
			}
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			NativeArray<Matrix4x4> nativeArray = new NativeArray<Matrix4x4>(indices.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			for (int i = 0; i < indices.Length; i++)
			{
				int index = indices[i];
				BasePlayer basePlayer = objects[index];
				bool flag = basePlayer.transform.parent == null;
				nativeArray[i] = (flag ? Matrix4x4.zero : basePlayer.transform.parent.localToWorldMatrix);
			}
			NativeArray<Vector3> starts = new NativeArray<Vector3>(objects.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			NativeArray<Vector3> ends = new NativeArray<Vector3>(objects.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			TransformStartEndTicksJob transformStartEndTicksJob = default(TransformStartEndTicksJob);
			transformStartEndTicksJob.Starts = starts;
			transformStartEndTicksJob.Ends = ends;
			transformStartEndTicksJob.Matrices = nativeArray.AsReadOnly();
			transformStartEndTicksJob.TickCache = playerStates.TickCache;
			transformStartEndTicksJob.Indices = indices;
			TransformStartEndTicksJob jobData3 = transformStartEndTicksJob;
			JobHandle jobHandle2 = IJobForExtensions.ScheduleByRef(ref jobData3, indices.Length, default(JobHandle));
			nativeArray.Dispose(jobHandle2);
			NativeArray<RDC> nativeArray2 = new NativeArray<RDC>(objects.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			JobHandle dependency;
			if (ConVar.AntiHack.speedhack_protection >= 2)
			{
				CalculateRDCsJob calculateRDCsJob = default(CalculateRDCsJob);
				calculateRDCsJob.RDCs = nativeArray2;
				calculateRDCsJob.States = playerStates.CachedStates;
				calculateRDCsJob.MsFlags = playerStates.PlayerModelStateFlags;
				calculateRDCsJob.Ducking = playerStates.PlayerModelStateDucking;
				calculateRDCsJob.Indices = indices;
				CalculateRDCsJob jobData4 = calculateRDCsJob;
				dependency = IJobForExtensions.ScheduleByRef(ref jobData4, indices.Length, default(JobHandle));
			}
			else
			{
				FillJob<RDC> fillJob2 = default(FillJob<RDC>);
				fillJob2.Values = nativeArray2;
				fillJob2.Value = new RDC
				{
					Running = 1f,
					Ducking = 0f,
					Crawling = 0f
				};
				FillJob<RDC> jobData5 = fillJob2;
				dependency = IJobExtensions.ScheduleByRef(ref jobData5);
			}
			NativeArray<float> speed = new NativeArray<float>(indices.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			CalcSpeedHackSpeedJob calcSpeedHackSpeedJob = default(CalcSpeedHackSpeedJob);
			calcSpeedHackSpeedJob.Speed = speed;
			calcSpeedHackSpeedJob.States = playerStates.CachedStates;
			calcSpeedHackSpeedJob.RDCs = nativeArray2.AsReadOnly();
			calcSpeedHackSpeedJob.Indices = indices;
			calcSpeedHackSpeedJob.WaterThreshold = ConVar.AntiHack.speedhack_water_threshold;
			CalcSpeedHackSpeedJob jobData6 = calcSpeedHackSpeedJob;
			JobHandle jobHandle3 = IJobForExtensions.ScheduleByRef(ref jobData6, indices.Length, dependency);
			nativeArray2.Dispose(jobHandle3);
			NativeArray<(float, float)> distAndBudget = new NativeArray<(float, float)>(objects.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			NativeList<int> indicesForNormalSample = new NativeList<int>(indices.Length, Allocator.TempJob);
			CalcDistAndBudgetJob calcDistAndBudgetJob = default(CalcDistAndBudgetJob);
			calcDistAndBudgetJob.DistAndBudget = distAndBudget;
			calcDistAndBudgetJob.IndicesForNormalSample = indicesForNormalSample;
			calcDistAndBudgetJob.Start = starts.AsReadOnly();
			calcDistAndBudgetJob.End = ends.AsReadOnly();
			calcDistAndBudgetJob.Speed = speed.AsReadOnly();
			calcDistAndBudgetJob.DeltaTime = playerStates.TickDeltaTime;
			calcDistAndBudgetJob.States = playerStates.CachedStates;
			calcDistAndBudgetJob.Indices = indices;
			calcDistAndBudgetJob.Use3DMagnitude = ConVar.AntiHack.speedhack_protection >= 3;
			CalcDistAndBudgetJob jobData7 = calcDistAndBudgetJob;
			JobHandle dependency2 = JobHandle.CombineDependencies(jobHandle2, jobHandle3);
			JobHandle jobHandle4 = IJobForExtensions.ScheduleByRef(ref jobData7, indices.Length, dependency2);
			speed.Dispose(jobHandle4);
			NativeArray<Vector3> results2 = new NativeArray<Vector3>(objects.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			JobHandle dependsOn;
			if (TerrainMeta.HeightMap != null)
			{
				dependsOn = TerrainMeta.HeightMap.GetNormalsIndirect(starts.AsReadOnly(), results2, indicesForNormalSample.AsDeferredJobArray(), jobHandle4);
			}
			else
			{
				ScatterValueToJobDeferred<Vector3> scatterValueToJobDeferred = default(ScatterValueToJobDeferred<Vector3>);
				scatterValueToJobDeferred.Results = results2;
				scatterValueToJobDeferred.Value = Vector3.up;
				scatterValueToJobDeferred.Indices = indicesForNormalSample.AsDeferredJobArray();
				ScatterValueToJobDeferred<Vector3> jobData8 = scatterValueToJobDeferred;
				dependsOn = IJobExtensions.ScheduleByRef(ref jobData8, jobHandle4);
			}
			AdjustDistBasedOnNormalsJob adjustDistBasedOnNormalsJob = default(AdjustDistBasedOnNormalsJob);
			adjustDistBasedOnNormalsJob.DistAndBudget = distAndBudget;
			adjustDistBasedOnNormalsJob.Start = starts.AsReadOnly();
			adjustDistBasedOnNormalsJob.End = ends.AsReadOnly();
			adjustDistBasedOnNormalsJob.Normals = results2.AsReadOnly();
			adjustDistBasedOnNormalsJob.DeltaTime = playerStates.TickDeltaTime;
			adjustDistBasedOnNormalsJob.Indices = indicesForNormalSample.AsDeferredJobArray().AsReadOnly();
			adjustDistBasedOnNormalsJob.SlopeSpeed = ConVar.AntiHack.speedhack_slopespeed;
			AdjustDistBasedOnNormalsJob jobData9 = adjustDistBasedOnNormalsJob;
			JobHandle jobHandle5 = IJobExtensions.ScheduleByRef(ref jobData9, dependsOn);
			results2.Dispose(jobHandle5);
			indicesForNormalSample.Dispose(jobHandle5);
			starts.Dispose(jobHandle5);
			ends.Dispose(jobHandle5);
			TestAreSpeedingJob testAreSpeedingJob = default(TestAreSpeedingJob);
			testAreSpeedingJob.Results = results;
			testAreSpeedingJob.PlayerStates = speedStateCache;
			testAreSpeedingJob.DistAndBudget = distAndBudget.AsReadOnly();
			testAreSpeedingJob.DeltaTime = playerStates.TickDeltaTime;
			testAreSpeedingJob.Indices = indices;
			testAreSpeedingJob.ForgivenessInertia = ConVar.AntiHack.speedhack_forgiveness_inertia;
			testAreSpeedingJob.Forgiveness = ConVar.AntiHack.speedhack_forgiveness;
			TestAreSpeedingJob jobData10 = testAreSpeedingJob;
			JobHandle dependency3 = JobHandle.CombineDependencies(jobHandle, jobHandle5);
			JobHandle inputDeps = IJobForExtensions.ScheduleByRef(ref jobData10, indices.Length, dependency3);
			distAndBudget.Dispose(inputDeps);
			inputDeps.Complete();
		}
	}

	public static void AreFlying(in BasePlayer.PlayerServerStates.ReadOnly playerStates, NativeArray<PlayerState>.ReadOnly ahStates, NativeArray<PlayerFlyhackState> playerFlyStates, NativeArray<int>.ReadOnly indices, NativeArray<bool> results)
	{
		FillJob<bool> fillJob = default(FillJob<bool>);
		fillJob.Value = false;
		fillJob.Values = results;
		FillJob<bool> jobData = fillJob;
		IJobExtensions.RunByRef(ref jobData);
		ProcessFlyhackPauseTimeJob processFlyhackPauseTimeJob = default(ProcessFlyhackPauseTimeJob);
		processFlyhackPauseTimeJob.PlayerStates = playerFlyStates;
		processFlyhackPauseTimeJob.Indices = indices;
		processFlyhackPauseTimeJob.DeltaTimes = playerStates.TickDeltaTime;
		ProcessFlyhackPauseTimeJob jobData2 = processFlyhackPauseTimeJob;
		IJobExtensions.RunByRef(ref jobData2);
		if (ConVar.AntiHack.flyhack_protection <= 0)
		{
			return;
		}
		ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
		NativeArray<Matrix4x4> nativeArray = new NativeArray<Matrix4x4>(indices.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		for (int i = 0; i < indices.Length; i++)
		{
			int index = indices[i];
			BasePlayer basePlayer = objects[index];
			bool flag = basePlayer.transform.parent == null;
			nativeArray[i] = (flag ? Matrix4x4.zero : basePlayer.transform.parent.localToWorldMatrix);
		}
		using NativeArray<FlyingBatch> batches = new NativeArray<FlyingBatch>(indices.Length, Allocator.TempJob);
		using NativeList<Vector3> from = new NativeList<Vector3>(indices.Length * ConVar.AntiHack.flyhack_maxsteps, Allocator.TempJob);
		using NativeList<Vector3> to = new NativeList<Vector3>(indices.Length * ConVar.AntiHack.flyhack_maxsteps, Allocator.TempJob);
		using NativeList<Vector3> checkPoses = new NativeList<Vector3>(indices.Length * ConVar.AntiHack.flyhack_maxsteps, Allocator.TempJob);
		GatherFlyingBatchesJob gatherFlyingBatchesJob = default(GatherFlyingBatchesJob);
		gatherFlyingBatchesJob.From = from;
		gatherFlyingBatchesJob.To = to;
		gatherFlyingBatchesJob.CheckPoses = checkPoses;
		gatherFlyingBatchesJob.Batches = batches;
		gatherFlyingBatchesJob.TickCache = playerStates.TickCache;
		gatherFlyingBatchesJob.Indices = indices;
		gatherFlyingBatchesJob.Matrices = nativeArray.AsReadOnly();
		gatherFlyingBatchesJob.MaxSteps = ConVar.AntiHack.flyhack_maxsteps;
		gatherFlyingBatchesJob.DefaultStepSize = Mathf.Max(ConVar.AntiHack.flyhack_stepsize, 0.1f);
		gatherFlyingBatchesJob.Protection = ConVar.AntiHack.flyhack_protection;
		GatherFlyingBatchesJob jobData3 = gatherFlyingBatchesJob;
		IJobExtensions.RunByRef(ref jobData3);
		nativeArray.Dispose();
		if (batches.Length > 0)
		{
			TestAreFlying(in playerStates, from.AsReadOnly(), to.AsReadOnly(), checkPoses.AsReadOnly(), ahStates, playerFlyStates, batches.AsReadOnly(), ConVar.AntiHack.flyhack_protection >= 2, indices, results);
		}
	}

	public static void TestAreFlying(in BasePlayer.PlayerServerStates.ReadOnly playerStates, NativeArray<Vector3>.ReadOnly oldPoses, NativeArray<Vector3>.ReadOnly newPoses, NativeArray<Vector3>.ReadOnly checkPoses, NativeArray<PlayerState>.ReadOnly ahStates, NativeArray<PlayerFlyhackState> flyStates, NativeArray<FlyingBatch>.ReadOnly flyingBatches, bool verifyGrounded, NativeArray<int>.ReadOnly indices, NativeArray<bool> results)
	{
		int defaultMaxResultsPerQuery = GamePhysics.DefaultMaxResultsPerQuery;
		NativeArray<Vector3> results2 = new NativeArray<Vector3>(checkPoses.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		NativeArray<Vector3> results3 = new NativeArray<Vector3>(checkPoses.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		NativeArray<float> values = new NativeArray<float>(checkPoses.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		NativeArray<int> values2 = new NativeArray<int>(checkPoses.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		NativeArray<ColliderHit> hits = new NativeArray<ColliderHit>(checkPoses.Length * defaultMaxResultsPerQuery, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		AddVectorJob addVectorJob = default(AddVectorJob);
		addVectorJob.Inputs = checkPoses;
		addVectorJob.Results = results2;
		addVectorJob.Modification = new Vector3(0f, 0.5f - ConVar.AntiHack.flyhack_extrusion, 0f);
		AddVectorJob jobData = addVectorJob;
		IJobExtensions.RunByRef(ref jobData);
		addVectorJob = default(AddVectorJob);
		addVectorJob.Inputs = checkPoses;
		addVectorJob.Results = results3;
		addVectorJob.Modification = new Vector3(0f, 1.3f, 0f);
		AddVectorJob jobData2 = addVectorJob;
		IJobExtensions.RunByRef(ref jobData2);
		FillJob<float> fillJob = default(FillJob<float>);
		fillJob.Values = values;
		fillJob.Value = 0.5f - ConVar.AntiHack.flyhack_margin;
		FillJob<float> jobData3 = fillJob;
		IJobExtensions.RunByRef(ref jobData3);
		FillJob<int> fillJob2 = default(FillJob<int>);
		fillJob2.Values = values2;
		fillJob2.Value = 1503895809;
		FillJob<int> jobData4 = fillJob2;
		IJobExtensions.RunByRef(ref jobData4);
		JobHandle inputDeps = GamePhysics.OverlapCapsules(results2.AsReadOnly(), results3.AsReadOnly(), values.AsReadOnly(), values2.AsReadOnly(), hits, defaultMaxResultsPerQuery, QueryTriggerInteraction.Ignore, GamePhysics.MasksToValidate.None);
		using NativeArray<Vector3> results4 = new NativeArray<Vector3>(checkPoses.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		addVectorJob = default(AddVectorJob);
		addVectorJob.Inputs = checkPoses;
		addVectorJob.Results = results4;
		addVectorJob.Modification = new Vector3(0f, 0f - ConVar.AntiHack.flyhack_extrusion, 0f);
		AddVectorJob jobData5 = addVectorJob;
		IJobExtensions.RunByRef(ref jobData5);
		using NativeArray<WaterLevel.WaterInfo> results5 = new NativeArray<WaterLevel.WaterInfo>(checkPoses.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		WaterLevel.GetWaterInfos(results4.AsReadOnly(), waves: true, volumes: false, null, results5);
		using NativeArray<bool> results6 = new NativeArray<bool>(checkPoses.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		GatherValidWaterIndicesJob gatherValidWaterIndicesJob = default(GatherValidWaterIndicesJob);
		gatherValidWaterIndicesJob.WaterInfos = results5.AsReadOnly();
		gatherValidWaterIndicesJob.Results = results6;
		GatherValidWaterIndicesJob jobData6 = gatherValidWaterIndicesJob;
		IJobExtensions.RunByRef(ref jobData6);
		results2.Dispose(inputDeps);
		results3.Dispose(inputDeps);
		values.Dispose(inputDeps);
		values2.Dispose(inputDeps);
		using NativeArray<float> values3 = new NativeArray<float>(checkPoses.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		fillJob = default(FillJob<float>);
		fillJob.Values = values3;
		fillJob.Value = 0.01f;
		FillJob<float> jobData7 = fillJob;
		IJobExtensions.RunByRef(ref jobData7);
		using NativeArray<int> values4 = new NativeArray<int>(checkPoses.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		fillJob2 = default(FillJob<int>);
		fillJob2.Values = values4;
		fillJob2.Value = 262144;
		FillJob<int> jobData8 = fillJob2;
		IJobExtensions.RunByRef(ref jobData8);
		using NativeArray<EnvironmentType> results7 = new NativeArray<EnvironmentType>(checkPoses.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		EnvironmentManager.Get(checkPoses, values3.AsReadOnly(), values4.AsReadOnly(), results7, GamePhysics.DefaultMaxResultsPerQuery, QueryTriggerInteraction.Collide, GamePhysics.MasksToValidate.None);
		using NativeArray<bool> results8 = new NativeArray<bool>(checkPoses.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		CheckAnyEnvironmentTypeInGroupJob checkAnyEnvironmentTypeInGroupJob = default(CheckAnyEnvironmentTypeInGroupJob);
		checkAnyEnvironmentTypeInGroupJob.Hits = results7.AsReadOnly();
		checkAnyEnvironmentTypeInGroupJob.Results = results8;
		checkAnyEnvironmentTypeInGroupJob.GroupSize = defaultMaxResultsPerQuery;
		checkAnyEnvironmentTypeInGroupJob.TypeToTest = EnvironmentType.Elevator;
		CheckAnyEnvironmentTypeInGroupJob jobData9 = checkAnyEnvironmentTypeInGroupJob;
		IJobExtensions.RunByRef(ref jobData9);
		inputDeps.Complete();
		using NativeList<int> results9 = new NativeList<int>(results2.Length * defaultMaxResultsPerQuery, Allocator.TempJob);
		GatherHitColliderIndicesJob gatherHitColliderIndicesJob = default(GatherHitColliderIndicesJob);
		gatherHitColliderIndicesJob.Hits = hits.AsReadOnly();
		gatherHitColliderIndicesJob.Results = results9;
		gatherHitColliderIndicesJob.ResultsPerQuery = defaultMaxResultsPerQuery;
		GatherHitColliderIndicesJob jobData10 = gatherHitColliderIndicesJob;
		IJobExtensions.RunByRef(ref jobData10);
		using NativeArray<int> lookup = new NativeArray<int>(checkPoses.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		BuildFlyingBatchLookupMapJob buildFlyingBatchLookupMapJob = default(BuildFlyingBatchLookupMapJob);
		buildFlyingBatchLookupMapJob.Lookup = lookup;
		buildFlyingBatchLookupMapJob.Batches = flyingBatches;
		BuildFlyingBatchLookupMapJob jobData11 = buildFlyingBatchLookupMapJob;
		IJobExtensions.RunByRef(ref jobData11);
		NativeArray<bool> nativeArray = new NativeArray<bool>(checkPoses.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		FillJob<bool> fillJob3 = default(FillJob<bool>);
		fillJob3.Values = nativeArray;
		fillJob3.Value = true;
		FillJob<bool> jobData12 = fillJob3;
		IJobExtensions.RunByRef(ref jobData12);
		ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
		Span<PlayerFlyhackState> span = flyStates;
		for (int i = 0; i < results9.Length; i++)
		{
			int index = results9[i] / defaultMaxResultsPerQuery;
			int index2 = lookup[index];
			FlyingBatch flyingBatch = flyingBatches[index2];
			if (!nativeArray[index])
			{
				continue;
			}
			BasePlayer basePlayer = objects[flyingBatch.PlayerIndex];
			ref PlayerFlyhackState reference = ref span[flyingBatch.PlayerIndex];
			reference.IsOnPlayer = false;
			Collider collider = hits[results9[i]].collider;
			if ((0x20000u & (uint)(1 << collider.gameObject.layer)) != 0)
			{
				BasePlayer basePlayer2 = GameObjectEx.ToBaseEntity(collider) as BasePlayer;
				if (basePlayer2 == basePlayer)
				{
					continue;
				}
				if (basePlayer2.ActivePlayerInd != -1)
				{
					PlayerFlyhackState playerFlyhackState = flyStates[basePlayer2.ActivePlayerInd];
					if (playerFlyhackState.IsInAir || playerFlyhackState.IsOnPlayer || basePlayer2.TriggeredAntiHack(ahStates))
					{
						continue;
					}
				}
				if (!basePlayer2.IsSleeping())
				{
					reference.IsOnPlayer = true;
					nativeArray[index] = false;
				}
			}
			else
			{
				nativeArray[index] = false;
			}
		}
		hits.Dispose();
		DetermineInAirJob determineInAirJob = default(DetermineInAirJob);
		determineInAirJob.Results = nativeArray;
		determineInAirJob.PlayerFlyStates = flyStates;
		determineInAirJob.PlayerMSFlags = playerStates.PlayerModelStateFlags;
		determineInAirJob.PlayerStates = playerStates.CachedStates;
		determineInAirJob.FlyingBatches = flyingBatches;
		determineInAirJob.OldPoses = oldPoses;
		determineInAirJob.WaterValidStates = results6.AsReadOnly();
		determineInAirJob.ElevatorValidStates = results8.AsReadOnly();
		determineInAirJob.Indices = indices;
		determineInAirJob.verifyGrounded = verifyGrounded;
		DetermineInAirJob jobData13 = determineInAirJob;
		IJobExtensions.RunByRef(ref jobData13);
		using NativeArray<bool> results10 = new NativeArray<bool>(indices.Length, Allocator.TempJob);
		GatherWasInAirStatesJob gatherWasInAirStatesJob = default(GatherWasInAirStatesJob);
		gatherWasInAirStatesJob.Results = results10;
		gatherWasInAirStatesJob.PlayerStates = flyStates.AsReadOnly();
		gatherWasInAirStatesJob.Indices = indices;
		GatherWasInAirStatesJob jobData14 = gatherWasInAirStatesJob;
		IJobExtensions.RunByRef(ref jobData14);
		CacheInAirStateJob cacheInAirStateJob = default(CacheInAirStateJob);
		cacheInAirStateJob.PlayerStates = flyStates;
		cacheInAirStateJob.Indices = indices;
		cacheInAirStateJob.BatchMap = lookup.AsReadOnly();
		cacheInAirStateJob.PlayersInAir = nativeArray.AsReadOnly();
		cacheInAirStateJob.OldPoses = oldPoses;
		CacheInAirStateJob jobData15 = cacheInAirStateJob;
		IJobExtensions.RunByRef(ref jobData15);
		TestAreFlyingJob testAreFlyingJob = default(TestAreFlyingJob);
		testAreFlyingJob.Results = results;
		testAreFlyingJob.PlayerStates = flyStates;
		testAreFlyingJob.Indices = indices;
		testAreFlyingJob.BatchMap = lookup.AsReadOnly();
		testAreFlyingJob.OldPoses = oldPoses;
		testAreFlyingJob.NewPoses = newPoses;
		testAreFlyingJob.PlayersInAir = nativeArray.AsReadOnly();
		testAreFlyingJob.WasInAirStates = results10.AsReadOnly();
		testAreFlyingJob.ForgivenessVerticalInertia = ConVar.AntiHack.flyhack_forgiveness_vertical_inertia;
		testAreFlyingJob.ForgivenessVertical = ConVar.AntiHack.flyhack_forgiveness_vertical;
		testAreFlyingJob.ForgivenessHorizontalInertia = ConVar.AntiHack.flyhack_forgiveness_horizontal_inertia;
		testAreFlyingJob.ForgivenessHorizontal = ConVar.AntiHack.flyhack_forgiveness_horizontal;
		testAreFlyingJob.TimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
		TestAreFlyingJob jobData16 = testAreFlyingJob;
		IJobExtensions.RunByRef(ref jobData16);
		nativeArray.Dispose();
	}

	public static bool TestIsBuildingInsideSomething(Construction.Target target, Vector3 deployPos)
	{
		if (ConVar.AntiHack.build_inside_check <= 0)
		{
			return false;
		}
		foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
		{
			if (monument.IsInBounds(deployPos))
			{
				return false;
			}
		}
		if (IsInsideMesh(deployPos) && IsInsideMesh(target.ray.origin))
		{
			LogToConsoleBatched(target.player, AntiHackType.InsideGeometry, "Tried to build while clipped inside " + isInsideRayHit.collider.name, 25f);
			if (ConVar.AntiHack.build_inside_check > 1)
			{
				return true;
			}
		}
		return false;
	}

	public static void FadeViolations(BasePlayer ply, float deltaTime)
	{
		ref PlayerState reference = ref ((Span<PlayerState>)PlayerStates)[ply.ActivePlayerInd];
		if (UnityEngine.Time.realtimeSinceStartup - reference.LastViolationTime > ConVar.AntiHack.relaxationpause)
		{
			reference.ViolationLevel = Mathf.Max(0f, reference.ViolationLevel - ConVar.AntiHack.relaxationrate * deltaTime);
		}
	}

	public static bool EnforceViolations(BasePlayer ply)
	{
		PlayerState playerState = PlayerStates[ply.ActivePlayerInd];
		if (playerState.ViolationLevel > ConVar.AntiHack.maxviolation)
		{
			if (ConVar.AntiHack.debuglevel >= 1)
			{
				LogToConsole(ply, playerState.LastViolationType, $"Enforcing (violation of {playerState.ViolationLevel})");
			}
			string reason = $"{playerState.LastViolationType} Violation Level {playerState.ViolationLevel}";
			if (ConVar.AntiHack.enforcementlevel > 1)
			{
				Kick(ply, reason);
			}
			else
			{
				Kick(ply, reason);
			}
			return true;
		}
		return false;
	}

	public static void Log(BasePlayer ply, AntiHackType type, string message, bool logToAnalytics = true)
	{
		if (ConVar.AntiHack.debuglevel > 1)
		{
			LogToConsole(ply, type, message);
		}
		if (logToAnalytics)
		{
			Facepunch.Rust.Analytics.Azure.OnAntihackViolation(ply, type, message);
		}
		LogToEAC(ply, type, message);
	}

	public static void LogToConsoleBatched(BasePlayer ply, AntiHackType type, string message, float maxDistance)
	{
		string playerName = ply.ToString();
		Vector3 position = ply.transform.position;
		foreach (GroupedLog groupedLog2 in groupedLogs)
		{
			if (groupedLog2.TryGroup(playerName, type, message, position, maxDistance))
			{
				return;
			}
		}
		GroupedLog groupedLog = Facepunch.Pool.Get<GroupedLog>();
		groupedLog.SetInitial(playerName, type, message, position);
		groupedLogs.Enqueue(groupedLog);
	}

	private static void LogToConsole(BasePlayer ply, AntiHackType type, string message)
	{
		Debug.LogWarning(ply?.ToString() + " " + type.ToString() + ": " + message + " at " + ply.transform.position);
	}

	private static void LogToConsole(string plyName, AntiHackType type, string message, Vector3 pos)
	{
		Debug.LogWarning(plyName + " " + type.ToString() + ": " + message + " at " + pos);
	}

	private static void LogToEAC(BasePlayer ply, AntiHackType type, string message)
	{
		if (ConVar.AntiHack.reporting)
		{
			EACServer.SendPlayerBehaviorReport(PlayerReportsCategory.Exploiting, ply.UserIDString, type.ToString() + ": " + message);
		}
	}

	public static void AddViolation(BasePlayer ply, AntiHackType type, float amount, GameObject gameObject = null)
	{
		if (Interface.CallHook("OnPlayerViolation", ply, type, amount, gameObject) != null || ply.ActivePlayerInd == -1)
		{
			return;
		}
		using (TimeWarning.New("AntiHack.AddViolation"))
		{
			ref PlayerState reference = ref ((Span<PlayerState>)PlayerStates)[ply.ActivePlayerInd];
			reference.LastViolationType = type;
			reference.LastViolationTime = UnityEngine.Time.realtimeSinceStartup;
			reference.ViolationLevel += amount;
			if (type == AntiHackType.NoClip || type == AntiHackType.FlyHack || type == AntiHackType.SpeedHack || type == AntiHackType.InsideGeometry || type == AntiHackType.InsideTerrain || type == AntiHackType.Ticks)
			{
				reference.LastMovementViolationTime = UnityEngine.Time.realtimeSinceStartup;
			}
			if ((ConVar.AntiHack.debuglevel < 2 || !(amount > 0f)) && (ConVar.AntiHack.debuglevel < 3 || type == AntiHackType.NoClip) && ConVar.AntiHack.debuglevel < 4)
			{
				return;
			}
			string text = "Added violation of " + amount + " in frame " + UnityEngine.Time.frameCount + " (now has " + reference.ViolationLevel + ")";
			if (gameObject != null)
			{
				text = text + " " + gameObject.name;
				BaseEntity baseEntity = GameObjectEx.ToBaseEntity(gameObject);
				if (baseEntity != null)
				{
					text = text + " (entity: " + baseEntity.ShortPrefabName + ")";
				}
			}
			LogToConsole(ply, type, text);
		}
	}

	public static void Kick(BasePlayer ply, string reason)
	{
		AddRecord(ply, kicks);
		ply.Kick(reason);
	}

	public static void Ban(BasePlayer ply, string reason)
	{
		AddRecord(ply, bans);
		ConsoleSystem.Run(ConsoleSystem.Option.Server, "ban", ply.userID.Get(), reason);
	}

	private static void AddRecord(BasePlayer ply, Dictionary<ulong, int> records)
	{
		if (records.ContainsKey(ply.userID))
		{
			records[ply.userID]++;
		}
		else
		{
			records.Add(ply.userID, 1);
		}
	}

	public static int GetKickRecord(BasePlayer ply)
	{
		return GetRecord(ply, kicks);
	}

	public static int GetBanRecord(BasePlayer ply)
	{
		return GetRecord(ply, bans);
	}

	private static int GetRecord(BasePlayer ply, Dictionary<ulong, int> records)
	{
		if (!records.ContainsKey(ply.userID))
		{
			return 0;
		}
		return records[ply.userID];
	}
}
