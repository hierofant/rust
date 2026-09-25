using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class AppEntityChanged : IDisposable, Pool.IPooled, IProto<AppEntityChanged>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public NetworkableId entityId;

	[NonSerialized]
	public AppEntityPayload payload;

	public static void ResetToPool(AppEntityChanged instance)
	{
		if (instance.ShouldPool)
		{
			instance.entityId = default(NetworkableId);
			if (instance.payload != null)
			{
				instance.payload.ResetToPool();
				instance.payload = null;
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
			throw new Exception("Trying to dispose AppEntityChanged with ShouldPool set to false!");
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

	public void CopyTo(AppEntityChanged instance)
	{
		instance.entityId = entityId;
		if (payload != null)
		{
			if (instance.payload == null)
			{
				instance.payload = payload.Copy();
			}
			else
			{
				payload.CopyTo(instance.payload);
			}
		}
		else
		{
			instance.payload = null;
		}
	}

	public AppEntityChanged Copy()
	{
		AppEntityChanged appEntityChanged = Pool.Get<AppEntityChanged>();
		CopyTo(appEntityChanged);
		return appEntityChanged;
	}

	public static AppEntityChanged Deserialize(BufferStream stream)
	{
		AppEntityChanged appEntityChanged = Pool.Get<AppEntityChanged>();
		Deserialize(stream, appEntityChanged, isDelta: false);
		return appEntityChanged;
	}

	public static AppEntityChanged DeserializeLengthDelimited(BufferStream stream)
	{
		AppEntityChanged appEntityChanged = Pool.Get<AppEntityChanged>();
		DeserializeLengthDelimited(stream, appEntityChanged, isDelta: false);
		return appEntityChanged;
	}

	public static AppEntityChanged DeserializeLength(BufferStream stream, int length)
	{
		AppEntityChanged appEntityChanged = Pool.Get<AppEntityChanged>();
		DeserializeLength(stream, length, appEntityChanged, isDelta: false);
		return appEntityChanged;
	}

	public static AppEntityChanged Deserialize(byte[] buffer)
	{
		AppEntityChanged appEntityChanged = Pool.Get<AppEntityChanged>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, appEntityChanged, isDelta: false);
		return appEntityChanged;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, AppEntityChanged previous)
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

	public static AppEntityChanged Deserialize(BufferStream stream, AppEntityChanged instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.AppEntityChanged");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppEntityChanged");
				instance.entityId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppEntityChanged");
				if (instance.payload == null)
				{
					instance.payload = AppEntityPayload.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppEntityPayload.DeserializeLengthDelimited(stream, instance.payload, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppEntityChanged");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppEntityChanged");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppEntityChanged");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AppEntityChanged DeserializeLengthDelimited(BufferStream stream, AppEntityChanged instance, bool isDelta)
	{
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
			stream.ConsumeFieldOperation("ProtoBuf.AppEntityChanged");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppEntityChanged");
				instance.entityId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppEntityChanged");
				if (instance.payload == null)
				{
					instance.payload = AppEntityPayload.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppEntityPayload.DeserializeLengthDelimited(stream, instance.payload, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppEntityChanged");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppEntityChanged");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppEntityChanged");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AppEntityChanged DeserializeLength(BufferStream stream, int length, AppEntityChanged instance, bool isDelta)
	{
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
			stream.ConsumeFieldOperation("ProtoBuf.AppEntityChanged");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppEntityChanged");
				instance.entityId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppEntityChanged");
				if (instance.payload == null)
				{
					instance.payload = AppEntityPayload.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppEntityPayload.DeserializeLengthDelimited(stream, instance.payload, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppEntityChanged");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppEntityChanged");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppEntityChanged");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, AppEntityChanged instance, AppEntityChanged previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, instance.entityId.Value);
		if (instance.payload == null)
		{
			throw new ArgumentNullException("payload", "Required by proto specification.");
		}
		stream.WriteByte(18);
		BufferStream.RangeHandle range = stream.GetRange(3);
		int position = stream.Position;
		AppEntityPayload.SerializeDelta(stream, instance.payload, previous.payload);
		int num = stream.Position - position;
		if (num > 2097151)
		{
			throw new InvalidOperationException("Not enough space was reserved for the length prefix of field payload (ProtoBuf.AppEntityPayload)");
		}
		Span<byte> span = range.GetSpan();
		int num2 = ProtocolParser.WriteUInt32((uint)num, span, 0);
		if (num2 < 3)
		{
			span[num2 - 1] |= 128;
			while (num2 < 2)
			{
				span[num2++] = 128;
			}
			span[2] = 0;
		}
	}

	public static void Serialize(BufferStream stream, AppEntityChanged instance)
	{
		if (instance.entityId != default(NetworkableId))
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.entityId.Value);
		}
		if (instance.payload == null)
		{
			throw new ArgumentNullException("payload", "Required by proto specification.");
		}
		stream.WriteByte(18);
		BufferStream.RangeHandle range = stream.GetRange(3);
		int position = stream.Position;
		AppEntityPayload.Serialize(stream, instance.payload);
		int num = stream.Position - position;
		if (num > 2097151)
		{
			throw new InvalidOperationException("Not enough space was reserved for the length prefix of field payload (ProtoBuf.AppEntityPayload)");
		}
		Span<byte> span = range.GetSpan();
		int num2 = ProtocolParser.WriteUInt32((uint)num, span, 0);
		if (num2 < 3)
		{
			span[num2 - 1] |= 128;
			while (num2 < 2)
			{
				span[num2++] = 128;
			}
			span[2] = 0;
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref entityId.Value);
		payload?.InspectUids(action);
	}
}
