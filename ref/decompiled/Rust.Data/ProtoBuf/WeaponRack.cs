using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class WeaponRack : IDisposable, Pool.IPooled, IProto<WeaponRack>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<WeaponRackItem> items;

	public static void ResetToPool(WeaponRack instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.items != null)
		{
			for (int i = 0; i < instance.items.Count; i++)
			{
				if (instance.items[i] != null)
				{
					instance.items[i].ResetToPool();
					instance.items[i] = null;
				}
			}
			List<WeaponRackItem> obj = instance.items;
			Pool.Free(ref obj, freeElements: false);
			instance.items = obj;
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
			throw new Exception("Trying to dispose WeaponRack with ShouldPool set to false!");
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

	public void CopyTo(WeaponRack instance)
	{
		if (items != null)
		{
			instance.items = Pool.Get<List<WeaponRackItem>>();
			for (int i = 0; i < items.Count; i++)
			{
				WeaponRackItem item = items[i].Copy();
				instance.items.Add(item);
			}
		}
		else
		{
			instance.items = null;
		}
	}

	public WeaponRack Copy()
	{
		WeaponRack weaponRack = Pool.Get<WeaponRack>();
		CopyTo(weaponRack);
		return weaponRack;
	}

	public static WeaponRack Deserialize(BufferStream stream)
	{
		WeaponRack weaponRack = Pool.Get<WeaponRack>();
		Deserialize(stream, weaponRack, isDelta: false);
		return weaponRack;
	}

	public static WeaponRack DeserializeLengthDelimited(BufferStream stream)
	{
		WeaponRack weaponRack = Pool.Get<WeaponRack>();
		DeserializeLengthDelimited(stream, weaponRack, isDelta: false);
		return weaponRack;
	}

	public static WeaponRack DeserializeLength(BufferStream stream, int length)
	{
		WeaponRack weaponRack = Pool.Get<WeaponRack>();
		DeserializeLength(stream, length, weaponRack, isDelta: false);
		return weaponRack;
	}

	public static WeaponRack Deserialize(byte[] buffer)
	{
		WeaponRack weaponRack = Pool.Get<WeaponRack>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, weaponRack, isDelta: false);
		return weaponRack;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, WeaponRack previous)
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

	public static WeaponRack Deserialize(BufferStream stream, WeaponRack instance, bool isDelta)
	{
		if (!isDelta && instance.items == null)
		{
			instance.items = Pool.Get<List<WeaponRackItem>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.WeaponRack");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.WeaponRack");
				stream.ConsumeRepeatedElement();
				instance.items.Add(WeaponRackItem.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.WeaponRack");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.WeaponRack");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static WeaponRack DeserializeLengthDelimited(BufferStream stream, WeaponRack instance, bool isDelta)
	{
		if (!isDelta && instance.items == null)
		{
			instance.items = Pool.Get<List<WeaponRackItem>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.WeaponRack");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.WeaponRack");
				stream.ConsumeRepeatedElement();
				instance.items.Add(WeaponRackItem.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.WeaponRack");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.WeaponRack");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static WeaponRack DeserializeLength(BufferStream stream, int length, WeaponRack instance, bool isDelta)
	{
		if (!isDelta && instance.items == null)
		{
			instance.items = Pool.Get<List<WeaponRackItem>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.WeaponRack");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.WeaponRack");
				stream.ConsumeRepeatedElement();
				instance.items.Add(WeaponRackItem.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.WeaponRack");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.WeaponRack");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, WeaponRack instance, WeaponRack previous)
	{
		if (instance.items == null)
		{
			return;
		}
		for (int i = 0; i < instance.items.Count; i++)
		{
			WeaponRackItem weaponRackItem = instance.items[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			WeaponRackItem.SerializeDelta(stream, weaponRackItem, weaponRackItem);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field items (ProtoBuf.WeaponRackItem)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public static void Serialize(BufferStream stream, WeaponRack instance)
	{
		if (instance.items == null)
		{
			return;
		}
		for (int i = 0; i < instance.items.Count; i++)
		{
			WeaponRackItem instance2 = instance.items[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			WeaponRackItem.Serialize(stream, instance2);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field items (ProtoBuf.WeaponRackItem)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (items != null)
		{
			for (int i = 0; i < items.Count; i++)
			{
				items[i]?.InspectUids(action);
			}
		}
	}
}
