using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class PhoneDirectory : IDisposable, Pool.IPooled, IProto<PhoneDirectory>, IProto
{
	public class DirectoryEntry : IDisposable, Pool.IPooled, IProto<DirectoryEntry>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public int phoneNumber;

		[NonSerialized]
		public string phoneName;

		public static void ResetToPool(DirectoryEntry instance)
		{
			if (instance.ShouldPool)
			{
				instance.phoneNumber = 0;
				instance.phoneName = string.Empty;
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
				throw new Exception("Trying to dispose DirectoryEntry with ShouldPool set to false!");
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

		public void CopyTo(DirectoryEntry instance)
		{
			instance.phoneNumber = phoneNumber;
			instance.phoneName = phoneName;
		}

		public DirectoryEntry Copy()
		{
			DirectoryEntry directoryEntry = Pool.Get<DirectoryEntry>();
			CopyTo(directoryEntry);
			return directoryEntry;
		}

		public static DirectoryEntry Deserialize(BufferStream stream)
		{
			DirectoryEntry directoryEntry = Pool.Get<DirectoryEntry>();
			Deserialize(stream, directoryEntry, isDelta: false);
			return directoryEntry;
		}

		public static DirectoryEntry DeserializeLengthDelimited(BufferStream stream)
		{
			DirectoryEntry directoryEntry = Pool.Get<DirectoryEntry>();
			DeserializeLengthDelimited(stream, directoryEntry, isDelta: false);
			return directoryEntry;
		}

		public static DirectoryEntry DeserializeLength(BufferStream stream, int length)
		{
			DirectoryEntry directoryEntry = Pool.Get<DirectoryEntry>();
			DeserializeLength(stream, length, directoryEntry, isDelta: false);
			return directoryEntry;
		}

		public static DirectoryEntry Deserialize(byte[] buffer)
		{
			DirectoryEntry directoryEntry = Pool.Get<DirectoryEntry>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, directoryEntry, isDelta: false);
			return directoryEntry;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, DirectoryEntry previous)
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

		public static DirectoryEntry Deserialize(BufferStream stream, DirectoryEntry instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.PhoneDirectory.DirectoryEntry");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PhoneDirectory.DirectoryEntry");
					instance.phoneNumber = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PhoneDirectory.DirectoryEntry");
					instance.phoneName = ProtocolParser.ReadString(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PhoneDirectory.DirectoryEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PhoneDirectory.DirectoryEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PhoneDirectory.DirectoryEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static DirectoryEntry DeserializeLengthDelimited(BufferStream stream, DirectoryEntry instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.PhoneDirectory.DirectoryEntry");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PhoneDirectory.DirectoryEntry");
					instance.phoneNumber = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PhoneDirectory.DirectoryEntry");
					instance.phoneName = ProtocolParser.ReadString(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PhoneDirectory.DirectoryEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PhoneDirectory.DirectoryEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PhoneDirectory.DirectoryEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static DirectoryEntry DeserializeLength(BufferStream stream, int length, DirectoryEntry instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.PhoneDirectory.DirectoryEntry");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PhoneDirectory.DirectoryEntry");
					instance.phoneNumber = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PhoneDirectory.DirectoryEntry");
					instance.phoneName = ProtocolParser.ReadString(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PhoneDirectory.DirectoryEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PhoneDirectory.DirectoryEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PhoneDirectory.DirectoryEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, DirectoryEntry instance, DirectoryEntry previous)
		{
			if (instance.phoneNumber != previous.phoneNumber)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.phoneNumber);
			}
			if (instance.phoneName != previous.phoneName)
			{
				if (instance.phoneName == null)
				{
					throw new ArgumentNullException("phoneName", "Required by proto specification.");
				}
				stream.WriteByte(18);
				ProtocolParser.WriteString(stream, instance.phoneName);
			}
		}

		public static void Serialize(BufferStream stream, DirectoryEntry instance)
		{
			if (instance.phoneNumber != 0)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.phoneNumber);
			}
			if (instance.phoneName == null)
			{
				throw new ArgumentNullException("phoneName", "Required by proto specification.");
			}
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.phoneName);
		}

		public void ToProto(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public void InspectUids(UidInspector<ulong> action)
		{
		}
	}

	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<DirectoryEntry> entries;

	[NonSerialized]
	public bool atEnd;

	public static void ResetToPool(PhoneDirectory instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.entries != null)
		{
			for (int i = 0; i < instance.entries.Count; i++)
			{
				if (instance.entries[i] != null)
				{
					instance.entries[i].ResetToPool();
					instance.entries[i] = null;
				}
			}
			List<DirectoryEntry> obj = instance.entries;
			Pool.Free(ref obj, freeElements: false);
			instance.entries = obj;
		}
		instance.atEnd = false;
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
			throw new Exception("Trying to dispose PhoneDirectory with ShouldPool set to false!");
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

	public void CopyTo(PhoneDirectory instance)
	{
		if (entries != null)
		{
			instance.entries = Pool.Get<List<DirectoryEntry>>();
			for (int i = 0; i < entries.Count; i++)
			{
				DirectoryEntry item = entries[i].Copy();
				instance.entries.Add(item);
			}
		}
		else
		{
			instance.entries = null;
		}
		instance.atEnd = atEnd;
	}

	public PhoneDirectory Copy()
	{
		PhoneDirectory phoneDirectory = Pool.Get<PhoneDirectory>();
		CopyTo(phoneDirectory);
		return phoneDirectory;
	}

	public static PhoneDirectory Deserialize(BufferStream stream)
	{
		PhoneDirectory phoneDirectory = Pool.Get<PhoneDirectory>();
		Deserialize(stream, phoneDirectory, isDelta: false);
		return phoneDirectory;
	}

	public static PhoneDirectory DeserializeLengthDelimited(BufferStream stream)
	{
		PhoneDirectory phoneDirectory = Pool.Get<PhoneDirectory>();
		DeserializeLengthDelimited(stream, phoneDirectory, isDelta: false);
		return phoneDirectory;
	}

	public static PhoneDirectory DeserializeLength(BufferStream stream, int length)
	{
		PhoneDirectory phoneDirectory = Pool.Get<PhoneDirectory>();
		DeserializeLength(stream, length, phoneDirectory, isDelta: false);
		return phoneDirectory;
	}

	public static PhoneDirectory Deserialize(byte[] buffer)
	{
		PhoneDirectory phoneDirectory = Pool.Get<PhoneDirectory>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, phoneDirectory, isDelta: false);
		return phoneDirectory;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, PhoneDirectory previous)
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

	public static PhoneDirectory Deserialize(BufferStream stream, PhoneDirectory instance, bool isDelta)
	{
		if (!isDelta && instance.entries == null)
		{
			instance.entries = Pool.Get<List<DirectoryEntry>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.PhoneDirectory");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.PhoneDirectory");
				stream.ConsumeRepeatedElement();
				instance.entries.Add(DirectoryEntry.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PhoneDirectory");
				instance.atEnd = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.PhoneDirectory");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PhoneDirectory");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PhoneDirectory");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PhoneDirectory DeserializeLengthDelimited(BufferStream stream, PhoneDirectory instance, bool isDelta)
	{
		if (!isDelta && instance.entries == null)
		{
			instance.entries = Pool.Get<List<DirectoryEntry>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.PhoneDirectory");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.PhoneDirectory");
				stream.ConsumeRepeatedElement();
				instance.entries.Add(DirectoryEntry.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PhoneDirectory");
				instance.atEnd = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.PhoneDirectory");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PhoneDirectory");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PhoneDirectory");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PhoneDirectory DeserializeLength(BufferStream stream, int length, PhoneDirectory instance, bool isDelta)
	{
		if (!isDelta && instance.entries == null)
		{
			instance.entries = Pool.Get<List<DirectoryEntry>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.PhoneDirectory");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.PhoneDirectory");
				stream.ConsumeRepeatedElement();
				instance.entries.Add(DirectoryEntry.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PhoneDirectory");
				instance.atEnd = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.PhoneDirectory");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PhoneDirectory");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PhoneDirectory");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, PhoneDirectory instance, PhoneDirectory previous)
	{
		if (instance.entries != null)
		{
			for (int i = 0; i < instance.entries.Count; i++)
			{
				DirectoryEntry directoryEntry = instance.entries[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				DirectoryEntry.SerializeDelta(stream, directoryEntry, directoryEntry);
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
		stream.WriteByte(16);
		ProtocolParser.WriteBool(stream, instance.atEnd);
	}

	public static void Serialize(BufferStream stream, PhoneDirectory instance)
	{
		if (instance.entries != null)
		{
			for (int i = 0; i < instance.entries.Count; i++)
			{
				DirectoryEntry instance2 = instance.entries[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				DirectoryEntry.Serialize(stream, instance2);
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
		if (instance.atEnd)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteBool(stream, instance.atEnd);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (entries != null)
		{
			for (int i = 0; i < entries.Count; i++)
			{
				entries[i]?.InspectUids(action);
			}
		}
	}
}
