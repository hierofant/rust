using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class IndustrialCrafter : IDisposable, Pool.IPooled, IProto<IndustrialCrafter>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public int currentlyCrafting;

	[NonSerialized]
	public int currentlyCraftingAmount;

	public static void ResetToPool(IndustrialCrafter instance)
	{
		if (instance.ShouldPool)
		{
			instance.currentlyCrafting = 0;
			instance.currentlyCraftingAmount = 0;
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
			throw new Exception("Trying to dispose IndustrialCrafter with ShouldPool set to false!");
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

	public void CopyTo(IndustrialCrafter instance)
	{
		instance.currentlyCrafting = currentlyCrafting;
		instance.currentlyCraftingAmount = currentlyCraftingAmount;
	}

	public IndustrialCrafter Copy()
	{
		IndustrialCrafter industrialCrafter = Pool.Get<IndustrialCrafter>();
		CopyTo(industrialCrafter);
		return industrialCrafter;
	}

	public static IndustrialCrafter Deserialize(BufferStream stream)
	{
		IndustrialCrafter industrialCrafter = Pool.Get<IndustrialCrafter>();
		Deserialize(stream, industrialCrafter, isDelta: false);
		return industrialCrafter;
	}

	public static IndustrialCrafter DeserializeLengthDelimited(BufferStream stream)
	{
		IndustrialCrafter industrialCrafter = Pool.Get<IndustrialCrafter>();
		DeserializeLengthDelimited(stream, industrialCrafter, isDelta: false);
		return industrialCrafter;
	}

	public static IndustrialCrafter DeserializeLength(BufferStream stream, int length)
	{
		IndustrialCrafter industrialCrafter = Pool.Get<IndustrialCrafter>();
		DeserializeLength(stream, length, industrialCrafter, isDelta: false);
		return industrialCrafter;
	}

	public static IndustrialCrafter Deserialize(byte[] buffer)
	{
		IndustrialCrafter industrialCrafter = Pool.Get<IndustrialCrafter>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, industrialCrafter, isDelta: false);
		return industrialCrafter;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, IndustrialCrafter previous)
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

	public static IndustrialCrafter Deserialize(BufferStream stream, IndustrialCrafter instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.IndustrialCrafter");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IndustrialCrafter");
				instance.currentlyCrafting = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IndustrialCrafter");
				instance.currentlyCraftingAmount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IndustrialCrafter");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IndustrialCrafter");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.IndustrialCrafter");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static IndustrialCrafter DeserializeLengthDelimited(BufferStream stream, IndustrialCrafter instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.IndustrialCrafter");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IndustrialCrafter");
				instance.currentlyCrafting = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IndustrialCrafter");
				instance.currentlyCraftingAmount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IndustrialCrafter");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IndustrialCrafter");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.IndustrialCrafter");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static IndustrialCrafter DeserializeLength(BufferStream stream, int length, IndustrialCrafter instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.IndustrialCrafter");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IndustrialCrafter");
				instance.currentlyCrafting = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IndustrialCrafter");
				instance.currentlyCraftingAmount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IndustrialCrafter");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IndustrialCrafter");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.IndustrialCrafter");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, IndustrialCrafter instance, IndustrialCrafter previous)
	{
		if (instance.currentlyCrafting != previous.currentlyCrafting)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.currentlyCrafting);
		}
		if (instance.currentlyCraftingAmount != previous.currentlyCraftingAmount)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.currentlyCraftingAmount);
		}
	}

	public static void Serialize(BufferStream stream, IndustrialCrafter instance)
	{
		if (instance.currentlyCrafting != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.currentlyCrafting);
		}
		if (instance.currentlyCraftingAmount != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.currentlyCraftingAmount);
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
