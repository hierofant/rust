using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class Workbench : IDisposable, Pool.IPooled, IProto<Workbench>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<int> upgradeItemIds;

	public static void ResetToPool(Workbench instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.upgradeItemIds != null)
			{
				List<int> obj = instance.upgradeItemIds;
				Pool.FreeUnmanaged(ref obj);
				instance.upgradeItemIds = obj;
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
			throw new Exception("Trying to dispose Workbench with ShouldPool set to false!");
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

	public void CopyTo(Workbench instance)
	{
		if (upgradeItemIds != null)
		{
			instance.upgradeItemIds = Pool.Get<List<int>>();
			for (int i = 0; i < upgradeItemIds.Count; i++)
			{
				int item = upgradeItemIds[i];
				instance.upgradeItemIds.Add(item);
			}
		}
		else
		{
			instance.upgradeItemIds = null;
		}
	}

	public Workbench Copy()
	{
		Workbench workbench = Pool.Get<Workbench>();
		CopyTo(workbench);
		return workbench;
	}

	public static Workbench Deserialize(BufferStream stream)
	{
		Workbench workbench = Pool.Get<Workbench>();
		Deserialize(stream, workbench, isDelta: false);
		return workbench;
	}

	public static Workbench DeserializeLengthDelimited(BufferStream stream)
	{
		Workbench workbench = Pool.Get<Workbench>();
		DeserializeLengthDelimited(stream, workbench, isDelta: false);
		return workbench;
	}

	public static Workbench DeserializeLength(BufferStream stream, int length)
	{
		Workbench workbench = Pool.Get<Workbench>();
		DeserializeLength(stream, length, workbench, isDelta: false);
		return workbench;
	}

	public static Workbench Deserialize(byte[] buffer)
	{
		Workbench workbench = Pool.Get<Workbench>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, workbench, isDelta: false);
		return workbench;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, Workbench previous)
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

	public static Workbench Deserialize(BufferStream stream, Workbench instance, bool isDelta)
	{
		if (!isDelta && instance.upgradeItemIds == null)
		{
			instance.upgradeItemIds = Pool.Get<List<int>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Workbench");
			if (num == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Workbench");
				stream.ConsumeRepeatedElement();
				instance.upgradeItemIds.Add((int)ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Workbench");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Workbench");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static Workbench DeserializeLengthDelimited(BufferStream stream, Workbench instance, bool isDelta)
	{
		if (!isDelta && instance.upgradeItemIds == null)
		{
			instance.upgradeItemIds = Pool.Get<List<int>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Workbench");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Workbench");
				stream.ConsumeRepeatedElement();
				instance.upgradeItemIds.Add((int)ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Workbench");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Workbench");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static Workbench DeserializeLength(BufferStream stream, int length, Workbench instance, bool isDelta)
	{
		if (!isDelta && instance.upgradeItemIds == null)
		{
			instance.upgradeItemIds = Pool.Get<List<int>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Workbench");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Workbench");
				stream.ConsumeRepeatedElement();
				instance.upgradeItemIds.Add((int)ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Workbench");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Workbench");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, Workbench instance, Workbench previous)
	{
		if (instance.upgradeItemIds != null)
		{
			for (int i = 0; i < instance.upgradeItemIds.Count; i++)
			{
				int num = instance.upgradeItemIds[i];
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)num);
			}
		}
	}

	public static void Serialize(BufferStream stream, Workbench instance)
	{
		if (instance.upgradeItemIds != null)
		{
			for (int i = 0; i < instance.upgradeItemIds.Count; i++)
			{
				int num = instance.upgradeItemIds[i];
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)num);
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
