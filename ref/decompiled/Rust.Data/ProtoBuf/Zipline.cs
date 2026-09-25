using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class Zipline : IDisposable, Pool.IPooled, IProto<Zipline>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<VectorData> destinationPoints;

	public static void ResetToPool(Zipline instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.destinationPoints != null)
			{
				List<VectorData> obj = instance.destinationPoints;
				Pool.FreeUnmanaged(ref obj);
				instance.destinationPoints = obj;
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
			throw new Exception("Trying to dispose Zipline with ShouldPool set to false!");
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

	public void CopyTo(Zipline instance)
	{
		if (destinationPoints != null)
		{
			instance.destinationPoints = Pool.Get<List<VectorData>>();
			for (int i = 0; i < destinationPoints.Count; i++)
			{
				VectorData item = destinationPoints[i];
				instance.destinationPoints.Add(item);
			}
		}
		else
		{
			instance.destinationPoints = null;
		}
	}

	public Zipline Copy()
	{
		Zipline zipline = Pool.Get<Zipline>();
		CopyTo(zipline);
		return zipline;
	}

	public static Zipline Deserialize(BufferStream stream)
	{
		Zipline zipline = Pool.Get<Zipline>();
		Deserialize(stream, zipline, isDelta: false);
		return zipline;
	}

	public static Zipline DeserializeLengthDelimited(BufferStream stream)
	{
		Zipline zipline = Pool.Get<Zipline>();
		DeserializeLengthDelimited(stream, zipline, isDelta: false);
		return zipline;
	}

	public static Zipline DeserializeLength(BufferStream stream, int length)
	{
		Zipline zipline = Pool.Get<Zipline>();
		DeserializeLength(stream, length, zipline, isDelta: false);
		return zipline;
	}

	public static Zipline Deserialize(byte[] buffer)
	{
		Zipline zipline = Pool.Get<Zipline>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, zipline, isDelta: false);
		return zipline;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, Zipline previous)
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

	public static Zipline Deserialize(BufferStream stream, Zipline instance, bool isDelta)
	{
		if (!isDelta && instance.destinationPoints == null)
		{
			instance.destinationPoints = Pool.Get<List<VectorData>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Zipline");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Zipline");
				stream.ConsumeRepeatedElement();
				VectorData instance2 = default(VectorData);
				VectorData.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.destinationPoints.Add(instance2);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Zipline");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Zipline");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static Zipline DeserializeLengthDelimited(BufferStream stream, Zipline instance, bool isDelta)
	{
		if (!isDelta && instance.destinationPoints == null)
		{
			instance.destinationPoints = Pool.Get<List<VectorData>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Zipline");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Zipline");
				stream.ConsumeRepeatedElement();
				VectorData instance2 = default(VectorData);
				VectorData.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.destinationPoints.Add(instance2);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Zipline");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Zipline");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static Zipline DeserializeLength(BufferStream stream, int length, Zipline instance, bool isDelta)
	{
		if (!isDelta && instance.destinationPoints == null)
		{
			instance.destinationPoints = Pool.Get<List<VectorData>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Zipline");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Zipline");
				stream.ConsumeRepeatedElement();
				VectorData instance2 = default(VectorData);
				VectorData.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.destinationPoints.Add(instance2);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Zipline");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Zipline");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, Zipline instance, Zipline previous)
	{
		if (instance.destinationPoints == null)
		{
			return;
		}
		for (int i = 0; i < instance.destinationPoints.Count; i++)
		{
			VectorData vectorData = instance.destinationPoints[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			VectorData.SerializeDelta(stream, vectorData, vectorData);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field destinationPoints (ProtoBuf.VectorData)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public static void Serialize(BufferStream stream, Zipline instance)
	{
		if (instance.destinationPoints == null)
		{
			return;
		}
		for (int i = 0; i < instance.destinationPoints.Count; i++)
		{
			VectorData instance2 = instance.destinationPoints[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			VectorData.Serialize(stream, instance2);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field destinationPoints (ProtoBuf.VectorData)");
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
		if (destinationPoints != null)
		{
			for (int i = 0; i < destinationPoints.Count; i++)
			{
				destinationPoints[i].InspectUids(action);
			}
		}
	}
}
