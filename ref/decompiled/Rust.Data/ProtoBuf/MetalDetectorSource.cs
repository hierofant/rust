using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class MetalDetectorSource : IDisposable, Pool.IPooled, IProto<MetalDetectorSource>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<Vector3> spawnLocations;

	[NonSerialized]
	public float spawnRadius;

	public static void ResetToPool(MetalDetectorSource instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.spawnLocations != null)
			{
				List<Vector3> obj = instance.spawnLocations;
				Pool.FreeUnmanaged(ref obj);
				instance.spawnLocations = obj;
			}
			instance.spawnRadius = 0f;
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
			throw new Exception("Trying to dispose MetalDetectorSource with ShouldPool set to false!");
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

	public void CopyTo(MetalDetectorSource instance)
	{
		if (spawnLocations != null)
		{
			instance.spawnLocations = Pool.Get<List<Vector3>>();
			for (int i = 0; i < spawnLocations.Count; i++)
			{
				Vector3 item = spawnLocations[i];
				instance.spawnLocations.Add(item);
			}
		}
		else
		{
			instance.spawnLocations = null;
		}
		instance.spawnRadius = spawnRadius;
	}

	public MetalDetectorSource Copy()
	{
		MetalDetectorSource metalDetectorSource = Pool.Get<MetalDetectorSource>();
		CopyTo(metalDetectorSource);
		return metalDetectorSource;
	}

	public static MetalDetectorSource Deserialize(BufferStream stream)
	{
		MetalDetectorSource metalDetectorSource = Pool.Get<MetalDetectorSource>();
		Deserialize(stream, metalDetectorSource, isDelta: false);
		return metalDetectorSource;
	}

	public static MetalDetectorSource DeserializeLengthDelimited(BufferStream stream)
	{
		MetalDetectorSource metalDetectorSource = Pool.Get<MetalDetectorSource>();
		DeserializeLengthDelimited(stream, metalDetectorSource, isDelta: false);
		return metalDetectorSource;
	}

	public static MetalDetectorSource DeserializeLength(BufferStream stream, int length)
	{
		MetalDetectorSource metalDetectorSource = Pool.Get<MetalDetectorSource>();
		DeserializeLength(stream, length, metalDetectorSource, isDelta: false);
		return metalDetectorSource;
	}

	public static MetalDetectorSource Deserialize(byte[] buffer)
	{
		MetalDetectorSource metalDetectorSource = Pool.Get<MetalDetectorSource>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, metalDetectorSource, isDelta: false);
		return metalDetectorSource;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, MetalDetectorSource previous)
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

	public static MetalDetectorSource Deserialize(BufferStream stream, MetalDetectorSource instance, bool isDelta)
	{
		if (!isDelta && instance.spawnLocations == null)
		{
			instance.spawnLocations = Pool.Get<List<Vector3>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.MetalDetectorSource");
			switch (num)
			{
			case 10:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.MetalDetectorSource");
				stream.ConsumeRepeatedElement();
				Vector3 instance2 = default(Vector3);
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.spawnLocations.Add(instance2);
				continue;
			}
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MetalDetectorSource");
				instance.spawnRadius = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.MetalDetectorSource");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MetalDetectorSource");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MetalDetectorSource");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static MetalDetectorSource DeserializeLengthDelimited(BufferStream stream, MetalDetectorSource instance, bool isDelta)
	{
		if (!isDelta && instance.spawnLocations == null)
		{
			instance.spawnLocations = Pool.Get<List<Vector3>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.MetalDetectorSource");
			switch (num2)
			{
			case 10:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.MetalDetectorSource");
				stream.ConsumeRepeatedElement();
				Vector3 instance2 = default(Vector3);
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.spawnLocations.Add(instance2);
				continue;
			}
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MetalDetectorSource");
				instance.spawnRadius = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.MetalDetectorSource");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MetalDetectorSource");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MetalDetectorSource");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static MetalDetectorSource DeserializeLength(BufferStream stream, int length, MetalDetectorSource instance, bool isDelta)
	{
		if (!isDelta && instance.spawnLocations == null)
		{
			instance.spawnLocations = Pool.Get<List<Vector3>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.MetalDetectorSource");
			switch (num2)
			{
			case 10:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.MetalDetectorSource");
				stream.ConsumeRepeatedElement();
				Vector3 instance2 = default(Vector3);
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.spawnLocations.Add(instance2);
				continue;
			}
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MetalDetectorSource");
				instance.spawnRadius = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.MetalDetectorSource");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MetalDetectorSource");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MetalDetectorSource");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, MetalDetectorSource instance, MetalDetectorSource previous)
	{
		if (instance.spawnLocations != null)
		{
			for (int i = 0; i < instance.spawnLocations.Count; i++)
			{
				Vector3 vector = instance.spawnLocations[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				Vector3Serialized.SerializeDelta(stream, vector, vector);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field spawnLocations (UnityEngine.Vector3)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.spawnRadius != previous.spawnRadius)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.spawnRadius);
		}
	}

	public static void Serialize(BufferStream stream, MetalDetectorSource instance)
	{
		if (instance.spawnLocations != null)
		{
			for (int i = 0; i < instance.spawnLocations.Count; i++)
			{
				Vector3 instance2 = instance.spawnLocations[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				Vector3Serialized.Serialize(stream, instance2);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field spawnLocations (UnityEngine.Vector3)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.spawnRadius != 0f)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.spawnRadius);
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
