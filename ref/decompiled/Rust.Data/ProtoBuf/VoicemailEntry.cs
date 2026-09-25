using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class VoicemailEntry : IDisposable, Pool.IPooled, IProto<VoicemailEntry>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public uint audioId;

	[NonSerialized]
	public string userName;

	[NonSerialized]
	public long timestamp;

	public static void ResetToPool(VoicemailEntry instance)
	{
		if (instance.ShouldPool)
		{
			instance.audioId = 0u;
			instance.userName = string.Empty;
			instance.timestamp = 0L;
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
			throw new Exception("Trying to dispose VoicemailEntry with ShouldPool set to false!");
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

	public void CopyTo(VoicemailEntry instance)
	{
		instance.audioId = audioId;
		instance.userName = userName;
		instance.timestamp = timestamp;
	}

	public VoicemailEntry Copy()
	{
		VoicemailEntry voicemailEntry = Pool.Get<VoicemailEntry>();
		CopyTo(voicemailEntry);
		return voicemailEntry;
	}

	public static VoicemailEntry Deserialize(BufferStream stream)
	{
		VoicemailEntry voicemailEntry = Pool.Get<VoicemailEntry>();
		Deserialize(stream, voicemailEntry, isDelta: false);
		return voicemailEntry;
	}

	public static VoicemailEntry DeserializeLengthDelimited(BufferStream stream)
	{
		VoicemailEntry voicemailEntry = Pool.Get<VoicemailEntry>();
		DeserializeLengthDelimited(stream, voicemailEntry, isDelta: false);
		return voicemailEntry;
	}

	public static VoicemailEntry DeserializeLength(BufferStream stream, int length)
	{
		VoicemailEntry voicemailEntry = Pool.Get<VoicemailEntry>();
		DeserializeLength(stream, length, voicemailEntry, isDelta: false);
		return voicemailEntry;
	}

	public static VoicemailEntry Deserialize(byte[] buffer)
	{
		VoicemailEntry voicemailEntry = Pool.Get<VoicemailEntry>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, voicemailEntry, isDelta: false);
		return voicemailEntry;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, VoicemailEntry previous)
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

	public static VoicemailEntry Deserialize(BufferStream stream, VoicemailEntry instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.VoicemailEntry");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VoicemailEntry");
				instance.audioId = ProtocolParser.ReadUInt32(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VoicemailEntry");
				instance.userName = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VoicemailEntry");
				instance.timestamp = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VoicemailEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VoicemailEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VoicemailEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VoicemailEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static VoicemailEntry DeserializeLengthDelimited(BufferStream stream, VoicemailEntry instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.VoicemailEntry");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VoicemailEntry");
				instance.audioId = ProtocolParser.ReadUInt32(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VoicemailEntry");
				instance.userName = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VoicemailEntry");
				instance.timestamp = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VoicemailEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VoicemailEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VoicemailEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VoicemailEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static VoicemailEntry DeserializeLength(BufferStream stream, int length, VoicemailEntry instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.VoicemailEntry");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VoicemailEntry");
				instance.audioId = ProtocolParser.ReadUInt32(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VoicemailEntry");
				instance.userName = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VoicemailEntry");
				instance.timestamp = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VoicemailEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VoicemailEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VoicemailEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VoicemailEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, VoicemailEntry instance, VoicemailEntry previous)
	{
		if (instance.audioId != previous.audioId)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt32(stream, instance.audioId);
		}
		if (instance.userName != previous.userName)
		{
			if (instance.userName == null)
			{
				throw new ArgumentNullException("userName", "Required by proto specification.");
			}
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.userName);
		}
		stream.WriteByte(24);
		ProtocolParser.WriteUInt64(stream, (ulong)instance.timestamp);
	}

	public static void Serialize(BufferStream stream, VoicemailEntry instance)
	{
		if (instance.audioId != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt32(stream, instance.audioId);
		}
		if (instance.userName == null)
		{
			throw new ArgumentNullException("userName", "Required by proto specification.");
		}
		stream.WriteByte(18);
		ProtocolParser.WriteString(stream, instance.userName);
		if (instance.timestamp != 0L)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.timestamp);
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
