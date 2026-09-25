using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class WireLineAnchorInfo : IDisposable, Pool.IPooled, IProto<WireLineAnchorInfo>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public NetworkableId parentID;

	[NonSerialized]
	public string boneName;

	[NonSerialized]
	public long index;

	[NonSerialized]
	public Vector3 position;

	public static void ResetToPool(WireLineAnchorInfo instance)
	{
		if (instance.ShouldPool)
		{
			instance.parentID = default(NetworkableId);
			instance.boneName = string.Empty;
			instance.index = 0L;
			instance.position = default(Vector3);
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
			throw new Exception("Trying to dispose WireLineAnchorInfo with ShouldPool set to false!");
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

	public void CopyTo(WireLineAnchorInfo instance)
	{
		instance.parentID = parentID;
		instance.boneName = boneName;
		instance.index = index;
		instance.position = position;
	}

	public WireLineAnchorInfo Copy()
	{
		WireLineAnchorInfo wireLineAnchorInfo = Pool.Get<WireLineAnchorInfo>();
		CopyTo(wireLineAnchorInfo);
		return wireLineAnchorInfo;
	}

	public static WireLineAnchorInfo Deserialize(BufferStream stream)
	{
		WireLineAnchorInfo wireLineAnchorInfo = Pool.Get<WireLineAnchorInfo>();
		Deserialize(stream, wireLineAnchorInfo, isDelta: false);
		return wireLineAnchorInfo;
	}

	public static WireLineAnchorInfo DeserializeLengthDelimited(BufferStream stream)
	{
		WireLineAnchorInfo wireLineAnchorInfo = Pool.Get<WireLineAnchorInfo>();
		DeserializeLengthDelimited(stream, wireLineAnchorInfo, isDelta: false);
		return wireLineAnchorInfo;
	}

	public static WireLineAnchorInfo DeserializeLength(BufferStream stream, int length)
	{
		WireLineAnchorInfo wireLineAnchorInfo = Pool.Get<WireLineAnchorInfo>();
		DeserializeLength(stream, length, wireLineAnchorInfo, isDelta: false);
		return wireLineAnchorInfo;
	}

	public static WireLineAnchorInfo Deserialize(byte[] buffer)
	{
		WireLineAnchorInfo wireLineAnchorInfo = Pool.Get<WireLineAnchorInfo>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, wireLineAnchorInfo, isDelta: false);
		return wireLineAnchorInfo;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, WireLineAnchorInfo previous)
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

	public static WireLineAnchorInfo Deserialize(BufferStream stream, WireLineAnchorInfo instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.WireLineAnchorInfo");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WireLineAnchorInfo");
				instance.parentID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.WireLineAnchorInfo");
				instance.boneName = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.WireLineAnchorInfo");
				instance.index = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.WireLineAnchorInfo");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.position, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WireLineAnchorInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.WireLineAnchorInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.WireLineAnchorInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.WireLineAnchorInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.WireLineAnchorInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static WireLineAnchorInfo DeserializeLengthDelimited(BufferStream stream, WireLineAnchorInfo instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.WireLineAnchorInfo");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WireLineAnchorInfo");
				instance.parentID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.WireLineAnchorInfo");
				instance.boneName = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.WireLineAnchorInfo");
				instance.index = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.WireLineAnchorInfo");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.position, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WireLineAnchorInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.WireLineAnchorInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.WireLineAnchorInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.WireLineAnchorInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.WireLineAnchorInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static WireLineAnchorInfo DeserializeLength(BufferStream stream, int length, WireLineAnchorInfo instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.WireLineAnchorInfo");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WireLineAnchorInfo");
				instance.parentID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.WireLineAnchorInfo");
				instance.boneName = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.WireLineAnchorInfo");
				instance.index = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.WireLineAnchorInfo");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.position, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WireLineAnchorInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.WireLineAnchorInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.WireLineAnchorInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.WireLineAnchorInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.WireLineAnchorInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, WireLineAnchorInfo instance, WireLineAnchorInfo previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, instance.parentID.Value);
		if (instance.boneName != previous.boneName)
		{
			if (instance.boneName == null)
			{
				throw new ArgumentNullException("boneName", "Required by proto specification.");
			}
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.boneName);
		}
		stream.WriteByte(24);
		ProtocolParser.WriteUInt64(stream, (ulong)instance.index);
		if (instance.position != previous.position)
		{
			stream.WriteByte(34);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int num = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.position, previous.position);
			int num2 = stream.Position - num;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field position (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span, 0);
		}
	}

	public static void Serialize(BufferStream stream, WireLineAnchorInfo instance)
	{
		if (instance.parentID != default(NetworkableId))
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.parentID.Value);
		}
		if (instance.boneName == null)
		{
			throw new ArgumentNullException("boneName", "Required by proto specification.");
		}
		stream.WriteByte(18);
		ProtocolParser.WriteString(stream, instance.boneName);
		if (instance.index != 0L)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.index);
		}
		if (instance.position != default(Vector3))
		{
			stream.WriteByte(34);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int num = stream.Position;
			Vector3Serialized.Serialize(stream, instance.position);
			int num2 = stream.Position - num;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field position (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span, 0);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref parentID.Value);
	}
}
