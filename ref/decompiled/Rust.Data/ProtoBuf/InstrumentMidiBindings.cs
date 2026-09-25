using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class InstrumentMidiBindings : IDisposable, Pool.IPooled, IProto<InstrumentMidiBindings>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<InstrumentMidiBinding> bindings;

	[NonSerialized]
	public uint forInstrument;

	public static void ResetToPool(InstrumentMidiBindings instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.bindings != null)
		{
			for (int i = 0; i < instance.bindings.Count; i++)
			{
				if (instance.bindings[i] != null)
				{
					instance.bindings[i].ResetToPool();
					instance.bindings[i] = null;
				}
			}
			List<InstrumentMidiBinding> obj = instance.bindings;
			Pool.Free(ref obj, freeElements: false);
			instance.bindings = obj;
		}
		instance.forInstrument = 0u;
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
			throw new Exception("Trying to dispose InstrumentMidiBindings with ShouldPool set to false!");
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

	public void CopyTo(InstrumentMidiBindings instance)
	{
		if (bindings != null)
		{
			instance.bindings = Pool.Get<List<InstrumentMidiBinding>>();
			for (int i = 0; i < bindings.Count; i++)
			{
				InstrumentMidiBinding item = bindings[i].Copy();
				instance.bindings.Add(item);
			}
		}
		else
		{
			instance.bindings = null;
		}
		instance.forInstrument = forInstrument;
	}

	public InstrumentMidiBindings Copy()
	{
		InstrumentMidiBindings instrumentMidiBindings = Pool.Get<InstrumentMidiBindings>();
		CopyTo(instrumentMidiBindings);
		return instrumentMidiBindings;
	}

	public static InstrumentMidiBindings Deserialize(BufferStream stream)
	{
		InstrumentMidiBindings instrumentMidiBindings = Pool.Get<InstrumentMidiBindings>();
		Deserialize(stream, instrumentMidiBindings, isDelta: false);
		return instrumentMidiBindings;
	}

	public static InstrumentMidiBindings DeserializeLengthDelimited(BufferStream stream)
	{
		InstrumentMidiBindings instrumentMidiBindings = Pool.Get<InstrumentMidiBindings>();
		DeserializeLengthDelimited(stream, instrumentMidiBindings, isDelta: false);
		return instrumentMidiBindings;
	}

	public static InstrumentMidiBindings DeserializeLength(BufferStream stream, int length)
	{
		InstrumentMidiBindings instrumentMidiBindings = Pool.Get<InstrumentMidiBindings>();
		DeserializeLength(stream, length, instrumentMidiBindings, isDelta: false);
		return instrumentMidiBindings;
	}

	public static InstrumentMidiBindings Deserialize(byte[] buffer)
	{
		InstrumentMidiBindings instrumentMidiBindings = Pool.Get<InstrumentMidiBindings>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, instrumentMidiBindings, isDelta: false);
		return instrumentMidiBindings;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, InstrumentMidiBindings previous)
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

	public static InstrumentMidiBindings Deserialize(BufferStream stream, InstrumentMidiBindings instance, bool isDelta)
	{
		if (!isDelta && instance.bindings == null)
		{
			instance.bindings = Pool.Get<List<InstrumentMidiBinding>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.InstrumentMidiBindings");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.InstrumentMidiBindings");
				stream.ConsumeRepeatedElement();
				instance.bindings.Add(InstrumentMidiBinding.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.InstrumentMidiBindings");
				instance.forInstrument = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.InstrumentMidiBindings");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.InstrumentMidiBindings");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.InstrumentMidiBindings");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static InstrumentMidiBindings DeserializeLengthDelimited(BufferStream stream, InstrumentMidiBindings instance, bool isDelta)
	{
		if (!isDelta && instance.bindings == null)
		{
			instance.bindings = Pool.Get<List<InstrumentMidiBinding>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.InstrumentMidiBindings");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.InstrumentMidiBindings");
				stream.ConsumeRepeatedElement();
				instance.bindings.Add(InstrumentMidiBinding.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.InstrumentMidiBindings");
				instance.forInstrument = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.InstrumentMidiBindings");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.InstrumentMidiBindings");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.InstrumentMidiBindings");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static InstrumentMidiBindings DeserializeLength(BufferStream stream, int length, InstrumentMidiBindings instance, bool isDelta)
	{
		if (!isDelta && instance.bindings == null)
		{
			instance.bindings = Pool.Get<List<InstrumentMidiBinding>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.InstrumentMidiBindings");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.InstrumentMidiBindings");
				stream.ConsumeRepeatedElement();
				instance.bindings.Add(InstrumentMidiBinding.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.InstrumentMidiBindings");
				instance.forInstrument = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.InstrumentMidiBindings");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.InstrumentMidiBindings");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.InstrumentMidiBindings");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, InstrumentMidiBindings instance, InstrumentMidiBindings previous)
	{
		if (instance.bindings != null)
		{
			for (int i = 0; i < instance.bindings.Count; i++)
			{
				InstrumentMidiBinding instrumentMidiBinding = instance.bindings[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				InstrumentMidiBinding.SerializeDelta(stream, instrumentMidiBinding, instrumentMidiBinding);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field bindings (ProtoBuf.InstrumentMidiBinding)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.forInstrument != previous.forInstrument)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt32(stream, instance.forInstrument);
		}
	}

	public static void Serialize(BufferStream stream, InstrumentMidiBindings instance)
	{
		if (instance.bindings != null)
		{
			for (int i = 0; i < instance.bindings.Count; i++)
			{
				InstrumentMidiBinding instance2 = instance.bindings[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				InstrumentMidiBinding.Serialize(stream, instance2);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field bindings (ProtoBuf.InstrumentMidiBinding)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.forInstrument != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt32(stream, instance.forInstrument);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (bindings != null)
		{
			for (int i = 0; i < bindings.Count; i++)
			{
				bindings[i]?.InspectUids(action);
			}
		}
	}
}
