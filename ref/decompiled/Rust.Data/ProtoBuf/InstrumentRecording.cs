using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class InstrumentRecording : IDisposable, Pool.IPooled, IProto<InstrumentRecording>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<InstrumentRecordingNote> notes;

	[NonSerialized]
	public int forInstrument;

	public static void ResetToPool(InstrumentRecording instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.notes != null)
		{
			for (int i = 0; i < instance.notes.Count; i++)
			{
				if (instance.notes[i] != null)
				{
					instance.notes[i].ResetToPool();
					instance.notes[i] = null;
				}
			}
			List<InstrumentRecordingNote> obj = instance.notes;
			Pool.Free(ref obj, freeElements: false);
			instance.notes = obj;
		}
		instance.forInstrument = 0;
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
			throw new Exception("Trying to dispose InstrumentRecording with ShouldPool set to false!");
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

	public void CopyTo(InstrumentRecording instance)
	{
		if (notes != null)
		{
			instance.notes = Pool.Get<List<InstrumentRecordingNote>>();
			for (int i = 0; i < notes.Count; i++)
			{
				InstrumentRecordingNote item = notes[i].Copy();
				instance.notes.Add(item);
			}
		}
		else
		{
			instance.notes = null;
		}
		instance.forInstrument = forInstrument;
	}

	public InstrumentRecording Copy()
	{
		InstrumentRecording instrumentRecording = Pool.Get<InstrumentRecording>();
		CopyTo(instrumentRecording);
		return instrumentRecording;
	}

	public static InstrumentRecording Deserialize(BufferStream stream)
	{
		InstrumentRecording instrumentRecording = Pool.Get<InstrumentRecording>();
		Deserialize(stream, instrumentRecording, isDelta: false);
		return instrumentRecording;
	}

	public static InstrumentRecording DeserializeLengthDelimited(BufferStream stream)
	{
		InstrumentRecording instrumentRecording = Pool.Get<InstrumentRecording>();
		DeserializeLengthDelimited(stream, instrumentRecording, isDelta: false);
		return instrumentRecording;
	}

	public static InstrumentRecording DeserializeLength(BufferStream stream, int length)
	{
		InstrumentRecording instrumentRecording = Pool.Get<InstrumentRecording>();
		DeserializeLength(stream, length, instrumentRecording, isDelta: false);
		return instrumentRecording;
	}

	public static InstrumentRecording Deserialize(byte[] buffer)
	{
		InstrumentRecording instrumentRecording = Pool.Get<InstrumentRecording>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, instrumentRecording, isDelta: false);
		return instrumentRecording;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, InstrumentRecording previous)
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

	public static InstrumentRecording Deserialize(BufferStream stream, InstrumentRecording instance, bool isDelta)
	{
		if (!isDelta && instance.notes == null)
		{
			instance.notes = Pool.Get<List<InstrumentRecordingNote>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.InstrumentRecording");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.InstrumentRecording");
				stream.ConsumeRepeatedElement();
				instance.notes.Add(InstrumentRecordingNote.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.InstrumentRecording");
				instance.forInstrument = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.InstrumentRecording");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.InstrumentRecording");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.InstrumentRecording");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static InstrumentRecording DeserializeLengthDelimited(BufferStream stream, InstrumentRecording instance, bool isDelta)
	{
		if (!isDelta && instance.notes == null)
		{
			instance.notes = Pool.Get<List<InstrumentRecordingNote>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.InstrumentRecording");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.InstrumentRecording");
				stream.ConsumeRepeatedElement();
				instance.notes.Add(InstrumentRecordingNote.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.InstrumentRecording");
				instance.forInstrument = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.InstrumentRecording");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.InstrumentRecording");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.InstrumentRecording");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static InstrumentRecording DeserializeLength(BufferStream stream, int length, InstrumentRecording instance, bool isDelta)
	{
		if (!isDelta && instance.notes == null)
		{
			instance.notes = Pool.Get<List<InstrumentRecordingNote>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.InstrumentRecording");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.InstrumentRecording");
				stream.ConsumeRepeatedElement();
				instance.notes.Add(InstrumentRecordingNote.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.InstrumentRecording");
				instance.forInstrument = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.InstrumentRecording");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.InstrumentRecording");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.InstrumentRecording");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, InstrumentRecording instance, InstrumentRecording previous)
	{
		if (instance.notes != null)
		{
			for (int i = 0; i < instance.notes.Count; i++)
			{
				InstrumentRecordingNote instrumentRecordingNote = instance.notes[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				InstrumentRecordingNote.SerializeDelta(stream, instrumentRecordingNote, instrumentRecordingNote);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field notes (ProtoBuf.InstrumentRecordingNote)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.forInstrument != previous.forInstrument)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.forInstrument);
		}
	}

	public static void Serialize(BufferStream stream, InstrumentRecording instance)
	{
		if (instance.notes != null)
		{
			for (int i = 0; i < instance.notes.Count; i++)
			{
				InstrumentRecordingNote instance2 = instance.notes[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				InstrumentRecordingNote.Serialize(stream, instance2);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field notes (ProtoBuf.InstrumentRecordingNote)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.forInstrument != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.forInstrument);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (notes != null)
		{
			for (int i = 0; i < notes.Count; i++)
			{
				notes[i]?.InspectUids(action);
			}
		}
	}
}
