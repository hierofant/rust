using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class VectorList : IDisposable, Pool.IPooled, IProto<VectorList>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<Vector3> vectorPoints;

	public static void ResetToPool(VectorList instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.vectorPoints != null)
			{
				List<Vector3> obj = instance.vectorPoints;
				Pool.FreeUnmanaged(ref obj);
				instance.vectorPoints = obj;
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
			throw new Exception("Trying to dispose VectorList with ShouldPool set to false!");
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

	public void CopyTo(VectorList instance)
	{
		if (vectorPoints != null)
		{
			instance.vectorPoints = Pool.Get<List<Vector3>>();
			for (int i = 0; i < vectorPoints.Count; i++)
			{
				Vector3 item = vectorPoints[i];
				instance.vectorPoints.Add(item);
			}
		}
		else
		{
			instance.vectorPoints = null;
		}
	}

	public VectorList Copy()
	{
		VectorList vectorList = Pool.Get<VectorList>();
		CopyTo(vectorList);
		return vectorList;
	}

	public static VectorList Deserialize(BufferStream stream)
	{
		VectorList vectorList = Pool.Get<VectorList>();
		Deserialize(stream, vectorList, isDelta: false);
		return vectorList;
	}

	public static VectorList DeserializeLengthDelimited(BufferStream stream)
	{
		VectorList vectorList = Pool.Get<VectorList>();
		DeserializeLengthDelimited(stream, vectorList, isDelta: false);
		return vectorList;
	}

	public static VectorList DeserializeLength(BufferStream stream, int length)
	{
		VectorList vectorList = Pool.Get<VectorList>();
		DeserializeLength(stream, length, vectorList, isDelta: false);
		return vectorList;
	}

	public static VectorList Deserialize(byte[] buffer)
	{
		VectorList vectorList = Pool.Get<VectorList>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, vectorList, isDelta: false);
		return vectorList;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, VectorList previous)
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

	public static VectorList Deserialize(BufferStream stream, VectorList instance, bool isDelta)
	{
		if (!isDelta && instance.vectorPoints == null)
		{
			instance.vectorPoints = Pool.Get<List<Vector3>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.VectorList");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VectorList");
				stream.ConsumeRepeatedElement();
				Vector3 instance2 = default(Vector3);
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.vectorPoints.Add(instance2);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VectorList");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VectorList");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static VectorList DeserializeLengthDelimited(BufferStream stream, VectorList instance, bool isDelta)
	{
		if (!isDelta && instance.vectorPoints == null)
		{
			instance.vectorPoints = Pool.Get<List<Vector3>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.VectorList");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VectorList");
				stream.ConsumeRepeatedElement();
				Vector3 instance2 = default(Vector3);
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.vectorPoints.Add(instance2);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VectorList");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VectorList");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static VectorList DeserializeLength(BufferStream stream, int length, VectorList instance, bool isDelta)
	{
		if (!isDelta && instance.vectorPoints == null)
		{
			instance.vectorPoints = Pool.Get<List<Vector3>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.VectorList");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VectorList");
				stream.ConsumeRepeatedElement();
				Vector3 instance2 = default(Vector3);
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.vectorPoints.Add(instance2);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VectorList");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VectorList");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, VectorList instance, VectorList previous)
	{
		if (instance.vectorPoints == null)
		{
			return;
		}
		for (int i = 0; i < instance.vectorPoints.Count; i++)
		{
			Vector3 vector = instance.vectorPoints[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.SerializeDelta(stream, vector, vector);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field vectorPoints (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public static void Serialize(BufferStream stream, VectorList instance)
	{
		if (instance.vectorPoints == null)
		{
			return;
		}
		for (int i = 0; i < instance.vectorPoints.Count; i++)
		{
			Vector3 instance2 = instance.vectorPoints[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.Serialize(stream, instance2);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field vectorPoints (UnityEngine.Vector3)");
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
