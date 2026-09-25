using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class PlayerInventory : IDisposable, Pool.IPooled, IProto<PlayerInventory>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public ItemContainer invMain;

	[NonSerialized]
	public ItemContainer invBelt;

	[NonSerialized]
	public ItemContainer invWear;

	public static void ResetToPool(PlayerInventory instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.invMain != null)
			{
				instance.invMain.ResetToPool();
				instance.invMain = null;
			}
			if (instance.invBelt != null)
			{
				instance.invBelt.ResetToPool();
				instance.invBelt = null;
			}
			if (instance.invWear != null)
			{
				instance.invWear.ResetToPool();
				instance.invWear = null;
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
			throw new Exception("Trying to dispose PlayerInventory with ShouldPool set to false!");
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

	public void CopyTo(PlayerInventory instance)
	{
		if (invMain != null)
		{
			if (instance.invMain == null)
			{
				instance.invMain = invMain.Copy();
			}
			else
			{
				invMain.CopyTo(instance.invMain);
			}
		}
		else
		{
			instance.invMain = null;
		}
		if (invBelt != null)
		{
			if (instance.invBelt == null)
			{
				instance.invBelt = invBelt.Copy();
			}
			else
			{
				invBelt.CopyTo(instance.invBelt);
			}
		}
		else
		{
			instance.invBelt = null;
		}
		if (invWear != null)
		{
			if (instance.invWear == null)
			{
				instance.invWear = invWear.Copy();
			}
			else
			{
				invWear.CopyTo(instance.invWear);
			}
		}
		else
		{
			instance.invWear = null;
		}
	}

	public PlayerInventory Copy()
	{
		PlayerInventory playerInventory = Pool.Get<PlayerInventory>();
		CopyTo(playerInventory);
		return playerInventory;
	}

	public static PlayerInventory Deserialize(BufferStream stream)
	{
		PlayerInventory playerInventory = Pool.Get<PlayerInventory>();
		Deserialize(stream, playerInventory, isDelta: false);
		return playerInventory;
	}

	public static PlayerInventory DeserializeLengthDelimited(BufferStream stream)
	{
		PlayerInventory playerInventory = Pool.Get<PlayerInventory>();
		DeserializeLengthDelimited(stream, playerInventory, isDelta: false);
		return playerInventory;
	}

	public static PlayerInventory DeserializeLength(BufferStream stream, int length)
	{
		PlayerInventory playerInventory = Pool.Get<PlayerInventory>();
		DeserializeLength(stream, length, playerInventory, isDelta: false);
		return playerInventory;
	}

	public static PlayerInventory Deserialize(byte[] buffer)
	{
		PlayerInventory playerInventory = Pool.Get<PlayerInventory>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, playerInventory, isDelta: false);
		return playerInventory;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, PlayerInventory previous)
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

	public static PlayerInventory Deserialize(BufferStream stream, PlayerInventory instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.PlayerInventory");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerInventory");
				if (instance.invMain == null)
				{
					instance.invMain = ItemContainer.DeserializeLengthDelimited(stream);
				}
				else
				{
					ItemContainer.DeserializeLengthDelimited(stream, instance.invMain, isDelta);
				}
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerInventory");
				if (instance.invBelt == null)
				{
					instance.invBelt = ItemContainer.DeserializeLengthDelimited(stream);
				}
				else
				{
					ItemContainer.DeserializeLengthDelimited(stream, instance.invBelt, isDelta);
				}
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerInventory");
				if (instance.invWear == null)
				{
					instance.invWear = ItemContainer.DeserializeLengthDelimited(stream);
				}
				else
				{
					ItemContainer.DeserializeLengthDelimited(stream, instance.invWear, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerInventory");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerInventory");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerInventory");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerInventory");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PlayerInventory DeserializeLengthDelimited(BufferStream stream, PlayerInventory instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.PlayerInventory");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerInventory");
				if (instance.invMain == null)
				{
					instance.invMain = ItemContainer.DeserializeLengthDelimited(stream);
				}
				else
				{
					ItemContainer.DeserializeLengthDelimited(stream, instance.invMain, isDelta);
				}
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerInventory");
				if (instance.invBelt == null)
				{
					instance.invBelt = ItemContainer.DeserializeLengthDelimited(stream);
				}
				else
				{
					ItemContainer.DeserializeLengthDelimited(stream, instance.invBelt, isDelta);
				}
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerInventory");
				if (instance.invWear == null)
				{
					instance.invWear = ItemContainer.DeserializeLengthDelimited(stream);
				}
				else
				{
					ItemContainer.DeserializeLengthDelimited(stream, instance.invWear, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerInventory");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerInventory");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerInventory");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerInventory");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PlayerInventory DeserializeLength(BufferStream stream, int length, PlayerInventory instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.PlayerInventory");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerInventory");
				if (instance.invMain == null)
				{
					instance.invMain = ItemContainer.DeserializeLengthDelimited(stream);
				}
				else
				{
					ItemContainer.DeserializeLengthDelimited(stream, instance.invMain, isDelta);
				}
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerInventory");
				if (instance.invBelt == null)
				{
					instance.invBelt = ItemContainer.DeserializeLengthDelimited(stream);
				}
				else
				{
					ItemContainer.DeserializeLengthDelimited(stream, instance.invBelt, isDelta);
				}
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerInventory");
				if (instance.invWear == null)
				{
					instance.invWear = ItemContainer.DeserializeLengthDelimited(stream);
				}
				else
				{
					ItemContainer.DeserializeLengthDelimited(stream, instance.invWear, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerInventory");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerInventory");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerInventory");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerInventory");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, PlayerInventory instance, PlayerInventory previous)
	{
		if (instance.invMain != null)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			ItemContainer.SerializeDelta(stream, instance.invMain, previous.invMain);
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
		if (instance.invBelt != null)
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(5);
			int position2 = stream.Position;
			ItemContainer.SerializeDelta(stream, instance.invBelt, previous.invBelt);
			int val2 = stream.Position - position2;
			Span<byte> span2 = range2.GetSpan();
			int num2 = ProtocolParser.WriteUInt32((uint)val2, span2, 0);
			if (num2 < 5)
			{
				span2[num2 - 1] |= 128;
				while (num2 < 4)
				{
					span2[num2++] = 128;
				}
				span2[4] = 0;
			}
		}
		if (instance.invWear == null)
		{
			return;
		}
		stream.WriteByte(26);
		BufferStream.RangeHandle range3 = stream.GetRange(5);
		int position3 = stream.Position;
		ItemContainer.SerializeDelta(stream, instance.invWear, previous.invWear);
		int val3 = stream.Position - position3;
		Span<byte> span3 = range3.GetSpan();
		int num3 = ProtocolParser.WriteUInt32((uint)val3, span3, 0);
		if (num3 < 5)
		{
			span3[num3 - 1] |= 128;
			while (num3 < 4)
			{
				span3[num3++] = 128;
			}
			span3[4] = 0;
		}
	}

	public static void Serialize(BufferStream stream, PlayerInventory instance)
	{
		if (instance.invMain != null)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			ItemContainer.Serialize(stream, instance.invMain);
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
		if (instance.invBelt != null)
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(5);
			int position2 = stream.Position;
			ItemContainer.Serialize(stream, instance.invBelt);
			int val2 = stream.Position - position2;
			Span<byte> span2 = range2.GetSpan();
			int num2 = ProtocolParser.WriteUInt32((uint)val2, span2, 0);
			if (num2 < 5)
			{
				span2[num2 - 1] |= 128;
				while (num2 < 4)
				{
					span2[num2++] = 128;
				}
				span2[4] = 0;
			}
		}
		if (instance.invWear == null)
		{
			return;
		}
		stream.WriteByte(26);
		BufferStream.RangeHandle range3 = stream.GetRange(5);
		int position3 = stream.Position;
		ItemContainer.Serialize(stream, instance.invWear);
		int val3 = stream.Position - position3;
		Span<byte> span3 = range3.GetSpan();
		int num3 = ProtocolParser.WriteUInt32((uint)val3, span3, 0);
		if (num3 < 5)
		{
			span3[num3 - 1] |= 128;
			while (num3 < 4)
			{
				span3[num3++] = 128;
			}
			span3[4] = 0;
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		invMain?.InspectUids(action);
		invBelt?.InspectUids(action);
		invWear?.InspectUids(action);
	}
}
