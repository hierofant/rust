using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class DigitalClock : IDisposable, Pool.IPooled, IProto<DigitalClock>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<DigitalClockAlarm> alarms;

	[NonSerialized]
	public bool muted;

	public static void ResetToPool(DigitalClock instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.alarms != null)
		{
			for (int i = 0; i < instance.alarms.Count; i++)
			{
				if (instance.alarms[i] != null)
				{
					instance.alarms[i].ResetToPool();
					instance.alarms[i] = null;
				}
			}
			List<DigitalClockAlarm> obj = instance.alarms;
			Pool.Free(ref obj, freeElements: false);
			instance.alarms = obj;
		}
		instance.muted = false;
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
			throw new Exception("Trying to dispose DigitalClock with ShouldPool set to false!");
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

	public void CopyTo(DigitalClock instance)
	{
		if (alarms != null)
		{
			instance.alarms = Pool.Get<List<DigitalClockAlarm>>();
			for (int i = 0; i < alarms.Count; i++)
			{
				DigitalClockAlarm item = alarms[i].Copy();
				instance.alarms.Add(item);
			}
		}
		else
		{
			instance.alarms = null;
		}
		instance.muted = muted;
	}

	public DigitalClock Copy()
	{
		DigitalClock digitalClock = Pool.Get<DigitalClock>();
		CopyTo(digitalClock);
		return digitalClock;
	}

	public static DigitalClock Deserialize(BufferStream stream)
	{
		DigitalClock digitalClock = Pool.Get<DigitalClock>();
		Deserialize(stream, digitalClock, isDelta: false);
		return digitalClock;
	}

	public static DigitalClock DeserializeLengthDelimited(BufferStream stream)
	{
		DigitalClock digitalClock = Pool.Get<DigitalClock>();
		DeserializeLengthDelimited(stream, digitalClock, isDelta: false);
		return digitalClock;
	}

	public static DigitalClock DeserializeLength(BufferStream stream, int length)
	{
		DigitalClock digitalClock = Pool.Get<DigitalClock>();
		DeserializeLength(stream, length, digitalClock, isDelta: false);
		return digitalClock;
	}

	public static DigitalClock Deserialize(byte[] buffer)
	{
		DigitalClock digitalClock = Pool.Get<DigitalClock>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, digitalClock, isDelta: false);
		return digitalClock;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, DigitalClock previous)
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

	public static DigitalClock Deserialize(BufferStream stream, DigitalClock instance, bool isDelta)
	{
		if (!isDelta && instance.alarms == null)
		{
			instance.alarms = Pool.Get<List<DigitalClockAlarm>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.DigitalClock");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.DigitalClock");
				if (instance.alarms.Count >= 5)
				{
					throw new ProtocolBufferException("Repeated field alarms exceeded max count of 5");
				}
				stream.ConsumeRepeatedElement();
				instance.alarms.Add(DigitalClockAlarm.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DigitalClock");
				instance.muted = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.DigitalClock");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DigitalClock");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DigitalClock");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DigitalClock DeserializeLengthDelimited(BufferStream stream, DigitalClock instance, bool isDelta)
	{
		if (!isDelta && instance.alarms == null)
		{
			instance.alarms = Pool.Get<List<DigitalClockAlarm>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.DigitalClock");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.DigitalClock");
				if (instance.alarms.Count >= 5)
				{
					throw new ProtocolBufferException("Repeated field alarms exceeded max count of 5");
				}
				stream.ConsumeRepeatedElement();
				instance.alarms.Add(DigitalClockAlarm.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DigitalClock");
				instance.muted = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.DigitalClock");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DigitalClock");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DigitalClock");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DigitalClock DeserializeLength(BufferStream stream, int length, DigitalClock instance, bool isDelta)
	{
		if (!isDelta && instance.alarms == null)
		{
			instance.alarms = Pool.Get<List<DigitalClockAlarm>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.DigitalClock");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.DigitalClock");
				if (instance.alarms.Count >= 5)
				{
					throw new ProtocolBufferException("Repeated field alarms exceeded max count of 5");
				}
				stream.ConsumeRepeatedElement();
				instance.alarms.Add(DigitalClockAlarm.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DigitalClock");
				instance.muted = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.DigitalClock");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DigitalClock");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DigitalClock");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, DigitalClock instance, DigitalClock previous)
	{
		if (instance.alarms != null)
		{
			if (instance.alarms.Count > 5)
			{
				throw new InvalidOperationException("Repeated field alarms exceeded max count of 5");
			}
			for (int i = 0; i < instance.alarms.Count; i++)
			{
				DigitalClockAlarm digitalClockAlarm = instance.alarms[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				DigitalClockAlarm.SerializeDelta(stream, digitalClockAlarm, digitalClockAlarm);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field alarms (ProtoBuf.DigitalClockAlarm)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		stream.WriteByte(16);
		ProtocolParser.WriteBool(stream, instance.muted);
	}

	public static void Serialize(BufferStream stream, DigitalClock instance)
	{
		if (instance.alarms != null)
		{
			if (instance.alarms.Count > 5)
			{
				throw new InvalidOperationException("Repeated field alarms exceeded max count of 5");
			}
			for (int i = 0; i < instance.alarms.Count; i++)
			{
				DigitalClockAlarm instance2 = instance.alarms[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				DigitalClockAlarm.Serialize(stream, instance2);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field alarms (ProtoBuf.DigitalClockAlarm)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.muted)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteBool(stream, instance.muted);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (alarms != null)
		{
			for (int i = 0; i < alarms.Count; i++)
			{
				alarms[i]?.InspectUids(action);
			}
		}
	}
}
