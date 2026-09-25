using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class AppResponse : IDisposable, Pool.IPooled, IProto<AppResponse>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public uint seq;

	[NonSerialized]
	public AppSuccess success;

	[NonSerialized]
	public AppError error;

	[NonSerialized]
	public AppInfo info;

	[NonSerialized]
	public AppTime time;

	[NonSerialized]
	public AppMap map;

	[NonSerialized]
	public AppTeamInfo teamInfo;

	[NonSerialized]
	public AppTeamChat teamChat;

	[NonSerialized]
	public AppEntityInfo entityInfo;

	[NonSerialized]
	public AppFlag flag;

	[NonSerialized]
	public AppMapMarkers mapMarkers;

	[NonSerialized]
	public AppClanInfo clanInfo;

	[NonSerialized]
	public AppClanChat clanChat;

	[NonSerialized]
	public AppNexusAuth nexusAuth;

	[NonSerialized]
	public AppCameraInfo cameraSubscribeInfo;

	public static void ResetToPool(AppResponse instance)
	{
		if (instance.ShouldPool)
		{
			instance.seq = 0u;
			if (instance.success != null)
			{
				instance.success.ResetToPool();
				instance.success = null;
			}
			if (instance.error != null)
			{
				instance.error.ResetToPool();
				instance.error = null;
			}
			if (instance.info != null)
			{
				instance.info.ResetToPool();
				instance.info = null;
			}
			if (instance.time != null)
			{
				instance.time.ResetToPool();
				instance.time = null;
			}
			if (instance.map != null)
			{
				instance.map.ResetToPool();
				instance.map = null;
			}
			if (instance.teamInfo != null)
			{
				instance.teamInfo.ResetToPool();
				instance.teamInfo = null;
			}
			if (instance.teamChat != null)
			{
				instance.teamChat.ResetToPool();
				instance.teamChat = null;
			}
			if (instance.entityInfo != null)
			{
				instance.entityInfo.ResetToPool();
				instance.entityInfo = null;
			}
			if (instance.flag != null)
			{
				instance.flag.ResetToPool();
				instance.flag = null;
			}
			if (instance.mapMarkers != null)
			{
				instance.mapMarkers.ResetToPool();
				instance.mapMarkers = null;
			}
			if (instance.clanInfo != null)
			{
				instance.clanInfo.ResetToPool();
				instance.clanInfo = null;
			}
			if (instance.clanChat != null)
			{
				instance.clanChat.ResetToPool();
				instance.clanChat = null;
			}
			if (instance.nexusAuth != null)
			{
				instance.nexusAuth.ResetToPool();
				instance.nexusAuth = null;
			}
			if (instance.cameraSubscribeInfo != null)
			{
				instance.cameraSubscribeInfo.ResetToPool();
				instance.cameraSubscribeInfo = null;
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
			throw new Exception("Trying to dispose AppResponse with ShouldPool set to false!");
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

	public void CopyTo(AppResponse instance)
	{
		instance.seq = seq;
		if (success != null)
		{
			if (instance.success == null)
			{
				instance.success = success.Copy();
			}
			else
			{
				success.CopyTo(instance.success);
			}
		}
		else
		{
			instance.success = null;
		}
		if (error != null)
		{
			if (instance.error == null)
			{
				instance.error = error.Copy();
			}
			else
			{
				error.CopyTo(instance.error);
			}
		}
		else
		{
			instance.error = null;
		}
		if (info != null)
		{
			if (instance.info == null)
			{
				instance.info = info.Copy();
			}
			else
			{
				info.CopyTo(instance.info);
			}
		}
		else
		{
			instance.info = null;
		}
		if (time != null)
		{
			if (instance.time == null)
			{
				instance.time = time.Copy();
			}
			else
			{
				time.CopyTo(instance.time);
			}
		}
		else
		{
			instance.time = null;
		}
		if (map != null)
		{
			if (instance.map == null)
			{
				instance.map = map.Copy();
			}
			else
			{
				map.CopyTo(instance.map);
			}
		}
		else
		{
			instance.map = null;
		}
		if (teamInfo != null)
		{
			if (instance.teamInfo == null)
			{
				instance.teamInfo = teamInfo.Copy();
			}
			else
			{
				teamInfo.CopyTo(instance.teamInfo);
			}
		}
		else
		{
			instance.teamInfo = null;
		}
		if (teamChat != null)
		{
			if (instance.teamChat == null)
			{
				instance.teamChat = teamChat.Copy();
			}
			else
			{
				teamChat.CopyTo(instance.teamChat);
			}
		}
		else
		{
			instance.teamChat = null;
		}
		if (entityInfo != null)
		{
			if (instance.entityInfo == null)
			{
				instance.entityInfo = entityInfo.Copy();
			}
			else
			{
				entityInfo.CopyTo(instance.entityInfo);
			}
		}
		else
		{
			instance.entityInfo = null;
		}
		if (flag != null)
		{
			if (instance.flag == null)
			{
				instance.flag = flag.Copy();
			}
			else
			{
				flag.CopyTo(instance.flag);
			}
		}
		else
		{
			instance.flag = null;
		}
		if (mapMarkers != null)
		{
			if (instance.mapMarkers == null)
			{
				instance.mapMarkers = mapMarkers.Copy();
			}
			else
			{
				mapMarkers.CopyTo(instance.mapMarkers);
			}
		}
		else
		{
			instance.mapMarkers = null;
		}
		if (clanInfo != null)
		{
			if (instance.clanInfo == null)
			{
				instance.clanInfo = clanInfo.Copy();
			}
			else
			{
				clanInfo.CopyTo(instance.clanInfo);
			}
		}
		else
		{
			instance.clanInfo = null;
		}
		if (clanChat != null)
		{
			if (instance.clanChat == null)
			{
				instance.clanChat = clanChat.Copy();
			}
			else
			{
				clanChat.CopyTo(instance.clanChat);
			}
		}
		else
		{
			instance.clanChat = null;
		}
		if (nexusAuth != null)
		{
			if (instance.nexusAuth == null)
			{
				instance.nexusAuth = nexusAuth.Copy();
			}
			else
			{
				nexusAuth.CopyTo(instance.nexusAuth);
			}
		}
		else
		{
			instance.nexusAuth = null;
		}
		if (cameraSubscribeInfo != null)
		{
			if (instance.cameraSubscribeInfo == null)
			{
				instance.cameraSubscribeInfo = cameraSubscribeInfo.Copy();
			}
			else
			{
				cameraSubscribeInfo.CopyTo(instance.cameraSubscribeInfo);
			}
		}
		else
		{
			instance.cameraSubscribeInfo = null;
		}
	}

	public AppResponse Copy()
	{
		AppResponse appResponse = Pool.Get<AppResponse>();
		CopyTo(appResponse);
		return appResponse;
	}

	public static AppResponse Deserialize(BufferStream stream)
	{
		AppResponse appResponse = Pool.Get<AppResponse>();
		Deserialize(stream, appResponse, isDelta: false);
		return appResponse;
	}

	public static AppResponse DeserializeLengthDelimited(BufferStream stream)
	{
		AppResponse appResponse = Pool.Get<AppResponse>();
		DeserializeLengthDelimited(stream, appResponse, isDelta: false);
		return appResponse;
	}

	public static AppResponse DeserializeLength(BufferStream stream, int length)
	{
		AppResponse appResponse = Pool.Get<AppResponse>();
		DeserializeLength(stream, length, appResponse, isDelta: false);
		return appResponse;
	}

	public static AppResponse Deserialize(byte[] buffer)
	{
		AppResponse appResponse = Pool.Get<AppResponse>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, appResponse, isDelta: false);
		return appResponse;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, AppResponse previous)
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

	public static AppResponse Deserialize(BufferStream stream, AppResponse instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.seq = 0u;
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.AppResponse");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				instance.seq = ProtocolParser.ReadUInt32(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.success == null)
				{
					instance.success = AppSuccess.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppSuccess.DeserializeLengthDelimited(stream, instance.success, isDelta);
				}
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.error == null)
				{
					instance.error = AppError.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppError.DeserializeLengthDelimited(stream, instance.error, isDelta);
				}
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.info == null)
				{
					instance.info = AppInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppInfo.DeserializeLengthDelimited(stream, instance.info, isDelta);
				}
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.time == null)
				{
					instance.time = AppTime.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppTime.DeserializeLengthDelimited(stream, instance.time, isDelta);
				}
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.map == null)
				{
					instance.map = AppMap.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppMap.DeserializeLengthDelimited(stream, instance.map, isDelta);
				}
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.teamInfo == null)
				{
					instance.teamInfo = AppTeamInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppTeamInfo.DeserializeLengthDelimited(stream, instance.teamInfo, isDelta);
				}
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.teamChat == null)
				{
					instance.teamChat = AppTeamChat.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppTeamChat.DeserializeLengthDelimited(stream, instance.teamChat, isDelta);
				}
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.entityInfo == null)
				{
					instance.entityInfo = AppEntityInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppEntityInfo.DeserializeLengthDelimited(stream, instance.entityInfo, isDelta);
				}
				continue;
			case 98:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.flag == null)
				{
					instance.flag = AppFlag.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppFlag.DeserializeLengthDelimited(stream, instance.flag, isDelta);
				}
				continue;
			case 106:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.mapMarkers == null)
				{
					instance.mapMarkers = AppMapMarkers.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppMapMarkers.DeserializeLengthDelimited(stream, instance.mapMarkers, isDelta);
				}
				continue;
			case 122:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.clanInfo == null)
				{
					instance.clanInfo = AppClanInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppClanInfo.DeserializeLengthDelimited(stream, instance.clanInfo, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.clanChat == null)
					{
						instance.clanChat = AppClanChat.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppClanChat.DeserializeLengthDelimited(stream, instance.clanChat, isDelta);
					}
				}
				break;
			case 17u:
				stream.ValidateFieldOrder(ref lastFieldId, 17u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.nexusAuth == null)
					{
						instance.nexusAuth = AppNexusAuth.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppNexusAuth.DeserializeLengthDelimited(stream, instance.nexusAuth, isDelta);
					}
				}
				break;
			case 20u:
				stream.ValidateFieldOrder(ref lastFieldId, 20u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.cameraSubscribeInfo == null)
					{
						instance.cameraSubscribeInfo = AppCameraInfo.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppCameraInfo.DeserializeLengthDelimited(stream, instance.cameraSubscribeInfo, isDelta);
					}
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AppResponse DeserializeLengthDelimited(BufferStream stream, AppResponse instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.seq = 0u;
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
			stream.ConsumeFieldOperation("ProtoBuf.AppResponse");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				instance.seq = ProtocolParser.ReadUInt32(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.success == null)
				{
					instance.success = AppSuccess.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppSuccess.DeserializeLengthDelimited(stream, instance.success, isDelta);
				}
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.error == null)
				{
					instance.error = AppError.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppError.DeserializeLengthDelimited(stream, instance.error, isDelta);
				}
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.info == null)
				{
					instance.info = AppInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppInfo.DeserializeLengthDelimited(stream, instance.info, isDelta);
				}
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.time == null)
				{
					instance.time = AppTime.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppTime.DeserializeLengthDelimited(stream, instance.time, isDelta);
				}
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.map == null)
				{
					instance.map = AppMap.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppMap.DeserializeLengthDelimited(stream, instance.map, isDelta);
				}
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.teamInfo == null)
				{
					instance.teamInfo = AppTeamInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppTeamInfo.DeserializeLengthDelimited(stream, instance.teamInfo, isDelta);
				}
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.teamChat == null)
				{
					instance.teamChat = AppTeamChat.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppTeamChat.DeserializeLengthDelimited(stream, instance.teamChat, isDelta);
				}
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.entityInfo == null)
				{
					instance.entityInfo = AppEntityInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppEntityInfo.DeserializeLengthDelimited(stream, instance.entityInfo, isDelta);
				}
				continue;
			case 98:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.flag == null)
				{
					instance.flag = AppFlag.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppFlag.DeserializeLengthDelimited(stream, instance.flag, isDelta);
				}
				continue;
			case 106:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.mapMarkers == null)
				{
					instance.mapMarkers = AppMapMarkers.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppMapMarkers.DeserializeLengthDelimited(stream, instance.mapMarkers, isDelta);
				}
				continue;
			case 122:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.clanInfo == null)
				{
					instance.clanInfo = AppClanInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppClanInfo.DeserializeLengthDelimited(stream, instance.clanInfo, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.clanChat == null)
					{
						instance.clanChat = AppClanChat.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppClanChat.DeserializeLengthDelimited(stream, instance.clanChat, isDelta);
					}
				}
				break;
			case 17u:
				stream.ValidateFieldOrder(ref lastFieldId, 17u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.nexusAuth == null)
					{
						instance.nexusAuth = AppNexusAuth.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppNexusAuth.DeserializeLengthDelimited(stream, instance.nexusAuth, isDelta);
					}
				}
				break;
			case 20u:
				stream.ValidateFieldOrder(ref lastFieldId, 20u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.cameraSubscribeInfo == null)
					{
						instance.cameraSubscribeInfo = AppCameraInfo.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppCameraInfo.DeserializeLengthDelimited(stream, instance.cameraSubscribeInfo, isDelta);
					}
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AppResponse DeserializeLength(BufferStream stream, int length, AppResponse instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.seq = 0u;
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
			stream.ConsumeFieldOperation("ProtoBuf.AppResponse");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				instance.seq = ProtocolParser.ReadUInt32(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.success == null)
				{
					instance.success = AppSuccess.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppSuccess.DeserializeLengthDelimited(stream, instance.success, isDelta);
				}
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.error == null)
				{
					instance.error = AppError.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppError.DeserializeLengthDelimited(stream, instance.error, isDelta);
				}
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.info == null)
				{
					instance.info = AppInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppInfo.DeserializeLengthDelimited(stream, instance.info, isDelta);
				}
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.time == null)
				{
					instance.time = AppTime.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppTime.DeserializeLengthDelimited(stream, instance.time, isDelta);
				}
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.map == null)
				{
					instance.map = AppMap.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppMap.DeserializeLengthDelimited(stream, instance.map, isDelta);
				}
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.teamInfo == null)
				{
					instance.teamInfo = AppTeamInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppTeamInfo.DeserializeLengthDelimited(stream, instance.teamInfo, isDelta);
				}
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.teamChat == null)
				{
					instance.teamChat = AppTeamChat.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppTeamChat.DeserializeLengthDelimited(stream, instance.teamChat, isDelta);
				}
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.entityInfo == null)
				{
					instance.entityInfo = AppEntityInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppEntityInfo.DeserializeLengthDelimited(stream, instance.entityInfo, isDelta);
				}
				continue;
			case 98:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.flag == null)
				{
					instance.flag = AppFlag.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppFlag.DeserializeLengthDelimited(stream, instance.flag, isDelta);
				}
				continue;
			case 106:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.mapMarkers == null)
				{
					instance.mapMarkers = AppMapMarkers.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppMapMarkers.DeserializeLengthDelimited(stream, instance.mapMarkers, isDelta);
				}
				continue;
			case 122:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (instance.clanInfo == null)
				{
					instance.clanInfo = AppClanInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppClanInfo.DeserializeLengthDelimited(stream, instance.clanInfo, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.clanChat == null)
					{
						instance.clanChat = AppClanChat.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppClanChat.DeserializeLengthDelimited(stream, instance.clanChat, isDelta);
					}
				}
				break;
			case 17u:
				stream.ValidateFieldOrder(ref lastFieldId, 17u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.nexusAuth == null)
					{
						instance.nexusAuth = AppNexusAuth.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppNexusAuth.DeserializeLengthDelimited(stream, instance.nexusAuth, isDelta);
					}
				}
				break;
			case 20u:
				stream.ValidateFieldOrder(ref lastFieldId, 20u, fieldIsRepeated: false, "ProtoBuf.AppResponse");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.cameraSubscribeInfo == null)
					{
						instance.cameraSubscribeInfo = AppCameraInfo.DeserializeLengthDelimited(stream);
					}
					else
					{
						AppCameraInfo.DeserializeLengthDelimited(stream, instance.cameraSubscribeInfo, isDelta);
					}
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppResponse");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, AppResponse instance, AppResponse previous)
	{
		if (instance.seq != previous.seq)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt32(stream, instance.seq);
		}
		if (instance.success != null)
		{
			stream.WriteByte(34);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			AppSuccess.SerializeDelta(stream, instance.success, previous.success);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field success (ProtoBuf.AppSuccess)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.error != null)
		{
			stream.WriteByte(42);
			BufferStream.RangeHandle range2 = stream.GetRange(5);
			int position2 = stream.Position;
			AppError.SerializeDelta(stream, instance.error, previous.error);
			int val = stream.Position - position2;
			Span<byte> span2 = range2.GetSpan();
			int num2 = ProtocolParser.WriteUInt32((uint)val, span2, 0);
			if (num2 < 5)
			{
				span2[num2 - 1] |= 128;
				while (num2 < 4)
				{
					span2[num2++] = 128;
				}
				span2[4] = 0;
			}
		}
		if (instance.info != null)
		{
			stream.WriteByte(50);
			BufferStream.RangeHandle range3 = stream.GetRange(5);
			int position3 = stream.Position;
			AppInfo.SerializeDelta(stream, instance.info, previous.info);
			int val2 = stream.Position - position3;
			Span<byte> span3 = range3.GetSpan();
			int num3 = ProtocolParser.WriteUInt32((uint)val2, span3, 0);
			if (num3 < 5)
			{
				span3[num3 - 1] |= 128;
				while (num3 < 4)
				{
					span3[num3++] = 128;
				}
				span3[4] = 0;
			}
		}
		if (instance.time != null)
		{
			stream.WriteByte(58);
			BufferStream.RangeHandle range4 = stream.GetRange(1);
			int position4 = stream.Position;
			AppTime.SerializeDelta(stream, instance.time, previous.time);
			int num4 = stream.Position - position4;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field time (ProtoBuf.AppTime)");
			}
			Span<byte> span4 = range4.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span4, 0);
		}
		if (instance.map != null)
		{
			stream.WriteByte(66);
			BufferStream.RangeHandle range5 = stream.GetRange(5);
			int position5 = stream.Position;
			AppMap.SerializeDelta(stream, instance.map, previous.map);
			int val3 = stream.Position - position5;
			Span<byte> span5 = range5.GetSpan();
			int num5 = ProtocolParser.WriteUInt32((uint)val3, span5, 0);
			if (num5 < 5)
			{
				span5[num5 - 1] |= 128;
				while (num5 < 4)
				{
					span5[num5++] = 128;
				}
				span5[4] = 0;
			}
		}
		if (instance.teamInfo != null)
		{
			stream.WriteByte(74);
			BufferStream.RangeHandle range6 = stream.GetRange(5);
			int position6 = stream.Position;
			AppTeamInfo.SerializeDelta(stream, instance.teamInfo, previous.teamInfo);
			int val4 = stream.Position - position6;
			Span<byte> span6 = range6.GetSpan();
			int num6 = ProtocolParser.WriteUInt32((uint)val4, span6, 0);
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
		if (instance.teamChat != null)
		{
			stream.WriteByte(82);
			BufferStream.RangeHandle range7 = stream.GetRange(5);
			int position7 = stream.Position;
			AppTeamChat.SerializeDelta(stream, instance.teamChat, previous.teamChat);
			int val5 = stream.Position - position7;
			Span<byte> span7 = range7.GetSpan();
			int num7 = ProtocolParser.WriteUInt32((uint)val5, span7, 0);
			if (num7 < 5)
			{
				span7[num7 - 1] |= 128;
				while (num7 < 4)
				{
					span7[num7++] = 128;
				}
				span7[4] = 0;
			}
		}
		if (instance.entityInfo != null)
		{
			stream.WriteByte(90);
			BufferStream.RangeHandle range8 = stream.GetRange(3);
			int position8 = stream.Position;
			AppEntityInfo.SerializeDelta(stream, instance.entityInfo, previous.entityInfo);
			int num8 = stream.Position - position8;
			if (num8 > 2097151)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field entityInfo (ProtoBuf.AppEntityInfo)");
			}
			Span<byte> span8 = range8.GetSpan();
			int num9 = ProtocolParser.WriteUInt32((uint)num8, span8, 0);
			if (num9 < 3)
			{
				span8[num9 - 1] |= 128;
				while (num9 < 2)
				{
					span8[num9++] = 128;
				}
				span8[2] = 0;
			}
		}
		if (instance.flag != null)
		{
			stream.WriteByte(98);
			BufferStream.RangeHandle range9 = stream.GetRange(1);
			int position9 = stream.Position;
			AppFlag.SerializeDelta(stream, instance.flag, previous.flag);
			int num10 = stream.Position - position9;
			if (num10 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field flag (ProtoBuf.AppFlag)");
			}
			Span<byte> span9 = range9.GetSpan();
			ProtocolParser.WriteUInt32((uint)num10, span9, 0);
		}
		if (instance.mapMarkers != null)
		{
			stream.WriteByte(106);
			BufferStream.RangeHandle range10 = stream.GetRange(5);
			int position10 = stream.Position;
			AppMapMarkers.SerializeDelta(stream, instance.mapMarkers, previous.mapMarkers);
			int val6 = stream.Position - position10;
			Span<byte> span10 = range10.GetSpan();
			int num11 = ProtocolParser.WriteUInt32((uint)val6, span10, 0);
			if (num11 < 5)
			{
				span10[num11 - 1] |= 128;
				while (num11 < 4)
				{
					span10[num11++] = 128;
				}
				span10[4] = 0;
			}
		}
		if (instance.clanInfo != null)
		{
			stream.WriteByte(122);
			BufferStream.RangeHandle range11 = stream.GetRange(5);
			int position11 = stream.Position;
			AppClanInfo.SerializeDelta(stream, instance.clanInfo, previous.clanInfo);
			int val7 = stream.Position - position11;
			Span<byte> span11 = range11.GetSpan();
			int num12 = ProtocolParser.WriteUInt32((uint)val7, span11, 0);
			if (num12 < 5)
			{
				span11[num12 - 1] |= 128;
				while (num12 < 4)
				{
					span11[num12++] = 128;
				}
				span11[4] = 0;
			}
		}
		if (instance.clanChat != null)
		{
			stream.WriteByte(130);
			stream.WriteByte(1);
			BufferStream.RangeHandle range12 = stream.GetRange(5);
			int position12 = stream.Position;
			AppClanChat.SerializeDelta(stream, instance.clanChat, previous.clanChat);
			int val8 = stream.Position - position12;
			Span<byte> span12 = range12.GetSpan();
			int num13 = ProtocolParser.WriteUInt32((uint)val8, span12, 0);
			if (num13 < 5)
			{
				span12[num13 - 1] |= 128;
				while (num13 < 4)
				{
					span12[num13++] = 128;
				}
				span12[4] = 0;
			}
		}
		if (instance.nexusAuth != null)
		{
			stream.WriteByte(138);
			stream.WriteByte(1);
			BufferStream.RangeHandle range13 = stream.GetRange(5);
			int position13 = stream.Position;
			AppNexusAuth.SerializeDelta(stream, instance.nexusAuth, previous.nexusAuth);
			int val9 = stream.Position - position13;
			Span<byte> span13 = range13.GetSpan();
			int num14 = ProtocolParser.WriteUInt32((uint)val9, span13, 0);
			if (num14 < 5)
			{
				span13[num14 - 1] |= 128;
				while (num14 < 4)
				{
					span13[num14++] = 128;
				}
				span13[4] = 0;
			}
		}
		if (instance.cameraSubscribeInfo != null)
		{
			stream.WriteByte(162);
			stream.WriteByte(1);
			BufferStream.RangeHandle range14 = stream.GetRange(1);
			int position14 = stream.Position;
			AppCameraInfo.SerializeDelta(stream, instance.cameraSubscribeInfo, previous.cameraSubscribeInfo);
			int num15 = stream.Position - position14;
			if (num15 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field cameraSubscribeInfo (ProtoBuf.AppCameraInfo)");
			}
			Span<byte> span14 = range14.GetSpan();
			ProtocolParser.WriteUInt32((uint)num15, span14, 0);
		}
	}

	public static void Serialize(BufferStream stream, AppResponse instance)
	{
		if (instance.seq != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt32(stream, instance.seq);
		}
		if (instance.success != null)
		{
			stream.WriteByte(34);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			AppSuccess.Serialize(stream, instance.success);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field success (ProtoBuf.AppSuccess)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.error != null)
		{
			stream.WriteByte(42);
			BufferStream.RangeHandle range2 = stream.GetRange(5);
			int position2 = stream.Position;
			AppError.Serialize(stream, instance.error);
			int val = stream.Position - position2;
			Span<byte> span2 = range2.GetSpan();
			int num2 = ProtocolParser.WriteUInt32((uint)val, span2, 0);
			if (num2 < 5)
			{
				span2[num2 - 1] |= 128;
				while (num2 < 4)
				{
					span2[num2++] = 128;
				}
				span2[4] = 0;
			}
		}
		if (instance.info != null)
		{
			stream.WriteByte(50);
			BufferStream.RangeHandle range3 = stream.GetRange(5);
			int position3 = stream.Position;
			AppInfo.Serialize(stream, instance.info);
			int val2 = stream.Position - position3;
			Span<byte> span3 = range3.GetSpan();
			int num3 = ProtocolParser.WriteUInt32((uint)val2, span3, 0);
			if (num3 < 5)
			{
				span3[num3 - 1] |= 128;
				while (num3 < 4)
				{
					span3[num3++] = 128;
				}
				span3[4] = 0;
			}
		}
		if (instance.time != null)
		{
			stream.WriteByte(58);
			BufferStream.RangeHandle range4 = stream.GetRange(1);
			int position4 = stream.Position;
			AppTime.Serialize(stream, instance.time);
			int num4 = stream.Position - position4;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field time (ProtoBuf.AppTime)");
			}
			Span<byte> span4 = range4.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span4, 0);
		}
		if (instance.map != null)
		{
			stream.WriteByte(66);
			BufferStream.RangeHandle range5 = stream.GetRange(5);
			int position5 = stream.Position;
			AppMap.Serialize(stream, instance.map);
			int val3 = stream.Position - position5;
			Span<byte> span5 = range5.GetSpan();
			int num5 = ProtocolParser.WriteUInt32((uint)val3, span5, 0);
			if (num5 < 5)
			{
				span5[num5 - 1] |= 128;
				while (num5 < 4)
				{
					span5[num5++] = 128;
				}
				span5[4] = 0;
			}
		}
		if (instance.teamInfo != null)
		{
			stream.WriteByte(74);
			BufferStream.RangeHandle range6 = stream.GetRange(5);
			int position6 = stream.Position;
			AppTeamInfo.Serialize(stream, instance.teamInfo);
			int val4 = stream.Position - position6;
			Span<byte> span6 = range6.GetSpan();
			int num6 = ProtocolParser.WriteUInt32((uint)val4, span6, 0);
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
		if (instance.teamChat != null)
		{
			stream.WriteByte(82);
			BufferStream.RangeHandle range7 = stream.GetRange(5);
			int position7 = stream.Position;
			AppTeamChat.Serialize(stream, instance.teamChat);
			int val5 = stream.Position - position7;
			Span<byte> span7 = range7.GetSpan();
			int num7 = ProtocolParser.WriteUInt32((uint)val5, span7, 0);
			if (num7 < 5)
			{
				span7[num7 - 1] |= 128;
				while (num7 < 4)
				{
					span7[num7++] = 128;
				}
				span7[4] = 0;
			}
		}
		if (instance.entityInfo != null)
		{
			stream.WriteByte(90);
			BufferStream.RangeHandle range8 = stream.GetRange(3);
			int position8 = stream.Position;
			AppEntityInfo.Serialize(stream, instance.entityInfo);
			int num8 = stream.Position - position8;
			if (num8 > 2097151)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field entityInfo (ProtoBuf.AppEntityInfo)");
			}
			Span<byte> span8 = range8.GetSpan();
			int num9 = ProtocolParser.WriteUInt32((uint)num8, span8, 0);
			if (num9 < 3)
			{
				span8[num9 - 1] |= 128;
				while (num9 < 2)
				{
					span8[num9++] = 128;
				}
				span8[2] = 0;
			}
		}
		if (instance.flag != null)
		{
			stream.WriteByte(98);
			BufferStream.RangeHandle range9 = stream.GetRange(1);
			int position9 = stream.Position;
			AppFlag.Serialize(stream, instance.flag);
			int num10 = stream.Position - position9;
			if (num10 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field flag (ProtoBuf.AppFlag)");
			}
			Span<byte> span9 = range9.GetSpan();
			ProtocolParser.WriteUInt32((uint)num10, span9, 0);
		}
		if (instance.mapMarkers != null)
		{
			stream.WriteByte(106);
			BufferStream.RangeHandle range10 = stream.GetRange(5);
			int position10 = stream.Position;
			AppMapMarkers.Serialize(stream, instance.mapMarkers);
			int val6 = stream.Position - position10;
			Span<byte> span10 = range10.GetSpan();
			int num11 = ProtocolParser.WriteUInt32((uint)val6, span10, 0);
			if (num11 < 5)
			{
				span10[num11 - 1] |= 128;
				while (num11 < 4)
				{
					span10[num11++] = 128;
				}
				span10[4] = 0;
			}
		}
		if (instance.clanInfo != null)
		{
			stream.WriteByte(122);
			BufferStream.RangeHandle range11 = stream.GetRange(5);
			int position11 = stream.Position;
			AppClanInfo.Serialize(stream, instance.clanInfo);
			int val7 = stream.Position - position11;
			Span<byte> span11 = range11.GetSpan();
			int num12 = ProtocolParser.WriteUInt32((uint)val7, span11, 0);
			if (num12 < 5)
			{
				span11[num12 - 1] |= 128;
				while (num12 < 4)
				{
					span11[num12++] = 128;
				}
				span11[4] = 0;
			}
		}
		if (instance.clanChat != null)
		{
			stream.WriteByte(130);
			stream.WriteByte(1);
			BufferStream.RangeHandle range12 = stream.GetRange(5);
			int position12 = stream.Position;
			AppClanChat.Serialize(stream, instance.clanChat);
			int val8 = stream.Position - position12;
			Span<byte> span12 = range12.GetSpan();
			int num13 = ProtocolParser.WriteUInt32((uint)val8, span12, 0);
			if (num13 < 5)
			{
				span12[num13 - 1] |= 128;
				while (num13 < 4)
				{
					span12[num13++] = 128;
				}
				span12[4] = 0;
			}
		}
		if (instance.nexusAuth != null)
		{
			stream.WriteByte(138);
			stream.WriteByte(1);
			BufferStream.RangeHandle range13 = stream.GetRange(5);
			int position13 = stream.Position;
			AppNexusAuth.Serialize(stream, instance.nexusAuth);
			int val9 = stream.Position - position13;
			Span<byte> span13 = range13.GetSpan();
			int num14 = ProtocolParser.WriteUInt32((uint)val9, span13, 0);
			if (num14 < 5)
			{
				span13[num14 - 1] |= 128;
				while (num14 < 4)
				{
					span13[num14++] = 128;
				}
				span13[4] = 0;
			}
		}
		if (instance.cameraSubscribeInfo != null)
		{
			stream.WriteByte(162);
			stream.WriteByte(1);
			BufferStream.RangeHandle range14 = stream.GetRange(1);
			int position14 = stream.Position;
			AppCameraInfo.Serialize(stream, instance.cameraSubscribeInfo);
			int num15 = stream.Position - position14;
			if (num15 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field cameraSubscribeInfo (ProtoBuf.AppCameraInfo)");
			}
			Span<byte> span14 = range14.GetSpan();
			ProtocolParser.WriteUInt32((uint)num15, span14, 0);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		success?.InspectUids(action);
		error?.InspectUids(action);
		info?.InspectUids(action);
		time?.InspectUids(action);
		map?.InspectUids(action);
		teamInfo?.InspectUids(action);
		teamChat?.InspectUids(action);
		entityInfo?.InspectUids(action);
		flag?.InspectUids(action);
		mapMarkers?.InspectUids(action);
		clanInfo?.InspectUids(action);
		clanChat?.InspectUids(action);
		nexusAuth?.InspectUids(action);
		cameraSubscribeInfo?.InspectUids(action);
	}
}
