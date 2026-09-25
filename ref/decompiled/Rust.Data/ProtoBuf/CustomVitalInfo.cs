using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class CustomVitalInfo : IDisposable, Pool.IPooled, IProto<CustomVitalInfo>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public Color backgroundColor;

	[NonSerialized]
	public Color leftTextColor;

	[NonSerialized]
	public Color rightTextColor;

	[NonSerialized]
	public Color iconColor;

	[NonSerialized]
	public string leftText;

	[NonSerialized]
	public string rightText;

	[NonSerialized]
	public string icon;

	[NonSerialized]
	public bool active;

	[NonSerialized]
	public int timeLeft;

	public static void ResetToPool(CustomVitalInfo instance)
	{
		if (instance.ShouldPool)
		{
			instance.backgroundColor = default(Color);
			instance.leftTextColor = default(Color);
			instance.rightTextColor = default(Color);
			instance.iconColor = default(Color);
			instance.leftText = string.Empty;
			instance.rightText = string.Empty;
			instance.icon = string.Empty;
			instance.active = false;
			instance.timeLeft = 0;
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
			throw new Exception("Trying to dispose CustomVitalInfo with ShouldPool set to false!");
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

	public void CopyTo(CustomVitalInfo instance)
	{
		instance.backgroundColor = backgroundColor;
		instance.leftTextColor = leftTextColor;
		instance.rightTextColor = rightTextColor;
		instance.iconColor = iconColor;
		instance.leftText = leftText;
		instance.rightText = rightText;
		instance.icon = icon;
		instance.active = active;
		instance.timeLeft = timeLeft;
	}

	public CustomVitalInfo Copy()
	{
		CustomVitalInfo customVitalInfo = Pool.Get<CustomVitalInfo>();
		CopyTo(customVitalInfo);
		return customVitalInfo;
	}

	public static CustomVitalInfo Deserialize(BufferStream stream)
	{
		CustomVitalInfo customVitalInfo = Pool.Get<CustomVitalInfo>();
		Deserialize(stream, customVitalInfo, isDelta: false);
		return customVitalInfo;
	}

	public static CustomVitalInfo DeserializeLengthDelimited(BufferStream stream)
	{
		CustomVitalInfo customVitalInfo = Pool.Get<CustomVitalInfo>();
		DeserializeLengthDelimited(stream, customVitalInfo, isDelta: false);
		return customVitalInfo;
	}

	public static CustomVitalInfo DeserializeLength(BufferStream stream, int length)
	{
		CustomVitalInfo customVitalInfo = Pool.Get<CustomVitalInfo>();
		DeserializeLength(stream, length, customVitalInfo, isDelta: false);
		return customVitalInfo;
	}

	public static CustomVitalInfo Deserialize(byte[] buffer)
	{
		CustomVitalInfo customVitalInfo = Pool.Get<CustomVitalInfo>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, customVitalInfo, isDelta: false);
		return customVitalInfo;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, CustomVitalInfo previous)
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

	public static CustomVitalInfo Deserialize(BufferStream stream, CustomVitalInfo instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.CustomVitalInfo");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ColorSerialized.DeserializeLengthDelimited(stream, ref instance.backgroundColor, isDelta);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ColorSerialized.DeserializeLengthDelimited(stream, ref instance.leftTextColor, isDelta);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ColorSerialized.DeserializeLengthDelimited(stream, ref instance.rightTextColor, isDelta);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ColorSerialized.DeserializeLengthDelimited(stream, ref instance.iconColor, isDelta);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				instance.leftText = ProtocolParser.ReadString(stream);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				instance.rightText = ProtocolParser.ReadString(stream);
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				instance.icon = ProtocolParser.ReadString(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				instance.active = ProtocolParser.ReadBool(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				instance.timeLeft = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CustomVitalInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static CustomVitalInfo DeserializeLengthDelimited(BufferStream stream, CustomVitalInfo instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.CustomVitalInfo");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ColorSerialized.DeserializeLengthDelimited(stream, ref instance.backgroundColor, isDelta);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ColorSerialized.DeserializeLengthDelimited(stream, ref instance.leftTextColor, isDelta);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ColorSerialized.DeserializeLengthDelimited(stream, ref instance.rightTextColor, isDelta);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ColorSerialized.DeserializeLengthDelimited(stream, ref instance.iconColor, isDelta);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				instance.leftText = ProtocolParser.ReadString(stream);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				instance.rightText = ProtocolParser.ReadString(stream);
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				instance.icon = ProtocolParser.ReadString(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				instance.active = ProtocolParser.ReadBool(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				instance.timeLeft = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CustomVitalInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static CustomVitalInfo DeserializeLength(BufferStream stream, int length, CustomVitalInfo instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.CustomVitalInfo");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ColorSerialized.DeserializeLengthDelimited(stream, ref instance.backgroundColor, isDelta);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ColorSerialized.DeserializeLengthDelimited(stream, ref instance.leftTextColor, isDelta);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ColorSerialized.DeserializeLengthDelimited(stream, ref instance.rightTextColor, isDelta);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ColorSerialized.DeserializeLengthDelimited(stream, ref instance.iconColor, isDelta);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				instance.leftText = ProtocolParser.ReadString(stream);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				instance.rightText = ProtocolParser.ReadString(stream);
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				instance.icon = ProtocolParser.ReadString(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				instance.active = ProtocolParser.ReadBool(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				instance.timeLeft = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.CustomVitalInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CustomVitalInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, CustomVitalInfo instance, CustomVitalInfo previous)
	{
		if (instance.backgroundColor != previous.backgroundColor)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			ColorSerialized.SerializeDelta(stream, instance.backgroundColor, previous.backgroundColor);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field backgroundColor (UnityEngine.Color)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.leftTextColor != previous.leftTextColor)
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			ColorSerialized.SerializeDelta(stream, instance.leftTextColor, previous.leftTextColor);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field leftTextColor (UnityEngine.Color)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.rightTextColor != previous.rightTextColor)
		{
			stream.WriteByte(26);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			ColorSerialized.SerializeDelta(stream, instance.rightTextColor, previous.rightTextColor);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field rightTextColor (UnityEngine.Color)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
		if (instance.iconColor != previous.iconColor)
		{
			stream.WriteByte(34);
			BufferStream.RangeHandle range4 = stream.GetRange(1);
			int position4 = stream.Position;
			ColorSerialized.SerializeDelta(stream, instance.iconColor, previous.iconColor);
			int num4 = stream.Position - position4;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field iconColor (UnityEngine.Color)");
			}
			Span<byte> span4 = range4.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span4, 0);
		}
		if (instance.leftText != null && instance.leftText != previous.leftText)
		{
			stream.WriteByte(42);
			ProtocolParser.WriteString(stream, instance.leftText);
		}
		if (instance.rightText != null && instance.rightText != previous.rightText)
		{
			stream.WriteByte(50);
			ProtocolParser.WriteString(stream, instance.rightText);
		}
		if (instance.icon != null && instance.icon != previous.icon)
		{
			stream.WriteByte(58);
			ProtocolParser.WriteString(stream, instance.icon);
		}
		stream.WriteByte(64);
		ProtocolParser.WriteBool(stream, instance.active);
		if (instance.timeLeft != previous.timeLeft)
		{
			stream.WriteByte(72);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.timeLeft);
		}
	}

	public static void Serialize(BufferStream stream, CustomVitalInfo instance)
	{
		if (instance.backgroundColor != default(Color))
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			ColorSerialized.Serialize(stream, instance.backgroundColor);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field backgroundColor (UnityEngine.Color)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.leftTextColor != default(Color))
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			ColorSerialized.Serialize(stream, instance.leftTextColor);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field leftTextColor (UnityEngine.Color)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.rightTextColor != default(Color))
		{
			stream.WriteByte(26);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			ColorSerialized.Serialize(stream, instance.rightTextColor);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field rightTextColor (UnityEngine.Color)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
		if (instance.iconColor != default(Color))
		{
			stream.WriteByte(34);
			BufferStream.RangeHandle range4 = stream.GetRange(1);
			int position4 = stream.Position;
			ColorSerialized.Serialize(stream, instance.iconColor);
			int num4 = stream.Position - position4;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field iconColor (UnityEngine.Color)");
			}
			Span<byte> span4 = range4.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span4, 0);
		}
		if (instance.leftText != null)
		{
			stream.WriteByte(42);
			ProtocolParser.WriteString(stream, instance.leftText);
		}
		if (instance.rightText != null)
		{
			stream.WriteByte(50);
			ProtocolParser.WriteString(stream, instance.rightText);
		}
		if (instance.icon != null)
		{
			stream.WriteByte(58);
			ProtocolParser.WriteString(stream, instance.icon);
		}
		if (instance.active)
		{
			stream.WriteByte(64);
			ProtocolParser.WriteBool(stream, instance.active);
		}
		if (instance.timeLeft != 0)
		{
			stream.WriteByte(72);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.timeLeft);
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
