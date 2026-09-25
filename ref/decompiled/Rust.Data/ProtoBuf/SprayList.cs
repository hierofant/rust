using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class SprayList : IDisposable, Pool.IPooled, IProto<SprayList>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<LinePoint> linePoints;

	public static void ResetToPool(SprayList instance)
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
			throw new Exception("Trying to dispose SprayList with ShouldPool set to false!");
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

	public void CopyTo(SprayList instance)
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
	}

	public SprayList Copy()
	{
		SprayList sprayList = Pool.Get<SprayList>();
		CopyTo(sprayList);
		return sprayList;
	}

	public static SprayList Deserialize(BufferStream stream)
	{
		SprayList sprayList = Pool.Get<SprayList>();
		Deserialize(stream, sprayList, isDelta: false);
		return sprayList;
	}

	public static SprayList DeserializeLengthDelimited(BufferStream stream)
	{
		SprayList sprayList = Pool.Get<SprayList>();
		DeserializeLengthDelimited(stream, sprayList, isDelta: false);
		return sprayList;
	}

	public static SprayList DeserializeLength(BufferStream stream, int length)
	{
		SprayList sprayList = Pool.Get<SprayList>();
		DeserializeLength(stream, length, sprayList, isDelta: false);
		return sprayList;
	}

	public static SprayList Deserialize(byte[] buffer)
	{
		SprayList sprayList = Pool.Get<SprayList>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, sprayList, isDelta: false);
		return sprayList;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, SprayList previous)
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

	public static SprayList Deserialize(BufferStream stream, SprayList instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.SprayList");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.SprayList");
				stream.ConsumeRepeatedElement();
				instance.linePoints.Add(LinePoint.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.SprayList");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SprayList");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static SprayList DeserializeLengthDelimited(BufferStream stream, SprayList instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.SprayList");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.SprayList");
				stream.ConsumeRepeatedElement();
				instance.linePoints.Add(LinePoint.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.SprayList");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SprayList");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static SprayList DeserializeLength(BufferStream stream, int length, SprayList instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.SprayList");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.SprayList");
				stream.ConsumeRepeatedElement();
				instance.linePoints.Add(LinePoint.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.SprayList");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SprayList");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, SprayList instance, SprayList previous)
	{
		if (instance.linePoints == null)
		{
			return;
		}
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

	public static void Serialize(BufferStream stream, SprayList instance)
	{
		if (instance.linePoints == null)
		{
			return;
		}
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
	}
}
