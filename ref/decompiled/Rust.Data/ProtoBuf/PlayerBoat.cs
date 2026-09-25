using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class PlayerBoat : IDisposable, Pool.IPooled, IProto<PlayerBoat>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public Vector3 size;

	[NonSerialized]
	public bool hasLock;

	[NonSerialized]
	public string lockCode;

	[NonSerialized]
	public List<ulong> whitelistUsers;

	[NonSerialized]
	public Vector3 lastEditLocalPos;

	[NonSerialized]
	public Vector3 lastEditLocalRot;

	[NonSerialized]
	public float timeSinceLastUsed;

	public static void ResetToPool(PlayerBoat instance)
	{
		if (instance.ShouldPool)
		{
			instance.size = default(Vector3);
			instance.hasLock = false;
			instance.lockCode = string.Empty;
			if (instance.whitelistUsers != null)
			{
				List<ulong> obj = instance.whitelistUsers;
				Pool.FreeUnmanaged(ref obj);
				instance.whitelistUsers = obj;
			}
			instance.lastEditLocalPos = default(Vector3);
			instance.lastEditLocalRot = default(Vector3);
			instance.timeSinceLastUsed = 0f;
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
			throw new Exception("Trying to dispose PlayerBoat with ShouldPool set to false!");
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

	public void CopyTo(PlayerBoat instance)
	{
		instance.size = size;
		instance.hasLock = hasLock;
		instance.lockCode = lockCode;
		if (whitelistUsers != null)
		{
			instance.whitelistUsers = Pool.Get<List<ulong>>();
			for (int i = 0; i < whitelistUsers.Count; i++)
			{
				ulong item = whitelistUsers[i];
				instance.whitelistUsers.Add(item);
			}
		}
		else
		{
			instance.whitelistUsers = null;
		}
		instance.lastEditLocalPos = lastEditLocalPos;
		instance.lastEditLocalRot = lastEditLocalRot;
		instance.timeSinceLastUsed = timeSinceLastUsed;
	}

	public PlayerBoat Copy()
	{
		PlayerBoat playerBoat = Pool.Get<PlayerBoat>();
		CopyTo(playerBoat);
		return playerBoat;
	}

	public static PlayerBoat Deserialize(BufferStream stream)
	{
		PlayerBoat playerBoat = Pool.Get<PlayerBoat>();
		Deserialize(stream, playerBoat, isDelta: false);
		return playerBoat;
	}

	public static PlayerBoat DeserializeLengthDelimited(BufferStream stream)
	{
		PlayerBoat playerBoat = Pool.Get<PlayerBoat>();
		DeserializeLengthDelimited(stream, playerBoat, isDelta: false);
		return playerBoat;
	}

	public static PlayerBoat DeserializeLength(BufferStream stream, int length)
	{
		PlayerBoat playerBoat = Pool.Get<PlayerBoat>();
		DeserializeLength(stream, length, playerBoat, isDelta: false);
		return playerBoat;
	}

	public static PlayerBoat Deserialize(byte[] buffer)
	{
		PlayerBoat playerBoat = Pool.Get<PlayerBoat>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, playerBoat, isDelta: false);
		return playerBoat;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, PlayerBoat previous)
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

	public static PlayerBoat Deserialize(BufferStream stream, PlayerBoat instance, bool isDelta)
	{
		if (!isDelta && instance.whitelistUsers == null)
		{
			instance.whitelistUsers = Pool.Get<List<ulong>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.PlayerBoat");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.size, isDelta);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				instance.hasLock = ProtocolParser.ReadBool(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				instance.lockCode = ProtocolParser.ReadString(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.PlayerBoat");
				stream.ConsumeRepeatedElement();
				instance.whitelistUsers.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.lastEditLocalPos, isDelta);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.lastEditLocalRot, isDelta);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				instance.timeSinceLastUsed = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.PlayerBoat");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerBoat");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PlayerBoat DeserializeLengthDelimited(BufferStream stream, PlayerBoat instance, bool isDelta)
	{
		if (!isDelta && instance.whitelistUsers == null)
		{
			instance.whitelistUsers = Pool.Get<List<ulong>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.PlayerBoat");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.size, isDelta);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				instance.hasLock = ProtocolParser.ReadBool(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				instance.lockCode = ProtocolParser.ReadString(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.PlayerBoat");
				stream.ConsumeRepeatedElement();
				instance.whitelistUsers.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.lastEditLocalPos, isDelta);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.lastEditLocalRot, isDelta);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				instance.timeSinceLastUsed = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.PlayerBoat");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerBoat");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PlayerBoat DeserializeLength(BufferStream stream, int length, PlayerBoat instance, bool isDelta)
	{
		if (!isDelta && instance.whitelistUsers == null)
		{
			instance.whitelistUsers = Pool.Get<List<ulong>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.PlayerBoat");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.size, isDelta);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				instance.hasLock = ProtocolParser.ReadBool(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				instance.lockCode = ProtocolParser.ReadString(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.PlayerBoat");
				stream.ConsumeRepeatedElement();
				instance.whitelistUsers.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.lastEditLocalPos, isDelta);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.lastEditLocalRot, isDelta);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				instance.timeSinceLastUsed = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.PlayerBoat");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PlayerBoat");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerBoat");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, PlayerBoat instance, PlayerBoat previous)
	{
		if (instance.size != previous.size)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.size, previous.size);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field size (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		stream.WriteByte(16);
		ProtocolParser.WriteBool(stream, instance.hasLock);
		if (instance.lockCode != null && instance.lockCode != previous.lockCode)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteString(stream, instance.lockCode);
		}
		if (instance.whitelistUsers != null)
		{
			for (int i = 0; i < instance.whitelistUsers.Count; i++)
			{
				ulong val = instance.whitelistUsers[i];
				stream.WriteByte(32);
				ProtocolParser.WriteUInt64(stream, val);
			}
		}
		if (instance.lastEditLocalPos != previous.lastEditLocalPos)
		{
			stream.WriteByte(42);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.lastEditLocalPos, previous.lastEditLocalPos);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field lastEditLocalPos (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.lastEditLocalRot != previous.lastEditLocalRot)
		{
			stream.WriteByte(50);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.lastEditLocalRot, previous.lastEditLocalRot);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field lastEditLocalRot (UnityEngine.Vector3)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
		if (instance.timeSinceLastUsed != previous.timeSinceLastUsed)
		{
			stream.WriteByte(61);
			ProtocolParser.WriteSingle(stream, instance.timeSinceLastUsed);
		}
	}

	public static void Serialize(BufferStream stream, PlayerBoat instance)
	{
		if (instance.size != default(Vector3))
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.Serialize(stream, instance.size);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field size (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.hasLock)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteBool(stream, instance.hasLock);
		}
		if (instance.lockCode != null)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteString(stream, instance.lockCode);
		}
		if (instance.whitelistUsers != null)
		{
			for (int i = 0; i < instance.whitelistUsers.Count; i++)
			{
				ulong val = instance.whitelistUsers[i];
				stream.WriteByte(32);
				ProtocolParser.WriteUInt64(stream, val);
			}
		}
		if (instance.lastEditLocalPos != default(Vector3))
		{
			stream.WriteByte(42);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.lastEditLocalPos);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field lastEditLocalPos (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.lastEditLocalRot != default(Vector3))
		{
			stream.WriteByte(50);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.lastEditLocalRot);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field lastEditLocalRot (UnityEngine.Vector3)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
		if (instance.timeSinceLastUsed != 0f)
		{
			stream.WriteByte(61);
			ProtocolParser.WriteSingle(stream, instance.timeSinceLastUsed);
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
