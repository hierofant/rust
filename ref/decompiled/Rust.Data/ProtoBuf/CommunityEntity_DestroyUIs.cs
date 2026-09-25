using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class CommunityEntity_DestroyUIs : IDisposable, Pool.IPooled, IProto<CommunityEntity_DestroyUIs>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<string> list;

	public static void ResetToPool(CommunityEntity_DestroyUIs instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.list != null)
			{
				List<string> obj = instance.list;
				Pool.FreeUnmanaged(ref obj);
				instance.list = obj;
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
			throw new Exception("Trying to dispose CommunityEntity_DestroyUIs with ShouldPool set to false!");
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

	public void CopyTo(CommunityEntity_DestroyUIs instance)
	{
		if (list != null)
		{
			instance.list = Pool.Get<List<string>>();
			for (int i = 0; i < list.Count; i++)
			{
				string item = list[i];
				instance.list.Add(item);
			}
		}
		else
		{
			instance.list = null;
		}
	}

	public CommunityEntity_DestroyUIs Copy()
	{
		CommunityEntity_DestroyUIs communityEntity_DestroyUIs = Pool.Get<CommunityEntity_DestroyUIs>();
		CopyTo(communityEntity_DestroyUIs);
		return communityEntity_DestroyUIs;
	}

	public static CommunityEntity_DestroyUIs Deserialize(BufferStream stream)
	{
		CommunityEntity_DestroyUIs communityEntity_DestroyUIs = Pool.Get<CommunityEntity_DestroyUIs>();
		Deserialize(stream, communityEntity_DestroyUIs, isDelta: false);
		return communityEntity_DestroyUIs;
	}

	public static CommunityEntity_DestroyUIs DeserializeLengthDelimited(BufferStream stream)
	{
		CommunityEntity_DestroyUIs communityEntity_DestroyUIs = Pool.Get<CommunityEntity_DestroyUIs>();
		DeserializeLengthDelimited(stream, communityEntity_DestroyUIs, isDelta: false);
		return communityEntity_DestroyUIs;
	}

	public static CommunityEntity_DestroyUIs DeserializeLength(BufferStream stream, int length)
	{
		CommunityEntity_DestroyUIs communityEntity_DestroyUIs = Pool.Get<CommunityEntity_DestroyUIs>();
		DeserializeLength(stream, length, communityEntity_DestroyUIs, isDelta: false);
		return communityEntity_DestroyUIs;
	}

	public static CommunityEntity_DestroyUIs Deserialize(byte[] buffer)
	{
		CommunityEntity_DestroyUIs communityEntity_DestroyUIs = Pool.Get<CommunityEntity_DestroyUIs>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, communityEntity_DestroyUIs, isDelta: false);
		return communityEntity_DestroyUIs;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, CommunityEntity_DestroyUIs previous)
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

	public static CommunityEntity_DestroyUIs Deserialize(BufferStream stream, CommunityEntity_DestroyUIs instance, bool isDelta)
	{
		if (!isDelta && instance.list == null)
		{
			instance.list = Pool.Get<List<string>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.CommunityEntity_DestroyUIs");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CommunityEntity_DestroyUIs");
				stream.ConsumeRepeatedElement();
				instance.list.Add(ProtocolParser.ReadString(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CommunityEntity_DestroyUIs");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CommunityEntity_DestroyUIs");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static CommunityEntity_DestroyUIs DeserializeLengthDelimited(BufferStream stream, CommunityEntity_DestroyUIs instance, bool isDelta)
	{
		if (!isDelta && instance.list == null)
		{
			instance.list = Pool.Get<List<string>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.CommunityEntity_DestroyUIs");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CommunityEntity_DestroyUIs");
				stream.ConsumeRepeatedElement();
				instance.list.Add(ProtocolParser.ReadString(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CommunityEntity_DestroyUIs");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CommunityEntity_DestroyUIs");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static CommunityEntity_DestroyUIs DeserializeLength(BufferStream stream, int length, CommunityEntity_DestroyUIs instance, bool isDelta)
	{
		if (!isDelta && instance.list == null)
		{
			instance.list = Pool.Get<List<string>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.CommunityEntity_DestroyUIs");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CommunityEntity_DestroyUIs");
				stream.ConsumeRepeatedElement();
				instance.list.Add(ProtocolParser.ReadString(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CommunityEntity_DestroyUIs");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CommunityEntity_DestroyUIs");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, CommunityEntity_DestroyUIs instance, CommunityEntity_DestroyUIs previous)
	{
		if (instance.list != null)
		{
			for (int i = 0; i < instance.list.Count; i++)
			{
				string val = instance.list[i];
				stream.WriteByte(10);
				ProtocolParser.WriteString(stream, val);
			}
		}
	}

	public static void Serialize(BufferStream stream, CommunityEntity_DestroyUIs instance)
	{
		if (instance.list != null)
		{
			for (int i = 0; i < instance.list.Count; i++)
			{
				string val = instance.list[i];
				stream.WriteByte(10);
				ProtocolParser.WriteString(stream, val);
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
