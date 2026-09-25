using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class PlayerUpdateLoot : IDisposable, Pool.IPooled, IProto<PlayerUpdateLoot>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public ItemId itemID;

	[NonSerialized]
	public NetworkableId entityID;

	[NonSerialized]
	public List<ItemContainer> containers;

	public static void ResetToPool(PlayerUpdateLoot instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.itemID = default(ItemId);
		instance.entityID = default(NetworkableId);
		if (instance.containers != null)
		{
			for (int i = 0; i < instance.containers.Count; i++)
			{
				if (instance.containers[i] != null)
				{
					instance.containers[i].ResetToPool();
					instance.containers[i] = null;
				}
			}
			List<ItemContainer> obj = instance.containers;
			Pool.Free(ref obj, freeElements: false);
			instance.containers = obj;
		}
		Pool.Free(ref instance);
	}

	public void ResetToPool()
	{
		ResetToPool(this);
	}

	public virtual void Dispose()
	{
		if (!ShouldPool)
		{
			throw new Exception("Trying to dispose PlayerUpdateLoot with ShouldPool set to false!");
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

	public void CopyTo(PlayerUpdateLoot instance)
	{
		instance.itemID = itemID;
		instance.entityID = entityID;
		if (containers != null)
		{
			instance.containers = Pool.Get<List<ItemContainer>>();
			for (int i = 0; i < containers.Count; i++)
			{
				ItemContainer item = containers[i].Copy();
				instance.containers.Add(item);
			}
		}
		else
		{
			instance.containers = null;
		}
	}

	public PlayerUpdateLoot Copy()
	{
		PlayerUpdateLoot playerUpdateLoot = Pool.Get<PlayerUpdateLoot>();
		CopyTo(playerUpdateLoot);
		return playerUpdateLoot;
	}

	public static PlayerUpdateLoot Deserialize(BufferStream stream)
	{
		PlayerUpdateLoot playerUpdateLoot = Pool.Get<PlayerUpdateLoot>();
		Deserialize(stream, playerUpdateLoot, isDelta: false);
		return playerUpdateLoot;
	}

	public static PlayerUpdateLoot DeserializeLengthDelimited(BufferStream stream)
	{
		PlayerUpdateLoot playerUpdateLoot = Pool.Get<PlayerUpdateLoot>();
		DeserializeLengthDelimited(stream, playerUpdateLoot, isDelta: false);
		return playerUpdateLoot;
	}

	public static PlayerUpdateLoot DeserializeLength(BufferStream stream, int length)
	{
		PlayerUpdateLoot playerUpdateLoot = Pool.Get<PlayerUpdateLoot>();
		DeserializeLength(stream, length, playerUpdateLoot, isDelta: false);
		return playerUpdateLoot;
	}

	public static PlayerUpdateLoot Deserialize(byte[] buffer)
	{
		PlayerUpdateLoot playerUpdateLoot = Pool.Get<PlayerUpdateLoot>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, playerUpdateLoot, isDelta: false);
		return playerUpdateLoot;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, PlayerUpdateLoot previous)
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

	public static PlayerUpdateLoot Deserialize(BufferStream stream, PlayerUpdateLoot instance, bool isDelta)
	{
		if (!isDelta && instance.containers == null)
		{
			instance.containers = Pool.Get<List<ItemContainer>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.PlayerUpdateLoot");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerUpdateLoot");
				instance.itemID = new ItemId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerUpdateLoot");
				instance.entityID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.PlayerUpdateLoot");
				stream.ConsumeRepeatedElement();
				instance.containers.Add(ItemContainer.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerUpdateLoot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerUpdateLoot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.PlayerUpdateLoot");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerUpdateLoot");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PlayerUpdateLoot DeserializeLengthDelimited(BufferStream stream, PlayerUpdateLoot instance, bool isDelta)
	{
		if (!isDelta && instance.containers == null)
		{
			instance.containers = Pool.Get<List<ItemContainer>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.PlayerUpdateLoot");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerUpdateLoot");
				instance.itemID = new ItemId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerUpdateLoot");
				instance.entityID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.PlayerUpdateLoot");
				stream.ConsumeRepeatedElement();
				instance.containers.Add(ItemContainer.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerUpdateLoot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerUpdateLoot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.PlayerUpdateLoot");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerUpdateLoot");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PlayerUpdateLoot DeserializeLength(BufferStream stream, int length, PlayerUpdateLoot instance, bool isDelta)
	{
		if (!isDelta && instance.containers == null)
		{
			instance.containers = Pool.Get<List<ItemContainer>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.PlayerUpdateLoot");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerUpdateLoot");
				instance.itemID = new ItemId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerUpdateLoot");
				instance.entityID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.PlayerUpdateLoot");
				stream.ConsumeRepeatedElement();
				instance.containers.Add(ItemContainer.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerUpdateLoot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerUpdateLoot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.PlayerUpdateLoot");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerUpdateLoot");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, PlayerUpdateLoot instance, PlayerUpdateLoot previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, instance.itemID.Value);
		stream.WriteByte(16);
		ProtocolParser.WriteUInt64(stream, instance.entityID.Value);
		if (instance.containers == null)
		{
			return;
		}
		for (int i = 0; i < instance.containers.Count; i++)
		{
			ItemContainer itemContainer = instance.containers[i];
			stream.WriteByte(26);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			ItemContainer.SerializeDelta(stream, itemContainer, itemContainer);
			int val = stream.Position - position;
			Span<byte> span = range.GetSpan();
			int num = ProtocolParser.WriteUInt32((uint)val, span, 0);
			if (num < 5)
			{
				span[num - 1] |= 128;
				while (num < 4)
				{
					span[num++] = 128;
				}
				span[4] = 0;
			}
		}
	}

	public static void Serialize(BufferStream stream, PlayerUpdateLoot instance)
	{
		if (instance.itemID != default(ItemId))
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.itemID.Value);
		}
		if (instance.entityID != default(NetworkableId))
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, instance.entityID.Value);
		}
		if (instance.containers == null)
		{
			return;
		}
		for (int i = 0; i < instance.containers.Count; i++)
		{
			ItemContainer instance2 = instance.containers[i];
			stream.WriteByte(26);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			ItemContainer.Serialize(stream, instance2);
			int val = stream.Position - position;
			Span<byte> span = range.GetSpan();
			int num = ProtocolParser.WriteUInt32((uint)val, span, 0);
			if (num < 5)
			{
				span[num - 1] |= 128;
				while (num < 4)
				{
					span[num++] = 128;
				}
				span[4] = 0;
			}
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.ItemId, ref itemID.Value);
		action(UidType.NetworkableId, ref entityID.Value);
		if (containers != null)
		{
			for (int i = 0; i < containers.Count; i++)
			{
				containers[i]?.InspectUids(action);
			}
		}
	}
}
