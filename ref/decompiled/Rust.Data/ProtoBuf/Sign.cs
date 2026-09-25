using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class Sign : IDisposable, Pool.IPooled, IProto<Sign>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public uint imageid;

	[NonSerialized]
	public List<uint> imageIds;

	[NonSerialized]
	public List<ulong> editHistory;

	public static void ResetToPool(Sign instance)
	{
		if (instance.ShouldPool)
		{
			instance.imageid = 0u;
			if (instance.imageIds != null)
			{
				List<uint> obj = instance.imageIds;
				Pool.FreeUnmanaged(ref obj);
				instance.imageIds = obj;
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
			throw new Exception("Trying to dispose Sign with ShouldPool set to false!");
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

	public void CopyTo(Sign instance)
	{
		instance.imageid = imageid;
		if (imageIds != null)
		{
			instance.imageIds = Pool.Get<List<uint>>();
			for (int i = 0; i < imageIds.Count; i++)
			{
				uint item = imageIds[i];
				instance.imageIds.Add(item);
			}
		}
		else
		{
			instance.imageIds = null;
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

	public Sign Copy()
	{
		Sign sign = Pool.Get<Sign>();
		CopyTo(sign);
		return sign;
	}

	public static Sign Deserialize(BufferStream stream)
	{
		Sign sign = Pool.Get<Sign>();
		Deserialize(stream, sign, isDelta: false);
		return sign;
	}

	public static Sign DeserializeLengthDelimited(BufferStream stream)
	{
		Sign sign = Pool.Get<Sign>();
		DeserializeLengthDelimited(stream, sign, isDelta: false);
		return sign;
	}

	public static Sign DeserializeLength(BufferStream stream, int length)
	{
		Sign sign = Pool.Get<Sign>();
		DeserializeLength(stream, length, sign, isDelta: false);
		return sign;
	}

	public static Sign Deserialize(byte[] buffer)
	{
		Sign sign = Pool.Get<Sign>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, sign, isDelta: false);
		return sign;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, Sign previous)
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

	public static Sign Deserialize(BufferStream stream, Sign instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.imageIds == null)
			{
				instance.imageIds = Pool.Get<List<uint>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Sign");
			switch (num)
			{
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Sign");
				instance.imageid = ProtocolParser.ReadUInt32(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.Sign");
				stream.ConsumeRepeatedElement();
				instance.imageIds.Add(ProtocolParser.ReadUInt32(stream));
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.Sign");
				stream.ConsumeRepeatedElement();
				instance.editHistory.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Sign");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.Sign");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.Sign");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Sign");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Sign DeserializeLengthDelimited(BufferStream stream, Sign instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.imageIds == null)
			{
				instance.imageIds = Pool.Get<List<uint>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Sign");
			switch (num2)
			{
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Sign");
				instance.imageid = ProtocolParser.ReadUInt32(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.Sign");
				stream.ConsumeRepeatedElement();
				instance.imageIds.Add(ProtocolParser.ReadUInt32(stream));
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.Sign");
				stream.ConsumeRepeatedElement();
				instance.editHistory.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Sign");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.Sign");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.Sign");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Sign");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Sign DeserializeLength(BufferStream stream, int length, Sign instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.imageIds == null)
			{
				instance.imageIds = Pool.Get<List<uint>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Sign");
			switch (num2)
			{
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Sign");
				instance.imageid = ProtocolParser.ReadUInt32(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.Sign");
				stream.ConsumeRepeatedElement();
				instance.imageIds.Add(ProtocolParser.ReadUInt32(stream));
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.Sign");
				stream.ConsumeRepeatedElement();
				instance.editHistory.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Sign");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.Sign");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.Sign");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Sign");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, Sign instance, Sign previous)
	{
		if (instance.imageid != previous.imageid)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt32(stream, instance.imageid);
		}
		if (instance.imageIds != null)
		{
			for (int i = 0; i < instance.imageIds.Count; i++)
			{
				uint val = instance.imageIds[i];
				stream.WriteByte(32);
				ProtocolParser.WriteUInt32(stream, val);
			}
		}
		if (instance.editHistory != null)
		{
			for (int j = 0; j < instance.editHistory.Count; j++)
			{
				ulong val2 = instance.editHistory[j];
				stream.WriteByte(40);
				ProtocolParser.WriteUInt64(stream, val2);
			}
		}
	}

	public static void Serialize(BufferStream stream, Sign instance)
	{
		if (instance.imageid != 0)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt32(stream, instance.imageid);
		}
		if (instance.imageIds != null)
		{
			for (int i = 0; i < instance.imageIds.Count; i++)
			{
				uint val = instance.imageIds[i];
				stream.WriteByte(32);
				ProtocolParser.WriteUInt32(stream, val);
			}
		}
		if (instance.editHistory != null)
		{
			for (int j = 0; j < instance.editHistory.Count; j++)
			{
				ulong val2 = instance.editHistory[j];
				stream.WriteByte(40);
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
