using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class TinCanAlarm : IDisposable, Pool.IPooled, IProto<TinCanAlarm>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public Vector3 endPoint;

	[NonSerialized]
	public int loadedAmmoItemDefId;

	public static void ResetToPool(TinCanAlarm instance)
	{
		if (instance.ShouldPool)
		{
			instance.endPoint = default(Vector3);
			instance.loadedAmmoItemDefId = 0;
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
			throw new Exception("Trying to dispose TinCanAlarm with ShouldPool set to false!");
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

	public void CopyTo(TinCanAlarm instance)
	{
		instance.endPoint = endPoint;
		instance.loadedAmmoItemDefId = loadedAmmoItemDefId;
	}

	public TinCanAlarm Copy()
	{
		TinCanAlarm tinCanAlarm = Pool.Get<TinCanAlarm>();
		CopyTo(tinCanAlarm);
		return tinCanAlarm;
	}

	public static TinCanAlarm Deserialize(BufferStream stream)
	{
		TinCanAlarm tinCanAlarm = Pool.Get<TinCanAlarm>();
		Deserialize(stream, tinCanAlarm, isDelta: false);
		return tinCanAlarm;
	}

	public static TinCanAlarm DeserializeLengthDelimited(BufferStream stream)
	{
		TinCanAlarm tinCanAlarm = Pool.Get<TinCanAlarm>();
		DeserializeLengthDelimited(stream, tinCanAlarm, isDelta: false);
		return tinCanAlarm;
	}

	public static TinCanAlarm DeserializeLength(BufferStream stream, int length)
	{
		TinCanAlarm tinCanAlarm = Pool.Get<TinCanAlarm>();
		DeserializeLength(stream, length, tinCanAlarm, isDelta: false);
		return tinCanAlarm;
	}

	public static TinCanAlarm Deserialize(byte[] buffer)
	{
		TinCanAlarm tinCanAlarm = Pool.Get<TinCanAlarm>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, tinCanAlarm, isDelta: false);
		return tinCanAlarm;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, TinCanAlarm previous)
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

	public static TinCanAlarm Deserialize(BufferStream stream, TinCanAlarm instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.TinCanAlarm");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TinCanAlarm");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.endPoint, isDelta);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TinCanAlarm");
				instance.loadedAmmoItemDefId = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TinCanAlarm");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TinCanAlarm");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.TinCanAlarm");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static TinCanAlarm DeserializeLengthDelimited(BufferStream stream, TinCanAlarm instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.TinCanAlarm");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TinCanAlarm");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.endPoint, isDelta);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TinCanAlarm");
				instance.loadedAmmoItemDefId = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TinCanAlarm");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TinCanAlarm");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.TinCanAlarm");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static TinCanAlarm DeserializeLength(BufferStream stream, int length, TinCanAlarm instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.TinCanAlarm");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TinCanAlarm");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.endPoint, isDelta);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TinCanAlarm");
				instance.loadedAmmoItemDefId = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TinCanAlarm");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TinCanAlarm");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.TinCanAlarm");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, TinCanAlarm instance, TinCanAlarm previous)
	{
		if (instance.endPoint != previous.endPoint)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.endPoint, previous.endPoint);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field endPoint (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.loadedAmmoItemDefId != previous.loadedAmmoItemDefId)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.loadedAmmoItemDefId);
		}
	}

	public static void Serialize(BufferStream stream, TinCanAlarm instance)
	{
		if (instance.endPoint != default(Vector3))
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.Serialize(stream, instance.endPoint);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field endPoint (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.loadedAmmoItemDefId != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.loadedAmmoItemDefId);
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
