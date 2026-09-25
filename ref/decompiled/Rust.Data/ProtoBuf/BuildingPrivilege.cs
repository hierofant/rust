using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class BuildingPrivilege : IDisposable, Pool.IPooled, IProto<BuildingPrivilege>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<PlayerNameID> users;

	[NonSerialized]
	public float upkeepPeriodMinutes;

	[NonSerialized]
	public float costFraction;

	[NonSerialized]
	public float protectedMinutes;

	[NonSerialized]
	public bool clientAuthed;

	[NonSerialized]
	public bool clientAnyAuthed;

	[NonSerialized]
	public float doorCostFraction;

	[NonSerialized]
	public List<BuildingPrivilegeGroupMember> recentGroupMembers;

	[NonSerialized]
	public float groupUpkeepMultiplier;

	[NonSerialized]
	public int groupAuthCount;

	public static void ResetToPool(BuildingPrivilege instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.users != null)
		{
			for (int i = 0; i < instance.users.Count; i++)
			{
				if (instance.users[i] != null)
				{
					instance.users[i].ResetToPool();
					instance.users[i] = null;
				}
			}
			List<PlayerNameID> obj = instance.users;
			Pool.Free(ref obj, freeElements: false);
			instance.users = obj;
		}
		instance.upkeepPeriodMinutes = 0f;
		instance.costFraction = 0f;
		instance.protectedMinutes = 0f;
		instance.clientAuthed = false;
		instance.clientAnyAuthed = false;
		instance.doorCostFraction = 0f;
		if (instance.recentGroupMembers != null)
		{
			for (int j = 0; j < instance.recentGroupMembers.Count; j++)
			{
				if (instance.recentGroupMembers[j] != null)
				{
					instance.recentGroupMembers[j].ResetToPool();
					instance.recentGroupMembers[j] = null;
				}
			}
			List<BuildingPrivilegeGroupMember> obj2 = instance.recentGroupMembers;
			Pool.Free(ref obj2, freeElements: false);
			instance.recentGroupMembers = obj2;
		}
		instance.groupUpkeepMultiplier = 0f;
		instance.groupAuthCount = 0;
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
			throw new Exception("Trying to dispose BuildingPrivilege with ShouldPool set to false!");
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

	public void CopyTo(BuildingPrivilege instance)
	{
		if (users != null)
		{
			instance.users = Pool.Get<List<PlayerNameID>>();
			for (int i = 0; i < users.Count; i++)
			{
				PlayerNameID item = users[i].Copy();
				instance.users.Add(item);
			}
		}
		else
		{
			instance.users = null;
		}
		instance.upkeepPeriodMinutes = upkeepPeriodMinutes;
		instance.costFraction = costFraction;
		instance.protectedMinutes = protectedMinutes;
		instance.clientAuthed = clientAuthed;
		instance.clientAnyAuthed = clientAnyAuthed;
		instance.doorCostFraction = doorCostFraction;
		if (recentGroupMembers != null)
		{
			instance.recentGroupMembers = Pool.Get<List<BuildingPrivilegeGroupMember>>();
			for (int j = 0; j < recentGroupMembers.Count; j++)
			{
				BuildingPrivilegeGroupMember item2 = recentGroupMembers[j].Copy();
				instance.recentGroupMembers.Add(item2);
			}
		}
		else
		{
			instance.recentGroupMembers = null;
		}
		instance.groupUpkeepMultiplier = groupUpkeepMultiplier;
		instance.groupAuthCount = groupAuthCount;
	}

	public BuildingPrivilege Copy()
	{
		BuildingPrivilege buildingPrivilege = Pool.Get<BuildingPrivilege>();
		CopyTo(buildingPrivilege);
		return buildingPrivilege;
	}

	public static BuildingPrivilege Deserialize(BufferStream stream)
	{
		BuildingPrivilege buildingPrivilege = Pool.Get<BuildingPrivilege>();
		Deserialize(stream, buildingPrivilege, isDelta: false);
		return buildingPrivilege;
	}

	public static BuildingPrivilege DeserializeLengthDelimited(BufferStream stream)
	{
		BuildingPrivilege buildingPrivilege = Pool.Get<BuildingPrivilege>();
		DeserializeLengthDelimited(stream, buildingPrivilege, isDelta: false);
		return buildingPrivilege;
	}

	public static BuildingPrivilege DeserializeLength(BufferStream stream, int length)
	{
		BuildingPrivilege buildingPrivilege = Pool.Get<BuildingPrivilege>();
		DeserializeLength(stream, length, buildingPrivilege, isDelta: false);
		return buildingPrivilege;
	}

	public static BuildingPrivilege Deserialize(byte[] buffer)
	{
		BuildingPrivilege buildingPrivilege = Pool.Get<BuildingPrivilege>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, buildingPrivilege, isDelta: false);
		return buildingPrivilege;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, BuildingPrivilege previous)
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

	public static BuildingPrivilege Deserialize(BufferStream stream, BuildingPrivilege instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.users == null)
			{
				instance.users = Pool.Get<List<PlayerNameID>>();
			}
			if (instance.recentGroupMembers == null)
			{
				instance.recentGroupMembers = Pool.Get<List<BuildingPrivilegeGroupMember>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.BuildingPrivilege");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilege");
				stream.ConsumeRepeatedElement();
				instance.users.Add(PlayerNameID.DeserializeLengthDelimited(stream));
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				instance.upkeepPeriodMinutes = ProtocolParser.ReadSingle(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				instance.costFraction = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				instance.protectedMinutes = ProtocolParser.ReadSingle(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				instance.clientAuthed = ProtocolParser.ReadBool(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				instance.clientAnyAuthed = ProtocolParser.ReadBool(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				instance.doorCostFraction = ProtocolParser.ReadSingle(stream);
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilege");
				stream.ConsumeRepeatedElement();
				instance.recentGroupMembers.Add(BuildingPrivilegeGroupMember.DeserializeLengthDelimited(stream));
				continue;
			case 77:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				instance.groupUpkeepMultiplier = ProtocolParser.ReadSingle(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				instance.groupAuthCount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static BuildingPrivilege DeserializeLengthDelimited(BufferStream stream, BuildingPrivilege instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.users == null)
			{
				instance.users = Pool.Get<List<PlayerNameID>>();
			}
			if (instance.recentGroupMembers == null)
			{
				instance.recentGroupMembers = Pool.Get<List<BuildingPrivilegeGroupMember>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.BuildingPrivilege");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilege");
				stream.ConsumeRepeatedElement();
				instance.users.Add(PlayerNameID.DeserializeLengthDelimited(stream));
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				instance.upkeepPeriodMinutes = ProtocolParser.ReadSingle(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				instance.costFraction = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				instance.protectedMinutes = ProtocolParser.ReadSingle(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				instance.clientAuthed = ProtocolParser.ReadBool(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				instance.clientAnyAuthed = ProtocolParser.ReadBool(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				instance.doorCostFraction = ProtocolParser.ReadSingle(stream);
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilege");
				stream.ConsumeRepeatedElement();
				instance.recentGroupMembers.Add(BuildingPrivilegeGroupMember.DeserializeLengthDelimited(stream));
				continue;
			case 77:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				instance.groupUpkeepMultiplier = ProtocolParser.ReadSingle(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				instance.groupAuthCount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static BuildingPrivilege DeserializeLength(BufferStream stream, int length, BuildingPrivilege instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.users == null)
			{
				instance.users = Pool.Get<List<PlayerNameID>>();
			}
			if (instance.recentGroupMembers == null)
			{
				instance.recentGroupMembers = Pool.Get<List<BuildingPrivilegeGroupMember>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.BuildingPrivilege");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilege");
				stream.ConsumeRepeatedElement();
				instance.users.Add(PlayerNameID.DeserializeLengthDelimited(stream));
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				instance.upkeepPeriodMinutes = ProtocolParser.ReadSingle(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				instance.costFraction = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				instance.protectedMinutes = ProtocolParser.ReadSingle(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				instance.clientAuthed = ProtocolParser.ReadBool(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				instance.clientAnyAuthed = ProtocolParser.ReadBool(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				instance.doorCostFraction = ProtocolParser.ReadSingle(stream);
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilege");
				stream.ConsumeRepeatedElement();
				instance.recentGroupMembers.Add(BuildingPrivilegeGroupMember.DeserializeLengthDelimited(stream));
				continue;
			case 77:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				instance.groupUpkeepMultiplier = ProtocolParser.ReadSingle(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				instance.groupAuthCount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilege");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, BuildingPrivilege instance, BuildingPrivilege previous)
	{
		if (instance.users != null)
		{
			for (int i = 0; i < instance.users.Count; i++)
			{
				PlayerNameID playerNameID = instance.users[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				PlayerNameID.SerializeDelta(stream, playerNameID, playerNameID);
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
		if (instance.upkeepPeriodMinutes != previous.upkeepPeriodMinutes)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.upkeepPeriodMinutes);
		}
		if (instance.costFraction != previous.costFraction)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.costFraction);
		}
		if (instance.protectedMinutes != previous.protectedMinutes)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.protectedMinutes);
		}
		stream.WriteByte(40);
		ProtocolParser.WriteBool(stream, instance.clientAuthed);
		stream.WriteByte(48);
		ProtocolParser.WriteBool(stream, instance.clientAnyAuthed);
		if (instance.doorCostFraction != previous.doorCostFraction)
		{
			stream.WriteByte(61);
			ProtocolParser.WriteSingle(stream, instance.doorCostFraction);
		}
		if (instance.recentGroupMembers != null)
		{
			for (int j = 0; j < instance.recentGroupMembers.Count; j++)
			{
				BuildingPrivilegeGroupMember buildingPrivilegeGroupMember = instance.recentGroupMembers[j];
				stream.WriteByte(66);
				BufferStream.RangeHandle range2 = stream.GetRange(1);
				int position2 = stream.Position;
				BuildingPrivilegeGroupMember.SerializeDelta(stream, buildingPrivilegeGroupMember, buildingPrivilegeGroupMember);
				int num2 = stream.Position - position2;
				if (num2 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field recentGroupMembers (ProtoBuf.BuildingPrivilegeGroupMember)");
				}
				Span<byte> span2 = range2.GetSpan();
				ProtocolParser.WriteUInt32((uint)num2, span2, 0);
			}
		}
		if (instance.groupUpkeepMultiplier != previous.groupUpkeepMultiplier)
		{
			stream.WriteByte(77);
			ProtocolParser.WriteSingle(stream, instance.groupUpkeepMultiplier);
		}
		if (instance.groupAuthCount != previous.groupAuthCount)
		{
			stream.WriteByte(80);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.groupAuthCount);
		}
	}

	public static void Serialize(BufferStream stream, BuildingPrivilege instance)
	{
		if (instance.users != null)
		{
			for (int i = 0; i < instance.users.Count; i++)
			{
				PlayerNameID instance2 = instance.users[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				PlayerNameID.Serialize(stream, instance2);
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
		if (instance.upkeepPeriodMinutes != 0f)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.upkeepPeriodMinutes);
		}
		if (instance.costFraction != 0f)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.costFraction);
		}
		if (instance.protectedMinutes != 0f)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.protectedMinutes);
		}
		if (instance.clientAuthed)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteBool(stream, instance.clientAuthed);
		}
		if (instance.clientAnyAuthed)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteBool(stream, instance.clientAnyAuthed);
		}
		if (instance.doorCostFraction != 0f)
		{
			stream.WriteByte(61);
			ProtocolParser.WriteSingle(stream, instance.doorCostFraction);
		}
		if (instance.recentGroupMembers != null)
		{
			for (int j = 0; j < instance.recentGroupMembers.Count; j++)
			{
				BuildingPrivilegeGroupMember instance3 = instance.recentGroupMembers[j];
				stream.WriteByte(66);
				BufferStream.RangeHandle range2 = stream.GetRange(1);
				int position2 = stream.Position;
				BuildingPrivilegeGroupMember.Serialize(stream, instance3);
				int num2 = stream.Position - position2;
				if (num2 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field recentGroupMembers (ProtoBuf.BuildingPrivilegeGroupMember)");
				}
				Span<byte> span2 = range2.GetSpan();
				ProtocolParser.WriteUInt32((uint)num2, span2, 0);
			}
		}
		if (instance.groupUpkeepMultiplier != 0f)
		{
			stream.WriteByte(77);
			ProtocolParser.WriteSingle(stream, instance.groupUpkeepMultiplier);
		}
		if (instance.groupAuthCount != 0)
		{
			stream.WriteByte(80);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.groupAuthCount);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (users != null)
		{
			for (int i = 0; i < users.Count; i++)
			{
				users[i]?.InspectUids(action);
			}
		}
		if (recentGroupMembers != null)
		{
			for (int j = 0; j < recentGroupMembers.Count; j++)
			{
				recentGroupMembers[j]?.InspectUids(action);
			}
		}
	}
}
