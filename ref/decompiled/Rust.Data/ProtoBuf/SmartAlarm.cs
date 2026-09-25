using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class SmartAlarm : IDisposable, Pool.IPooled, IProto<SmartAlarm>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<ulong> subscriptions;

	[NonSerialized]
	public string notificationTitle;

	[NonSerialized]
	public string notificationBody;

	public static void ResetToPool(SmartAlarm instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.subscriptions != null)
			{
				List<ulong> obj = instance.subscriptions;
				Pool.FreeUnmanaged(ref obj);
				instance.subscriptions = obj;
			}
			instance.notificationTitle = string.Empty;
			instance.notificationBody = string.Empty;
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
			throw new Exception("Trying to dispose SmartAlarm with ShouldPool set to false!");
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

	public void CopyTo(SmartAlarm instance)
	{
		if (subscriptions != null)
		{
			instance.subscriptions = Pool.Get<List<ulong>>();
			for (int i = 0; i < subscriptions.Count; i++)
			{
				ulong item = subscriptions[i];
				instance.subscriptions.Add(item);
			}
		}
		else
		{
			instance.subscriptions = null;
		}
		instance.notificationTitle = notificationTitle;
		instance.notificationBody = notificationBody;
	}

	public SmartAlarm Copy()
	{
		SmartAlarm smartAlarm = Pool.Get<SmartAlarm>();
		CopyTo(smartAlarm);
		return smartAlarm;
	}

	public static SmartAlarm Deserialize(BufferStream stream)
	{
		SmartAlarm smartAlarm = Pool.Get<SmartAlarm>();
		Deserialize(stream, smartAlarm, isDelta: false);
		return smartAlarm;
	}

	public static SmartAlarm DeserializeLengthDelimited(BufferStream stream)
	{
		SmartAlarm smartAlarm = Pool.Get<SmartAlarm>();
		DeserializeLengthDelimited(stream, smartAlarm, isDelta: false);
		return smartAlarm;
	}

	public static SmartAlarm DeserializeLength(BufferStream stream, int length)
	{
		SmartAlarm smartAlarm = Pool.Get<SmartAlarm>();
		DeserializeLength(stream, length, smartAlarm, isDelta: false);
		return smartAlarm;
	}

	public static SmartAlarm Deserialize(byte[] buffer)
	{
		SmartAlarm smartAlarm = Pool.Get<SmartAlarm>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, smartAlarm, isDelta: false);
		return smartAlarm;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, SmartAlarm previous)
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

	public static SmartAlarm Deserialize(BufferStream stream, SmartAlarm instance, bool isDelta)
	{
		if (!isDelta && instance.subscriptions == null)
		{
			instance.subscriptions = Pool.Get<List<ulong>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.SmartAlarm");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.SmartAlarm");
				stream.ConsumeRepeatedElement();
				instance.subscriptions.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SmartAlarm");
				instance.notificationTitle = ProtocolParser.ReadString(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SmartAlarm");
				instance.notificationBody = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.SmartAlarm");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SmartAlarm");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SmartAlarm");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SmartAlarm");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static SmartAlarm DeserializeLengthDelimited(BufferStream stream, SmartAlarm instance, bool isDelta)
	{
		if (!isDelta && instance.subscriptions == null)
		{
			instance.subscriptions = Pool.Get<List<ulong>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.SmartAlarm");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.SmartAlarm");
				stream.ConsumeRepeatedElement();
				instance.subscriptions.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SmartAlarm");
				instance.notificationTitle = ProtocolParser.ReadString(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SmartAlarm");
				instance.notificationBody = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.SmartAlarm");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SmartAlarm");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SmartAlarm");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SmartAlarm");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static SmartAlarm DeserializeLength(BufferStream stream, int length, SmartAlarm instance, bool isDelta)
	{
		if (!isDelta && instance.subscriptions == null)
		{
			instance.subscriptions = Pool.Get<List<ulong>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.SmartAlarm");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.SmartAlarm");
				stream.ConsumeRepeatedElement();
				instance.subscriptions.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SmartAlarm");
				instance.notificationTitle = ProtocolParser.ReadString(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SmartAlarm");
				instance.notificationBody = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.SmartAlarm");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SmartAlarm");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SmartAlarm");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SmartAlarm");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, SmartAlarm instance, SmartAlarm previous)
	{
		if (instance.subscriptions != null)
		{
			for (int i = 0; i < instance.subscriptions.Count; i++)
			{
				ulong val = instance.subscriptions[i];
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, val);
			}
		}
		if (instance.notificationTitle != null && instance.notificationTitle != previous.notificationTitle)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.notificationTitle);
		}
		if (instance.notificationBody != null && instance.notificationBody != previous.notificationBody)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteString(stream, instance.notificationBody);
		}
	}

	public static void Serialize(BufferStream stream, SmartAlarm instance)
	{
		if (instance.subscriptions != null)
		{
			for (int i = 0; i < instance.subscriptions.Count; i++)
			{
				ulong val = instance.subscriptions[i];
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, val);
			}
		}
		if (instance.notificationTitle != null)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.notificationTitle);
		}
		if (instance.notificationBody != null)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteString(stream, instance.notificationBody);
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
