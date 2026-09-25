using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class StorageAdaptor : IDisposable, Pool.IPooled, IProto<StorageAdaptor>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public SortSettings sortingSettings;

	public static void ResetToPool(StorageAdaptor instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.sortingSettings != null)
			{
				instance.sortingSettings.ResetToPool();
				instance.sortingSettings = null;
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
			throw new Exception("Trying to dispose StorageAdaptor with ShouldPool set to false!");
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

	public void CopyTo(StorageAdaptor instance)
	{
		if (sortingSettings != null)
		{
			if (instance.sortingSettings == null)
			{
				instance.sortingSettings = sortingSettings.Copy();
			}
			else
			{
				sortingSettings.CopyTo(instance.sortingSettings);
			}
		}
		else
		{
			instance.sortingSettings = null;
		}
	}

	public StorageAdaptor Copy()
	{
		StorageAdaptor storageAdaptor = Pool.Get<StorageAdaptor>();
		CopyTo(storageAdaptor);
		return storageAdaptor;
	}

	public static StorageAdaptor Deserialize(BufferStream stream)
	{
		StorageAdaptor storageAdaptor = Pool.Get<StorageAdaptor>();
		Deserialize(stream, storageAdaptor, isDelta: false);
		return storageAdaptor;
	}

	public static StorageAdaptor DeserializeLengthDelimited(BufferStream stream)
	{
		StorageAdaptor storageAdaptor = Pool.Get<StorageAdaptor>();
		DeserializeLengthDelimited(stream, storageAdaptor, isDelta: false);
		return storageAdaptor;
	}

	public static StorageAdaptor DeserializeLength(BufferStream stream, int length)
	{
		StorageAdaptor storageAdaptor = Pool.Get<StorageAdaptor>();
		DeserializeLength(stream, length, storageAdaptor, isDelta: false);
		return storageAdaptor;
	}

	public static StorageAdaptor Deserialize(byte[] buffer)
	{
		StorageAdaptor storageAdaptor = Pool.Get<StorageAdaptor>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, storageAdaptor, isDelta: false);
		return storageAdaptor;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, StorageAdaptor previous)
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

	public static StorageAdaptor Deserialize(BufferStream stream, StorageAdaptor instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.StorageAdaptor");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.StorageAdaptor");
				if (instance.sortingSettings == null)
				{
					instance.sortingSettings = SortSettings.DeserializeLengthDelimited(stream);
				}
				else
				{
					SortSettings.DeserializeLengthDelimited(stream, instance.sortingSettings, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.StorageAdaptor");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.StorageAdaptor");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static StorageAdaptor DeserializeLengthDelimited(BufferStream stream, StorageAdaptor instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.StorageAdaptor");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.StorageAdaptor");
				if (instance.sortingSettings == null)
				{
					instance.sortingSettings = SortSettings.DeserializeLengthDelimited(stream);
				}
				else
				{
					SortSettings.DeserializeLengthDelimited(stream, instance.sortingSettings, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.StorageAdaptor");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.StorageAdaptor");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static StorageAdaptor DeserializeLength(BufferStream stream, int length, StorageAdaptor instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.StorageAdaptor");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.StorageAdaptor");
				if (instance.sortingSettings == null)
				{
					instance.sortingSettings = SortSettings.DeserializeLengthDelimited(stream);
				}
				else
				{
					SortSettings.DeserializeLengthDelimited(stream, instance.sortingSettings, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.StorageAdaptor");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.StorageAdaptor");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, StorageAdaptor instance, StorageAdaptor previous)
	{
		if (instance.sortingSettings == null)
		{
			return;
		}
		stream.WriteByte(10);
		BufferStream.RangeHandle range = stream.GetRange(5);
		int position = stream.Position;
		SortSettings.SerializeDelta(stream, instance.sortingSettings, previous.sortingSettings);
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

	public static void Serialize(BufferStream stream, StorageAdaptor instance)
	{
		if (instance.sortingSettings == null)
		{
			return;
		}
		stream.WriteByte(10);
		BufferStream.RangeHandle range = stream.GetRange(5);
		int position = stream.Position;
		SortSettings.Serialize(stream, instance.sortingSettings);
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

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		sortingSettings?.InspectUids(action);
	}
}
