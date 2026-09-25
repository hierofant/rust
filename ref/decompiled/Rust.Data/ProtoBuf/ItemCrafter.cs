using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ItemCrafter : IDisposable, Pool.IPooled, IProto<ItemCrafter>, IProto
{
	public class Task : IDisposable, Pool.IPooled, IProto<Task>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public int itemID;

		[NonSerialized]
		public float remainingTime;

		[NonSerialized]
		public int taskUID;

		[NonSerialized]
		public bool cancelled;

		[NonSerialized]
		public Item.InstanceData instanceData;

		[NonSerialized]
		public int amount;

		[NonSerialized]
		public int skinID;

		[NonSerialized]
		public List<Item> takenItems;

		[NonSerialized]
		public int numCrafted;

		[NonSerialized]
		public float conditionScale;

		[NonSerialized]
		public NetworkableId workbenchEntity;

		public static void ResetToPool(Task instance)
		{
			if (!instance.ShouldPool)
			{
				return;
			}
			instance.itemID = 0;
			instance.remainingTime = 0f;
			instance.taskUID = 0;
			instance.cancelled = false;
			if (instance.instanceData != null)
			{
				instance.instanceData.ResetToPool();
				instance.instanceData = null;
			}
			instance.amount = 0;
			instance.skinID = 0;
			if (instance.takenItems != null)
			{
				for (int i = 0; i < instance.takenItems.Count; i++)
				{
					if (instance.takenItems[i] != null)
					{
						instance.takenItems[i].ResetToPool();
						instance.takenItems[i] = null;
					}
				}
				List<Item> obj = instance.takenItems;
				Pool.Free(ref obj, freeElements: false);
				instance.takenItems = obj;
			}
			instance.numCrafted = 0;
			instance.conditionScale = 0f;
			instance.workbenchEntity = default(NetworkableId);
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
				throw new Exception("Trying to dispose Task with ShouldPool set to false!");
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

		public void CopyTo(Task instance)
		{
			instance.itemID = itemID;
			instance.remainingTime = remainingTime;
			instance.taskUID = taskUID;
			instance.cancelled = cancelled;
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
			instance.amount = amount;
			instance.skinID = skinID;
			if (takenItems != null)
			{
				instance.takenItems = Pool.Get<List<Item>>();
				for (int i = 0; i < takenItems.Count; i++)
				{
					Item item = takenItems[i].Copy();
					instance.takenItems.Add(item);
				}
			}
			else
			{
				instance.takenItems = null;
			}
			instance.numCrafted = numCrafted;
			instance.conditionScale = conditionScale;
			instance.workbenchEntity = workbenchEntity;
		}

		public Task Copy()
		{
			Task task = Pool.Get<Task>();
			CopyTo(task);
			return task;
		}

		public static Task Deserialize(BufferStream stream)
		{
			Task task = Pool.Get<Task>();
			Deserialize(stream, task, isDelta: false);
			return task;
		}

		public static Task DeserializeLengthDelimited(BufferStream stream)
		{
			Task task = Pool.Get<Task>();
			DeserializeLengthDelimited(stream, task, isDelta: false);
			return task;
		}

		public static Task DeserializeLength(BufferStream stream, int length)
		{
			Task task = Pool.Get<Task>();
			DeserializeLength(stream, length, task, isDelta: false);
			return task;
		}

		public static Task Deserialize(byte[] buffer)
		{
			Task task = Pool.Get<Task>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, task, isDelta: false);
			return task;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, Task previous)
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

		public static Task Deserialize(BufferStream stream, Task instance, bool isDelta)
		{
			if (!isDelta && instance.takenItems == null)
			{
				instance.takenItems = Pool.Get<List<Item>>();
			}
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.ItemCrafter.Task");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					instance.itemID = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 21:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					instance.remainingTime = ProtocolParser.ReadSingle(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					instance.taskUID = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					instance.cancelled = ProtocolParser.ReadBool(stream);
					continue;
				case 42:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					if (instance.instanceData == null)
					{
						instance.instanceData = Item.InstanceData.DeserializeLengthDelimited(stream);
					}
					else
					{
						Item.InstanceData.DeserializeLengthDelimited(stream, instance.instanceData, isDelta);
					}
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					instance.amount = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					instance.skinID = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 66:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.ItemCrafter.Task");
					stream.ConsumeRepeatedElement();
					instance.takenItems.Add(Item.DeserializeLengthDelimited(stream));
					continue;
				case 72:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					instance.numCrafted = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 85:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					instance.conditionScale = ProtocolParser.ReadSingle(stream);
					continue;
				case 88:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					instance.workbenchEntity = new NetworkableId(ProtocolParser.ReadUInt64(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 9u:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 10u:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 11u:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Task DeserializeLengthDelimited(BufferStream stream, Task instance, bool isDelta)
		{
			if (!isDelta && instance.takenItems == null)
			{
				instance.takenItems = Pool.Get<List<Item>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.ItemCrafter.Task");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					instance.itemID = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 21:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					instance.remainingTime = ProtocolParser.ReadSingle(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					instance.taskUID = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					instance.cancelled = ProtocolParser.ReadBool(stream);
					continue;
				case 42:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					if (instance.instanceData == null)
					{
						instance.instanceData = Item.InstanceData.DeserializeLengthDelimited(stream);
					}
					else
					{
						Item.InstanceData.DeserializeLengthDelimited(stream, instance.instanceData, isDelta);
					}
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					instance.amount = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					instance.skinID = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 66:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.ItemCrafter.Task");
					stream.ConsumeRepeatedElement();
					instance.takenItems.Add(Item.DeserializeLengthDelimited(stream));
					continue;
				case 72:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					instance.numCrafted = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 85:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					instance.conditionScale = ProtocolParser.ReadSingle(stream);
					continue;
				case 88:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					instance.workbenchEntity = new NetworkableId(ProtocolParser.ReadUInt64(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 9u:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 10u:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 11u:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Task DeserializeLength(BufferStream stream, int length, Task instance, bool isDelta)
		{
			if (!isDelta && instance.takenItems == null)
			{
				instance.takenItems = Pool.Get<List<Item>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.ItemCrafter.Task");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					instance.itemID = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 21:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					instance.remainingTime = ProtocolParser.ReadSingle(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					instance.taskUID = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					instance.cancelled = ProtocolParser.ReadBool(stream);
					continue;
				case 42:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					if (instance.instanceData == null)
					{
						instance.instanceData = Item.InstanceData.DeserializeLengthDelimited(stream);
					}
					else
					{
						Item.InstanceData.DeserializeLengthDelimited(stream, instance.instanceData, isDelta);
					}
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					instance.amount = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					instance.skinID = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 66:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.ItemCrafter.Task");
					stream.ConsumeRepeatedElement();
					instance.takenItems.Add(Item.DeserializeLengthDelimited(stream));
					continue;
				case 72:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					instance.numCrafted = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 85:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					instance.conditionScale = ProtocolParser.ReadSingle(stream);
					continue;
				case 88:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					instance.workbenchEntity = new NetworkableId(ProtocolParser.ReadUInt64(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 9u:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 10u:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 11u:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ItemCrafter.Task");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, Task instance, Task previous)
		{
			if (instance.itemID != previous.itemID)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.itemID);
			}
			if (instance.remainingTime != previous.remainingTime)
			{
				stream.WriteByte(21);
				ProtocolParser.WriteSingle(stream, instance.remainingTime);
			}
			if (instance.taskUID != previous.taskUID)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.taskUID);
			}
			stream.WriteByte(32);
			ProtocolParser.WriteBool(stream, instance.cancelled);
			if (instance.instanceData != null)
			{
				stream.WriteByte(42);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				Item.InstanceData.SerializeDelta(stream, instance.instanceData, previous.instanceData);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field instanceData (ProtoBuf.Item.InstanceData)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
			if (instance.amount != previous.amount)
			{
				stream.WriteByte(48);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.amount);
			}
			if (instance.skinID != previous.skinID)
			{
				stream.WriteByte(56);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.skinID);
			}
			if (instance.takenItems != null)
			{
				for (int i = 0; i < instance.takenItems.Count; i++)
				{
					Item item = instance.takenItems[i];
					stream.WriteByte(66);
					BufferStream.RangeHandle range2 = stream.GetRange(5);
					int position2 = stream.Position;
					Item.SerializeDelta(stream, item, item);
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
			if (instance.numCrafted != previous.numCrafted)
			{
				stream.WriteByte(72);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.numCrafted);
			}
			if (instance.conditionScale != previous.conditionScale)
			{
				stream.WriteByte(85);
				ProtocolParser.WriteSingle(stream, instance.conditionScale);
			}
			stream.WriteByte(88);
			ProtocolParser.WriteUInt64(stream, instance.workbenchEntity.Value);
		}

		public static void Serialize(BufferStream stream, Task instance)
		{
			if (instance.itemID != 0)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.itemID);
			}
			if (instance.remainingTime != 0f)
			{
				stream.WriteByte(21);
				ProtocolParser.WriteSingle(stream, instance.remainingTime);
			}
			if (instance.taskUID != 0)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.taskUID);
			}
			if (instance.cancelled)
			{
				stream.WriteByte(32);
				ProtocolParser.WriteBool(stream, instance.cancelled);
			}
			if (instance.instanceData != null)
			{
				stream.WriteByte(42);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				Item.InstanceData.Serialize(stream, instance.instanceData);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field instanceData (ProtoBuf.Item.InstanceData)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
			if (instance.amount != 0)
			{
				stream.WriteByte(48);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.amount);
			}
			if (instance.skinID != 0)
			{
				stream.WriteByte(56);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.skinID);
			}
			if (instance.takenItems != null)
			{
				for (int i = 0; i < instance.takenItems.Count; i++)
				{
					Item instance2 = instance.takenItems[i];
					stream.WriteByte(66);
					BufferStream.RangeHandle range2 = stream.GetRange(5);
					int position2 = stream.Position;
					Item.Serialize(stream, instance2);
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
			if (instance.numCrafted != 0)
			{
				stream.WriteByte(72);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.numCrafted);
			}
			if (instance.conditionScale != 0f)
			{
				stream.WriteByte(85);
				ProtocolParser.WriteSingle(stream, instance.conditionScale);
			}
			if (instance.workbenchEntity != default(NetworkableId))
			{
				stream.WriteByte(88);
				ProtocolParser.WriteUInt64(stream, instance.workbenchEntity.Value);
			}
		}

		public void ToProto(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public void InspectUids(UidInspector<ulong> action)
		{
			instanceData?.InspectUids(action);
			if (takenItems != null)
			{
				for (int i = 0; i < takenItems.Count; i++)
				{
					takenItems[i]?.InspectUids(action);
				}
			}
			action(UidType.Clear, ref workbenchEntity.Value);
		}
	}

	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<Task> queue;

	public static void ResetToPool(ItemCrafter instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.queue != null)
		{
			for (int i = 0; i < instance.queue.Count; i++)
			{
				if (instance.queue[i] != null)
				{
					instance.queue[i].ResetToPool();
					instance.queue[i] = null;
				}
			}
			List<Task> obj = instance.queue;
			Pool.Free(ref obj, freeElements: false);
			instance.queue = obj;
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
			throw new Exception("Trying to dispose ItemCrafter with ShouldPool set to false!");
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

	public void CopyTo(ItemCrafter instance)
	{
		if (queue != null)
		{
			instance.queue = Pool.Get<List<Task>>();
			for (int i = 0; i < queue.Count; i++)
			{
				Task item = queue[i].Copy();
				instance.queue.Add(item);
			}
		}
		else
		{
			instance.queue = null;
		}
	}

	public ItemCrafter Copy()
	{
		ItemCrafter itemCrafter = Pool.Get<ItemCrafter>();
		CopyTo(itemCrafter);
		return itemCrafter;
	}

	public static ItemCrafter Deserialize(BufferStream stream)
	{
		ItemCrafter itemCrafter = Pool.Get<ItemCrafter>();
		Deserialize(stream, itemCrafter, isDelta: false);
		return itemCrafter;
	}

	public static ItemCrafter DeserializeLengthDelimited(BufferStream stream)
	{
		ItemCrafter itemCrafter = Pool.Get<ItemCrafter>();
		DeserializeLengthDelimited(stream, itemCrafter, isDelta: false);
		return itemCrafter;
	}

	public static ItemCrafter DeserializeLength(BufferStream stream, int length)
	{
		ItemCrafter itemCrafter = Pool.Get<ItemCrafter>();
		DeserializeLength(stream, length, itemCrafter, isDelta: false);
		return itemCrafter;
	}

	public static ItemCrafter Deserialize(byte[] buffer)
	{
		ItemCrafter itemCrafter = Pool.Get<ItemCrafter>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, itemCrafter, isDelta: false);
		return itemCrafter;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ItemCrafter previous)
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

	public static ItemCrafter Deserialize(BufferStream stream, ItemCrafter instance, bool isDelta)
	{
		if (!isDelta && instance.queue == null)
		{
			instance.queue = Pool.Get<List<Task>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ItemCrafter");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ItemCrafter");
				stream.ConsumeRepeatedElement();
				instance.queue.Add(Task.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ItemCrafter");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ItemCrafter");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ItemCrafter DeserializeLengthDelimited(BufferStream stream, ItemCrafter instance, bool isDelta)
	{
		if (!isDelta && instance.queue == null)
		{
			instance.queue = Pool.Get<List<Task>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ItemCrafter");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ItemCrafter");
				stream.ConsumeRepeatedElement();
				instance.queue.Add(Task.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ItemCrafter");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ItemCrafter");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ItemCrafter DeserializeLength(BufferStream stream, int length, ItemCrafter instance, bool isDelta)
	{
		if (!isDelta && instance.queue == null)
		{
			instance.queue = Pool.Get<List<Task>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ItemCrafter");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ItemCrafter");
				stream.ConsumeRepeatedElement();
				instance.queue.Add(Task.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ItemCrafter");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ItemCrafter");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ItemCrafter instance, ItemCrafter previous)
	{
		if (instance.queue == null)
		{
			return;
		}
		for (int i = 0; i < instance.queue.Count; i++)
		{
			Task task = instance.queue[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			Task.SerializeDelta(stream, task, task);
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

	public static void Serialize(BufferStream stream, ItemCrafter instance)
	{
		if (instance.queue == null)
		{
			return;
		}
		for (int i = 0; i < instance.queue.Count; i++)
		{
			Task instance2 = instance.queue[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			Task.Serialize(stream, instance2);
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

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (queue != null)
		{
			for (int i = 0; i < queue.Count; i++)
			{
				queue[i]?.InspectUids(action);
			}
		}
	}
}
