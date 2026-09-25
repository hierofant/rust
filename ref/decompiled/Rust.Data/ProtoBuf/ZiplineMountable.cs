using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ZiplineMountable : IDisposable, Pool.IPooled, IProto<ZiplineMountable>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<VectorData> linePoints;

	public static void ResetToPool(ZiplineMountable instance)
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
			throw new Exception("Trying to dispose ZiplineMountable with ShouldPool set to false!");
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

	public void CopyTo(ZiplineMountable instance)
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

	public ZiplineMountable Copy()
	{
		ZiplineMountable ziplineMountable = Pool.Get<ZiplineMountable>();
		CopyTo(ziplineMountable);
		return ziplineMountable;
	}

	public static ZiplineMountable Deserialize(BufferStream stream)
	{
		ZiplineMountable ziplineMountable = Pool.Get<ZiplineMountable>();
		Deserialize(stream, ziplineMountable, isDelta: false);
		return ziplineMountable;
	}

	public static ZiplineMountable DeserializeLengthDelimited(BufferStream stream)
	{
		ZiplineMountable ziplineMountable = Pool.Get<ZiplineMountable>();
		DeserializeLengthDelimited(stream, ziplineMountable, isDelta: false);
		return ziplineMountable;
	}

	public static ZiplineMountable DeserializeLength(BufferStream stream, int length)
	{
		ZiplineMountable ziplineMountable = Pool.Get<ZiplineMountable>();
		DeserializeLength(stream, length, ziplineMountable, isDelta: false);
		return ziplineMountable;
	}

	public static ZiplineMountable Deserialize(byte[] buffer)
	{
		ZiplineMountable ziplineMountable = Pool.Get<ZiplineMountable>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, ziplineMountable, isDelta: false);
		return ziplineMountable;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ZiplineMountable previous)
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

	public static ZiplineMountable Deserialize(BufferStream stream, ZiplineMountable instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ZiplineMountable");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ZiplineMountable");
				stream.ConsumeRepeatedElement();
				VectorData instance2 = default(VectorData);
				VectorData.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.linePoints.Add(instance2);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ZiplineMountable");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ZiplineMountable");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ZiplineMountable DeserializeLengthDelimited(BufferStream stream, ZiplineMountable instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ZiplineMountable");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ZiplineMountable");
				stream.ConsumeRepeatedElement();
				VectorData instance2 = default(VectorData);
				VectorData.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.linePoints.Add(instance2);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ZiplineMountable");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ZiplineMountable");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ZiplineMountable DeserializeLength(BufferStream stream, int length, ZiplineMountable instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ZiplineMountable");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ZiplineMountable");
				stream.ConsumeRepeatedElement();
				VectorData instance2 = default(VectorData);
				VectorData.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.linePoints.Add(instance2);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ZiplineMountable");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ZiplineMountable");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ZiplineMountable instance, ZiplineMountable previous)
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

	public static void Serialize(BufferStream stream, ZiplineMountable instance)
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
