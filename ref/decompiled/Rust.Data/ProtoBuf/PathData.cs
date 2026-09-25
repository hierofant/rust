using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class PathData : IDisposable, Pool.IPooled, IProto<PathData>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public string name;

	[NonSerialized]
	public bool spline;

	[NonSerialized]
	public bool start;

	[NonSerialized]
	public bool end;

	[NonSerialized]
	public float width;

	[NonSerialized]
	public float innerPadding;

	[NonSerialized]
	public float outerPadding;

	[NonSerialized]
	public float innerFade;

	[NonSerialized]
	public float outerFade;

	[NonSerialized]
	public float randomScale;

	[NonSerialized]
	public float meshOffset;

	[NonSerialized]
	public float terrainOffset;

	[NonSerialized]
	public int splat;

	[NonSerialized]
	public int topology;

	[NonSerialized]
	public List<VectorData> nodes;

	[NonSerialized]
	public int hierarchy;

	public static void ResetToPool(PathData instance)
	{
		if (instance.ShouldPool)
		{
			instance.name = string.Empty;
			instance.spline = false;
			instance.start = false;
			instance.end = false;
			instance.width = 0f;
			instance.innerPadding = 0f;
			instance.outerPadding = 0f;
			instance.innerFade = 0f;
			instance.outerFade = 0f;
			instance.randomScale = 0f;
			instance.meshOffset = 0f;
			instance.terrainOffset = 0f;
			instance.splat = 0;
			instance.topology = 0;
			if (instance.nodes != null)
			{
				List<VectorData> obj = instance.nodes;
				Pool.FreeUnmanaged(ref obj);
				instance.nodes = obj;
			}
			instance.hierarchy = 0;
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
			throw new Exception("Trying to dispose PathData with ShouldPool set to false!");
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

	public void CopyTo(PathData instance)
	{
		instance.name = name;
		instance.spline = spline;
		instance.start = start;
		instance.end = end;
		instance.width = width;
		instance.innerPadding = innerPadding;
		instance.outerPadding = outerPadding;
		instance.innerFade = innerFade;
		instance.outerFade = outerFade;
		instance.randomScale = randomScale;
		instance.meshOffset = meshOffset;
		instance.terrainOffset = terrainOffset;
		instance.splat = splat;
		instance.topology = topology;
		if (nodes != null)
		{
			instance.nodes = Pool.Get<List<VectorData>>();
			for (int i = 0; i < nodes.Count; i++)
			{
				VectorData item = nodes[i];
				instance.nodes.Add(item);
			}
		}
		else
		{
			instance.nodes = null;
		}
		instance.hierarchy = hierarchy;
	}

	public PathData Copy()
	{
		PathData pathData = Pool.Get<PathData>();
		CopyTo(pathData);
		return pathData;
	}

	public static PathData Deserialize(BufferStream stream)
	{
		PathData pathData = Pool.Get<PathData>();
		Deserialize(stream, pathData, isDelta: false);
		return pathData;
	}

	public static PathData DeserializeLengthDelimited(BufferStream stream)
	{
		PathData pathData = Pool.Get<PathData>();
		DeserializeLengthDelimited(stream, pathData, isDelta: false);
		return pathData;
	}

	public static PathData DeserializeLength(BufferStream stream, int length)
	{
		PathData pathData = Pool.Get<PathData>();
		DeserializeLength(stream, length, pathData, isDelta: false);
		return pathData;
	}

	public static PathData Deserialize(byte[] buffer)
	{
		PathData pathData = Pool.Get<PathData>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, pathData, isDelta: false);
		return pathData;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, PathData previous)
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

	public static PathData Deserialize(BufferStream stream, PathData instance, bool isDelta)
	{
		if (!isDelta && instance.nodes == null)
		{
			instance.nodes = Pool.Get<List<VectorData>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.PathData");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.name = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.spline = ProtocolParser.ReadBool(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.start = ProtocolParser.ReadBool(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.end = ProtocolParser.ReadBool(stream);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.width = ProtocolParser.ReadSingle(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.innerPadding = ProtocolParser.ReadSingle(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.outerPadding = ProtocolParser.ReadSingle(stream);
				continue;
			case 69:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.innerFade = ProtocolParser.ReadSingle(stream);
				continue;
			case 77:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.outerFade = ProtocolParser.ReadSingle(stream);
				continue;
			case 85:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.randomScale = ProtocolParser.ReadSingle(stream);
				continue;
			case 93:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.meshOffset = ProtocolParser.ReadSingle(stream);
				continue;
			case 101:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.terrainOffset = ProtocolParser.ReadSingle(stream);
				continue;
			case 104:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.splat = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 112:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.topology = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 122:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: true, "ProtoBuf.PathData");
				stream.ConsumeRepeatedElement();
				VectorData instance2 = default(VectorData);
				VectorData.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.nodes.Add(instance2);
				continue;
			}
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: true, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.PathData");
				if (key.WireType == Wire.Varint)
				{
					instance.hierarchy = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PathData DeserializeLengthDelimited(BufferStream stream, PathData instance, bool isDelta)
	{
		if (!isDelta && instance.nodes == null)
		{
			instance.nodes = Pool.Get<List<VectorData>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.PathData");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.name = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.spline = ProtocolParser.ReadBool(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.start = ProtocolParser.ReadBool(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.end = ProtocolParser.ReadBool(stream);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.width = ProtocolParser.ReadSingle(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.innerPadding = ProtocolParser.ReadSingle(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.outerPadding = ProtocolParser.ReadSingle(stream);
				continue;
			case 69:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.innerFade = ProtocolParser.ReadSingle(stream);
				continue;
			case 77:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.outerFade = ProtocolParser.ReadSingle(stream);
				continue;
			case 85:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.randomScale = ProtocolParser.ReadSingle(stream);
				continue;
			case 93:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.meshOffset = ProtocolParser.ReadSingle(stream);
				continue;
			case 101:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.terrainOffset = ProtocolParser.ReadSingle(stream);
				continue;
			case 104:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.splat = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 112:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.topology = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 122:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: true, "ProtoBuf.PathData");
				stream.ConsumeRepeatedElement();
				VectorData instance2 = default(VectorData);
				VectorData.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.nodes.Add(instance2);
				continue;
			}
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: true, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.PathData");
				if (key.WireType == Wire.Varint)
				{
					instance.hierarchy = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PathData DeserializeLength(BufferStream stream, int length, PathData instance, bool isDelta)
	{
		if (!isDelta && instance.nodes == null)
		{
			instance.nodes = Pool.Get<List<VectorData>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.PathData");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.name = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.spline = ProtocolParser.ReadBool(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.start = ProtocolParser.ReadBool(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.end = ProtocolParser.ReadBool(stream);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.width = ProtocolParser.ReadSingle(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.innerPadding = ProtocolParser.ReadSingle(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.outerPadding = ProtocolParser.ReadSingle(stream);
				continue;
			case 69:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.innerFade = ProtocolParser.ReadSingle(stream);
				continue;
			case 77:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.outerFade = ProtocolParser.ReadSingle(stream);
				continue;
			case 85:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.randomScale = ProtocolParser.ReadSingle(stream);
				continue;
			case 93:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.meshOffset = ProtocolParser.ReadSingle(stream);
				continue;
			case 101:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.terrainOffset = ProtocolParser.ReadSingle(stream);
				continue;
			case 104:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.splat = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 112:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.PathData");
				instance.topology = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 122:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: true, "ProtoBuf.PathData");
				stream.ConsumeRepeatedElement();
				VectorData instance2 = default(VectorData);
				VectorData.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.nodes.Add(instance2);
				continue;
			}
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: true, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.PathData");
				if (key.WireType == Wire.Varint)
				{
					instance.hierarchy = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PathData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, PathData instance, PathData previous)
	{
		if (instance.name != previous.name)
		{
			if (instance.name == null)
			{
				throw new ArgumentNullException("name", "Required by proto specification.");
			}
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.name);
		}
		stream.WriteByte(16);
		ProtocolParser.WriteBool(stream, instance.spline);
		stream.WriteByte(24);
		ProtocolParser.WriteBool(stream, instance.start);
		stream.WriteByte(32);
		ProtocolParser.WriteBool(stream, instance.end);
		if (instance.width != previous.width)
		{
			stream.WriteByte(45);
			ProtocolParser.WriteSingle(stream, instance.width);
		}
		if (instance.innerPadding != previous.innerPadding)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.innerPadding);
		}
		if (instance.outerPadding != previous.outerPadding)
		{
			stream.WriteByte(61);
			ProtocolParser.WriteSingle(stream, instance.outerPadding);
		}
		if (instance.innerFade != previous.innerFade)
		{
			stream.WriteByte(69);
			ProtocolParser.WriteSingle(stream, instance.innerFade);
		}
		if (instance.outerFade != previous.outerFade)
		{
			stream.WriteByte(77);
			ProtocolParser.WriteSingle(stream, instance.outerFade);
		}
		if (instance.randomScale != previous.randomScale)
		{
			stream.WriteByte(85);
			ProtocolParser.WriteSingle(stream, instance.randomScale);
		}
		if (instance.meshOffset != previous.meshOffset)
		{
			stream.WriteByte(93);
			ProtocolParser.WriteSingle(stream, instance.meshOffset);
		}
		if (instance.terrainOffset != previous.terrainOffset)
		{
			stream.WriteByte(101);
			ProtocolParser.WriteSingle(stream, instance.terrainOffset);
		}
		if (instance.splat != previous.splat)
		{
			stream.WriteByte(104);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.splat);
		}
		if (instance.topology != previous.topology)
		{
			stream.WriteByte(112);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.topology);
		}
		if (instance.nodes != null)
		{
			for (int i = 0; i < instance.nodes.Count; i++)
			{
				VectorData vectorData = instance.nodes[i];
				stream.WriteByte(122);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				VectorData.SerializeDelta(stream, vectorData, vectorData);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field nodes (ProtoBuf.VectorData)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.hierarchy != previous.hierarchy)
		{
			stream.WriteByte(128);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.hierarchy);
		}
	}

	public static void Serialize(BufferStream stream, PathData instance)
	{
		if (instance.name == null)
		{
			throw new ArgumentNullException("name", "Required by proto specification.");
		}
		stream.WriteByte(10);
		ProtocolParser.WriteString(stream, instance.name);
		if (instance.spline)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteBool(stream, instance.spline);
		}
		if (instance.start)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteBool(stream, instance.start);
		}
		if (instance.end)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteBool(stream, instance.end);
		}
		if (instance.width != 0f)
		{
			stream.WriteByte(45);
			ProtocolParser.WriteSingle(stream, instance.width);
		}
		if (instance.innerPadding != 0f)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.innerPadding);
		}
		if (instance.outerPadding != 0f)
		{
			stream.WriteByte(61);
			ProtocolParser.WriteSingle(stream, instance.outerPadding);
		}
		if (instance.innerFade != 0f)
		{
			stream.WriteByte(69);
			ProtocolParser.WriteSingle(stream, instance.innerFade);
		}
		if (instance.outerFade != 0f)
		{
			stream.WriteByte(77);
			ProtocolParser.WriteSingle(stream, instance.outerFade);
		}
		if (instance.randomScale != 0f)
		{
			stream.WriteByte(85);
			ProtocolParser.WriteSingle(stream, instance.randomScale);
		}
		if (instance.meshOffset != 0f)
		{
			stream.WriteByte(93);
			ProtocolParser.WriteSingle(stream, instance.meshOffset);
		}
		if (instance.terrainOffset != 0f)
		{
			stream.WriteByte(101);
			ProtocolParser.WriteSingle(stream, instance.terrainOffset);
		}
		if (instance.splat != 0)
		{
			stream.WriteByte(104);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.splat);
		}
		if (instance.topology != 0)
		{
			stream.WriteByte(112);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.topology);
		}
		if (instance.nodes != null)
		{
			for (int i = 0; i < instance.nodes.Count; i++)
			{
				VectorData instance2 = instance.nodes[i];
				stream.WriteByte(122);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				VectorData.Serialize(stream, instance2);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field nodes (ProtoBuf.VectorData)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.hierarchy != 0)
		{
			stream.WriteByte(128);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.hierarchy);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (nodes != null)
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				nodes[i].InspectUids(action);
			}
		}
	}
}
