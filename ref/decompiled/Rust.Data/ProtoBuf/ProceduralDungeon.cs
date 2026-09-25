using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class ProceduralDungeon : IDisposable, Pool.IPooled, IProto<ProceduralDungeon>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public uint seed;

	[NonSerialized]
	public NetworkableId exitPortalID;

	[NonSerialized]
	public Vector3 mapOffset;

	public static void ResetToPool(ProceduralDungeon instance)
	{
		if (instance.ShouldPool)
		{
			instance.seed = 0u;
			instance.exitPortalID = default(NetworkableId);
			instance.mapOffset = default(Vector3);
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
			throw new Exception("Trying to dispose ProceduralDungeon with ShouldPool set to false!");
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

	public void CopyTo(ProceduralDungeon instance)
	{
		instance.seed = seed;
		instance.exitPortalID = exitPortalID;
		instance.mapOffset = mapOffset;
	}

	public ProceduralDungeon Copy()
	{
		ProceduralDungeon proceduralDungeon = Pool.Get<ProceduralDungeon>();
		CopyTo(proceduralDungeon);
		return proceduralDungeon;
	}

	public static ProceduralDungeon Deserialize(BufferStream stream)
	{
		ProceduralDungeon proceduralDungeon = Pool.Get<ProceduralDungeon>();
		Deserialize(stream, proceduralDungeon, isDelta: false);
		return proceduralDungeon;
	}

	public static ProceduralDungeon DeserializeLengthDelimited(BufferStream stream)
	{
		ProceduralDungeon proceduralDungeon = Pool.Get<ProceduralDungeon>();
		DeserializeLengthDelimited(stream, proceduralDungeon, isDelta: false);
		return proceduralDungeon;
	}

	public static ProceduralDungeon DeserializeLength(BufferStream stream, int length)
	{
		ProceduralDungeon proceduralDungeon = Pool.Get<ProceduralDungeon>();
		DeserializeLength(stream, length, proceduralDungeon, isDelta: false);
		return proceduralDungeon;
	}

	public static ProceduralDungeon Deserialize(byte[] buffer)
	{
		ProceduralDungeon proceduralDungeon = Pool.Get<ProceduralDungeon>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, proceduralDungeon, isDelta: false);
		return proceduralDungeon;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ProceduralDungeon previous)
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

	public static ProceduralDungeon Deserialize(BufferStream stream, ProceduralDungeon instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ProceduralDungeon");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ProceduralDungeon");
				instance.seed = ProtocolParser.ReadUInt32(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ProceduralDungeon");
				instance.exitPortalID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ProceduralDungeon");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.mapOffset, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ProceduralDungeon");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ProceduralDungeon");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ProceduralDungeon");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ProceduralDungeon");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ProceduralDungeon DeserializeLengthDelimited(BufferStream stream, ProceduralDungeon instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ProceduralDungeon");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ProceduralDungeon");
				instance.seed = ProtocolParser.ReadUInt32(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ProceduralDungeon");
				instance.exitPortalID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ProceduralDungeon");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.mapOffset, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ProceduralDungeon");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ProceduralDungeon");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ProceduralDungeon");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ProceduralDungeon");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ProceduralDungeon DeserializeLength(BufferStream stream, int length, ProceduralDungeon instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ProceduralDungeon");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ProceduralDungeon");
				instance.seed = ProtocolParser.ReadUInt32(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ProceduralDungeon");
				instance.exitPortalID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ProceduralDungeon");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.mapOffset, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ProceduralDungeon");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ProceduralDungeon");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ProceduralDungeon");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ProceduralDungeon");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ProceduralDungeon instance, ProceduralDungeon previous)
	{
		if (instance.seed != previous.seed)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt32(stream, instance.seed);
		}
		stream.WriteByte(16);
		ProtocolParser.WriteUInt64(stream, instance.exitPortalID.Value);
		if (instance.mapOffset != previous.mapOffset)
		{
			stream.WriteByte(26);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.mapOffset, previous.mapOffset);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field mapOffset (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public static void Serialize(BufferStream stream, ProceduralDungeon instance)
	{
		if (instance.seed != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt32(stream, instance.seed);
		}
		if (instance.exitPortalID != default(NetworkableId))
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, instance.exitPortalID.Value);
		}
		if (instance.mapOffset != default(Vector3))
		{
			stream.WriteByte(26);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.Serialize(stream, instance.mapOffset);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field mapOffset (UnityEngine.Vector3)");
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
		action(UidType.NetworkableId, ref exitPortalID.Value);
	}
}
