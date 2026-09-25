using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class PowergridManager : IDisposable, Pool.IPooled, IProto<PowergridManager>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<ItemId> fuseItemIds;

	public static void ResetToPool(PowergridManager instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.fuseItemIds != null)
			{
				List<ItemId> obj = instance.fuseItemIds;
				Pool.FreeUnmanaged(ref obj);
				instance.fuseItemIds = obj;
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
			throw new Exception("Trying to dispose PowergridManager with ShouldPool set to false!");
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

	public void CopyTo(PowergridManager instance)
	{
		if (fuseItemIds != null)
		{
			instance.fuseItemIds = Pool.Get<List<ItemId>>();
			for (int i = 0; i < fuseItemIds.Count; i++)
			{
				ItemId item = fuseItemIds[i];
				instance.fuseItemIds.Add(item);
			}
		}
		else
		{
			instance.fuseItemIds = null;
		}
	}

	public PowergridManager Copy()
	{
		PowergridManager powergridManager = Pool.Get<PowergridManager>();
		CopyTo(powergridManager);
		return powergridManager;
	}

	public static PowergridManager Deserialize(BufferStream stream)
	{
		PowergridManager powergridManager = Pool.Get<PowergridManager>();
		Deserialize(stream, powergridManager, isDelta: false);
		return powergridManager;
	}

	public static PowergridManager DeserializeLengthDelimited(BufferStream stream)
	{
		PowergridManager powergridManager = Pool.Get<PowergridManager>();
		DeserializeLengthDelimited(stream, powergridManager, isDelta: false);
		return powergridManager;
	}

	public static PowergridManager DeserializeLength(BufferStream stream, int length)
	{
		PowergridManager powergridManager = Pool.Get<PowergridManager>();
		DeserializeLength(stream, length, powergridManager, isDelta: false);
		return powergridManager;
	}

	public static PowergridManager Deserialize(byte[] buffer)
	{
		PowergridManager powergridManager = Pool.Get<PowergridManager>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, powergridManager, isDelta: false);
		return powergridManager;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, PowergridManager previous)
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

	public static PowergridManager Deserialize(BufferStream stream, PowergridManager instance, bool isDelta)
	{
		if (!isDelta && instance.fuseItemIds == null)
		{
			instance.fuseItemIds = Pool.Get<List<ItemId>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.PowergridManager");
			if (num == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.PowergridManager");
				stream.ConsumeRepeatedElement();
				instance.fuseItemIds.Add(new ItemId(ProtocolParser.ReadUInt64(stream)));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.PowergridManager");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PowergridManager");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static PowergridManager DeserializeLengthDelimited(BufferStream stream, PowergridManager instance, bool isDelta)
	{
		if (!isDelta && instance.fuseItemIds == null)
		{
			instance.fuseItemIds = Pool.Get<List<ItemId>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.PowergridManager");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.PowergridManager");
				stream.ConsumeRepeatedElement();
				instance.fuseItemIds.Add(new ItemId(ProtocolParser.ReadUInt64(stream)));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.PowergridManager");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PowergridManager");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static PowergridManager DeserializeLength(BufferStream stream, int length, PowergridManager instance, bool isDelta)
	{
		if (!isDelta && instance.fuseItemIds == null)
		{
			instance.fuseItemIds = Pool.Get<List<ItemId>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.PowergridManager");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.PowergridManager");
				stream.ConsumeRepeatedElement();
				instance.fuseItemIds.Add(new ItemId(ProtocolParser.ReadUInt64(stream)));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.PowergridManager");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PowergridManager");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, PowergridManager instance, PowergridManager previous)
	{
		if (instance.fuseItemIds != null)
		{
			for (int i = 0; i < instance.fuseItemIds.Count; i++)
			{
				ItemId itemId = instance.fuseItemIds[i];
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, itemId.Value);
			}
		}
	}

	public static void Serialize(BufferStream stream, PowergridManager instance)
	{
		if (instance.fuseItemIds != null)
		{
			for (int i = 0; i < instance.fuseItemIds.Count; i++)
			{
				ItemId itemId = instance.fuseItemIds[i];
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, itemId.Value);
			}
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (fuseItemIds != null)
		{
			for (int i = 0; i < fuseItemIds.Count; i++)
			{
				ItemId value = fuseItemIds[i];
				action(UidType.ItemId, ref value.Value);
				fuseItemIds[i] = value;
			}
		}
	}
}
