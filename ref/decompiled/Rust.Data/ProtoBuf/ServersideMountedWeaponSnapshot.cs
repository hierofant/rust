using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ServersideMountedWeaponSnapshot : IDisposable, Pool.IPooled, IProto<ServersideMountedWeaponSnapshot>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public float time;

	[NonSerialized]
	public float pitch;

	[NonSerialized]
	public float yaw;

	[NonSerialized]
	public bool force;

	public static void ResetToPool(ServersideMountedWeaponSnapshot instance)
	{
		if (instance.ShouldPool)
		{
			instance.time = 0f;
			instance.pitch = 0f;
			instance.yaw = 0f;
			instance.force = false;
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
			throw new Exception("Trying to dispose ServersideMountedWeaponSnapshot with ShouldPool set to false!");
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

	public void CopyTo(ServersideMountedWeaponSnapshot instance)
	{
		instance.time = time;
		instance.pitch = pitch;
		instance.yaw = yaw;
		instance.force = force;
	}

	public ServersideMountedWeaponSnapshot Copy()
	{
		ServersideMountedWeaponSnapshot serversideMountedWeaponSnapshot = Pool.Get<ServersideMountedWeaponSnapshot>();
		CopyTo(serversideMountedWeaponSnapshot);
		return serversideMountedWeaponSnapshot;
	}

	public static ServersideMountedWeaponSnapshot Deserialize(BufferStream stream)
	{
		ServersideMountedWeaponSnapshot serversideMountedWeaponSnapshot = Pool.Get<ServersideMountedWeaponSnapshot>();
		Deserialize(stream, serversideMountedWeaponSnapshot, isDelta: false);
		return serversideMountedWeaponSnapshot;
	}

	public static ServersideMountedWeaponSnapshot DeserializeLengthDelimited(BufferStream stream)
	{
		ServersideMountedWeaponSnapshot serversideMountedWeaponSnapshot = Pool.Get<ServersideMountedWeaponSnapshot>();
		DeserializeLengthDelimited(stream, serversideMountedWeaponSnapshot, isDelta: false);
		return serversideMountedWeaponSnapshot;
	}

	public static ServersideMountedWeaponSnapshot DeserializeLength(BufferStream stream, int length)
	{
		ServersideMountedWeaponSnapshot serversideMountedWeaponSnapshot = Pool.Get<ServersideMountedWeaponSnapshot>();
		DeserializeLength(stream, length, serversideMountedWeaponSnapshot, isDelta: false);
		return serversideMountedWeaponSnapshot;
	}

	public static ServersideMountedWeaponSnapshot Deserialize(byte[] buffer)
	{
		ServersideMountedWeaponSnapshot serversideMountedWeaponSnapshot = Pool.Get<ServersideMountedWeaponSnapshot>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, serversideMountedWeaponSnapshot, isDelta: false);
		return serversideMountedWeaponSnapshot;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ServersideMountedWeaponSnapshot previous)
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

	public static ServersideMountedWeaponSnapshot Deserialize(BufferStream stream, ServersideMountedWeaponSnapshot instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ServersideMountedWeaponSnapshot");
			switch (num)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ServersideMountedWeaponSnapshot");
				instance.time = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ServersideMountedWeaponSnapshot");
				instance.pitch = ProtocolParser.ReadSingle(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ServersideMountedWeaponSnapshot");
				instance.yaw = ProtocolParser.ReadSingle(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ServersideMountedWeaponSnapshot");
				instance.force = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ServersideMountedWeaponSnapshot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ServersideMountedWeaponSnapshot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ServersideMountedWeaponSnapshot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ServersideMountedWeaponSnapshot");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ServersideMountedWeaponSnapshot");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ServersideMountedWeaponSnapshot DeserializeLengthDelimited(BufferStream stream, ServersideMountedWeaponSnapshot instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ServersideMountedWeaponSnapshot");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ServersideMountedWeaponSnapshot");
				instance.time = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ServersideMountedWeaponSnapshot");
				instance.pitch = ProtocolParser.ReadSingle(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ServersideMountedWeaponSnapshot");
				instance.yaw = ProtocolParser.ReadSingle(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ServersideMountedWeaponSnapshot");
				instance.force = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ServersideMountedWeaponSnapshot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ServersideMountedWeaponSnapshot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ServersideMountedWeaponSnapshot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ServersideMountedWeaponSnapshot");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ServersideMountedWeaponSnapshot");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ServersideMountedWeaponSnapshot DeserializeLength(BufferStream stream, int length, ServersideMountedWeaponSnapshot instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ServersideMountedWeaponSnapshot");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ServersideMountedWeaponSnapshot");
				instance.time = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ServersideMountedWeaponSnapshot");
				instance.pitch = ProtocolParser.ReadSingle(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ServersideMountedWeaponSnapshot");
				instance.yaw = ProtocolParser.ReadSingle(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ServersideMountedWeaponSnapshot");
				instance.force = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ServersideMountedWeaponSnapshot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ServersideMountedWeaponSnapshot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ServersideMountedWeaponSnapshot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ServersideMountedWeaponSnapshot");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ServersideMountedWeaponSnapshot");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ServersideMountedWeaponSnapshot instance, ServersideMountedWeaponSnapshot previous)
	{
		if (instance.time != previous.time)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.time);
		}
		if (instance.pitch != previous.pitch)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.pitch);
		}
		if (instance.yaw != previous.yaw)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.yaw);
		}
		stream.WriteByte(32);
		ProtocolParser.WriteBool(stream, instance.force);
	}

	public static void Serialize(BufferStream stream, ServersideMountedWeaponSnapshot instance)
	{
		if (instance.time != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.time);
		}
		if (instance.pitch != 0f)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.pitch);
		}
		if (instance.yaw != 0f)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.yaw);
		}
		if (instance.force)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteBool(stream, instance.force);
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
