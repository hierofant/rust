using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class NPCTargetState : IDisposable, Pool.IPooled, IProto<NPCTargetState>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public Vector3 lookDirection;

	[NonSerialized]
	public float desiredSwimDepth;

	public static void ResetToPool(NPCTargetState instance)
	{
		if (instance.ShouldPool)
		{
			instance.lookDirection = default(Vector3);
			instance.desiredSwimDepth = 0f;
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
			throw new Exception("Trying to dispose NPCTargetState with ShouldPool set to false!");
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

	public void CopyTo(NPCTargetState instance)
	{
		instance.lookDirection = lookDirection;
		instance.desiredSwimDepth = desiredSwimDepth;
	}

	public NPCTargetState Copy()
	{
		NPCTargetState nPCTargetState = Pool.Get<NPCTargetState>();
		CopyTo(nPCTargetState);
		return nPCTargetState;
	}

	public static NPCTargetState Deserialize(BufferStream stream)
	{
		NPCTargetState nPCTargetState = Pool.Get<NPCTargetState>();
		Deserialize(stream, nPCTargetState, isDelta: false);
		return nPCTargetState;
	}

	public static NPCTargetState DeserializeLengthDelimited(BufferStream stream)
	{
		NPCTargetState nPCTargetState = Pool.Get<NPCTargetState>();
		DeserializeLengthDelimited(stream, nPCTargetState, isDelta: false);
		return nPCTargetState;
	}

	public static NPCTargetState DeserializeLength(BufferStream stream, int length)
	{
		NPCTargetState nPCTargetState = Pool.Get<NPCTargetState>();
		DeserializeLength(stream, length, nPCTargetState, isDelta: false);
		return nPCTargetState;
	}

	public static NPCTargetState Deserialize(byte[] buffer)
	{
		NPCTargetState nPCTargetState = Pool.Get<NPCTargetState>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, nPCTargetState, isDelta: false);
		return nPCTargetState;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, NPCTargetState previous)
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

	public static NPCTargetState Deserialize(BufferStream stream, NPCTargetState instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.NPCTargetState");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NPCTargetState");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.lookDirection, isDelta);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.NPCTargetState");
				instance.desiredSwimDepth = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NPCTargetState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.NPCTargetState");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.NPCTargetState");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static NPCTargetState DeserializeLengthDelimited(BufferStream stream, NPCTargetState instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.NPCTargetState");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NPCTargetState");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.lookDirection, isDelta);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.NPCTargetState");
				instance.desiredSwimDepth = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NPCTargetState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.NPCTargetState");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.NPCTargetState");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static NPCTargetState DeserializeLength(BufferStream stream, int length, NPCTargetState instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.NPCTargetState");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NPCTargetState");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.lookDirection, isDelta);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.NPCTargetState");
				instance.desiredSwimDepth = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NPCTargetState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.NPCTargetState");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.NPCTargetState");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, NPCTargetState instance, NPCTargetState previous)
	{
		if (instance.lookDirection != previous.lookDirection)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.lookDirection, previous.lookDirection);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field lookDirection (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.desiredSwimDepth != previous.desiredSwimDepth)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.desiredSwimDepth);
		}
	}

	public static void Serialize(BufferStream stream, NPCTargetState instance)
	{
		if (instance.lookDirection != default(Vector3))
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.Serialize(stream, instance.lookDirection);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field lookDirection (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.desiredSwimDepth != 0f)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.desiredSwimDepth);
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
