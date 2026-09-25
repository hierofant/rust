using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class Missions : IDisposable, Pool.IPooled, IProto<Missions>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<MissionInstance> missions;

	[NonSerialized]
	public int activeMission;

	public static void ResetToPool(Missions instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.missions != null)
		{
			for (int i = 0; i < instance.missions.Count; i++)
			{
				if (instance.missions[i] != null)
				{
					instance.missions[i].ResetToPool();
					instance.missions[i] = null;
				}
			}
			List<MissionInstance> obj = instance.missions;
			Pool.Free(ref obj, freeElements: false);
			instance.missions = obj;
		}
		instance.activeMission = 0;
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
			throw new Exception("Trying to dispose Missions with ShouldPool set to false!");
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

	public void CopyTo(Missions instance)
	{
		if (missions != null)
		{
			instance.missions = Pool.Get<List<MissionInstance>>();
			for (int i = 0; i < missions.Count; i++)
			{
				MissionInstance item = missions[i].Copy();
				instance.missions.Add(item);
			}
		}
		else
		{
			instance.missions = null;
		}
		instance.activeMission = activeMission;
	}

	public Missions Copy()
	{
		Missions missions = Pool.Get<Missions>();
		CopyTo(missions);
		return missions;
	}

	public static Missions Deserialize(BufferStream stream)
	{
		Missions missions = Pool.Get<Missions>();
		Deserialize(stream, missions, isDelta: false);
		return missions;
	}

	public static Missions DeserializeLengthDelimited(BufferStream stream)
	{
		Missions missions = Pool.Get<Missions>();
		DeserializeLengthDelimited(stream, missions, isDelta: false);
		return missions;
	}

	public static Missions DeserializeLength(BufferStream stream, int length)
	{
		Missions missions = Pool.Get<Missions>();
		DeserializeLength(stream, length, missions, isDelta: false);
		return missions;
	}

	public static Missions Deserialize(byte[] buffer)
	{
		Missions missions = Pool.Get<Missions>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, missions, isDelta: false);
		return missions;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, Missions previous)
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

	public static Missions Deserialize(BufferStream stream, Missions instance, bool isDelta)
	{
		if (!isDelta && instance.missions == null)
		{
			instance.missions = Pool.Get<List<MissionInstance>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Missions");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Missions");
				stream.ConsumeRepeatedElement();
				instance.missions.Add(MissionInstance.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Missions");
				instance.activeMission = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Missions");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Missions");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Missions");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Missions DeserializeLengthDelimited(BufferStream stream, Missions instance, bool isDelta)
	{
		if (!isDelta && instance.missions == null)
		{
			instance.missions = Pool.Get<List<MissionInstance>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Missions");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Missions");
				stream.ConsumeRepeatedElement();
				instance.missions.Add(MissionInstance.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Missions");
				instance.activeMission = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Missions");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Missions");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Missions");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Missions DeserializeLength(BufferStream stream, int length, Missions instance, bool isDelta)
	{
		if (!isDelta && instance.missions == null)
		{
			instance.missions = Pool.Get<List<MissionInstance>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Missions");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Missions");
				stream.ConsumeRepeatedElement();
				instance.missions.Add(MissionInstance.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Missions");
				instance.activeMission = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Missions");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Missions");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Missions");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, Missions instance, Missions previous)
	{
		if (instance.missions != null)
		{
			for (int i = 0; i < instance.missions.Count; i++)
			{
				MissionInstance missionInstance = instance.missions[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				MissionInstance.SerializeDelta(stream, missionInstance, missionInstance);
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
		if (instance.activeMission != previous.activeMission)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.activeMission);
		}
	}

	public static void Serialize(BufferStream stream, Missions instance)
	{
		if (instance.missions != null)
		{
			for (int i = 0; i < instance.missions.Count; i++)
			{
				MissionInstance instance2 = instance.missions[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				MissionInstance.Serialize(stream, instance2);
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
		if (instance.activeMission != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.activeMission);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (missions != null)
		{
			for (int i = 0; i < missions.Count; i++)
			{
				missions[i]?.InspectUids(action);
			}
		}
	}
}
