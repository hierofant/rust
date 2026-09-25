using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ReclaimManager : IDisposable, Pool.IPooled, IProto<ReclaimManager>, IProto
{
	public class ReclaimInfo : IDisposable, Pool.IPooled, IProto<ReclaimInfo>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public ulong victimID;

		[NonSerialized]
		public ItemContainer mainInventory;

		[NonSerialized]
		public ItemContainer wearInventory;

		[NonSerialized]
		public ItemContainer beltInventory;

		[NonSerialized]
		public ItemContainer backpackInventory;

		[NonSerialized]
		public int reclaimId;

		public static void ResetToPool(ReclaimInfo instance)
		{
			if (instance.ShouldPool)
			{
				instance.victimID = 0uL;
				if (instance.mainInventory != null)
				{
					instance.mainInventory.ResetToPool();
					instance.mainInventory = null;
				}
				if (instance.wearInventory != null)
				{
					instance.wearInventory.ResetToPool();
					instance.wearInventory = null;
				}
				if (instance.beltInventory != null)
				{
					instance.beltInventory.ResetToPool();
					instance.beltInventory = null;
				}
				if (instance.backpackInventory != null)
				{
					instance.backpackInventory.ResetToPool();
					instance.backpackInventory = null;
				}
				instance.reclaimId = 0;
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
				throw new Exception("Trying to dispose ReclaimInfo with ShouldPool set to false!");
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

		public void CopyTo(ReclaimInfo instance)
		{
			instance.victimID = victimID;
			if (mainInventory != null)
			{
				if (instance.mainInventory == null)
				{
					instance.mainInventory = mainInventory.Copy();
				}
				else
				{
					mainInventory.CopyTo(instance.mainInventory);
				}
			}
			else
			{
				instance.mainInventory = null;
			}
			if (wearInventory != null)
			{
				if (instance.wearInventory == null)
				{
					instance.wearInventory = wearInventory.Copy();
				}
				else
				{
					wearInventory.CopyTo(instance.wearInventory);
				}
			}
			else
			{
				instance.wearInventory = null;
			}
			if (beltInventory != null)
			{
				if (instance.beltInventory == null)
				{
					instance.beltInventory = beltInventory.Copy();
				}
				else
				{
					beltInventory.CopyTo(instance.beltInventory);
				}
			}
			else
			{
				instance.beltInventory = null;
			}
			if (backpackInventory != null)
			{
				if (instance.backpackInventory == null)
				{
					instance.backpackInventory = backpackInventory.Copy();
				}
				else
				{
					backpackInventory.CopyTo(instance.backpackInventory);
				}
			}
			else
			{
				instance.backpackInventory = null;
			}
			instance.reclaimId = reclaimId;
		}

		public ReclaimInfo Copy()
		{
			ReclaimInfo reclaimInfo = Pool.Get<ReclaimInfo>();
			CopyTo(reclaimInfo);
			return reclaimInfo;
		}

		public static ReclaimInfo Deserialize(BufferStream stream)
		{
			ReclaimInfo reclaimInfo = Pool.Get<ReclaimInfo>();
			Deserialize(stream, reclaimInfo, isDelta: false);
			return reclaimInfo;
		}

		public static ReclaimInfo DeserializeLengthDelimited(BufferStream stream)
		{
			ReclaimInfo reclaimInfo = Pool.Get<ReclaimInfo>();
			DeserializeLengthDelimited(stream, reclaimInfo, isDelta: false);
			return reclaimInfo;
		}

		public static ReclaimInfo DeserializeLength(BufferStream stream, int length)
		{
			ReclaimInfo reclaimInfo = Pool.Get<ReclaimInfo>();
			DeserializeLength(stream, length, reclaimInfo, isDelta: false);
			return reclaimInfo;
		}

		public static ReclaimInfo Deserialize(byte[] buffer)
		{
			ReclaimInfo reclaimInfo = Pool.Get<ReclaimInfo>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, reclaimInfo, isDelta: false);
			return reclaimInfo;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, ReclaimInfo previous)
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

		public static ReclaimInfo Deserialize(BufferStream stream, ReclaimInfo instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.ReclaimManager.ReclaimInfo");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					instance.victimID = ProtocolParser.ReadUInt64(stream);
					continue;
				case 34:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					if (instance.mainInventory == null)
					{
						instance.mainInventory = ItemContainer.DeserializeLengthDelimited(stream);
					}
					else
					{
						ItemContainer.DeserializeLengthDelimited(stream, instance.mainInventory, isDelta);
					}
					continue;
				case 42:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					if (instance.wearInventory == null)
					{
						instance.wearInventory = ItemContainer.DeserializeLengthDelimited(stream);
					}
					else
					{
						ItemContainer.DeserializeLengthDelimited(stream, instance.wearInventory, isDelta);
					}
					continue;
				case 50:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					if (instance.beltInventory == null)
					{
						instance.beltInventory = ItemContainer.DeserializeLengthDelimited(stream);
					}
					else
					{
						ItemContainer.DeserializeLengthDelimited(stream, instance.beltInventory, isDelta);
					}
					continue;
				case 58:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					if (instance.backpackInventory == null)
					{
						instance.backpackInventory = ItemContainer.DeserializeLengthDelimited(stream);
					}
					else
					{
						ItemContainer.DeserializeLengthDelimited(stream, instance.backpackInventory, isDelta);
					}
					continue;
				case 64:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					instance.reclaimId = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ReclaimManager.ReclaimInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static ReclaimInfo DeserializeLengthDelimited(BufferStream stream, ReclaimInfo instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.ReclaimManager.ReclaimInfo");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					instance.victimID = ProtocolParser.ReadUInt64(stream);
					continue;
				case 34:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					if (instance.mainInventory == null)
					{
						instance.mainInventory = ItemContainer.DeserializeLengthDelimited(stream);
					}
					else
					{
						ItemContainer.DeserializeLengthDelimited(stream, instance.mainInventory, isDelta);
					}
					continue;
				case 42:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					if (instance.wearInventory == null)
					{
						instance.wearInventory = ItemContainer.DeserializeLengthDelimited(stream);
					}
					else
					{
						ItemContainer.DeserializeLengthDelimited(stream, instance.wearInventory, isDelta);
					}
					continue;
				case 50:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					if (instance.beltInventory == null)
					{
						instance.beltInventory = ItemContainer.DeserializeLengthDelimited(stream);
					}
					else
					{
						ItemContainer.DeserializeLengthDelimited(stream, instance.beltInventory, isDelta);
					}
					continue;
				case 58:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					if (instance.backpackInventory == null)
					{
						instance.backpackInventory = ItemContainer.DeserializeLengthDelimited(stream);
					}
					else
					{
						ItemContainer.DeserializeLengthDelimited(stream, instance.backpackInventory, isDelta);
					}
					continue;
				case 64:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					instance.reclaimId = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ReclaimManager.ReclaimInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static ReclaimInfo DeserializeLength(BufferStream stream, int length, ReclaimInfo instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.ReclaimManager.ReclaimInfo");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					instance.victimID = ProtocolParser.ReadUInt64(stream);
					continue;
				case 34:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					if (instance.mainInventory == null)
					{
						instance.mainInventory = ItemContainer.DeserializeLengthDelimited(stream);
					}
					else
					{
						ItemContainer.DeserializeLengthDelimited(stream, instance.mainInventory, isDelta);
					}
					continue;
				case 42:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					if (instance.wearInventory == null)
					{
						instance.wearInventory = ItemContainer.DeserializeLengthDelimited(stream);
					}
					else
					{
						ItemContainer.DeserializeLengthDelimited(stream, instance.wearInventory, isDelta);
					}
					continue;
				case 50:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					if (instance.beltInventory == null)
					{
						instance.beltInventory = ItemContainer.DeserializeLengthDelimited(stream);
					}
					else
					{
						ItemContainer.DeserializeLengthDelimited(stream, instance.beltInventory, isDelta);
					}
					continue;
				case 58:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					if (instance.backpackInventory == null)
					{
						instance.backpackInventory = ItemContainer.DeserializeLengthDelimited(stream);
					}
					else
					{
						ItemContainer.DeserializeLengthDelimited(stream, instance.backpackInventory, isDelta);
					}
					continue;
				case 64:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					instance.reclaimId = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager.ReclaimInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ReclaimManager.ReclaimInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, ReclaimInfo instance, ReclaimInfo previous)
		{
			if (instance.victimID != previous.victimID)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, instance.victimID);
			}
			if (instance.mainInventory == null)
			{
				throw new ArgumentNullException("mainInventory", "Required by proto specification.");
			}
			stream.WriteByte(34);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			ItemContainer.SerializeDelta(stream, instance.mainInventory, previous.mainInventory);
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
			if (instance.wearInventory == null)
			{
				throw new ArgumentNullException("wearInventory", "Required by proto specification.");
			}
			stream.WriteByte(42);
			BufferStream.RangeHandle range2 = stream.GetRange(5);
			int position2 = stream.Position;
			ItemContainer.SerializeDelta(stream, instance.wearInventory, previous.wearInventory);
			int val2 = stream.Position - position2;
			Span<byte> span2 = range2.GetSpan();
			int num2 = ProtocolParser.WriteUInt32((uint)val2, span2, 0);
			if (num2 < 5)
			{
				span2[num2 - 1] |= 128;
				while (num2 < 4)
				{
					span2[num2++] = 128;
				}
				span2[4] = 0;
			}
			if (instance.beltInventory == null)
			{
				throw new ArgumentNullException("beltInventory", "Required by proto specification.");
			}
			stream.WriteByte(50);
			BufferStream.RangeHandle range3 = stream.GetRange(5);
			int position3 = stream.Position;
			ItemContainer.SerializeDelta(stream, instance.beltInventory, previous.beltInventory);
			int val3 = stream.Position - position3;
			Span<byte> span3 = range3.GetSpan();
			int num3 = ProtocolParser.WriteUInt32((uint)val3, span3, 0);
			if (num3 < 5)
			{
				span3[num3 - 1] |= 128;
				while (num3 < 4)
				{
					span3[num3++] = 128;
				}
				span3[4] = 0;
			}
			if (instance.backpackInventory == null)
			{
				throw new ArgumentNullException("backpackInventory", "Required by proto specification.");
			}
			stream.WriteByte(58);
			BufferStream.RangeHandle range4 = stream.GetRange(5);
			int position4 = stream.Position;
			ItemContainer.SerializeDelta(stream, instance.backpackInventory, previous.backpackInventory);
			int val4 = stream.Position - position4;
			Span<byte> span4 = range4.GetSpan();
			int num4 = ProtocolParser.WriteUInt32((uint)val4, span4, 0);
			if (num4 < 5)
			{
				span4[num4 - 1] |= 128;
				while (num4 < 4)
				{
					span4[num4++] = 128;
				}
				span4[4] = 0;
			}
			if (instance.reclaimId != previous.reclaimId)
			{
				stream.WriteByte(64);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.reclaimId);
			}
		}

		public static void Serialize(BufferStream stream, ReclaimInfo instance)
		{
			if (instance.victimID != 0L)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, instance.victimID);
			}
			if (instance.mainInventory == null)
			{
				throw new ArgumentNullException("mainInventory", "Required by proto specification.");
			}
			stream.WriteByte(34);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			ItemContainer.Serialize(stream, instance.mainInventory);
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
			if (instance.wearInventory == null)
			{
				throw new ArgumentNullException("wearInventory", "Required by proto specification.");
			}
			stream.WriteByte(42);
			BufferStream.RangeHandle range2 = stream.GetRange(5);
			int position2 = stream.Position;
			ItemContainer.Serialize(stream, instance.wearInventory);
			int val2 = stream.Position - position2;
			Span<byte> span2 = range2.GetSpan();
			int num2 = ProtocolParser.WriteUInt32((uint)val2, span2, 0);
			if (num2 < 5)
			{
				span2[num2 - 1] |= 128;
				while (num2 < 4)
				{
					span2[num2++] = 128;
				}
				span2[4] = 0;
			}
			if (instance.beltInventory == null)
			{
				throw new ArgumentNullException("beltInventory", "Required by proto specification.");
			}
			stream.WriteByte(50);
			BufferStream.RangeHandle range3 = stream.GetRange(5);
			int position3 = stream.Position;
			ItemContainer.Serialize(stream, instance.beltInventory);
			int val3 = stream.Position - position3;
			Span<byte> span3 = range3.GetSpan();
			int num3 = ProtocolParser.WriteUInt32((uint)val3, span3, 0);
			if (num3 < 5)
			{
				span3[num3 - 1] |= 128;
				while (num3 < 4)
				{
					span3[num3++] = 128;
				}
				span3[4] = 0;
			}
			if (instance.backpackInventory == null)
			{
				throw new ArgumentNullException("backpackInventory", "Required by proto specification.");
			}
			stream.WriteByte(58);
			BufferStream.RangeHandle range4 = stream.GetRange(5);
			int position4 = stream.Position;
			ItemContainer.Serialize(stream, instance.backpackInventory);
			int val4 = stream.Position - position4;
			Span<byte> span4 = range4.GetSpan();
			int num4 = ProtocolParser.WriteUInt32((uint)val4, span4, 0);
			if (num4 < 5)
			{
				span4[num4 - 1] |= 128;
				while (num4 < 4)
				{
					span4[num4++] = 128;
				}
				span4[4] = 0;
			}
			if (instance.reclaimId != 0)
			{
				stream.WriteByte(64);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.reclaimId);
			}
		}

		public void ToProto(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public void InspectUids(UidInspector<ulong> action)
		{
			mainInventory?.InspectUids(action);
			wearInventory?.InspectUids(action);
			beltInventory?.InspectUids(action);
			backpackInventory?.InspectUids(action);
		}
	}

	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<ReclaimInfo> reclaimEntries;

	[NonSerialized]
	public int lastReclaimID;

	public static void ResetToPool(ReclaimManager instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.reclaimEntries != null)
		{
			for (int i = 0; i < instance.reclaimEntries.Count; i++)
			{
				if (instance.reclaimEntries[i] != null)
				{
					instance.reclaimEntries[i].ResetToPool();
					instance.reclaimEntries[i] = null;
				}
			}
			List<ReclaimInfo> obj = instance.reclaimEntries;
			Pool.Free(ref obj, freeElements: false);
			instance.reclaimEntries = obj;
		}
		instance.lastReclaimID = 0;
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
			throw new Exception("Trying to dispose ReclaimManager with ShouldPool set to false!");
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

	public void CopyTo(ReclaimManager instance)
	{
		if (reclaimEntries != null)
		{
			instance.reclaimEntries = Pool.Get<List<ReclaimInfo>>();
			for (int i = 0; i < reclaimEntries.Count; i++)
			{
				ReclaimInfo item = reclaimEntries[i].Copy();
				instance.reclaimEntries.Add(item);
			}
		}
		else
		{
			instance.reclaimEntries = null;
		}
		instance.lastReclaimID = lastReclaimID;
	}

	public ReclaimManager Copy()
	{
		ReclaimManager reclaimManager = Pool.Get<ReclaimManager>();
		CopyTo(reclaimManager);
		return reclaimManager;
	}

	public static ReclaimManager Deserialize(BufferStream stream)
	{
		ReclaimManager reclaimManager = Pool.Get<ReclaimManager>();
		Deserialize(stream, reclaimManager, isDelta: false);
		return reclaimManager;
	}

	public static ReclaimManager DeserializeLengthDelimited(BufferStream stream)
	{
		ReclaimManager reclaimManager = Pool.Get<ReclaimManager>();
		DeserializeLengthDelimited(stream, reclaimManager, isDelta: false);
		return reclaimManager;
	}

	public static ReclaimManager DeserializeLength(BufferStream stream, int length)
	{
		ReclaimManager reclaimManager = Pool.Get<ReclaimManager>();
		DeserializeLength(stream, length, reclaimManager, isDelta: false);
		return reclaimManager;
	}

	public static ReclaimManager Deserialize(byte[] buffer)
	{
		ReclaimManager reclaimManager = Pool.Get<ReclaimManager>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, reclaimManager, isDelta: false);
		return reclaimManager;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ReclaimManager previous)
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

	public static ReclaimManager Deserialize(BufferStream stream, ReclaimManager instance, bool isDelta)
	{
		if (!isDelta && instance.reclaimEntries == null)
		{
			instance.reclaimEntries = Pool.Get<List<ReclaimInfo>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ReclaimManager");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ReclaimManager");
				stream.ConsumeRepeatedElement();
				instance.reclaimEntries.Add(ReclaimInfo.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager");
				instance.lastReclaimID = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ReclaimManager");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ReclaimManager");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ReclaimManager DeserializeLengthDelimited(BufferStream stream, ReclaimManager instance, bool isDelta)
	{
		if (!isDelta && instance.reclaimEntries == null)
		{
			instance.reclaimEntries = Pool.Get<List<ReclaimInfo>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ReclaimManager");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ReclaimManager");
				stream.ConsumeRepeatedElement();
				instance.reclaimEntries.Add(ReclaimInfo.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager");
				instance.lastReclaimID = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ReclaimManager");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ReclaimManager");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ReclaimManager DeserializeLength(BufferStream stream, int length, ReclaimManager instance, bool isDelta)
	{
		if (!isDelta && instance.reclaimEntries == null)
		{
			instance.reclaimEntries = Pool.Get<List<ReclaimInfo>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ReclaimManager");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ReclaimManager");
				stream.ConsumeRepeatedElement();
				instance.reclaimEntries.Add(ReclaimInfo.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager");
				instance.lastReclaimID = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ReclaimManager");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ReclaimManager");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ReclaimManager");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ReclaimManager instance, ReclaimManager previous)
	{
		if (instance.reclaimEntries != null)
		{
			for (int i = 0; i < instance.reclaimEntries.Count; i++)
			{
				ReclaimInfo reclaimInfo = instance.reclaimEntries[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				ReclaimInfo.SerializeDelta(stream, reclaimInfo, reclaimInfo);
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
		if (instance.lastReclaimID != previous.lastReclaimID)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.lastReclaimID);
		}
	}

	public static void Serialize(BufferStream stream, ReclaimManager instance)
	{
		if (instance.reclaimEntries != null)
		{
			for (int i = 0; i < instance.reclaimEntries.Count; i++)
			{
				ReclaimInfo instance2 = instance.reclaimEntries[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				ReclaimInfo.Serialize(stream, instance2);
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
		if (instance.lastReclaimID != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.lastReclaimID);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (reclaimEntries != null)
		{
			for (int i = 0; i < reclaimEntries.Count; i++)
			{
				reclaimEntries[i]?.InspectUids(action);
			}
		}
	}
}
