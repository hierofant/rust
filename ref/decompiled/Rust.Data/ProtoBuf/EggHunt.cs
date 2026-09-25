using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class EggHunt : IDisposable, Pool.IPooled, IProto<EggHunt>, IProto
{
	public class EggHunter : IDisposable, Pool.IPooled, IProto<EggHunter>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public string displayName;

		[NonSerialized]
		public int numEggs;

		[NonSerialized]
		public ulong playerID;

		public static void ResetToPool(EggHunter instance)
		{
			if (instance.ShouldPool)
			{
				instance.displayName = string.Empty;
				instance.numEggs = 0;
				instance.playerID = 0uL;
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
				throw new Exception("Trying to dispose EggHunter with ShouldPool set to false!");
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

		public void CopyTo(EggHunter instance)
		{
			instance.displayName = displayName;
			instance.numEggs = numEggs;
			instance.playerID = playerID;
		}

		public EggHunter Copy()
		{
			EggHunter eggHunter = Pool.Get<EggHunter>();
			CopyTo(eggHunter);
			return eggHunter;
		}

		public static EggHunter Deserialize(BufferStream stream)
		{
			EggHunter eggHunter = Pool.Get<EggHunter>();
			Deserialize(stream, eggHunter, isDelta: false);
			return eggHunter;
		}

		public static EggHunter DeserializeLengthDelimited(BufferStream stream)
		{
			EggHunter eggHunter = Pool.Get<EggHunter>();
			DeserializeLengthDelimited(stream, eggHunter, isDelta: false);
			return eggHunter;
		}

		public static EggHunter DeserializeLength(BufferStream stream, int length)
		{
			EggHunter eggHunter = Pool.Get<EggHunter>();
			DeserializeLength(stream, length, eggHunter, isDelta: false);
			return eggHunter;
		}

		public static EggHunter Deserialize(byte[] buffer)
		{
			EggHunter eggHunter = Pool.Get<EggHunter>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, eggHunter, isDelta: false);
			return eggHunter;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, EggHunter previous)
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

		public static EggHunter Deserialize(BufferStream stream, EggHunter instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.EggHunt.EggHunter");
				switch (num)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.EggHunt.EggHunter");
					instance.displayName = ProtocolParser.ReadString(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.EggHunt.EggHunter");
					instance.numEggs = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.EggHunt.EggHunter");
					instance.playerID = ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.EggHunt.EggHunter");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.EggHunt.EggHunter");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.EggHunt.EggHunter");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.EggHunt.EggHunter");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static EggHunter DeserializeLengthDelimited(BufferStream stream, EggHunter instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.EggHunt.EggHunter");
				switch (num2)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.EggHunt.EggHunter");
					instance.displayName = ProtocolParser.ReadString(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.EggHunt.EggHunter");
					instance.numEggs = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.EggHunt.EggHunter");
					instance.playerID = ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.EggHunt.EggHunter");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.EggHunt.EggHunter");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.EggHunt.EggHunter");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.EggHunt.EggHunter");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static EggHunter DeserializeLength(BufferStream stream, int length, EggHunter instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.EggHunt.EggHunter");
				switch (num2)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.EggHunt.EggHunter");
					instance.displayName = ProtocolParser.ReadString(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.EggHunt.EggHunter");
					instance.numEggs = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.EggHunt.EggHunter");
					instance.playerID = ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.EggHunt.EggHunter");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.EggHunt.EggHunter");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.EggHunt.EggHunter");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.EggHunt.EggHunter");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, EggHunter instance, EggHunter previous)
		{
			if (instance.displayName != null && instance.displayName != previous.displayName)
			{
				stream.WriteByte(10);
				ProtocolParser.WriteString(stream, instance.displayName);
			}
			if (instance.numEggs != previous.numEggs)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.numEggs);
			}
			if (instance.playerID != previous.playerID)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, instance.playerID);
			}
		}

		public static void Serialize(BufferStream stream, EggHunter instance)
		{
			if (instance.displayName != null)
			{
				stream.WriteByte(10);
				ProtocolParser.WriteString(stream, instance.displayName);
			}
			if (instance.numEggs != 0)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.numEggs);
			}
			if (instance.playerID != 0L)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, instance.playerID);
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

	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<EggHunter> hunters;

	public static void ResetToPool(EggHunt instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.hunters != null)
		{
			for (int i = 0; i < instance.hunters.Count; i++)
			{
				if (instance.hunters[i] != null)
				{
					instance.hunters[i].ResetToPool();
					instance.hunters[i] = null;
				}
			}
			List<EggHunter> obj = instance.hunters;
			Pool.Free(ref obj, freeElements: false);
			instance.hunters = obj;
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
			throw new Exception("Trying to dispose EggHunt with ShouldPool set to false!");
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

	public void CopyTo(EggHunt instance)
	{
		if (hunters != null)
		{
			instance.hunters = Pool.Get<List<EggHunter>>();
			for (int i = 0; i < hunters.Count; i++)
			{
				EggHunter item = hunters[i].Copy();
				instance.hunters.Add(item);
			}
		}
		else
		{
			instance.hunters = null;
		}
	}

	public EggHunt Copy()
	{
		EggHunt eggHunt = Pool.Get<EggHunt>();
		CopyTo(eggHunt);
		return eggHunt;
	}

	public static EggHunt Deserialize(BufferStream stream)
	{
		EggHunt eggHunt = Pool.Get<EggHunt>();
		Deserialize(stream, eggHunt, isDelta: false);
		return eggHunt;
	}

	public static EggHunt DeserializeLengthDelimited(BufferStream stream)
	{
		EggHunt eggHunt = Pool.Get<EggHunt>();
		DeserializeLengthDelimited(stream, eggHunt, isDelta: false);
		return eggHunt;
	}

	public static EggHunt DeserializeLength(BufferStream stream, int length)
	{
		EggHunt eggHunt = Pool.Get<EggHunt>();
		DeserializeLength(stream, length, eggHunt, isDelta: false);
		return eggHunt;
	}

	public static EggHunt Deserialize(byte[] buffer)
	{
		EggHunt eggHunt = Pool.Get<EggHunt>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, eggHunt, isDelta: false);
		return eggHunt;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, EggHunt previous)
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

	public static EggHunt Deserialize(BufferStream stream, EggHunt instance, bool isDelta)
	{
		if (!isDelta && instance.hunters == null)
		{
			instance.hunters = Pool.Get<List<EggHunter>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.EggHunt");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.EggHunt");
				stream.ConsumeRepeatedElement();
				instance.hunters.Add(EggHunter.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.EggHunt");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.EggHunt");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static EggHunt DeserializeLengthDelimited(BufferStream stream, EggHunt instance, bool isDelta)
	{
		if (!isDelta && instance.hunters == null)
		{
			instance.hunters = Pool.Get<List<EggHunter>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.EggHunt");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.EggHunt");
				stream.ConsumeRepeatedElement();
				instance.hunters.Add(EggHunter.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.EggHunt");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.EggHunt");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static EggHunt DeserializeLength(BufferStream stream, int length, EggHunt instance, bool isDelta)
	{
		if (!isDelta && instance.hunters == null)
		{
			instance.hunters = Pool.Get<List<EggHunter>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.EggHunt");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.EggHunt");
				stream.ConsumeRepeatedElement();
				instance.hunters.Add(EggHunter.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.EggHunt");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.EggHunt");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, EggHunt instance, EggHunt previous)
	{
		if (instance.hunters == null)
		{
			return;
		}
		for (int i = 0; i < instance.hunters.Count; i++)
		{
			EggHunter eggHunter = instance.hunters[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			EggHunter.SerializeDelta(stream, eggHunter, eggHunter);
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

	public static void Serialize(BufferStream stream, EggHunt instance)
	{
		if (instance.hunters == null)
		{
			return;
		}
		for (int i = 0; i < instance.hunters.Count; i++)
		{
			EggHunter instance2 = instance.hunters[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			EggHunter.Serialize(stream, instance2);
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
		if (hunters != null)
		{
			for (int i = 0; i < hunters.Count; i++)
			{
				hunters[i]?.InspectUids(action);
			}
		}
	}
}
