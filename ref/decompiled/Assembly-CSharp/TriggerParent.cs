#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Rust;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UtilityJobs;

[RequireComponent(typeof(Collider))]
public class TriggerParent : TriggerBase, IServerComponent
{
	private struct AreSwimmingState : IDisposable
	{
		public ref struct ReadOnly
		{
			public NativeArray<int>.ReadOnly EntLookup;

			public BasePlayer[] Players;

			public NativeArray<Vector3>.ReadOnly Posi;

			public NativeArray<Quaternion>.ReadOnly Rots;
		}

		public NativeList<int> EntLookup;

		public BufferList<BasePlayer> Players;

		public NativeList<Vector3> Posi;

		public NativeList<Quaternion> Rots;

		public AreSwimmingState(int playerCount)
		{
			EntLookup = new NativeList<int>(playerCount, Allocator.Temp);
			Players = Facepunch.Pool.Get<BufferList<BasePlayer>>();
			if (Players.Capacity < playerCount)
			{
				Players.Resize(playerCount);
			}
			Posi = new NativeList<Vector3>(playerCount, Allocator.TempJob);
			Rots = new NativeList<Quaternion>(playerCount, Allocator.TempJob);
		}

		public ReadOnly AsReadOnly()
		{
			ReadOnly result = default(ReadOnly);
			result.EntLookup = EntLookup.AsReadOnly();
			result.Players = Players.Buffer;
			result.Posi = Posi.AsReadOnly();
			result.Rots = Rots.AsReadOnly();
			return result;
		}

		public void Dispose()
		{
			Rots.Dispose();
			Posi.Dispose();
			Facepunch.Pool.FreeUnmanaged(ref Players);
			EntLookup.Dispose();
		}
	}

	private struct HasObjUnderFeetState : IDisposable
	{
		public ref struct ReadOnly
		{
			public NativeArray<int>.ReadOnly TriggerLookup;

			public NativeArray<int>.ReadOnly EntLookup;

			public NativeArray<RaycastCommand> Commands;

			public ReadOnlySpan<BaseEntity> IgnoreEnts;
		}

		public NativeList<int> TriggerLookup;

		public NativeList<int> EntLookup;

		public NativeList<RaycastCommand> Commands;

		public BufferList<BaseEntity> IgnoreEnts;

		public HasObjUnderFeetState(int overlapCount, NativeList<RaycastCommand> commands)
		{
			TriggerLookup = new NativeList<int>(overlapCount, Allocator.Temp);
			EntLookup = new NativeList<int>(overlapCount, Allocator.Temp);
			Commands = commands;
			IgnoreEnts = Facepunch.Pool.Get<BufferList<BaseEntity>>();
			if (IgnoreEnts.Capacity < overlapCount)
			{
				IgnoreEnts.Resize(overlapCount);
			}
		}

		public ReadOnly AsReadOnly()
		{
			ReadOnly result = default(ReadOnly);
			result.TriggerLookup = TriggerLookup.AsReadOnly();
			result.EntLookup = EntLookup.AsReadOnly();
			result.Commands = Commands.AsArray();
			result.IgnoreEnts = IgnoreEnts.ContentReadOnlySpan();
			return result;
		}

		public void Dispose()
		{
			TriggerLookup.Dispose();
			EntLookup.Dispose();
			Facepunch.Pool.FreeUnmanaged(ref IgnoreEnts);
		}
	}

	private struct IsClippingState : IDisposable
	{
		public ref struct ReadOnly
		{
			public NativeArray<int>.ReadOnly TriggerLookup;

			public NativeArray<int>.ReadOnly EntLookup;

			public NativeArray<OBB>.ReadOnly OBBs;

			public NativeArray<int>.ReadOnly OBBLayerMasks;

			public JobHandle OBBOverlapsHandle;
		}

		public const int MaxOverlaps = 6;

		public NativeList<int> TriggerLookup;

		public NativeList<int> EntLookup;

		public NativeList<OBB> OBBs;

		public NativeList<int> OBBLayerMasks;

		public JobHandle OBBOverlapsHandle;

		public IsClippingState(int overlapCount, NativeList<OBB> obbs, NativeList<int> obbLayerMasks)
		{
			TriggerLookup = new NativeList<int>(overlapCount, Allocator.Temp);
			EntLookup = new NativeList<int>(overlapCount, Allocator.Temp);
			OBBs = obbs;
			OBBLayerMasks = obbLayerMasks;
			OBBOverlapsHandle = default(JobHandle);
		}

		public ReadOnly AsReadOnly()
		{
			ReadOnly result = default(ReadOnly);
			result.TriggerLookup = TriggerLookup.AsReadOnly();
			result.EntLookup = EntLookup.AsReadOnly();
			result.OBBs = OBBs.AsReadOnly();
			result.OBBLayerMasks = OBBLayerMasks.AsReadOnly();
			result.OBBOverlapsHandle = OBBOverlapsHandle;
			return result;
		}

		public void Dispose()
		{
			TriggerLookup.Dispose();
			EntLookup.Dispose();
		}
	}

	private struct IsInsideState : IDisposable
	{
		public ref struct ReadOnly
		{
			public NativeArray<int>.ReadOnly EntLookup;

			public NativeArray<OBB>.ReadOnly TriggerOBBs;

			public NativeArray<Vector3>.ReadOnly EntPoints;
		}

		public NativeList<int> EntLookup;

		public NativeList<OBB> TriggerOBBs;

		public NativeList<Vector3> EntPoints;

		public IsInsideState(int overlapCount, NativeList<OBB> obbs, NativeList<Vector3> entPoints)
		{
			EntLookup = new NativeList<int>(overlapCount, Allocator.TempJob);
			TriggerOBBs = obbs;
			EntPoints = entPoints;
		}

		public ReadOnly AsReadOnly()
		{
			ReadOnly result = default(ReadOnly);
			result.EntLookup = EntLookup.AsReadOnly();
			result.TriggerOBBs = TriggerOBBs.AsReadOnly();
			result.EntPoints = EntPoints.AsReadOnly();
			return result;
		}

		public void Dispose()
		{
			EntLookup.Dispose();
		}
	}

	private static int _TickMode = 1;

	[ServerVar(Help = "Allow triggers to sleep if both they and their contents are stationary (TickMode 1 only)")]
	public static bool AllowTriggerSleeping = true;

	[ServerVar(Help = "world units a trigger can move in WS before it is woken")]
	public static float sleeping_trigger_mask_epsilon = 0.001f;

	private const int InitialCapacity = 64;

	private static StableObjectArray<TriggerParent> ActiveTriggers;

	private static NativeArray<Vector3> LastTriggerWorldPos;

	private static NativeList<RaycastCommand> TraceCommands;

	private static NativeArray<RaycastHit> TraceHits;

	private static NativeList<OBB> ClippingOBBs;

	private static NativeList<int> ClippingOBBLayerMasks;

	private static NativeArray<ColliderHit> ClippingHits;

	private static NativeList<OBB> IsInsideOBBs;

	private static NativeList<Vector3> IsInsideEntPoints;

	private static NativeArray<bool> IsInsideResults;

	[NonSerialized]
	public int StableIndex = -1;

	[SerializeField]
	[Tooltip("Deparent if the parented entity clips into an obstacle")]
	[Header("General")]
	protected bool doClippingCheck;

	[Tooltip("If deparenting via clipping, this will be used (if assigned) to also move the entity to a valid dismount position")]
	public BaseMountable associatedMountable;

	[Tooltip("Needed if the player might dismount inside the trigger and the trigger might be moving. Being mounting inside the trigger lets them dismount in local trigger-space, which means client and server will sync up.Otherwise the client/server delay can have them dismounting into invalid space.")]
	public bool parentMountedPlayers;

	[Tooltip("Sleepers don't have all the checks (e.g. clipping) that awake players get. If that might be a problem,sleeper parenting can be disabled. You'll need an associatedMountable though so that the sleeper can be dismounted.")]
	public bool parentSleepers = true;

	[Tooltip("This was added to allow parenting in some cases with sinking tugboats, it's generally not needed")]
	public bool parentSwimmers;

	[Header("NPC")]
	public bool ParentNPCPlayers;

	[Tooltip("When parenting an NPC don't check if they are shop keepers or mission providers.")]
	public bool SkipNPCChecks;

	[Header("Other")]
	[Tooltip("If the player is already parented to something else, they'll switch over to another parent only if this is true")]
	public bool overrideOtherTriggers;

	[Tooltip("Requires associatedMountable to be set. Prevents players entering the trigger if there's something between their feet and the bottom of the parent trigger")]
	public bool checkForObjUnderFeet;

	[SerializeField]
	[Header("You probably don't need it")]
	private bool doExpensivePlayerClippingChecks;

	[Tooltip("TickMode 1 only - Allow trigger to sleep if it hasn't moved on selected axis. Set to None if it should never ignore")]
	public BaseEntity.Axis TriggerMovementMask = BaseEntity.Axis.XYZ;

	[Tooltip("TickMode 1 only - Allow to ignore entity that haven't moved on selected axis. Set to None if it should never ignore")]
	public BaseEntity.Axis EntityMovementMask = BaseEntity.Axis.XYZ;

	public const int CLIP_CHECK_MASK = 1218511105;

	protected float triggerHeight;

	private BaseEntity cachedEntity;

	private BasePlayer killPlayerTemp;

	[ServerVar(Help = "0 - old InvokeHandler, 1 - Jobs", Default = "1")]
	public static int TickMode
	{
		get
		{
			return _TickMode;
		}
		set
		{
			if (_TickMode == value)
			{
				return;
			}
			switch (_TickMode)
			{
			case 0:
			{
				ReadOnlySpan<TriggerParent> objects = ActiveTriggers.Objects;
				for (int i = 0; i < objects.Length; i++)
				{
					TriggerParent obj = objects[i];
					obj.CancelInvoke(obj.OnTick);
				}
				break;
			}
			}
			_TickMode = value;
			switch (_TickMode)
			{
			case 0:
			{
				ReadOnlySpan<TriggerParent> objects = ActiveTriggers.Objects;
				for (int i = 0; i < objects.Length; i++)
				{
					TriggerParent obj2 = objects[i];
					obj2.InvokeRepeating(obj2.OnTick, 0f, 0f);
				}
				break;
			}
			case 1:
			{
				NativeArrayEx.Expand(ref LastTriggerWorldPos, ActiveTriggers.Capacity, NativeArrayOptions.UninitializedMemory);
				FillJob<Vector3> fillJob = default(FillJob<Vector3>);
				fillJob.Values = LastTriggerWorldPos;
				fillJob.Value = Vector3.negativeInfinity;
				FillJob<Vector3> jobData = fillJob;
				IJobExtensions.RunByRef(ref jobData);
				break;
			}
			}
		}
	}

	protected BaseEntity Entity
	{
		get
		{
			if (cachedEntity.IsRealNull())
			{
				cachedEntity = GameObjectEx.ToBaseEntity(base.gameObject);
				if (ObjectEx.IsUnityNull(cachedEntity))
				{
					cachedEntity = null;
				}
			}
			else if (cachedEntity != null && cachedEntity.IsDestroyed)
			{
				return null;
			}
			return cachedEntity;
		}
	}

	public static void InitInternalState()
	{
		DisposeInternalState();
		ActiveTriggers = new StableObjectArray<TriggerParent>(64);
		TraceCommands = new NativeList<RaycastCommand>(64, Allocator.Persistent);
		TraceHits = new NativeArray<RaycastHit>(64, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		ClippingOBBs = new NativeList<OBB>(64, Allocator.Persistent);
		ClippingOBBLayerMasks = new NativeList<int>(64, Allocator.Persistent);
		ClippingHits = new NativeArray<ColliderHit>(64, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		IsInsideOBBs = new NativeList<OBB>(64, Allocator.Persistent);
		IsInsideEntPoints = new NativeList<Vector3>(64, Allocator.Persistent);
		IsInsideResults = new NativeArray<bool>(64, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		LastTriggerWorldPos = new NativeArray<Vector3>(64, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public static void DisposeInternalState()
	{
		ActiveTriggers?.Dispose();
		ActiveTriggers = null;
		NativeListEx.SafeDispose(ref TraceCommands);
		NativeArrayEx.SafeDispose(ref TraceHits);
		NativeListEx.SafeDispose(ref ClippingOBBs);
		NativeListEx.SafeDispose(ref ClippingOBBLayerMasks);
		NativeArrayEx.SafeDispose(ref ClippingHits);
		NativeListEx.SafeDispose(ref IsInsideOBBs);
		NativeListEx.SafeDispose(ref IsInsideEntPoints);
		NativeArrayEx.SafeDispose(ref IsInsideResults);
		NativeArrayEx.SafeDispose(ref LastTriggerWorldPos);
	}

	private void AddToActiveTriggers()
	{
		if (StableIndex == -1)
		{
			StableIndex = ActiveTriggers.Add(this);
			switch (_TickMode)
			{
			case 0:
				InvokeRepeating(OnTick, 0f, 0f);
				break;
			case 1:
				NativeArrayEx.Expand(ref LastTriggerWorldPos, ActiveTriggers.Capacity, NativeArrayOptions.UninitializedMemory);
				LastTriggerWorldPos[StableIndex] = Vector3.negativeInfinity;
				break;
			}
		}
	}

	private void RemoveFromActiveTriggers()
	{
		if (StableIndex != -1)
		{
			int indexForSyncRemove = ActiveTriggers.GetIndexForSyncRemove(StableIndex);
			ActiveTriggers.RemoveAtSwapback(StableIndex, invalidateStableIndex: true);
			StableIndex = -1;
			int count = ActiveTriggers.Count;
			if (indexForSyncRemove != count)
			{
				Debug.Assert(indexForSyncRemove < count, "Unexpected swap indices, expecting to swap from end to earlier in range!");
				ActiveTriggers.Objects[indexForSyncRemove].StableIndex = indexForSyncRemove;
				LastTriggerWorldPos[indexForSyncRemove] = LastTriggerWorldPos[count];
			}
			if (_TickMode == 0)
			{
				CancelInvoke(OnTick);
			}
		}
	}

	public static void RunOnTick()
	{
		int tickMode = TickMode;
		if (tickMode != 0 && tickMode == 1)
		{
			RunCustomJobsQueue();
		}
	}

	private static void RunCustomJobsQueue()
	{
		NativeList<int> toUpdate = new NativeList<int>(ActiveTriggers.Count, Allocator.Temp);
		int overlapCount = GatherTriggersToUpdate(toUpdate);
		ShouldParentEntitiesJobs(toUpdate.AsReadOnly(), overlapCount);
		toUpdate.Dispose();
	}

	private static int GatherTriggersToUpdate(NativeList<int> toUpdate)
	{
		int num = 0;
		ReadOnlySpan<TriggerParent> objects = ActiveTriggers.Objects;
		for (int i = 0; i < objects.Length; i++)
		{
			TriggerParent triggerParent = objects[i];
			if (!CollectionEx.IsNullOrEmpty(triggerParent.entityContents))
			{
				toUpdate.AddNoResize(triggerParent.StableIndex);
				num += triggerParent.entityContents.Count;
			}
		}
		return num;
	}

	private static void ShouldParentEntitiesJobs(NativeArray<int>.ReadOnly toUpdate, int overlapCount)
	{
		using (TimeWarning.New("ShouldParentEntitiesJobs"))
		{
			AreSwimmingState areSwimmingState = new AreSwimmingState(overlapCount);
			try
			{
				NativeListEx.Expand(ref TraceCommands, overlapCount, copyContents: false);
				HasObjUnderFeetState hasObjUnderFeetState = new HasObjUnderFeetState(overlapCount, TraceCommands);
				NativeListEx.Expand(ref ClippingOBBs, overlapCount, copyContents: false);
				NativeListEx.Expand(ref ClippingOBBLayerMasks, overlapCount, copyContents: false);
				IsClippingState isClippingState = new IsClippingState(overlapCount, ClippingOBBs, ClippingOBBLayerMasks);
				NativeListEx.Expand(ref IsInsideOBBs, overlapCount, copyContents: false);
				NativeListEx.Expand(ref IsInsideEntPoints, overlapCount, copyContents: false);
				IsInsideState isInsideState = new IsInsideState(overlapCount, IsInsideOBBs, IsInsideEntPoints);
				BufferList<BaseEntity> obj = Facepunch.Pool.Get<BufferList<BaseEntity>>();
				if (obj.Capacity < overlapCount)
				{
					obj.Resize(overlapCount);
				}
				using NativeList<int> entToTriggerLookup = new NativeList<int>(overlapCount, Allocator.TempJob);
				using NativeList<bool> shouldParentEnt = new NativeList<bool>(overlapCount, Allocator.TempJob);
				InitialShouldParentChecks(toUpdate, in areSwimmingState, in isClippingState, in hasObjUnderFeetState, in isInsideState, obj, entToTriggerLookup, shouldParentEnt);
				isClippingState.OBBOverlapsHandle = default(JobHandle);
				IsClippingState.ReadOnly state;
				if (isClippingState.OBBs.Length > 0)
				{
					state = isClippingState.AsReadOnly();
					isClippingState.OBBOverlapsHandle = ScheduleClippingOverlaps(in state);
				}
				JobHandle jobHandle = default(JobHandle);
				if (isInsideState.TriggerOBBs.Length > 0)
				{
					NativeArrayEx.Expand(ref IsInsideResults, isInsideState.TriggerOBBs.Length, NativeArrayOptions.UninitializedMemory, copyContents: false);
					OBB.ContainsJob containsJob = default(OBB.ContainsJob);
					containsJob.OBBs = isInsideState.TriggerOBBs.AsReadOnly();
					containsJob.Points = isInsideState.EntPoints.AsReadOnly();
					containsJob.Results = IsInsideResults;
					OBB.ContainsJob jobData = containsJob;
					jobHandle = IJobParallelForExtensions.ScheduleByRef(ref jobData, isInsideState.TriggerOBBs.Length, isInsideState.TriggerOBBs.Length);
				}
				if (areSwimmingState.Players.Count > 0)
				{
					AreSwimmingState.ReadOnly state2 = areSwimmingState.AsReadOnly();
					RunAreSwimmingChecks(in state2, shouldParentEnt.AsArray());
				}
				if (hasObjUnderFeetState.Commands.Length > 0)
				{
					HasObjUnderFeetState.ReadOnly state3 = hasObjUnderFeetState.AsReadOnly();
					RunCheckForObjUnderFeet(in state3, shouldParentEnt.AsArray());
				}
				if (jobHandle != default(JobHandle))
				{
					jobHandle.Complete();
					ScatterToAndJob scatterToAndJob = default(ScatterToAndJob);
					scatterToAndJob.To = shouldParentEnt.AsArray();
					scatterToAndJob.From = IsInsideResults.AsReadOnly();
					scatterToAndJob.Indices = isInsideState.EntLookup.AsReadOnly();
					ScatterToAndJob jobData2 = scatterToAndJob;
					IJobExtensions.RunByRef(ref jobData2);
				}
				if (isClippingState.OBBs.Length > 0)
				{
					state = isClippingState.AsReadOnly();
					RunClippingChecks(in state, obj.ContentReadOnlySpan(), shouldParentEnt.AsArray());
				}
				CommitShouldParentResults(obj.ContentReadOnlySpan(), entToTriggerLookup.AsReadOnly(), shouldParentEnt.AsReadOnly());
				Facepunch.Pool.FreeUnmanaged(ref obj);
				hasObjUnderFeetState.Dispose();
				isClippingState.Dispose();
				isInsideState.Dispose();
			}
			finally
			{
				((IDisposable)areSwimmingState).Dispose();
			}
		}
	}

	private static void InitialShouldParentChecks(NativeArray<int>.ReadOnly toUpdate, in AreSwimmingState areSwimmingState, in IsClippingState isClippingState, in HasObjUnderFeetState hasObjUnderFeetState, in IsInsideState isInsideState, BufferList<BaseEntity> entities, NativeList<int> entToTriggerLookup, NativeList<bool> shouldParentEnt)
	{
		using (TimeWarning.New("InitialShouldParentChecks"))
		{
			int frameCount = UnityEngine.Time.frameCount;
			ReadOnlySpan<TriggerParent> objects = ActiveTriggers.Objects;
			NativeArray<BasePlayer.CachedState>.ReadOnly cachedStates = BasePlayer.PlayerReadOnlyStates.CachedStates;
			_ = BasePlayer.PlayerReadOnlyStates;
			foreach (int item in toUpdate)
			{
				TriggerParent triggerParent = objects[item];
				BaseMountable baseMountable = triggerParent.associatedMountable;
				bool flag = baseMountable != null;
				bool flag2 = !AllowTriggerSleeping;
				if (!flag2)
				{
					flag2 = triggerParent.TriggerMovementMask == BaseEntity.Axis.None;
				}
				if (!flag2)
				{
					Vector3 position = triggerParent.transform.position;
					flag2 = (BaseEntity.ComparePos(LastTriggerWorldPos[item], position, sleeping_trigger_mask_epsilon) & triggerParent.TriggerMovementMask) != 0;
					if (flag2)
					{
						LastTriggerWorldPos[item] = position;
					}
				}
				bool flag3 = triggerParent is TriggerParentEnclosed;
				OBB value = default(OBB);
				TriggerParentEnclosed.TriggerMode triggerMode = TriggerParentEnclosed.TriggerMode.TriggerPoint;
				if (flag3)
				{
					TriggerParentEnclosed triggerParentEnclosed = triggerParent as TriggerParentEnclosed;
					BoxCollider boxCollider = triggerParentEnclosed.boxCollider;
					Bounds bounds = new Bounds(boxCollider.center, boxCollider.size);
					if (triggerParentEnclosed.Padding > 0f)
					{
						bounds.Expand(triggerParentEnclosed.Padding);
					}
					value = new OBB(boxCollider.transform, bounds);
					triggerMode = triggerParentEnclosed.intersectionMode;
				}
				foreach (BaseEntity entityContent in triggerParent.entityContents)
				{
					if (!entityContent.IsValid() || entityContent.IsDestroyed)
					{
						continue;
					}
					bool flag4 = triggerParent.EntityMovementMask == BaseEntity.Axis.None;
					if (!flag4)
					{
						flag4 = (entityContent.HasMovedInLS(frameCount) & triggerParent.EntityMovementMask) != 0;
						if (!flag4 && entityContent is BasePlayer basePlayer)
						{
							flag4 |= basePlayer.TriggeredNoclip();
						}
					}
					if (!flag2 && !flag4)
					{
						continue;
					}
					bool flag5 = false;
					bool flag6 = false;
					BasePlayer basePlayer2 = null;
					if (entityContent.canTriggerParent)
					{
						if (!triggerParent.overrideOtherTriggers)
						{
							BaseEntity parentEntity = entityContent.GetParentEntity();
							if (parentEntity.IsValid() && parentEntity != triggerParent.Entity)
							{
								goto IL_0319;
							}
						}
						TriggerParentExclusion triggerParentExclusion = entityContent.FindTrigger<TriggerParentExclusion>();
						if (!(triggerParentExclusion != null) || !triggerParentExclusion.IgnoreIfOnLadder || !(entityContent is BasePlayer basePlayer3) || !((basePlayer3.ActivePlayerInd != -1) ? cachedStates[basePlayer3.ActivePlayerInd].IsOnLadder : basePlayer3.OnLadder()))
						{
							basePlayer2 = entityContent.ToPlayer();
							if (basePlayer2 != null)
							{
								if (!triggerParent.parentSwimmers)
								{
									if (basePlayer2.ActivePlayerInd != -1)
									{
										if (cachedStates[basePlayer2.ActivePlayerInd].IsSwimming)
										{
											goto IL_0319;
										}
									}
									else
									{
										flag6 = true;
									}
								}
								if ((!triggerParent.parentMountedPlayers && basePlayer2.isMounted) || (!triggerParent.parentSleepers && basePlayer2.IsSleeping()) || (triggerParent.parentSleepers && basePlayer2.IsServerFalling() && !basePlayer2.IsLoadingAfterTransfer() && !basePlayer2.IsReceivingSnapshot) || (flag && basePlayer2.isMounted && !triggerParent.IsParentedToUs(basePlayer2) && !baseMountable.HasValidDismountPosition(basePlayer2)))
								{
									goto IL_0319;
								}
							}
							flag5 = true;
						}
					}
					goto IL_0319;
					IL_0319:
					if (flag5)
					{
						if (!triggerParent.parentSwimmers && flag6)
						{
							Transform transform = basePlayer2.transform;
							areSwimmingState.Players.Add(basePlayer2);
							areSwimmingState.Posi.AddNoResize(transform.position);
							areSwimmingState.Rots.AddNoResize(transform.rotation);
							areSwimmingState.EntLookup.AddNoResize(shouldParentEnt.Length);
						}
						if (triggerParent.checkForObjUnderFeet)
						{
							Vector3 from = entityContent.PivotPoint() + entityContent.transform.up * 0.1f;
							float distance = triggerParent.triggerHeight + 0.1f;
							RaycastCommand value2 = new RaycastCommand(queryParameters: new QueryParameters(1503731969, hitMultipleFaces: false, QueryTriggerInteraction.Ignore), from: from, direction: -triggerParent.transform.up, distance: distance);
							hasObjUnderFeetState.Commands.AddNoResize(value2);
							hasObjUnderFeetState.IgnoreEnts.Add(entityContent);
							hasObjUnderFeetState.TriggerLookup.AddNoResize(item);
							hasObjUnderFeetState.EntLookup.AddNoResize(shouldParentEnt.Length);
						}
						if (triggerParent.doClippingCheck && !(entityContent is BaseCorpse))
						{
							isClippingState.OBBs.AddNoResize(entityContent.WorldSpaceBounds());
							isClippingState.OBBLayerMasks.AddNoResize(1218511105);
							isClippingState.TriggerLookup.AddNoResize(item);
							isClippingState.EntLookup.AddNoResize(shouldParentEnt.Length);
						}
						if (flag3)
						{
							isInsideState.TriggerOBBs.AddNoResize(value);
							Vector3 value3 = ((triggerMode == TriggerParentEnclosed.TriggerMode.TriggerPoint) ? entityContent.TriggerPoint() : entityContent.PivotPoint());
							isInsideState.EntPoints.AddNoResize(value3);
							isInsideState.EntLookup.AddNoResize(shouldParentEnt.Length);
						}
					}
					entities.Add(entityContent);
					entToTriggerLookup.AddNoResize(item);
					shouldParentEnt.AddNoResize(flag5);
				}
			}
		}
	}

	private static JobHandle ScheduleClippingOverlaps(in IsClippingState.ReadOnly state)
	{
		using (TimeWarning.New("ScheduleClippingOverlaps"))
		{
			NativeArrayEx.Expand(ref ClippingHits, state.OBBs.Length * 6, NativeArrayOptions.UninitializedMemory, copyContents: false);
			JobHandle result = GamePhysics.OverlapOBBs(state.OBBs, state.OBBLayerMasks, ClippingHits, 6, QueryTriggerInteraction.Ignore, GamePhysics.MasksToValidate.Terrain);
			JobHandle.ScheduleBatchedJobs();
			return result;
		}
	}

	private static void RunAreSwimmingChecks(in AreSwimmingState.ReadOnly state, NativeArray<bool> shouldParentEnt)
	{
		using (TimeWarning.New("RunAreSwimmingChecks"))
		{
			using NativeList<int> values = new NativeList<int>(state.Posi.Length, Allocator.TempJob);
			GenerateAscSeqListJob generateAscSeqListJob = default(GenerateAscSeqListJob);
			generateAscSeqListJob.Values = values;
			generateAscSeqListJob.Start = 0;
			generateAscSeqListJob.Step = 1;
			generateAscSeqListJob.Count = state.Posi.Length;
			GenerateAscSeqListJob jobData = generateAscSeqListJob;
			IJobExtensions.RunByRef(ref jobData);
			using NativeArray<WaterLevel.WaterInfo> infos = new NativeArray<WaterLevel.WaterInfo>(state.Posi.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			using NativeArray<float> factors = new NativeArray<float>(state.Posi.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			BasePlayer.GetWaterFactors(state.Players, state.Posi, state.Rots, values.AsReadOnly(), infos, factors);
			for (int i = 0; i < state.Posi.Length; i++)
			{
				if (BasePlayer.IsSwimming(factors[i]))
				{
					int index = state.EntLookup[i];
					shouldParentEnt[index] = false;
				}
			}
		}
	}

	private static void RunCheckForObjUnderFeet(in HasObjUnderFeetState.ReadOnly state, NativeArray<bool> shouldParentEnt)
	{
		using (TimeWarning.New("RunCheckForObjUnderFeet"))
		{
			NativeArrayEx.Expand(ref TraceHits, state.Commands.Length, NativeArrayOptions.UninitializedMemory, copyContents: false);
			GamePhysics.TraceRealmRays(GamePhysics.Realm.Server, state.Commands, TraceHits, traceWater: false, state.IgnoreEnts);
			ReadOnlySpan<TriggerParent> objects = ActiveTriggers.Objects;
			for (int i = 0; i < state.Commands.Length; i++)
			{
				RaycastHit raycastHit = TraceHits[i];
				bool flag = false;
				if (raycastHit.colliderInstanceID != 0)
				{
					TriggerParent triggerParent = objects[state.TriggerLookup[i]];
					BaseEntity entity = triggerParent.Entity;
					BaseEntity baseEntity = GameObjectEx.ToBaseEntity(raycastHit.collider);
					if (baseEntity == null || !baseEntity.HasEntityInParents(entity) || ((bool)triggerParent.associatedMountable && !baseEntity.HasEntityInParents(triggerParent.associatedMountable)))
					{
						flag = true;
					}
				}
				if (flag)
				{
					int index = state.EntLookup[i];
					shouldParentEnt[index] = false;
				}
			}
		}
	}

	private static void RunClippingChecks(in IsClippingState.ReadOnly state, ReadOnlySpan<BaseEntity> entities, NativeArray<bool> shouldParentEnt)
	{
		using (TimeWarning.New("RunCheckForObjUnderFeet"))
		{
			using (TimeWarning.New("Wait for overlaps"))
			{
				state.OBBOverlapsHandle.Complete();
			}
			ReadOnlySpan<TriggerParent> objects = ActiveTriggers.Objects;
			for (int i = 0; i < state.OBBs.Length; i++)
			{
				int index = state.EntLookup[i];
				if (!shouldParentEnt[index])
				{
					continue;
				}
				TriggerParent triggerParent = objects[state.TriggerLookup[i]];
				bool flag = triggerParent.associatedMountable != null;
				bool flag2 = false;
				for (int j = 0; j < 6; j++)
				{
					ColliderHit colliderHit = ClippingHits[i * 6 + j];
					if (colliderHit.instanceID == 0)
					{
						break;
					}
					if (flag)
					{
						BaseEntity baseEntity = GameObjectEx.ToBaseEntity(colliderHit.collider);
						if (!baseEntity || !baseEntity.isServer || baseEntity.GetRootParentEntity() == triggerParent.associatedMountable)
						{
							continue;
						}
					}
					flag2 = true;
				}
				if (flag2 && triggerParent.doExpensivePlayerClippingChecks)
				{
					BasePlayer basePlayer = entities[index].ToPlayer();
					if (basePlayer != null)
					{
						Vector3 vector = basePlayer.TriggerPoint();
						float radius = BasePlayer.NoClipRadius(ConVar.AntiHack.noclip_margin);
						if (!AntiHack.TestNoClipping(basePlayer, vector, vector + Vector3.one * 0.0001f, radius, ConVar.AntiHack.noclip_backtracking, out var _, overlapVehicleLayer: true, triggerParent.associatedMountable))
						{
							flag2 = false;
						}
					}
				}
				if (flag2)
				{
					shouldParentEnt[index] = false;
				}
			}
		}
	}

	private static void CommitShouldParentResults(ReadOnlySpan<BaseEntity> entities, NativeArray<int>.ReadOnly triggerLookup, NativeArray<bool>.ReadOnly shouldParentEnt)
	{
		using (TimeWarning.New("CommitShouldParentResults"))
		{
			ReadOnlySpan<TriggerParent> objects = ActiveTriggers.Objects;
			for (int i = 0; i < entities.Length; i++)
			{
				BaseEntity ent = entities[i];
				TriggerParent triggerParent = objects[triggerLookup[i]];
				if (shouldParentEnt[i])
				{
					triggerParent.Parent(ent);
				}
				else
				{
					triggerParent.Unparent(ent);
				}
			}
		}
	}

	private void OnTransformParentChanged()
	{
		cachedEntity = null;
	}

	protected override void Awake()
	{
		base.Awake();
		Collider component = GetComponent<Collider>();
		triggerHeight = component.bounds.size.y;
	}

	public override GameObject InterestedInObject(GameObject obj)
	{
		obj = base.InterestedInObject(obj);
		if (obj == null)
		{
			return null;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
		if (baseEntity == null)
		{
			return null;
		}
		if (baseEntity.isClient)
		{
			return null;
		}
		return baseEntity.gameObject;
	}

	public override void OnEntityEnter(BaseEntity ent)
	{
		if (!(ent is NPCPlayer npcPly) || (ParentNPCPlayers && CanParentNPC(npcPly)))
		{
			if (ShouldParent(ent))
			{
				Parent(ent);
			}
			base.OnEntityEnter(ent);
			if (entityContents != null && entityContents.Count == 1)
			{
				AddToActiveTriggers();
			}
		}
	}

	public override void OnEntityLeave(BaseEntity ent)
	{
		base.OnEntityLeave(ent);
		if (entityContents == null || entityContents.Count == 0)
		{
			RemoveFromActiveTriggers();
		}
		BasePlayer basePlayer = ent.ToPlayer();
		if (!parentSleepers || !(basePlayer != null) || !basePlayer.IsSleeping())
		{
			Unparent(ent);
		}
	}

	internal override void OnEmpty()
	{
		base.OnEmpty();
		RemoveFromActiveTriggers();
	}

	internal virtual bool CanParentNPC(NPCPlayer npcPly)
	{
		if (SkipNPCChecks)
		{
			return true;
		}
		if (npcPly is NPCShopKeeper)
		{
			return false;
		}
		return true;
	}

	public virtual bool ShouldParent(BaseEntity ent, bool bypassOtherTriggerCheck = false)
	{
		if (!ent.canTriggerParent)
		{
			return false;
		}
		if (!bypassOtherTriggerCheck && !overrideOtherTriggers)
		{
			BaseEntity parentEntity = ent.GetParentEntity();
			if (parentEntity.IsValid() && parentEntity != Entity)
			{
				return false;
			}
		}
		TriggerParentExclusion triggerParentExclusion = ent.FindTrigger<TriggerParentExclusion>();
		if (triggerParentExclusion != null && (!triggerParentExclusion.IgnoreIfOnLadder || !(ent is BasePlayer basePlayer) || !basePlayer.OnLadder()))
		{
			return false;
		}
		if (doClippingCheck && !(ent is BaseCorpse) && IsClipping(ent))
		{
			return false;
		}
		if (checkForObjUnderFeet && HasObjUnderFeet(ent))
		{
			return false;
		}
		BasePlayer basePlayer2 = ent.ToPlayer();
		if (basePlayer2 != null)
		{
			if (!parentSwimmers && basePlayer2.IsSwimming())
			{
				return false;
			}
			if (!parentMountedPlayers && basePlayer2.isMounted)
			{
				return false;
			}
			if (!parentSleepers && basePlayer2.IsSleeping())
			{
				return false;
			}
			if (parentSleepers && basePlayer2.IsServerFalling() && !basePlayer2.IsLoadingAfterTransfer() && !basePlayer2.IsReceivingSnapshot)
			{
				return false;
			}
			if (basePlayer2.isMounted && associatedMountable != null && !IsParentedToUs(basePlayer2) && !associatedMountable.HasValidDismountPosition(basePlayer2))
			{
				return false;
			}
		}
		return true;
	}

	public void ForceParentEarly(BaseEntity ent)
	{
		if (contents == null)
		{
			contents = new HashSet<GameObject>();
		}
		contents.Add(ent.gameObject);
		OnEntityEnter(ent);
		Invoke(CheckAllParenting, 0.1f);
	}

	private void CheckAllParenting()
	{
		List<BaseEntity> obj = Facepunch.Pool.Get<List<BaseEntity>>();
		if (contents != null)
		{
			foreach (GameObject content in contents)
			{
				if (!(content == null))
				{
					BaseEntity baseEntity = GameObjectEx.ToBaseEntity(content);
					if (baseEntity != null && !obj.Contains(baseEntity))
					{
						obj.Add(baseEntity);
					}
				}
			}
		}
		List<BaseEntity> obj2 = Facepunch.Pool.Get<List<BaseEntity>>();
		if (entityContents != null)
		{
			foreach (BaseEntity entityContent in entityContents)
			{
				if (!obj.Contains(entityContent))
				{
					obj2.Add(entityContent);
				}
			}
		}
		foreach (BaseEntity item in obj2)
		{
			OnEntityLeave(item);
		}
		Facepunch.Pool.FreeUnmanaged(ref obj2);
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	protected void Parent(BaseEntity ent)
	{
		BaseEntity entity = Entity;
		if (!(ent.GetParentEntity() == entity) && !(entity.GetParentEntity() == ent))
		{
			ent.SetParent(Entity, worldPositionStays: true, sendImmediate: true);
		}
	}

	protected void Unparent(BaseEntity ent)
	{
		if (!ent || ent.GetParentEntity() != Entity)
		{
			return;
		}
		if (ent.IsValid() && !ent.IsDestroyed)
		{
			TriggerParent triggerParent = ent.FindSuitableParent();
			if (triggerParent != null && triggerParent.Entity.IsValid())
			{
				triggerParent.Parent(ent);
				return;
			}
		}
		ent.SetParent(null, worldPositionStays: true, sendImmediate: true);
		BasePlayer ply = ent.ToPlayer();
		if (!(ply != null))
		{
			return;
		}
		ply.UpdateUnparentTime();
		ply.PauseFlyHackDetection(5f);
		ply.PauseSpeedHackDetection(5f);
		ply.PauseTickDistanceDetection(5f);
		if (AntiHack.TestNoClipping(ply, ply.transform.position, ply.transform.position, BasePlayer.NoClipRadius(ConVar.AntiHack.noclip_margin), ConVar.AntiHack.noclip_backtracking, out var _, overlapVehicleLayer: true))
		{
			ply.PauseVehicleNoClipDetection(5f);
		}
		if (associatedMountable != null && doClippingCheck && IsClipping(ent))
		{
			if (associatedMountable.GetDismountPosition(ply, out var res))
			{
				ply.MovePosition(res, forceUpdateTriggers: false);
				ply.transform.rotation = Quaternion.identity;
				ply.SendNetworkUpdateImmediate();
				ply.ClientRPC(RpcTarget.Player("ForcePositionTo", ply), res);
			}
			else
			{
				killPlayerTemp = ply;
				Invoke(KillPlayerDelayed, 0f);
			}
		}
		if (ply.IsSleeping())
		{
			ply.Invoke(delegate
			{
				ply.SetServerFall(wantsOn: true);
			}, 0.05f);
		}
	}

	private bool IsParentedToUs(BaseEntity ent)
	{
		BaseEntity entity = Entity;
		return ent.GetParentEntity() == entity;
	}

	private void KillPlayerDelayed()
	{
		if (killPlayerTemp.IsValid() && !killPlayerTemp.IsDead())
		{
			killPlayerTemp.Hurt(1000f, DamageType.Suicide, killPlayerTemp, useProtection: false);
		}
		killPlayerTemp = null;
	}

	private void OnTick()
	{
		if (entityContents == null || !Entity.IsValid())
		{
			return;
		}
		foreach (BaseEntity entityContent in entityContents)
		{
			if (entityContent.IsValid() && !entityContent.IsDestroyed)
			{
				if (ShouldParent(entityContent))
				{
					Parent(entityContent);
				}
				else
				{
					Unparent(entityContent);
				}
			}
		}
	}

	protected virtual bool IsClipping(BaseEntity ent)
	{
		bool flag = associatedMountable != null;
		List<Collider> obj = Facepunch.Pool.Get<List<Collider>>();
		GamePhysics.OverlapOBB(ent.WorldSpaceBounds(), obj, 1218511105);
		bool flag2 = false;
		foreach (Collider item in obj)
		{
			if (flag)
			{
				BaseEntity baseEntity = GameObjectEx.ToBaseEntity(item);
				if (!baseEntity || !baseEntity.isServer || baseEntity.GetRootParentEntity() == associatedMountable)
				{
					continue;
				}
			}
			flag2 = true;
			break;
		}
		BasePlayer basePlayer = null;
		if (doExpensivePlayerClippingChecks && flag2 && (basePlayer = ent.ToPlayer()) != null)
		{
			Vector3 vector = basePlayer.TriggerPoint();
			Collider col;
			bool result = AntiHack.TestNoClipping(basePlayer, vector, vector + Vector3.one * 0.0001f, BasePlayer.NoClipRadius(ConVar.AntiHack.noclip_margin), ConVar.AntiHack.noclip_backtracking, out col, overlapVehicleLayer: true, associatedMountable);
			Facepunch.Pool.FreeUnmanaged(ref obj);
			return result;
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		return flag2;
	}

	private bool HasObjUnderFeet(BaseEntity ent)
	{
		Vector3 origin = ent.PivotPoint() + ent.transform.up * 0.1f;
		float maxDistance = triggerHeight + 0.1f;
		Ray ray = new Ray(origin, -base.transform.up);
		if (GamePhysics.TraceRealm(GamePhysics.Realm.Server, ray, 0f, out var hitInfo, maxDistance, 1503731969, QueryTriggerInteraction.Ignore, ent) && hitInfo.collider != null)
		{
			BaseEntity entity = Entity;
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(hitInfo.collider);
			if (baseEntity == null || !baseEntity.HasEntityInParents(entity) || ((bool)associatedMountable && !baseEntity.HasEntityInParents(associatedMountable)))
			{
				return true;
			}
		}
		return false;
	}
}
