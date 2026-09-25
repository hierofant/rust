using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class BallistaGun : IDisposable, Pool.IPooled, IProto<BallistaGun>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public Magazine magazine;

	[NonSerialized]
	public float reloadProgress;

	[NonSerialized]
	public Vector3 aimDir;

	public static void ResetToPool(BallistaGun instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.magazine != null)
			{
				instance.magazine.ResetToPool();
				instance.magazine = null;
			}
			instance.reloadProgress = 0f;
			instance.aimDir = default(Vector3);
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
			throw new Exception("Trying to dispose BallistaGun with ShouldPool set to false!");
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

	public void CopyTo(BallistaGun instance)
	{
		if (magazine != null)
		{
			if (instance.magazine == null)
			{
				instance.magazine = magazine.Copy();
			}
			else
			{
				magazine.CopyTo(instance.magazine);
			}
		}
		else
		{
			instance.magazine = null;
		}
		instance.reloadProgress = reloadProgress;
		instance.aimDir = aimDir;
	}

	public BallistaGun Copy()
	{
		BallistaGun ballistaGun = Pool.Get<BallistaGun>();
		CopyTo(ballistaGun);
		return ballistaGun;
	}

	public static BallistaGun Deserialize(BufferStream stream)
	{
		BallistaGun ballistaGun = Pool.Get<BallistaGun>();
		Deserialize(stream, ballistaGun, isDelta: false);
		return ballistaGun;
	}

	public static BallistaGun DeserializeLengthDelimited(BufferStream stream)
	{
		BallistaGun ballistaGun = Pool.Get<BallistaGun>();
		DeserializeLengthDelimited(stream, ballistaGun, isDelta: false);
		return ballistaGun;
	}

	public static BallistaGun DeserializeLength(BufferStream stream, int length)
	{
		BallistaGun ballistaGun = Pool.Get<BallistaGun>();
		DeserializeLength(stream, length, ballistaGun, isDelta: false);
		return ballistaGun;
	}

	public static BallistaGun Deserialize(byte[] buffer)
	{
		BallistaGun ballistaGun = Pool.Get<BallistaGun>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, ballistaGun, isDelta: false);
		return ballistaGun;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, BallistaGun previous)
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

	public static BallistaGun Deserialize(BufferStream stream, BallistaGun instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.BallistaGun");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BallistaGun");
				if (instance.magazine == null)
				{
					instance.magazine = Magazine.DeserializeLengthDelimited(stream);
				}
				else
				{
					Magazine.DeserializeLengthDelimited(stream, instance.magazine, isDelta);
				}
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BallistaGun");
				instance.reloadProgress = ProtocolParser.ReadSingle(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BallistaGun");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.aimDir, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BallistaGun");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BallistaGun");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BallistaGun");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BallistaGun");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static BallistaGun DeserializeLengthDelimited(BufferStream stream, BallistaGun instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.BallistaGun");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BallistaGun");
				if (instance.magazine == null)
				{
					instance.magazine = Magazine.DeserializeLengthDelimited(stream);
				}
				else
				{
					Magazine.DeserializeLengthDelimited(stream, instance.magazine, isDelta);
				}
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BallistaGun");
				instance.reloadProgress = ProtocolParser.ReadSingle(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BallistaGun");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.aimDir, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BallistaGun");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BallistaGun");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BallistaGun");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BallistaGun");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static BallistaGun DeserializeLength(BufferStream stream, int length, BallistaGun instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.BallistaGun");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BallistaGun");
				if (instance.magazine == null)
				{
					instance.magazine = Magazine.DeserializeLengthDelimited(stream);
				}
				else
				{
					Magazine.DeserializeLengthDelimited(stream, instance.magazine, isDelta);
				}
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BallistaGun");
				instance.reloadProgress = ProtocolParser.ReadSingle(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BallistaGun");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.aimDir, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BallistaGun");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BallistaGun");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BallistaGun");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BallistaGun");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, BallistaGun instance, BallistaGun previous)
	{
		if (instance.magazine != null)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Magazine.SerializeDelta(stream, instance.magazine, previous.magazine);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field magazine (ProtoBuf.Magazine)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.reloadProgress != previous.reloadProgress)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.reloadProgress);
		}
		if (instance.aimDir != previous.aimDir)
		{
			stream.WriteByte(26);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.aimDir, previous.aimDir);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field aimDir (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
	}

	public static void Serialize(BufferStream stream, BallistaGun instance)
	{
		if (instance.magazine != null)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Magazine.Serialize(stream, instance.magazine);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field magazine (ProtoBuf.Magazine)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.reloadProgress != 0f)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.reloadProgress);
		}
		if (instance.aimDir != default(Vector3))
		{
			stream.WriteByte(26);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.aimDir);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field aimDir (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		magazine?.InspectUids(action);
	}
}
