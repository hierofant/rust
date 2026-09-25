using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class Mailbox : IDisposable, Pool.IPooled, IProto<Mailbox>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public ItemContainer inventory;

	public static void ResetToPool(Mailbox instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.inventory != null)
			{
				instance.inventory.ResetToPool();
				instance.inventory = null;
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
			throw new Exception("Trying to dispose Mailbox with ShouldPool set to false!");
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

	public void CopyTo(Mailbox instance)
	{
		if (inventory != null)
		{
			if (instance.inventory == null)
			{
				instance.inventory = inventory.Copy();
			}
			else
			{
				inventory.CopyTo(instance.inventory);
			}
		}
		else
		{
			instance.inventory = null;
		}
	}

	public Mailbox Copy()
	{
		Mailbox mailbox = Pool.Get<Mailbox>();
		CopyTo(mailbox);
		return mailbox;
	}

	public static Mailbox Deserialize(BufferStream stream)
	{
		Mailbox mailbox = Pool.Get<Mailbox>();
		Deserialize(stream, mailbox, isDelta: false);
		return mailbox;
	}

	public static Mailbox DeserializeLengthDelimited(BufferStream stream)
	{
		Mailbox mailbox = Pool.Get<Mailbox>();
		DeserializeLengthDelimited(stream, mailbox, isDelta: false);
		return mailbox;
	}

	public static Mailbox DeserializeLength(BufferStream stream, int length)
	{
		Mailbox mailbox = Pool.Get<Mailbox>();
		DeserializeLength(stream, length, mailbox, isDelta: false);
		return mailbox;
	}

	public static Mailbox Deserialize(byte[] buffer)
	{
		Mailbox mailbox = Pool.Get<Mailbox>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, mailbox, isDelta: false);
		return mailbox;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, Mailbox previous)
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

	public static Mailbox Deserialize(BufferStream stream, Mailbox instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Mailbox");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Mailbox");
				if (instance.inventory == null)
				{
					instance.inventory = ItemContainer.DeserializeLengthDelimited(stream);
				}
				else
				{
					ItemContainer.DeserializeLengthDelimited(stream, instance.inventory, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Mailbox");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Mailbox");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static Mailbox DeserializeLengthDelimited(BufferStream stream, Mailbox instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Mailbox");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Mailbox");
				if (instance.inventory == null)
				{
					instance.inventory = ItemContainer.DeserializeLengthDelimited(stream);
				}
				else
				{
					ItemContainer.DeserializeLengthDelimited(stream, instance.inventory, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Mailbox");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Mailbox");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static Mailbox DeserializeLength(BufferStream stream, int length, Mailbox instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Mailbox");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Mailbox");
				if (instance.inventory == null)
				{
					instance.inventory = ItemContainer.DeserializeLengthDelimited(stream);
				}
				else
				{
					ItemContainer.DeserializeLengthDelimited(stream, instance.inventory, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Mailbox");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Mailbox");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, Mailbox instance, Mailbox previous)
	{
		if (instance.inventory == null)
		{
			return;
		}
		stream.WriteByte(10);
		BufferStream.RangeHandle range = stream.GetRange(5);
		int position = stream.Position;
		ItemContainer.SerializeDelta(stream, instance.inventory, previous.inventory);
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

	public static void Serialize(BufferStream stream, Mailbox instance)
	{
		if (instance.inventory == null)
		{
			return;
		}
		stream.WriteByte(10);
		BufferStream.RangeHandle range = stream.GetRange(5);
		int position = stream.Position;
		ItemContainer.Serialize(stream, instance.inventory);
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

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		inventory?.InspectUids(action);
	}
}
