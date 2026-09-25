using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class Item : IDisposable, Pool.IPooled, IProto<Item>, IProto
{
	public class ConditionData : IDisposable, Pool.IPooled, IProto<ConditionData>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public float condition;

		[NonSerialized]
		public float maxCondition;

		public static void ResetToPool(ConditionData instance)
		{
			if (instance.ShouldPool)
			{
				instance.condition = 0f;
				instance.maxCondition = 0f;
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
				throw new Exception("Trying to dispose ConditionData with ShouldPool set to false!");
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

		public void CopyTo(ConditionData instance)
		{
			instance.condition = condition;
			instance.maxCondition = maxCondition;
		}

		public ConditionData Copy()
		{
			ConditionData conditionData = Pool.Get<ConditionData>();
			CopyTo(conditionData);
			return conditionData;
		}

		public static ConditionData Deserialize(BufferStream stream)
		{
			ConditionData conditionData = Pool.Get<ConditionData>();
			Deserialize(stream, conditionData, isDelta: false);
			return conditionData;
		}

		public static ConditionData DeserializeLengthDelimited(BufferStream stream)
		{
			ConditionData conditionData = Pool.Get<ConditionData>();
			DeserializeLengthDelimited(stream, conditionData, isDelta: false);
			return conditionData;
		}

		public static ConditionData DeserializeLength(BufferStream stream, int length)
		{
			ConditionData conditionData = Pool.Get<ConditionData>();
			DeserializeLength(stream, length, conditionData, isDelta: false);
			return conditionData;
		}

		public static ConditionData Deserialize(byte[] buffer)
		{
			ConditionData conditionData = Pool.Get<ConditionData>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, conditionData, isDelta: false);
			return conditionData;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, ConditionData previous)
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

		public static ConditionData Deserialize(BufferStream stream, ConditionData instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.Item.ConditionData");
				switch (num)
				{
				case 13:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Item.ConditionData");
					instance.condition = ProtocolParser.ReadSingle(stream);
					continue;
				case 21:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Item.ConditionData");
					instance.maxCondition = ProtocolParser.ReadSingle(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Item.ConditionData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Item.ConditionData");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Item.ConditionData");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static ConditionData DeserializeLengthDelimited(BufferStream stream, ConditionData instance, bool isDelta)
		{
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
				stream.ConsumeFieldOperation("ProtoBuf.Item.ConditionData");
				switch (num2)
				{
				case 13:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Item.ConditionData");
					instance.condition = ProtocolParser.ReadSingle(stream);
					continue;
				case 21:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Item.ConditionData");
					instance.maxCondition = ProtocolParser.ReadSingle(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Item.ConditionData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Item.ConditionData");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Item.ConditionData");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static ConditionData DeserializeLength(BufferStream stream, int length, ConditionData instance, bool isDelta)
		{
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
				stream.ConsumeFieldOperation("ProtoBuf.Item.ConditionData");
				switch (num2)
				{
				case 13:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Item.ConditionData");
					instance.condition = ProtocolParser.ReadSingle(stream);
					continue;
				case 21:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Item.ConditionData");
					instance.maxCondition = ProtocolParser.ReadSingle(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Item.ConditionData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Item.ConditionData");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Item.ConditionData");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, ConditionData instance, ConditionData previous)
		{
			if (instance.condition != previous.condition)
			{
				stream.WriteByte(13);
				ProtocolParser.WriteSingle(stream, instance.condition);
			}
			if (instance.maxCondition != previous.maxCondition)
			{
				stream.WriteByte(21);
				ProtocolParser.WriteSingle(stream, instance.maxCondition);
			}
		}

		public static void Serialize(BufferStream stream, ConditionData instance)
		{
			if (instance.condition != 0f)
			{
				stream.WriteByte(13);
				ProtocolParser.WriteSingle(stream, instance.condition);
			}
			if (instance.maxCondition != 0f)
			{
				stream.WriteByte(21);
				ProtocolParser.WriteSingle(stream, instance.maxCondition);
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

	public class InstanceData : IDisposable, Pool.IPooled, IProto<InstanceData>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public int dataInt;

		[NonSerialized]
		public int blueprintTarget;

		[NonSerialized]
		public int blueprintAmount;

		[NonSerialized]
		public NetworkableId subEntity;

		[NonSerialized]
		public float dataFloat;

		public static void ResetToPool(InstanceData instance)
		{
			if (instance.ShouldPool)
			{
				instance.dataInt = 0;
				instance.blueprintTarget = 0;
				instance.blueprintAmount = 0;
				instance.subEntity = default(NetworkableId);
				instance.dataFloat = 0f;
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
				throw new Exception("Trying to dispose InstanceData with ShouldPool set to false!");
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

		public void CopyTo(InstanceData instance)
		{
			instance.dataInt = dataInt;
			instance.blueprintTarget = blueprintTarget;
			instance.blueprintAmount = blueprintAmount;
			instance.subEntity = subEntity;
			instance.dataFloat = dataFloat;
		}

		public InstanceData Copy()
		{
			InstanceData instanceData = Pool.Get<InstanceData>();
			CopyTo(instanceData);
			return instanceData;
		}

		public static InstanceData Deserialize(BufferStream stream)
		{
			InstanceData instanceData = Pool.Get<InstanceData>();
			Deserialize(stream, instanceData, isDelta: false);
			return instanceData;
		}

		public static InstanceData DeserializeLengthDelimited(BufferStream stream)
		{
			InstanceData instanceData = Pool.Get<InstanceData>();
			DeserializeLengthDelimited(stream, instanceData, isDelta: false);
			return instanceData;
		}

		public static InstanceData DeserializeLength(BufferStream stream, int length)
		{
			InstanceData instanceData = Pool.Get<InstanceData>();
			DeserializeLength(stream, length, instanceData, isDelta: false);
			return instanceData;
		}

		public static InstanceData Deserialize(byte[] buffer)
		{
			InstanceData instanceData = Pool.Get<InstanceData>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, instanceData, isDelta: false);
			return instanceData;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, InstanceData previous)
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

		public static InstanceData Deserialize(BufferStream stream, InstanceData instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.Item.InstanceData");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Item.InstanceData");
					instance.dataInt = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Item.InstanceData");
					instance.blueprintTarget = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Item.InstanceData");
					instance.blueprintAmount = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Item.InstanceData");
					instance.subEntity = new NetworkableId(ProtocolParser.ReadUInt64(stream));
					continue;
				case 45:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Item.InstanceData");
					instance.dataFloat = ProtocolParser.ReadSingle(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Item.InstanceData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Item.InstanceData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Item.InstanceData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Item.InstanceData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Item.InstanceData");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Item.InstanceData");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static InstanceData DeserializeLengthDelimited(BufferStream stream, InstanceData instance, bool isDelta)
		{
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
				stream.ConsumeFieldOperation("ProtoBuf.Item.InstanceData");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Item.InstanceData");
					instance.dataInt = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Item.InstanceData");
					instance.blueprintTarget = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Item.InstanceData");
					instance.blueprintAmount = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Item.InstanceData");
					instance.subEntity = new NetworkableId(ProtocolParser.ReadUInt64(stream));
					continue;
				case 45:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Item.InstanceData");
					instance.dataFloat = ProtocolParser.ReadSingle(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Item.InstanceData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Item.InstanceData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Item.InstanceData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Item.InstanceData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Item.InstanceData");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Item.InstanceData");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static InstanceData DeserializeLength(BufferStream stream, int length, InstanceData instance, bool isDelta)
		{
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
				stream.ConsumeFieldOperation("ProtoBuf.Item.InstanceData");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Item.InstanceData");
					instance.dataInt = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Item.InstanceData");
					instance.blueprintTarget = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Item.InstanceData");
					instance.blueprintAmount = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Item.InstanceData");
					instance.subEntity = new NetworkableId(ProtocolParser.ReadUInt64(stream));
					continue;
				case 45:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Item.InstanceData");
					instance.dataFloat = ProtocolParser.ReadSingle(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Item.InstanceData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Item.InstanceData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Item.InstanceData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Item.InstanceData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Item.InstanceData");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Item.InstanceData");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, InstanceData instance, InstanceData previous)
		{
			if (instance.dataInt != previous.dataInt)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.dataInt);
			}
			if (instance.blueprintTarget != previous.blueprintTarget)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.blueprintTarget);
			}
			if (instance.blueprintAmount != previous.blueprintAmount)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.blueprintAmount);
			}
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, instance.subEntity.Value);
			if (instance.dataFloat != previous.dataFloat)
			{
				stream.WriteByte(45);
				ProtocolParser.WriteSingle(stream, instance.dataFloat);
			}
		}

		public static void Serialize(BufferStream stream, InstanceData instance)
		{
			if (instance.dataInt != 0)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.dataInt);
			}
			if (instance.blueprintTarget != 0)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.blueprintTarget);
			}
			if (instance.blueprintAmount != 0)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.blueprintAmount);
			}
			if (instance.subEntity != default(NetworkableId))
			{
				stream.WriteByte(32);
				ProtocolParser.WriteUInt64(stream, instance.subEntity.Value);
			}
			if (instance.dataFloat != 0f)
			{
				stream.WriteByte(45);
				ProtocolParser.WriteSingle(stream, instance.dataFloat);
			}
		}

		public void ToProto(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public void InspectUids(UidInspector<ulong> action)
		{
			action(UidType.NetworkableId, ref subEntity.Value);
		}
	}

	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public ItemId UID;

	[NonSerialized]
	public int itemid;

	[NonSerialized]
	public int slot;

	[NonSerialized]
	public int amount;

	[NonSerialized]
	public int flags;

	[NonSerialized]
	public float removetime;

	[NonSerialized]
	public float locktime;

	[NonSerialized]
	public NetworkableId worldEntity;

	[NonSerialized]
	public InstanceData instanceData;

	[NonSerialized]
	public NetworkableId heldEntity;

	[NonSerialized]
	public ConditionData conditionData;

	[NonSerialized]
	public string name;

	[NonSerialized]
	public string text;

	[NonSerialized]
	public ulong skinid;

	[NonSerialized]
	public float cooktime;

	[NonSerialized]
	public string streamerName;

	[NonSerialized]
	public int ammoCount;

	[NonSerialized]
	public List<ItemOwnershipAmount> ownership;

	[NonSerialized]
	public uint iconImageId;

	[NonSerialized]
	public ulong attachmentid;

	[NonSerialized]
	public ItemContainer contents;

	public static void ResetToPool(Item instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.UID = default(ItemId);
		instance.itemid = 0;
		instance.slot = 0;
		instance.amount = 0;
		instance.flags = 0;
		instance.removetime = 0f;
		instance.locktime = 0f;
		instance.worldEntity = default(NetworkableId);
		if (instance.instanceData != null)
		{
			instance.instanceData.ResetToPool();
			instance.instanceData = null;
		}
		instance.heldEntity = default(NetworkableId);
		if (instance.conditionData != null)
		{
			instance.conditionData.ResetToPool();
			instance.conditionData = null;
		}
		instance.name = string.Empty;
		instance.text = string.Empty;
		instance.skinid = 0uL;
		instance.cooktime = 0f;
		instance.streamerName = string.Empty;
		instance.ammoCount = 0;
		if (instance.ownership != null)
		{
			for (int i = 0; i < instance.ownership.Count; i++)
			{
				if (instance.ownership[i] != null)
				{
					instance.ownership[i].ResetToPool();
					instance.ownership[i] = null;
				}
			}
			List<ItemOwnershipAmount> obj = instance.ownership;
			Pool.Free(ref obj, freeElements: false);
			instance.ownership = obj;
		}
		instance.iconImageId = 0u;
		instance.attachmentid = 0uL;
		if (instance.contents != null)
		{
			instance.contents.ResetToPool();
			instance.contents = null;
		}
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
			throw new Exception("Trying to dispose Item with ShouldPool set to false!");
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

	public void CopyTo(Item instance)
	{
		instance.UID = UID;
		instance.itemid = itemid;
		instance.slot = slot;
		instance.amount = amount;
		instance.flags = flags;
		instance.removetime = removetime;
		instance.locktime = locktime;
		instance.worldEntity = worldEntity;
		if (instanceData != null)
		{
			if (instance.instanceData == null)
			{
				instance.instanceData = instanceData.Copy();
			}
			else
			{
				instanceData.CopyTo(instance.instanceData);
			}
		}
		else
		{
			instance.instanceData = null;
		}
		instance.heldEntity = heldEntity;
		if (conditionData != null)
		{
			if (instance.conditionData == null)
			{
				instance.conditionData = conditionData.Copy();
			}
			else
			{
				conditionData.CopyTo(instance.conditionData);
			}
		}
		else
		{
			instance.conditionData = null;
		}
		instance.name = name;
		instance.text = text;
		instance.skinid = skinid;
		instance.cooktime = cooktime;
		instance.streamerName = streamerName;
		instance.ammoCount = ammoCount;
		if (ownership != null)
		{
			instance.ownership = Pool.Get<List<ItemOwnershipAmount>>();
			for (int i = 0; i < ownership.Count; i++)
			{
				ItemOwnershipAmount item = ownership[i].Copy();
				instance.ownership.Add(item);
			}
		}
		else
		{
			instance.ownership = null;
		}
		instance.iconImageId = iconImageId;
		instance.attachmentid = attachmentid;
		if (contents != null)
		{
			if (instance.contents == null)
			{
				instance.contents = contents.Copy();
			}
			else
			{
				contents.CopyTo(instance.contents);
			}
		}
		else
		{
			instance.contents = null;
		}
	}

	public Item Copy()
	{
		Item item = Pool.Get<Item>();
		CopyTo(item);
		return item;
	}

	public static Item Deserialize(BufferStream stream)
	{
		Item item = Pool.Get<Item>();
		Deserialize(stream, item, isDelta: false);
		return item;
	}

	public static Item DeserializeLengthDelimited(BufferStream stream)
	{
		Item item = Pool.Get<Item>();
		DeserializeLengthDelimited(stream, item, isDelta: false);
		return item;
	}

	public static Item DeserializeLength(BufferStream stream, int length)
	{
		Item item = Pool.Get<Item>();
		DeserializeLength(stream, length, item, isDelta: false);
		return item;
	}

	public static Item Deserialize(byte[] buffer)
	{
		Item item = Pool.Get<Item>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, item, isDelta: false);
		return item;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, Item previous)
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

	public static Item Deserialize(BufferStream stream, Item instance, bool isDelta)
	{
		if (!isDelta && instance.ownership == null)
		{
			instance.ownership = Pool.Get<List<ItemOwnershipAmount>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Item");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.UID = new ItemId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.itemid = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.slot = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.amount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.flags = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.removetime = ProtocolParser.ReadSingle(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.locktime = ProtocolParser.ReadSingle(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.worldEntity = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.Item");
				if (instance.instanceData == null)
				{
					instance.instanceData = InstanceData.DeserializeLengthDelimited(stream);
				}
				else
				{
					InstanceData.DeserializeLengthDelimited(stream, instance.instanceData, isDelta);
				}
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.heldEntity = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.Item");
				if (instance.conditionData == null)
				{
					instance.conditionData = ConditionData.DeserializeLengthDelimited(stream);
				}
				else
				{
					ConditionData.DeserializeLengthDelimited(stream, instance.conditionData, isDelta);
				}
				continue;
			case 114:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.name = ProtocolParser.ReadString(stream);
				continue;
			case 122:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.text = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.Item");
				if (key.WireType == Wire.Varint)
				{
					instance.skinid = ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 17u:
				stream.ValidateFieldOrder(ref lastFieldId, 17u, fieldIsRepeated: false, "ProtoBuf.Item");
				if (key.WireType == Wire.Fixed32)
				{
					instance.cooktime = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 18u:
				stream.ValidateFieldOrder(ref lastFieldId, 18u, fieldIsRepeated: false, "ProtoBuf.Item");
				if (key.WireType == Wire.LengthDelimited)
				{
					instance.streamerName = ProtocolParser.ReadString(stream);
				}
				break;
			case 19u:
				stream.ValidateFieldOrder(ref lastFieldId, 19u, fieldIsRepeated: false, "ProtoBuf.Item");
				if (key.WireType == Wire.Varint)
				{
					instance.ammoCount = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 20u:
				stream.ValidateFieldOrder(ref lastFieldId, 20u, fieldIsRepeated: true, "ProtoBuf.Item");
				if (key.WireType == Wire.LengthDelimited)
				{
					stream.ConsumeRepeatedElement();
					instance.ownership.Add(ItemOwnershipAmount.DeserializeLengthDelimited(stream));
				}
				break;
			case 21u:
				stream.ValidateFieldOrder(ref lastFieldId, 21u, fieldIsRepeated: false, "ProtoBuf.Item");
				if (key.WireType == Wire.Varint)
				{
					instance.iconImageId = ProtocolParser.ReadUInt32(stream);
				}
				break;
			case 22u:
				stream.ValidateFieldOrder(ref lastFieldId, 22u, fieldIsRepeated: false, "ProtoBuf.Item");
				if (key.WireType == Wire.Varint)
				{
					instance.attachmentid = ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 100u:
				stream.ValidateFieldOrder(ref lastFieldId, 100u, fieldIsRepeated: false, "ProtoBuf.Item");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.contents == null)
					{
						instance.contents = ItemContainer.DeserializeLengthDelimited(stream);
					}
					else
					{
						ItemContainer.DeserializeLengthDelimited(stream, instance.contents, isDelta);
					}
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Item DeserializeLengthDelimited(BufferStream stream, Item instance, bool isDelta)
	{
		if (!isDelta && instance.ownership == null)
		{
			instance.ownership = Pool.Get<List<ItemOwnershipAmount>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Item");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.UID = new ItemId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.itemid = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.slot = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.amount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.flags = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.removetime = ProtocolParser.ReadSingle(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.locktime = ProtocolParser.ReadSingle(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.worldEntity = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.Item");
				if (instance.instanceData == null)
				{
					instance.instanceData = InstanceData.DeserializeLengthDelimited(stream);
				}
				else
				{
					InstanceData.DeserializeLengthDelimited(stream, instance.instanceData, isDelta);
				}
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.heldEntity = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.Item");
				if (instance.conditionData == null)
				{
					instance.conditionData = ConditionData.DeserializeLengthDelimited(stream);
				}
				else
				{
					ConditionData.DeserializeLengthDelimited(stream, instance.conditionData, isDelta);
				}
				continue;
			case 114:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.name = ProtocolParser.ReadString(stream);
				continue;
			case 122:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.text = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.Item");
				if (key.WireType == Wire.Varint)
				{
					instance.skinid = ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 17u:
				stream.ValidateFieldOrder(ref lastFieldId, 17u, fieldIsRepeated: false, "ProtoBuf.Item");
				if (key.WireType == Wire.Fixed32)
				{
					instance.cooktime = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 18u:
				stream.ValidateFieldOrder(ref lastFieldId, 18u, fieldIsRepeated: false, "ProtoBuf.Item");
				if (key.WireType == Wire.LengthDelimited)
				{
					instance.streamerName = ProtocolParser.ReadString(stream);
				}
				break;
			case 19u:
				stream.ValidateFieldOrder(ref lastFieldId, 19u, fieldIsRepeated: false, "ProtoBuf.Item");
				if (key.WireType == Wire.Varint)
				{
					instance.ammoCount = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 20u:
				stream.ValidateFieldOrder(ref lastFieldId, 20u, fieldIsRepeated: true, "ProtoBuf.Item");
				if (key.WireType == Wire.LengthDelimited)
				{
					stream.ConsumeRepeatedElement();
					instance.ownership.Add(ItemOwnershipAmount.DeserializeLengthDelimited(stream));
				}
				break;
			case 21u:
				stream.ValidateFieldOrder(ref lastFieldId, 21u, fieldIsRepeated: false, "ProtoBuf.Item");
				if (key.WireType == Wire.Varint)
				{
					instance.iconImageId = ProtocolParser.ReadUInt32(stream);
				}
				break;
			case 22u:
				stream.ValidateFieldOrder(ref lastFieldId, 22u, fieldIsRepeated: false, "ProtoBuf.Item");
				if (key.WireType == Wire.Varint)
				{
					instance.attachmentid = ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 100u:
				stream.ValidateFieldOrder(ref lastFieldId, 100u, fieldIsRepeated: false, "ProtoBuf.Item");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.contents == null)
					{
						instance.contents = ItemContainer.DeserializeLengthDelimited(stream);
					}
					else
					{
						ItemContainer.DeserializeLengthDelimited(stream, instance.contents, isDelta);
					}
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Item DeserializeLength(BufferStream stream, int length, Item instance, bool isDelta)
	{
		if (!isDelta && instance.ownership == null)
		{
			instance.ownership = Pool.Get<List<ItemOwnershipAmount>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Item");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.UID = new ItemId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.itemid = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.slot = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.amount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.flags = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.removetime = ProtocolParser.ReadSingle(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.locktime = ProtocolParser.ReadSingle(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.worldEntity = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.Item");
				if (instance.instanceData == null)
				{
					instance.instanceData = InstanceData.DeserializeLengthDelimited(stream);
				}
				else
				{
					InstanceData.DeserializeLengthDelimited(stream, instance.instanceData, isDelta);
				}
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.heldEntity = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.Item");
				if (instance.conditionData == null)
				{
					instance.conditionData = ConditionData.DeserializeLengthDelimited(stream);
				}
				else
				{
					ConditionData.DeserializeLengthDelimited(stream, instance.conditionData, isDelta);
				}
				continue;
			case 114:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.name = ProtocolParser.ReadString(stream);
				continue;
			case 122:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.Item");
				instance.text = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.Item");
				if (key.WireType == Wire.Varint)
				{
					instance.skinid = ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 17u:
				stream.ValidateFieldOrder(ref lastFieldId, 17u, fieldIsRepeated: false, "ProtoBuf.Item");
				if (key.WireType == Wire.Fixed32)
				{
					instance.cooktime = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 18u:
				stream.ValidateFieldOrder(ref lastFieldId, 18u, fieldIsRepeated: false, "ProtoBuf.Item");
				if (key.WireType == Wire.LengthDelimited)
				{
					instance.streamerName = ProtocolParser.ReadString(stream);
				}
				break;
			case 19u:
				stream.ValidateFieldOrder(ref lastFieldId, 19u, fieldIsRepeated: false, "ProtoBuf.Item");
				if (key.WireType == Wire.Varint)
				{
					instance.ammoCount = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 20u:
				stream.ValidateFieldOrder(ref lastFieldId, 20u, fieldIsRepeated: true, "ProtoBuf.Item");
				if (key.WireType == Wire.LengthDelimited)
				{
					stream.ConsumeRepeatedElement();
					instance.ownership.Add(ItemOwnershipAmount.DeserializeLengthDelimited(stream));
				}
				break;
			case 21u:
				stream.ValidateFieldOrder(ref lastFieldId, 21u, fieldIsRepeated: false, "ProtoBuf.Item");
				if (key.WireType == Wire.Varint)
				{
					instance.iconImageId = ProtocolParser.ReadUInt32(stream);
				}
				break;
			case 22u:
				stream.ValidateFieldOrder(ref lastFieldId, 22u, fieldIsRepeated: false, "ProtoBuf.Item");
				if (key.WireType == Wire.Varint)
				{
					instance.attachmentid = ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 100u:
				stream.ValidateFieldOrder(ref lastFieldId, 100u, fieldIsRepeated: false, "ProtoBuf.Item");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.contents == null)
					{
						instance.contents = ItemContainer.DeserializeLengthDelimited(stream);
					}
					else
					{
						ItemContainer.DeserializeLengthDelimited(stream, instance.contents, isDelta);
					}
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Item");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, Item instance, Item previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, instance.UID.Value);
		if (instance.itemid != previous.itemid)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.itemid);
		}
		if (instance.slot != previous.slot)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.slot);
		}
		if (instance.amount != previous.amount)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.amount);
		}
		if (instance.flags != previous.flags)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.flags);
		}
		if (instance.removetime != previous.removetime)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.removetime);
		}
		if (instance.locktime != previous.locktime)
		{
			stream.WriteByte(61);
			ProtocolParser.WriteSingle(stream, instance.locktime);
		}
		stream.WriteByte(64);
		ProtocolParser.WriteUInt64(stream, instance.worldEntity.Value);
		if (instance.instanceData != null)
		{
			stream.WriteByte(74);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			InstanceData.SerializeDelta(stream, instance.instanceData, previous.instanceData);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field instanceData (ProtoBuf.Item.InstanceData)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		stream.WriteByte(80);
		ProtocolParser.WriteUInt64(stream, instance.heldEntity.Value);
		if (instance.conditionData != null)
		{
			stream.WriteByte(90);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			ConditionData.SerializeDelta(stream, instance.conditionData, previous.conditionData);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field conditionData (ProtoBuf.Item.ConditionData)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.name != null && instance.name != previous.name)
		{
			stream.WriteByte(114);
			ProtocolParser.WriteString(stream, instance.name);
		}
		if (instance.text != null && instance.text != previous.text)
		{
			stream.WriteByte(122);
			ProtocolParser.WriteString(stream, instance.text);
		}
		if (instance.skinid != previous.skinid)
		{
			stream.WriteByte(128);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, instance.skinid);
		}
		if (instance.cooktime != previous.cooktime)
		{
			stream.WriteByte(141);
			stream.WriteByte(1);
			ProtocolParser.WriteSingle(stream, instance.cooktime);
		}
		if (instance.streamerName != null && instance.streamerName != previous.streamerName)
		{
			stream.WriteByte(146);
			stream.WriteByte(1);
			ProtocolParser.WriteString(stream, instance.streamerName);
		}
		if (instance.ammoCount != previous.ammoCount)
		{
			stream.WriteByte(152);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ammoCount);
		}
		if (instance.ownership != null)
		{
			for (int i = 0; i < instance.ownership.Count; i++)
			{
				ItemOwnershipAmount itemOwnershipAmount = instance.ownership[i];
				stream.WriteByte(162);
				stream.WriteByte(1);
				BufferStream.RangeHandle range3 = stream.GetRange(5);
				int position3 = stream.Position;
				ItemOwnershipAmount.SerializeDelta(stream, itemOwnershipAmount, itemOwnershipAmount);
				int val = stream.Position - position3;
				Span<byte> span3 = range3.GetSpan();
				int num3 = ProtocolParser.WriteUInt32((uint)val, span3, 0);
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
		if (instance.iconImageId != previous.iconImageId)
		{
			stream.WriteByte(168);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt32(stream, instance.iconImageId);
		}
		if (instance.attachmentid != previous.attachmentid)
		{
			stream.WriteByte(176);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, instance.attachmentid);
		}
		if (instance.contents == null)
		{
			return;
		}
		stream.WriteByte(162);
		stream.WriteByte(6);
		BufferStream.RangeHandle range4 = stream.GetRange(5);
		int position4 = stream.Position;
		ItemContainer.SerializeDelta(stream, instance.contents, previous.contents);
		int val2 = stream.Position - position4;
		Span<byte> span4 = range4.GetSpan();
		int num4 = ProtocolParser.WriteUInt32((uint)val2, span4, 0);
		if (num4 < 5)
		{
			span4[num4 - 1] |= 128;
			while (num4 < 4)
			{
				span4[num4++] = 128;
			}
			span4[4] = 0;
		}
	}

	public static void Serialize(BufferStream stream, Item instance)
	{
		if (instance.UID != default(ItemId))
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.UID.Value);
		}
		if (instance.itemid != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.itemid);
		}
		if (instance.slot != 0)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.slot);
		}
		if (instance.amount != 0)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.amount);
		}
		if (instance.flags != 0)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.flags);
		}
		if (instance.removetime != 0f)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.removetime);
		}
		if (instance.locktime != 0f)
		{
			stream.WriteByte(61);
			ProtocolParser.WriteSingle(stream, instance.locktime);
		}
		if (instance.worldEntity != default(NetworkableId))
		{
			stream.WriteByte(64);
			ProtocolParser.WriteUInt64(stream, instance.worldEntity.Value);
		}
		if (instance.instanceData != null)
		{
			stream.WriteByte(74);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			InstanceData.Serialize(stream, instance.instanceData);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field instanceData (ProtoBuf.Item.InstanceData)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.heldEntity != default(NetworkableId))
		{
			stream.WriteByte(80);
			ProtocolParser.WriteUInt64(stream, instance.heldEntity.Value);
		}
		if (instance.conditionData != null)
		{
			stream.WriteByte(90);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			ConditionData.Serialize(stream, instance.conditionData);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field conditionData (ProtoBuf.Item.ConditionData)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.name != null)
		{
			stream.WriteByte(114);
			ProtocolParser.WriteString(stream, instance.name);
		}
		if (instance.text != null)
		{
			stream.WriteByte(122);
			ProtocolParser.WriteString(stream, instance.text);
		}
		if (instance.skinid != 0L)
		{
			stream.WriteByte(128);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, instance.skinid);
		}
		if (instance.cooktime != 0f)
		{
			stream.WriteByte(141);
			stream.WriteByte(1);
			ProtocolParser.WriteSingle(stream, instance.cooktime);
		}
		if (instance.streamerName != null)
		{
			stream.WriteByte(146);
			stream.WriteByte(1);
			ProtocolParser.WriteString(stream, instance.streamerName);
		}
		if (instance.ammoCount != 0)
		{
			stream.WriteByte(152);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ammoCount);
		}
		if (instance.ownership != null)
		{
			for (int i = 0; i < instance.ownership.Count; i++)
			{
				ItemOwnershipAmount instance2 = instance.ownership[i];
				stream.WriteByte(162);
				stream.WriteByte(1);
				BufferStream.RangeHandle range3 = stream.GetRange(5);
				int position3 = stream.Position;
				ItemOwnershipAmount.Serialize(stream, instance2);
				int val = stream.Position - position3;
				Span<byte> span3 = range3.GetSpan();
				int num3 = ProtocolParser.WriteUInt32((uint)val, span3, 0);
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
		if (instance.iconImageId != 0)
		{
			stream.WriteByte(168);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt32(stream, instance.iconImageId);
		}
		if (instance.attachmentid != 0L)
		{
			stream.WriteByte(176);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, instance.attachmentid);
		}
		if (instance.contents == null)
		{
			return;
		}
		stream.WriteByte(162);
		stream.WriteByte(6);
		BufferStream.RangeHandle range4 = stream.GetRange(5);
		int position4 = stream.Position;
		ItemContainer.Serialize(stream, instance.contents);
		int val2 = stream.Position - position4;
		Span<byte> span4 = range4.GetSpan();
		int num4 = ProtocolParser.WriteUInt32((uint)val2, span4, 0);
		if (num4 < 5)
		{
			span4[num4 - 1] |= 128;
			while (num4 < 4)
			{
				span4[num4++] = 128;
			}
			span4[4] = 0;
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.ItemId, ref UID.Value);
		action(UidType.NetworkableId, ref worldEntity.Value);
		instanceData?.InspectUids(action);
		action(UidType.NetworkableId, ref heldEntity.Value);
		conditionData?.InspectUids(action);
		if (ownership != null)
		{
			for (int i = 0; i < ownership.Count; i++)
			{
				ownership[i]?.InspectUids(action);
			}
		}
		contents?.InspectUids(action);
	}
}
