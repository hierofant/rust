using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ZiplineArrivalPoint : IDisposable, Pool.IPooled, IProto<ZiplineArrivalPoint>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<VectorData> linePoints;

	public static void ResetToPool(ZiplineArrivalPoint instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.linePoints != null)
			{
				List<VectorData> obj = instance.linePoints;
				Pool.FreeUnmanaged(ref obj);
				instance.linePoints = obj;
			}
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
			throw new Exception("Trying to dispose ZiplineArrivalPoint with ShouldPool set to false!");
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

	public void CopyTo(ZiplineArrivalPoint instance)
	{
		if (linePoints != null)
		{
			instance.linePoints = Pool.Get<List<VectorData>>();
			for (int i = 0; i < linePoints.Count; i++)
			{
				VectorData item = linePoints[i];
				instance.linePoints.Add(item);
			}
		}
		else
		{
			instance.linePoints = null;
		}
	}

	public ZiplineArrivalPoint Copy()
	{
		ZiplineArrivalPoint ziplineArrivalPoint = Pool.Get<ZiplineArrivalPoint>();
		CopyTo(ziplineArrivalPoint);
		return ziplineArrivalPoint;
	}

	public static ZiplineArrivalPoint Deserialize(BufferStream stream)
	{
		ZiplineArrivalPoint ziplineArrivalPoint = Pool.Get<ZiplineArrivalPoint>();
		Deserialize(stream, ziplineArrivalPoint, isDelta: false);
		return ziplineArrivalPoint;
	}

	public static ZiplineArrivalPoint DeserializeLengthDelimited(BufferStream stream)
	{
		ZiplineArrivalPoint ziplineArrivalPoint = Pool.Get<ZiplineArrivalPoint>();
		DeserializeLengthDelimited(stream, ziplineArrivalPoint, isDelta: false);
		return ziplineArrivalPoint;
	}

	public static ZiplineArrivalPoint DeserializeLength(BufferStream stream, int length)
	{
		ZiplineArrivalPoint ziplineArrivalPoint = Pool.Get<ZiplineArrivalPoint>();
		DeserializeLength(stream, length, ziplineArrivalPoint, isDelta: false);
		return ziplineArrivalPoint;
	}

	public static ZiplineArrivalPoint Deserialize(byte[] buffer)
	{
		ZiplineArrivalPoint ziplineArrivalPoint = Pool.Get<ZiplineArrivalPoint>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, ziplineArrivalPoint, isDelta: false);
		return ziplineArrivalPoint;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ZiplineArrivalPoint previous)
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

	public static ZiplineArrivalPoint Deserialize(BufferStream stream, ZiplineArrivalPoint instance, bool isDelta)
	{
		if (!isDelta && instance.linePoints == null)
		{
			instance.linePoints = Pool.Get<List<VectorData>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ZiplineArrivalPoint");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ZiplineArrivalPoint");
				stream.ConsumeRepeatedElement();
				VectorData instance2 = default(VectorData);
				VectorData.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.linePoints.Add(instance2);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ZiplineArrivalPoint");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ZiplineArrivalPoint");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ZiplineArrivalPoint DeserializeLengthDelimited(BufferStream stream, ZiplineArrivalPoint instance, bool isDelta)
	{
		if (!isDelta && instance.linePoints == null)
		{
			instance.linePoints = Pool.Get<List<VectorData>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ZiplineArrivalPoint");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ZiplineArrivalPoint");
				stream.ConsumeRepeatedElement();
				VectorData instance2 = default(VectorData);
				VectorData.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.linePoints.Add(instance2);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ZiplineArrivalPoint");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ZiplineArrivalPoint");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ZiplineArrivalPoint DeserializeLength(BufferStream stream, int length, ZiplineArrivalPoint instance, bool isDelta)
	{
		if (!isDelta && instance.linePoints == null)
		{
			instance.linePoints = Pool.Get<List<VectorData>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ZiplineArrivalPoint");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ZiplineArrivalPoint");
				stream.ConsumeRepeatedElement();
				VectorData instance2 = default(VectorData);
				VectorData.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.linePoints.Add(instance2);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ZiplineArrivalPoint");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ZiplineArrivalPoint");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ZiplineArrivalPoint instance, ZiplineArrivalPoint previous)
	{
		if (instance.linePoints == null)
		{
			return;
		}
		for (int i = 0; i < instance.linePoints.Count; i++)
		{
			VectorData vectorData = instance.linePoints[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			VectorData.SerializeDelta(stream, vectorData, vectorData);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field linePoints (ProtoBuf.VectorData)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public static void Serialize(BufferStream stream, ZiplineArrivalPoint instance)
	{
		if (instance.linePoints == null)
		{
			return;
		}
		for (int i = 0; i < instance.linePoints.Count; i++)
		{
			VectorData instance2 = instance.linePoints[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			VectorData.Serialize(stream, instance2);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field linePoints (ProtoBuf.VectorData)");
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
				linePoints[i].InspectUids(action);
			}
		}
	}
}
