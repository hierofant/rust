using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class BeeMasterSwarm : IDisposable, Pool.IPooled, IProto<BeeMasterSwarm>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public float population;

	public static void ResetToPool(BeeMasterSwarm instance)
	{
		if (instance.ShouldPool)
		{
			instance.population = 0f;
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
			throw new Exception("Trying to dispose BeeMasterSwarm with ShouldPool set to false!");
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

	public void CopyTo(BeeMasterSwarm instance)
	{
		instance.population = population;
	}

	public BeeMasterSwarm Copy()
	{
		BeeMasterSwarm beeMasterSwarm = Pool.Get<BeeMasterSwarm>();
		CopyTo(beeMasterSwarm);
		return beeMasterSwarm;
	}

	public static BeeMasterSwarm Deserialize(BufferStream stream)
	{
		BeeMasterSwarm beeMasterSwarm = Pool.Get<BeeMasterSwarm>();
		Deserialize(stream, beeMasterSwarm, isDelta: false);
		return beeMasterSwarm;
	}

	public static BeeMasterSwarm DeserializeLengthDelimited(BufferStream stream)
	{
		BeeMasterSwarm beeMasterSwarm = Pool.Get<BeeMasterSwarm>();
		DeserializeLengthDelimited(stream, beeMasterSwarm, isDelta: false);
		return beeMasterSwarm;
	}

	public static BeeMasterSwarm DeserializeLength(BufferStream stream, int length)
	{
		BeeMasterSwarm beeMasterSwarm = Pool.Get<BeeMasterSwarm>();
		DeserializeLength(stream, length, beeMasterSwarm, isDelta: false);
		return beeMasterSwarm;
	}

	public static BeeMasterSwarm Deserialize(byte[] buffer)
	{
		BeeMasterSwarm beeMasterSwarm = Pool.Get<BeeMasterSwarm>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, beeMasterSwarm, isDelta: false);
		return beeMasterSwarm;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, BeeMasterSwarm previous)
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

	public static BeeMasterSwarm Deserialize(BufferStream stream, BeeMasterSwarm instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.BeeMasterSwarm");
			if (num == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BeeMasterSwarm");
				instance.population = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BeeMasterSwarm");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BeeMasterSwarm");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static BeeMasterSwarm DeserializeLengthDelimited(BufferStream stream, BeeMasterSwarm instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.BeeMasterSwarm");
			if (num2 == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BeeMasterSwarm");
				instance.population = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BeeMasterSwarm");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BeeMasterSwarm");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static BeeMasterSwarm DeserializeLength(BufferStream stream, int length, BeeMasterSwarm instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.BeeMasterSwarm");
			if (num2 == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BeeMasterSwarm");
				instance.population = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BeeMasterSwarm");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BeeMasterSwarm");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, BeeMasterSwarm instance, BeeMasterSwarm previous)
	{
		if (instance.population != previous.population)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.population);
		}
	}

	public static void Serialize(BufferStream stream, BeeMasterSwarm instance)
	{
		if (instance.population != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.population);
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
