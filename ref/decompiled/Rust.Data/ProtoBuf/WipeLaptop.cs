using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class WipeLaptop : IDisposable, Pool.IPooled, IProto<WipeLaptop>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public int timeLeft;

	[NonSerialized]
	public float armTime;

	[NonSerialized]
	public float disarmTime;

	public static void ResetToPool(WipeLaptop instance)
	{
		if (instance.ShouldPool)
		{
			instance.timeLeft = 0;
			instance.armTime = 0f;
			instance.disarmTime = 0f;
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
			throw new Exception("Trying to dispose WipeLaptop with ShouldPool set to false!");
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

	public void CopyTo(WipeLaptop instance)
	{
		instance.timeLeft = timeLeft;
		instance.armTime = armTime;
		instance.disarmTime = disarmTime;
	}

	public WipeLaptop Copy()
	{
		WipeLaptop wipeLaptop = Pool.Get<WipeLaptop>();
		CopyTo(wipeLaptop);
		return wipeLaptop;
	}

	public static WipeLaptop Deserialize(BufferStream stream)
	{
		WipeLaptop wipeLaptop = Pool.Get<WipeLaptop>();
		Deserialize(stream, wipeLaptop, isDelta: false);
		return wipeLaptop;
	}

	public static WipeLaptop DeserializeLengthDelimited(BufferStream stream)
	{
		WipeLaptop wipeLaptop = Pool.Get<WipeLaptop>();
		DeserializeLengthDelimited(stream, wipeLaptop, isDelta: false);
		return wipeLaptop;
	}

	public static WipeLaptop DeserializeLength(BufferStream stream, int length)
	{
		WipeLaptop wipeLaptop = Pool.Get<WipeLaptop>();
		DeserializeLength(stream, length, wipeLaptop, isDelta: false);
		return wipeLaptop;
	}

	public static WipeLaptop Deserialize(byte[] buffer)
	{
		WipeLaptop wipeLaptop = Pool.Get<WipeLaptop>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, wipeLaptop, isDelta: false);
		return wipeLaptop;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, WipeLaptop previous)
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

	public static WipeLaptop Deserialize(BufferStream stream, WipeLaptop instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.WipeLaptop");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WipeLaptop");
				instance.timeLeft = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.WipeLaptop");
				instance.armTime = ProtocolParser.ReadSingle(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.WipeLaptop");
				instance.disarmTime = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WipeLaptop");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.WipeLaptop");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.WipeLaptop");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.WipeLaptop");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static WipeLaptop DeserializeLengthDelimited(BufferStream stream, WipeLaptop instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.WipeLaptop");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WipeLaptop");
				instance.timeLeft = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.WipeLaptop");
				instance.armTime = ProtocolParser.ReadSingle(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.WipeLaptop");
				instance.disarmTime = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WipeLaptop");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.WipeLaptop");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.WipeLaptop");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.WipeLaptop");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static WipeLaptop DeserializeLength(BufferStream stream, int length, WipeLaptop instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.WipeLaptop");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WipeLaptop");
				instance.timeLeft = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.WipeLaptop");
				instance.armTime = ProtocolParser.ReadSingle(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.WipeLaptop");
				instance.disarmTime = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WipeLaptop");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.WipeLaptop");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.WipeLaptop");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.WipeLaptop");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, WipeLaptop instance, WipeLaptop previous)
	{
		if (instance.timeLeft != previous.timeLeft)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.timeLeft);
		}
		if (instance.armTime != previous.armTime)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.armTime);
		}
		if (instance.disarmTime != previous.disarmTime)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.disarmTime);
		}
	}

	public static void Serialize(BufferStream stream, WipeLaptop instance)
	{
		if (instance.timeLeft != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.timeLeft);
		}
		if (instance.armTime != 0f)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.armTime);
		}
		if (instance.disarmTime != 0f)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.disarmTime);
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
