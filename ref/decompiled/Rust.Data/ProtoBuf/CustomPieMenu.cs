using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class CustomPieMenu : IDisposable, Pool.IPooled, IProto<CustomPieMenu>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public string name;

	[NonSerialized]
	public string description;

	[NonSerialized]
	public string command;

	[NonSerialized]
	public string sprite;

	[NonSerialized]
	public uint imageId;

	[NonSerialized]
	public bool disabled;

	[NonSerialized]
	public bool selected;

	[NonSerialized]
	public int order;

	[NonSerialized]
	public int colorMode;

	[NonSerialized]
	public Color color;

	[NonSerialized]
	public string nextCommand;

	[NonSerialized]
	public string prevCommand;

	[NonSerialized]
	public string disabledCommand;

	public static void ResetToPool(CustomPieMenu instance)
	{
		if (instance.ShouldPool)
		{
			instance.name = string.Empty;
			instance.description = string.Empty;
			instance.command = string.Empty;
			instance.sprite = string.Empty;
			instance.imageId = 0u;
			instance.disabled = false;
			instance.selected = false;
			instance.order = 0;
			instance.colorMode = 0;
			instance.color = default(Color);
			instance.nextCommand = string.Empty;
			instance.prevCommand = string.Empty;
			instance.disabledCommand = string.Empty;
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
			throw new Exception("Trying to dispose CustomPieMenu with ShouldPool set to false!");
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

	public void CopyTo(CustomPieMenu instance)
	{
		instance.name = name;
		instance.description = description;
		instance.command = command;
		instance.sprite = sprite;
		instance.imageId = imageId;
		instance.disabled = disabled;
		instance.selected = selected;
		instance.order = order;
		instance.colorMode = colorMode;
		instance.color = color;
		instance.nextCommand = nextCommand;
		instance.prevCommand = prevCommand;
		instance.disabledCommand = disabledCommand;
	}

	public CustomPieMenu Copy()
	{
		CustomPieMenu customPieMenu = Pool.Get<CustomPieMenu>();
		CopyTo(customPieMenu);
		return customPieMenu;
	}

	public static CustomPieMenu Deserialize(BufferStream stream)
	{
		CustomPieMenu customPieMenu = Pool.Get<CustomPieMenu>();
		Deserialize(stream, customPieMenu, isDelta: false);
		return customPieMenu;
	}

	public static CustomPieMenu DeserializeLengthDelimited(BufferStream stream)
	{
		CustomPieMenu customPieMenu = Pool.Get<CustomPieMenu>();
		DeserializeLengthDelimited(stream, customPieMenu, isDelta: false);
		return customPieMenu;
	}

	public static CustomPieMenu DeserializeLength(BufferStream stream, int length)
	{
		CustomPieMenu customPieMenu = Pool.Get<CustomPieMenu>();
		DeserializeLength(stream, length, customPieMenu, isDelta: false);
		return customPieMenu;
	}

	public static CustomPieMenu Deserialize(byte[] buffer)
	{
		CustomPieMenu customPieMenu = Pool.Get<CustomPieMenu>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, customPieMenu, isDelta: false);
		return customPieMenu;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, CustomPieMenu previous)
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

	public static CustomPieMenu Deserialize(BufferStream stream, CustomPieMenu instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.CustomPieMenu");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.name = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.description = ProtocolParser.ReadString(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.command = ProtocolParser.ReadString(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.sprite = ProtocolParser.ReadString(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.imageId = ProtocolParser.ReadUInt32(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.disabled = ProtocolParser.ReadBool(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.selected = ProtocolParser.ReadBool(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.order = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.colorMode = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ColorSerialized.DeserializeLengthDelimited(stream, ref instance.color, isDelta);
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.nextCommand = ProtocolParser.ReadString(stream);
				continue;
			case 98:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.prevCommand = ProtocolParser.ReadString(stream);
				continue;
			case 106:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.disabledCommand = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static CustomPieMenu DeserializeLengthDelimited(BufferStream stream, CustomPieMenu instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.CustomPieMenu");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.name = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.description = ProtocolParser.ReadString(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.command = ProtocolParser.ReadString(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.sprite = ProtocolParser.ReadString(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.imageId = ProtocolParser.ReadUInt32(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.disabled = ProtocolParser.ReadBool(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.selected = ProtocolParser.ReadBool(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.order = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.colorMode = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ColorSerialized.DeserializeLengthDelimited(stream, ref instance.color, isDelta);
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.nextCommand = ProtocolParser.ReadString(stream);
				continue;
			case 98:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.prevCommand = ProtocolParser.ReadString(stream);
				continue;
			case 106:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.disabledCommand = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static CustomPieMenu DeserializeLength(BufferStream stream, int length, CustomPieMenu instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.CustomPieMenu");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.name = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.description = ProtocolParser.ReadString(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.command = ProtocolParser.ReadString(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.sprite = ProtocolParser.ReadString(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.imageId = ProtocolParser.ReadUInt32(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.disabled = ProtocolParser.ReadBool(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.selected = ProtocolParser.ReadBool(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.order = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.colorMode = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ColorSerialized.DeserializeLengthDelimited(stream, ref instance.color, isDelta);
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.nextCommand = ProtocolParser.ReadString(stream);
				continue;
			case 98:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.prevCommand = ProtocolParser.ReadString(stream);
				continue;
			case 106:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				instance.disabledCommand = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CustomPieMenu");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, CustomPieMenu instance, CustomPieMenu previous)
	{
		if (instance.name != null && instance.name != previous.name)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.name);
		}
		if (instance.description != null && instance.description != previous.description)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.description);
		}
		if (instance.command != null && instance.command != previous.command)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteString(stream, instance.command);
		}
		if (instance.sprite != null && instance.sprite != previous.sprite)
		{
			stream.WriteByte(34);
			ProtocolParser.WriteString(stream, instance.sprite);
		}
		if (instance.imageId != previous.imageId)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt32(stream, instance.imageId);
		}
		stream.WriteByte(48);
		ProtocolParser.WriteBool(stream, instance.disabled);
		stream.WriteByte(56);
		ProtocolParser.WriteBool(stream, instance.selected);
		if (instance.order != previous.order)
		{
			stream.WriteByte(64);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.order);
		}
		if (instance.colorMode != previous.colorMode)
		{
			stream.WriteByte(72);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.colorMode);
		}
		if (instance.color != previous.color)
		{
			stream.WriteByte(82);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			ColorSerialized.SerializeDelta(stream, instance.color, previous.color);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field color (UnityEngine.Color)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.nextCommand != null && instance.nextCommand != previous.nextCommand)
		{
			stream.WriteByte(90);
			ProtocolParser.WriteString(stream, instance.nextCommand);
		}
		if (instance.prevCommand != null && instance.prevCommand != previous.prevCommand)
		{
			stream.WriteByte(98);
			ProtocolParser.WriteString(stream, instance.prevCommand);
		}
		if (instance.disabledCommand != null && instance.disabledCommand != previous.disabledCommand)
		{
			stream.WriteByte(106);
			ProtocolParser.WriteString(stream, instance.disabledCommand);
		}
	}

	public static void Serialize(BufferStream stream, CustomPieMenu instance)
	{
		if (instance.name != null)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.name);
		}
		if (instance.description != null)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.description);
		}
		if (instance.command != null)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteString(stream, instance.command);
		}
		if (instance.sprite != null)
		{
			stream.WriteByte(34);
			ProtocolParser.WriteString(stream, instance.sprite);
		}
		if (instance.imageId != 0)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt32(stream, instance.imageId);
		}
		if (instance.disabled)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteBool(stream, instance.disabled);
		}
		if (instance.selected)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteBool(stream, instance.selected);
		}
		if (instance.order != 0)
		{
			stream.WriteByte(64);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.order);
		}
		if (instance.colorMode != 0)
		{
			stream.WriteByte(72);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.colorMode);
		}
		if (instance.color != default(Color))
		{
			stream.WriteByte(82);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			ColorSerialized.Serialize(stream, instance.color);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field color (UnityEngine.Color)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.nextCommand != null)
		{
			stream.WriteByte(90);
			ProtocolParser.WriteString(stream, instance.nextCommand);
		}
		if (instance.prevCommand != null)
		{
			stream.WriteByte(98);
			ProtocolParser.WriteString(stream, instance.prevCommand);
		}
		if (instance.disabledCommand != null)
		{
			stream.WriteByte(106);
			ProtocolParser.WriteString(stream, instance.disabledCommand);
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
