using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class PhotoFrame : IDisposable, Pool.IPooled, IProto<PhotoFrame>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public NetworkableId photoEntityId;

	[NonSerialized]
	public uint overlayImageCrc;

	[NonSerialized]
	public List<ulong> editHistory;

	public static void ResetToPool(PhotoFrame instance)
	{
		if (instance.ShouldPool)
		{
			instance.photoEntityId = default(NetworkableId);
			instance.overlayImageCrc = 0u;
			if (instance.editHistory != null)
			{
				List<ulong> obj = instance.editHistory;
				Pool.FreeUnmanaged(ref obj);
				instance.editHistory = obj;
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
			throw new Exception("Trying to dispose PhotoFrame with ShouldPool set to false!");
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

	public void CopyTo(PhotoFrame instance)
	{
		instance.photoEntityId = photoEntityId;
		instance.overlayImageCrc = overlayImageCrc;
		if (editHistory != null)
		{
			instance.editHistory = Pool.Get<List<ulong>>();
			for (int i = 0; i < editHistory.Count; i++)
			{
				ulong item = editHistory[i];
				instance.editHistory.Add(item);
			}
		}
		else
		{
			instance.editHistory = null;
		}
	}

	public PhotoFrame Copy()
	{
		PhotoFrame photoFrame = Pool.Get<PhotoFrame>();
		CopyTo(photoFrame);
		return photoFrame;
	}

	public static PhotoFrame Deserialize(BufferStream stream)
	{
		PhotoFrame photoFrame = Pool.Get<PhotoFrame>();
		Deserialize(stream, photoFrame, isDelta: false);
		return photoFrame;
	}

	public static PhotoFrame DeserializeLengthDelimited(BufferStream stream)
	{
		PhotoFrame photoFrame = Pool.Get<PhotoFrame>();
		DeserializeLengthDelimited(stream, photoFrame, isDelta: false);
		return photoFrame;
	}

	public static PhotoFrame DeserializeLength(BufferStream stream, int length)
	{
		PhotoFrame photoFrame = Pool.Get<PhotoFrame>();
		DeserializeLength(stream, length, photoFrame, isDelta: false);
		return photoFrame;
	}

	public static PhotoFrame Deserialize(byte[] buffer)
	{
		PhotoFrame photoFrame = Pool.Get<PhotoFrame>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, photoFrame, isDelta: false);
		return photoFrame;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, PhotoFrame previous)
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

	public static PhotoFrame Deserialize(BufferStream stream, PhotoFrame instance, bool isDelta)
	{
		if (!isDelta && instance.editHistory == null)
		{
			instance.editHistory = Pool.Get<List<ulong>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.PhotoFrame");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PhotoFrame");
				instance.photoEntityId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PhotoFrame");
				instance.overlayImageCrc = ProtocolParser.ReadUInt32(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.PhotoFrame");
				stream.ConsumeRepeatedElement();
				instance.editHistory.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PhotoFrame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PhotoFrame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.PhotoFrame");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PhotoFrame");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PhotoFrame DeserializeLengthDelimited(BufferStream stream, PhotoFrame instance, bool isDelta)
	{
		if (!isDelta && instance.editHistory == null)
		{
			instance.editHistory = Pool.Get<List<ulong>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.PhotoFrame");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PhotoFrame");
				instance.photoEntityId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PhotoFrame");
				instance.overlayImageCrc = ProtocolParser.ReadUInt32(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.PhotoFrame");
				stream.ConsumeRepeatedElement();
				instance.editHistory.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PhotoFrame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PhotoFrame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.PhotoFrame");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PhotoFrame");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PhotoFrame DeserializeLength(BufferStream stream, int length, PhotoFrame instance, bool isDelta)
	{
		if (!isDelta && instance.editHistory == null)
		{
			instance.editHistory = Pool.Get<List<ulong>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.PhotoFrame");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PhotoFrame");
				instance.photoEntityId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PhotoFrame");
				instance.overlayImageCrc = ProtocolParser.ReadUInt32(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.PhotoFrame");
				stream.ConsumeRepeatedElement();
				instance.editHistory.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PhotoFrame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PhotoFrame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.PhotoFrame");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PhotoFrame");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, PhotoFrame instance, PhotoFrame previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, instance.photoEntityId.Value);
		if (instance.overlayImageCrc != previous.overlayImageCrc)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt32(stream, instance.overlayImageCrc);
		}
		if (instance.editHistory != null)
		{
			for (int i = 0; i < instance.editHistory.Count; i++)
			{
				ulong val = instance.editHistory[i];
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, val);
			}
		}
	}

	public static void Serialize(BufferStream stream, PhotoFrame instance)
	{
		if (instance.photoEntityId != default(NetworkableId))
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.photoEntityId.Value);
		}
		if (instance.overlayImageCrc != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt32(stream, instance.overlayImageCrc);
		}
		if (instance.editHistory != null)
		{
			for (int i = 0; i < instance.editHistory.Count; i++)
			{
				ulong val = instance.editHistory[i];
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, val);
			}
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref photoEntityId.Value);
	}
}
