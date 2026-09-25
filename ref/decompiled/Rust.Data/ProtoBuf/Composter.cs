using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class Composter : IDisposable, Pool.IPooled, IProto<Composter>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public float fertilizerProductionProgress;

	public static void ResetToPool(Composter instance)
	{
		if (instance.ShouldPool)
		{
			instance.fertilizerProductionProgress = 0f;
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
			throw new Exception("Trying to dispose Composter with ShouldPool set to false!");
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

	public void CopyTo(Composter instance)
	{
		instance.fertilizerProductionProgress = fertilizerProductionProgress;
	}

	public Composter Copy()
	{
		Composter composter = Pool.Get<Composter>();
		CopyTo(composter);
		return composter;
	}

	public static Composter Deserialize(BufferStream stream)
	{
		Composter composter = Pool.Get<Composter>();
		Deserialize(stream, composter, isDelta: false);
		return composter;
	}

	public static Composter DeserializeLengthDelimited(BufferStream stream)
	{
		Composter composter = Pool.Get<Composter>();
		DeserializeLengthDelimited(stream, composter, isDelta: false);
		return composter;
	}

	public static Composter DeserializeLength(BufferStream stream, int length)
	{
		Composter composter = Pool.Get<Composter>();
		DeserializeLength(stream, length, composter, isDelta: false);
		return composter;
	}

	public static Composter Deserialize(byte[] buffer)
	{
		Composter composter = Pool.Get<Composter>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, composter, isDelta: false);
		return composter;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, Composter previous)
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

	public static Composter Deserialize(BufferStream stream, Composter instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Composter");
			if (num == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Composter");
				instance.fertilizerProductionProgress = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Composter");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Composter");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static Composter DeserializeLengthDelimited(BufferStream stream, Composter instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Composter");
			if (num2 == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Composter");
				instance.fertilizerProductionProgress = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Composter");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Composter");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static Composter DeserializeLength(BufferStream stream, int length, Composter instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Composter");
			if (num2 == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Composter");
				instance.fertilizerProductionProgress = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Composter");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Composter");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, Composter instance, Composter previous)
	{
		if (instance.fertilizerProductionProgress != previous.fertilizerProductionProgress)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.fertilizerProductionProgress);
		}
	}

	public static void Serialize(BufferStream stream, Composter instance)
	{
		if (instance.fertilizerProductionProgress != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.fertilizerProductionProgress);
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
