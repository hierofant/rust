using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class MapNoteList : IDisposable, Pool.IPooled, IProto<MapNoteList>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<MapNote> notes;

	public static void ResetToPool(MapNoteList instance)
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
			List<MapNote> obj = instance.notes;
			Pool.Free(ref obj, freeElements: false);
			instance.notes = obj;
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
			throw new Exception("Trying to dispose MapNoteList with ShouldPool set to false!");
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

	public void CopyTo(MapNoteList instance)
	{
		if (notes != null)
		{
			instance.notes = Pool.Get<List<MapNote>>();
			for (int i = 0; i < notes.Count; i++)
			{
				MapNote item = notes[i].Copy();
				instance.notes.Add(item);
			}
		}
		else
		{
			instance.notes = null;
		}
	}

	public MapNoteList Copy()
	{
		MapNoteList mapNoteList = Pool.Get<MapNoteList>();
		CopyTo(mapNoteList);
		return mapNoteList;
	}

	public static MapNoteList Deserialize(BufferStream stream)
	{
		MapNoteList mapNoteList = Pool.Get<MapNoteList>();
		Deserialize(stream, mapNoteList, isDelta: false);
		return mapNoteList;
	}

	public static MapNoteList DeserializeLengthDelimited(BufferStream stream)
	{
		MapNoteList mapNoteList = Pool.Get<MapNoteList>();
		DeserializeLengthDelimited(stream, mapNoteList, isDelta: false);
		return mapNoteList;
	}

	public static MapNoteList DeserializeLength(BufferStream stream, int length)
	{
		MapNoteList mapNoteList = Pool.Get<MapNoteList>();
		DeserializeLength(stream, length, mapNoteList, isDelta: false);
		return mapNoteList;
	}

	public static MapNoteList Deserialize(byte[] buffer)
	{
		MapNoteList mapNoteList = Pool.Get<MapNoteList>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, mapNoteList, isDelta: false);
		return mapNoteList;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, MapNoteList previous)
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

	public static MapNoteList Deserialize(BufferStream stream, MapNoteList instance, bool isDelta)
	{
		if (!isDelta && instance.notes == null)
		{
			instance.notes = Pool.Get<List<MapNote>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.MapNoteList");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.MapNoteList");
				stream.ConsumeRepeatedElement();
				instance.notes.Add(MapNote.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.MapNoteList");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MapNoteList");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static MapNoteList DeserializeLengthDelimited(BufferStream stream, MapNoteList instance, bool isDelta)
	{
		if (!isDelta && instance.notes == null)
		{
			instance.notes = Pool.Get<List<MapNote>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.MapNoteList");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.MapNoteList");
				stream.ConsumeRepeatedElement();
				instance.notes.Add(MapNote.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.MapNoteList");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MapNoteList");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static MapNoteList DeserializeLength(BufferStream stream, int length, MapNoteList instance, bool isDelta)
	{
		if (!isDelta && instance.notes == null)
		{
			instance.notes = Pool.Get<List<MapNote>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.MapNoteList");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.MapNoteList");
				stream.ConsumeRepeatedElement();
				instance.notes.Add(MapNote.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.MapNoteList");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MapNoteList");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, MapNoteList instance, MapNoteList previous)
	{
		if (instance.notes == null)
		{
			return;
		}
		for (int i = 0; i < instance.notes.Count; i++)
		{
			MapNote mapNote = instance.notes[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			MapNote.SerializeDelta(stream, mapNote, mapNote);
			int val = stream.Position - position;
			Span<byte> span = range.GetSpan();
			int num = ProtocolParser.WriteUInt32((uint)val, span, 0);
			if (num < 5)
			{
				span[num - 1] |= 128;
				while (num < 4)
				{
					span[num++] = 128;
				}
				span[4] = 0;
			}
		}
	}

	public static void Serialize(BufferStream stream, MapNoteList instance)
	{
		if (instance.notes == null)
		{
			return;
		}
		for (int i = 0; i < instance.notes.Count; i++)
		{
			MapNote instance2 = instance.notes[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			MapNote.Serialize(stream, instance2);
			int val = stream.Position - position;
			Span<byte> span = range.GetSpan();
			int num = ProtocolParser.WriteUInt32((uint)val, span, 0);
			if (num < 5)
			{
				span[num - 1] |= 128;
				while (num < 4)
				{
					span[num++] = 128;
				}
				span[4] = 0;
			}
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
