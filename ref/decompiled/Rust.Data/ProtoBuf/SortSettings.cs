using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class SortSettings : IDisposable, Pool.IPooled, IProto<SortSettings>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public bool enabled;

	[NonSerialized]
	public int sortMode;

	[NonSerialized]
	public bool reverse;

	[NonSerialized]
	public bool stack;

	[NonSerialized]
	public List<string> customList;

	[NonSerialized]
	public string translateLanguage;

	public static void ResetToPool(SortSettings instance)
	{
		if (instance.ShouldPool)
		{
			instance.enabled = false;
			instance.sortMode = 0;
			instance.reverse = false;
			instance.stack = false;
			if (instance.customList != null)
			{
				List<string> obj = instance.customList;
				Pool.FreeUnmanaged(ref obj);
				instance.customList = obj;
			}
			instance.translateLanguage = string.Empty;
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
			throw new Exception("Trying to dispose SortSettings with ShouldPool set to false!");
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

	public void CopyTo(SortSettings instance)
	{
		instance.enabled = enabled;
		instance.sortMode = sortMode;
		instance.reverse = reverse;
		instance.stack = stack;
		if (customList != null)
		{
			instance.customList = Pool.Get<List<string>>();
			for (int i = 0; i < customList.Count; i++)
			{
				string item = customList[i];
				instance.customList.Add(item);
			}
		}
		else
		{
			instance.customList = null;
		}
		instance.translateLanguage = translateLanguage;
	}

	public SortSettings Copy()
	{
		SortSettings sortSettings = Pool.Get<SortSettings>();
		CopyTo(sortSettings);
		return sortSettings;
	}

	public static SortSettings Deserialize(BufferStream stream)
	{
		SortSettings sortSettings = Pool.Get<SortSettings>();
		Deserialize(stream, sortSettings, isDelta: false);
		return sortSettings;
	}

	public static SortSettings DeserializeLengthDelimited(BufferStream stream)
	{
		SortSettings sortSettings = Pool.Get<SortSettings>();
		DeserializeLengthDelimited(stream, sortSettings, isDelta: false);
		return sortSettings;
	}

	public static SortSettings DeserializeLength(BufferStream stream, int length)
	{
		SortSettings sortSettings = Pool.Get<SortSettings>();
		DeserializeLength(stream, length, sortSettings, isDelta: false);
		return sortSettings;
	}

	public static SortSettings Deserialize(byte[] buffer)
	{
		SortSettings sortSettings = Pool.Get<SortSettings>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, sortSettings, isDelta: false);
		return sortSettings;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, SortSettings previous)
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

	public static SortSettings Deserialize(BufferStream stream, SortSettings instance, bool isDelta)
	{
		if (!isDelta && instance.customList == null)
		{
			instance.customList = Pool.Get<List<string>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.SortSettings");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SortSettings");
				instance.enabled = ProtocolParser.ReadBool(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SortSettings");
				instance.sortMode = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SortSettings");
				instance.reverse = ProtocolParser.ReadBool(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SortSettings");
				instance.stack = ProtocolParser.ReadBool(stream);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.SortSettings");
				stream.ConsumeRepeatedElement();
				instance.customList.Add(ProtocolParser.ReadString(stream));
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.SortSettings");
				instance.translateLanguage = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SortSettings");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SortSettings");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SortSettings");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SortSettings");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.SortSettings");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.SortSettings");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SortSettings");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static SortSettings DeserializeLengthDelimited(BufferStream stream, SortSettings instance, bool isDelta)
	{
		if (!isDelta && instance.customList == null)
		{
			instance.customList = Pool.Get<List<string>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.SortSettings");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SortSettings");
				instance.enabled = ProtocolParser.ReadBool(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SortSettings");
				instance.sortMode = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SortSettings");
				instance.reverse = ProtocolParser.ReadBool(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SortSettings");
				instance.stack = ProtocolParser.ReadBool(stream);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.SortSettings");
				stream.ConsumeRepeatedElement();
				instance.customList.Add(ProtocolParser.ReadString(stream));
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.SortSettings");
				instance.translateLanguage = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SortSettings");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SortSettings");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SortSettings");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SortSettings");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.SortSettings");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.SortSettings");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SortSettings");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static SortSettings DeserializeLength(BufferStream stream, int length, SortSettings instance, bool isDelta)
	{
		if (!isDelta && instance.customList == null)
		{
			instance.customList = Pool.Get<List<string>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.SortSettings");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SortSettings");
				instance.enabled = ProtocolParser.ReadBool(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SortSettings");
				instance.sortMode = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SortSettings");
				instance.reverse = ProtocolParser.ReadBool(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SortSettings");
				instance.stack = ProtocolParser.ReadBool(stream);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.SortSettings");
				stream.ConsumeRepeatedElement();
				instance.customList.Add(ProtocolParser.ReadString(stream));
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.SortSettings");
				instance.translateLanguage = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SortSettings");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SortSettings");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SortSettings");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SortSettings");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.SortSettings");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.SortSettings");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SortSettings");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, SortSettings instance, SortSettings previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteBool(stream, instance.enabled);
		if (instance.sortMode != previous.sortMode)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.sortMode);
		}
		stream.WriteByte(24);
		ProtocolParser.WriteBool(stream, instance.reverse);
		stream.WriteByte(32);
		ProtocolParser.WriteBool(stream, instance.stack);
		if (instance.customList != null)
		{
			for (int i = 0; i < instance.customList.Count; i++)
			{
				string val = instance.customList[i];
				stream.WriteByte(42);
				ProtocolParser.WriteString(stream, val);
			}
		}
		if (instance.translateLanguage != null && instance.translateLanguage != previous.translateLanguage)
		{
			stream.WriteByte(58);
			ProtocolParser.WriteString(stream, instance.translateLanguage);
		}
	}

	public static void Serialize(BufferStream stream, SortSettings instance)
	{
		if (instance.enabled)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteBool(stream, instance.enabled);
		}
		if (instance.sortMode != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.sortMode);
		}
		if (instance.reverse)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteBool(stream, instance.reverse);
		}
		if (instance.stack)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteBool(stream, instance.stack);
		}
		if (instance.customList != null)
		{
			for (int i = 0; i < instance.customList.Count; i++)
			{
				string val = instance.customList[i];
				stream.WriteByte(42);
				ProtocolParser.WriteString(stream, val);
			}
		}
		if (instance.translateLanguage != null)
		{
			stream.WriteByte(58);
			ProtocolParser.WriteString(stream, instance.translateLanguage);
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
