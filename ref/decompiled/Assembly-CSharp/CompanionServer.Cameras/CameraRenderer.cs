using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;

namespace CompanionServer.Cameras;

public class CameraRenderer : Pool.IPooled
{
	[ServerVar(Help = "(Generated) When enabled, the companion server camera rendering system is active and processes camera render requests from the companion app")]
	public static bool enabled = true;

	[ServerVar(Help = "(Generated) Per-frame CPU budget in milliseconds for completing pending companion server camera renders")]
	public static float completionFrameBudgetMs = 5f;

	[ServerVar(Help = "(Generated) Maximum number of camera render tasks that can complete per frame for companion server cameras")]
	public static int maxRendersPerFrame = 25;

	[ServerVar(Help = "(Generated) Maximum number of raycasts per frame used for companion server camera depth sampling")]
	public static int maxRaysPerFrame = 100000;

	[ServerVar(Help = "(Generated) Width in pixels of the companion server camera render output; default 320")]
	public static int width = 320;

	[ServerVar(Help = "(Generated) Height in pixels of the companion server camera render output; default 180")]
	public static int height = 180;

	[ServerVar(Help = "(Generated) Vertical field of view in degrees for companion server camera renders; default 65")]
	public static float verticalFov = 65f;

	[ServerVar(Help = "(Generated) Near clipping plane distance for companion server camera renders; 0 = use default")]
	public static float nearPlane = 0f;

	[ServerVar(Help = "(Generated) Far clipping plane distance in metres for companion server camera renders; default 250")]
	public static float farPlane = 250f;

	[ServerVar(Help = "(Generated) Physics layer mask used for raycasting in companion server camera depth sampling; defaults to solid, water, and player movement layers")]
	public static int layerMask = 1218656529;

	[ServerVar(Help = "(Generated) Interval in seconds between successive companion server camera render dispatches; default 0.05s (20 Hz)")]
	public static float renderInterval = 0.05f;

	[ServerVar(Help = "(Generated) Number of raycast samples taken per companion server camera render pass for depth reconstruction")]
	public static int samplesPerRender = 3000;

	[ServerVar(Help = "(Generated) Maximum per-axis camera rotation jitter, in sample cells, applied to each companion server camera render so a stationary camera still returns a natural scatter of ray samples instead of a rigid grid; 0 disables")]
	public static float rayJitter = 0.5f;

	[ServerVar(Help = "(Generated) Maximum age in frames for a known collider entity entry in the companion server camera cache before it is evicted")]
	public static int entityMaxAge = 5;

	[ServerVar(Help = "(Generated) Maximum distance in metres from the companion server camera at which entity colliders are tracked for rendering")]
	public static int entityMaxDistance = 100;

	[ServerVar(Help = "(Generated) Maximum distance in metres at which player entities are included in companion server camera renders")]
	public static int playerMaxDistance = 30;

	[ServerVar(Help = "(Generated) Maximum distance in metres at which player name labels are included in companion server camera render output")]
	public static int playerNameMaxDistance = 10;

	[ServerVar(Help = "Enable developer-specific permissions for camera access (less restricted)")]
	public static bool developerPermissions = true;

	private readonly Dictionary<int, (byte MaterialIndex, int Age)> _knownColliders = new Dictionary<int, (byte, int)>();

	private readonly Dictionary<int, BaseEntity> _colliderToEntity = new Dictionary<int, BaseEntity>();

	private double _lastRenderTimestamp;

	private float _fieldOfView;

	private Matrix4x4 _renderTransform;

	private Quaternion _renderRotation;

	private int _sampleOffset;

	private int _nextSampleOffset;

	private int _sampleCount;

	private CameraRenderTask _task;

	private ulong? _cachedViewerSteamId;

	private BasePlayer _cachedViewer;

	private ulong _entityIdOffset;

	public CameraRendererState state;

	public IRemoteControllable rc;

	public BaseEntity entity;

	public CameraRenderer()
	{
		Reset();
	}

	public void EnterPool()
	{
		Reset();
	}

	public void LeavePool()
	{
	}

	public void Reset()
	{
		_knownColliders.Clear();
		_colliderToEntity.Clear();
		_lastRenderTimestamp = 0.0;
		_fieldOfView = 0f;
		_renderTransform = Matrix4x4.identity;
		_renderRotation = Quaternion.identity;
		_sampleOffset = 0;
		_nextSampleOffset = 0;
		_sampleCount = 0;
		if (_task != null)
		{
			CameraRendererManager instance = SingletonComponent<CameraRendererManager>.Instance;
			if (instance != null)
			{
				instance.ReturnTask(ref _task);
			}
		}
		_cachedViewerSteamId = null;
		_cachedViewer = null;
		state = CameraRendererState.Invalid;
		rc = null;
		entity = null;
	}

	public void Init(IRemoteControllable remoteControllable)
	{
		if (remoteControllable == null)
		{
			throw new ArgumentNullException("remoteControllable");
		}
		rc = remoteControllable;
		entity = remoteControllable.GetEnt();
		if (entity == null || !entity.IsValid())
		{
			throw new ArgumentException("RemoteControllable's entity is null or invalid", "rc");
		}
		_entityIdOffset = (ulong)UnityEngine.Random.Range(1, 100000);
		state = CameraRendererState.WaitingToRender;
	}

	public bool CanRender()
	{
		if (state != CameraRendererState.WaitingToRender)
		{
			return false;
		}
		if (TimeEx.realtimeSinceStartup - _lastRenderTimestamp < (double)renderInterval)
		{
			return false;
		}
		return true;
	}

	public void Render(int maxSampleCount)
	{
		CameraRendererManager instance = SingletonComponent<CameraRendererManager>.Instance;
		if (instance == null)
		{
			state = CameraRendererState.Invalid;
			return;
		}
		if (state != CameraRendererState.WaitingToRender)
		{
			throw new InvalidOperationException($"CameraRenderer cannot render in state {state}");
		}
		if (ObjectEx.IsUnityNull(rc) || !entity.IsValid())
		{
			state = CameraRendererState.Invalid;
			return;
		}
		if (rc.GetEyes() == null)
		{
			state = CameraRendererState.Invalid;
			return;
		}
		if (_task != null)
		{
			Debug.LogError("CameraRenderer: Trying to render but a task is already allocated?", entity);
			instance.ReturnTask(ref _task);
		}
		Matrix4x4 transf = rc.GetEyesMatrix();
		_fieldOfView = verticalFov / Mathf.Clamp(rc.GetFovScale(), 1f, 8f);
		_renderRotation = transf.rotation;
		if (rayJitter > 0f)
		{
			float num = 2f * Mathf.Tan(MathF.PI / 360f * _fieldOfView);
			float num2 = 57.29578f * num / (float)height;
			float x = UnityEngine.Random.Range(0f - rayJitter, rayJitter) * num2;
			float y = UnityEngine.Random.Range(0f - rayJitter, rayJitter) * num2;
			Quaternion q = transf.rotation * Quaternion.Euler(x, y, 0f);
			transf = Matrix4x4.TRS(transf.GetPosition(), q, Vector3.one);
		}
		_renderTransform = transf;
		_sampleCount = Mathf.Clamp(samplesPerRender, 1, Mathf.Min(width * height, maxSampleCount));
		_task = instance.BorrowTask();
		_nextSampleOffset = _task.Start(width, height, _fieldOfView, nearPlane, farPlane, layerMask, in transf, _sampleCount, _sampleOffset, _knownColliders);
		state = CameraRendererState.Rendering;
	}

	public void CompleteRender()
	{
		CameraRendererManager instance = SingletonComponent<CameraRendererManager>.Instance;
		if (instance == null)
		{
			state = CameraRendererState.Invalid;
			return;
		}
		if (state != CameraRendererState.Rendering)
		{
			throw new InvalidOperationException($"CameraRenderer cannot complete render in state {state}");
		}
		if (_task == null)
		{
			Debug.LogError("CameraRenderer: Trying to complete render but no task is allocated?", this.entity);
			state = CameraRendererState.Invalid;
		}
		else
		{
			if (_task.keepWaiting)
			{
				return;
			}
			if (ObjectEx.IsUnityNull(rc) || !this.entity.IsValid())
			{
				instance.ReturnTask(ref _task);
				state = CameraRendererState.Invalid;
				return;
			}
			if (!(rc.GetEyes() == null))
			{
				int minSize = _sampleCount * 4;
				byte[] array = BufferStream.Shared.ArrayPool.Rent(minSize);
				List<int> obj = Pool.Get<List<int>>();
				List<int> obj2 = Pool.Get<List<int>>();
				int count = _task.ExtractRayData(array, obj, obj2);
				instance.ReturnTask(ref _task);
				UpdateCollidersMap(obj2);
				Pool.FreeUnmanaged(ref obj);
				Pool.FreeUnmanaged(ref obj2);
				ulong num = rc.ControllingViewerId?.SteamId ?? 0;
				if (num == 0L)
				{
					_cachedViewerSteamId = null;
					_cachedViewer = null;
				}
				else if (num != _cachedViewerSteamId)
				{
					_cachedViewerSteamId = num;
					_cachedViewer = BasePlayer.FindByID(num) ?? BasePlayer.FindSleeping(num);
				}
				float distance = (_cachedViewer.IsValid() ? Mathf.Clamp01(Vector3.Distance(_cachedViewer.transform.position, this.entity.transform.position) / rc.MaxRange) : 0f);
				Matrix4x4 renderTransform = _renderTransform;
				Vector3 position = renderTransform.GetPosition();
				Quaternion rotation = renderTransform.rotation;
				Matrix4x4 inverse = renderTransform.inverse;
				NetworkableId iD = this.entity.net.ID;
				using AppBroadcast appBroadcast = Pool.Get<AppBroadcast>();
				appBroadcast.cameraRays = Pool.Get<AppCameraRays>();
				appBroadcast.cameraRays.verticalFov = _fieldOfView;
				appBroadcast.cameraRays.sampleOffset = _sampleOffset;
				appBroadcast.cameraRays.rayData = new ArraySegment<byte>(array, 0, count);
				appBroadcast.cameraRays.distance = distance;
				appBroadcast.cameraRays.entities = Pool.Get<List<AppCameraRays.Entity>>();
				appBroadcast.cameraRays.timeOfDay = ((TOD_Sky.Instance != null) ? TOD_Sky.Instance.LerpValue : 1f);
				appBroadcast.cameraRays.cameraPosition = position;
				appBroadcast.cameraRays.cameraRotation = _renderRotation.eulerAngles * (MathF.PI / 180f);
				appBroadcast.cameraRays.sampleRotation = rotation.eulerAngles * (MathF.PI / 180f);
				foreach (BaseEntity value in _colliderToEntity.Values)
				{
					if (!value.IsValid())
					{
						continue;
					}
					Vector3 position2 = value.transform.position;
					float num2 = Vector3.Distance(position2, position);
					if (num2 > (float)entityMaxDistance)
					{
						continue;
					}
					string name = null;
					if (value is BasePlayer basePlayer)
					{
						if (num2 > (float)playerMaxDistance)
						{
							continue;
						}
						if (num2 <= (float)playerNameMaxDistance)
						{
							name = basePlayer.displayName;
						}
					}
					AppCameraRays.Entity entity = Pool.Get<AppCameraRays.Entity>();
					entity.type = ((value is TreeEntity) ? AppCameraRays.EntityType.Tree : AppCameraRays.EntityType.Player);
					entity.entityId = ObscureEntityId(value.net.ID);
					entity.position = inverse.MultiplyPoint3x4(position2);
					entity.rotation = (Quaternion.Inverse(value.transform.rotation) * rotation).eulerAngles * (MathF.PI / 180f);
					entity.size = Vector3.Scale(value.bounds.size, value.transform.localScale);
					entity.name = name;
					appBroadcast.cameraRays.entities.Add(entity);
				}
				appBroadcast.cameraRays.entities.Sort((AppCameraRays.Entity x, AppCameraRays.Entity y) => x.entityId.Value.CompareTo(y.entityId.Value));
				Server.Broadcast(new CameraTarget(iD), appBroadcast);
				_sampleOffset = _nextSampleOffset;
				if (!Server.HasAnySubscribers(new CameraTarget(iD)))
				{
					state = CameraRendererState.Invalid;
					return;
				}
				_lastRenderTimestamp = TimeEx.realtimeSinceStartup;
				state = CameraRendererState.WaitingToRender;
				return;
			}
			instance.ReturnTask(ref _task);
			state = CameraRendererState.Invalid;
		}
	}

	private void UpdateCollidersMap(List<int> foundColliderIds)
	{
		List<int> obj = Pool.Get<List<int>>();
		foreach (int key in _knownColliders.Keys)
		{
			obj.Add(key);
		}
		List<int> obj2 = Pool.Get<List<int>>();
		foreach (int item2 in obj)
		{
			if (_knownColliders.TryGetValue(item2, out (byte, int) value))
			{
				if (value.Item2 > entityMaxAge)
				{
					obj2.Add(item2);
				}
				else
				{
					_knownColliders[item2] = (value.Item1, value.Item2 + 1);
				}
			}
		}
		Pool.FreeUnmanaged(ref obj);
		foreach (int item3 in obj2)
		{
			_knownColliders.Remove(item3);
			_colliderToEntity.Remove(item3);
		}
		Pool.FreeUnmanaged(ref obj2);
		foreach (int foundColliderId in foundColliderIds)
		{
			if (_knownColliders.Count >= 512)
			{
				break;
			}
			Collider collider = CompanionServer.Cameras.CameraBurstUtil.GetCollider(foundColliderId);
			if (collider == null)
			{
				continue;
			}
			byte item;
			if (collider is TerrainCollider)
			{
				item = 1;
			}
			else
			{
				BaseEntity baseEntity = GameObjectEx.ToBaseEntity(collider);
				item = GetMaterialIndex(collider.sharedMaterial, baseEntity);
				if (baseEntity is TreeEntity || baseEntity is BasePlayer)
				{
					_colliderToEntity[foundColliderId] = baseEntity;
				}
			}
			_knownColliders[foundColliderId] = (item, 0);
		}
	}

	private NetworkableId ObscureEntityId(NetworkableId realId)
	{
		return new NetworkableId(realId.Value + _entityIdOffset);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static byte GetMaterialIndex(PhysicsMaterial material, BaseEntity entity)
	{
		switch (AssetNameCache.GetName(material))
		{
		case "Water":
			return 2;
		case "Rock":
			return 3;
		case "Stones":
			return 4;
		case "Wood":
			return 5;
		case "Metal":
			return 6;
		default:
			if (entity != null && entity is BasePlayer)
			{
				return 7;
			}
			return 0;
		}
	}
}
