using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class PaintableSign : IDisposable, Pool.IPooled, IProto<PaintableSign>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<uint> crcs;

	[NonSerialized]
	public List<ulong> editHistory;

	public static void ResetToPool(PaintableSign instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.crcs != null)
			{
				List<uint> obj = instance.crcs;
				Pool.FreeUnmanaged(ref obj);
				instance.crcs = obj;
			}
			if (instance.editHistory != null)
			{
				List<ulong> obj2 = instance.editHistory;
				Pool.FreeUnmanaged(ref obj2);
				instance.editHistory = obj2;
			}
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
			throw new Exception("Trying to dispose PaintableSign with ShouldPool set to false!");
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

	public void CopyTo(PaintableSign instance)
	{
		if (crcs != null)
		{
			instance.crcs = Pool.Get<List<uint>>();
			for (int i = 0; i < crcs.Count; i++)
			{
				uint item = crcs[i];
				instance.crcs.Add(item);
			}
		}
		else
		{
			instance.crcs = null;
		}
		if (editHistory != null)
		{
			instance.editHistory = Pool.Get<List<ulong>>();
			for (int j = 0; j < editHistory.Count; j++)
			{
				ulong item2 = editHistory[j];
				instance.editHistory.Add(item2);
			}
		}
		else
		{
			instance.editHistory = null;
		}
	}

	public PaintableSign Copy()
	{
		PaintableSign paintableSign = Pool.Get<PaintableSign>();
		CopyTo(paintableSign);
		return paintableSign;
	}

	public static PaintableSign Deserialize(BufferStream stream)
	{
		PaintableSign paintableSign = Pool.Get<PaintableSign>();
		Deserialize(stream, paintableSign, isDelta: false);
		return paintableSign;
	}

	public static PaintableSign DeserializeLengthDelimited(BufferStream stream)
	{
		PaintableSign paintableSign = Pool.Get<PaintableSign>();
		DeserializeLengthDelimited(stream, paintableSign, isDelta: false);
		return paintableSign;
	}

	public static PaintableSign DeserializeLength(BufferStream stream, int length)
	{
		PaintableSign paintableSign = Pool.Get<PaintableSign>();
		DeserializeLength(stream, length, paintableSign, isDelta: false);
		return paintableSign;
	}

	public static PaintableSign Deserialize(byte[] buffer)
	{
		PaintableSign paintableSign = Pool.Get<PaintableSign>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, paintableSign, isDelta: false);
		return paintableSign;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, PaintableSign previous)
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

	public static PaintableSign Deserialize(BufferStream stream, PaintableSign instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.crcs == null)
			{
				instance.crcs = Pool.Get<List<uint>>();
			}
			if (instance.editHistory == null)
			{
				instance.editHistory = Pool.Get<List<ulong>>();
			}
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.PaintableSign");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.PaintableSign");
				stream.ConsumeRepeatedElement();
				instance.crcs.Add(ProtocolParser.ReadUInt32(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.PaintableSign");
				stream.ConsumeRepeatedElement();
				instance.editHistory.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.PaintableSign");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.PaintableSign");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PaintableSign");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PaintableSign DeserializeLengthDelimited(BufferStream stream, PaintableSign instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.crcs == null)
			{
				instance.crcs = Pool.Get<List<uint>>();
			}
			if (instance.editHistory == null)
			{
				instance.editHistory = Pool.Get<List<ulong>>();
			}
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
			stream.ConsumeFieldOperation("ProtoBuf.PaintableSign");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.PaintableSign");
				stream.ConsumeRepeatedElement();
				instance.crcs.Add(ProtocolParser.ReadUInt32(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.PaintableSign");
				stream.ConsumeRepeatedElement();
				instance.editHistory.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.PaintableSign");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.PaintableSign");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PaintableSign");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PaintableSign DeserializeLength(BufferStream stream, int length, PaintableSign instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.crcs == null)
			{
				instance.crcs = Pool.Get<List<uint>>();
			}
			if (instance.editHistory == null)
			{
				instance.editHistory = Pool.Get<List<ulong>>();
			}
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
			stream.ConsumeFieldOperation("ProtoBuf.PaintableSign");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.PaintableSign");
				stream.ConsumeRepeatedElement();
				instance.crcs.Add(ProtocolParser.ReadUInt32(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.PaintableSign");
				stream.ConsumeRepeatedElement();
				instance.editHistory.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.PaintableSign");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.PaintableSign");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PaintableSign");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, PaintableSign instance, PaintableSign previous)
	{
		if (instance.crcs != null)
		{
			for (int i = 0; i < instance.crcs.Count; i++)
			{
				uint val = instance.crcs[i];
				stream.WriteByte(8);
				ProtocolParser.WriteUInt32(stream, val);
			}
		}
		if (instance.editHistory != null)
		{
			for (int j = 0; j < instance.editHistory.Count; j++)
			{
				ulong val2 = instance.editHistory[j];
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, val2);
			}
		}
	}

	public static void Serialize(BufferStream stream, PaintableSign instance)
	{
		if (instance.crcs != null)
		{
			for (int i = 0; i < instance.crcs.Count; i++)
			{
				uint val = instance.crcs[i];
				stream.WriteByte(8);
				ProtocolParser.WriteUInt32(stream, val);
			}
		}
		if (instance.editHistory != null)
		{
			for (int j = 0; j < instance.editHistory.Count; j++)
			{
				ulong val2 = instance.editHistory[j];
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, val2);
			}
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
