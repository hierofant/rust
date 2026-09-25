using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class AppTeamChanged : IDisposable, Pool.IPooled, IProto<AppTeamChanged>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public ulong playerId;

	[NonSerialized]
	public AppTeamInfo teamInfo;

	public static void ResetToPool(AppTeamChanged instance)
	{
		if (instance.ShouldPool)
		{
			instance.playerId = 0uL;
			if (instance.teamInfo != null)
			{
				instance.teamInfo.ResetToPool();
				instance.teamInfo = null;
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
			throw new Exception("Trying to dispose AppTeamChanged with ShouldPool set to false!");
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

	public void CopyTo(AppTeamChanged instance)
	{
		instance.playerId = playerId;
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
	}

	public AppTeamChanged Copy()
	{
		AppTeamChanged appTeamChanged = Pool.Get<AppTeamChanged>();
		CopyTo(appTeamChanged);
		return appTeamChanged;
	}

	public static AppTeamChanged Deserialize(BufferStream stream)
	{
		AppTeamChanged appTeamChanged = Pool.Get<AppTeamChanged>();
		Deserialize(stream, appTeamChanged, isDelta: false);
		return appTeamChanged;
	}

	public static AppTeamChanged DeserializeLengthDelimited(BufferStream stream)
	{
		AppTeamChanged appTeamChanged = Pool.Get<AppTeamChanged>();
		DeserializeLengthDelimited(stream, appTeamChanged, isDelta: false);
		return appTeamChanged;
	}

	public static AppTeamChanged DeserializeLength(BufferStream stream, int length)
	{
		AppTeamChanged appTeamChanged = Pool.Get<AppTeamChanged>();
		DeserializeLength(stream, length, appTeamChanged, isDelta: false);
		return appTeamChanged;
	}

	public static AppTeamChanged Deserialize(byte[] buffer)
	{
		AppTeamChanged appTeamChanged = Pool.Get<AppTeamChanged>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, appTeamChanged, isDelta: false);
		return appTeamChanged;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, AppTeamChanged previous)
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

	public static AppTeamChanged Deserialize(BufferStream stream, AppTeamChanged instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.playerId = 0uL;
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.AppTeamChanged");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppTeamChanged");
				instance.playerId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppTeamChanged");
				if (instance.teamInfo == null)
				{
					instance.teamInfo = AppTeamInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppTeamInfo.DeserializeLengthDelimited(stream, instance.teamInfo, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppTeamChanged");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppTeamChanged");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppTeamChanged");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AppTeamChanged DeserializeLengthDelimited(BufferStream stream, AppTeamChanged instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.playerId = 0uL;
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
			stream.ConsumeFieldOperation("ProtoBuf.AppTeamChanged");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppTeamChanged");
				instance.playerId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppTeamChanged");
				if (instance.teamInfo == null)
				{
					instance.teamInfo = AppTeamInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppTeamInfo.DeserializeLengthDelimited(stream, instance.teamInfo, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppTeamChanged");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppTeamChanged");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppTeamChanged");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AppTeamChanged DeserializeLength(BufferStream stream, int length, AppTeamChanged instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.playerId = 0uL;
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
			stream.ConsumeFieldOperation("ProtoBuf.AppTeamChanged");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppTeamChanged");
				instance.playerId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppTeamChanged");
				if (instance.teamInfo == null)
				{
					instance.teamInfo = AppTeamInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppTeamInfo.DeserializeLengthDelimited(stream, instance.teamInfo, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppTeamChanged");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppTeamChanged");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppTeamChanged");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, AppTeamChanged instance, AppTeamChanged previous)
	{
		if (instance.playerId != previous.playerId)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.playerId);
		}
		if (instance.teamInfo == null)
		{
			throw new ArgumentNullException("teamInfo", "Required by proto specification.");
		}
		stream.WriteByte(18);
		BufferStream.RangeHandle range = stream.GetRange(5);
		int position = stream.Position;
		AppTeamInfo.SerializeDelta(stream, instance.teamInfo, previous.teamInfo);
		int val = stream.Position - position;
		Span<byte> span = range.GetSpan();
		int num = ProtocolParser.WriteUInt32((uint)val, span, 0);
		if (num < 5)
		{
			span[num - 1] |= 128;
			while (num < 4)
			{
				span[num++] = 128;
			}
			span[4] = 0;
		}
	}

	public static void Serialize(BufferStream stream, AppTeamChanged instance)
	{
		if (instance.playerId != 0L)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.playerId);
		}
		if (instance.teamInfo == null)
		{
			throw new ArgumentNullException("teamInfo", "Required by proto specification.");
		}
		stream.WriteByte(18);
		BufferStream.RangeHandle range = stream.GetRange(5);
		int position = stream.Position;
		AppTeamInfo.Serialize(stream, instance.teamInfo);
		int val = stream.Position - position;
		Span<byte> span = range.GetSpan();
		int num = ProtocolParser.WriteUInt32((uint)val, span, 0);
		if (num < 5)
		{
			span[num - 1] |= 128;
			while (num < 4)
			{
				span[num++] = 128;
			}
			span[4] = 0;
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		teamInfo?.InspectUids(action);
	}
}
