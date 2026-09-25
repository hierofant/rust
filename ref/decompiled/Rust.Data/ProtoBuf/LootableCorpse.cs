using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class LootableCorpse : IDisposable, Pool.IPooled, IProto<LootableCorpse>, IProto
{
	public class Private : IDisposable, Pool.IPooled, IProto<Private>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public List<ItemContainer> container;

		public static void ResetToPool(Private instance)
		{
			if (!instance.ShouldPool)
			{
				return;
			}
			if (instance.container != null)
			{
				for (int i = 0; i < instance.container.Count; i++)
				{
					if (instance.container[i] != null)
					{
						instance.container[i].ResetToPool();
						instance.container[i] = null;
					}
				}
				List<ItemContainer> obj = instance.container;
				Pool.Free(ref obj, freeElements: false);
				instance.container = obj;
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
				throw new Exception("Trying to dispose Private with ShouldPool set to false!");
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

		public void CopyTo(Private instance)
		{
			if (container != null)
			{
				instance.container = Pool.Get<List<ItemContainer>>();
				for (int i = 0; i < container.Count; i++)
				{
					ItemContainer item = container[i].Copy();
					instance.container.Add(item);
				}
			}
			else
			{
				instance.container = null;
			}
		}

		public Private Copy()
		{
			Private @private = Pool.Get<Private>();
			CopyTo(@private);
			return @private;
		}

		public static Private Deserialize(BufferStream stream)
		{
			Private @private = Pool.Get<Private>();
			Deserialize(stream, @private, isDelta: false);
			return @private;
		}

		public static Private DeserializeLengthDelimited(BufferStream stream)
		{
			Private @private = Pool.Get<Private>();
			DeserializeLengthDelimited(stream, @private, isDelta: false);
			return @private;
		}

		public static Private DeserializeLength(BufferStream stream, int length)
		{
			Private @private = Pool.Get<Private>();
			DeserializeLength(stream, length, @private, isDelta: false);
			return @private;
		}

		public static Private Deserialize(byte[] buffer)
		{
			Private @private = Pool.Get<Private>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, @private, isDelta: false);
			return @private;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, Private previous)
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

		public static Private Deserialize(BufferStream stream, Private instance, bool isDelta)
		{
			if (!isDelta && instance.container == null)
			{
				instance.container = Pool.Get<List<ItemContainer>>();
			}
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.LootableCorpse.Private");
				if (num == 10)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.LootableCorpse.Private");
					stream.ConsumeRepeatedElement();
					instance.container.Add(ItemContainer.DeserializeLengthDelimited(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				if (key.Field == 1)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.LootableCorpse.Private");
					ProtocolParser.SkipKey(stream, key);
				}
				else
				{
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.LootableCorpse.Private");
					ProtocolParser.SkipKey(stream, key);
				}
			}
			return instance;
		}

		public static Private DeserializeLengthDelimited(BufferStream stream, Private instance, bool isDelta)
		{
			if (!isDelta && instance.container == null)
			{
				instance.container = Pool.Get<List<ItemContainer>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.LootableCorpse.Private");
				if (num2 == 10)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.LootableCorpse.Private");
					stream.ConsumeRepeatedElement();
					instance.container.Add(ItemContainer.DeserializeLengthDelimited(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				if (key.Field == 1)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.LootableCorpse.Private");
					ProtocolParser.SkipKey(stream, key);
				}
				else
				{
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.LootableCorpse.Private");
					ProtocolParser.SkipKey(stream, key);
				}
			}
			return instance;
		}

		public static Private DeserializeLength(BufferStream stream, int length, Private instance, bool isDelta)
		{
			if (!isDelta && instance.container == null)
			{
				instance.container = Pool.Get<List<ItemContainer>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.LootableCorpse.Private");
				if (num2 == 10)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.LootableCorpse.Private");
					stream.ConsumeRepeatedElement();
					instance.container.Add(ItemContainer.DeserializeLengthDelimited(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				if (key.Field == 1)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.LootableCorpse.Private");
					ProtocolParser.SkipKey(stream, key);
				}
				else
				{
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.LootableCorpse.Private");
					ProtocolParser.SkipKey(stream, key);
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, Private instance, Private previous)
		{
			if (instance.container == null)
			{
				return;
			}
			for (int i = 0; i < instance.container.Count; i++)
			{
				ItemContainer itemContainer = instance.container[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				ItemContainer.SerializeDelta(stream, itemContainer, itemContainer);
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

		public static void Serialize(BufferStream stream, Private instance)
		{
			if (instance.container == null)
			{
				return;
			}
			for (int i = 0; i < instance.container.Count; i++)
			{
				ItemContainer instance2 = instance.container[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				ItemContainer.Serialize(stream, instance2);
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
			if (container != null)
			{
				for (int i = 0; i < container.Count; i++)
				{
					container[i]?.InspectUids(action);
				}
			}
		}
	}

	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public Private privateData;

	[NonSerialized]
	public ulong playerID;

	[NonSerialized]
	public string playerName;

	[NonSerialized]
	public uint underwearSkin;

	[NonSerialized]
	public string streamerName;

	[NonSerialized]
	public int paintballColor;

	public static void ResetToPool(LootableCorpse instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.privateData != null)
			{
				instance.privateData.ResetToPool();
				instance.privateData = null;
			}
			instance.playerID = 0uL;
			instance.playerName = string.Empty;
			instance.underwearSkin = 0u;
			instance.streamerName = string.Empty;
			instance.paintballColor = 0;
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
			throw new Exception("Trying to dispose LootableCorpse with ShouldPool set to false!");
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

	public void CopyTo(LootableCorpse instance)
	{
		if (privateData != null)
		{
			if (instance.privateData == null)
			{
				instance.privateData = privateData.Copy();
			}
			else
			{
				privateData.CopyTo(instance.privateData);
			}
		}
		else
		{
			instance.privateData = null;
		}
		instance.playerID = playerID;
		instance.playerName = playerName;
		instance.underwearSkin = underwearSkin;
		instance.streamerName = streamerName;
		instance.paintballColor = paintballColor;
	}

	public LootableCorpse Copy()
	{
		LootableCorpse lootableCorpse = Pool.Get<LootableCorpse>();
		CopyTo(lootableCorpse);
		return lootableCorpse;
	}

	public static LootableCorpse Deserialize(BufferStream stream)
	{
		LootableCorpse lootableCorpse = Pool.Get<LootableCorpse>();
		Deserialize(stream, lootableCorpse, isDelta: false);
		return lootableCorpse;
	}

	public static LootableCorpse DeserializeLengthDelimited(BufferStream stream)
	{
		LootableCorpse lootableCorpse = Pool.Get<LootableCorpse>();
		DeserializeLengthDelimited(stream, lootableCorpse, isDelta: false);
		return lootableCorpse;
	}

	public static LootableCorpse DeserializeLength(BufferStream stream, int length)
	{
		LootableCorpse lootableCorpse = Pool.Get<LootableCorpse>();
		DeserializeLength(stream, length, lootableCorpse, isDelta: false);
		return lootableCorpse;
	}

	public static LootableCorpse Deserialize(byte[] buffer)
	{
		LootableCorpse lootableCorpse = Pool.Get<LootableCorpse>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, lootableCorpse, isDelta: false);
		return lootableCorpse;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, LootableCorpse previous)
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

	public static LootableCorpse Deserialize(BufferStream stream, LootableCorpse instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.LootableCorpse");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				if (instance.privateData == null)
				{
					instance.privateData = Private.DeserializeLengthDelimited(stream);
				}
				else
				{
					Private.DeserializeLengthDelimited(stream, instance.privateData, isDelta);
				}
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				instance.playerID = ProtocolParser.ReadUInt64(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				instance.playerName = ProtocolParser.ReadString(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				instance.underwearSkin = ProtocolParser.ReadUInt32(stream);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				instance.streamerName = ProtocolParser.ReadString(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				instance.paintballColor = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.LootableCorpse");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static LootableCorpse DeserializeLengthDelimited(BufferStream stream, LootableCorpse instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.LootableCorpse");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				if (instance.privateData == null)
				{
					instance.privateData = Private.DeserializeLengthDelimited(stream);
				}
				else
				{
					Private.DeserializeLengthDelimited(stream, instance.privateData, isDelta);
				}
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				instance.playerID = ProtocolParser.ReadUInt64(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				instance.playerName = ProtocolParser.ReadString(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				instance.underwearSkin = ProtocolParser.ReadUInt32(stream);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				instance.streamerName = ProtocolParser.ReadString(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				instance.paintballColor = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.LootableCorpse");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static LootableCorpse DeserializeLength(BufferStream stream, int length, LootableCorpse instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.LootableCorpse");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				if (instance.privateData == null)
				{
					instance.privateData = Private.DeserializeLengthDelimited(stream);
				}
				else
				{
					Private.DeserializeLengthDelimited(stream, instance.privateData, isDelta);
				}
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				instance.playerID = ProtocolParser.ReadUInt64(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				instance.playerName = ProtocolParser.ReadString(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				instance.underwearSkin = ProtocolParser.ReadUInt32(stream);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				instance.streamerName = ProtocolParser.ReadString(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				instance.paintballColor = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.LootableCorpse");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.LootableCorpse");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, LootableCorpse instance, LootableCorpse previous)
	{
		if (instance.privateData != null)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			Private.SerializeDelta(stream, instance.privateData, previous.privateData);
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
		if (instance.playerID != previous.playerID)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, instance.playerID);
		}
		if (instance.playerName != null && instance.playerName != previous.playerName)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteString(stream, instance.playerName);
		}
		if (instance.underwearSkin != previous.underwearSkin)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt32(stream, instance.underwearSkin);
		}
		if (instance.streamerName != null && instance.streamerName != previous.streamerName)
		{
			stream.WriteByte(42);
			ProtocolParser.WriteString(stream, instance.streamerName);
		}
		if (instance.paintballColor != previous.paintballColor)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.paintballColor);
		}
	}

	public static void Serialize(BufferStream stream, LootableCorpse instance)
	{
		if (instance.privateData != null)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			Private.Serialize(stream, instance.privateData);
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
		if (instance.playerID != 0L)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, instance.playerID);
		}
		if (instance.playerName != null)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteString(stream, instance.playerName);
		}
		if (instance.underwearSkin != 0)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt32(stream, instance.underwearSkin);
		}
		if (instance.streamerName != null)
		{
			stream.WriteByte(42);
			ProtocolParser.WriteString(stream, instance.streamerName);
		}
		if (instance.paintballColor != 0)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.paintballColor);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		privateData?.InspectUids(action);
	}
}
