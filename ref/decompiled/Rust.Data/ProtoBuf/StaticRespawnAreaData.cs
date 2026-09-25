using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class StaticRespawnAreaData : IDisposable, Pool.IPooled, IProto<StaticRespawnAreaData>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<ulong> authorizedUsers;

	public static void ResetToPool(StaticRespawnAreaData instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.authorizedUsers != null)
			{
				List<ulong> obj = instance.authorizedUsers;
				Pool.FreeUnmanaged(ref obj);
				instance.authorizedUsers = obj;
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
			throw new Exception("Trying to dispose StaticRespawnAreaData with ShouldPool set to false!");
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

	public void CopyTo(StaticRespawnAreaData instance)
	{
		if (authorizedUsers != null)
		{
			instance.authorizedUsers = Pool.Get<List<ulong>>();
			for (int i = 0; i < authorizedUsers.Count; i++)
			{
				ulong item = authorizedUsers[i];
				instance.authorizedUsers.Add(item);
			}
		}
		else
		{
			instance.authorizedUsers = null;
		}
	}

	public StaticRespawnAreaData Copy()
	{
		StaticRespawnAreaData staticRespawnAreaData = Pool.Get<StaticRespawnAreaData>();
		CopyTo(staticRespawnAreaData);
		return staticRespawnAreaData;
	}

	public static StaticRespawnAreaData Deserialize(BufferStream stream)
	{
		StaticRespawnAreaData staticRespawnAreaData = Pool.Get<StaticRespawnAreaData>();
		Deserialize(stream, staticRespawnAreaData, isDelta: false);
		return staticRespawnAreaData;
	}

	public static StaticRespawnAreaData DeserializeLengthDelimited(BufferStream stream)
	{
		StaticRespawnAreaData staticRespawnAreaData = Pool.Get<StaticRespawnAreaData>();
		DeserializeLengthDelimited(stream, staticRespawnAreaData, isDelta: false);
		return staticRespawnAreaData;
	}

	public static StaticRespawnAreaData DeserializeLength(BufferStream stream, int length)
	{
		StaticRespawnAreaData staticRespawnAreaData = Pool.Get<StaticRespawnAreaData>();
		DeserializeLength(stream, length, staticRespawnAreaData, isDelta: false);
		return staticRespawnAreaData;
	}

	public static StaticRespawnAreaData Deserialize(byte[] buffer)
	{
		StaticRespawnAreaData staticRespawnAreaData = Pool.Get<StaticRespawnAreaData>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, staticRespawnAreaData, isDelta: false);
		return staticRespawnAreaData;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, StaticRespawnAreaData previous)
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

	public static StaticRespawnAreaData Deserialize(BufferStream stream, StaticRespawnAreaData instance, bool isDelta)
	{
		if (!isDelta && instance.authorizedUsers == null)
		{
			instance.authorizedUsers = Pool.Get<List<ulong>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.StaticRespawnAreaData");
			if (num == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.StaticRespawnAreaData");
				stream.ConsumeRepeatedElement();
				instance.authorizedUsers.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.StaticRespawnAreaData");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.StaticRespawnAreaData");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static StaticRespawnAreaData DeserializeLengthDelimited(BufferStream stream, StaticRespawnAreaData instance, bool isDelta)
	{
		if (!isDelta && instance.authorizedUsers == null)
		{
			instance.authorizedUsers = Pool.Get<List<ulong>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.StaticRespawnAreaData");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.StaticRespawnAreaData");
				stream.ConsumeRepeatedElement();
				instance.authorizedUsers.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.StaticRespawnAreaData");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.StaticRespawnAreaData");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static StaticRespawnAreaData DeserializeLength(BufferStream stream, int length, StaticRespawnAreaData instance, bool isDelta)
	{
		if (!isDelta && instance.authorizedUsers == null)
		{
			instance.authorizedUsers = Pool.Get<List<ulong>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.StaticRespawnAreaData");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.StaticRespawnAreaData");
				stream.ConsumeRepeatedElement();
				instance.authorizedUsers.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.StaticRespawnAreaData");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.StaticRespawnAreaData");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, StaticRespawnAreaData instance, StaticRespawnAreaData previous)
	{
		if (instance.authorizedUsers != null)
		{
			for (int i = 0; i < instance.authorizedUsers.Count; i++)
			{
				ulong val = instance.authorizedUsers[i];
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, val);
			}
		}
	}

	public static void Serialize(BufferStream stream, StaticRespawnAreaData instance)
	{
		if (instance.authorizedUsers != null)
		{
			for (int i = 0; i < instance.authorizedUsers.Count; i++)
			{
				ulong val = instance.authorizedUsers[i];
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, val);
			}
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
