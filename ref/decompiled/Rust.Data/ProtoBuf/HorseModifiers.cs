using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class HorseModifiers : IDisposable, Pool.IPooled, IProto<HorseModifiers>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<Modifier> modifiers;

	public static void ResetToPool(HorseModifiers instance)
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
			throw new Exception("Trying to dispose HorseModifiers with ShouldPool set to false!");
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

	public void CopyTo(HorseModifiers instance)
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

	public HorseModifiers Copy()
	{
		HorseModifiers horseModifiers = Pool.Get<HorseModifiers>();
		CopyTo(horseModifiers);
		return horseModifiers;
	}

	public static HorseModifiers Deserialize(BufferStream stream)
	{
		HorseModifiers horseModifiers = Pool.Get<HorseModifiers>();
		Deserialize(stream, horseModifiers, isDelta: false);
		return horseModifiers;
	}

	public static HorseModifiers DeserializeLengthDelimited(BufferStream stream)
	{
		HorseModifiers horseModifiers = Pool.Get<HorseModifiers>();
		DeserializeLengthDelimited(stream, horseModifiers, isDelta: false);
		return horseModifiers;
	}

	public static HorseModifiers DeserializeLength(BufferStream stream, int length)
	{
		HorseModifiers horseModifiers = Pool.Get<HorseModifiers>();
		DeserializeLength(stream, length, horseModifiers, isDelta: false);
		return horseModifiers;
	}

	public static HorseModifiers Deserialize(byte[] buffer)
	{
		HorseModifiers horseModifiers = Pool.Get<HorseModifiers>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, horseModifiers, isDelta: false);
		return horseModifiers;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, HorseModifiers previous)
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

	public static HorseModifiers Deserialize(BufferStream stream, HorseModifiers instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.HorseModifiers");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.HorseModifiers");
				stream.ConsumeRepeatedElement();
				instance.modifiers.Add(Modifier.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.HorseModifiers");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.HorseModifiers");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static HorseModifiers DeserializeLengthDelimited(BufferStream stream, HorseModifiers instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.HorseModifiers");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.HorseModifiers");
				stream.ConsumeRepeatedElement();
				instance.modifiers.Add(Modifier.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.HorseModifiers");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.HorseModifiers");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static HorseModifiers DeserializeLength(BufferStream stream, int length, HorseModifiers instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.HorseModifiers");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.HorseModifiers");
				stream.ConsumeRepeatedElement();
				instance.modifiers.Add(Modifier.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.HorseModifiers");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.HorseModifiers");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, HorseModifiers instance, HorseModifiers previous)
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

	public static void Serialize(BufferStream stream, HorseModifiers instance)
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
