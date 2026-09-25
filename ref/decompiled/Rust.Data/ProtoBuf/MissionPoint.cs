using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class MissionPoint : IDisposable, Pool.IPooled, IProto<MissionPoint>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public string identifier;

	[NonSerialized]
	public Vector3 location;

	public static void ResetToPool(MissionPoint instance)
	{
		if (instance.ShouldPool)
		{
			instance.identifier = string.Empty;
			instance.location = default(Vector3);
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
			throw new Exception("Trying to dispose MissionPoint with ShouldPool set to false!");
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

	public void CopyTo(MissionPoint instance)
	{
		instance.identifier = identifier;
		instance.location = location;
	}

	public MissionPoint Copy()
	{
		MissionPoint missionPoint = Pool.Get<MissionPoint>();
		CopyTo(missionPoint);
		return missionPoint;
	}

	public static MissionPoint Deserialize(BufferStream stream)
	{
		MissionPoint missionPoint = Pool.Get<MissionPoint>();
		Deserialize(stream, missionPoint, isDelta: false);
		return missionPoint;
	}

	public static MissionPoint DeserializeLengthDelimited(BufferStream stream)
	{
		MissionPoint missionPoint = Pool.Get<MissionPoint>();
		DeserializeLengthDelimited(stream, missionPoint, isDelta: false);
		return missionPoint;
	}

	public static MissionPoint DeserializeLength(BufferStream stream, int length)
	{
		MissionPoint missionPoint = Pool.Get<MissionPoint>();
		DeserializeLength(stream, length, missionPoint, isDelta: false);
		return missionPoint;
	}

	public static MissionPoint Deserialize(byte[] buffer)
	{
		MissionPoint missionPoint = Pool.Get<MissionPoint>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, missionPoint, isDelta: false);
		return missionPoint;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, MissionPoint previous)
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

	public static MissionPoint Deserialize(BufferStream stream, MissionPoint instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.MissionPoint");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MissionPoint");
				instance.identifier = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MissionPoint");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.location, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MissionPoint");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MissionPoint");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MissionPoint");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static MissionPoint DeserializeLengthDelimited(BufferStream stream, MissionPoint instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.MissionPoint");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MissionPoint");
				instance.identifier = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MissionPoint");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.location, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MissionPoint");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MissionPoint");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MissionPoint");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static MissionPoint DeserializeLength(BufferStream stream, int length, MissionPoint instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.MissionPoint");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MissionPoint");
				instance.identifier = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MissionPoint");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.location, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MissionPoint");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MissionPoint");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MissionPoint");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, MissionPoint instance, MissionPoint previous)
	{
		if (instance.identifier != previous.identifier)
		{
			if (instance.identifier == null)
			{
				throw new ArgumentNullException("identifier", "Required by proto specification.");
			}
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.identifier);
		}
		if (instance.location != previous.location)
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.location, previous.location);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field location (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public static void Serialize(BufferStream stream, MissionPoint instance)
	{
		if (instance.identifier == null)
		{
			throw new ArgumentNullException("identifier", "Required by proto specification.");
		}
		stream.WriteByte(10);
		ProtocolParser.WriteString(stream, instance.identifier);
		if (instance.location != default(Vector3))
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.Serialize(stream, instance.location);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field location (UnityEngine.Vector3)");
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
	}
}
