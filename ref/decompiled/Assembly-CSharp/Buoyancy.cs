using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ConVar;
using Development.Attributes;
using Facepunch;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UtilityJobs;
using WaterLevelJobs;

[ResetStaticFields]
public class Buoyancy : ListComponent<Buoyancy>, IServerComponent, IPrefabPreProcess
{
	public enum Priority
	{
		High,
		Low
	}

	[Serializable]
	public struct BuoyancyPointData
	{
		[ReadOnly]
		public Vector3 localPosition;

		[ReadOnly]
		public Vector3 rootToPoint;

		[NonSerialized]
		public Vector3 position;
	}

	public BuoyancyPoint[] points;

	public GameObjectRef[] waterImpacts;

	public Rigidbody rigidBody;

	public float buoyancyScale = 1f;

	public bool scaleForceWithMass;

	public bool doEffects = true;

	public float flowMovementScale = 1f;

	public float requiredSubmergedFraction = 0.5f;

	public bool useUnderwaterDrag;

	[Range(0f, 3f)]
	public float underwaterDrag = 2f;

	[Tooltip("How much this object will pay attention to the wave system, 0 = flat water, 1 = full waves (default 1)")]
	[Range(0f, 1f)]
	[FormerlySerializedAs("flatWaterLerp")]
	public float wavesEffect = 1f;

	public Action<bool> SubmergedChanged;

	public BaseEntity forEntity;

	[NonSerialized]
	public float submergedFraction;

	public bool FlowForceDisabled;

	[SerializeField]
	[ReadOnly]
	private BuoyancyPointData[] pointData;

	private bool initedPointArrays;

	private Vector2[] pointPositionArray;

	private Vector2[] pointPositionUVArray;

	private float[] pointShoreDistanceArray;

	private float[] pointTerrainHeightArray;

	private float[] pointWaterHeightArray;

	private float defaultDrag;

	private float defaultAngularDrag;

	private float timeInWater;

	[NonSerialized]
	public float? ArtificialHeight;

	private BaseVehicle forVehicle;

	private bool hasLocalPlayers;

	private bool hadLocalPlayers;

	private static NativeArray<Vector2> allPositions2D;

	private static NativeArray<Vector3> allPositions3D;

	private static NativeArray<Vector2> allUVPositions;

	private static NativeArray<float> pointWaterHeightNativeArray;

	private static NativeArray<float> pointShoreDistanceNativeArray;

	private static NativeArray<float> pointTerrainHeightNativeArray;

	private static NativeArray<WaterLevel.WaterInfo> pointWaterInfoNativeArray;

	private static NativeArray<float> radiiIgnores;

	private static NativeArray<bool> waterIgnoreResults;

	private static NativeArray<bool> instanceDoDeepWaterChecksStateArray;

	private static NativeArray<int> instancePointCountNativeArray;

	[ServerVar(Help = "(Generated) When enabled, buoyancy point physics updates are batched together each fixed update for better CPU efficiency")]
	public static bool use_batching = true;

	private Action _sleepCallback;

	private Action _wakeCallback;

	public float timeOutOfWater { get; private set; }

	public bool InWater => submergedFraction > requiredSubmergedFraction;

	bool IPrefabPreProcess.CanRunDuringBundling => false;

	public Priority BuoyancyPriority { get; set; }

	public static void Cleanup()
	{
	}

	public void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		if (!UnityEngine.Application.isPlaying || serverside)
		{
			SavePointData(forced: false);
		}
	}

	public void SavePointData(bool forced)
	{
		if (points == null || points.Length == 0)
		{
			Rigidbody rigidbody = GetComponent<Rigidbody>();
			if (rigidbody == null)
			{
				rigidbody = base.gameObject.AddComponent<Rigidbody>();
			}
			GameObject obj = new GameObject("BuoyancyPoint");
			obj.transform.parent = rigidbody.gameObject.transform;
			obj.transform.localPosition = rigidbody.centerOfMass;
			BuoyancyPoint buoyancyPoint = obj.AddComponent<BuoyancyPoint>();
			buoyancyPoint.buoyancyForce = rigidbody.mass * (0f - UnityEngine.Physics.gravity.y);
			buoyancyPoint.buoyancyForce *= 1.32f;
			buoyancyPoint.size = 0.2f;
			points = new BuoyancyPoint[1];
			points[0] = buoyancyPoint;
		}
		if (pointData == null || pointData.Length != points.Length || forced)
		{
			pointData = new BuoyancyPointData[points.Length];
			for (int i = 0; i < points.Length; i++)
			{
				Transform transform = points[i].transform;
				pointData[i].localPosition = transform.localPosition;
				pointData[i].rootToPoint = base.transform.InverseTransformPoint(transform.position);
			}
		}
	}

	public void SetBuoyancyPointLocations(List<Vector3> locations)
	{
		if (points == null || points.Length != locations.Count)
		{
			Debug.LogWarning("Trying to SetBuoyancyPointLocations with mismatching array lengths");
			return;
		}
		int num = 0;
		BuoyancyPoint[] array = points;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].transform.position = locations[num];
			num++;
		}
	}

	public static string DefaultWaterImpact()
	{
		return "assets/bundled/prefabs/fx/impacts/physics/water-enter-exit.prefab";
	}

	private void Awake()
	{
		forVehicle = forEntity as BaseVehicle;
		InvokeRandomized(CheckSleepState, 0.5f, 5f, 1f);
	}

	public void Sleep()
	{
		if ((forEntity == null || !forEntity.BuoyancySleep(InWater)) && rigidBody != null)
		{
			rigidBody.Sleep();
		}
		base.enabled = false;
	}

	private static void ClearArrays()
	{
		NativeArrayEx.MemClear(in allPositions2D);
		NativeArrayEx.MemClear(in allPositions3D);
		NativeArrayEx.MemClear(in allUVPositions);
		NativeArrayEx.MemClear(in pointShoreDistanceNativeArray);
		NativeArrayEx.MemClear(in pointTerrainHeightNativeArray);
		NativeArrayEx.MemClear(in pointWaterHeightNativeArray);
		NativeArrayEx.MemClear(in pointWaterInfoNativeArray);
		NativeArrayEx.MemClear(in radiiIgnores);
		NativeArrayEx.MemClear(in waterIgnoreResults);
		NativeArrayEx.MemClear(in instanceDoDeepWaterChecksStateArray);
		NativeArrayEx.MemClear(in instancePointCountNativeArray);
	}

	private static void EnsureNativeArrayLength(int maxPoints, int maxInstances)
	{
		if (maxPoints > allPositions2D.Length || maxInstances > instancePointCountNativeArray.Length)
		{
			DisposeNativeArrays();
			allPositions2D = new NativeArray<Vector2>(maxPoints, Allocator.Persistent);
			allPositions3D = new NativeArray<Vector3>(maxPoints, Allocator.Persistent);
			allUVPositions = new NativeArray<Vector2>(maxPoints, Allocator.Persistent);
			pointWaterHeightNativeArray = new NativeArray<float>(maxPoints, Allocator.Persistent);
			pointShoreDistanceNativeArray = new NativeArray<float>(maxPoints, Allocator.Persistent);
			pointTerrainHeightNativeArray = new NativeArray<float>(maxPoints, Allocator.Persistent);
			pointWaterHeightNativeArray = new NativeArray<float>(maxPoints, Allocator.Persistent);
			pointWaterInfoNativeArray = new NativeArray<WaterLevel.WaterInfo>(maxPoints, Allocator.Persistent);
			radiiIgnores = new NativeArray<float>(maxPoints, Allocator.Persistent);
			waterIgnoreResults = new NativeArray<bool>(maxPoints, Allocator.Persistent);
			instanceDoDeepWaterChecksStateArray = new NativeArray<bool>(maxInstances, Allocator.Persistent);
			instancePointCountNativeArray = new NativeArray<int>(maxInstances, Allocator.Persistent);
		}
	}

	private static void DisposeNativeArrays()
	{
		NativeArrayEx.SafeDispose(ref allPositions2D);
		NativeArrayEx.SafeDispose(ref allPositions3D);
		NativeArrayEx.SafeDispose(ref allUVPositions);
		NativeArrayEx.SafeDispose(ref pointShoreDistanceNativeArray);
		NativeArrayEx.SafeDispose(ref pointTerrainHeightNativeArray);
		NativeArrayEx.SafeDispose(ref pointWaterHeightNativeArray);
		NativeArrayEx.SafeDispose(ref pointWaterInfoNativeArray);
		NativeArrayEx.SafeDispose(ref radiiIgnores);
		NativeArrayEx.SafeDispose(ref waterIgnoreResults);
		NativeArrayEx.SafeDispose(ref instanceDoDeepWaterChecksStateArray);
		NativeArrayEx.SafeDispose(ref instancePointCountNativeArray);
	}

	public void Wake()
	{
		if ((forEntity == null || !forEntity.BuoyancyWake()) && rigidBody != null)
		{
			rigidBody.WakeUp();
		}
		base.enabled = true;
	}

	public void CheckSleepState()
	{
		if (base.transform == null || rigidBody == null)
		{
			return;
		}
		hasLocalPlayers = HasLocalPlayers();
		bool flag = rigidBody.IsSleeping() || rigidBody.isKinematic;
		bool flag2 = flag || (!hasLocalPlayers && timeInWater > 6f);
		if (forVehicle != null && forVehicle.IsOn())
		{
			flag2 = false;
		}
		if (base.enabled && flag2)
		{
			if (_sleepCallback == null)
			{
				_sleepCallback = Sleep;
			}
			Invoke(_sleepCallback, 0f);
			return;
		}
		if (!base.enabled && hasLocalPlayers && !hadLocalPlayers)
		{
			DoCycle(forced: true);
		}
		bool flag3 = !flag || ShouldWake(hasLocalPlayers);
		if (!base.enabled && flag3)
		{
			if (_wakeCallback == null)
			{
				_wakeCallback = Wake;
			}
			Invoke(_wakeCallback, 0f);
		}
		hadLocalPlayers = hasLocalPlayers;
	}

	public void LowPriorityCheck(bool forceHighPriority)
	{
		Priority buoyancyPriority = BuoyancyPriority;
		Priority priority = buoyancyPriority;
		if (forceHighPriority)
		{
			priority = Priority.High;
		}
		else
		{
			Vector3 position = base.transform.position;
			priority = ((!BaseNetworkable.HasCloseConnections(position, Server.lowPriorityBuoyancyRange)) ? Priority.Low : Priority.High);
			if (priority == Priority.Low && priority != buoyancyPriority)
			{
				Vector3 vector = base.transform.TransformPoint(Vector3.forward * 2f).WithY(position.y);
				rigidBody.rotation = Quaternion.LookRotation((vector - rigidBody.position).normalized, Vector3.up);
			}
		}
		if (priority != buoyancyPriority)
		{
			rigidBody.linearVelocity = Vector3.zero;
			rigidBody.angularVelocity = Vector3.zero;
			BuoyancyPriority = priority;
		}
	}

	public bool ShouldWake()
	{
		return ShouldWake(HasLocalPlayers());
	}

	public bool ShouldWake(bool hasLocalPlayers)
	{
		return hasLocalPlayers;
	}

	private bool HasLocalPlayers()
	{
		return BaseNetworkable.HasCloseConnections(base.transform.position, 100f);
	}

	private static void DoCycleBatched(ReadOnlySpan<Buoyancy> buoyancies, bool forced)
	{
		if (buoyancies.Length == 0)
		{
			return;
		}
		PooledArray<bool> wasSubmergedStates = new PooledArray<bool>(buoyancies.Length);
		try
		{
			PooledArray<bool> wasSubmergedStates2 = new PooledArray<bool>(buoyancies.Length);
			try
			{
				using PooledList<Buoyancy> pooledList2 = Facepunch.Pool.Get<PooledList<Buoyancy>>();
				using PooledList<Buoyancy> pooledList = Facepunch.Pool.Get<PooledList<Buoyancy>>();
				int num = 0;
				int num2 = 0;
				for (int i = 0; i < buoyancies.Length; i++)
				{
					Buoyancy buoyancy = buoyancies[i];
					if (!(buoyancy == null) && !(buoyancy.rigidBody == null))
					{
						if (buoyancy.buoyancyScale == 0f)
						{
							buoyancy.Sleep();
						}
						else if (DeepSeaManager.IsInsideDeepSea(buoyancy.transform.position))
						{
							pooledList.Add(buoyancy);
							num2 += buoyancy.GetPointDataCount();
							wasSubmergedStates2[pooledList.Count - 1] = buoyancy.submergedFraction > 0f;
						}
						else
						{
							num += buoyancy.GetPointDataCount();
							pooledList2.Add(buoyancy);
							wasSubmergedStates[pooledList2.Count - 1] = buoyancy.submergedFraction > 0f;
						}
					}
				}
				int count = pooledList2.Count;
				int count2 = pooledList.Count;
				int maxPoints = math.max(num, num2);
				int maxInstances = math.max(count, count2);
				EnsureNativeArrayLength(maxPoints, maxInstances);
				if (count > 0)
				{
					ClearArrays();
					BuoyancyFixedUpdateBatched(pooledList2.ListAsReadOnlySpan(), isDeepSea: false);
				}
				if (count2 > 0)
				{
					ClearArrays();
					BuoyancyFixedUpdateBatched(pooledList.ListAsReadOnlySpan(), isDeepSea: true);
				}
				UpdateSubmergedState(pooledList2.ListAsReadOnlySpan(), wasSubmergedStates);
				UpdateSubmergedState(pooledList.ListAsReadOnlySpan(), wasSubmergedStates2);
			}
			finally
			{
				((IDisposable)wasSubmergedStates2).Dispose();
			}
		}
		finally
		{
			((IDisposable)wasSubmergedStates).Dispose();
		}
	}

	private static void UpdateSubmergedState(ReadOnlySpan<Buoyancy> buoyancies, PooledArray<bool> wasSubmergedStates)
	{
		for (int i = 0; i < buoyancies.Length; i++)
		{
			Buoyancy buoyancy = buoyancies[i];
			Rigidbody rigidbody = buoyancy.rigidBody;
			bool flag = buoyancy.submergedFraction > 0f;
			if (wasSubmergedStates[i] == flag)
			{
				continue;
			}
			if (buoyancy.useUnderwaterDrag && rigidbody != null)
			{
				if (flag)
				{
					buoyancy.defaultDrag = rigidbody.linearDamping;
					buoyancy.defaultAngularDrag = rigidbody.angularDamping;
					rigidbody.linearDamping = buoyancy.underwaterDrag;
					rigidbody.angularDamping = buoyancy.underwaterDrag;
				}
				else
				{
					rigidbody.linearDamping = buoyancy.defaultDrag;
					rigidbody.angularDamping = buoyancy.defaultAngularDrag;
				}
			}
			buoyancy.SubmergedChanged?.Invoke(flag);
		}
	}

	protected void DoCycle(bool forced = false)
	{
		if (!base.enabled && !forced)
		{
			return;
		}
		bool num = submergedFraction > 0f;
		BuoyancyFixedUpdate();
		bool flag = submergedFraction > 0f;
		if (num == flag)
		{
			return;
		}
		if (useUnderwaterDrag && rigidBody != null)
		{
			if (flag)
			{
				defaultDrag = rigidBody.linearDamping;
				defaultAngularDrag = rigidBody.angularDamping;
				rigidBody.linearDamping = underwaterDrag;
				rigidBody.angularDamping = underwaterDrag;
			}
			else
			{
				rigidBody.linearDamping = defaultDrag;
				rigidBody.angularDamping = defaultAngularDrag;
			}
		}
		if (SubmergedChanged != null)
		{
			SubmergedChanged(flag);
		}
	}

	public static void Cycle()
	{
		bool autoSyncTransforms = UnityEngine.Physics.autoSyncTransforms;
		try
		{
			UnityEngine.Physics.autoSyncTransforms = false;
			Buoyancy[] buffer = ListComponent<Buoyancy>.InstanceList.Values.Buffer;
			int count = ListComponent<Buoyancy>.InstanceList.Count;
			if (use_batching)
			{
				DoCycleBatched(ListComponent<Buoyancy>.InstanceList.Values.ContentReadOnlySpan(), forced: false);
				return;
			}
			for (int i = 0; i < count; i++)
			{
				buffer[i].DoCycle();
			}
		}
		finally
		{
			if (autoSyncTransforms)
			{
				UnityEngine.Physics.SyncTransforms();
			}
			UnityEngine.Physics.autoSyncTransforms = autoSyncTransforms;
		}
	}

	private static Vector3 GetFlowDirection(Vector3 worldPos)
	{
		return WaterLevel.GetWaterFlowDirection(worldPos);
	}

	private static void BuoyancyFixedUpdateBatched(ReadOnlySpan<Buoyancy> activeComponents, bool isDeepSea)
	{
		Bounds deepSeaBounds = DeepSeaManager.DeepSeaBounds;
		Vector3 terrainPosition = TerrainMeta.Position;
		Vector3 terrainOneOverSize = TerrainMeta.OneOverSize;
		int pointIndexOffset = 0;
		for (int i = 0; i < activeComponents.Length; i++)
		{
			Buoyancy buoyancy = activeComponents[i];
			if (buoyancy.BuoyancyPriority == Priority.High)
			{
				NativeArray<BuoyancyPointData> nativeArray = buoyancy.pointData.CopyToNativeArray(Allocator.Temp);
				Matrix4x4 rootToWorld = buoyancy.transform.localToWorldMatrix;
				BuoyancyBurstUtility.FillPointData(in pointIndexOffset, ref allPositions2D, ref allUVPositions, in rootToWorld, ref nativeArray, in deepSeaBounds, in terrainPosition, in terrainOneOverSize, in isDeepSea, ref allPositions3D, out var pointCount);
				pointIndexOffset += pointCount;
				instancePointCountNativeArray[i] = pointCount;
				nativeArray.Dispose();
			}
			else
			{
				allPositions2D[pointIndexOffset] = buoyancy.transform.position;
				allPositions3D[pointIndexOffset] = buoyancy.transform.position;
				pointIndexOffset++;
				instancePointCountNativeArray[i] = 1;
			}
			instanceDoDeepWaterChecksStateArray[i] = !buoyancy.ArtificialHeight.HasValue;
		}
		NativeArray<Vector2> pos = allPositions2D.GetSubArray(0, pointIndexOffset);
		NativeArray<Vector3> allPositions = allPositions3D.GetSubArray(0, pointIndexOffset);
		NativeArray<Vector2> posUV = allUVPositions.GetSubArray(0, pointIndexOffset);
		NativeArray<float> shore = pointShoreDistanceNativeArray.GetSubArray(0, pointIndexOffset);
		NativeArray<float> terrainHeight = pointTerrainHeightNativeArray.GetSubArray(0, pointIndexOffset);
		NativeArray<float> waterHeight = pointWaterHeightNativeArray.GetSubArray(0, pointIndexOffset);
		NativeArray<WaterLevel.WaterInfo> pointWaterInfo = pointWaterInfoNativeArray.GetSubArray(0, pointIndexOffset);
		NativeArray<float> subArray = radiiIgnores.GetSubArray(0, pointIndexOffset);
		NativeArray<bool> waterIgnoreStates = waterIgnoreResults.GetSubArray(0, pointIndexOffset);
		NativeArray<int> subArray2 = instancePointCountNativeArray.GetSubArray(0, activeComponents.Length);
		WaterSystem.GetHeightArray(in pos, in posUV, ref shore, ref terrainHeight, ref waterHeight, isDeepSea);
		TerrainTopologyMap.TopologyQueryStructure topologyMap = TerrainMeta.TopologyMap.GetQueryStructure();
		FillJob<float> fillJob = default(FillJob<float>);
		fillJob.Values = subArray;
		fillJob.Value = 0.01f;
		FillJob<float> jobData = fillJob;
		IJobExtensions.RunByRef(ref jobData);
		WaterSystem.Collision?.GetIgnore(allPositions.AsReadOnly(), subArray.AsReadOnly(), waterIgnoreStates);
		NativeArray<bool> needsDeepWaterChecks = new NativeArray<bool>(pointWaterInfo.Length, Allocator.Temp);
		int instanceCount = activeComponents.Length;
		WaterLevelBurst.GetBuoyancyWaterInfoBatched(in allPositions, in posUV, in terrainHeight, in waterHeight, in instanceDoDeepWaterChecksStateArray, ref pointWaterInfo, in subArray2, in instanceCount, in topologyMap, in waterIgnoreStates, ref needsDeepWaterChecks, isDeepSea, out var hasAnyDeepWaterChecks);
		if (hasAnyDeepWaterChecks)
		{
			WaterLevelBurst.ConstructDeepWaterCommands(in allPositions, in pointWaterInfo, in needsDeepWaterChecks, out var deepWaterCasts, out var raycastPointIndices, Allocator.TempJob);
			if (deepWaterCasts.Length > 0)
			{
				NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(deepWaterCasts.Length, Allocator.TempJob);
				RaycastCommand.ScheduleBatch(deepWaterCasts, results, 1).Complete();
				for (int j = 0; j < deepWaterCasts.Length; j++)
				{
					if (results[j].colliderInstanceID != 0)
					{
						int index = raycastPointIndices[j];
						WaterLevel.WaterInfo waterInfo = pointWaterInfo[index];
						float num = Mathf.Min(waterInfo.surfaceLevel, results[j].collider.bounds.max.y);
						waterInfo.currentDepth = Mathf.Max(0f, num - allPositions[index].y);
						waterInfo.overallDepth = Mathf.Max(0f, num - terrainHeight[index]);
						waterInfo.surfaceLevel = num;
					}
				}
				results.Dispose();
			}
			deepWaterCasts.Dispose();
		}
		float time = UnityEngine.Time.time;
		float fixedDeltaTime = UnityEngine.Time.fixedDeltaTime;
		NativeArray<BuoyancyForceAccumulationBurst.InstanceInput> instances = new NativeArray<BuoyancyForceAccumulationBurst.InstanceInput>(activeComponents.Length, Allocator.Temp);
		NativeArray<BuoyancyForceAccumulationBurst.InstanceOutput> results2 = new NativeArray<BuoyancyForceAccumulationBurst.InstanceOutput>(activeComponents.Length, Allocator.Temp);
		NativeArray<float> pointSize = new NativeArray<float>(pointIndexOffset, Allocator.Temp);
		NativeArray<float> pointBuoyancyForce = new NativeArray<float>(pointIndexOffset, Allocator.Temp);
		NativeArray<float> pointRandomOffset = new NativeArray<float>(pointIndexOffset, Allocator.Temp);
		NativeArray<float> pointWaveFrequency = new NativeArray<float>(pointIndexOffset, Allocator.Temp);
		NativeArray<float> pointWaveScale = new NativeArray<float>(pointIndexOffset, Allocator.Temp);
		int num2 = 0;
		for (int k = 0; k < activeComponents.Length; k++)
		{
			Buoyancy buoyancy2 = activeComponents[k];
			int num3 = subArray2[k];
			Rigidbody rigidbody = buoyancy2.rigidBody;
			instances[k] = new BuoyancyForceAccumulationBurst.InstanceInput
			{
				pointStartIndex = num2,
				pointCount = num3,
				buoyancyScale = buoyancy2.buoyancyScale,
				rigidBodyMass = ((rigidbody != null) ? rigidbody.mass : 0f),
				wavesEffect = buoyancy2.wavesEffect,
				scaleForceWithMass = buoyancy2.scaleForceWithMass,
				flowForceDisabled = buoyancy2.FlowForceDisabled,
				flowMovementScale = buoyancy2.flowMovementScale,
				worldCom = ((rigidbody != null) ? ((float3)rigidbody.worldCenterOfMass) : float3.zero)
			};
			if (buoyancy2.BuoyancyPriority == Priority.High)
			{
				for (int l = 0; l < buoyancy2.points.Length; l++)
				{
					BuoyancyPoint buoyancyPoint = buoyancy2.points[l];
					int index2 = num2 + l;
					pointSize[index2] = buoyancyPoint.size;
					pointBuoyancyForce[index2] = buoyancyPoint.buoyancyForce;
					pointRandomOffset[index2] = buoyancyPoint.randomOffset;
					pointWaveFrequency[index2] = buoyancyPoint.waveFrequency;
					pointWaveScale[index2] = buoyancyPoint.waveScale;
				}
			}
			else
			{
				int index3 = num2;
				if (buoyancy2.points != null && buoyancy2.points.Length != 0)
				{
					BuoyancyPoint buoyancyPoint2 = buoyancy2.points[0];
					pointSize[index3] = buoyancyPoint2.size;
					pointBuoyancyForce[index3] = buoyancyPoint2.buoyancyForce;
					pointRandomOffset[index3] = buoyancyPoint2.randomOffset;
					pointWaveFrequency[index3] = buoyancyPoint2.waveFrequency;
					pointWaveScale[index3] = buoyancyPoint2.waveScale;
				}
				else
				{
					pointSize[index3] = 0f;
					pointBuoyancyForce[index3] = 0f;
					pointRandomOffset[index3] = 0f;
					pointWaveFrequency[index3] = 0f;
					pointWaveScale[index3] = 0f;
				}
			}
			num2 += num3;
		}
		NativeArray<float3> pointFlowDirection = ((TerrainMeta.WaterFlowMap != null) ? TerrainMeta.WaterFlowMap.GetFlowDirections(allPositions, Allocator.Temp) : new NativeArray<float3>(pointIndexOffset, Allocator.Temp));
		NativeArray<float3> nativeArray2 = allPositions.Reinterpret<float3>();
		BuoyancyForceAccumulationBurst.Compute(in instances, in nativeArray2, in shore, in pointWaterInfo, in pointSize, in pointBuoyancyForce, in pointRandomOffset, in pointWaveFrequency, in pointWaveScale, in pointFlowDirection, time, ref results2);
		num2 = 0;
		for (int m = 0; m < activeComponents.Length; m++)
		{
			Buoyancy buoyancy3 = activeComponents[m];
			Rigidbody rigidbody2 = buoyancy3.rigidBody;
			if (buoyancy3.BuoyancyPriority == Priority.Low)
			{
				Vector3 vector = allPositions[num2];
				WaterLevel.WaterInfo waterInfo2 = pointWaterInfo[num2];
				if (vector.y < waterInfo2.surfaceLevel)
				{
					rigidbody2.position = new Vector3(vector.x, waterInfo2.surfaceLevel, vector.z);
				}
				num2++;
				continue;
			}
			int pointCount2 = instances[m].pointCount;
			int num4 = num2 + pointCount2;
			GameObjectRef[] array = buoyancy3.waterImpacts;
			bool flag = buoyancy3.doEffects;
			bool inWater = buoyancy3.InWater;
			BuoyancyForceAccumulationBurst.InstanceOutput instanceOutput = results2[m];
			int numSubmerged = instanceOutput.numSubmerged;
			Vector3 force = instanceOutput.netForce;
			Vector3 torque = instanceOutput.netTorque;
			int num5 = 0;
			for (int n = num2; n < num4; n++)
			{
				BuoyancyPoint buoyancyPoint3 = buoyancy3.points[num5];
				Vector3 localPosition = buoyancy3.pointData[num5].localPosition;
				Vector3 vector2 = allPositions[n];
				WaterLevel.WaterInfo waterInfo3 = pointWaterInfo[n];
				bool flag2 = waterInfo3.isValid && vector2.y < waterInfo3.surfaceLevel;
				if (buoyancyPoint3.doSplashEffects && ((!buoyancyPoint3.wasSubmergedLastFrame && flag2) || (!flag2 && buoyancyPoint3.wasSubmergedLastFrame)) && flag && rigidbody2.GetRelativePointVelocity(localPosition).magnitude > 1f)
				{
					string strName = ((array != null && array.Length != 0 && array[0].isValid) ? array[0].resourcePath : DefaultWaterImpact());
					Vector3 vector3 = new Vector3(UnityEngine.Random.Range(-0.25f, 0.25f), 0f, UnityEngine.Random.Range(-0.25f, 0.25f));
					Effect.server.Run(strName, vector2 + vector3, Vector3.up);
					buoyancyPoint3.nexSplashTime = time + 0.25f;
				}
				buoyancyPoint3.wasSubmergedLastFrame = flag2;
				num5++;
			}
			num2 += pointCount2;
			if (force.sqrMagnitude > 0f)
			{
				rigidbody2.AddForce(force, ForceMode.Force);
			}
			if (torque.sqrMagnitude > 0f)
			{
				rigidbody2.AddTorque(torque, ForceMode.Force);
			}
			if (pointCount2 > 0)
			{
				buoyancy3.submergedFraction = (float)numSubmerged / (float)pointCount2;
			}
			if (inWater)
			{
				buoyancy3.timeInWater += fixedDeltaTime;
				buoyancy3.timeOutOfWater = 0f;
			}
			else
			{
				buoyancy3.timeOutOfWater += fixedDeltaTime;
				buoyancy3.timeInWater = 0f;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int GetPointDataCount()
	{
		if (BuoyancyPriority == Priority.High)
		{
			return pointData.Length;
		}
		return 1;
	}

	public void BuoyancyFixedUpdate()
	{
		if (rigidBody == null)
		{
			return;
		}
		if (buoyancyScale == 0f)
		{
			Invoke(Sleep, 0f);
			return;
		}
		if (BuoyancyPriority == Priority.Low)
		{
			WaterLevel.WaterInfo waterInfo = WaterLevel.GetWaterInfo(base.transform.position, waves: true, volumes: true, forEntity);
			Vector3 position = rigidBody.position;
			if (position.y < waterInfo.surfaceLevel)
			{
				rigidBody.position = new Vector3(position.x, waterInfo.surfaceLevel, position.z);
			}
			return;
		}
		if (!initedPointArrays)
		{
			InitPointArrays();
		}
		float time = UnityEngine.Time.time;
		Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
		bool flag = DeepSeaManager.IsInsideDeepSea(localToWorldMatrix.GetPosition());
		float x;
		float z;
		float x2;
		float z2;
		if (flag)
		{
			x = DeepSeaManager.DeepSeaBounds.min.x;
			z = DeepSeaManager.DeepSeaBounds.min.z;
			x2 = DeepSeaManager.DeepSeaBounds.size.Inverse().x;
			z2 = DeepSeaManager.DeepSeaBounds.size.Inverse().z;
		}
		else
		{
			x = TerrainMeta.Position.x;
			z = TerrainMeta.Position.z;
			x2 = TerrainMeta.OneOverSize.x;
			z2 = TerrainMeta.OneOverSize.z;
		}
		for (int i = 0; i < pointData.Length; i++)
		{
			Vector3 position2 = localToWorldMatrix.MultiplyPoint3x4(pointData[i].rootToPoint);
			pointData[i].position = position2;
			float x3 = (position2.x - x) * x2;
			float y = (position2.z - z) * z2;
			pointPositionArray[i] = new Vector2(position2.x, position2.z);
			pointPositionUVArray[i] = new Vector2(x3, y);
		}
		WaterSystem.GetHeightArray(pointPositionArray, pointPositionUVArray, pointShoreDistanceArray, pointTerrainHeightArray, pointWaterHeightArray, flag);
		bool flag2 = wavesEffect < 1f;
		int num = 0;
		for (int j = 0; j < points.Length; j++)
		{
			BuoyancyPoint buoyancyPoint = points[j];
			Vector3 pos = pointData[j].position;
			Vector3 localPosition = pointData[j].localPosition;
			Vector2 posUV = pointPositionUVArray[j];
			float terrainHeight = pointTerrainHeightArray[j];
			float num2 = pointWaterHeightArray[j];
			if (ArtificialHeight.HasValue)
			{
				num2 = ArtificialHeight.Value;
			}
			else if (flag2)
			{
				num2 = Mathf.Lerp(0f, num2, wavesEffect);
			}
			bool doDeepwaterChecks = !ArtificialHeight.HasValue;
			WaterLevel.WaterInfo waterInfo2 = WaterLevel.GetBuoyancyWaterInfo(pos, posUV, terrainHeight, num2, doDeepwaterChecks, forEntity);
			if (flag2 && waterInfo2.isValid)
			{
				waterInfo2.currentDepth = Mathf.Lerp(waterInfo2.currentDepth, waterInfo2.surfaceLevel - pos.y, wavesEffect);
			}
			bool flag3 = false;
			if (pos.y < waterInfo2.surfaceLevel && waterInfo2.isValid)
			{
				flag3 = true;
				num++;
				float currentDepth = waterInfo2.currentDepth;
				float num3 = Mathf.InverseLerp(0f, buoyancyPoint.size, currentDepth);
				float num4 = 1f + Mathf.PerlinNoise(buoyancyPoint.randomOffset + time * buoyancyPoint.waveFrequency, 0f) * buoyancyPoint.waveScale;
				float scaledBuoyancyForce = buoyancyPoint.buoyancyForce * buoyancyScale;
				if (scaleForceWithMass)
				{
					scaledBuoyancyForce *= rigidBody.mass;
				}
				Vector3 accumForce = new Vector3(0f, num4 * num3 * scaledBuoyancyForce, 0f);
				float shoreDistance = Mathf.Abs(pointShoreDistanceArray[j]);
				AccumulateFlowForce(ref accumForce, in pos, in waterInfo2, in shoreDistance, ref scaledBuoyancyForce, in FlowForceDisabled, in flowMovementScale);
				rigidBody.AddForceAtPosition(accumForce, pos, ForceMode.Force);
			}
			if (buoyancyPoint.doSplashEffects && ((!buoyancyPoint.wasSubmergedLastFrame && flag3) || (!flag3 && buoyancyPoint.wasSubmergedLastFrame)) && doEffects && rigidBody.GetRelativePointVelocity(localPosition).magnitude > 1f)
			{
				string strName = ((waterImpacts != null && waterImpacts.Length != 0 && waterImpacts[0].isValid) ? waterImpacts[0].resourcePath : DefaultWaterImpact());
				Vector3 vector = new Vector3(UnityEngine.Random.Range(-0.25f, 0.25f), 0f, UnityEngine.Random.Range(-0.25f, 0.25f));
				Effect.server.Run(strName, pos + vector, Vector3.up);
				buoyancyPoint.nexSplashTime = UnityEngine.Time.time + 0.25f;
			}
			buoyancyPoint.wasSubmergedLastFrame = flag3;
		}
		if (points.Length != 0)
		{
			submergedFraction = (float)num / (float)points.Length;
		}
		if (InWater)
		{
			timeInWater += UnityEngine.Time.fixedDeltaTime;
			timeOutOfWater = 0f;
		}
		else
		{
			timeOutOfWater += UnityEngine.Time.fixedDeltaTime;
			timeInWater = 0f;
		}
	}

	public static void AccumulateFlowForce(ref Vector3 accumForce, in Vector3 pos, in WaterLevel.WaterInfo waterInfo, in float shoreDistance, ref float scaledBuoyancyForce, in bool flowForceDisabled, in float flowMovementScale)
	{
		if (!waterInfo.artificalWater && !flowForceDisabled && (waterInfo.topology & 0x10000) == 0)
		{
			float num = Mathf.Clamp01(Mathf.InverseLerp(60f, 0f, shoreDistance));
			if (!(num <= Mathf.Epsilon))
			{
				num = Mathf.Pow(num, 0.5f);
				Vector3 flowDirection = GetFlowDirection(pos);
				scaledBuoyancyForce *= 0.025f * num;
				accumForce.x += flowDirection.x * scaledBuoyancyForce * flowMovementScale;
				accumForce.z += flowDirection.z * scaledBuoyancyForce * flowMovementScale;
			}
		}
	}

	private void InitPointArrays()
	{
		pointPositionArray = new Vector2[points.Length];
		pointPositionUVArray = new Vector2[points.Length];
		pointShoreDistanceArray = new float[points.Length];
		pointTerrainHeightArray = new float[points.Length];
		pointWaterHeightArray = new float[points.Length];
		initedPointArrays = true;
	}
}
