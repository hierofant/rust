using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class MixingTable : IDisposable, Pool.IPooled, IProto<MixingTable>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public float totalMixTime;

	[NonSerialized]
	public float remainingMixTime;

	[NonSerialized]
	public int currentRecipe;

	[NonSerialized]
	public int pendingItem;

	public static void ResetToPool(MixingTable instance)
	{
		if (instance.ShouldPool)
		{
			instance.totalMixTime = 0f;
			instance.remainingMixTime = 0f;
			instance.currentRecipe = 0;
			instance.pendingItem = 0;
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
			throw new Exception("Trying to dispose MixingTable with ShouldPool set to false!");
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

	public void CopyTo(MixingTable instance)
	{
		instance.totalMixTime = totalMixTime;
		instance.remainingMixTime = remainingMixTime;
		instance.currentRecipe = currentRecipe;
		instance.pendingItem = pendingItem;
	}

	public MixingTable Copy()
	{
		MixingTable mixingTable = Pool.Get<MixingTable>();
		CopyTo(mixingTable);
		return mixingTable;
	}

	public static MixingTable Deserialize(BufferStream stream)
	{
		MixingTable mixingTable = Pool.Get<MixingTable>();
		Deserialize(stream, mixingTable, isDelta: false);
		return mixingTable;
	}

	public static MixingTable DeserializeLengthDelimited(BufferStream stream)
	{
		MixingTable mixingTable = Pool.Get<MixingTable>();
		DeserializeLengthDelimited(stream, mixingTable, isDelta: false);
		return mixingTable;
	}

	public static MixingTable DeserializeLength(BufferStream stream, int length)
	{
		MixingTable mixingTable = Pool.Get<MixingTable>();
		DeserializeLength(stream, length, mixingTable, isDelta: false);
		return mixingTable;
	}

	public static MixingTable Deserialize(byte[] buffer)
	{
		MixingTable mixingTable = Pool.Get<MixingTable>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, mixingTable, isDelta: false);
		return mixingTable;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, MixingTable previous)
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

	public static MixingTable Deserialize(BufferStream stream, MixingTable instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.MixingTable");
			switch (num)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MixingTable");
				instance.totalMixTime = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MixingTable");
				instance.remainingMixTime = ProtocolParser.ReadSingle(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MixingTable");
				instance.currentRecipe = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.MixingTable");
				instance.pendingItem = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MixingTable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MixingTable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MixingTable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.MixingTable");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MixingTable");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static MixingTable DeserializeLengthDelimited(BufferStream stream, MixingTable instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.MixingTable");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MixingTable");
				instance.totalMixTime = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MixingTable");
				instance.remainingMixTime = ProtocolParser.ReadSingle(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MixingTable");
				instance.currentRecipe = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.MixingTable");
				instance.pendingItem = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MixingTable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MixingTable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MixingTable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.MixingTable");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MixingTable");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static MixingTable DeserializeLength(BufferStream stream, int length, MixingTable instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.MixingTable");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MixingTable");
				instance.totalMixTime = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MixingTable");
				instance.remainingMixTime = ProtocolParser.ReadSingle(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MixingTable");
				instance.currentRecipe = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.MixingTable");
				instance.pendingItem = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MixingTable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MixingTable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MixingTable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.MixingTable");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MixingTable");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, MixingTable instance, MixingTable previous)
	{
		if (instance.totalMixTime != previous.totalMixTime)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.totalMixTime);
		}
		if (instance.remainingMixTime != previous.remainingMixTime)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.remainingMixTime);
		}
		if (instance.currentRecipe != previous.currentRecipe)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.currentRecipe);
		}
		if (instance.pendingItem != previous.pendingItem)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.pendingItem);
		}
	}

	public static void Serialize(BufferStream stream, MixingTable instance)
	{
		if (instance.totalMixTime != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.totalMixTime);
		}
		if (instance.remainingMixTime != 0f)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.remainingMixTime);
		}
		if (instance.currentRecipe != 0)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.currentRecipe);
		}
		if (instance.pendingItem != 0)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.pendingItem);
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
