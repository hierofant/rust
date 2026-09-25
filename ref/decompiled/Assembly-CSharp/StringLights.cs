#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class StringLights : IOEntity
{
	public struct PointEntry
	{
		public Vector3 point;

		public Vector3 normal;

		public float slack;
	}

	[Serializable]
	public struct BulbSettings
	{
		public GameObjectRef BulbPrefab;

		public float Weight;
	}

	private const int PLACEMENT_LAYER = 1218510849;

	private const float MIN_SLACK = 0f;

	private const float MAX_SLACK = 2f;

	private BasePlayer usingPlayer;

	[SerializeField]
	private float lengthPerAmount = 0.5f;

	[SerializeField]
	private float maxPlaceDistance = 5f;

	[SerializeField]
	private ItemDefinition itemToConsume;

	[SerializeField]
	[Header("Line Generation Settings")]
	protected BulbSettings[] bulbSettings;

	[SerializeField]
	private GameObjectRef pointLightPrefab;

	[SerializeField]
	private Transform lightsParent;

	[SerializeField]
	private Transform wireOrigin;

	[SerializeField]
	protected float bulbSpacing = 0.25f;

	[SerializeField]
	protected float wireThickness = 0.02f;

	[SerializeField]
	protected float maxDeviation = 0.25f;

	[SerializeField]
	protected float deviationFactor = 1f;

	[SerializeField]
	protected bool bulbFaceNormal;

	[SerializeField]
	[Space]
	protected LineRenderer lineRenderer;

	[SerializeField]
	protected RendererLOD rendererLod;

	protected readonly List<PointEntry> points = new List<PointEntry>();

	protected readonly List<StringLightsBulb> bulbs = new List<StringLightsBulb>();

	protected readonly List<Light> pointLights = new List<Light>();

	private readonly Dictionary<int, GameObject> prefabLookup = new Dictionary<int, GameObject>();

	public bool useBatching;

	protected List<StringLightsBulb> lastBatchedMeshes = new List<StringLightsBulb>();

	private int lengthUsed;

	private const Flags Flag_Used = Flags.Reserved5;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("StringLights.OnRpcMessage"))
		{
			if (rpc == 4045900594u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SERVER_AddPoint");
				}
				using (TimeWarning.New("SERVER_AddPoint"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(4045900594u, "SERVER_AddPoint", this, player, 3uL))
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
							SERVER_AddPoint(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SERVER_AddPoint");
					}
				}
				return true;
			}
			if (rpc == 3733663691u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SERVER_RemovePoint");
				}
				using (TimeWarning.New("SERVER_RemovePoint"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3733663691u, "SERVER_RemovePoint", this, player, 3uL))
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
							SERVER_RemovePoint(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in SERVER_RemovePoint");
					}
				}
				return true;
			}
			if (rpc == 2400039444u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SERVER_StartDeploying");
				}
				using (TimeWarning.New("SERVER_StartDeploying"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2400039444u, "SERVER_StartDeploying", this, player, 3f))
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
							RPCMessage msg4 = rPCMessage;
							SERVER_StartDeploying(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in SERVER_StartDeploying");
					}
				}
				return true;
			}
			if (rpc == 2702400742u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SERVER_StopDeploying");
				}
				using (TimeWarning.New("SERVER_StopDeploying"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg5 = rPCMessage;
							SERVER_StopDeploying(msg5);
						}
					}
					catch (Exception exception4)
					{
						Debug.LogException(exception4);
						player.Kick("RPC Error in SERVER_StopDeploying");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	protected Item GetOwnerItem()
	{
		BasePlayer basePlayer = null;
		if (base.isServer)
		{
			basePlayer = usingPlayer;
		}
		if (basePlayer == null || basePlayer.inventory == null)
		{
			return null;
		}
		return basePlayer.inventory.FindItemByItemID(itemToConsume.itemid);
	}

	public override Item GetItem()
	{
		return GetOwnerItem();
	}

	private bool CheckValidPlacement(Vector3 position, float radius, int layerMask)
	{
		bool result = true;
		List<BaseEntity> obj = Facepunch.Pool.Get<List<BaseEntity>>();
		Vis.Entities(position, radius, obj, layerMask);
		foreach (BaseEntity item in obj)
		{
			if (item is AnimatedBuildingBlock)
			{
				result = false;
				break;
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		return result;
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		if (base.isServer)
		{
			lengthUsed = 1;
			PlayerStartsDeploying(deployedBy);
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void SERVER_StartDeploying(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!IsUsed() && player.CanBuild())
		{
			PlayerStartsDeploying(player);
		}
	}

	[RPC_Server]
	public void SERVER_StopDeploying(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(usingPlayer != player) && player.CanBuild())
		{
			PlayerStopsDeploying(player);
		}
	}

	public void PlayerStartsDeploying(BasePlayer player)
	{
		if (!IsUsed() && !(player == null))
		{
			usingPlayer = player;
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved5, b: true);
			}
			if (IsInvoking(ServerWireDeployingTick))
			{
				CancelInvoke(ServerWireDeployingTick);
			}
			InvokeRepeating(ServerWireDeployingTick, 0f, 0f);
			ClientRPC(RpcTarget.Player("CLIENT_StartDeploying", player));
		}
	}

	public void PlayerStopsDeploying(BasePlayer player)
	{
		usingPlayer = null;
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved5, b: false);
		}
		CancelInvoke(ServerWireDeployingTick);
		ClientRPC(RpcTarget.Player("CLIENT_StopDeploying", player));
	}

	public void ServerWireDeployingTick()
	{
		if (!usingPlayer.IsValid() || !usingPlayer.IsConnected)
		{
			PlayerStopsDeploying(usingPlayer);
		}
	}

	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server]
	public void SERVER_AddPoint(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (player != usingPlayer)
		{
			return;
		}
		Vector3 vector = msg.read.Vector3();
		Vector3 vector2 = msg.read.Vector3();
		float num = msg.read.Float();
		if (vector.IsNaNOrInfinity() || vector2.IsNaNOrInfinity() || num.IsNaNOrInfinity())
		{
			return;
		}
		num = Mathf.Clamp(num, 0f, 2f);
		Item item = GetItem();
		if (item != null && item.amount >= 1 && CanPlayerUse(player) && !(Vector3.Distance(vector, player.eyes.position) > maxPlaceDistance) && CheckValidPlacement(vector, 0.1f, 1218510849) && Interface.CallHook("OnPoweredLightsPointAdd", this, player, vector, vector2) == null)
		{
			int num2 = 1;
			float num3 = 0f;
			Vector3 vector3 = ((points.Count > 0) ? base.transform.TransformPoint(points[points.Count - 1].point) : wireOrigin.position);
			num3 = Vector3.Distance(vector, vector3);
			num3 = Mathf.Max(num3, lengthPerAmount);
			float num4 = (float)item.amount * lengthPerAmount;
			if (player.IsInCreativeMode && Creative.unlimitedIo)
			{
				num4 = 200f;
			}
			if (num3 > num4)
			{
				num3 = num4;
				vector = vector3 + Vector3Ex.Direction(vector, vector3) * num3;
			}
			num3 = Mathf.Min(num4, num3);
			num2 = Mathf.CeilToInt(num3 / lengthPerAmount);
			if (player.IsInCreativeMode && Creative.unlimitedIo)
			{
				num2 = 0;
			}
			AddPoint(base.transform.InverseTransformPoint(vector), base.transform.InverseTransformDirection(vector2), num);
			UseItemAmount(num2);
			AddLengthUsed(num2);
			SendNetworkUpdate();
		}
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(3uL)]
	public void SERVER_RemovePoint(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player != usingPlayer) && CanPlayerUse(player) && points.Count != 0)
		{
			Vector3 a = base.transform.TransformPoint(points[points.Count - 1].point);
			Vector3 b = base.transform.position;
			if (points.Count > 1)
			{
				b = base.transform.TransformPoint(points[points.Count - 2].point);
			}
			int num = Mathf.CeilToInt(Vector3.Distance(a, b) / lengthPerAmount);
			RemoveLastPoint();
			if (!player.IsInCreativeMode || !Creative.unlimitedIo)
			{
				GiveItemAmount(player, num);
				AddLengthUsed(-num);
			}
			SendNetworkUpdate();
		}
	}

	private void GiveItemAmount(BasePlayer player, int amount)
	{
		if (amount > 0)
		{
			Item ownerItem = GetOwnerItem();
			if (ownerItem == null)
			{
				ownerItem = ItemManager.Create(itemToConsume, amount, 0uL, isServerSide: true, 0uL);
				player.GiveItem(ownerItem, GiveItemReason.PickedUp);
			}
			else
			{
				ownerItem.amount += amount;
				ownerItem.MarkDirty();
				ownerItem.ReduceItemOwnership(amount);
			}
		}
	}

	protected void UseItemAmount(int amount)
	{
		if (amount <= 0)
		{
			return;
		}
		Item ownerItem = GetOwnerItem();
		if (ownerItem != null)
		{
			ownerItem.amount -= amount;
			ownerItem.MarkDirty();
			ownerItem.ReduceItemOwnership(amount);
			if (ownerItem.amount <= 0)
			{
				ownerItem.Remove();
			}
		}
	}

	public bool IsUsed()
	{
		return HasFlag(Flags.Reserved5);
	}

	public void ClearPoints()
	{
		points.Clear();
	}

	public void AddPoint(Vector3 newPoint, Vector3 newNormal, float slackLevel, bool addFirstPoint = true)
	{
		if (addFirstPoint && base.isServer && points.Count == 0)
		{
			PointEntry item = default(PointEntry);
			item.point = base.transform.InverseTransformPoint(wireOrigin.position);
			item.normal = newNormal;
			item.slack = slackLevel;
			points.Add(item);
		}
		slackLevel = Mathf.Clamp(slackLevel, 0f, 2f);
		PointEntry item2 = default(PointEntry);
		item2.point = newPoint;
		item2.normal = newNormal;
		item2.slack = slackLevel;
		points.Add(item2);
	}

	public void RemoveLastPoint()
	{
		points.RemoveAt(points.Count - 1);
		if (points.Count == 1)
		{
			points.Clear();
		}
	}

	public override int ConsumptionAmount()
	{
		return 5;
	}

	protected override int GetPickupCount()
	{
		return Mathf.Max(lengthUsed, 1);
	}

	public void AddLengthUsed(int addLength)
	{
		lengthUsed += addLength;
	}

	protected bool CanPlayerUse(BasePlayer player)
	{
		if (player.CanBuild())
		{
			return !GamePhysics.CheckSphere(player.eyes.position, 0.1f, 536870912, QueryTriggerInteraction.Collide);
		}
		return false;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.lightString = Facepunch.Pool.Get<LightString>();
		info.msg.lightString.points = Facepunch.Pool.Get<List<LightString.StringPoint>>();
		info.msg.lightString.lengthUsed = lengthUsed;
		foreach (PointEntry point in points)
		{
			LightString.StringPoint stringPoint = Facepunch.Pool.Get<LightString.StringPoint>();
			stringPoint.point = point.point;
			stringPoint.normal = point.normal;
			stringPoint.slack = point.slack;
			info.msg.lightString.points.Add(stringPoint);
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.lightString == null)
		{
			return;
		}
		ClearPoints();
		foreach (LightString.StringPoint point in info.msg.lightString.points)
		{
			AddPoint(point.point, point.normal, point.slack, addFirstPoint: false);
		}
		lengthUsed = info.msg.lightString.lengthUsed;
	}
}
