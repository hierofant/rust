using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class VineTree : IDisposable, Pool.IPooled, IProto<VineTree>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<NetworkableId> spawnedVines;

	public static void ResetToPool(VineTree instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.spawnedVines != null)
			{
				List<NetworkableId> obj = instance.spawnedVines;
				Pool.FreeUnmanaged(ref obj);
				instance.spawnedVines = obj;
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
			throw new Exception("Trying to dispose VineTree with ShouldPool set to false!");
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

	public void CopyTo(VineTree instance)
	{
		if (spawnedVines != null)
		{
			instance.spawnedVines = Pool.Get<List<NetworkableId>>();
			for (int i = 0; i < spawnedVines.Count; i++)
			{
				NetworkableId item = spawnedVines[i];
				instance.spawnedVines.Add(item);
			}
		}
		else
		{
			instance.spawnedVines = null;
		}
	}

	public VineTree Copy()
	{
		VineTree vineTree = Pool.Get<VineTree>();
		CopyTo(vineTree);
		return vineTree;
	}

	public static VineTree Deserialize(BufferStream stream)
	{
		VineTree vineTree = Pool.Get<VineTree>();
		Deserialize(stream, vineTree, isDelta: false);
		return vineTree;
	}

	public static VineTree DeserializeLengthDelimited(BufferStream stream)
	{
		VineTree vineTree = Pool.Get<VineTree>();
		DeserializeLengthDelimited(stream, vineTree, isDelta: false);
		return vineTree;
	}

	public static VineTree DeserializeLength(BufferStream stream, int length)
	{
		VineTree vineTree = Pool.Get<VineTree>();
		DeserializeLength(stream, length, vineTree, isDelta: false);
		return vineTree;
	}

	public static VineTree Deserialize(byte[] buffer)
	{
		VineTree vineTree = Pool.Get<VineTree>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, vineTree, isDelta: false);
		return vineTree;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, VineTree previous)
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

	public static VineTree Deserialize(BufferStream stream, VineTree instance, bool isDelta)
	{
		if (!isDelta && instance.spawnedVines == null)
		{
			instance.spawnedVines = Pool.Get<List<NetworkableId>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.VineTree");
			if (num == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VineTree");
				stream.ConsumeRepeatedElement();
				instance.spawnedVines.Add(new NetworkableId(ProtocolParser.ReadUInt64(stream)));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VineTree");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VineTree");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static VineTree DeserializeLengthDelimited(BufferStream stream, VineTree instance, bool isDelta)
	{
		if (!isDelta && instance.spawnedVines == null)
		{
			instance.spawnedVines = Pool.Get<List<NetworkableId>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.VineTree");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VineTree");
				stream.ConsumeRepeatedElement();
				instance.spawnedVines.Add(new NetworkableId(ProtocolParser.ReadUInt64(stream)));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VineTree");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VineTree");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static VineTree DeserializeLength(BufferStream stream, int length, VineTree instance, bool isDelta)
	{
		if (!isDelta && instance.spawnedVines == null)
		{
			instance.spawnedVines = Pool.Get<List<NetworkableId>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.VineTree");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VineTree");
				stream.ConsumeRepeatedElement();
				instance.spawnedVines.Add(new NetworkableId(ProtocolParser.ReadUInt64(stream)));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VineTree");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VineTree");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, VineTree instance, VineTree previous)
	{
		if (instance.spawnedVines != null)
		{
			for (int i = 0; i < instance.spawnedVines.Count; i++)
			{
				NetworkableId networkableId = instance.spawnedVines[i];
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, networkableId.Value);
			}
		}
	}

	public static void Serialize(BufferStream stream, VineTree instance)
	{
		if (instance.spawnedVines != null)
		{
			for (int i = 0; i < instance.spawnedVines.Count; i++)
			{
				NetworkableId networkableId = instance.spawnedVines[i];
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, networkableId.Value);
			}
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (spawnedVines != null)
		{
			for (int i = 0; i < spawnedVines.Count; i++)
			{
				NetworkableId value = spawnedVines[i];
				action(UidType.NetworkableId, ref value.Value);
				spawnedVines[i] = value;
			}
		}
	}
}
