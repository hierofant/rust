using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class NavMeshData : IDisposable, Pool.IPooled, IProto<NavMeshData>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<VectorList> polygons;

	public static void ResetToPool(NavMeshData instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.polygons != null)
		{
			for (int i = 0; i < instance.polygons.Count; i++)
			{
				if (instance.polygons[i] != null)
				{
					instance.polygons[i].ResetToPool();
					instance.polygons[i] = null;
				}
			}
			List<VectorList> obj = instance.polygons;
			Pool.Free(ref obj, freeElements: false);
			instance.polygons = obj;
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
			throw new Exception("Trying to dispose NavMeshData with ShouldPool set to false!");
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

	public void CopyTo(NavMeshData instance)
	{
		if (polygons != null)
		{
			instance.polygons = Pool.Get<List<VectorList>>();
			for (int i = 0; i < polygons.Count; i++)
			{
				VectorList item = polygons[i].Copy();
				instance.polygons.Add(item);
			}
		}
		else
		{
			instance.polygons = null;
		}
	}

	public NavMeshData Copy()
	{
		NavMeshData navMeshData = Pool.Get<NavMeshData>();
		CopyTo(navMeshData);
		return navMeshData;
	}

	public static NavMeshData Deserialize(BufferStream stream)
	{
		NavMeshData navMeshData = Pool.Get<NavMeshData>();
		Deserialize(stream, navMeshData, isDelta: false);
		return navMeshData;
	}

	public static NavMeshData DeserializeLengthDelimited(BufferStream stream)
	{
		NavMeshData navMeshData = Pool.Get<NavMeshData>();
		DeserializeLengthDelimited(stream, navMeshData, isDelta: false);
		return navMeshData;
	}

	public static NavMeshData DeserializeLength(BufferStream stream, int length)
	{
		NavMeshData navMeshData = Pool.Get<NavMeshData>();
		DeserializeLength(stream, length, navMeshData, isDelta: false);
		return navMeshData;
	}

	public static NavMeshData Deserialize(byte[] buffer)
	{
		NavMeshData navMeshData = Pool.Get<NavMeshData>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, navMeshData, isDelta: false);
		return navMeshData;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, NavMeshData previous)
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

	public static NavMeshData Deserialize(BufferStream stream, NavMeshData instance, bool isDelta)
	{
		if (!isDelta && instance.polygons == null)
		{
			instance.polygons = Pool.Get<List<VectorList>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.NavMeshData");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.NavMeshData");
				stream.ConsumeRepeatedElement();
				instance.polygons.Add(VectorList.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.NavMeshData");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.NavMeshData");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static NavMeshData DeserializeLengthDelimited(BufferStream stream, NavMeshData instance, bool isDelta)
	{
		if (!isDelta && instance.polygons == null)
		{
			instance.polygons = Pool.Get<List<VectorList>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.NavMeshData");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.NavMeshData");
				stream.ConsumeRepeatedElement();
				instance.polygons.Add(VectorList.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.NavMeshData");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.NavMeshData");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static NavMeshData DeserializeLength(BufferStream stream, int length, NavMeshData instance, bool isDelta)
	{
		if (!isDelta && instance.polygons == null)
		{
			instance.polygons = Pool.Get<List<VectorList>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.NavMeshData");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.NavMeshData");
				stream.ConsumeRepeatedElement();
				instance.polygons.Add(VectorList.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.NavMeshData");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.NavMeshData");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, NavMeshData instance, NavMeshData previous)
	{
		if (instance.polygons == null)
		{
			return;
		}
		for (int i = 0; i < instance.polygons.Count; i++)
		{
			VectorList vectorList = instance.polygons[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(3);
			int position = stream.Position;
			VectorList.SerializeDelta(stream, vectorList, vectorList);
			int num = stream.Position - position;
			if (num > 2097151)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field polygons (ProtoBuf.VectorList)");
			}
			Span<byte> span = range.GetSpan();
			int num2 = ProtocolParser.WriteUInt32((uint)num, span, 0);
			if (num2 < 3)
			{
				span[num2 - 1] |= 128;
				while (num2 < 2)
				{
					span[num2++] = 128;
				}
				span[2] = 0;
			}
		}
	}

	public static void Serialize(BufferStream stream, NavMeshData instance)
	{
		if (instance.polygons == null)
		{
			return;
		}
		for (int i = 0; i < instance.polygons.Count; i++)
		{
			VectorList instance2 = instance.polygons[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(3);
			int position = stream.Position;
			VectorList.Serialize(stream, instance2);
			int num = stream.Position - position;
			if (num > 2097151)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field polygons (ProtoBuf.VectorList)");
			}
			Span<byte> span = range.GetSpan();
			int num2 = ProtocolParser.WriteUInt32((uint)num, span, 0);
			if (num2 < 3)
			{
				span[num2 - 1] |= 128;
				while (num2 < 2)
				{
					span[num2++] = 128;
				}
				span[2] = 0;
			}
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (polygons != null)
		{
			for (int i = 0; i < polygons.Count; i++)
			{
				polygons[i]?.InspectUids(action);
			}
		}
	}
}
