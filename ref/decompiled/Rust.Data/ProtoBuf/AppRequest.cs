using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class AppRequest : IDisposable, Pool.IPooled, IProto<AppRequest>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public uint seq;

	[NonSerialized]
	public ulong playerId;

	[NonSerialized]
	public int playerToken;

	[NonSerialized]
	public NetworkableId entityId;

	[NonSerialized]
	public AppEmpty getInfo;

	[NonSerialized]
	public AppEmpty getTime;

	[NonSerialized]
	public AppEmpty getMap;

	[NonSerialized]
	public AppEmpty getTeamInfo;

	[NonSerialized]
	public AppEmpty getTeamChat;

	[NonSerialized]
	public AppSendMessage sendTeamMessage;

	[NonSerialized]
	public AppEmpty getEntityInfo;

	[NonSerialized]
	public AppSetEntityValue setEntityValue;

	[NonSerialized]
	public AppEmpty checkSubscription;

	[NonSerialized]
	public AppFlag setSubscription;

	[NonSerialized]
	public AppEmpty getMapMarkers;

	[NonSerialized]
	public AppPromoteToLeader promoteToLeader;

	[NonSerialized]
	public AppEmpty getClanInfo;

	[NonSerialized]
	public AppSendMessage setClanMotd;

	[NonSerialized]
	public AppEmpty getClanChat;

	[NonSerialized]
	public AppSendMessage sendClanMessage;

	[NonSerialized]
	public AppGetNexusAuth getNexusAuth;

	[NonSerialized]
	public AppCameraSubscribe cameraSubscribe;

	[NonSerialized]
	public AppEmpty cameraUnsubscribe;

	[NonSerialized]
	public AppCameraInput cameraInput;

	[NonSerialized]
	public AppTeamKick kickFromTeam;

	public static void ResetToPool(AppRequest instance)
	{
		if (instance.ShouldPool)
		{
			instance.seq = 0u;
			instance.playerId = 0uL;
			instance.playerToken = 0;
			instance.entityId = default(NetworkableId);
			if (instance.getInfo != null)
			{
				instance.getInfo.ResetToPool();
				instance.getInfo = null;
			}
			if (instance.getTime != null)
			{
				instance.getTime.ResetToPool();
				instance.getTime = null;
			}
			if (instance.getMap != null)
			{
				instance.getMap.ResetToPool();
				instance.getMap = null;
			}
			if (instance.getTeamInfo != null)
			{
				instance.getTeamInfo.ResetToPool();
				instance.getTeamInfo = null;
			}
			if (instance.getTeamChat != null)
			{
				instance.getTeamChat.ResetToPool();
				instance.getTeamChat = null;
			}
			if (instance.sendTeamMessage != null)
			{
				instance.sendTeamMessage.ResetToPool();
				instance.sendTeamMessage = null;
			}
			if (instance.getEntityInfo != null)
			{
				instance.getEntityInfo.ResetToPool();
				instance.getEntityInfo = null;
			}
			if (instance.setEntityValue != null)
			{
				instance.setEntityValue.ResetToPool();
				instance.setEntityValue = null;
			}
			if (instance.checkSubscription != null)
			{
				instance.checkSubscription.ResetToPool();
				instance.checkSubscription = null;
			}
			if (instance.setSubscription != null)
			{
				instance.setSubscription.ResetToPool();
				instance.setSubscription = null;
			}
			if (instance.getMapMarkers != null)
			{
				instance.getMapMarkers.ResetToPool();
				instance.getMapMarkers = null;
			}
			if (instance.promoteToLeader != null)
			{
				instance.promoteToLeader.ResetToPool();
				instance.promoteToLeader = null;
			}
			if (instance.getClanInfo != null)
			{
				instance.getClanInfo.ResetToPool();
				instance.getClanInfo = null;
			}
			if (instance.setClanMotd != null)
			{
				instance.setClanMotd.ResetToPool();
				instance.setClanMotd = null;
			}
			if (instance.getClanChat != null)
			{
				instance.getClanChat.ResetToPool();
				instance.getClanChat = null;
			}
			if (instance.sendClanMessage != null)
			{
				instance.sendClanMessage.ResetToPool();
				instance.sendClanMessage = null;
			}
			if (instance.getNexusAuth != null)
			{
				instance.getNexusAuth.ResetToPool();
				instance.getNexusAuth = null;
			}
			if (instance.cameraSubscribe != null)
			{
				instance.cameraSubscribe.ResetToPool();
				instance.cameraSubscribe = null;
			}
			if (instance.cameraUnsubscribe != null)
			{
				instance.cameraUnsubscribe.ResetToPool();
				instance.cameraUnsubscribe = null;
			}
			if (instance.cameraInput != null)
			{
				instance.cameraInput.ResetToPool();
				instance.cameraInput = null;
			}
			if (instance.kickFromTeam != null)
			{
				instance.kickFromTeam.ResetToPool();
				instance.kickFromTeam = null;
			}
			Pool.Free(ref instance);
		}
	}

	public void ResetToPool()
	{
		ResetToPool(this);
	}

	public virtual void Dispose()
	{
		if (!ShouldPool)
		{
			throw new Exception("Trying to dispose AppRequest with ShouldPool set to false!");
		}
		if (!_disposed)
		{
			ResetToPool();
			_disposed = true;
		}
	}

	public virtual void EnterPool()
	{
		_disposed = true;
	}

	public virtual void LeavePool()
	{
		_disposed = false;
	}

	public void CopyTo(AppRequest instance)
	{
		instance.seq = seq;
		instance.playerId = playerId;
		instance.playerToken = playerToken;
		instance.entityId = entityId;
		if (getInfo != null)
		{
			if (instance.getInfo == null)
			{
				instance.getInfo = getInfo.Copy();
			}
			else
			{
				getInfo.CopyTo(instance.getInfo);
			}
		}
		else
		{
			instance.getInfo = null;
		}
		if (getTime != null)
		{
			if (instance.getTime == null)
			{
				instance.getTime = getTime.Copy();
			}
			else
			{
				getTime.CopyTo(instance.getTime);
			}
		}
		else
		{
			instance.getTime = null;
		}
		if (getMap != null)
		{
			if (instance.getMap == null)
			{
				instance.getMap = getMap.Copy();
			}
			else
			{
				getMap.CopyTo(instance.getMap);
			}
		}
		else
		{
			instance.getMap = null;
		}
		if (getTeamInfo != null)
		{
			if (instance.getTeamInfo == null)
			{
				instance.getTeamInfo = getTeamInfo.Copy();
			}
			else
			{
				getTeamInfo.CopyTo(instance.getTeamInfo);
			}
		}
		else
		{
			instance.getTeamInfo = null;
		}
		if (getTeamChat != null)
		{
			if (instance.getTeamChat == null)
			{
				instance.getTeamChat = getTeamChat.Copy();
			}
			else
			{
				getTeamChat.CopyTo(instance.getTeamChat);
			}
		}
		else
		{
			instance.getTeamChat = null;
		}
		if (sendTeamMessage != null)
		{
			if (instance.sendTeamMessage == null)
			{
				instance.sendTeamMessage = sendTeamMessage.Copy();
			}
			else
			{
				sendTeamMessage.CopyTo(instance.sendTeamMessage);
			}
		}
		else
		{
			instance.sendTeamMessage = null;
		}
		if (getEntityInfo != null)
		{
			if (instance.getEntityInfo == null)
			{
				instance.getEntityInfo = getEntityInfo.Copy();
			}
			else
			{
				getEntityInfo.CopyTo(instance.getEntityInfo);
			}
		}
		else
		{
			instance.getEntityInfo = null;
		}
		if (setEntityValue != null)
		{
			if (instance.setEntityValue == null)
			{
				instance.setEntityValue = setEntityValue.Copy();
			}
			else
			{
				setEntityValue.CopyTo(instance.setEntityValue);
			}
		}
		else
		{
			instance.setEntityValue = null;
		}
		if (checkSubscription != null)
		{
			if (instance.checkSubscription == null)
			{
				instance.checkSubscription = checkSubscription.Copy();
			}
			else
			{
				checkSubscription.CopyTo(instance.checkSubscription);
			}
		}
		else
		{
			instance.checkSubscription = null;
		}
		if (setSubscription != null)
		{
			if (instance.setSubscription == null)
			{
				instance.setSubscription = setSubscription.Copy();
			}
			else
			{
				setSubscription.CopyTo(instance.setSubscription);
			}
		}
		else
		{
			instance.setSubscription = null;
		}
		if (getMapMarkers != null)
		{
			if (instance.getMapMarkers == null)
			{
				instance.getMapMarkers = getMapMarkers.Copy();
			}
			else
			{
				getMapMarkers.CopyTo(instance.getMapMarkers);
			}
		}
		else
		{
			instance.getMapMarkers = null;
		}
		if (promoteToLeader != null)
		{
			if (instance.promoteToLeader == null)
			{
				instance.promoteToLeader = promoteToLeader.Copy();
			}
			else
			{
				promoteToLeader.CopyTo(instance.promoteToLeader);
			}
		}
		else
		{
			instance.promoteToLeader = null;
		}
		if (getClanInfo != null)
		{
			if (instance.getClanInfo == null)
			{
				instance.getClanInfo = getClanInfo.Copy();
			}
			else
			{
				getClanInfo.CopyTo(instance.getClanInfo);
			}
		}
		else
		{
			instance.getClanInfo = null;
		}
		if (setClanMotd != null)
		{
			if (instance.setClanMotd == null)
			{
				instance.setClanMotd = setClanMotd.Copy();
			}
			else
			{
				setClanMotd.CopyTo(instance.setClanMotd);
			}
		}
		else
		{
			instance.setClanMotd = null;
		}
		if (getClanChat != null)
		{
			if (instance.getClanChat == null)
			{
				instance.getClanChat = getClanChat.Copy();
			}
			else
			{
				getClanChat.CopyTo(instance.getClanChat);
			}
		}
		else
		{
			instance.getClanChat = null;
		}
		if (sendClanMessage != null)
		{
			if (instance.sendClanMessage == null)
			{
				instance.sendClanMessage = sendClanMessage.Copy();
			}
			else
			{
				sendClanMessage.CopyTo(instance.sendClanMessage);
			}
		}
		else
		{
			instance.sendClanMessage = null;
		}
		if (getNexusAuth != null)
		{
			if (instance.getNexusAuth == null)
			{
				instance.getNexusAuth = getNexusAuth.Copy();
			}
			else
			{
				getNexusAuth.CopyTo(instance.getNexusAuth);
			}
		}
		else
		{
			instance.getNexusAuth = null;
		}
		if (cameraSubscribe != null)
		{
			if (instance.cameraSubscribe == null)
			{
				instance.cameraSubscribe = cameraSubscribe.Copy();
			}
			else
			{
				cameraSubscribe.CopyTo(instance.cameraSubscribe);
			}
		}
		else
		{
			instance.cameraSubscribe = null;
		}
		if (cameraUnsubscribe != null)
		{
			if (instance.cameraUnsubscribe == null)
			{
				instance.cameraUnsubscribe = cameraUnsubscribe.Copy();
			}
			else
			{
				cameraUnsubscribe.CopyTo(instance.cameraUnsubscribe);
			}
		}
		else
		{
			instance.cameraUnsubscribe = null;
		}
		if (cameraInput != null)
		{
			if (instance.cameraInput == null)
			{
				instance.cameraInput = cameraInput.Copy();
			}
			else
			{
				cameraInput.CopyTo(instance.cameraInput);
			}
		}
		else
		{
			instance.cameraInput = null;
		}
		if (kickFromTeam != null)
		{
			if (instance.kickFromTeam == null)
			{
				instance.kickFromTeam = kickFromTeam.Copy();
			}
			else
			{
				kickFromTeam.CopyTo(instance.kickFromTeam);
			}
		}
		else
		{
			instance.kickFromTeam = null;
		}
	}

	public AppRequest Copy()
	{
		AppRequest appRequest = Pool.Get<AppRequest>();
		CopyTo(appRequest);
		return appRequest;
	}

	public static AppRequest Deserialize(BufferStream stream)
	{
		AppRequest appRequest = Pool.Get<AppRequest>();
		Deserialize(stream, appRequest, isDelta: false);
		return appRequest;
	}

	public static AppRequest DeserializeLengthDelimited(BufferStream stream)
	{
		AppRequest appRequest = Pool.Get<AppRequest>();
		DeserializeLengthDelimited(stream, appRequest, isDelta: false);
		return appRequest;
	}

	public static AppRequest DeserializeLength(BufferStream stream, int length)
	{
		AppRequest appRequest = Pool.Get<AppRequest>();
		DeserializeLength(stream, length, appRequest, isDelta: false);
		return appRequest;
	}

	public static AppRequest Deserialize(byte[] buffer)
	{
		AppRequest appRequest = Pool.Get<AppRequest>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, appRequest, isDelta: false);
		return appRequest;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, AppRequest previous)
	{
		if (previous == null)
		{
			Serialize(stream, this);
		}
		else
		{
			SerializeDelta(stream, this, previous);
		}
	}

	public virtual void ReadFromStream(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void ReadFromStream(BufferStream stream, int size, bool isDelta = false)
	{
		DeserializeLength(stream, size, this, isDelta);
	}

	public static AppRequest Deserialize(BufferStream stream, AppRequest instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.seq = 0u;
			instance.playerId = 0uL;
			instance.playerToken = 0;
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.AppRequest");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				instance.seq = ProtocolParser.ReadUInt32(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				instance.playerId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				instance.playerToken = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				instance.entityId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (instance.getInfo == null)
				{
					instance.getInfo = AppEmpty.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppEmpty.DeserializeLengthDelimited(stream, instance.getInfo, isDelta);
				}
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (instance.getTime == null)
				{
					instance.getTime = AppEmpty.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppEmpty.DeserializeLengthDelimited(stream, instance.getTime, isDelta);
				}
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (instance.getMap == null)
				{
					instance.getMap = AppEmpty.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppEmpty.DeserializeLengthDelimited(stream, instance.getMap, isDelta);
				}
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (instance.getTeamInfo == null)
				{
					instance.getTeamInfo = AppEmpty.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppEmpty.DeserializeLengthDelimited(stream, instance.getTeamInfo, isDelta);
				}
				continue;
			case 98:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (instance.getTeamChat == null)
				{
					instance.getTeamChat = AppEmpty.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppEmpty.DeserializeLengthDelimited(stream, instance.getTeamChat, isDelta);
				}
				continue;
			case 106:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (instance.sendTeamMessage == null)
				{
					instance.sendTeamMessage = AppSendMessage.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppSendMessage.DeserializeLengthDelimited(stream, instance.sendTeamMessage, isDelta);
				}
				continue;
			case 114:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (instance.getEntityInfo == null)
				{
					instance.getEntityInfo = AppEmpty.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppEmpty.DeserializeLengthDelimited(stream, instance.getEntityInfo, isDelta);
				}
				continue;
			case 122:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (instance.setEntityValue == null)
				{
					instance.setEntityValue = AppSetEntityValue.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppSetEntityValue.DeserializeLengthDelimited(stream, instance.setEntityValue, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.checkSubscription == null)
					{
						instance.checkSubscription = AppEmpty.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppEmpty.DeserializeLengthDelimited(stream, instance.checkSubscription, isDelta);
					}
				}
				break;
			case 17u:
				stream.ValidateFieldOrder(ref lastFieldId, 17u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.setSubscription == null)
					{
						instance.setSubscription = AppFlag.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppFlag.DeserializeLengthDelimited(stream, instance.setSubscription, isDelta);
					}
				}
				break;
			case 18u:
				stream.ValidateFieldOrder(ref lastFieldId, 18u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.getMapMarkers == null)
					{
						instance.getMapMarkers = AppEmpty.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppEmpty.DeserializeLengthDelimited(stream, instance.getMapMarkers, isDelta);
					}
				}
				break;
			case 20u:
				stream.ValidateFieldOrder(ref lastFieldId, 20u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.promoteToLeader == null)
					{
						instance.promoteToLeader = AppPromoteToLeader.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppPromoteToLeader.DeserializeLengthDelimited(stream, instance.promoteToLeader, isDelta);
					}
				}
				break;
			case 21u:
				stream.ValidateFieldOrder(ref lastFieldId, 21u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.getClanInfo == null)
					{
						instance.getClanInfo = AppEmpty.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppEmpty.DeserializeLengthDelimited(stream, instance.getClanInfo, isDelta);
					}
				}
				break;
			case 22u:
				stream.ValidateFieldOrder(ref lastFieldId, 22u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.setClanMotd == null)
					{
						instance.setClanMotd = AppSendMessage.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppSendMessage.DeserializeLengthDelimited(stream, instance.setClanMotd, isDelta);
					}
				}
				break;
			case 23u:
				stream.ValidateFieldOrder(ref lastFieldId, 23u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.getClanChat == null)
					{
						instance.getClanChat = AppEmpty.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppEmpty.DeserializeLengthDelimited(stream, instance.getClanChat, isDelta);
					}
				}
				break;
			case 24u:
				stream.ValidateFieldOrder(ref lastFieldId, 24u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.sendClanMessage == null)
					{
						instance.sendClanMessage = AppSendMessage.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppSendMessage.DeserializeLengthDelimited(stream, instance.sendClanMessage, isDelta);
					}
				}
				break;
			case 25u:
				stream.ValidateFieldOrder(ref lastFieldId, 25u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.getNexusAuth == null)
					{
						instance.getNexusAuth = AppGetNexusAuth.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppGetNexusAuth.DeserializeLengthDelimited(stream, instance.getNexusAuth, isDelta);
					}
				}
				break;
			case 30u:
				stream.ValidateFieldOrder(ref lastFieldId, 30u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.cameraSubscribe == null)
					{
						instance.cameraSubscribe = AppCameraSubscribe.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppCameraSubscribe.DeserializeLengthDelimited(stream, instance.cameraSubscribe, isDelta);
					}
				}
				break;
			case 31u:
				stream.ValidateFieldOrder(ref lastFieldId, 31u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.cameraUnsubscribe == null)
					{
						instance.cameraUnsubscribe = AppEmpty.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppEmpty.DeserializeLengthDelimited(stream, instance.cameraUnsubscribe, isDelta);
					}
				}
				break;
			case 32u:
				stream.ValidateFieldOrder(ref lastFieldId, 32u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.cameraInput == null)
					{
						instance.cameraInput = AppCameraInput.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppCameraInput.DeserializeLengthDelimited(stream, instance.cameraInput, isDelta);
					}
				}
				break;
			case 33u:
				stream.ValidateFieldOrder(ref lastFieldId, 33u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.kickFromTeam == null)
					{
						instance.kickFromTeam = AppTeamKick.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppTeamKick.DeserializeLengthDelimited(stream, instance.kickFromTeam, isDelta);
					}
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AppRequest DeserializeLengthDelimited(BufferStream stream, AppRequest instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.seq = 0u;
			instance.playerId = 0uL;
			instance.playerToken = 0;
		}
		long num = ProtocolParser.ReadUInt32(stream);
		num += stream.Position;
		uint lastFieldId = 0u;
		while (true)
		{
			if (stream.Position >= num)
			{
				if (stream.Position == num)
				{
					break;
				}
				throw new ProtocolBufferException("Read past max limit");
			}
			int num2 = stream.ReadByte();
			if (num2 == -1)
			{
				throw new EndOfStreamException();
			}
			stream.ConsumeFieldOperation("ProtoBuf.AppRequest");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				instance.seq = ProtocolParser.ReadUInt32(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				instance.playerId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				instance.playerToken = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				instance.entityId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (instance.getInfo == null)
				{
					instance.getInfo = AppEmpty.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppEmpty.DeserializeLengthDelimited(stream, instance.getInfo, isDelta);
				}
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (instance.getTime == null)
				{
					instance.getTime = AppEmpty.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppEmpty.DeserializeLengthDelimited(stream, instance.getTime, isDelta);
				}
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (instance.getMap == null)
				{
					instance.getMap = AppEmpty.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppEmpty.DeserializeLengthDelimited(stream, instance.getMap, isDelta);
				}
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (instance.getTeamInfo == null)
				{
					instance.getTeamInfo = AppEmpty.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppEmpty.DeserializeLengthDelimited(stream, instance.getTeamInfo, isDelta);
				}
				continue;
			case 98:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (instance.getTeamChat == null)
				{
					instance.getTeamChat = AppEmpty.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppEmpty.DeserializeLengthDelimited(stream, instance.getTeamChat, isDelta);
				}
				continue;
			case 106:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (instance.sendTeamMessage == null)
				{
					instance.sendTeamMessage = AppSendMessage.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppSendMessage.DeserializeLengthDelimited(stream, instance.sendTeamMessage, isDelta);
				}
				continue;
			case 114:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (instance.getEntityInfo == null)
				{
					instance.getEntityInfo = AppEmpty.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppEmpty.DeserializeLengthDelimited(stream, instance.getEntityInfo, isDelta);
				}
				continue;
			case 122:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (instance.setEntityValue == null)
				{
					instance.setEntityValue = AppSetEntityValue.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppSetEntityValue.DeserializeLengthDelimited(stream, instance.setEntityValue, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.checkSubscription == null)
					{
						instance.checkSubscription = AppEmpty.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppEmpty.DeserializeLengthDelimited(stream, instance.checkSubscription, isDelta);
					}
				}
				break;
			case 17u:
				stream.ValidateFieldOrder(ref lastFieldId, 17u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.setSubscription == null)
					{
						instance.setSubscription = AppFlag.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppFlag.DeserializeLengthDelimited(stream, instance.setSubscription, isDelta);
					}
				}
				break;
			case 18u:
				stream.ValidateFieldOrder(ref lastFieldId, 18u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.getMapMarkers == null)
					{
						instance.getMapMarkers = AppEmpty.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppEmpty.DeserializeLengthDelimited(stream, instance.getMapMarkers, isDelta);
					}
				}
				break;
			case 20u:
				stream.ValidateFieldOrder(ref lastFieldId, 20u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.promoteToLeader == null)
					{
						instance.promoteToLeader = AppPromoteToLeader.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppPromoteToLeader.DeserializeLengthDelimited(stream, instance.promoteToLeader, isDelta);
					}
				}
				break;
			case 21u:
				stream.ValidateFieldOrder(ref lastFieldId, 21u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.getClanInfo == null)
					{
						instance.getClanInfo = AppEmpty.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppEmpty.DeserializeLengthDelimited(stream, instance.getClanInfo, isDelta);
					}
				}
				break;
			case 22u:
				stream.ValidateFieldOrder(ref lastFieldId, 22u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.setClanMotd == null)
					{
						instance.setClanMotd = AppSendMessage.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppSendMessage.DeserializeLengthDelimited(stream, instance.setClanMotd, isDelta);
					}
				}
				break;
			case 23u:
				stream.ValidateFieldOrder(ref lastFieldId, 23u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.getClanChat == null)
					{
						instance.getClanChat = AppEmpty.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppEmpty.DeserializeLengthDelimited(stream, instance.getClanChat, isDelta);
					}
				}
				break;
			case 24u:
				stream.ValidateFieldOrder(ref lastFieldId, 24u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.sendClanMessage == null)
					{
						instance.sendClanMessage = AppSendMessage.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppSendMessage.DeserializeLengthDelimited(stream, instance.sendClanMessage, isDelta);
					}
				}
				break;
			case 25u:
				stream.ValidateFieldOrder(ref lastFieldId, 25u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.getNexusAuth == null)
					{
						instance.getNexusAuth = AppGetNexusAuth.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppGetNexusAuth.DeserializeLengthDelimited(stream, instance.getNexusAuth, isDelta);
					}
				}
				break;
			case 30u:
				stream.ValidateFieldOrder(ref lastFieldId, 30u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.cameraSubscribe == null)
					{
						instance.cameraSubscribe = AppCameraSubscribe.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppCameraSubscribe.DeserializeLengthDelimited(stream, instance.cameraSubscribe, isDelta);
					}
				}
				break;
			case 31u:
				stream.ValidateFieldOrder(ref lastFieldId, 31u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.cameraUnsubscribe == null)
					{
						instance.cameraUnsubscribe = AppEmpty.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppEmpty.DeserializeLengthDelimited(stream, instance.cameraUnsubscribe, isDelta);
					}
				}
				break;
			case 32u:
				stream.ValidateFieldOrder(ref lastFieldId, 32u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.cameraInput == null)
					{
						instance.cameraInput = AppCameraInput.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppCameraInput.DeserializeLengthDelimited(stream, instance.cameraInput, isDelta);
					}
				}
				break;
			case 33u:
				stream.ValidateFieldOrder(ref lastFieldId, 33u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.kickFromTeam == null)
					{
						instance.kickFromTeam = AppTeamKick.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppTeamKick.DeserializeLengthDelimited(stream, instance.kickFromTeam, isDelta);
					}
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AppRequest DeserializeLength(BufferStream stream, int length, AppRequest instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.seq = 0u;
			instance.playerId = 0uL;
			instance.playerToken = 0;
		}
		long num = stream.Position + length;
		uint lastFieldId = 0u;
		while (true)
		{
			if (stream.Position >= num)
			{
				if (stream.Position == num)
				{
					break;
				}
				throw new ProtocolBufferException("Read past max limit");
			}
			int num2 = stream.ReadByte();
			if (num2 == -1)
			{
				throw new EndOfStreamException();
			}
			stream.ConsumeFieldOperation("ProtoBuf.AppRequest");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				instance.seq = ProtocolParser.ReadUInt32(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				instance.playerId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				instance.playerToken = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				instance.entityId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (instance.getInfo == null)
				{
					instance.getInfo = AppEmpty.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppEmpty.DeserializeLengthDelimited(stream, instance.getInfo, isDelta);
				}
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (instance.getTime == null)
				{
					instance.getTime = AppEmpty.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppEmpty.DeserializeLengthDelimited(stream, instance.getTime, isDelta);
				}
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (instance.getMap == null)
				{
					instance.getMap = AppEmpty.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppEmpty.DeserializeLengthDelimited(stream, instance.getMap, isDelta);
				}
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (instance.getTeamInfo == null)
				{
					instance.getTeamInfo = AppEmpty.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppEmpty.DeserializeLengthDelimited(stream, instance.getTeamInfo, isDelta);
				}
				continue;
			case 98:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (instance.getTeamChat == null)
				{
					instance.getTeamChat = AppEmpty.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppEmpty.DeserializeLengthDelimited(stream, instance.getTeamChat, isDelta);
				}
				continue;
			case 106:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (instance.sendTeamMessage == null)
				{
					instance.sendTeamMessage = AppSendMessage.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppSendMessage.DeserializeLengthDelimited(stream, instance.sendTeamMessage, isDelta);
				}
				continue;
			case 114:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (instance.getEntityInfo == null)
				{
					instance.getEntityInfo = AppEmpty.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppEmpty.DeserializeLengthDelimited(stream, instance.getEntityInfo, isDelta);
				}
				continue;
			case 122:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (instance.setEntityValue == null)
				{
					instance.setEntityValue = AppSetEntityValue.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppSetEntityValue.DeserializeLengthDelimited(stream, instance.setEntityValue, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.checkSubscription == null)
					{
						instance.checkSubscription = AppEmpty.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppEmpty.DeserializeLengthDelimited(stream, instance.checkSubscription, isDelta);
					}
				}
				break;
			case 17u:
				stream.ValidateFieldOrder(ref lastFieldId, 17u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.setSubscription == null)
					{
						instance.setSubscription = AppFlag.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppFlag.DeserializeLengthDelimited(stream, instance.setSubscription, isDelta);
					}
				}
				break;
			case 18u:
				stream.ValidateFieldOrder(ref lastFieldId, 18u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.getMapMarkers == null)
					{
						instance.getMapMarkers = AppEmpty.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppEmpty.DeserializeLengthDelimited(stream, instance.getMapMarkers, isDelta);
					}
				}
				break;
			case 20u:
				stream.ValidateFieldOrder(ref lastFieldId, 20u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.promoteToLeader == null)
					{
						instance.promoteToLeader = AppPromoteToLeader.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppPromoteToLeader.DeserializeLengthDelimited(stream, instance.promoteToLeader, isDelta);
					}
				}
				break;
			case 21u:
				stream.ValidateFieldOrder(ref lastFieldId, 21u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.getClanInfo == null)
					{
						instance.getClanInfo = AppEmpty.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppEmpty.DeserializeLengthDelimited(stream, instance.getClanInfo, isDelta);
					}
				}
				break;
			case 22u:
				stream.ValidateFieldOrder(ref lastFieldId, 22u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.setClanMotd == null)
					{
						instance.setClanMotd = AppSendMessage.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppSendMessage.DeserializeLengthDelimited(stream, instance.setClanMotd, isDelta);
					}
				}
				break;
			case 23u:
				stream.ValidateFieldOrder(ref lastFieldId, 23u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.getClanChat == null)
					{
						instance.getClanChat = AppEmpty.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppEmpty.DeserializeLengthDelimited(stream, instance.getClanChat, isDelta);
					}
				}
				break;
			case 24u:
				stream.ValidateFieldOrder(ref lastFieldId, 24u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.sendClanMessage == null)
					{
						instance.sendClanMessage = AppSendMessage.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppSendMessage.DeserializeLengthDelimited(stream, instance.sendClanMessage, isDelta);
					}
				}
				break;
			case 25u:
				stream.ValidateFieldOrder(ref lastFieldId, 25u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.getNexusAuth == null)
					{
						instance.getNexusAuth = AppGetNexusAuth.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppGetNexusAuth.DeserializeLengthDelimited(stream, instance.getNexusAuth, isDelta);
					}
				}
				break;
			case 30u:
				stream.ValidateFieldOrder(ref lastFieldId, 30u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.cameraSubscribe == null)
					{
						instance.cameraSubscribe = AppCameraSubscribe.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppCameraSubscribe.DeserializeLengthDelimited(stream, instance.cameraSubscribe, isDelta);
					}
				}
				break;
			case 31u:
				stream.ValidateFieldOrder(ref lastFieldId, 31u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.cameraUnsubscribe == null)
					{
						instance.cameraUnsubscribe = AppEmpty.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppEmpty.DeserializeLengthDelimited(stream, instance.cameraUnsubscribe, isDelta);
					}
				}
				break;
			case 32u:
				stream.ValidateFieldOrder(ref lastFieldId, 32u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.cameraInput == null)
					{
						instance.cameraInput = AppCameraInput.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppCameraInput.DeserializeLengthDelimited(stream, instance.cameraInput, isDelta);
					}
				}
				break;
			case 33u:
				stream.ValidateFieldOrder(ref lastFieldId, 33u, fieldIsRepeated: false, "ProtoBuf.AppRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.kickFromTeam == null)
					{
						instance.kickFromTeam = AppTeamKick.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppTeamKick.DeserializeLengthDelimited(stream, instance.kickFromTeam, isDelta);
					}
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, AppRequest instance, AppRequest previous)
	{
		if (instance.seq != previous.seq)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt32(stream, instance.seq);
		}
		if (instance.playerId != previous.playerId)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, instance.playerId);
		}
		if (instance.playerToken != previous.playerToken)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.playerToken);
		}
		stream.WriteByte(32);
		ProtocolParser.WriteUInt64(stream, instance.entityId.Value);
		if (instance.getInfo != null)
		{
			stream.WriteByte(66);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			AppEmpty.SerializeDelta(stream, instance.getInfo, previous.getInfo);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field getInfo (ProtoBuf.AppEmpty)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.getTime != null)
		{
			stream.WriteByte(74);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			AppEmpty.SerializeDelta(stream, instance.getTime, previous.getTime);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field getTime (ProtoBuf.AppEmpty)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.getMap != null)
		{
			stream.WriteByte(82);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			AppEmpty.SerializeDelta(stream, instance.getMap, previous.getMap);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field getMap (ProtoBuf.AppEmpty)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
		if (instance.getTeamInfo != null)
		{
			stream.WriteByte(90);
			BufferStream.RangeHandle range4 = stream.GetRange(1);
			int position4 = stream.Position;
			AppEmpty.SerializeDelta(stream, instance.getTeamInfo, previous.getTeamInfo);
			int num4 = stream.Position - position4;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field getTeamInfo (ProtoBuf.AppEmpty)");
			}
			Span<byte> span4 = range4.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span4, 0);
		}
		if (instance.getTeamChat != null)
		{
			stream.WriteByte(98);
			BufferStream.RangeHandle range5 = stream.GetRange(1);
			int position5 = stream.Position;
			AppEmpty.SerializeDelta(stream, instance.getTeamChat, previous.getTeamChat);
			int num5 = stream.Position - position5;
			if (num5 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field getTeamChat (ProtoBuf.AppEmpty)");
			}
			Span<byte> span5 = range5.GetSpan();
			ProtocolParser.WriteUInt32((uint)num5, span5, 0);
		}
		if (instance.sendTeamMessage != null)
		{
			stream.WriteByte(106);
			BufferStream.RangeHandle range6 = stream.GetRange(5);
			int position6 = stream.Position;
			AppSendMessage.SerializeDelta(stream, instance.sendTeamMessage, previous.sendTeamMessage);
			int val = stream.Position - position6;
			Span<byte> span6 = range6.GetSpan();
			int num6 = ProtocolParser.WriteUInt32((uint)val, span6, 0);
			if (num6 < 5)
			{
				span6[num6 - 1] |= 128;
				while (num6 < 4)
				{
					span6[num6++] = 128;
				}
				span6[4] = 0;
			}
		}
		if (instance.getEntityInfo != null)
		{
			stream.WriteByte(114);
			BufferStream.RangeHandle range7 = stream.GetRange(1);
			int position7 = stream.Position;
			AppEmpty.SerializeDelta(stream, instance.getEntityInfo, previous.getEntityInfo);
			int num7 = stream.Position - position7;
			if (num7 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field getEntityInfo (ProtoBuf.AppEmpty)");
			}
			Span<byte> span7 = range7.GetSpan();
			ProtocolParser.WriteUInt32((uint)num7, span7, 0);
		}
		if (instance.setEntityValue != null)
		{
			stream.WriteByte(122);
			BufferStream.RangeHandle range8 = stream.GetRange(1);
			int position8 = stream.Position;
			AppSetEntityValue.SerializeDelta(stream, instance.setEntityValue, previous.setEntityValue);
			int num8 = stream.Position - position8;
			if (num8 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field setEntityValue (ProtoBuf.AppSetEntityValue)");
			}
			Span<byte> span8 = range8.GetSpan();
			ProtocolParser.WriteUInt32((uint)num8, span8, 0);
		}
		if (instance.checkSubscription != null)
		{
			stream.WriteByte(130);
			stream.WriteByte(1);
			BufferStream.RangeHandle range9 = stream.GetRange(1);
			int position9 = stream.Position;
			AppEmpty.SerializeDelta(stream, instance.checkSubscription, previous.checkSubscription);
			int num9 = stream.Position - position9;
			if (num9 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field checkSubscription (ProtoBuf.AppEmpty)");
			}
			Span<byte> span9 = range9.GetSpan();
			ProtocolParser.WriteUInt32((uint)num9, span9, 0);
		}
		if (instance.setSubscription != null)
		{
			stream.WriteByte(138);
			stream.WriteByte(1);
			BufferStream.RangeHandle range10 = stream.GetRange(1);
			int position10 = stream.Position;
			AppFlag.SerializeDelta(stream, instance.setSubscription, previous.setSubscription);
			int num10 = stream.Position - position10;
			if (num10 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field setSubscription (ProtoBuf.AppFlag)");
			}
			Span<byte> span10 = range10.GetSpan();
			ProtocolParser.WriteUInt32((uint)num10, span10, 0);
		}
		if (instance.getMapMarkers != null)
		{
			stream.WriteByte(146);
			stream.WriteByte(1);
			BufferStream.RangeHandle range11 = stream.GetRange(1);
			int position11 = stream.Position;
			AppEmpty.SerializeDelta(stream, instance.getMapMarkers, previous.getMapMarkers);
			int num11 = stream.Position - position11;
			if (num11 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field getMapMarkers (ProtoBuf.AppEmpty)");
			}
			Span<byte> span11 = range11.GetSpan();
			ProtocolParser.WriteUInt32((uint)num11, span11, 0);
		}
		if (instance.promoteToLeader != null)
		{
			stream.WriteByte(162);
			stream.WriteByte(1);
			BufferStream.RangeHandle range12 = stream.GetRange(1);
			int position12 = stream.Position;
			AppPromoteToLeader.SerializeDelta(stream, instance.promoteToLeader, previous.promoteToLeader);
			int num12 = stream.Position - position12;
			if (num12 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field promoteToLeader (ProtoBuf.AppPromoteToLeader)");
			}
			Span<byte> span12 = range12.GetSpan();
			ProtocolParser.WriteUInt32((uint)num12, span12, 0);
		}
		if (instance.getClanInfo != null)
		{
			stream.WriteByte(170);
			stream.WriteByte(1);
			BufferStream.RangeHandle range13 = stream.GetRange(1);
			int position13 = stream.Position;
			AppEmpty.SerializeDelta(stream, instance.getClanInfo, previous.getClanInfo);
			int num13 = stream.Position - position13;
			if (num13 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field getClanInfo (ProtoBuf.AppEmpty)");
			}
			Span<byte> span13 = range13.GetSpan();
			ProtocolParser.WriteUInt32((uint)num13, span13, 0);
		}
		if (instance.setClanMotd != null)
		{
			stream.WriteByte(178);
			stream.WriteByte(1);
			BufferStream.RangeHandle range14 = stream.GetRange(5);
			int position14 = stream.Position;
			AppSendMessage.SerializeDelta(stream, instance.setClanMotd, previous.setClanMotd);
			int val2 = stream.Position - position14;
			Span<byte> span14 = range14.GetSpan();
			int num14 = ProtocolParser.WriteUInt32((uint)val2, span14, 0);
			if (num14 < 5)
			{
				span14[num14 - 1] |= 128;
				while (num14 < 4)
				{
					span14[num14++] = 128;
				}
				span14[4] = 0;
			}
		}
		if (instance.getClanChat != null)
		{
			stream.WriteByte(186);
			stream.WriteByte(1);
			BufferStream.RangeHandle range15 = stream.GetRange(1);
			int position15 = stream.Position;
			AppEmpty.SerializeDelta(stream, instance.getClanChat, previous.getClanChat);
			int num15 = stream.Position - position15;
			if (num15 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field getClanChat (ProtoBuf.AppEmpty)");
			}
			Span<byte> span15 = range15.GetSpan();
			ProtocolParser.WriteUInt32((uint)num15, span15, 0);
		}
		if (instance.sendClanMessage != null)
		{
			stream.WriteByte(194);
			stream.WriteByte(1);
			BufferStream.RangeHandle range16 = stream.GetRange(5);
			int position16 = stream.Position;
			AppSendMessage.SerializeDelta(stream, instance.sendClanMessage, previous.sendClanMessage);
			int val3 = stream.Position - position16;
			Span<byte> span16 = range16.GetSpan();
			int num16 = ProtocolParser.WriteUInt32((uint)val3, span16, 0);
			if (num16 < 5)
			{
				span16[num16 - 1] |= 128;
				while (num16 < 4)
				{
					span16[num16++] = 128;
				}
				span16[4] = 0;
			}
		}
		if (instance.getNexusAuth != null)
		{
			stream.WriteByte(202);
			stream.WriteByte(1);
			BufferStream.RangeHandle range17 = stream.GetRange(5);
			int position17 = stream.Position;
			AppGetNexusAuth.SerializeDelta(stream, instance.getNexusAuth, previous.getNexusAuth);
			int val4 = stream.Position - position17;
			Span<byte> span17 = range17.GetSpan();
			int num17 = ProtocolParser.WriteUInt32((uint)val4, span17, 0);
			if (num17 < 5)
			{
				span17[num17 - 1] |= 128;
				while (num17 < 4)
				{
					span17[num17++] = 128;
				}
				span17[4] = 0;
			}
		}
		if (instance.cameraSubscribe != null)
		{
			stream.WriteByte(242);
			stream.WriteByte(1);
			BufferStream.RangeHandle range18 = stream.GetRange(5);
			int position18 = stream.Position;
			AppCameraSubscribe.SerializeDelta(stream, instance.cameraSubscribe, previous.cameraSubscribe);
			int val5 = stream.Position - position18;
			Span<byte> span18 = range18.GetSpan();
			int num18 = ProtocolParser.WriteUInt32((uint)val5, span18, 0);
			if (num18 < 5)
			{
				span18[num18 - 1] |= 128;
				while (num18 < 4)
				{
					span18[num18++] = 128;
				}
				span18[4] = 0;
			}
		}
		if (instance.cameraUnsubscribe != null)
		{
			stream.WriteByte(250);
			stream.WriteByte(1);
			BufferStream.RangeHandle range19 = stream.GetRange(1);
			int position19 = stream.Position;
			AppEmpty.SerializeDelta(stream, instance.cameraUnsubscribe, previous.cameraUnsubscribe);
			int num19 = stream.Position - position19;
			if (num19 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field cameraUnsubscribe (ProtoBuf.AppEmpty)");
			}
			Span<byte> span19 = range19.GetSpan();
			ProtocolParser.WriteUInt32((uint)num19, span19, 0);
		}
		if (instance.cameraInput != null)
		{
			stream.WriteByte(130);
			stream.WriteByte(2);
			BufferStream.RangeHandle range20 = stream.GetRange(1);
			int position20 = stream.Position;
			AppCameraInput.SerializeDelta(stream, instance.cameraInput, previous.cameraInput);
			int num20 = stream.Position - position20;
			if (num20 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field cameraInput (ProtoBuf.AppCameraInput)");
			}
			Span<byte> span20 = range20.GetSpan();
			ProtocolParser.WriteUInt32((uint)num20, span20, 0);
		}
		if (instance.kickFromTeam != null)
		{
			stream.WriteByte(138);
			stream.WriteByte(2);
			BufferStream.RangeHandle range21 = stream.GetRange(1);
			int position21 = stream.Position;
			AppTeamKick.SerializeDelta(stream, instance.kickFromTeam, previous.kickFromTeam);
			int num21 = stream.Position - position21;
			if (num21 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field kickFromTeam (ProtoBuf.AppTeamKick)");
			}
			Span<byte> span21 = range21.GetSpan();
			ProtocolParser.WriteUInt32((uint)num21, span21, 0);
		}
	}

	public static void Serialize(BufferStream stream, AppRequest instance)
	{
		if (instance.seq != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt32(stream, instance.seq);
		}
		if (instance.playerId != 0L)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, instance.playerId);
		}
		if (instance.playerToken != 0)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.playerToken);
		}
		if (instance.entityId != default(NetworkableId))
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, instance.entityId.Value);
		}
		if (instance.getInfo != null)
		{
			stream.WriteByte(66);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			AppEmpty.Serialize(stream, instance.getInfo);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field getInfo (ProtoBuf.AppEmpty)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.getTime != null)
		{
			stream.WriteByte(74);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			AppEmpty.Serialize(stream, instance.getTime);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field getTime (ProtoBuf.AppEmpty)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.getMap != null)
		{
			stream.WriteByte(82);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			AppEmpty.Serialize(stream, instance.getMap);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field getMap (ProtoBuf.AppEmpty)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
		if (instance.getTeamInfo != null)
		{
			stream.WriteByte(90);
			BufferStream.RangeHandle range4 = stream.GetRange(1);
			int position4 = stream.Position;
			AppEmpty.Serialize(stream, instance.getTeamInfo);
			int num4 = stream.Position - position4;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field getTeamInfo (ProtoBuf.AppEmpty)");
			}
			Span<byte> span4 = range4.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span4, 0);
		}
		if (instance.getTeamChat != null)
		{
			stream.WriteByte(98);
			BufferStream.RangeHandle range5 = stream.GetRange(1);
			int position5 = stream.Position;
			AppEmpty.Serialize(stream, instance.getTeamChat);
			int num5 = stream.Position - position5;
			if (num5 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field getTeamChat (ProtoBuf.AppEmpty)");
			}
			Span<byte> span5 = range5.GetSpan();
			ProtocolParser.WriteUInt32((uint)num5, span5, 0);
		}
		if (instance.sendTeamMessage != null)
		{
			stream.WriteByte(106);
			BufferStream.RangeHandle range6 = stream.GetRange(5);
			int position6 = stream.Position;
			AppSendMessage.Serialize(stream, instance.sendTeamMessage);
			int val = stream.Position - position6;
			Span<byte> span6 = range6.GetSpan();
			int num6 = ProtocolParser.WriteUInt32((uint)val, span6, 0);
			if (num6 < 5)
			{
				span6[num6 - 1] |= 128;
				while (num6 < 4)
				{
					span6[num6++] = 128;
				}
				span6[4] = 0;
			}
		}
		if (instance.getEntityInfo != null)
		{
			stream.WriteByte(114);
			BufferStream.RangeHandle range7 = stream.GetRange(1);
			int position7 = stream.Position;
			AppEmpty.Serialize(stream, instance.getEntityInfo);
			int num7 = stream.Position - position7;
			if (num7 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field getEntityInfo (ProtoBuf.AppEmpty)");
			}
			Span<byte> span7 = range7.GetSpan();
			ProtocolParser.WriteUInt32((uint)num7, span7, 0);
		}
		if (instance.setEntityValue != null)
		{
			stream.WriteByte(122);
			BufferStream.RangeHandle range8 = stream.GetRange(1);
			int position8 = stream.Position;
			AppSetEntityValue.Serialize(stream, instance.setEntityValue);
			int num8 = stream.Position - position8;
			if (num8 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field setEntityValue (ProtoBuf.AppSetEntityValue)");
			}
			Span<byte> span8 = range8.GetSpan();
			ProtocolParser.WriteUInt32((uint)num8, span8, 0);
		}
		if (instance.checkSubscription != null)
		{
			stream.WriteByte(130);
			stream.WriteByte(1);
			BufferStream.RangeHandle range9 = stream.GetRange(1);
			int position9 = stream.Position;
			AppEmpty.Serialize(stream, instance.checkSubscription);
			int num9 = stream.Position - position9;
			if (num9 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field checkSubscription (ProtoBuf.AppEmpty)");
			}
			Span<byte> span9 = range9.GetSpan();
			ProtocolParser.WriteUInt32((uint)num9, span9, 0);
		}
		if (instance.setSubscription != null)
		{
			stream.WriteByte(138);
			stream.WriteByte(1);
			BufferStream.RangeHandle range10 = stream.GetRange(1);
			int position10 = stream.Position;
			AppFlag.Serialize(stream, instance.setSubscription);
			int num10 = stream.Position - position10;
			if (num10 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field setSubscription (ProtoBuf.AppFlag)");
			}
			Span<byte> span10 = range10.GetSpan();
			ProtocolParser.WriteUInt32((uint)num10, span10, 0);
		}
		if (instance.getMapMarkers != null)
		{
			stream.WriteByte(146);
			stream.WriteByte(1);
			BufferStream.RangeHandle range11 = stream.GetRange(1);
			int position11 = stream.Position;
			AppEmpty.Serialize(stream, instance.getMapMarkers);
			int num11 = stream.Position - position11;
			if (num11 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field getMapMarkers (ProtoBuf.AppEmpty)");
			}
			Span<byte> span11 = range11.GetSpan();
			ProtocolParser.WriteUInt32((uint)num11, span11, 0);
		}
		if (instance.promoteToLeader != null)
		{
			stream.WriteByte(162);
			stream.WriteByte(1);
			BufferStream.RangeHandle range12 = stream.GetRange(1);
			int position12 = stream.Position;
			AppPromoteToLeader.Serialize(stream, instance.promoteToLeader);
			int num12 = stream.Position - position12;
			if (num12 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field promoteToLeader (ProtoBuf.AppPromoteToLeader)");
			}
			Span<byte> span12 = range12.GetSpan();
			ProtocolParser.WriteUInt32((uint)num12, span12, 0);
		}
		if (instance.getClanInfo != null)
		{
			stream.WriteByte(170);
			stream.WriteByte(1);
			BufferStream.RangeHandle range13 = stream.GetRange(1);
			int position13 = stream.Position;
			AppEmpty.Serialize(stream, instance.getClanInfo);
			int num13 = stream.Position - position13;
			if (num13 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field getClanInfo (ProtoBuf.AppEmpty)");
			}
			Span<byte> span13 = range13.GetSpan();
			ProtocolParser.WriteUInt32((uint)num13, span13, 0);
		}
		if (instance.setClanMotd != null)
		{
			stream.WriteByte(178);
			stream.WriteByte(1);
			BufferStream.RangeHandle range14 = stream.GetRange(5);
			int position14 = stream.Position;
			AppSendMessage.Serialize(stream, instance.setClanMotd);
			int val2 = stream.Position - position14;
			Span<byte> span14 = range14.GetSpan();
			int num14 = ProtocolParser.WriteUInt32((uint)val2, span14, 0);
			if (num14 < 5)
			{
				span14[num14 - 1] |= 128;
				while (num14 < 4)
				{
					span14[num14++] = 128;
				}
				span14[4] = 0;
			}
		}
		if (instance.getClanChat != null)
		{
			stream.WriteByte(186);
			stream.WriteByte(1);
			BufferStream.RangeHandle range15 = stream.GetRange(1);
			int position15 = stream.Position;
			AppEmpty.Serialize(stream, instance.getClanChat);
			int num15 = stream.Position - position15;
			if (num15 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field getClanChat (ProtoBuf.AppEmpty)");
			}
			Span<byte> span15 = range15.GetSpan();
			ProtocolParser.WriteUInt32((uint)num15, span15, 0);
		}
		if (instance.sendClanMessage != null)
		{
			stream.WriteByte(194);
			stream.WriteByte(1);
			BufferStream.RangeHandle range16 = stream.GetRange(5);
			int position16 = stream.Position;
			AppSendMessage.Serialize(stream, instance.sendClanMessage);
			int val3 = stream.Position - position16;
			Span<byte> span16 = range16.GetSpan();
			int num16 = ProtocolParser.WriteUInt32((uint)val3, span16, 0);
			if (num16 < 5)
			{
				span16[num16 - 1] |= 128;
				while (num16 < 4)
				{
					span16[num16++] = 128;
				}
				span16[4] = 0;
			}
		}
		if (instance.getNexusAuth != null)
		{
			stream.WriteByte(202);
			stream.WriteByte(1);
			BufferStream.RangeHandle range17 = stream.GetRange(5);
			int position17 = stream.Position;
			AppGetNexusAuth.Serialize(stream, instance.getNexusAuth);
			int val4 = stream.Position - position17;
			Span<byte> span17 = range17.GetSpan();
			int num17 = ProtocolParser.WriteUInt32((uint)val4, span17, 0);
			if (num17 < 5)
			{
				span17[num17 - 1] |= 128;
				while (num17 < 4)
				{
					span17[num17++] = 128;
				}
				span17[4] = 0;
			}
		}
		if (instance.cameraSubscribe != null)
		{
			stream.WriteByte(242);
			stream.WriteByte(1);
			BufferStream.RangeHandle range18 = stream.GetRange(5);
			int position18 = stream.Position;
			AppCameraSubscribe.Serialize(stream, instance.cameraSubscribe);
			int val5 = stream.Position - position18;
			Span<byte> span18 = range18.GetSpan();
			int num18 = ProtocolParser.WriteUInt32((uint)val5, span18, 0);
			if (num18 < 5)
			{
				span18[num18 - 1] |= 128;
				while (num18 < 4)
				{
					span18[num18++] = 128;
				}
				span18[4] = 0;
			}
		}
		if (instance.cameraUnsubscribe != null)
		{
			stream.WriteByte(250);
			stream.WriteByte(1);
			BufferStream.RangeHandle range19 = stream.GetRange(1);
			int position19 = stream.Position;
			AppEmpty.Serialize(stream, instance.cameraUnsubscribe);
			int num19 = stream.Position - position19;
			if (num19 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field cameraUnsubscribe (ProtoBuf.AppEmpty)");
			}
			Span<byte> span19 = range19.GetSpan();
			ProtocolParser.WriteUInt32((uint)num19, span19, 0);
		}
		if (instance.cameraInput != null)
		{
			stream.WriteByte(130);
			stream.WriteByte(2);
			BufferStream.RangeHandle range20 = stream.GetRange(1);
			int position20 = stream.Position;
			AppCameraInput.Serialize(stream, instance.cameraInput);
			int num20 = stream.Position - position20;
			if (num20 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field cameraInput (ProtoBuf.AppCameraInput)");
			}
			Span<byte> span20 = range20.GetSpan();
			ProtocolParser.WriteUInt32((uint)num20, span20, 0);
		}
		if (instance.kickFromTeam != null)
		{
			stream.WriteByte(138);
			stream.WriteByte(2);
			BufferStream.RangeHandle range21 = stream.GetRange(1);
			int position21 = stream.Position;
			AppTeamKick.Serialize(stream, instance.kickFromTeam);
			int num21 = stream.Position - position21;
			if (num21 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field kickFromTeam (ProtoBuf.AppTeamKick)");
			}
			Span<byte> span21 = range21.GetSpan();
			ProtocolParser.WriteUInt32((uint)num21, span21, 0);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref entityId.Value);
		getInfo?.InspectUids(action);
		getTime?.InspectUids(action);
		getMap?.InspectUids(action);
		getTeamInfo?.InspectUids(action);
		getTeamChat?.InspectUids(action);
		sendTeamMessage?.InspectUids(action);
		getEntityInfo?.InspectUids(action);
		setEntityValue?.InspectUids(action);
		checkSubscription?.InspectUids(action);
		setSubscription?.InspectUids(action);
		getMapMarkers?.InspectUids(action);
		promoteToLeader?.InspectUids(action);
		getClanInfo?.InspectUids(action);
		setClanMotd?.InspectUids(action);
		getClanChat?.InspectUids(action);
		sendClanMessage?.InspectUids(action);
		getNexusAuth?.InspectUids(action);
		cameraSubscribe?.InspectUids(action);
		cameraUnsubscribe?.InspectUids(action);
		cameraInput?.InspectUids(action);
		kickFromTeam?.InspectUids(action);
	}
}
