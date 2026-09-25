using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class RCEntity : IDisposable, Pool.IPooled, IProto<RCEntity>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public Vector3 aim;

	[NonSerialized]
	public float zoom;

	[NonSerialized]
	public string identifier;

	[NonSerialized]
	public string password;

	public static void ResetToPool(RCEntity instance)
	{
		if (instance.ShouldPool)
		{
			instance.aim = default(Vector3);
			instance.zoom = 0f;
			instance.identifier = string.Empty;
			instance.password = string.Empty;
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
			throw new Exception("Trying to dispose RCEntity with ShouldPool set to false!");
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

	public void CopyTo(RCEntity instance)
	{
		instance.aim = aim;
		instance.zoom = zoom;
		instance.identifier = identifier;
		instance.password = password;
	}

	public RCEntity Copy()
	{
		RCEntity rCEntity = Pool.Get<RCEntity>();
		CopyTo(rCEntity);
		return rCEntity;
	}

	public static RCEntity Deserialize(BufferStream stream)
	{
		RCEntity rCEntity = Pool.Get<RCEntity>();
		Deserialize(stream, rCEntity, isDelta: false);
		return rCEntity;
	}

	public static RCEntity DeserializeLengthDelimited(BufferStream stream)
	{
		RCEntity rCEntity = Pool.Get<RCEntity>();
		DeserializeLengthDelimited(stream, rCEntity, isDelta: false);
		return rCEntity;
	}

	public static RCEntity DeserializeLength(BufferStream stream, int length)
	{
		RCEntity rCEntity = Pool.Get<RCEntity>();
		DeserializeLength(stream, length, rCEntity, isDelta: false);
		return rCEntity;
	}

	public static RCEntity Deserialize(byte[] buffer)
	{
		RCEntity rCEntity = Pool.Get<RCEntity>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, rCEntity, isDelta: false);
		return rCEntity;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, RCEntity previous)
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

	public static RCEntity Deserialize(BufferStream stream, RCEntity instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.RCEntity");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.RCEntity");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.aim, isDelta);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.RCEntity");
				instance.zoom = ProtocolParser.ReadSingle(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.RCEntity");
				instance.identifier = ProtocolParser.ReadString(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.RCEntity");
				instance.password = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.RCEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.RCEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.RCEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.RCEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.RCEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static RCEntity DeserializeLengthDelimited(BufferStream stream, RCEntity instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.RCEntity");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.RCEntity");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.aim, isDelta);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.RCEntity");
				instance.zoom = ProtocolParser.ReadSingle(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.RCEntity");
				instance.identifier = ProtocolParser.ReadString(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.RCEntity");
				instance.password = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.RCEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.RCEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.RCEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.RCEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.RCEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static RCEntity DeserializeLength(BufferStream stream, int length, RCEntity instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.RCEntity");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.RCEntity");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.aim, isDelta);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.RCEntity");
				instance.zoom = ProtocolParser.ReadSingle(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.RCEntity");
				instance.identifier = ProtocolParser.ReadString(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.RCEntity");
				instance.password = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.RCEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.RCEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.RCEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.RCEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.RCEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, RCEntity instance, RCEntity previous)
	{
		if (instance.aim != previous.aim)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.aim, previous.aim);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field aim (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.zoom != previous.zoom)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.zoom);
		}
		if (instance.identifier != null && instance.identifier != previous.identifier)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteString(stream, instance.identifier);
		}
		if (instance.password != null && instance.password != previous.password)
		{
			stream.WriteByte(34);
			ProtocolParser.WriteString(stream, instance.password);
		}
	}

	public static void Serialize(BufferStream stream, RCEntity instance)
	{
		if (instance.aim != default(Vector3))
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.Serialize(stream, instance.aim);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field aim (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.zoom != 0f)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.zoom);
		}
		if (instance.identifier != null)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteString(stream, instance.identifier);
		}
		if (instance.password != null)
		{
			stream.WriteByte(34);
			ProtocolParser.WriteString(stream, instance.password);
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
