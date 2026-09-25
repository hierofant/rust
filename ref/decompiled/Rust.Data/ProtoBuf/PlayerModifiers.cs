using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class PlayerModifiers : IDisposable, Pool.IPooled, IProto<PlayerModifiers>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<Modifier> modifiers;

	public static void ResetToPool(PlayerModifiers instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.modifiers != null)
		{
			for (int i = 0; i < instance.modifiers.Count; i++)
			{
				if (instance.modifiers[i] != null)
				{
					instance.modifiers[i].ResetToPool();
					instance.modifiers[i] = null;
				}
			}
			List<Modifier> obj = instance.modifiers;
			Pool.Free(ref obj, freeElements: false);
			instance.modifiers = obj;
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
			throw new Exception("Trying to dispose PlayerModifiers with ShouldPool set to false!");
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

	public void CopyTo(PlayerModifiers instance)
	{
		if (modifiers != null)
		{
			instance.modifiers = Pool.Get<List<Modifier>>();
			for (int i = 0; i < modifiers.Count; i++)
			{
				Modifier item = modifiers[i].Copy();
				instance.modifiers.Add(item);
			}
		}
		else
		{
			instance.modifiers = null;
		}
	}

	public PlayerModifiers Copy()
	{
		PlayerModifiers playerModifiers = Pool.Get<PlayerModifiers>();
		CopyTo(playerModifiers);
		return playerModifiers;
	}

	public static PlayerModifiers Deserialize(BufferStream stream)
	{
		PlayerModifiers playerModifiers = Pool.Get<PlayerModifiers>();
		Deserialize(stream, playerModifiers, isDelta: false);
		return playerModifiers;
	}

	public static PlayerModifiers DeserializeLengthDelimited(BufferStream stream)
	{
		PlayerModifiers playerModifiers = Pool.Get<PlayerModifiers>();
		DeserializeLengthDelimited(stream, playerModifiers, isDelta: false);
		return playerModifiers;
	}

	public static PlayerModifiers DeserializeLength(BufferStream stream, int length)
	{
		PlayerModifiers playerModifiers = Pool.Get<PlayerModifiers>();
		DeserializeLength(stream, length, playerModifiers, isDelta: false);
		return playerModifiers;
	}

	public static PlayerModifiers Deserialize(byte[] buffer)
	{
		PlayerModifiers playerModifiers = Pool.Get<PlayerModifiers>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, playerModifiers, isDelta: false);
		return playerModifiers;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, PlayerModifiers previous)
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

	public static PlayerModifiers Deserialize(BufferStream stream, PlayerModifiers instance, bool isDelta)
	{
		if (!isDelta && instance.modifiers == null)
		{
			instance.modifiers = Pool.Get<List<Modifier>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.PlayerModifiers");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.PlayerModifiers");
				stream.ConsumeRepeatedElement();
				instance.modifiers.Add(Modifier.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.PlayerModifiers");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerModifiers");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static PlayerModifiers DeserializeLengthDelimited(BufferStream stream, PlayerModifiers instance, bool isDelta)
	{
		if (!isDelta && instance.modifiers == null)
		{
			instance.modifiers = Pool.Get<List<Modifier>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.PlayerModifiers");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.PlayerModifiers");
				stream.ConsumeRepeatedElement();
				instance.modifiers.Add(Modifier.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.PlayerModifiers");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerModifiers");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static PlayerModifiers DeserializeLength(BufferStream stream, int length, PlayerModifiers instance, bool isDelta)
	{
		if (!isDelta && instance.modifiers == null)
		{
			instance.modifiers = Pool.Get<List<Modifier>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.PlayerModifiers");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.PlayerModifiers");
				stream.ConsumeRepeatedElement();
				instance.modifiers.Add(Modifier.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.PlayerModifiers");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerModifiers");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, PlayerModifiers instance, PlayerModifiers previous)
	{
		if (instance.modifiers == null)
		{
			return;
		}
		for (int i = 0; i < instance.modifiers.Count; i++)
		{
			Modifier modifier = instance.modifiers[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Modifier.SerializeDelta(stream, modifier, modifier);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field modifiers (ProtoBuf.Modifier)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public static void Serialize(BufferStream stream, PlayerModifiers instance)
	{
		if (instance.modifiers == null)
		{
			return;
		}
		for (int i = 0; i < instance.modifiers.Count; i++)
		{
			Modifier instance2 = instance.modifiers[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Modifier.Serialize(stream, instance2);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field modifiers (ProtoBuf.Modifier)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (modifiers != null)
		{
			for (int i = 0; i < modifiers.Count; i++)
			{
				modifiers[i]?.InspectUids(action);
			}
		}
	}
}
