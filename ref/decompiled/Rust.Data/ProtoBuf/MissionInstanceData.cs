using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class MissionInstanceData : IDisposable, Pool.IPooled, IProto<MissionInstanceData>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public NetworkableId providerID;

	[NonSerialized]
	public List<ObjectiveStatus> objectiveStatuses;

	[NonSerialized]
	public List<MissionPoint> missionPoints;

	[NonSerialized]
	public List<MissionEntity> missionEntities;

	[NonSerialized]
	public List<PersistentMissionEntityData> persistentMissionEntities;

	[NonSerialized]
	public int playerInputRequired;

	[NonSerialized]
	public long startTimeUtcSeconds;

	[NonSerialized]
	public long endTimeUtcSeconds;

	[NonSerialized]
	public bool hasDispensedRewards;

	public static void ResetToPool(MissionInstanceData instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.providerID = default(NetworkableId);
		if (instance.objectiveStatuses != null)
		{
			for (int i = 0; i < instance.objectiveStatuses.Count; i++)
			{
				if (instance.objectiveStatuses[i] != null)
				{
					instance.objectiveStatuses[i].ResetToPool();
					instance.objectiveStatuses[i] = null;
				}
			}
			List<ObjectiveStatus> obj = instance.objectiveStatuses;
			Pool.Free(ref obj, freeElements: false);
			instance.objectiveStatuses = obj;
		}
		if (instance.missionPoints != null)
		{
			for (int j = 0; j < instance.missionPoints.Count; j++)
			{
				if (instance.missionPoints[j] != null)
				{
					instance.missionPoints[j].ResetToPool();
					instance.missionPoints[j] = null;
				}
			}
			List<MissionPoint> obj2 = instance.missionPoints;
			Pool.Free(ref obj2, freeElements: false);
			instance.missionPoints = obj2;
		}
		if (instance.missionEntities != null)
		{
			for (int k = 0; k < instance.missionEntities.Count; k++)
			{
				if (instance.missionEntities[k] != null)
				{
					instance.missionEntities[k].ResetToPool();
					instance.missionEntities[k] = null;
				}
			}
			List<MissionEntity> obj3 = instance.missionEntities;
			Pool.Free(ref obj3, freeElements: false);
			instance.missionEntities = obj3;
		}
		if (instance.persistentMissionEntities != null)
		{
			for (int l = 0; l < instance.persistentMissionEntities.Count; l++)
			{
				if (instance.persistentMissionEntities[l] != null)
				{
					instance.persistentMissionEntities[l].ResetToPool();
					instance.persistentMissionEntities[l] = null;
				}
			}
			List<PersistentMissionEntityData> obj4 = instance.persistentMissionEntities;
			Pool.Free(ref obj4, freeElements: false);
			instance.persistentMissionEntities = obj4;
		}
		instance.playerInputRequired = 0;
		instance.startTimeUtcSeconds = 0L;
		instance.endTimeUtcSeconds = 0L;
		instance.hasDispensedRewards = false;
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
			throw new Exception("Trying to dispose MissionInstanceData with ShouldPool set to false!");
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

	public void CopyTo(MissionInstanceData instance)
	{
		instance.providerID = providerID;
		if (objectiveStatuses != null)
		{
			instance.objectiveStatuses = Pool.Get<List<ObjectiveStatus>>();
			for (int i = 0; i < objectiveStatuses.Count; i++)
			{
				ObjectiveStatus item = objectiveStatuses[i].Copy();
				instance.objectiveStatuses.Add(item);
			}
		}
		else
		{
			instance.objectiveStatuses = null;
		}
		if (missionPoints != null)
		{
			instance.missionPoints = Pool.Get<List<MissionPoint>>();
			for (int j = 0; j < missionPoints.Count; j++)
			{
				MissionPoint item2 = missionPoints[j].Copy();
				instance.missionPoints.Add(item2);
			}
		}
		else
		{
			instance.missionPoints = null;
		}
		if (missionEntities != null)
		{
			instance.missionEntities = Pool.Get<List<MissionEntity>>();
			for (int k = 0; k < missionEntities.Count; k++)
			{
				MissionEntity item3 = missionEntities[k].Copy();
				instance.missionEntities.Add(item3);
			}
		}
		else
		{
			instance.missionEntities = null;
		}
		if (persistentMissionEntities != null)
		{
			instance.persistentMissionEntities = Pool.Get<List<PersistentMissionEntityData>>();
			for (int l = 0; l < persistentMissionEntities.Count; l++)
			{
				PersistentMissionEntityData item4 = persistentMissionEntities[l].Copy();
				instance.persistentMissionEntities.Add(item4);
			}
		}
		else
		{
			instance.persistentMissionEntities = null;
		}
		instance.playerInputRequired = playerInputRequired;
		instance.startTimeUtcSeconds = startTimeUtcSeconds;
		instance.endTimeUtcSeconds = endTimeUtcSeconds;
		instance.hasDispensedRewards = hasDispensedRewards;
	}

	public MissionInstanceData Copy()
	{
		MissionInstanceData missionInstanceData = Pool.Get<MissionInstanceData>();
		CopyTo(missionInstanceData);
		return missionInstanceData;
	}

	public static MissionInstanceData Deserialize(BufferStream stream)
	{
		MissionInstanceData missionInstanceData = Pool.Get<MissionInstanceData>();
		Deserialize(stream, missionInstanceData, isDelta: false);
		return missionInstanceData;
	}

	public static MissionInstanceData DeserializeLengthDelimited(BufferStream stream)
	{
		MissionInstanceData missionInstanceData = Pool.Get<MissionInstanceData>();
		DeserializeLengthDelimited(stream, missionInstanceData, isDelta: false);
		return missionInstanceData;
	}

	public static MissionInstanceData DeserializeLength(BufferStream stream, int length)
	{
		MissionInstanceData missionInstanceData = Pool.Get<MissionInstanceData>();
		DeserializeLength(stream, length, missionInstanceData, isDelta: false);
		return missionInstanceData;
	}

	public static MissionInstanceData Deserialize(byte[] buffer)
	{
		MissionInstanceData missionInstanceData = Pool.Get<MissionInstanceData>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, missionInstanceData, isDelta: false);
		return missionInstanceData;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, MissionInstanceData previous)
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

	public static MissionInstanceData Deserialize(BufferStream stream, MissionInstanceData instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.objectiveStatuses == null)
			{
				instance.objectiveStatuses = Pool.Get<List<ObjectiveStatus>>();
			}
			if (instance.missionPoints == null)
			{
				instance.missionPoints = Pool.Get<List<MissionPoint>>();
			}
			if (instance.missionEntities == null)
			{
				instance.missionEntities = Pool.Get<List<MissionEntity>>();
			}
			if (instance.persistentMissionEntities == null)
			{
				instance.persistentMissionEntities = Pool.Get<List<PersistentMissionEntityData>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.MissionInstanceData");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MissionInstanceData");
				instance.providerID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.MissionInstanceData");
				stream.ConsumeRepeatedElement();
				instance.objectiveStatuses.Add(ObjectiveStatus.DeserializeLengthDelimited(stream));
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: true, "ProtoBuf.MissionInstanceData");
				stream.ConsumeRepeatedElement();
				instance.missionPoints.Add(MissionPoint.DeserializeLengthDelimited(stream));
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.MissionInstanceData");
				stream.ConsumeRepeatedElement();
				instance.missionEntities.Add(MissionEntity.DeserializeLengthDelimited(stream));
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.MissionInstanceData");
				stream.ConsumeRepeatedElement();
				instance.persistentMissionEntities.Add(PersistentMissionEntityData.DeserializeLengthDelimited(stream));
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.MissionInstanceData");
				instance.playerInputRequired = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.MissionInstanceData");
				instance.startTimeUtcSeconds = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 88:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.MissionInstanceData");
				instance.endTimeUtcSeconds = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 96:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.MissionInstanceData");
				instance.hasDispensedRewards = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MissionInstanceData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.MissionInstanceData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: true, "ProtoBuf.MissionInstanceData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.MissionInstanceData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.MissionInstanceData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.MissionInstanceData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.MissionInstanceData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.MissionInstanceData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.MissionInstanceData");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MissionInstanceData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static MissionInstanceData DeserializeLengthDelimited(BufferStream stream, MissionInstanceData instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.objectiveStatuses == null)
			{
				instance.objectiveStatuses = Pool.Get<List<ObjectiveStatus>>();
			}
			if (instance.missionPoints == null)
			{
				instance.missionPoints = Pool.Get<List<MissionPoint>>();
			}
			if (instance.missionEntities == null)
			{
				instance.missionEntities = Pool.Get<List<MissionEntity>>();
			}
			if (instance.persistentMissionEntities == null)
			{
				instance.persistentMissionEntities = Pool.Get<List<PersistentMissionEntityData>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.MissionInstanceData");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MissionInstanceData");
				instance.providerID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.MissionInstanceData");
				stream.ConsumeRepeatedElement();
				instance.objectiveStatuses.Add(ObjectiveStatus.DeserializeLengthDelimited(stream));
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: true, "ProtoBuf.MissionInstanceData");
				stream.ConsumeRepeatedElement();
				instance.missionPoints.Add(MissionPoint.DeserializeLengthDelimited(stream));
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.MissionInstanceData");
				stream.ConsumeRepeatedElement();
				instance.missionEntities.Add(MissionEntity.DeserializeLengthDelimited(stream));
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.MissionInstanceData");
				stream.ConsumeRepeatedElement();
				instance.persistentMissionEntities.Add(PersistentMissionEntityData.DeserializeLengthDelimited(stream));
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.MissionInstanceData");
				instance.playerInputRequired = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.MissionInstanceData");
				instance.startTimeUtcSeconds = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 88:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.MissionInstanceData");
				instance.endTimeUtcSeconds = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 96:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.MissionInstanceData");
				instance.hasDispensedRewards = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MissionInstanceData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.MissionInstanceData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: true, "ProtoBuf.MissionInstanceData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.MissionInstanceData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.MissionInstanceData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.MissionInstanceData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.MissionInstanceData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.MissionInstanceData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.MissionInstanceData");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MissionInstanceData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static MissionInstanceData DeserializeLength(BufferStream stream, int length, MissionInstanceData instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.objectiveStatuses == null)
			{
				instance.objectiveStatuses = Pool.Get<List<ObjectiveStatus>>();
			}
			if (instance.missionPoints == null)
			{
				instance.missionPoints = Pool.Get<List<MissionPoint>>();
			}
			if (instance.missionEntities == null)
			{
				instance.missionEntities = Pool.Get<List<MissionEntity>>();
			}
			if (instance.persistentMissionEntities == null)
			{
				instance.persistentMissionEntities = Pool.Get<List<PersistentMissionEntityData>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.MissionInstanceData");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MissionInstanceData");
				instance.providerID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.MissionInstanceData");
				stream.ConsumeRepeatedElement();
				instance.objectiveStatuses.Add(ObjectiveStatus.DeserializeLengthDelimited(stream));
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: true, "ProtoBuf.MissionInstanceData");
				stream.ConsumeRepeatedElement();
				instance.missionPoints.Add(MissionPoint.DeserializeLengthDelimited(stream));
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.MissionInstanceData");
				stream.ConsumeRepeatedElement();
				instance.missionEntities.Add(MissionEntity.DeserializeLengthDelimited(stream));
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.MissionInstanceData");
				stream.ConsumeRepeatedElement();
				instance.persistentMissionEntities.Add(PersistentMissionEntityData.DeserializeLengthDelimited(stream));
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.MissionInstanceData");
				instance.playerInputRequired = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.MissionInstanceData");
				instance.startTimeUtcSeconds = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 88:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.MissionInstanceData");
				instance.endTimeUtcSeconds = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 96:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.MissionInstanceData");
				instance.hasDispensedRewards = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MissionInstanceData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.MissionInstanceData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: true, "ProtoBuf.MissionInstanceData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.MissionInstanceData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.MissionInstanceData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.MissionInstanceData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.MissionInstanceData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.MissionInstanceData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.MissionInstanceData");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MissionInstanceData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, MissionInstanceData instance, MissionInstanceData previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, instance.providerID.Value);
		if (instance.objectiveStatuses != null)
		{
			for (int i = 0; i < instance.objectiveStatuses.Count; i++)
			{
				ObjectiveStatus objectiveStatus = instance.objectiveStatuses[i];
				stream.WriteByte(42);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				ObjectiveStatus.SerializeDelta(stream, objectiveStatus, objectiveStatus);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field objectiveStatuses (ProtoBuf.ObjectiveStatus)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.missionPoints != null)
		{
			for (int j = 0; j < instance.missionPoints.Count; j++)
			{
				MissionPoint missionPoint = instance.missionPoints[j];
				stream.WriteByte(50);
				BufferStream.RangeHandle range2 = stream.GetRange(5);
				int position2 = stream.Position;
				MissionPoint.SerializeDelta(stream, missionPoint, missionPoint);
				int val = stream.Position - position2;
				Span<byte> span2 = range2.GetSpan();
				int num2 = ProtocolParser.WriteUInt32((uint)val, span2, 0);
				if (num2 < 5)
				{
					span2[num2 - 1] |= 128;
					while (num2 < 4)
					{
						span2[num2++] = 128;
					}
					span2[4] = 0;
				}
			}
		}
		if (instance.missionEntities != null)
		{
			for (int k = 0; k < instance.missionEntities.Count; k++)
			{
				MissionEntity missionEntity = instance.missionEntities[k];
				stream.WriteByte(58);
				BufferStream.RangeHandle range3 = stream.GetRange(5);
				int position3 = stream.Position;
				MissionEntity.SerializeDelta(stream, missionEntity, missionEntity);
				int val2 = stream.Position - position3;
				Span<byte> span3 = range3.GetSpan();
				int num3 = ProtocolParser.WriteUInt32((uint)val2, span3, 0);
				if (num3 < 5)
				{
					span3[num3 - 1] |= 128;
					while (num3 < 4)
					{
						span3[num3++] = 128;
					}
					span3[4] = 0;
				}
			}
		}
		if (instance.persistentMissionEntities != null)
		{
			for (int l = 0; l < instance.persistentMissionEntities.Count; l++)
			{
				PersistentMissionEntityData persistentMissionEntityData = instance.persistentMissionEntities[l];
				stream.WriteByte(66);
				BufferStream.RangeHandle range4 = stream.GetRange(1);
				int position4 = stream.Position;
				PersistentMissionEntityData.SerializeDelta(stream, persistentMissionEntityData, persistentMissionEntityData);
				int num4 = stream.Position - position4;
				if (num4 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field persistentMissionEntities (ProtoBuf.PersistentMissionEntityData)");
				}
				Span<byte> span4 = range4.GetSpan();
				ProtocolParser.WriteUInt32((uint)num4, span4, 0);
			}
		}
		if (instance.playerInputRequired != previous.playerInputRequired)
		{
			stream.WriteByte(72);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.playerInputRequired);
		}
		stream.WriteByte(80);
		ProtocolParser.WriteUInt64(stream, (ulong)instance.startTimeUtcSeconds);
		stream.WriteByte(88);
		ProtocolParser.WriteUInt64(stream, (ulong)instance.endTimeUtcSeconds);
		stream.WriteByte(96);
		ProtocolParser.WriteBool(stream, instance.hasDispensedRewards);
	}

	public static void Serialize(BufferStream stream, MissionInstanceData instance)
	{
		if (instance.providerID != default(NetworkableId))
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.providerID.Value);
		}
		if (instance.objectiveStatuses != null)
		{
			for (int i = 0; i < instance.objectiveStatuses.Count; i++)
			{
				ObjectiveStatus instance2 = instance.objectiveStatuses[i];
				stream.WriteByte(42);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				ObjectiveStatus.Serialize(stream, instance2);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field objectiveStatuses (ProtoBuf.ObjectiveStatus)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.missionPoints != null)
		{
			for (int j = 0; j < instance.missionPoints.Count; j++)
			{
				MissionPoint instance3 = instance.missionPoints[j];
				stream.WriteByte(50);
				BufferStream.RangeHandle range2 = stream.GetRange(5);
				int position2 = stream.Position;
				MissionPoint.Serialize(stream, instance3);
				int val = stream.Position - position2;
				Span<byte> span2 = range2.GetSpan();
				int num2 = ProtocolParser.WriteUInt32((uint)val, span2, 0);
				if (num2 < 5)
				{
					span2[num2 - 1] |= 128;
					while (num2 < 4)
					{
						span2[num2++] = 128;
					}
					span2[4] = 0;
				}
			}
		}
		if (instance.missionEntities != null)
		{
			for (int k = 0; k < instance.missionEntities.Count; k++)
			{
				MissionEntity instance4 = instance.missionEntities[k];
				stream.WriteByte(58);
				BufferStream.RangeHandle range3 = stream.GetRange(5);
				int position3 = stream.Position;
				MissionEntity.Serialize(stream, instance4);
				int val2 = stream.Position - position3;
				Span<byte> span3 = range3.GetSpan();
				int num3 = ProtocolParser.WriteUInt32((uint)val2, span3, 0);
				if (num3 < 5)
				{
					span3[num3 - 1] |= 128;
					while (num3 < 4)
					{
						span3[num3++] = 128;
					}
					span3[4] = 0;
				}
			}
		}
		if (instance.persistentMissionEntities != null)
		{
			for (int l = 0; l < instance.persistentMissionEntities.Count; l++)
			{
				PersistentMissionEntityData instance5 = instance.persistentMissionEntities[l];
				stream.WriteByte(66);
				BufferStream.RangeHandle range4 = stream.GetRange(1);
				int position4 = stream.Position;
				PersistentMissionEntityData.Serialize(stream, instance5);
				int num4 = stream.Position - position4;
				if (num4 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field persistentMissionEntities (ProtoBuf.PersistentMissionEntityData)");
				}
				Span<byte> span4 = range4.GetSpan();
				ProtocolParser.WriteUInt32((uint)num4, span4, 0);
			}
		}
		if (instance.playerInputRequired != 0)
		{
			stream.WriteByte(72);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.playerInputRequired);
		}
		if (instance.startTimeUtcSeconds != 0L)
		{
			stream.WriteByte(80);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.startTimeUtcSeconds);
		}
		if (instance.endTimeUtcSeconds != 0L)
		{
			stream.WriteByte(88);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.endTimeUtcSeconds);
		}
		if (instance.hasDispensedRewards)
		{
			stream.WriteByte(96);
			ProtocolParser.WriteBool(stream, instance.hasDispensedRewards);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref providerID.Value);
		if (objectiveStatuses != null)
		{
			for (int i = 0; i < objectiveStatuses.Count; i++)
			{
				objectiveStatuses[i]?.InspectUids(action);
			}
		}
		if (missionPoints != null)
		{
			for (int j = 0; j < missionPoints.Count; j++)
			{
				missionPoints[j]?.InspectUids(action);
			}
		}
		if (missionEntities != null)
		{
			for (int k = 0; k < missionEntities.Count; k++)
			{
				missionEntities[k]?.InspectUids(action);
			}
		}
		if (persistentMissionEntities != null)
		{
			for (int l = 0; l < persistentMissionEntities.Count; l++)
			{
				persistentMissionEntities[l]?.InspectUids(action);
			}
		}
	}
}
