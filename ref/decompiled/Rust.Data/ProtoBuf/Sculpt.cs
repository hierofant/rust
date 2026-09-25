using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class Sculpt : IDisposable, Pool.IPooled, IProto<Sculpt>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public Vector3 bounds;

	[NonSerialized]
	public ArraySegment<byte> data;

	[NonSerialized]
	public NetworkableId ownerId;

	public static void ResetToPool(Sculpt instance)
	{
		if (instance.ShouldPool)
		{
			instance.bounds = default(Vector3);
			if (instance.data.Array != null)
			{
				BufferStream.Shared.ArrayPool.Return(instance.data.Array);
			}
			instance.data = default(ArraySegment<byte>);
			instance.ownerId = default(NetworkableId);
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
			throw new Exception("Trying to dispose Sculpt with ShouldPool set to false!");
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

	public void CopyTo(Sculpt instance)
	{
		instance.bounds = bounds;
		if (data.Array == null)
		{
			instance.data = default(ArraySegment<byte>);
		}
		else
		{
			byte[] array = BufferStream.Shared.ArrayPool.Rent(data.Count);
			Array.Copy(data.Array, 0, array, 0, data.Count);
			instance.data = new ArraySegment<byte>(array, 0, data.Count);
		}
		instance.ownerId = ownerId;
	}

	public Sculpt Copy()
	{
		Sculpt sculpt = Pool.Get<Sculpt>();
		CopyTo(sculpt);
		return sculpt;
	}

	public static Sculpt Deserialize(BufferStream stream)
	{
		Sculpt sculpt = Pool.Get<Sculpt>();
		Deserialize(stream, sculpt, isDelta: false);
		return sculpt;
	}

	public static Sculpt DeserializeLengthDelimited(BufferStream stream)
	{
		Sculpt sculpt = Pool.Get<Sculpt>();
		DeserializeLengthDelimited(stream, sculpt, isDelta: false);
		return sculpt;
	}

	public static Sculpt DeserializeLength(BufferStream stream, int length)
	{
		Sculpt sculpt = Pool.Get<Sculpt>();
		DeserializeLength(stream, length, sculpt, isDelta: false);
		return sculpt;
	}

	public static Sculpt Deserialize(byte[] buffer)
	{
		Sculpt sculpt = Pool.Get<Sculpt>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, sculpt, isDelta: false);
		return sculpt;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, Sculpt previous)
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

	public static Sculpt Deserialize(BufferStream stream, Sculpt instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Sculpt");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Sculpt");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.bounds, isDelta);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Sculpt");
				instance.data = ProtocolParser.ReadPooledBytes(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Sculpt");
				instance.ownerId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Sculpt");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Sculpt");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Sculpt");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Sculpt");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Sculpt DeserializeLengthDelimited(BufferStream stream, Sculpt instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Sculpt");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Sculpt");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.bounds, isDelta);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Sculpt");
				instance.data = ProtocolParser.ReadPooledBytes(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Sculpt");
				instance.ownerId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Sculpt");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Sculpt");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Sculpt");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Sculpt");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Sculpt DeserializeLength(BufferStream stream, int length, Sculpt instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Sculpt");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Sculpt");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.bounds, isDelta);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Sculpt");
				instance.data = ProtocolParser.ReadPooledBytes(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Sculpt");
				instance.ownerId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Sculpt");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Sculpt");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Sculpt");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Sculpt");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, Sculpt instance, Sculpt previous)
	{
		if (instance.bounds != previous.bounds)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.bounds, previous.bounds);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field bounds (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.data.Array != null)
		{
			stream.WriteByte(18);
			ProtocolParser.WritePooledBytes(stream, instance.data);
		}
		stream.WriteByte(24);
		ProtocolParser.WriteUInt64(stream, instance.ownerId.Value);
	}

	public static void Serialize(BufferStream stream, Sculpt instance)
	{
		if (instance.bounds != default(Vector3))
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.Serialize(stream, instance.bounds);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field bounds (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.data.Array != null)
		{
			stream.WriteByte(18);
			ProtocolParser.WritePooledBytes(stream, instance.data);
		}
		if (instance.ownerId != default(NetworkableId))
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, instance.ownerId.Value);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref ownerId.Value);
	}
}
