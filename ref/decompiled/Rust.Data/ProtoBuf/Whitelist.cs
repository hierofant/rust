using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class Whitelist : IDisposable, Pool.IPooled, IProto<Whitelist>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<ulong> users;

	public static void ResetToPool(Whitelist instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.users != null)
			{
				List<ulong> obj = instance.users;
				Pool.FreeUnmanaged(ref obj);
				instance.users = obj;
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
			throw new Exception("Trying to dispose Whitelist with ShouldPool set to false!");
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

	public void CopyTo(Whitelist instance)
	{
		if (users != null)
		{
			instance.users = Pool.Get<List<ulong>>();
			for (int i = 0; i < users.Count; i++)
			{
				ulong item = users[i];
				instance.users.Add(item);
			}
		}
		else
		{
			instance.users = null;
		}
	}

	public Whitelist Copy()
	{
		Whitelist whitelist = Pool.Get<Whitelist>();
		CopyTo(whitelist);
		return whitelist;
	}

	public static Whitelist Deserialize(BufferStream stream)
	{
		Whitelist whitelist = Pool.Get<Whitelist>();
		Deserialize(stream, whitelist, isDelta: false);
		return whitelist;
	}

	public static Whitelist DeserializeLengthDelimited(BufferStream stream)
	{
		Whitelist whitelist = Pool.Get<Whitelist>();
		DeserializeLengthDelimited(stream, whitelist, isDelta: false);
		return whitelist;
	}

	public static Whitelist DeserializeLength(BufferStream stream, int length)
	{
		Whitelist whitelist = Pool.Get<Whitelist>();
		DeserializeLength(stream, length, whitelist, isDelta: false);
		return whitelist;
	}

	public static Whitelist Deserialize(byte[] buffer)
	{
		Whitelist whitelist = Pool.Get<Whitelist>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, whitelist, isDelta: false);
		return whitelist;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, Whitelist previous)
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

	public static Whitelist Deserialize(BufferStream stream, Whitelist instance, bool isDelta)
	{
		if (!isDelta && instance.users == null)
		{
			instance.users = Pool.Get<List<ulong>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Whitelist");
			if (num == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Whitelist");
				stream.ConsumeRepeatedElement();
				instance.users.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Whitelist");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Whitelist");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static Whitelist DeserializeLengthDelimited(BufferStream stream, Whitelist instance, bool isDelta)
	{
		if (!isDelta && instance.users == null)
		{
			instance.users = Pool.Get<List<ulong>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Whitelist");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Whitelist");
				stream.ConsumeRepeatedElement();
				instance.users.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Whitelist");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Whitelist");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static Whitelist DeserializeLength(BufferStream stream, int length, Whitelist instance, bool isDelta)
	{
		if (!isDelta && instance.users == null)
		{
			instance.users = Pool.Get<List<ulong>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Whitelist");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Whitelist");
				stream.ConsumeRepeatedElement();
				instance.users.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Whitelist");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Whitelist");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, Whitelist instance, Whitelist previous)
	{
		if (instance.users != null)
		{
			for (int i = 0; i < instance.users.Count; i++)
			{
				ulong val = instance.users[i];
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, val);
			}
		}
	}

	public static void Serialize(BufferStream stream, Whitelist instance)
	{
		if (instance.users != null)
		{
			for (int i = 0; i < instance.users.Count; i++)
			{
				ulong val = instance.users[i];
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
