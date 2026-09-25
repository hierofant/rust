using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ScientistBoatOilrigManager : IDisposable, Pool.IPooled, IProto<ScientistBoatOilrigManager>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<NetworkableId> boatIds;

	public static void ResetToPool(ScientistBoatOilrigManager instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.boatIds != null)
			{
				List<NetworkableId> obj = instance.boatIds;
				Pool.FreeUnmanaged(ref obj);
				instance.boatIds = obj;
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
			throw new Exception("Trying to dispose ScientistBoatOilrigManager with ShouldPool set to false!");
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

	public void CopyTo(ScientistBoatOilrigManager instance)
	{
		if (boatIds != null)
		{
			instance.boatIds = Pool.Get<List<NetworkableId>>();
			for (int i = 0; i < boatIds.Count; i++)
			{
				NetworkableId item = boatIds[i];
				instance.boatIds.Add(item);
			}
		}
		else
		{
			instance.boatIds = null;
		}
	}

	public ScientistBoatOilrigManager Copy()
	{
		ScientistBoatOilrigManager scientistBoatOilrigManager = Pool.Get<ScientistBoatOilrigManager>();
		CopyTo(scientistBoatOilrigManager);
		return scientistBoatOilrigManager;
	}

	public static ScientistBoatOilrigManager Deserialize(BufferStream stream)
	{
		ScientistBoatOilrigManager scientistBoatOilrigManager = Pool.Get<ScientistBoatOilrigManager>();
		Deserialize(stream, scientistBoatOilrigManager, isDelta: false);
		return scientistBoatOilrigManager;
	}

	public static ScientistBoatOilrigManager DeserializeLengthDelimited(BufferStream stream)
	{
		ScientistBoatOilrigManager scientistBoatOilrigManager = Pool.Get<ScientistBoatOilrigManager>();
		DeserializeLengthDelimited(stream, scientistBoatOilrigManager, isDelta: false);
		return scientistBoatOilrigManager;
	}

	public static ScientistBoatOilrigManager DeserializeLength(BufferStream stream, int length)
	{
		ScientistBoatOilrigManager scientistBoatOilrigManager = Pool.Get<ScientistBoatOilrigManager>();
		DeserializeLength(stream, length, scientistBoatOilrigManager, isDelta: false);
		return scientistBoatOilrigManager;
	}

	public static ScientistBoatOilrigManager Deserialize(byte[] buffer)
	{
		ScientistBoatOilrigManager scientistBoatOilrigManager = Pool.Get<ScientistBoatOilrigManager>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, scientistBoatOilrigManager, isDelta: false);
		return scientistBoatOilrigManager;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ScientistBoatOilrigManager previous)
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

	public static ScientistBoatOilrigManager Deserialize(BufferStream stream, ScientistBoatOilrigManager instance, bool isDelta)
	{
		if (!isDelta && instance.boatIds == null)
		{
			instance.boatIds = Pool.Get<List<NetworkableId>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ScientistBoatOilrigManager");
			if (num == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ScientistBoatOilrigManager");
				stream.ConsumeRepeatedElement();
				instance.boatIds.Add(new NetworkableId(ProtocolParser.ReadUInt64(stream)));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ScientistBoatOilrigManager");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ScientistBoatOilrigManager");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ScientistBoatOilrigManager DeserializeLengthDelimited(BufferStream stream, ScientistBoatOilrigManager instance, bool isDelta)
	{
		if (!isDelta && instance.boatIds == null)
		{
			instance.boatIds = Pool.Get<List<NetworkableId>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ScientistBoatOilrigManager");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ScientistBoatOilrigManager");
				stream.ConsumeRepeatedElement();
				instance.boatIds.Add(new NetworkableId(ProtocolParser.ReadUInt64(stream)));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ScientistBoatOilrigManager");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ScientistBoatOilrigManager");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ScientistBoatOilrigManager DeserializeLength(BufferStream stream, int length, ScientistBoatOilrigManager instance, bool isDelta)
	{
		if (!isDelta && instance.boatIds == null)
		{
			instance.boatIds = Pool.Get<List<NetworkableId>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ScientistBoatOilrigManager");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ScientistBoatOilrigManager");
				stream.ConsumeRepeatedElement();
				instance.boatIds.Add(new NetworkableId(ProtocolParser.ReadUInt64(stream)));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ScientistBoatOilrigManager");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ScientistBoatOilrigManager");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ScientistBoatOilrigManager instance, ScientistBoatOilrigManager previous)
	{
		if (instance.boatIds != null)
		{
			for (int i = 0; i < instance.boatIds.Count; i++)
			{
				NetworkableId networkableId = instance.boatIds[i];
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, networkableId.Value);
			}
		}
	}

	public static void Serialize(BufferStream stream, ScientistBoatOilrigManager instance)
	{
		if (instance.boatIds != null)
		{
			for (int i = 0; i < instance.boatIds.Count; i++)
			{
				NetworkableId networkableId = instance.boatIds[i];
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
		if (boatIds != null)
		{
			for (int i = 0; i < boatIds.Count; i++)
			{
				NetworkableId value = boatIds[i];
				action(UidType.NetworkableId, ref value.Value);
				boatIds[i] = value;
			}
		}
	}
}
