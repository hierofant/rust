using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class Elevator : IDisposable, Pool.IPooled, IProto<Elevator>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public int floor;

	[NonSerialized]
	public NetworkableId spawnedLift;

	[NonSerialized]
	public NetworkableId topFloor;

	[NonSerialized]
	public List<NetworkableId> floorList;

	[NonSerialized]
	public int floorDisplay;

	public static void ResetToPool(Elevator instance)
	{
		if (instance.ShouldPool)
		{
			instance.floor = 0;
			instance.spawnedLift = default(NetworkableId);
			instance.topFloor = default(NetworkableId);
			if (instance.floorList != null)
			{
				List<NetworkableId> obj = instance.floorList;
				Pool.FreeUnmanaged(ref obj);
				instance.floorList = obj;
			}
			instance.floorDisplay = 0;
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
			throw new Exception("Trying to dispose Elevator with ShouldPool set to false!");
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

	public void CopyTo(Elevator instance)
	{
		instance.floor = floor;
		instance.spawnedLift = spawnedLift;
		instance.topFloor = topFloor;
		if (floorList != null)
		{
			instance.floorList = Pool.Get<List<NetworkableId>>();
			for (int i = 0; i < floorList.Count; i++)
			{
				NetworkableId item = floorList[i];
				instance.floorList.Add(item);
			}
		}
		else
		{
			instance.floorList = null;
		}
		instance.floorDisplay = floorDisplay;
	}

	public Elevator Copy()
	{
		Elevator elevator = Pool.Get<Elevator>();
		CopyTo(elevator);
		return elevator;
	}

	public static Elevator Deserialize(BufferStream stream)
	{
		Elevator elevator = Pool.Get<Elevator>();
		Deserialize(stream, elevator, isDelta: false);
		return elevator;
	}

	public static Elevator DeserializeLengthDelimited(BufferStream stream)
	{
		Elevator elevator = Pool.Get<Elevator>();
		DeserializeLengthDelimited(stream, elevator, isDelta: false);
		return elevator;
	}

	public static Elevator DeserializeLength(BufferStream stream, int length)
	{
		Elevator elevator = Pool.Get<Elevator>();
		DeserializeLength(stream, length, elevator, isDelta: false);
		return elevator;
	}

	public static Elevator Deserialize(byte[] buffer)
	{
		Elevator elevator = Pool.Get<Elevator>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, elevator, isDelta: false);
		return elevator;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, Elevator previous)
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

	public static Elevator Deserialize(BufferStream stream, Elevator instance, bool isDelta)
	{
		if (!isDelta && instance.floorList == null)
		{
			instance.floorList = Pool.Get<List<NetworkableId>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Elevator");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Elevator");
				instance.floor = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Elevator");
				instance.spawnedLift = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Elevator");
				instance.topFloor = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.Elevator");
				stream.ConsumeRepeatedElement();
				instance.floorList.Add(new NetworkableId(ProtocolParser.ReadUInt64(stream)));
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Elevator");
				instance.floorDisplay = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Elevator");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Elevator");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Elevator");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.Elevator");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Elevator");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Elevator");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Elevator DeserializeLengthDelimited(BufferStream stream, Elevator instance, bool isDelta)
	{
		if (!isDelta && instance.floorList == null)
		{
			instance.floorList = Pool.Get<List<NetworkableId>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Elevator");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Elevator");
				instance.floor = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Elevator");
				instance.spawnedLift = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Elevator");
				instance.topFloor = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.Elevator");
				stream.ConsumeRepeatedElement();
				instance.floorList.Add(new NetworkableId(ProtocolParser.ReadUInt64(stream)));
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Elevator");
				instance.floorDisplay = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Elevator");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Elevator");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Elevator");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.Elevator");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Elevator");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Elevator");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Elevator DeserializeLength(BufferStream stream, int length, Elevator instance, bool isDelta)
	{
		if (!isDelta && instance.floorList == null)
		{
			instance.floorList = Pool.Get<List<NetworkableId>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Elevator");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Elevator");
				instance.floor = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Elevator");
				instance.spawnedLift = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Elevator");
				instance.topFloor = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.Elevator");
				stream.ConsumeRepeatedElement();
				instance.floorList.Add(new NetworkableId(ProtocolParser.ReadUInt64(stream)));
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Elevator");
				instance.floorDisplay = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Elevator");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Elevator");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Elevator");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.Elevator");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Elevator");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Elevator");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, Elevator instance, Elevator previous)
	{
		if (instance.floor != previous.floor)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.floor);
		}
		stream.WriteByte(16);
		ProtocolParser.WriteUInt64(stream, instance.spawnedLift.Value);
		stream.WriteByte(24);
		ProtocolParser.WriteUInt64(stream, instance.topFloor.Value);
		if (instance.floorList != null)
		{
			for (int i = 0; i < instance.floorList.Count; i++)
			{
				NetworkableId networkableId = instance.floorList[i];
				stream.WriteByte(32);
				ProtocolParser.WriteUInt64(stream, networkableId.Value);
			}
		}
		if (instance.floorDisplay != previous.floorDisplay)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.floorDisplay);
		}
	}

	public static void Serialize(BufferStream stream, Elevator instance)
	{
		if (instance.floor != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.floor);
		}
		if (instance.spawnedLift != default(NetworkableId))
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, instance.spawnedLift.Value);
		}
		if (instance.topFloor != default(NetworkableId))
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, instance.topFloor.Value);
		}
		if (instance.floorList != null)
		{
			for (int i = 0; i < instance.floorList.Count; i++)
			{
				NetworkableId networkableId = instance.floorList[i];
				stream.WriteByte(32);
				ProtocolParser.WriteUInt64(stream, networkableId.Value);
			}
		}
		if (instance.floorDisplay != 0)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.floorDisplay);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref spawnedLift.Value);
		action(UidType.NetworkableId, ref topFloor.Value);
		if (floorList != null)
		{
			for (int i = 0; i < floorList.Count; i++)
			{
				NetworkableId value = floorList[i];
				action(UidType.NetworkableId, ref value.Value);
				floorList[i] = value;
			}
		}
	}
}
