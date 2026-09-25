#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class WireTool : HeldEntity
{
	public enum WireColour
	{
		Gray,
		Red,
		Green,
		Blue,
		Yellow,
		Pink,
		Purple,
		Orange,
		White,
		LightBlue,
		Invisible,
		Count
	}

	public struct PendingPlug
	{
		public IOEntity ent;

		public bool isInput;

		public int index;
	}

	private const int maxLineNodes = 16;

	private const float industrialWallOffset = 0.04f;

	public IOEntity.IOType wireType;

	public WireColour DefaultColor;

	public float radialMenuHoldTime = 0.25f;

	public float disconnectDelay = 0.15f;

	public float clearDelay = 0.65f;

	private bool justCleared;

	public GameObjectRef plugEffect;

	public SoundDefinition clearStartSoundDef;

	public SoundDefinition clearSoundDef;

	public PendingPlug pendingPlug;

	public const float MIN_SLACK = 0f;

	public const float MAX_SLACK = 2f;

	private const float wireValidationDist = 5f;

	private const float wireValidationDistSqr = 25f;

	private const float IndustrialThickness = 0.01f;

	private bool CanChangeColours
	{
		get
		{
			IOEntity.IOType iOType = wireType;
			return iOType == IOEntity.IOType.Electric || iOType == IOEntity.IOType.Fluidic || iOType == IOEntity.IOType.Industrial;
		}
	}

	public NetworkableId validatedWireEntity { get; private set; }

	public int validatedWireSlot { get; private set; } = -1;


	public bool validatedWireIsInput { get; private set; }

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("WireTool.OnRpcMessage"))
		{
			if (rpc == 2640128661u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_CancelPendingWire");
				}
				using (TimeWarning.New("RPC_CancelPendingWire"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2640128661u, "RPC_CancelPendingWire", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(2640128661u, "RPC_CancelPendingWire", this, player))
						{
							return true;
						}
						if (!RPC_Server.IsActiveItem.Test(2640128661u, "RPC_CancelPendingWire", this, player))
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
							RPC_CancelPendingWire(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_CancelPendingWire");
					}
				}
				return true;
			}
			if (rpc == 2571821359u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_MakeConnection");
				}
				using (TimeWarning.New("RPC_MakeConnection"))
				{
					bool flag = HasUnlimitedIo(player);
					using (msg.read.UseRepeatedElementLimit(flag ? (-1) : 54))
					{
						using (msg.read.SuspendProtoFieldOperationLimit(flag))
						{
							using (TimeWarning.New("Conditions"))
							{
								if (!RPC_Server.CallsPerSecond.Test(2571821359u, "RPC_MakeConnection", this, player, 5uL))
								{
									return true;
								}
								if (!RPC_Server.FromOwner.Test(2571821359u, "RPC_MakeConnection", this, player))
								{
									return true;
								}
								if (!RPC_Server.IsActiveItem.Test(2571821359u, "RPC_MakeConnection", this, player))
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
									RPCMessage rpc2 = rPCMessage;
									RPC_MakeConnection(rpc2);
								}
							}
							catch (Exception exception2)
							{
								Debug.LogException(exception2);
								player.Kick("RPC Error in RPC_MakeConnection");
							}
						}
					}
				}
				return true;
			}
			if (rpc == 986119119 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_RequestChangeColor");
				}
				using (TimeWarning.New("RPC_RequestChangeColor"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(986119119u, "RPC_RequestChangeColor", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(986119119u, "RPC_RequestChangeColor", this, player))
						{
							return true;
						}
						if (!RPC_Server.IsActiveItem.Test(986119119u, "RPC_RequestChangeColor", this, player))
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
							RPC_RequestChangeColor(msg3);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in RPC_RequestChangeColor");
					}
				}
				return true;
			}
			if (rpc == 1514179840 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_RequestClear");
				}
				using (TimeWarning.New("RPC_RequestClear"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1514179840u, "RPC_RequestClear", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(1514179840u, "RPC_RequestClear", this, player))
						{
							return true;
						}
						if (!RPC_Server.IsActiveItem.Test(1514179840u, "RPC_RequestClear", this, player))
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
							RPC_RequestClear(msg4);
						}
					}
					catch (Exception exception4)
					{
						Debug.LogException(exception4);
						player.Kick("RPC Error in RPC_RequestClear");
					}
				}
				return true;
			}
			if (rpc == 4283846014u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_WireStarted");
				}
				using (TimeWarning.New("RPC_WireStarted"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(4283846014u, "RPC_WireStarted", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(4283846014u, "RPC_WireStarted", this, player))
						{
							return true;
						}
						if (!RPC_Server.IsActiveItem.Test(4283846014u, "RPC_WireStarted", this, player))
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
							RPCMessage msg5 = rPCMessage;
							RPC_WireStarted(msg5);
						}
					}
					catch (Exception exception5)
					{
						Debug.LogException(exception5);
						player.Kick("RPC Error in RPC_WireStarted");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public float GetMaxWireLength(BasePlayer forPlayer)
	{
		if (forPlayer == null || !forPlayer.IsInCreativeMode || !Creative.unlimitedIo)
		{
			return 30f;
		}
		return 200f;
	}

	private static float CombineMinSlack(float a, float b)
	{
		float value = ((!(a > 0f) || !(b > 0f)) ? Mathf.Max(a, b) : Mathf.Min(a, b));
		return Mathf.Clamp(value, 0f, 2f);
	}

	private static float CombineMaxSlack(float a, float b)
	{
		float value = ((!(a < 2f) || !(b < 2f)) ? Mathf.Min(a, b) : Mathf.Max(a, b));
		return Mathf.Clamp(value, 0f, 2f);
	}

	private bool HasUnlimitedIo(BasePlayer player)
	{
		if (player.IsInCreativeMode)
		{
			return Creative.unlimitedIo;
		}
		return false;
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(5uL)]
	public void RPC_WireStarted(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		NetworkableId uid = msg.read.EntityID();
		int num = msg.read.Int32();
		bool flag = msg.read.Bit();
		IOEntity iOEntity = BaseNetworkable.serverEntities.Find(uid) as IOEntity;
		if (iOEntity == null || !CanPlayerUseWires(player, cached: false, 1f, iOEntity))
		{
			return;
		}
		IOEntity.IOSlot[] array = (flag ? iOEntity.inputs : iOEntity.outputs);
		if (num >= 0 && num < array.Length)
		{
			Vector3 vector = iOEntity.transform.TransformPoint(array[num].handlePosition);
			if (!(Vector3.SqrMagnitude(player.transform.position - vector) > 25f) && array[num].type == wireType && CanModifyEntity(player, iOEntity))
			{
				validatedWireEntity = uid;
				validatedWireSlot = num;
				validatedWireIsInput = flag;
			}
		}
	}

	[RPC_Server.IgnoreConditional("HasUnlimitedIo", new Type[]
	{
		typeof(RPC_Server.MaxRepeatedElements),
		typeof(RPC_Server.IgnoreProtoFieldOperationLimit)
	})]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.MaxRepeatedElements(54)]
	[RPC_Server.IsActiveItem]
	[RPC_Server]
	[RPC_Server.FromOwner]
	public void RPC_MakeConnection(RPCMessage rpc)
	{
		BasePlayer player = rpc.player;
		using WireConnectionMessage wireConnectionMessage = rpc.read.Proto<WireConnectionMessage>();
		List<Vector3> linePoints = wireConnectionMessage.linePoints;
		int inputIndex = wireConnectionMessage.inputIndex;
		int outputIndex = wireConnectionMessage.outputIndex;
		IOEntity iOEntity = new EntityRef<IOEntity>(wireConnectionMessage.inputID).Get(serverside: true);
		IOEntity iOEntity2 = new EntityRef<IOEntity>(wireConnectionMessage.outputID).Get(serverside: true);
		if (iOEntity == null || iOEntity2 == null || !CanPlayerUseWires(player, cached: false, 1f, iOEntity.CanSkipWireToolBuildAuthorisation() ? iOEntity : iOEntity2) || !SharesRootParent(iOEntity, iOEntity2) || !ValidateLine(linePoints, iOEntity, iOEntity2, player, outputIndex) || inputIndex >= iOEntity.inputs.Length || outputIndex >= iOEntity2.outputs.Length || iOEntity.inputs[inputIndex].connectedTo.Get() != null || iOEntity2.outputs[outputIndex].connectedTo.Get() != null || (iOEntity.inputs[inputIndex].rootConnectionsOnly && !iOEntity2.IsRootEntity()) || !CanModifyEntity(player, iOEntity) || !CanModifyEntity(player, iOEntity2))
		{
			return;
		}
		NetworkableId obj = (validatedWireIsInput ? wireConnectionMessage.inputID : wireConnectionMessage.outputID);
		int num = (validatedWireIsInput ? inputIndex : outputIndex);
		if (obj != validatedWireEntity || num != validatedWireSlot)
		{
			return;
		}
		validatedWireEntity = default(NetworkableId);
		validatedWireSlot = -1;
		List<float> slackLevels = wireConnectionMessage.slackLevels;
		if (slackLevels.Count != linePoints.Count)
		{
			return;
		}
		for (int i = 0; i < slackLevels.Count; i++)
		{
			if (slackLevels[i] < 0f || slackLevels[i] > 2f)
			{
				return;
			}
		}
		float num2 = CombineMinSlack(Mathf.Clamp(iOEntity.GetMinWireSlack(isInput: true, inputIndex), 0f, 2f), Mathf.Clamp(iOEntity2.GetMinWireSlack(isInput: false, outputIndex), 0f, 2f));
		float a = CombineMaxSlack(Mathf.Clamp(iOEntity.GetMaxWireSlack(isInput: true, inputIndex), 0f, 2f), Mathf.Clamp(iOEntity2.GetMaxWireSlack(isInput: false, outputIndex), 0f, 2f));
		a = Mathf.Max(a, num2);
		if (num2 > 0f || a < 2f)
		{
			if (slackLevels.Count < 2)
			{
				return;
			}
			int index = (validatedWireIsInput ? (slackLevels.Count - 2) : 0);
			float num3 = slackLevels[index];
			if (num3 < num2 - 0.001f || num3 > a + 0.001f)
			{
				return;
			}
		}
		IOEntity.LineAnchor[] array = new IOEntity.LineAnchor[wireConnectionMessage.lineAnchors.Count];
		if (!ValidateLineAnchors(iOEntity, wireConnectionMessage.lineAnchors, array, linePoints, player))
		{
			return;
		}
		WireColour wireColour = IntToColour(wireConnectionMessage.wireColor);
		if (Interface.CallHook("OnWireConnect", player, iOEntity, inputIndex, iOEntity2, outputIndex, wireConnectionMessage.linePoints, slackLevels) == null)
		{
			if (wireColour == WireColour.Invisible && !player.IsInCreativeMode)
			{
				wireColour = DefaultColor;
			}
			iOEntity2.ConnectTo(iOEntity, outputIndex, inputIndex, linePoints, slackLevels, array, wireColour);
			if (wireType == IOEntity.IOType.Industrial)
			{
				iOEntity.NotifyIndustrialNetworkChanged();
				iOEntity2.NotifyIndustrialNetworkChanged();
			}
			Facepunch.Rust.Analytics.Azure.OnIOEntityConnected(player, iOEntity, iOEntity2, wireColour);
		}
	}

	private bool ValidateLineAnchors(IOEntity ioEnt, List<WireLineAnchorInfo> lineAnchors, IOEntity.LineAnchor[] receivedAnchors, List<Vector3> linePoints, BasePlayer ply)
	{
		for (int i = 0; i < lineAnchors.Count; i++)
		{
			WireLineAnchorInfo wireLineAnchorInfo = lineAnchors[i];
			if (wireLineAnchorInfo.index < 0 || wireLineAnchorInfo.index >= linePoints.Count)
			{
				return false;
			}
			EntityRef<Door> entityRef = new EntityRef<Door>(wireLineAnchorInfo.parentID);
			Door door = entityRef.Get(serverside: true);
			if (door == null || door.model == null)
			{
				return false;
			}
			float num = 35f;
			if (Vector3.Distance(door.transform.position, ioEnt.transform.position) > num)
			{
				return false;
			}
			if (string.IsNullOrEmpty(wireLineAnchorInfo.boneName) || !door.model.HasBone(wireLineAnchorInfo.boneName, out var _))
			{
				return false;
			}
			Door door2 = door.LookupPrefab<Door>();
			if (door2 == null || door2.model == null || !door2.model.HasBone(wireLineAnchorInfo.boneName, out var bone2))
			{
				return false;
			}
			Vector3 point = (door2.transform.worldToLocalMatrix * bone2.localToWorldMatrix).MultiplyPoint3x4(wireLineAnchorInfo.position);
			Bounds bounds = door.bounds;
			bounds.Expand(0.25f);
			if (!bounds.Contains(point))
			{
				return false;
			}
			receivedAnchors[i].entityRef = entityRef;
			receivedAnchors[i].boneName = wireLineAnchorInfo.boneName;
			receivedAnchors[i].index = (int)wireLineAnchorInfo.index;
			receivedAnchors[i].position = wireLineAnchorInfo.position;
		}
		return true;
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(5uL)]
	public void RPC_RequestClear(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		NetworkableId uid = msg.read.EntityID();
		int num = msg.read.Int32();
		bool flag = msg.read.Bit();
		bool flag2 = msg.read.Bit();
		bool flag3 = msg.read.Bit();
		IOEntity iOEntity = BaseNetworkable.serverEntities.Find(uid) as IOEntity;
		if (iOEntity == null || !CanPlayerUseWires(player, cached: false, 1f, iOEntity))
		{
			return;
		}
		IOEntity.IOSlot[] array = (flag ? iOEntity.inputs : iOEntity.outputs);
		if (num >= 0 && num < array.Length)
		{
			Vector3 vector = iOEntity.transform.TransformPoint(array[num].handlePosition);
			if (Vector3.SqrMagnitude(player.transform.position - vector) > 25f)
			{
				return;
			}
		}
		else if (Vector3.SqrMagnitude(player.transform.position - iOEntity.transform.position) > 25f)
		{
			return;
		}
		if (!GamePhysics.LineOfSight(player.eyes.center, player.eyes.position, 1218519041) || (!iOEntity.IsVisible(player.eyes.HeadRay(), 1218519041, 5f) && !iOEntity.IsVisible(player.eyes.position, 5f)))
		{
			return;
		}
		WireReconnectMessage wireReconnectMessage = Facepunch.Pool.Get<WireReconnectMessage>();
		if (flag2)
		{
			if (num < 0 || num >= array.Length)
			{
				wireReconnectMessage.Dispose();
				return;
			}
			IOEntity.IOSlot iOSlot = array[num];
			IOEntity iOEntity2 = iOSlot.connectedTo.Get();
			if (iOEntity2 == null)
			{
				wireReconnectMessage.Dispose();
				return;
			}
			IOEntity.IOSlot iOSlot2 = (flag ? iOEntity2.outputs : iOEntity2.inputs)[iOSlot.connectedToSlot];
			wireReconnectMessage.isInput = !flag;
			wireReconnectMessage.slotIndex = iOSlot.connectedToSlot;
			wireReconnectMessage.otherEntityId = iOEntity2.net.ID;
			wireReconnectMessage.wireColor = (int)iOSlot.wireColour;
			wireReconnectMessage.linePoints = Facepunch.Pool.Get<List<Vector3>>();
			wireReconnectMessage.slackLevels = Facepunch.Pool.Get<List<float>>();
			wireReconnectMessage.lineAnchors = Facepunch.Pool.Get<List<WireLineAnchorInfo>>();
			wireReconnectMessage.clearedEntityId = iOEntity.net.ID;
			IOEntity iOEntity3 = iOEntity;
			Vector3[] array2 = iOSlot.linePoints;
			if (array2 == null || array2.Length == 0)
			{
				iOEntity3 = iOEntity2;
				array2 = iOSlot2.linePoints;
			}
			if (array2 == null)
			{
				array2 = Array.Empty<Vector3>();
			}
			bool flag4 = iOEntity3 != iOEntity;
			if (iOEntity == iOEntity3 && flag)
			{
				flag4 = true;
			}
			wireReconnectMessage.linePoints.AddRange(array2);
			float[] slackLevels = iOSlot.slackLevels;
			if (slackLevels == null || slackLevels.Length == 0)
			{
				slackLevels = iOSlot2.slackLevels;
			}
			float[] array3 = slackLevels;
			foreach (float item in array3)
			{
				wireReconnectMessage.slackLevels.Add(item);
			}
			IOEntity.LineAnchor[] lineAnchors = iOSlot.lineAnchors;
			if (lineAnchors == null || lineAnchors.Length == 0)
			{
				lineAnchors = iOSlot2.lineAnchors;
			}
			if (lineAnchors != null)
			{
				IOEntity.LineAnchor[] array4 = lineAnchors;
				for (int i = 0; i < array4.Length; i++)
				{
					IOEntity.LineAnchor lineAnchor = array4[i];
					EntityRef<Door> entityRef = lineAnchor.entityRef;
					if (entityRef.Get(serverside: true).IsValid())
					{
						wireReconnectMessage.lineAnchors.Add(lineAnchor.ToInfo());
					}
				}
			}
			wireReconnectMessage.slackLevels.RemoveAt(wireReconnectMessage.slackLevels.Count - 1);
			if (flag4)
			{
				wireReconnectMessage.linePoints.Reverse();
				wireReconnectMessage.slackLevels.Reverse();
				int num2 = wireReconnectMessage.linePoints.Count - 1;
				foreach (WireLineAnchorInfo lineAnchor2 in wireReconnectMessage.lineAnchors)
				{
					lineAnchor2.index = num2 - lineAnchor2.index;
				}
			}
			if (wireReconnectMessage.lineAnchors.Count >= 0)
			{
				List<WireLineAnchorInfo> obj = Facepunch.Pool.Get<List<WireLineAnchorInfo>>();
				foreach (WireLineAnchorInfo lineAnchor3 in wireReconnectMessage.lineAnchors)
				{
					if (lineAnchor3.index == 0L || lineAnchor3.index == wireReconnectMessage.linePoints.Count - 1)
					{
						obj.Add(lineAnchor3);
					}
				}
				foreach (WireLineAnchorInfo item2 in obj)
				{
					wireReconnectMessage.lineAnchors.Remove(item2);
				}
				Facepunch.Pool.Free(ref obj, freeElements: false);
			}
			if (wireReconnectMessage.linePoints.Count >= 0)
			{
				wireReconnectMessage.linePoints.RemoveAt(0);
				wireReconnectMessage.linePoints.RemoveAt(wireReconnectMessage.linePoints.Count - 1);
			}
			if (wireReconnectMessage.slackLevels.Count >= 0)
			{
				wireReconnectMessage.slackLevels.RemoveAt(wireReconnectMessage.slackLevels.Count - 1);
			}
		}
		if (AttemptClearSlot(iOEntity, player, num, flag))
		{
			if (flag2)
			{
				if (validatedWireEntity == default(NetworkableId))
				{
					validatedWireEntity = wireReconnectMessage.otherEntityId;
					validatedWireSlot = wireReconnectMessage.slotIndex;
					validatedWireIsInput = wireReconnectMessage.isInput;
				}
				ClientRPC(RpcTarget.Player("RPC_OnWireDisconnected", player), wireReconnectMessage);
			}
			else if (!flag3)
			{
				validatedWireEntity = default(NetworkableId);
				validatedWireSlot = -1;
			}
		}
		wireReconnectMessage.Dispose();
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	[RPC_Server.IsActiveItem]
	[RPC_Server.FromOwner]
	public void RPC_CancelPendingWire(RPCMessage msg)
	{
		validatedWireEntity = default(NetworkableId);
		validatedWireSlot = -1;
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(5uL)]
	public void RPC_RequestChangeColor(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		NetworkableId uid = msg.read.EntityID();
		IOEntity iOEntity = BaseNetworkable.serverEntities.Find(uid) as IOEntity;
		if (iOEntity == null || Vector3.SqrMagnitude(player.transform.position - iOEntity.transform.position) > 25f || !CanModifyEntity(player, iOEntity))
		{
			return;
		}
		int index = msg.read.Int32();
		bool flag = msg.read.Bit();
		WireColour wireColour = IntToColour(msg.read.Int32());
		if (wireColour == WireColour.Invisible && !player.IsInCreativeMode)
		{
			return;
		}
		IOEntity.IOSlot iOSlot = (flag ? iOEntity.inputs.ElementAtOrDefault(index) : iOEntity.outputs.ElementAtOrDefault(index));
		if (iOSlot != null)
		{
			IOEntity iOEntity2 = iOSlot.connectedTo.Get();
			if (!(iOEntity2 == null))
			{
				IOEntity.IOSlot obj = (flag ? iOEntity2.outputs : iOEntity2.inputs)[iOSlot.connectedToSlot];
				iOSlot.wireColour = wireColour;
				iOEntity.SendNetworkUpdate();
				obj.wireColour = wireColour;
				iOEntity2.SendNetworkUpdate();
			}
		}
	}

	public static bool AttemptClearSlot(BaseNetworkable clearEnt, BasePlayer ply, int clearIndex, bool isInput)
	{
		IOEntity iOEntity = ((clearEnt != null) ? clearEnt.GetComponent<IOEntity>() : null);
		IOEntity iOEntity2 = (IOEntity)(object)(isInput ? iOEntity.inputs[clearIndex] : iOEntity.outputs[clearIndex]);
		if (((IOEntity.IOSlot)(object)iOEntity2).connectedTo.Get() == null)
		{
			return false;
		}
		iOEntity2 = ((IOEntity.IOSlot)(object)iOEntity2).connectedTo.Get();
		object obj = Interface.CallHook("OnWireClear", ply, iOEntity, clearIndex, iOEntity2, isInput);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (iOEntity == null)
		{
			return false;
		}
		if (ply != null && !CanClearEntity(ply, iOEntity, clearIndex, isInput))
		{
			return false;
		}
		return iOEntity.Disconnect(clearIndex, isInput);
	}

	public WireColour IntToColour(int i)
	{
		if (i < 0)
		{
			i = 0;
		}
		if (i > 11)
		{
			i = 10;
		}
		i %= 11;
		return (WireColour)i;
	}

	public bool ValidateLine(List<Vector3> lineList, IOEntity inputEntity, IOEntity outputEntity, BasePlayer byPlayer, int outputIndex)
	{
		if (byPlayer != null && byPlayer.IsInCreativeMode && Creative.unlimitedIo)
		{
			return true;
		}
		if (lineList.Count < 2 || lineList.Count > 18)
		{
			return false;
		}
		if (inputEntity == null || outputEntity == null)
		{
			return false;
		}
		Vector3 a = lineList[0];
		float num = 0f;
		int count = lineList.Count;
		float maxWireLength = GetMaxWireLength(byPlayer);
		for (int i = 1; i < count; i++)
		{
			Vector3 vector = lineList[i];
			num += Vector3.Distance(a, vector);
			if (num > maxWireLength)
			{
				return false;
			}
			a = vector;
		}
		Vector3 point = lineList[count - 1];
		Bounds bounds = outputEntity.bounds;
		bounds.Expand(0.5f);
		if (!bounds.Contains(point))
		{
			return false;
		}
		Vector3 position = outputEntity.transform.TransformPoint(lineList[0]);
		point = inputEntity.transform.InverseTransformPoint(position);
		Bounds bounds2 = inputEntity.bounds;
		bounds2.Expand(0.5f);
		if (!bounds2.Contains(point))
		{
			return false;
		}
		if (byPlayer == null)
		{
			return false;
		}
		Vector3 position2 = outputEntity.transform.TransformPoint(lineList[lineList.Count - 1]);
		if (byPlayer.Distance(position2) > 5f && byPlayer.Distance(position) > 5f)
		{
			return false;
		}
		if (outputIndex >= 0 && outputIndex < outputEntity.outputs.Length && outputEntity.outputs[outputIndex].type == IOEntity.IOType.Industrial && !VerifyLineOfSight(lineList, outputEntity.transform.localToWorldMatrix))
		{
			return false;
		}
		return true;
	}

	public bool VerifyLineOfSight(List<Vector3> positions, Matrix4x4 localToWorldSpace)
	{
		Vector3 worldSpaceA = localToWorldSpace.MultiplyPoint3x4(positions[0]);
		for (int i = 1; i < positions.Count; i++)
		{
			Vector3 vector = localToWorldSpace.MultiplyPoint3x4(positions[i]);
			if (!VerifyLineOfSight(worldSpaceA, vector))
			{
				return false;
			}
			worldSpaceA = vector;
		}
		return true;
	}

	public bool VerifyLineOfSight(Vector3 worldSpaceA, Vector3 worldSpaceB)
	{
		float maxDistance = Vector3.Distance(worldSpaceA, worldSpaceB);
		Vector3 normalized = (worldSpaceA - worldSpaceB).normalized;
		List<RaycastHit> obj = Facepunch.Pool.Get<List<RaycastHit>>();
		GamePhysics.TraceAll(new Ray(worldSpaceB, normalized), 0.01f, obj, maxDistance, 136380672);
		bool result = true;
		foreach (RaycastHit item in obj)
		{
			BaseEntity entity = RaycastHitEx.GetEntity(item);
			if (entity != null && RaycastHitEx.IsOnLayer(item, Rust.Layer.Deployed))
			{
				if (entity is VendingMachine)
				{
					result = false;
					break;
				}
			}
			else if (!(entity != null) || !(entity is Door))
			{
				result = false;
				break;
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		return result;
	}

	public bool HasPendingPlug()
	{
		if (pendingPlug.ent != null)
		{
			return pendingPlug.index != -1;
		}
		return false;
	}

	public bool PendingPlugIsInput()
	{
		if (pendingPlug.ent != null && pendingPlug.index != -1)
		{
			return pendingPlug.isInput;
		}
		return false;
	}

	public bool PendingPlugIsType(IOEntity.IOType type)
	{
		if (pendingPlug.ent == null || pendingPlug.index == -1)
		{
			return false;
		}
		IOEntity.IOSlot[] array = (pendingPlug.isInput ? pendingPlug.ent.inputs : pendingPlug.ent.outputs);
		if (pendingPlug.index < 0 || pendingPlug.index >= array.Length)
		{
			return false;
		}
		return array[pendingPlug.index].type == type;
	}

	public bool PendingPlugIsOutput()
	{
		if (pendingPlug.ent != null && pendingPlug.index != -1)
		{
			return !pendingPlug.isInput;
		}
		return false;
	}

	public bool PendingPlugIsRoot()
	{
		if (pendingPlug.ent != null)
		{
			return pendingPlug.ent.IsRootEntity();
		}
		return false;
	}

	private void ResetPendingPlug()
	{
		pendingPlug.ent = null;
		pendingPlug.index = -1;
	}

	public static bool CanPlayerUseWires(BasePlayer player, bool cached = false, float cacheDuration = 1f, IOEntity targetIoEnt = null)
	{
		object obj = Interface.CallHook("CanUseWires", player, cached, cacheDuration, targetIoEnt);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (player != null && player.IsInCreativeMode && Creative.unlimitedIo)
		{
			return true;
		}
		if (!player.TryGetHeldEntity(out WireTool heldEntity))
		{
			return false;
		}
		if (!heldEntity.CanBeUsedInWater() && player.IsSwimming())
		{
			return false;
		}
		if (!player.CanBuild(cached, cacheDuration) && (targetIoEnt == null || !targetIoEnt.CanSkipWireToolBuildAuthorisation()))
		{
			return false;
		}
		if (player.FindTrigger<TriggerMonumentIOArea>(out var _))
		{
			return true;
		}
		List<Collider> obj2 = Facepunch.Pool.Get<List<Collider>>();
		GamePhysics.OverlapSphere(player.eyes.position, 0.1f, obj2, 536870912, QueryTriggerInteraction.Collide);
		bool result2 = true;
		foreach (Collider item in obj2)
		{
			if (!item.gameObject.CompareTag("IgnoreWireCheck"))
			{
				result2 = false;
				break;
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj2);
		return result2;
	}

	private static bool CanModifyEntity(BasePlayer player, IOEntity ent)
	{
		if (ent.AllowWireConnections())
		{
			if (!player.CanBuild(ent.transform.position, ent.transform.rotation, ent.bounds))
			{
				if (player.IsInCreativeMode)
				{
					return Creative.unlimitedIo;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private static bool CanClearEntity(BasePlayer player, IOEntity ent, int slotIndex, bool isInput)
	{
		if (ent.AllowWireConnections())
		{
			if (!ent.CanBreakConnection(player, slotIndex, isInput))
			{
				if (player.IsInCreativeMode)
				{
					return Creative.unlimitedIo;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private static bool SharesRootParent(BaseEntity a, BaseEntity b)
	{
		bool flag = a.IsOnMovingObject();
		bool flag2 = b.IsOnMovingObject();
		if (flag && flag2)
		{
			if (IsCarFuelTank(a) && IsCarFuelTank(b))
			{
				return true;
			}
			return a.GetRootParentEntity() == b.GetRootParentEntity();
		}
		if ((flag || flag2) && !AllowDifferentParentConnections(a) && !AllowDifferentParentConnections(b))
		{
			return false;
		}
		BoatBuildingStation stationOverlappingPosition = BoatBuildingStation.GetStationOverlappingPosition(a.transform.position, a.isServer, 1.5f);
		BoatBuildingStation stationOverlappingPosition2 = BoatBuildingStation.GetStationOverlappingPosition(b.transform.position, b.isServer, 1.5f);
		return stationOverlappingPosition == stationOverlappingPosition2;
	}

	private static bool AllowDifferentParentConnections(BaseEntity ent)
	{
		if (IsCarFuelTank(ent))
		{
			return true;
		}
		return false;
	}

	private static bool IsCarFuelTank(BaseEntity ent)
	{
		if (ent is LiquidContainer)
		{
			return ent.GetParentEntity() is VehicleModuleStorage;
		}
		return false;
	}
}
