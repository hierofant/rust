using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class OceanPaths : IDisposable, Pool.IPooled, IProto<OceanPaths>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<Vector3> cargoPatrolPath;

	[NonSerialized]
	public List<VectorList> harborApproaches;

	public static void ResetToPool(OceanPaths instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.cargoPatrolPath != null)
		{
			List<Vector3> obj = instance.cargoPatrolPath;
			Pool.FreeUnmanaged(ref obj);
			instance.cargoPatrolPath = obj;
		}
		if (instance.harborApproaches != null)
		{
			for (int i = 0; i < instance.harborApproaches.Count; i++)
			{
				if (instance.harborApproaches[i] != null)
				{
					instance.harborApproaches[i].ResetToPool();
					instance.harborApproaches[i] = null;
				}
			}
			List<VectorList> obj2 = instance.harborApproaches;
			Pool.Free(ref obj2, freeElements: false);
			instance.harborApproaches = obj2;
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
			throw new Exception("Trying to dispose OceanPaths with ShouldPool set to false!");
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

	public void CopyTo(OceanPaths instance)
	{
		if (cargoPatrolPath != null)
		{
			instance.cargoPatrolPath = Pool.Get<List<Vector3>>();
			for (int i = 0; i < cargoPatrolPath.Count; i++)
			{
				Vector3 item = cargoPatrolPath[i];
				instance.cargoPatrolPath.Add(item);
			}
		}
		else
		{
			instance.cargoPatrolPath = null;
		}
		if (harborApproaches != null)
		{
			instance.harborApproaches = Pool.Get<List<VectorList>>();
			for (int j = 0; j < harborApproaches.Count; j++)
			{
				VectorList item2 = harborApproaches[j].Copy();
				instance.harborApproaches.Add(item2);
			}
		}
		else
		{
			instance.harborApproaches = null;
		}
	}

	public OceanPaths Copy()
	{
		OceanPaths oceanPaths = Pool.Get<OceanPaths>();
		CopyTo(oceanPaths);
		return oceanPaths;
	}

	public static OceanPaths Deserialize(BufferStream stream)
	{
		OceanPaths oceanPaths = Pool.Get<OceanPaths>();
		Deserialize(stream, oceanPaths, isDelta: false);
		return oceanPaths;
	}

	public static OceanPaths DeserializeLengthDelimited(BufferStream stream)
	{
		OceanPaths oceanPaths = Pool.Get<OceanPaths>();
		DeserializeLengthDelimited(stream, oceanPaths, isDelta: false);
		return oceanPaths;
	}

	public static OceanPaths DeserializeLength(BufferStream stream, int length)
	{
		OceanPaths oceanPaths = Pool.Get<OceanPaths>();
		DeserializeLength(stream, length, oceanPaths, isDelta: false);
		return oceanPaths;
	}

	public static OceanPaths Deserialize(byte[] buffer)
	{
		OceanPaths oceanPaths = Pool.Get<OceanPaths>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, oceanPaths, isDelta: false);
		return oceanPaths;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, OceanPaths previous)
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

	public static OceanPaths Deserialize(BufferStream stream, OceanPaths instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.cargoPatrolPath == null)
			{
				instance.cargoPatrolPath = Pool.Get<List<Vector3>>();
			}
			if (instance.harborApproaches == null)
			{
				instance.harborApproaches = Pool.Get<List<VectorList>>();
			}
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.OceanPaths");
			switch (num)
			{
			case 10:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.OceanPaths");
				stream.ConsumeRepeatedElement();
				Vector3 instance2 = default(Vector3);
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.cargoPatrolPath.Add(instance2);
				continue;
			}
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.OceanPaths");
				stream.ConsumeRepeatedElement();
				instance.harborApproaches.Add(VectorList.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.OceanPaths");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.OceanPaths");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.OceanPaths");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static OceanPaths DeserializeLengthDelimited(BufferStream stream, OceanPaths instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.cargoPatrolPath == null)
			{
				instance.cargoPatrolPath = Pool.Get<List<Vector3>>();
			}
			if (instance.harborApproaches == null)
			{
				instance.harborApproaches = Pool.Get<List<VectorList>>();
			}
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
			stream.ConsumeFieldOperation("ProtoBuf.OceanPaths");
			switch (num2)
			{
			case 10:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.OceanPaths");
				stream.ConsumeRepeatedElement();
				Vector3 instance2 = default(Vector3);
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.cargoPatrolPath.Add(instance2);
				continue;
			}
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.OceanPaths");
				stream.ConsumeRepeatedElement();
				instance.harborApproaches.Add(VectorList.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.OceanPaths");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.OceanPaths");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.OceanPaths");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static OceanPaths DeserializeLength(BufferStream stream, int length, OceanPaths instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.cargoPatrolPath == null)
			{
				instance.cargoPatrolPath = Pool.Get<List<Vector3>>();
			}
			if (instance.harborApproaches == null)
			{
				instance.harborApproaches = Pool.Get<List<VectorList>>();
			}
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
			stream.ConsumeFieldOperation("ProtoBuf.OceanPaths");
			switch (num2)
			{
			case 10:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.OceanPaths");
				stream.ConsumeRepeatedElement();
				Vector3 instance2 = default(Vector3);
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.cargoPatrolPath.Add(instance2);
				continue;
			}
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.OceanPaths");
				stream.ConsumeRepeatedElement();
				instance.harborApproaches.Add(VectorList.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.OceanPaths");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.OceanPaths");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.OceanPaths");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, OceanPaths instance, OceanPaths previous)
	{
		if (instance.cargoPatrolPath != null)
		{
			for (int i = 0; i < instance.cargoPatrolPath.Count; i++)
			{
				Vector3 vector = instance.cargoPatrolPath[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				Vector3Serialized.SerializeDelta(stream, vector, vector);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field cargoPatrolPath (UnityEngine.Vector3)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.harborApproaches == null)
		{
			return;
		}
		for (int j = 0; j < instance.harborApproaches.Count; j++)
		{
			VectorList vectorList = instance.harborApproaches[j];
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(3);
			int position2 = stream.Position;
			VectorList.SerializeDelta(stream, vectorList, vectorList);
			int num2 = stream.Position - position2;
			if (num2 > 2097151)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field harborApproaches (ProtoBuf.VectorList)");
			}
			Span<byte> span2 = range2.GetSpan();
			int num3 = ProtocolParser.WriteUInt32((uint)num2, span2, 0);
			if (num3 < 3)
			{
				span2[num3 - 1] |= 128;
				while (num3 < 2)
				{
					span2[num3++] = 128;
				}
				span2[2] = 0;
			}
		}
	}

	public static void Serialize(BufferStream stream, OceanPaths instance)
	{
		if (instance.cargoPatrolPath != null)
		{
			for (int i = 0; i < instance.cargoPatrolPath.Count; i++)
			{
				Vector3 instance2 = instance.cargoPatrolPath[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				Vector3Serialized.Serialize(stream, instance2);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field cargoPatrolPath (UnityEngine.Vector3)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.harborApproaches == null)
		{
			return;
		}
		for (int j = 0; j < instance.harborApproaches.Count; j++)
		{
			VectorList instance3 = instance.harborApproaches[j];
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(3);
			int position2 = stream.Position;
			VectorList.Serialize(stream, instance3);
			int num2 = stream.Position - position2;
			if (num2 > 2097151)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field harborApproaches (ProtoBuf.VectorList)");
			}
			Span<byte> span2 = range2.GetSpan();
			int num3 = ProtocolParser.WriteUInt32((uint)num2, span2, 0);
			if (num3 < 3)
			{
				span2[num3 - 1] |= 128;
				while (num3 < 2)
				{
					span2[num3++] = 128;
				}
				span2[2] = 0;
			}
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (harborApproaches != null)
		{
			for (int i = 0; i < harborApproaches.Count; i++)
			{
				harborApproaches[i]?.InspectUids(action);
			}
		}
	}
}
