using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class WorldMessage : IDisposable, Pool.IPooled, IProto<WorldMessage>, IProto
{
	public enum MessageType
	{
		Request = 1,
		Receive,
		Done
	}

	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public MessageType status;

	[NonSerialized]
	public List<PrefabData> prefabs;

	[NonSerialized]
	public List<PathData> paths;

	public static void ResetToPool(WorldMessage instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.status = (MessageType)0;
		if (instance.prefabs != null)
		{
			for (int i = 0; i < instance.prefabs.Count; i++)
			{
				if (instance.prefabs[i] != null)
				{
					instance.prefabs[i].ResetToPool();
					instance.prefabs[i] = null;
				}
			}
			List<PrefabData> obj = instance.prefabs;
			Pool.Free(ref obj, freeElements: false);
			instance.prefabs = obj;
		}
		if (instance.paths != null)
		{
			for (int j = 0; j < instance.paths.Count; j++)
			{
				if (instance.paths[j] != null)
				{
					instance.paths[j].ResetToPool();
					instance.paths[j] = null;
				}
			}
			List<PathData> obj2 = instance.paths;
			Pool.Free(ref obj2, freeElements: false);
			instance.paths = obj2;
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
			throw new Exception("Trying to dispose WorldMessage with ShouldPool set to false!");
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

	public void CopyTo(WorldMessage instance)
	{
		instance.status = status;
		if (prefabs != null)
		{
			instance.prefabs = Pool.Get<List<PrefabData>>();
			for (int i = 0; i < prefabs.Count; i++)
			{
				PrefabData item = prefabs[i].Copy();
				instance.prefabs.Add(item);
			}
		}
		else
		{
			instance.prefabs = null;
		}
		if (paths != null)
		{
			instance.paths = Pool.Get<List<PathData>>();
			for (int j = 0; j < paths.Count; j++)
			{
				PathData item2 = paths[j].Copy();
				instance.paths.Add(item2);
			}
		}
		else
		{
			instance.paths = null;
		}
	}

	public WorldMessage Copy()
	{
		WorldMessage worldMessage = Pool.Get<WorldMessage>();
		CopyTo(worldMessage);
		return worldMessage;
	}

	public static WorldMessage Deserialize(BufferStream stream)
	{
		WorldMessage worldMessage = Pool.Get<WorldMessage>();
		Deserialize(stream, worldMessage, isDelta: false);
		return worldMessage;
	}

	public static WorldMessage DeserializeLengthDelimited(BufferStream stream)
	{
		WorldMessage worldMessage = Pool.Get<WorldMessage>();
		DeserializeLengthDelimited(stream, worldMessage, isDelta: false);
		return worldMessage;
	}

	public static WorldMessage DeserializeLength(BufferStream stream, int length)
	{
		WorldMessage worldMessage = Pool.Get<WorldMessage>();
		DeserializeLength(stream, length, worldMessage, isDelta: false);
		return worldMessage;
	}

	public static WorldMessage Deserialize(byte[] buffer)
	{
		WorldMessage worldMessage = Pool.Get<WorldMessage>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, worldMessage, isDelta: false);
		return worldMessage;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, WorldMessage previous)
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

	public static WorldMessage Deserialize(BufferStream stream, WorldMessage instance, bool isDelta)
	{
		if (!isDelta)
		{
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
			stream.ConsumeFieldOperation("ProtoBuf.WorldMessage");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WorldMessage");
				instance.status = (MessageType)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.WorldMessage");
				stream.ConsumeRepeatedElement();
				instance.prefabs.Add(PrefabData.DeserializeLengthDelimited(stream));
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.WorldMessage");
				stream.ConsumeRepeatedElement();
				instance.paths.Add(PathData.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WorldMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.WorldMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.WorldMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.WorldMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static WorldMessage DeserializeLengthDelimited(BufferStream stream, WorldMessage instance, bool isDelta)
	{
		if (!isDelta)
		{
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
			stream.ConsumeFieldOperation("ProtoBuf.WorldMessage");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WorldMessage");
				instance.status = (MessageType)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.WorldMessage");
				stream.ConsumeRepeatedElement();
				instance.prefabs.Add(PrefabData.DeserializeLengthDelimited(stream));
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.WorldMessage");
				stream.ConsumeRepeatedElement();
				instance.paths.Add(PathData.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WorldMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.WorldMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.WorldMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.WorldMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static WorldMessage DeserializeLength(BufferStream stream, int length, WorldMessage instance, bool isDelta)
	{
		if (!isDelta)
		{
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
			stream.ConsumeFieldOperation("ProtoBuf.WorldMessage");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WorldMessage");
				instance.status = (MessageType)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.WorldMessage");
				stream.ConsumeRepeatedElement();
				instance.prefabs.Add(PrefabData.DeserializeLengthDelimited(stream));
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.WorldMessage");
				stream.ConsumeRepeatedElement();
				instance.paths.Add(PathData.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WorldMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.WorldMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.WorldMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.WorldMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, WorldMessage instance, WorldMessage previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, (ulong)instance.status);
		if (instance.prefabs != null)
		{
			for (int i = 0; i < instance.prefabs.Count; i++)
			{
				PrefabData prefabData = instance.prefabs[i];
				stream.WriteByte(18);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				PrefabData.SerializeDelta(stream, prefabData, prefabData);
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
		if (instance.paths == null)
		{
			return;
		}
		for (int j = 0; j < instance.paths.Count; j++)
		{
			PathData pathData = instance.paths[j];
			stream.WriteByte(26);
			BufferStream.RangeHandle range2 = stream.GetRange(5);
			int position2 = stream.Position;
			PathData.SerializeDelta(stream, pathData, pathData);
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

	public static void Serialize(BufferStream stream, WorldMessage instance)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, (ulong)instance.status);
		if (instance.prefabs != null)
		{
			for (int i = 0; i < instance.prefabs.Count; i++)
			{
				PrefabData instance2 = instance.prefabs[i];
				stream.WriteByte(18);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				PrefabData.Serialize(stream, instance2);
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
		if (instance.paths == null)
		{
			return;
		}
		for (int j = 0; j < instance.paths.Count; j++)
		{
			PathData instance3 = instance.paths[j];
			stream.WriteByte(26);
			BufferStream.RangeHandle range2 = stream.GetRange(5);
			int position2 = stream.Position;
			PathData.Serialize(stream, instance3);
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

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (prefabs != null)
		{
			for (int i = 0; i < prefabs.Count; i++)
			{
				prefabs[i]?.InspectUids(action);
			}
		}
		if (paths != null)
		{
			for (int j = 0; j < paths.Count; j++)
			{
				paths[j]?.InspectUids(action);
			}
		}
	}
}
