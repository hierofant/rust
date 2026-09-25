using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class SAMSite : IDisposable, Pool.IPooled, IProto<SAMSite>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public Vector3 aimDir;

	public static void ResetToPool(SAMSite instance)
	{
		if (instance.ShouldPool)
		{
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
			throw new Exception("Trying to dispose SAMSite with ShouldPool set to false!");
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

	public void CopyTo(SAMSite instance)
	{
		instance.aimDir = aimDir;
	}

	public SAMSite Copy()
	{
		SAMSite sAMSite = Pool.Get<SAMSite>();
		CopyTo(sAMSite);
		return sAMSite;
	}

	public static SAMSite Deserialize(BufferStream stream)
	{
		SAMSite sAMSite = Pool.Get<SAMSite>();
		Deserialize(stream, sAMSite, isDelta: false);
		return sAMSite;
	}

	public static SAMSite DeserializeLengthDelimited(BufferStream stream)
	{
		SAMSite sAMSite = Pool.Get<SAMSite>();
		DeserializeLengthDelimited(stream, sAMSite, isDelta: false);
		return sAMSite;
	}

	public static SAMSite DeserializeLength(BufferStream stream, int length)
	{
		SAMSite sAMSite = Pool.Get<SAMSite>();
		DeserializeLength(stream, length, sAMSite, isDelta: false);
		return sAMSite;
	}

	public static SAMSite Deserialize(byte[] buffer)
	{
		SAMSite sAMSite = Pool.Get<SAMSite>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, sAMSite, isDelta: false);
		return sAMSite;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, SAMSite previous)
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

	public static SAMSite Deserialize(BufferStream stream, SAMSite instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.SAMSite");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SAMSite");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.aimDir, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SAMSite");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SAMSite");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static SAMSite DeserializeLengthDelimited(BufferStream stream, SAMSite instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.SAMSite");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SAMSite");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.aimDir, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SAMSite");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SAMSite");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static SAMSite DeserializeLength(BufferStream stream, int length, SAMSite instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.SAMSite");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SAMSite");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.aimDir, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SAMSite");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SAMSite");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, SAMSite instance, SAMSite previous)
	{
		if (instance.aimDir != previous.aimDir)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.aimDir, previous.aimDir);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field aimDir (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public static void Serialize(BufferStream stream, SAMSite instance)
	{
		if (instance.aimDir != default(Vector3))
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.Serialize(stream, instance.aimDir);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field aimDir (UnityEngine.Vector3)");
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
