using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class SprayLine : IDisposable, Pool.IPooled, IProto<SprayLine>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<LinePoint> linePoints;

	[NonSerialized]
	public Vector3 colour;

	[NonSerialized]
	public float width;

	[NonSerialized]
	public NetworkableId editingPlayer;

	public static void ResetToPool(SprayLine instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.linePoints != null)
		{
			for (int i = 0; i < instance.linePoints.Count; i++)
			{
				if (instance.linePoints[i] != null)
				{
					instance.linePoints[i].ResetToPool();
					instance.linePoints[i] = null;
				}
			}
			List<LinePoint> obj = instance.linePoints;
			Pool.Free(ref obj, freeElements: false);
			instance.linePoints = obj;
		}
		instance.colour = default(Vector3);
		instance.width = 0f;
		instance.editingPlayer = default(NetworkableId);
		Pool.Free(ref instance);
	}

	public void ResetToPool()
	{
		ResetToPool(this);
	}

	public virtual void Dispose()
	{
		if (!ShouldPool)
		{
			throw new Exception("Trying to dispose SprayLine with ShouldPool set to false!");
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

	public void CopyTo(SprayLine instance)
	{
		if (linePoints != null)
		{
			instance.linePoints = Pool.Get<List<LinePoint>>();
			for (int i = 0; i < linePoints.Count; i++)
			{
				LinePoint item = linePoints[i].Copy();
				instance.linePoints.Add(item);
			}
		}
		else
		{
			instance.linePoints = null;
		}
		instance.colour = colour;
		instance.width = width;
		instance.editingPlayer = editingPlayer;
	}

	public SprayLine Copy()
	{
		SprayLine sprayLine = Pool.Get<SprayLine>();
		CopyTo(sprayLine);
		return sprayLine;
	}

	public static SprayLine Deserialize(BufferStream stream)
	{
		SprayLine sprayLine = Pool.Get<SprayLine>();
		Deserialize(stream, sprayLine, isDelta: false);
		return sprayLine;
	}

	public static SprayLine DeserializeLengthDelimited(BufferStream stream)
	{
		SprayLine sprayLine = Pool.Get<SprayLine>();
		DeserializeLengthDelimited(stream, sprayLine, isDelta: false);
		return sprayLine;
	}

	public static SprayLine DeserializeLength(BufferStream stream, int length)
	{
		SprayLine sprayLine = Pool.Get<SprayLine>();
		DeserializeLength(stream, length, sprayLine, isDelta: false);
		return sprayLine;
	}

	public static SprayLine Deserialize(byte[] buffer)
	{
		SprayLine sprayLine = Pool.Get<SprayLine>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, sprayLine, isDelta: false);
		return sprayLine;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, SprayLine previous)
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

	public static SprayLine Deserialize(BufferStream stream, SprayLine instance, bool isDelta)
	{
		if (!isDelta && instance.linePoints == null)
		{
			instance.linePoints = Pool.Get<List<LinePoint>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.SprayLine");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.SprayLine");
				stream.ConsumeRepeatedElement();
				instance.linePoints.Add(LinePoint.DeserializeLengthDelimited(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SprayLine");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.colour, isDelta);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SprayLine");
				instance.width = ProtocolParser.ReadSingle(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SprayLine");
				instance.editingPlayer = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.SprayLine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SprayLine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SprayLine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SprayLine");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SprayLine");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static SprayLine DeserializeLengthDelimited(BufferStream stream, SprayLine instance, bool isDelta)
	{
		if (!isDelta && instance.linePoints == null)
		{
			instance.linePoints = Pool.Get<List<LinePoint>>();
		}
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
			stream.ConsumeFieldOperation("ProtoBuf.SprayLine");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.SprayLine");
				stream.ConsumeRepeatedElement();
				instance.linePoints.Add(LinePoint.DeserializeLengthDelimited(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SprayLine");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.colour, isDelta);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SprayLine");
				instance.width = ProtocolParser.ReadSingle(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SprayLine");
				instance.editingPlayer = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.SprayLine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SprayLine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SprayLine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SprayLine");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SprayLine");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static SprayLine DeserializeLength(BufferStream stream, int length, SprayLine instance, bool isDelta)
	{
		if (!isDelta && instance.linePoints == null)
		{
			instance.linePoints = Pool.Get<List<LinePoint>>();
		}
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
			stream.ConsumeFieldOperation("ProtoBuf.SprayLine");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.SprayLine");
				stream.ConsumeRepeatedElement();
				instance.linePoints.Add(LinePoint.DeserializeLengthDelimited(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SprayLine");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.colour, isDelta);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SprayLine");
				instance.width = ProtocolParser.ReadSingle(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SprayLine");
				instance.editingPlayer = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.SprayLine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SprayLine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SprayLine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SprayLine");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SprayLine");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, SprayLine instance, SprayLine previous)
	{
		if (instance.linePoints != null)
		{
			for (int i = 0; i < instance.linePoints.Count; i++)
			{
				LinePoint linePoint = instance.linePoints[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				LinePoint.SerializeDelta(stream, linePoint, linePoint);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field linePoints (ProtoBuf.LinePoint)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.colour != previous.colour)
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.colour, previous.colour);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field colour (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.width != previous.width)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.width);
		}
		stream.WriteByte(32);
		ProtocolParser.WriteUInt64(stream, instance.editingPlayer.Value);
	}

	public static void Serialize(BufferStream stream, SprayLine instance)
	{
		if (instance.linePoints != null)
		{
			for (int i = 0; i < instance.linePoints.Count; i++)
			{
				LinePoint instance2 = instance.linePoints[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				LinePoint.Serialize(stream, instance2);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field linePoints (ProtoBuf.LinePoint)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.colour != default(Vector3))
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.colour);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field colour (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.width != 0f)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.width);
		}
		if (instance.editingPlayer != default(NetworkableId))
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, instance.editingPlayer.Value);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (linePoints != null)
		{
			for (int i = 0; i < linePoints.Count; i++)
			{
				linePoints[i]?.InspectUids(action);
			}
		}
		action(UidType.NetworkableId, ref editingPlayer.Value);
	}
}
