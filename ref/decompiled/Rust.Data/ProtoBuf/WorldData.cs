using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class WorldData : IDisposable, Pool.IPooled, IProto<WorldData>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public uint size;

	[NonSerialized]
	public List<MapData> maps;

	[NonSerialized]
	public List<PrefabData> prefabs;

	[NonSerialized]
	public List<PathData> paths;

	public static void ResetToPool(WorldData instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.size = 0u;
		if (instance.maps != null)
		{
			for (int i = 0; i < instance.maps.Count; i++)
			{
				if (instance.maps[i] != null)
				{
					instance.maps[i].ResetToPool();
					instance.maps[i] = null;
				}
			}
			List<MapData> obj = instance.maps;
			Pool.Free(ref obj, freeElements: false);
			instance.maps = obj;
		}
		if (instance.prefabs != null)
		{
			for (int j = 0; j < instance.prefabs.Count; j++)
			{
				if (instance.prefabs[j] != null)
				{
					instance.prefabs[j].ResetToPool();
					instance.prefabs[j] = null;
				}
			}
			List<PrefabData> obj2 = instance.prefabs;
			Pool.Free(ref obj2, freeElements: false);
			instance.prefabs = obj2;
		}
		if (instance.paths != null)
		{
			for (int k = 0; k < instance.paths.Count; k++)
			{
				if (instance.paths[k] != null)
				{
					instance.paths[k].ResetToPool();
					instance.paths[k] = null;
				}
			}
			List<PathData> obj3 = instance.paths;
			Pool.Free(ref obj3, freeElements: false);
			instance.paths = obj3;
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
			throw new Exception("Trying to dispose WorldData with ShouldPool set to false!");
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

	public void CopyTo(WorldData instance)
	{
		instance.size = size;
		if (maps != null)
		{
			instance.maps = Pool.Get<List<MapData>>();
			for (int i = 0; i < maps.Count; i++)
			{
				MapData item = maps[i].Copy();
				instance.maps.Add(item);
			}
		}
		else
		{
			instance.maps = null;
		}
		if (prefabs != null)
		{
			instance.prefabs = Pool.Get<List<PrefabData>>();
			for (int j = 0; j < prefabs.Count; j++)
			{
				PrefabData item2 = prefabs[j].Copy();
				instance.prefabs.Add(item2);
			}
		}
		else
		{
			instance.prefabs = null;
		}
		if (paths != null)
		{
			instance.paths = Pool.Get<List<PathData>>();
			for (int k = 0; k < paths.Count; k++)
			{
				PathData item3 = paths[k].Copy();
				instance.paths.Add(item3);
			}
		}
		else
		{
			instance.paths = null;
		}
	}

	public WorldData Copy()
	{
		WorldData worldData = Pool.Get<WorldData>();
		CopyTo(worldData);
		return worldData;
	}

	public static WorldData Deserialize(BufferStream stream)
	{
		WorldData worldData = Pool.Get<WorldData>();
		Deserialize(stream, worldData, isDelta: false);
		return worldData;
	}

	public static WorldData DeserializeLengthDelimited(BufferStream stream)
	{
		WorldData worldData = Pool.Get<WorldData>();
		DeserializeLengthDelimited(stream, worldData, isDelta: false);
		return worldData;
	}

	public static WorldData DeserializeLength(BufferStream stream, int length)
	{
		WorldData worldData = Pool.Get<WorldData>();
		DeserializeLength(stream, length, worldData, isDelta: false);
		return worldData;
	}

	public static WorldData Deserialize(byte[] buffer)
	{
		WorldData worldData = Pool.Get<WorldData>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, worldData, isDelta: false);
		return worldData;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, WorldData previous)
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

	public static WorldData Deserialize(BufferStream stream, WorldData instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.maps == null)
			{
				instance.maps = Pool.Get<List<MapData>>();
			}
			if (instance.prefabs == null)
			{
				instance.prefabs = Pool.Get<List<PrefabData>>();
			}
			if (instance.paths == null)
			{
				instance.paths = Pool.Get<List<PathData>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.WorldData");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WorldData");
				instance.size = ProtocolParser.ReadUInt32(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.WorldData");
				stream.ConsumeRepeatedElement();
				instance.maps.Add(MapData.DeserializeLengthDelimited(stream));
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.WorldData");
				stream.ConsumeRepeatedElement();
				instance.prefabs.Add(PrefabData.DeserializeLengthDelimited(stream));
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.WorldData");
				stream.ConsumeRepeatedElement();
				instance.paths.Add(PathData.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WorldData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.WorldData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.WorldData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.WorldData");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.WorldData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static WorldData DeserializeLengthDelimited(BufferStream stream, WorldData instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.maps == null)
			{
				instance.maps = Pool.Get<List<MapData>>();
			}
			if (instance.prefabs == null)
			{
				instance.prefabs = Pool.Get<List<PrefabData>>();
			}
			if (instance.paths == null)
			{
				instance.paths = Pool.Get<List<PathData>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.WorldData");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WorldData");
				instance.size = ProtocolParser.ReadUInt32(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.WorldData");
				stream.ConsumeRepeatedElement();
				instance.maps.Add(MapData.DeserializeLengthDelimited(stream));
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.WorldData");
				stream.ConsumeRepeatedElement();
				instance.prefabs.Add(PrefabData.DeserializeLengthDelimited(stream));
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.WorldData");
				stream.ConsumeRepeatedElement();
				instance.paths.Add(PathData.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WorldData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.WorldData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.WorldData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.WorldData");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.WorldData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static WorldData DeserializeLength(BufferStream stream, int length, WorldData instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.maps == null)
			{
				instance.maps = Pool.Get<List<MapData>>();
			}
			if (instance.prefabs == null)
			{
				instance.prefabs = Pool.Get<List<PrefabData>>();
			}
			if (instance.paths == null)
			{
				instance.paths = Pool.Get<List<PathData>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.WorldData");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WorldData");
				instance.size = ProtocolParser.ReadUInt32(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.WorldData");
				stream.ConsumeRepeatedElement();
				instance.maps.Add(MapData.DeserializeLengthDelimited(stream));
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.WorldData");
				stream.ConsumeRepeatedElement();
				instance.prefabs.Add(PrefabData.DeserializeLengthDelimited(stream));
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.WorldData");
				stream.ConsumeRepeatedElement();
				instance.paths.Add(PathData.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WorldData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.WorldData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.WorldData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.WorldData");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.WorldData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, WorldData instance, WorldData previous)
	{
		if (instance.size != previous.size)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt32(stream, instance.size);
		}
		if (instance.maps != null)
		{
			for (int i = 0; i < instance.maps.Count; i++)
			{
				MapData mapData = instance.maps[i];
				stream.WriteByte(18);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				MapData.SerializeDelta(stream, mapData, mapData);
				int val = stream.Position - position;
				Span<byte> span = range.GetSpan();
				int num = ProtocolParser.WriteUInt32((uint)val, span, 0);
				if (num < 5)
				{
					span[num - 1] |= 128;
					while (num < 4)
					{
						span[num++] = 128;
					}
					span[4] = 0;
				}
			}
		}
		if (instance.prefabs != null)
		{
			for (int j = 0; j < instance.prefabs.Count; j++)
			{
				PrefabData prefabData = instance.prefabs[j];
				stream.WriteByte(26);
				BufferStream.RangeHandle range2 = stream.GetRange(5);
				int position2 = stream.Position;
				PrefabData.SerializeDelta(stream, prefabData, prefabData);
				int val2 = stream.Position - position2;
				Span<byte> span2 = range2.GetSpan();
				int num2 = ProtocolParser.WriteUInt32((uint)val2, span2, 0);
				if (num2 < 5)
				{
					span2[num2 - 1] |= 128;
					while (num2 < 4)
					{
						span2[num2++] = 128;
					}
					span2[4] = 0;
				}
			}
		}
		if (instance.paths == null)
		{
			return;
		}
		for (int k = 0; k < instance.paths.Count; k++)
		{
			PathData pathData = instance.paths[k];
			stream.WriteByte(34);
			BufferStream.RangeHandle range3 = stream.GetRange(5);
			int position3 = stream.Position;
			PathData.SerializeDelta(stream, pathData, pathData);
			int val3 = stream.Position - position3;
			Span<byte> span3 = range3.GetSpan();
			int num3 = ProtocolParser.WriteUInt32((uint)val3, span3, 0);
			if (num3 < 5)
			{
				span3[num3 - 1] |= 128;
				while (num3 < 4)
				{
					span3[num3++] = 128;
				}
				span3[4] = 0;
			}
		}
	}

	public static void Serialize(BufferStream stream, WorldData instance)
	{
		if (instance.size != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt32(stream, instance.size);
		}
		if (instance.maps != null)
		{
			for (int i = 0; i < instance.maps.Count; i++)
			{
				MapData instance2 = instance.maps[i];
				stream.WriteByte(18);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				MapData.Serialize(stream, instance2);
				int val = stream.Position - position;
				Span<byte> span = range.GetSpan();
				int num = ProtocolParser.WriteUInt32((uint)val, span, 0);
				if (num < 5)
				{
					span[num - 1] |= 128;
					while (num < 4)
					{
						span[num++] = 128;
					}
					span[4] = 0;
				}
			}
		}
		if (instance.prefabs != null)
		{
			for (int j = 0; j < instance.prefabs.Count; j++)
			{
				PrefabData instance3 = instance.prefabs[j];
				stream.WriteByte(26);
				BufferStream.RangeHandle range2 = stream.GetRange(5);
				int position2 = stream.Position;
				PrefabData.Serialize(stream, instance3);
				int val2 = stream.Position - position2;
				Span<byte> span2 = range2.GetSpan();
				int num2 = ProtocolParser.WriteUInt32((uint)val2, span2, 0);
				if (num2 < 5)
				{
					span2[num2 - 1] |= 128;
					while (num2 < 4)
					{
						span2[num2++] = 128;
					}
					span2[4] = 0;
				}
			}
		}
		if (instance.paths == null)
		{
			return;
		}
		for (int k = 0; k < instance.paths.Count; k++)
		{
			PathData instance4 = instance.paths[k];
			stream.WriteByte(34);
			BufferStream.RangeHandle range3 = stream.GetRange(5);
			int position3 = stream.Position;
			PathData.Serialize(stream, instance4);
			int val3 = stream.Position - position3;
			Span<byte> span3 = range3.GetSpan();
			int num3 = ProtocolParser.WriteUInt32((uint)val3, span3, 0);
			if (num3 < 5)
			{
				span3[num3 - 1] |= 128;
				while (num3 < 4)
				{
					span3[num3++] = 128;
				}
				span3[4] = 0;
			}
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (maps != null)
		{
			for (int i = 0; i < maps.Count; i++)
			{
				maps[i]?.InspectUids(action);
			}
		}
		if (prefabs != null)
		{
			for (int j = 0; j < prefabs.Count; j++)
			{
				prefabs[j]?.InspectUids(action);
			}
		}
		if (paths != null)
		{
			for (int k = 0; k < paths.Count; k++)
			{
				paths[k]?.InspectUids(action);
			}
		}
	}
}
