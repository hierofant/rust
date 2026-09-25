using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class DisplayingBoxStorage : IDisposable, Pool.IPooled, IProto<DisplayingBoxStorage>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public NetworkableId privelegeEntityId;

	[NonSerialized]
	public List<float> resources;

	public static void ResetToPool(DisplayingBoxStorage instance)
	{
		if (instance.ShouldPool)
		{
			instance.privelegeEntityId = default(NetworkableId);
			if (instance.resources != null)
			{
				List<float> obj = instance.resources;
				Pool.FreeUnmanaged(ref obj);
				instance.resources = obj;
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
			throw new Exception("Trying to dispose DisplayingBoxStorage with ShouldPool set to false!");
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

	public void CopyTo(DisplayingBoxStorage instance)
	{
		instance.privelegeEntityId = privelegeEntityId;
		if (resources != null)
		{
			instance.resources = Pool.Get<List<float>>();
			for (int i = 0; i < resources.Count; i++)
			{
				float item = resources[i];
				instance.resources.Add(item);
			}
		}
		else
		{
			instance.resources = null;
		}
	}

	public DisplayingBoxStorage Copy()
	{
		DisplayingBoxStorage displayingBoxStorage = Pool.Get<DisplayingBoxStorage>();
		CopyTo(displayingBoxStorage);
		return displayingBoxStorage;
	}

	public static DisplayingBoxStorage Deserialize(BufferStream stream)
	{
		DisplayingBoxStorage displayingBoxStorage = Pool.Get<DisplayingBoxStorage>();
		Deserialize(stream, displayingBoxStorage, isDelta: false);
		return displayingBoxStorage;
	}

	public static DisplayingBoxStorage DeserializeLengthDelimited(BufferStream stream)
	{
		DisplayingBoxStorage displayingBoxStorage = Pool.Get<DisplayingBoxStorage>();
		DeserializeLengthDelimited(stream, displayingBoxStorage, isDelta: false);
		return displayingBoxStorage;
	}

	public static DisplayingBoxStorage DeserializeLength(BufferStream stream, int length)
	{
		DisplayingBoxStorage displayingBoxStorage = Pool.Get<DisplayingBoxStorage>();
		DeserializeLength(stream, length, displayingBoxStorage, isDelta: false);
		return displayingBoxStorage;
	}

	public static DisplayingBoxStorage Deserialize(byte[] buffer)
	{
		DisplayingBoxStorage displayingBoxStorage = Pool.Get<DisplayingBoxStorage>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, displayingBoxStorage, isDelta: false);
		return displayingBoxStorage;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, DisplayingBoxStorage previous)
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

	public static DisplayingBoxStorage Deserialize(BufferStream stream, DisplayingBoxStorage instance, bool isDelta)
	{
		if (!isDelta && instance.resources == null)
		{
			instance.resources = Pool.Get<List<float>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.DisplayingBoxStorage");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DisplayingBoxStorage");
				instance.privelegeEntityId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.DisplayingBoxStorage");
				stream.ConsumeRepeatedElement();
				instance.resources.Add(ProtocolParser.ReadSingle(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DisplayingBoxStorage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.DisplayingBoxStorage");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DisplayingBoxStorage");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DisplayingBoxStorage DeserializeLengthDelimited(BufferStream stream, DisplayingBoxStorage instance, bool isDelta)
	{
		if (!isDelta && instance.resources == null)
		{
			instance.resources = Pool.Get<List<float>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.DisplayingBoxStorage");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DisplayingBoxStorage");
				instance.privelegeEntityId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.DisplayingBoxStorage");
				stream.ConsumeRepeatedElement();
				instance.resources.Add(ProtocolParser.ReadSingle(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DisplayingBoxStorage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.DisplayingBoxStorage");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DisplayingBoxStorage");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DisplayingBoxStorage DeserializeLength(BufferStream stream, int length, DisplayingBoxStorage instance, bool isDelta)
	{
		if (!isDelta && instance.resources == null)
		{
			instance.resources = Pool.Get<List<float>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.DisplayingBoxStorage");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DisplayingBoxStorage");
				instance.privelegeEntityId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.DisplayingBoxStorage");
				stream.ConsumeRepeatedElement();
				instance.resources.Add(ProtocolParser.ReadSingle(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DisplayingBoxStorage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.DisplayingBoxStorage");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DisplayingBoxStorage");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, DisplayingBoxStorage instance, DisplayingBoxStorage previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, instance.privelegeEntityId.Value);
		if (instance.resources != null)
		{
			for (int i = 0; i < instance.resources.Count; i++)
			{
				float f = instance.resources[i];
				stream.WriteByte(29);
				ProtocolParser.WriteSingle(stream, f);
			}
		}
	}

	public static void Serialize(BufferStream stream, DisplayingBoxStorage instance)
	{
		if (instance.privelegeEntityId != default(NetworkableId))
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.privelegeEntityId.Value);
		}
		if (instance.resources != null)
		{
			for (int i = 0; i < instance.resources.Count; i++)
			{
				float f = instance.resources[i];
				stream.WriteByte(29);
				ProtocolParser.WriteSingle(stream, f);
			}
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref privelegeEntityId.Value);
	}
}
