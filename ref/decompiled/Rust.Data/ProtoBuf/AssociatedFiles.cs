using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class AssociatedFiles : IDisposable, Pool.IPooled, IProto<AssociatedFiles>, IProto
{
	public class AssociatedFile : IDisposable, Pool.IPooled, IProto<AssociatedFile>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public int type;

		[NonSerialized]
		public uint crc;

		[NonSerialized]
		public uint numID;

		[NonSerialized]
		public byte[] data;

		public static void ResetToPool(AssociatedFile instance)
		{
			if (instance.ShouldPool)
			{
				instance.type = 0;
				instance.crc = 0u;
				instance.numID = 0u;
				instance.data = null;
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
				throw new Exception("Trying to dispose AssociatedFile with ShouldPool set to false!");
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

		public void CopyTo(AssociatedFile instance)
		{
			instance.type = type;
			instance.crc = crc;
			instance.numID = numID;
			if (data == null)
			{
				instance.data = null;
				return;
			}
			instance.data = new byte[data.Length];
			Array.Copy(data, instance.data, instance.data.Length);
		}

		public AssociatedFile Copy()
		{
			AssociatedFile associatedFile = Pool.Get<AssociatedFile>();
			CopyTo(associatedFile);
			return associatedFile;
		}

		public static AssociatedFile Deserialize(BufferStream stream)
		{
			AssociatedFile associatedFile = Pool.Get<AssociatedFile>();
			Deserialize(stream, associatedFile, isDelta: false);
			return associatedFile;
		}

		public static AssociatedFile DeserializeLengthDelimited(BufferStream stream)
		{
			AssociatedFile associatedFile = Pool.Get<AssociatedFile>();
			DeserializeLengthDelimited(stream, associatedFile, isDelta: false);
			return associatedFile;
		}

		public static AssociatedFile DeserializeLength(BufferStream stream, int length)
		{
			AssociatedFile associatedFile = Pool.Get<AssociatedFile>();
			DeserializeLength(stream, length, associatedFile, isDelta: false);
			return associatedFile;
		}

		public static AssociatedFile Deserialize(byte[] buffer)
		{
			AssociatedFile associatedFile = Pool.Get<AssociatedFile>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, associatedFile, isDelta: false);
			return associatedFile;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, AssociatedFile previous)
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

		public static AssociatedFile Deserialize(BufferStream stream, AssociatedFile instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.AssociatedFiles.AssociatedFile");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AssociatedFiles.AssociatedFile");
					instance.type = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AssociatedFiles.AssociatedFile");
					instance.crc = ProtocolParser.ReadUInt32(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AssociatedFiles.AssociatedFile");
					instance.numID = ProtocolParser.ReadUInt32(stream);
					continue;
				case 34:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AssociatedFiles.AssociatedFile");
					instance.data = ProtocolParser.ReadBytes(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AssociatedFiles.AssociatedFile");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AssociatedFiles.AssociatedFile");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AssociatedFiles.AssociatedFile");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AssociatedFiles.AssociatedFile");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AssociatedFiles.AssociatedFile");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static AssociatedFile DeserializeLengthDelimited(BufferStream stream, AssociatedFile instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.AssociatedFiles.AssociatedFile");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AssociatedFiles.AssociatedFile");
					instance.type = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AssociatedFiles.AssociatedFile");
					instance.crc = ProtocolParser.ReadUInt32(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AssociatedFiles.AssociatedFile");
					instance.numID = ProtocolParser.ReadUInt32(stream);
					continue;
				case 34:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AssociatedFiles.AssociatedFile");
					instance.data = ProtocolParser.ReadBytes(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AssociatedFiles.AssociatedFile");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AssociatedFiles.AssociatedFile");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AssociatedFiles.AssociatedFile");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AssociatedFiles.AssociatedFile");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AssociatedFiles.AssociatedFile");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static AssociatedFile DeserializeLength(BufferStream stream, int length, AssociatedFile instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.AssociatedFiles.AssociatedFile");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AssociatedFiles.AssociatedFile");
					instance.type = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AssociatedFiles.AssociatedFile");
					instance.crc = ProtocolParser.ReadUInt32(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AssociatedFiles.AssociatedFile");
					instance.numID = ProtocolParser.ReadUInt32(stream);
					continue;
				case 34:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AssociatedFiles.AssociatedFile");
					instance.data = ProtocolParser.ReadBytes(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AssociatedFiles.AssociatedFile");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AssociatedFiles.AssociatedFile");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AssociatedFiles.AssociatedFile");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AssociatedFiles.AssociatedFile");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AssociatedFiles.AssociatedFile");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, AssociatedFile instance, AssociatedFile previous)
		{
			if (instance.type != previous.type)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.type);
			}
			if (instance.crc != previous.crc)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt32(stream, instance.crc);
			}
			if (instance.numID != previous.numID)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt32(stream, instance.numID);
			}
			if (instance.data == null)
			{
				throw new ArgumentNullException("data", "Required by proto specification.");
			}
			stream.WriteByte(34);
			ProtocolParser.WriteBytes(stream, instance.data);
		}

		public static void Serialize(BufferStream stream, AssociatedFile instance)
		{
			if (instance.type != 0)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.type);
			}
			if (instance.crc != 0)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt32(stream, instance.crc);
			}
			if (instance.numID != 0)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt32(stream, instance.numID);
			}
			if (instance.data == null)
			{
				throw new ArgumentNullException("data", "Required by proto specification.");
			}
			stream.WriteByte(34);
			ProtocolParser.WriteBytes(stream, instance.data);
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
	public List<AssociatedFile> files;

	public static void ResetToPool(AssociatedFiles instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.files != null)
		{
			for (int i = 0; i < instance.files.Count; i++)
			{
				if (instance.files[i] != null)
				{
					instance.files[i].ResetToPool();
					instance.files[i] = null;
				}
			}
			List<AssociatedFile> obj = instance.files;
			Pool.Free(ref obj, freeElements: false);
			instance.files = obj;
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
			throw new Exception("Trying to dispose AssociatedFiles with ShouldPool set to false!");
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

	public void CopyTo(AssociatedFiles instance)
	{
		if (files != null)
		{
			instance.files = Pool.Get<List<AssociatedFile>>();
			for (int i = 0; i < files.Count; i++)
			{
				AssociatedFile item = files[i].Copy();
				instance.files.Add(item);
			}
		}
		else
		{
			instance.files = null;
		}
	}

	public AssociatedFiles Copy()
	{
		AssociatedFiles associatedFiles = Pool.Get<AssociatedFiles>();
		CopyTo(associatedFiles);
		return associatedFiles;
	}

	public static AssociatedFiles Deserialize(BufferStream stream)
	{
		AssociatedFiles associatedFiles = Pool.Get<AssociatedFiles>();
		Deserialize(stream, associatedFiles, isDelta: false);
		return associatedFiles;
	}

	public static AssociatedFiles DeserializeLengthDelimited(BufferStream stream)
	{
		AssociatedFiles associatedFiles = Pool.Get<AssociatedFiles>();
		DeserializeLengthDelimited(stream, associatedFiles, isDelta: false);
		return associatedFiles;
	}

	public static AssociatedFiles DeserializeLength(BufferStream stream, int length)
	{
		AssociatedFiles associatedFiles = Pool.Get<AssociatedFiles>();
		DeserializeLength(stream, length, associatedFiles, isDelta: false);
		return associatedFiles;
	}

	public static AssociatedFiles Deserialize(byte[] buffer)
	{
		AssociatedFiles associatedFiles = Pool.Get<AssociatedFiles>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, associatedFiles, isDelta: false);
		return associatedFiles;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, AssociatedFiles previous)
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

	public static AssociatedFiles Deserialize(BufferStream stream, AssociatedFiles instance, bool isDelta)
	{
		if (!isDelta && instance.files == null)
		{
			instance.files = Pool.Get<List<AssociatedFile>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.AssociatedFiles");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.AssociatedFiles");
				stream.ConsumeRepeatedElement();
				instance.files.Add(AssociatedFile.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.AssociatedFiles");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AssociatedFiles");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static AssociatedFiles DeserializeLengthDelimited(BufferStream stream, AssociatedFiles instance, bool isDelta)
	{
		if (!isDelta && instance.files == null)
		{
			instance.files = Pool.Get<List<AssociatedFile>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.AssociatedFiles");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.AssociatedFiles");
				stream.ConsumeRepeatedElement();
				instance.files.Add(AssociatedFile.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.AssociatedFiles");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AssociatedFiles");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static AssociatedFiles DeserializeLength(BufferStream stream, int length, AssociatedFiles instance, bool isDelta)
	{
		if (!isDelta && instance.files == null)
		{
			instance.files = Pool.Get<List<AssociatedFile>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.AssociatedFiles");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.AssociatedFiles");
				stream.ConsumeRepeatedElement();
				instance.files.Add(AssociatedFile.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.AssociatedFiles");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AssociatedFiles");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, AssociatedFiles instance, AssociatedFiles previous)
	{
		if (instance.files == null)
		{
			return;
		}
		for (int i = 0; i < instance.files.Count; i++)
		{
			AssociatedFile associatedFile = instance.files[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			AssociatedFile.SerializeDelta(stream, associatedFile, associatedFile);
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

	public static void Serialize(BufferStream stream, AssociatedFiles instance)
	{
		if (instance.files == null)
		{
			return;
		}
		for (int i = 0; i < instance.files.Count; i++)
		{
			AssociatedFile instance2 = instance.files[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			AssociatedFile.Serialize(stream, instance2);
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
		if (files != null)
		{
			for (int i = 0; i < files.Count; i++)
			{
				files[i]?.InspectUids(action);
			}
		}
	}
}
