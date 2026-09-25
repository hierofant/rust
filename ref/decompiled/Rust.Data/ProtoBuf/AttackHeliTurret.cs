using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class AttackHeliTurret : IDisposable, Pool.IPooled, IProto<AttackHeliTurret>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public int clipAmmo;

	[NonSerialized]
	public int totalAmmo;

	[NonSerialized]
	public int gunState;

	[NonSerialized]
	public float xRot;

	[NonSerialized]
	public float yRot;

	[NonSerialized]
	public NetworkableId heldEntityID;

	public static void ResetToPool(AttackHeliTurret instance)
	{
		if (instance.ShouldPool)
		{
			instance.clipAmmo = 0;
			instance.totalAmmo = 0;
			instance.gunState = 0;
			instance.xRot = 0f;
			instance.yRot = 0f;
			instance.heldEntityID = default(NetworkableId);
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
			throw new Exception("Trying to dispose AttackHeliTurret with ShouldPool set to false!");
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

	public void CopyTo(AttackHeliTurret instance)
	{
		instance.clipAmmo = clipAmmo;
		instance.totalAmmo = totalAmmo;
		instance.gunState = gunState;
		instance.xRot = xRot;
		instance.yRot = yRot;
		instance.heldEntityID = heldEntityID;
	}

	public AttackHeliTurret Copy()
	{
		AttackHeliTurret attackHeliTurret = Pool.Get<AttackHeliTurret>();
		CopyTo(attackHeliTurret);
		return attackHeliTurret;
	}

	public static AttackHeliTurret Deserialize(BufferStream stream)
	{
		AttackHeliTurret attackHeliTurret = Pool.Get<AttackHeliTurret>();
		Deserialize(stream, attackHeliTurret, isDelta: false);
		return attackHeliTurret;
	}

	public static AttackHeliTurret DeserializeLengthDelimited(BufferStream stream)
	{
		AttackHeliTurret attackHeliTurret = Pool.Get<AttackHeliTurret>();
		DeserializeLengthDelimited(stream, attackHeliTurret, isDelta: false);
		return attackHeliTurret;
	}

	public static AttackHeliTurret DeserializeLength(BufferStream stream, int length)
	{
		AttackHeliTurret attackHeliTurret = Pool.Get<AttackHeliTurret>();
		DeserializeLength(stream, length, attackHeliTurret, isDelta: false);
		return attackHeliTurret;
	}

	public static AttackHeliTurret Deserialize(byte[] buffer)
	{
		AttackHeliTurret attackHeliTurret = Pool.Get<AttackHeliTurret>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, attackHeliTurret, isDelta: false);
		return attackHeliTurret;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, AttackHeliTurret previous)
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

	public static AttackHeliTurret Deserialize(BufferStream stream, AttackHeliTurret instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.AttackHeliTurret");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				instance.clipAmmo = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				instance.totalAmmo = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				instance.gunState = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				instance.xRot = ProtocolParser.ReadSingle(stream);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				instance.yRot = ProtocolParser.ReadSingle(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				instance.heldEntityID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AttackHeliTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AttackHeliTurret DeserializeLengthDelimited(BufferStream stream, AttackHeliTurret instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.AttackHeliTurret");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				instance.clipAmmo = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				instance.totalAmmo = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				instance.gunState = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				instance.xRot = ProtocolParser.ReadSingle(stream);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				instance.yRot = ProtocolParser.ReadSingle(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				instance.heldEntityID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AttackHeliTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AttackHeliTurret DeserializeLength(BufferStream stream, int length, AttackHeliTurret instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.AttackHeliTurret");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				instance.clipAmmo = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				instance.totalAmmo = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				instance.gunState = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				instance.xRot = ProtocolParser.ReadSingle(stream);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				instance.yRot = ProtocolParser.ReadSingle(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				instance.heldEntityID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AttackHeliTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AttackHeliTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, AttackHeliTurret instance, AttackHeliTurret previous)
	{
		if (instance.clipAmmo != previous.clipAmmo)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.clipAmmo);
		}
		if (instance.totalAmmo != previous.totalAmmo)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.totalAmmo);
		}
		if (instance.gunState != previous.gunState)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.gunState);
		}
		if (instance.xRot != previous.xRot)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.xRot);
		}
		if (instance.yRot != previous.yRot)
		{
			stream.WriteByte(45);
			ProtocolParser.WriteSingle(stream, instance.yRot);
		}
		stream.WriteByte(48);
		ProtocolParser.WriteUInt64(stream, instance.heldEntityID.Value);
	}

	public static void Serialize(BufferStream stream, AttackHeliTurret instance)
	{
		if (instance.clipAmmo != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.clipAmmo);
		}
		if (instance.totalAmmo != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.totalAmmo);
		}
		if (instance.gunState != 0)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.gunState);
		}
		if (instance.xRot != 0f)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.xRot);
		}
		if (instance.yRot != 0f)
		{
			stream.WriteByte(45);
			ProtocolParser.WriteSingle(stream, instance.yRot);
		}
		if (instance.heldEntityID != default(NetworkableId))
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, instance.heldEntityID.Value);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref heldEntityID.Value);
	}
}
