#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Spatial;
using UnityEngine;
using UnityEngine.Assertions;

public class VineMountable : BaseMountable
{
	public struct VinePoint
	{
		public EntityRef<VineSwingingTree> TreeEntity;

		public int PointIndex;

		public VineLaunchPoint Get(bool isServer)
		{
			VineSwingingTree vineSwingingTree = TreeEntity.Get(isServer);
			if (vineSwingingTree != null)
			{
				return vineSwingingTree.LaunchPoints[PointIndex];
			}
			return null;
		}

		public void Set(VineLaunchPoint launchPoint)
		{
			TreeEntity.Set(launchPoint.ParentTree);
			PointIndex = launchPoint.Index();
		}

		public VineDestination Save()
		{
			VineDestination vineDestination = Facepunch.Pool.Get<VineDestination>();
			vineDestination.index = PointIndex;
			vineDestination.targetTree = TreeEntity.uid;
			return vineDestination;
		}

		public void Load(VineDestination destination)
		{
			PointIndex = destination.index;
			TreeEntity.uid = destination.targetTree;
		}
	}

	public float moveSpeed;

	[Header("Rotation Settings")]
	public float rotationSpeed = 0.5f;

	public float descendSpeed = 5f;

	public Vector3 WorldSpaceAnchorPoint;

	private List<VinePoint> destinations = new List<VinePoint>();

	private VinePoint origin;

	public VinePoint currentLocation;

	public const Flags Away = Flags.Reserved1;

	public const Flags Descending = Flags.Reserved2;

	public const Flags Finished = Flags.Reserved3;

	public ViewModel VineViewModel;

	public float DismountViewmodelHoldTime = 0.2f;

	public GameObjectRef VineWorldModel;

	public Transform[] VineDirectionArrows;

	public CapsuleCollider ThisCollider;

	[ServerVar]
	public static bool allowChaining = true;

	private static readonly int DescendHash = Animator.StringToHash("descend");

	private static readonly int VineDescendingHash = Animator.StringToHash("vineDescending");

	private VineLaunchPoint activeOriginPoint;

	private VineLaunchPoint activeDestinationPoint;

	private float currentTime;

	private Vector3 lastPosition;

	private bool isDescending;

	private bool wantsToSyncPos;

	private VineMountable chainTarget;

	private Vector3 lastValidLocation = Vector3.zero;

	private TimeSince lastValidLocationTime;

	private Action processMovementAction;

	private Action stopReplicatingPosCallback;

	private Action syncVineAtEndAction;

	public int DestinationCount => destinations.Count;

	public static Grid<VineMountable> pointGrid { get; private set; } = new Grid<VineMountable>();


	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("VineMountable.OnRpcMessage"))
		{
			if (rpc == 2800581258u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SV_Descend");
				}
				using (TimeWarning.New("SV_Descend"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2800581258u, "SV_Descend", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg2 = rPCMessage;
							SV_Descend(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SV_Descend");
					}
				}
				return true;
			}
			if (rpc == 2867502127u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SV_Swing");
				}
				using (TimeWarning.New("SV_Swing"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2867502127u, "SV_Swing", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg3 = rPCMessage;
							SV_Swing(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in SV_Swing");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public static void NotifyVinesLaunchSiteRemoved(VineLaunchPoint point)
	{
		using PooledList<VineMountable> pooledList = Facepunch.Pool.Get<PooledList<VineMountable>>();
		Vector3 position = point.transform.position;
		pointGrid.Query(position.x, position.z, 100f, pooledList);
		foreach (VineMountable item in pooledList)
		{
			if (item.origin.Get(isServer: true) == point)
			{
				item.Kill();
				continue;
			}
			for (int i = 0; i < item.destinations.Count; i++)
			{
				if (item.destinations[i].Get(isServer: true) == point)
				{
					item.destinations.RemoveAt(i);
					if (item.HasFlag(Flags.Reserved1) && item.destinations.Count > 0)
					{
						item.Swing(null, shouldMount: false);
					}
					i--;
				}
			}
			if (item.destinations.Count == 0)
			{
				item.Kill();
			}
			else
			{
				item.SendNetworkUpdate();
			}
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		pointGrid.Add(this, base.transform.position.x, base.transform.position.z);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		VineLaunchPoint vineLaunchPoint = origin.Get(isServer: true);
		if (vineLaunchPoint != null)
		{
			vineLaunchPoint.OnVineKilled();
		}
		pointGrid.Remove(this);
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		base.PlayerServerInput(inputState, player);
		chainTarget = null;
		if (!allowChaining || isDescending || !(currentTime > 0.5f) || !(activeDestinationPoint != null) || !inputState.IsDown(BUTTON.USE))
		{
			return;
		}
		using PooledList<VineMountable> pooledList = Facepunch.Pool.Get<PooledList<VineMountable>>();
		Vector3 position = activeDestinationPoint.transform.position;
		pointGrid.Query(position.x, position.z, 5f, pooledList);
		foreach (VineMountable item in pooledList)
		{
			if (item.isServer && item != this && item.Distance(position) < 5f && item.GetTargetDestination(base.transform.position, base.transform.forward, out var foundAngle) != null && foundAngle < 90f)
			{
				chainTarget = item;
				break;
			}
		}
	}

	public bool AttackedByPlayer(BasePlayer bp)
	{
		if (!ConVar.Server.allowVineSwinging)
		{
			return false;
		}
		float num = 2f;
		if (bp != null)
		{
			if (bp.Distance(this) < 2f)
			{
				return false;
			}
			if (HasFlag(Flags.Reserved1))
			{
				VineLaunchPoint vineLaunchPoint = origin.Get(base.isServer);
				if (vineLaunchPoint != null && bp.Distance(vineLaunchPoint.transform.position) < num)
				{
					Swing(null, shouldMount: false);
					return true;
				}
			}
			else
			{
				foreach (VinePoint destination in destinations)
				{
					VineLaunchPoint vineLaunchPoint2 = destination.Get(isServer: true);
					if (vineLaunchPoint2 != null && bp.Distance(vineLaunchPoint2.transform.position) < num)
					{
						Swing(null, shouldMount: false, vineLaunchPoint2);
						return true;
					}
				}
			}
		}
		return false;
	}

	private void ProcessMovement()
	{
		lastPosition = base.transform.position;
		VineLaunchPoint vineLaunchPoint = activeDestinationPoint;
		VineLaunchPoint vineLaunchPoint2 = activeOriginPoint;
		Flags flags = base.flags;
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		if (HasFlag(Flags.Reserved3))
		{
			flagsUpdateScope.Set(Flags.Reserved3, b: false);
		}
		currentTime = Mathf.MoveTowards(currentTime, 1f, moveSpeed * UnityEngine.Time.deltaTime);
		float time = Mathf.SmoothStep(0f, 1f, currentTime);
		if (isDescending && vineLaunchPoint2 != null)
		{
			if (GamePhysics.Trace(new Ray(base.transform.position, -Vector3.up), 0.2f, out var hitInfo, 50f, 1218519297, QueryTriggerInteraction.UseGlobal, this) && hitInfo.distance < 1.5f && !(RaycastHitEx.GetEntity(hitInfo) is VineMountable))
			{
				if (Vector3.Distance(base.transform.position, vineLaunchPoint2.transform.position) < 2f)
				{
					base.transform.position = vineLaunchPoint2.transform.position;
				}
				isDescending = false;
				flagsUpdateScope.Set(Flags.Reserved2, b: false);
				flagsUpdateScope.Set(Flags.Reserved1, b: false);
				OnArrived(null);
				if (!HasFlag(Flags.Reserved3))
				{
					flagsUpdateScope.Set(Flags.Reserved3, b: true);
				}
				if (flags == base.flags)
				{
					SendNetworkUpdate();
				}
				base.transform.position = vineLaunchPoint2.transform.position;
			}
			else
			{
				base.transform.Translate(-Vector3.up * descendSpeed * UnityEngine.Time.deltaTime);
			}
			return;
		}
		if (vineLaunchPoint == null || vineLaunchPoint2 == null)
		{
			DismountAllPlayers();
			VineLaunchPoint vineLaunchPoint3 = origin.Get(isServer: true);
			if (vineLaunchPoint3 != null)
			{
				OnArrived(vineLaunchPoint3);
			}
			return;
		}
		if ((float)lastValidLocationTime > 0.1f)
		{
			lastValidLocation = base.transform.position;
			lastValidLocationTime = 0f;
		}
		Vector3 swingPointAtTime = vineLaunchPoint2.GetSwingPointAtTime(time, vineLaunchPoint);
		Vector3 position = base.transform.position;
		Vector3 normalized = (swingPointAtTime - position).normalized;
		using PooledList<RaycastHit> pooledList = Facepunch.Pool.Get<PooledList<RaycastHit>>();
		float num = ThisCollider.height * 0.5f;
		Vector3 position2 = swingPointAtTime.WithY(swingPointAtTime.y - num);
		Vector3 position3 = swingPointAtTime.WithY(swingPointAtTime.y + num);
		GamePhysics.CapsuleSweep(position2, position3, ThisCollider.radius, normalized, Vector3.Distance(base.transform.position, swingPointAtTime) * 2f, pooledList, 2097152);
		if (pooledList.Count > 0)
		{
			if (Vector3.Distance(lastValidLocation, vineLaunchPoint2.transform.position) < 2f)
			{
				lastValidLocation = vineLaunchPoint2.transform.position;
			}
			base.transform.position = lastValidLocation;
			DismountAllPlayers();
			return;
		}
		base.transform.position = swingPointAtTime;
		Vector3 vector = swingPointAtTime - lastPosition;
		Quaternion quaternion = ((!(vector.normalized.sqrMagnitude > Mathf.Epsilon)) ? base.transform.rotation : Quaternion.LookRotation(vector.normalized, Vector3.up));
		float value = Mathf.Abs((base.transform.position.y - lastPosition.y) / UnityEngine.Time.deltaTime);
		float num2 = Mathf.Clamp01(Mathf.InverseLerp(0f, 6f, value));
		Vector3 eulerAngles = base.transform.rotation.eulerAngles;
		float y = Mathf.Clamp(quaternion.eulerAngles.y, 0f - num2, num2);
		Quaternion quaternion2 = Quaternion.Euler(eulerAngles.x, y, eulerAngles.z);
		Quaternion rotation = Quaternion.Slerp(base.transform.rotation, quaternion2 * quaternion, UnityEngine.Time.deltaTime * rotationSpeed);
		base.transform.rotation = rotation;
		if (currentTime >= 1f)
		{
			base.transform.position = vineLaunchPoint2.GetSwingPointAtTime(1f, vineLaunchPoint);
			OnArrived(vineLaunchPoint);
			if (!HasFlag(Flags.Reserved3))
			{
				flagsUpdateScope.Set(Flags.Reserved3, b: true);
			}
			if (flags == base.flags)
			{
				SendNetworkUpdate();
			}
		}
	}

	public void Initialise(VineLaunchPoint originPoint, List<VineLaunchPoint> destinationPoints, Vector3 anchor)
	{
		origin.Set(originPoint);
		currentLocation.Set(originPoint);
		destinations.Clear();
		foreach (VineLaunchPoint destinationPoint in destinationPoints)
		{
			VinePoint item = default(VinePoint);
			item.Set(destinationPoint);
			destinations.Add(item);
		}
		WorldSpaceAnchorPoint = anchor;
		Vector3 normalized = (destinationPoints[0].transform.position - base.transform.position).normalized;
		base.transform.rotation = Quaternion.LookRotation(normalized, Vector3.up);
		base.transform.localEulerAngles = base.transform.localEulerAngles.WithX(0f);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.vineMountable = Facepunch.Pool.Get<ProtoBuf.VineMountable>();
		info.msg.vineMountable.anchorPoint = WorldSpaceAnchorPoint;
		info.msg.vineMountable.originPoint = origin.Save();
		info.msg.vineMountable.currentLocation = currentLocation.Save();
		info.msg.vineMountable.destinations = Facepunch.Pool.Get<List<VineDestination>>();
		foreach (VinePoint destination in destinations)
		{
			info.msg.vineMountable.destinations.Add(destination.Save());
		}
	}

	public override float AntiHackVelocity()
	{
		if (ObjectEx.IsUnityNull(activeOriginPoint) || ObjectEx.IsUnityNull(activeDestinationPoint))
		{
			return 1f;
		}
		float num = Vector3.Distance(activeOriginPoint.transform.position, activeDestinationPoint.transform.position);
		float num2 = ((moveSpeed > 0f) ? (1f / moveSpeed) : 1f);
		return Mathf.Clamp(num / num2, 1f, 50f);
	}

	private float GetMaxVineDistance(Vector3 origin)
	{
		float num = 0f;
		foreach (VinePoint destination in destinations)
		{
			VineLaunchPoint vineLaunchPoint = destination.Get(base.isServer);
			if (vineLaunchPoint != null)
			{
				num = Mathf.Max(Vector3.Distance(vineLaunchPoint.transform.position, origin), num);
			}
		}
		if (num == 0f)
		{
			Debug.Log(" there are " + destinations.Count + " destinations");
			foreach (VinePoint destination2 in destinations)
			{
				VineLaunchPoint vineLaunchPoint2 = destination2.Get(isServer: false);
				if (vineLaunchPoint2 != null)
				{
					float num2 = Vector3.Distance(vineLaunchPoint2.transform.position, origin);
					Debug.LogWarning("Detected broken distance between " + vineLaunchPoint2.transform.position.ToString() + " and origin " + origin);
					Debug.LogWarning("home " + base.transform.position);
					Debug.LogWarning("dist is  " + num2);
				}
			}
			return 5f;
		}
		return num;
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		VineLaunchPoint vineLaunchPoint = origin.Get(base.isServer);
		if (vineLaunchPoint != null)
		{
			base.transform.position = vineLaunchPoint.transform.position;
			using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.Reserved1, b: false);
		}
	}

	private void Descend(BasePlayer forPlayer)
	{
		if (forPlayer == null || forPlayer.isMounted)
		{
			return;
		}
		StartReplicatingPos();
		isDescending = true;
		activeOriginPoint = origin.Get(isServer: true);
		base.transform.forward = forPlayer.eyes.BodyForward().WithY(0f);
		currentTime = 0f;
		MountPlayer(forPlayer);
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.On, b: true);
		flagsUpdateScope.Set(Flags.Reserved2, b: true);
	}

	private void Swing(BasePlayer forPlayer, bool shouldMount, VineLaunchPoint overridePoint = null)
	{
		Vector3 forward = ((forPlayer != null) ? forPlayer.eyes.BodyForward() : Vector3.forward);
		Vector3 playerPos = ((forPlayer != null) ? forPlayer.transform.position : base.transform.position);
		VineLaunchPoint vineLaunchPoint = null;
		vineLaunchPoint = (HasFlag(Flags.Reserved1) ? origin.Get(base.isServer) : ((!(overridePoint != null)) ? GetTargetDestination(playerPos, forward, out var _) : overridePoint));
		if (vineLaunchPoint == null)
		{
			Debug.Log("Could not find valid vine launch destination, should not happen");
			return;
		}
		Vector3 normalized = (vineLaunchPoint.transform.position - base.transform.position).normalized;
		base.transform.rotation = Quaternion.LookRotation(normalized, Vector3.up);
		activeOriginPoint = currentLocation.Get(base.isServer);
		activeDestinationPoint = vineLaunchPoint;
		if (forPlayer != null)
		{
			lastPosition = forPlayer.transform.position;
			if (shouldMount)
			{
				MountPlayer(forPlayer);
			}
		}
		lastValidLocation = base.transform.position;
		lastValidLocationTime = 0f;
		currentTime = 0f;
		currentLocation.Set(vineLaunchPoint);
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved1, !HasFlag(Flags.Reserved1));
			flagsUpdateScope.Set(Flags.On, b: true);
			flagsUpdateScope.Set(Flags.Reserved2, b: false);
		}
		StartReplicatingPos();
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void SV_Swing(RPCMessage msg)
	{
		if (!IsMounted() && ConVar.Server.allowVineSwinging)
		{
			BasePlayer player = msg.player;
			bool flag = msg.read.Bool();
			if (!flag || !(player != null) || !player.isMounted)
			{
				Swing(player, flag);
			}
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void SV_Descend(RPCMessage msg)
	{
		if (!IsMounted() && ConVar.Server.allowVineSwinging)
		{
			BasePlayer player = msg.player;
			Descend(player);
		}
	}

	private void OnArrived(VineLaunchPoint point)
	{
		base.transform.forward = -base.transform.forward;
		base.transform.localEulerAngles = base.transform.localEulerAngles.WithX(0f);
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.On, b: false);
		}
		if (point != null && point.FindVacantArrivalPoint(this, out var worldPos))
		{
			base.transform.position = worldPos;
		}
		DeferredStopReplicatingPos();
		BasePlayer mounted = GetMounted();
		DismountAllPlayers();
		if (chainTarget != null)
		{
			chainTarget.Swing(mounted, shouldMount: true);
		}
	}

	private void StartReplicatingPos()
	{
		wantsToSyncPos = true;
		if (stopReplicatingPosCallback == null)
		{
			stopReplicatingPosCallback = StopReplcatingPos;
			ToggleNetworkPositionTick(isEnabled: true);
		}
		else if (!IsInvoking(stopReplicatingPosCallback))
		{
			ToggleNetworkPositionTick(isEnabled: true);
		}
	}

	private void StopReplcatingPos()
	{
		if (!wantsToSyncPos)
		{
			ToggleNetworkPositionTick(isEnabled: false);
		}
	}

	private void DeferredStopReplicatingPos()
	{
		wantsToSyncPos = false;
		Invoke(stopReplicatingPosCallback, 0.5f);
	}

	public void Highlight(BasePlayer forPlayer)
	{
		Vector3 position = origin.Get(base.isServer).transform.position;
		foreach (VinePoint destination in destinations)
		{
			Vector3 position2 = destination.Get(base.isServer).transform.position;
			forPlayer.SendConsoleCommand("ddraw.arrow", "60", Color.red, position, position2, 25, 0, 0);
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.vineMountable == null)
		{
			return;
		}
		WorldSpaceAnchorPoint = info.msg.vineMountable.anchorPoint;
		origin.Load(info.msg.vineMountable.originPoint);
		currentLocation.Load(info.msg.vineMountable.currentLocation);
		destinations.Clear();
		foreach (VineDestination destination in info.msg.vineMountable.destinations)
		{
			VinePoint item = default(VinePoint);
			item.Load(destination);
			destinations.Add(item);
		}
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (!base.isServer)
		{
			return;
		}
		if (processMovementAction == null)
		{
			processMovementAction = ProcessMovement;
		}
		bool flag = IsOn();
		bool flag2 = IsInvoking(processMovementAction);
		if (flag != flag2)
		{
			if (flag)
			{
				InvokeRepeating(processMovementAction, 0f, 0f);
			}
			else
			{
				CancelInvoke(processMovementAction);
			}
		}
	}

	private VineLaunchPoint GetTargetDestination(Vector3 playerPos, Vector3 forward, out float foundAngle)
	{
		float num = float.MaxValue;
		VineLaunchPoint result = null;
		forward.y = 0f;
		foreach (VinePoint destination in destinations)
		{
			VineLaunchPoint vineLaunchPoint = destination.Get(base.isServer);
			if (vineLaunchPoint != null)
			{
				float num2 = Vector3.Angle(forward, (vineLaunchPoint.transform.position - playerPos).normalized.WithY(0f));
				if (num2 < num)
				{
					result = vineLaunchPoint;
					num = num2;
				}
			}
		}
		foundAngle = num;
		return result;
	}
}
