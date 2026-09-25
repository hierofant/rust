using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class GrowableEntity : IDisposable, Pool.IPooled, IProto<GrowableEntity>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public int state;

	[NonSerialized]
	public float age;

	[NonSerialized]
	public int water;

	[NonSerialized]
	public float healthy;

	[NonSerialized]
	public float totalAge;

	[NonSerialized]
	public float growthAge;

	[NonSerialized]
	public float yieldFraction;

	[NonSerialized]
	public float stageAge;

	[NonSerialized]
	public int genes;

	[NonSerialized]
	public float lightModifier;

	[NonSerialized]
	public float waterModifier;

	[NonSerialized]
	public float groundModifier;

	[NonSerialized]
	public float happiness;

	[NonSerialized]
	public float temperatureModifier;

	[NonSerialized]
	public float waterConsumption;

	[NonSerialized]
	public float yieldPool;

	[NonSerialized]
	public bool fertilized;

	[NonSerialized]
	public int previousGenes;

	public static void ResetToPool(GrowableEntity instance)
	{
		if (instance.ShouldPool)
		{
			instance.state = 0;
			instance.age = 0f;
			instance.water = 0;
			instance.healthy = 0f;
			instance.totalAge = 0f;
			instance.growthAge = 0f;
			instance.yieldFraction = 0f;
			instance.stageAge = 0f;
			instance.genes = 0;
			instance.lightModifier = 0f;
			instance.waterModifier = 0f;
			instance.groundModifier = 0f;
			instance.happiness = 0f;
			instance.temperatureModifier = 0f;
			instance.waterConsumption = 0f;
			instance.yieldPool = 0f;
			instance.fertilized = false;
			instance.previousGenes = 0;
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
			throw new Exception("Trying to dispose GrowableEntity with ShouldPool set to false!");
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

	public void CopyTo(GrowableEntity instance)
	{
		instance.state = state;
		instance.age = age;
		instance.water = water;
		instance.healthy = healthy;
		instance.totalAge = totalAge;
		instance.growthAge = growthAge;
		instance.yieldFraction = yieldFraction;
		instance.stageAge = stageAge;
		instance.genes = genes;
		instance.lightModifier = lightModifier;
		instance.waterModifier = waterModifier;
		instance.groundModifier = groundModifier;
		instance.happiness = happiness;
		instance.temperatureModifier = temperatureModifier;
		instance.waterConsumption = waterConsumption;
		instance.yieldPool = yieldPool;
		instance.fertilized = fertilized;
		instance.previousGenes = previousGenes;
	}

	public GrowableEntity Copy()
	{
		GrowableEntity growableEntity = Pool.Get<GrowableEntity>();
		CopyTo(growableEntity);
		return growableEntity;
	}

	public static GrowableEntity Deserialize(BufferStream stream)
	{
		GrowableEntity growableEntity = Pool.Get<GrowableEntity>();
		Deserialize(stream, growableEntity, isDelta: false);
		return growableEntity;
	}

	public static GrowableEntity DeserializeLengthDelimited(BufferStream stream)
	{
		GrowableEntity growableEntity = Pool.Get<GrowableEntity>();
		DeserializeLengthDelimited(stream, growableEntity, isDelta: false);
		return growableEntity;
	}

	public static GrowableEntity DeserializeLength(BufferStream stream, int length)
	{
		GrowableEntity growableEntity = Pool.Get<GrowableEntity>();
		DeserializeLength(stream, length, growableEntity, isDelta: false);
		return growableEntity;
	}

	public static GrowableEntity Deserialize(byte[] buffer)
	{
		GrowableEntity growableEntity = Pool.Get<GrowableEntity>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, growableEntity, isDelta: false);
		return growableEntity;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, GrowableEntity previous)
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

	public static GrowableEntity Deserialize(BufferStream stream, GrowableEntity instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.GrowableEntity");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.state = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.age = ProtocolParser.ReadSingle(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.water = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.healthy = ProtocolParser.ReadSingle(stream);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.totalAge = ProtocolParser.ReadSingle(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.growthAge = ProtocolParser.ReadSingle(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.yieldFraction = ProtocolParser.ReadSingle(stream);
				continue;
			case 69:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.stageAge = ProtocolParser.ReadSingle(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.genes = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 85:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.lightModifier = ProtocolParser.ReadSingle(stream);
				continue;
			case 93:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.waterModifier = ProtocolParser.ReadSingle(stream);
				continue;
			case 101:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.groundModifier = ProtocolParser.ReadSingle(stream);
				continue;
			case 109:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.happiness = ProtocolParser.ReadSingle(stream);
				continue;
			case 117:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.temperatureModifier = ProtocolParser.ReadSingle(stream);
				continue;
			case 125:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.waterConsumption = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				if (key.WireType == Wire.Fixed32)
				{
					instance.yieldPool = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 17u:
				stream.ValidateFieldOrder(ref lastFieldId, 17u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				if (key.WireType == Wire.Varint)
				{
					instance.fertilized = ProtocolParser.ReadBool(stream);
				}
				break;
			case 18u:
				stream.ValidateFieldOrder(ref lastFieldId, 18u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				if (key.WireType == Wire.Varint)
				{
					instance.previousGenes = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static GrowableEntity DeserializeLengthDelimited(BufferStream stream, GrowableEntity instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.GrowableEntity");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.state = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.age = ProtocolParser.ReadSingle(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.water = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.healthy = ProtocolParser.ReadSingle(stream);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.totalAge = ProtocolParser.ReadSingle(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.growthAge = ProtocolParser.ReadSingle(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.yieldFraction = ProtocolParser.ReadSingle(stream);
				continue;
			case 69:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.stageAge = ProtocolParser.ReadSingle(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.genes = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 85:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.lightModifier = ProtocolParser.ReadSingle(stream);
				continue;
			case 93:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.waterModifier = ProtocolParser.ReadSingle(stream);
				continue;
			case 101:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.groundModifier = ProtocolParser.ReadSingle(stream);
				continue;
			case 109:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.happiness = ProtocolParser.ReadSingle(stream);
				continue;
			case 117:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.temperatureModifier = ProtocolParser.ReadSingle(stream);
				continue;
			case 125:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.waterConsumption = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				if (key.WireType == Wire.Fixed32)
				{
					instance.yieldPool = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 17u:
				stream.ValidateFieldOrder(ref lastFieldId, 17u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				if (key.WireType == Wire.Varint)
				{
					instance.fertilized = ProtocolParser.ReadBool(stream);
				}
				break;
			case 18u:
				stream.ValidateFieldOrder(ref lastFieldId, 18u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				if (key.WireType == Wire.Varint)
				{
					instance.previousGenes = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static GrowableEntity DeserializeLength(BufferStream stream, int length, GrowableEntity instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.GrowableEntity");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.state = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.age = ProtocolParser.ReadSingle(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.water = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.healthy = ProtocolParser.ReadSingle(stream);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.totalAge = ProtocolParser.ReadSingle(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.growthAge = ProtocolParser.ReadSingle(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.yieldFraction = ProtocolParser.ReadSingle(stream);
				continue;
			case 69:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.stageAge = ProtocolParser.ReadSingle(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.genes = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 85:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.lightModifier = ProtocolParser.ReadSingle(stream);
				continue;
			case 93:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.waterModifier = ProtocolParser.ReadSingle(stream);
				continue;
			case 101:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.groundModifier = ProtocolParser.ReadSingle(stream);
				continue;
			case 109:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.happiness = ProtocolParser.ReadSingle(stream);
				continue;
			case 117:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.temperatureModifier = ProtocolParser.ReadSingle(stream);
				continue;
			case 125:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				instance.waterConsumption = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				if (key.WireType == Wire.Fixed32)
				{
					instance.yieldPool = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 17u:
				stream.ValidateFieldOrder(ref lastFieldId, 17u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				if (key.WireType == Wire.Varint)
				{
					instance.fertilized = ProtocolParser.ReadBool(stream);
				}
				break;
			case 18u:
				stream.ValidateFieldOrder(ref lastFieldId, 18u, fieldIsRepeated: false, "ProtoBuf.GrowableEntity");
				if (key.WireType == Wire.Varint)
				{
					instance.previousGenes = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.GrowableEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, GrowableEntity instance, GrowableEntity previous)
	{
		if (instance.state != previous.state)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.state);
		}
		if (instance.age != previous.age)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.age);
		}
		if (instance.water != previous.water)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.water);
		}
		if (instance.healthy != previous.healthy)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.healthy);
		}
		if (instance.totalAge != previous.totalAge)
		{
			stream.WriteByte(45);
			ProtocolParser.WriteSingle(stream, instance.totalAge);
		}
		if (instance.growthAge != previous.growthAge)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.growthAge);
		}
		if (instance.yieldFraction != previous.yieldFraction)
		{
			stream.WriteByte(61);
			ProtocolParser.WriteSingle(stream, instance.yieldFraction);
		}
		if (instance.stageAge != previous.stageAge)
		{
			stream.WriteByte(69);
			ProtocolParser.WriteSingle(stream, instance.stageAge);
		}
		if (instance.genes != previous.genes)
		{
			stream.WriteByte(72);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.genes);
		}
		if (instance.lightModifier != previous.lightModifier)
		{
			stream.WriteByte(85);
			ProtocolParser.WriteSingle(stream, instance.lightModifier);
		}
		if (instance.waterModifier != previous.waterModifier)
		{
			stream.WriteByte(93);
			ProtocolParser.WriteSingle(stream, instance.waterModifier);
		}
		if (instance.groundModifier != previous.groundModifier)
		{
			stream.WriteByte(101);
			ProtocolParser.WriteSingle(stream, instance.groundModifier);
		}
		if (instance.happiness != previous.happiness)
		{
			stream.WriteByte(109);
			ProtocolParser.WriteSingle(stream, instance.happiness);
		}
		if (instance.temperatureModifier != previous.temperatureModifier)
		{
			stream.WriteByte(117);
			ProtocolParser.WriteSingle(stream, instance.temperatureModifier);
		}
		if (instance.waterConsumption != previous.waterConsumption)
		{
			stream.WriteByte(125);
			ProtocolParser.WriteSingle(stream, instance.waterConsumption);
		}
		if (instance.yieldPool != previous.yieldPool)
		{
			stream.WriteByte(133);
			stream.WriteByte(1);
			ProtocolParser.WriteSingle(stream, instance.yieldPool);
		}
		stream.WriteByte(136);
		stream.WriteByte(1);
		ProtocolParser.WriteBool(stream, instance.fertilized);
		if (instance.previousGenes != previous.previousGenes)
		{
			stream.WriteByte(144);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.previousGenes);
		}
	}

	public static void Serialize(BufferStream stream, GrowableEntity instance)
	{
		if (instance.state != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.state);
		}
		if (instance.age != 0f)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.age);
		}
		if (instance.water != 0)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.water);
		}
		if (instance.healthy != 0f)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.healthy);
		}
		if (instance.totalAge != 0f)
		{
			stream.WriteByte(45);
			ProtocolParser.WriteSingle(stream, instance.totalAge);
		}
		if (instance.growthAge != 0f)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.growthAge);
		}
		if (instance.yieldFraction != 0f)
		{
			stream.WriteByte(61);
			ProtocolParser.WriteSingle(stream, instance.yieldFraction);
		}
		if (instance.stageAge != 0f)
		{
			stream.WriteByte(69);
			ProtocolParser.WriteSingle(stream, instance.stageAge);
		}
		if (instance.genes != 0)
		{
			stream.WriteByte(72);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.genes);
		}
		if (instance.lightModifier != 0f)
		{
			stream.WriteByte(85);
			ProtocolParser.WriteSingle(stream, instance.lightModifier);
		}
		if (instance.waterModifier != 0f)
		{
			stream.WriteByte(93);
			ProtocolParser.WriteSingle(stream, instance.waterModifier);
		}
		if (instance.groundModifier != 0f)
		{
			stream.WriteByte(101);
			ProtocolParser.WriteSingle(stream, instance.groundModifier);
		}
		if (instance.happiness != 0f)
		{
			stream.WriteByte(109);
			ProtocolParser.WriteSingle(stream, instance.happiness);
		}
		if (instance.temperatureModifier != 0f)
		{
			stream.WriteByte(117);
			ProtocolParser.WriteSingle(stream, instance.temperatureModifier);
		}
		if (instance.waterConsumption != 0f)
		{
			stream.WriteByte(125);
			ProtocolParser.WriteSingle(stream, instance.waterConsumption);
		}
		if (instance.yieldPool != 0f)
		{
			stream.WriteByte(133);
			stream.WriteByte(1);
			ProtocolParser.WriteSingle(stream, instance.yieldPool);
		}
		if (instance.fertilized)
		{
			stream.WriteByte(136);
			stream.WriteByte(1);
			ProtocolParser.WriteBool(stream, instance.fertilized);
		}
		if (instance.previousGenes != 0)
		{
			stream.WriteByte(144);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.previousGenes);
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
