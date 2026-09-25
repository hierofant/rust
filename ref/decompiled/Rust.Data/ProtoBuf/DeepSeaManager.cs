using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class DeepSeaManager : IDisposable, Pool.IPooled, IProto<DeepSeaManager>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<ulong> foodPaid;

	[NonSerialized]
	public bool localPlayerPaidFoodToll;

	public static void ResetToPool(DeepSeaManager instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.foodPaid != null)
			{
				List<ulong> obj = instance.foodPaid;
				Pool.FreeUnmanaged(ref obj);
				instance.foodPaid = obj;
			}
			instance.localPlayerPaidFoodToll = false;
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
			throw new Exception("Trying to dispose DeepSeaManager with ShouldPool set to false!");
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

	public void CopyTo(DeepSeaManager instance)
	{
		if (foodPaid != null)
		{
			instance.foodPaid = Pool.Get<List<ulong>>();
			for (int i = 0; i < foodPaid.Count; i++)
			{
				ulong item = foodPaid[i];
				instance.foodPaid.Add(item);
			}
		}
		else
		{
			instance.foodPaid = null;
		}
		instance.localPlayerPaidFoodToll = localPlayerPaidFoodToll;
	}

	public DeepSeaManager Copy()
	{
		DeepSeaManager deepSeaManager = Pool.Get<DeepSeaManager>();
		CopyTo(deepSeaManager);
		return deepSeaManager;
	}

	public static DeepSeaManager Deserialize(BufferStream stream)
	{
		DeepSeaManager deepSeaManager = Pool.Get<DeepSeaManager>();
		Deserialize(stream, deepSeaManager, isDelta: false);
		return deepSeaManager;
	}

	public static DeepSeaManager DeserializeLengthDelimited(BufferStream stream)
	{
		DeepSeaManager deepSeaManager = Pool.Get<DeepSeaManager>();
		DeserializeLengthDelimited(stream, deepSeaManager, isDelta: false);
		return deepSeaManager;
	}

	public static DeepSeaManager DeserializeLength(BufferStream stream, int length)
	{
		DeepSeaManager deepSeaManager = Pool.Get<DeepSeaManager>();
		DeserializeLength(stream, length, deepSeaManager, isDelta: false);
		return deepSeaManager;
	}

	public static DeepSeaManager Deserialize(byte[] buffer)
	{
		DeepSeaManager deepSeaManager = Pool.Get<DeepSeaManager>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, deepSeaManager, isDelta: false);
		return deepSeaManager;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, DeepSeaManager previous)
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

	public static DeepSeaManager Deserialize(BufferStream stream, DeepSeaManager instance, bool isDelta)
	{
		if (!isDelta && instance.foodPaid == null)
		{
			instance.foodPaid = Pool.Get<List<ulong>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.DeepSeaManager");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.DeepSeaManager");
				stream.ConsumeRepeatedElement();
				instance.foodPaid.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DeepSeaManager");
				instance.localPlayerPaidFoodToll = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.DeepSeaManager");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DeepSeaManager");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DeepSeaManager");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DeepSeaManager DeserializeLengthDelimited(BufferStream stream, DeepSeaManager instance, bool isDelta)
	{
		if (!isDelta && instance.foodPaid == null)
		{
			instance.foodPaid = Pool.Get<List<ulong>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.DeepSeaManager");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.DeepSeaManager");
				stream.ConsumeRepeatedElement();
				instance.foodPaid.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DeepSeaManager");
				instance.localPlayerPaidFoodToll = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.DeepSeaManager");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DeepSeaManager");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DeepSeaManager");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DeepSeaManager DeserializeLength(BufferStream stream, int length, DeepSeaManager instance, bool isDelta)
	{
		if (!isDelta && instance.foodPaid == null)
		{
			instance.foodPaid = Pool.Get<List<ulong>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.DeepSeaManager");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.DeepSeaManager");
				stream.ConsumeRepeatedElement();
				instance.foodPaid.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DeepSeaManager");
				instance.localPlayerPaidFoodToll = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.DeepSeaManager");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DeepSeaManager");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DeepSeaManager");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, DeepSeaManager instance, DeepSeaManager previous)
	{
		if (instance.foodPaid != null)
		{
			for (int i = 0; i < instance.foodPaid.Count; i++)
			{
				ulong val = instance.foodPaid[i];
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, val);
			}
		}
		stream.WriteByte(16);
		ProtocolParser.WriteBool(stream, instance.localPlayerPaidFoodToll);
	}

	public static void Serialize(BufferStream stream, DeepSeaManager instance)
	{
		if (instance.foodPaid != null)
		{
			for (int i = 0; i < instance.foodPaid.Count; i++)
			{
				ulong val = instance.foodPaid[i];
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, val);
			}
		}
		if (instance.localPlayerPaidFoodToll)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteBool(stream, instance.localPlayerPaidFoodToll);
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
