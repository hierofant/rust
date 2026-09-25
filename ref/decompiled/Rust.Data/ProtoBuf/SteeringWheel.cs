using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class SteeringWheel : IDisposable, Pool.IPooled, IProto<SteeringWheel>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public bool hasLock;

	[NonSerialized]
	public string lockCode;

	public static void ResetToPool(SteeringWheel instance)
	{
		if (instance.ShouldPool)
		{
			instance.hasLock = false;
			instance.lockCode = string.Empty;
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
			throw new Exception("Trying to dispose SteeringWheel with ShouldPool set to false!");
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

	public void CopyTo(SteeringWheel instance)
	{
		instance.hasLock = hasLock;
		instance.lockCode = lockCode;
	}

	public SteeringWheel Copy()
	{
		SteeringWheel steeringWheel = Pool.Get<SteeringWheel>();
		CopyTo(steeringWheel);
		return steeringWheel;
	}

	public static SteeringWheel Deserialize(BufferStream stream)
	{
		SteeringWheel steeringWheel = Pool.Get<SteeringWheel>();
		Deserialize(stream, steeringWheel, isDelta: false);
		return steeringWheel;
	}

	public static SteeringWheel DeserializeLengthDelimited(BufferStream stream)
	{
		SteeringWheel steeringWheel = Pool.Get<SteeringWheel>();
		DeserializeLengthDelimited(stream, steeringWheel, isDelta: false);
		return steeringWheel;
	}

	public static SteeringWheel DeserializeLength(BufferStream stream, int length)
	{
		SteeringWheel steeringWheel = Pool.Get<SteeringWheel>();
		DeserializeLength(stream, length, steeringWheel, isDelta: false);
		return steeringWheel;
	}

	public static SteeringWheel Deserialize(byte[] buffer)
	{
		SteeringWheel steeringWheel = Pool.Get<SteeringWheel>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, steeringWheel, isDelta: false);
		return steeringWheel;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, SteeringWheel previous)
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

	public static SteeringWheel Deserialize(BufferStream stream, SteeringWheel instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.SteeringWheel");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SteeringWheel");
				instance.hasLock = ProtocolParser.ReadBool(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SteeringWheel");
				instance.lockCode = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SteeringWheel");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SteeringWheel");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SteeringWheel");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static SteeringWheel DeserializeLengthDelimited(BufferStream stream, SteeringWheel instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.SteeringWheel");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SteeringWheel");
				instance.hasLock = ProtocolParser.ReadBool(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SteeringWheel");
				instance.lockCode = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SteeringWheel");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SteeringWheel");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SteeringWheel");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static SteeringWheel DeserializeLength(BufferStream stream, int length, SteeringWheel instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.SteeringWheel");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SteeringWheel");
				instance.hasLock = ProtocolParser.ReadBool(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SteeringWheel");
				instance.lockCode = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SteeringWheel");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SteeringWheel");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SteeringWheel");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, SteeringWheel instance, SteeringWheel previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteBool(stream, instance.hasLock);
		if (instance.lockCode != null && instance.lockCode != previous.lockCode)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.lockCode);
		}
	}

	public static void Serialize(BufferStream stream, SteeringWheel instance)
	{
		if (instance.hasLock)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteBool(stream, instance.hasLock);
		}
		if (instance.lockCode != null)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.lockCode);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
	}
}
