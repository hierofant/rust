using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class VDDrawEntry : IDisposable, Pool.IPooled, IProto<VDDrawEntry>, IProto
{
	public enum Category
	{
		Line,
		Log,
		Text,
		Sphere,
		Box
	}

	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public string entityName;

	[NonSerialized]
	public int frame;

	[NonSerialized]
	public string label;

	[NonSerialized]
	public Category category;

	[NonSerialized]
	public Color color;

	[NonSerialized]
	public Vector3 start;

	[NonSerialized]
	public Vector3 end;

	[NonSerialized]
	public float sizeX;

	[NonSerialized]
	public float sizeY;

	[NonSerialized]
	public float sizeZ;

	[NonSerialized]
	public string message;

	public static void ResetToPool(VDDrawEntry instance)
	{
		if (instance.ShouldPool)
		{
			instance.entityName = string.Empty;
			instance.frame = 0;
			instance.label = string.Empty;
			instance.category = Category.Line;
			instance.color = default(Color);
			instance.start = default(Vector3);
			instance.end = default(Vector3);
			instance.sizeX = 0f;
			instance.sizeY = 0f;
			instance.sizeZ = 0f;
			instance.message = string.Empty;
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
			throw new Exception("Trying to dispose VDDrawEntry with ShouldPool set to false!");
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

	public void CopyTo(VDDrawEntry instance)
	{
		instance.entityName = entityName;
		instance.frame = frame;
		instance.label = label;
		instance.category = category;
		instance.color = color;
		instance.start = start;
		instance.end = end;
		instance.sizeX = sizeX;
		instance.sizeY = sizeY;
		instance.sizeZ = sizeZ;
		instance.message = message;
	}

	public VDDrawEntry Copy()
	{
		VDDrawEntry vDDrawEntry = Pool.Get<VDDrawEntry>();
		CopyTo(vDDrawEntry);
		return vDDrawEntry;
	}

	public static VDDrawEntry Deserialize(BufferStream stream)
	{
		VDDrawEntry vDDrawEntry = Pool.Get<VDDrawEntry>();
		Deserialize(stream, vDDrawEntry, isDelta: false);
		return vDDrawEntry;
	}

	public static VDDrawEntry DeserializeLengthDelimited(BufferStream stream)
	{
		VDDrawEntry vDDrawEntry = Pool.Get<VDDrawEntry>();
		DeserializeLengthDelimited(stream, vDDrawEntry, isDelta: false);
		return vDDrawEntry;
	}

	public static VDDrawEntry DeserializeLength(BufferStream stream, int length)
	{
		VDDrawEntry vDDrawEntry = Pool.Get<VDDrawEntry>();
		DeserializeLength(stream, length, vDDrawEntry, isDelta: false);
		return vDDrawEntry;
	}

	public static VDDrawEntry Deserialize(byte[] buffer)
	{
		VDDrawEntry vDDrawEntry = Pool.Get<VDDrawEntry>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, vDDrawEntry, isDelta: false);
		return vDDrawEntry;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, VDDrawEntry previous)
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

	public static VDDrawEntry Deserialize(BufferStream stream, VDDrawEntry instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.VDDrawEntry");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				instance.entityName = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				instance.frame = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				instance.label = ProtocolParser.ReadString(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				instance.category = (Category)ProtocolParser.ReadUInt64(stream);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ColorSerialized.DeserializeLengthDelimited(stream, ref instance.color, isDelta);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.start, isDelta);
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.end, isDelta);
				continue;
			case 69:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				instance.sizeX = ProtocolParser.ReadSingle(stream);
				continue;
			case 77:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				instance.sizeY = ProtocolParser.ReadSingle(stream);
				continue;
			case 85:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				instance.sizeZ = ProtocolParser.ReadSingle(stream);
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				instance.message = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static VDDrawEntry DeserializeLengthDelimited(BufferStream stream, VDDrawEntry instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.VDDrawEntry");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				instance.entityName = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				instance.frame = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				instance.label = ProtocolParser.ReadString(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				instance.category = (Category)ProtocolParser.ReadUInt64(stream);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ColorSerialized.DeserializeLengthDelimited(stream, ref instance.color, isDelta);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.start, isDelta);
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.end, isDelta);
				continue;
			case 69:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				instance.sizeX = ProtocolParser.ReadSingle(stream);
				continue;
			case 77:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				instance.sizeY = ProtocolParser.ReadSingle(stream);
				continue;
			case 85:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				instance.sizeZ = ProtocolParser.ReadSingle(stream);
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				instance.message = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static VDDrawEntry DeserializeLength(BufferStream stream, int length, VDDrawEntry instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.VDDrawEntry");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				instance.entityName = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				instance.frame = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				instance.label = ProtocolParser.ReadString(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				instance.category = (Category)ProtocolParser.ReadUInt64(stream);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ColorSerialized.DeserializeLengthDelimited(stream, ref instance.color, isDelta);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.start, isDelta);
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.end, isDelta);
				continue;
			case 69:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				instance.sizeX = ProtocolParser.ReadSingle(stream);
				continue;
			case 77:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				instance.sizeY = ProtocolParser.ReadSingle(stream);
				continue;
			case 85:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				instance.sizeZ = ProtocolParser.ReadSingle(stream);
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				instance.message = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VDDrawEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, VDDrawEntry instance, VDDrawEntry previous)
	{
		if (instance.entityName != previous.entityName)
		{
			if (instance.entityName == null)
			{
				throw new ArgumentNullException("entityName", "Required by proto specification.");
			}
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.entityName);
		}
		if (instance.frame != previous.frame)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.frame);
		}
		if (instance.label != previous.label)
		{
			if (instance.label == null)
			{
				throw new ArgumentNullException("label", "Required by proto specification.");
			}
			stream.WriteByte(26);
			ProtocolParser.WriteString(stream, instance.label);
		}
		stream.WriteByte(32);
		ProtocolParser.WriteUInt64(stream, (ulong)instance.category);
		if (instance.color != previous.color)
		{
			stream.WriteByte(42);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			ColorSerialized.SerializeDelta(stream, instance.color, previous.color);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field color (UnityEngine.Color)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.start != previous.start)
		{
			stream.WriteByte(50);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.start, previous.start);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field start (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.end != previous.end)
		{
			stream.WriteByte(58);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.end, previous.end);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field end (UnityEngine.Vector3)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
		if (instance.sizeX != previous.sizeX)
		{
			stream.WriteByte(69);
			ProtocolParser.WriteSingle(stream, instance.sizeX);
		}
		if (instance.sizeY != previous.sizeY)
		{
			stream.WriteByte(77);
			ProtocolParser.WriteSingle(stream, instance.sizeY);
		}
		if (instance.sizeZ != previous.sizeZ)
		{
			stream.WriteByte(85);
			ProtocolParser.WriteSingle(stream, instance.sizeZ);
		}
		if (instance.message != null && instance.message != previous.message)
		{
			stream.WriteByte(90);
			ProtocolParser.WriteString(stream, instance.message);
		}
	}

	public static void Serialize(BufferStream stream, VDDrawEntry instance)
	{
		if (instance.entityName == null)
		{
			throw new ArgumentNullException("entityName", "Required by proto specification.");
		}
		stream.WriteByte(10);
		ProtocolParser.WriteString(stream, instance.entityName);
		if (instance.frame != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.frame);
		}
		if (instance.label == null)
		{
			throw new ArgumentNullException("label", "Required by proto specification.");
		}
		stream.WriteByte(26);
		ProtocolParser.WriteString(stream, instance.label);
		stream.WriteByte(32);
		ProtocolParser.WriteUInt64(stream, (ulong)instance.category);
		if (instance.color != default(Color))
		{
			stream.WriteByte(42);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			ColorSerialized.Serialize(stream, instance.color);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field color (UnityEngine.Color)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.start != default(Vector3))
		{
			stream.WriteByte(50);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.start);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field start (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.end != default(Vector3))
		{
			stream.WriteByte(58);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.end);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field end (UnityEngine.Vector3)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
		if (instance.sizeX != 0f)
		{
			stream.WriteByte(69);
			ProtocolParser.WriteSingle(stream, instance.sizeX);
		}
		if (instance.sizeY != 0f)
		{
			stream.WriteByte(77);
			ProtocolParser.WriteSingle(stream, instance.sizeY);
		}
		if (instance.sizeZ != 0f)
		{
			stream.WriteByte(85);
			ProtocolParser.WriteSingle(stream, instance.sizeZ);
		}
		if (instance.message != null)
		{
			stream.WriteByte(90);
			ProtocolParser.WriteString(stream, instance.message);
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
