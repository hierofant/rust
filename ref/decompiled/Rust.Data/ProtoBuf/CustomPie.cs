using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class CustomPie : IDisposable, Pool.IPooled, IProto<CustomPie>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public string closeCommand;

	[NonSerialized]
	public List<CustomPieMenu> menus;

	public static void ResetToPool(CustomPie instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.closeCommand = string.Empty;
		if (instance.menus != null)
		{
			for (int i = 0; i < instance.menus.Count; i++)
			{
				if (instance.menus[i] != null)
				{
					instance.menus[i].ResetToPool();
					instance.menus[i] = null;
				}
			}
			List<CustomPieMenu> obj = instance.menus;
			Pool.Free(ref obj, freeElements: false);
			instance.menus = obj;
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
			throw new Exception("Trying to dispose CustomPie with ShouldPool set to false!");
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

	public void CopyTo(CustomPie instance)
	{
		instance.closeCommand = closeCommand;
		if (menus != null)
		{
			instance.menus = Pool.Get<List<CustomPieMenu>>();
			for (int i = 0; i < menus.Count; i++)
			{
				CustomPieMenu item = menus[i].Copy();
				instance.menus.Add(item);
			}
		}
		else
		{
			instance.menus = null;
		}
	}

	public CustomPie Copy()
	{
		CustomPie customPie = Pool.Get<CustomPie>();
		CopyTo(customPie);
		return customPie;
	}

	public static CustomPie Deserialize(BufferStream stream)
	{
		CustomPie customPie = Pool.Get<CustomPie>();
		Deserialize(stream, customPie, isDelta: false);
		return customPie;
	}

	public static CustomPie DeserializeLengthDelimited(BufferStream stream)
	{
		CustomPie customPie = Pool.Get<CustomPie>();
		DeserializeLengthDelimited(stream, customPie, isDelta: false);
		return customPie;
	}

	public static CustomPie DeserializeLength(BufferStream stream, int length)
	{
		CustomPie customPie = Pool.Get<CustomPie>();
		DeserializeLength(stream, length, customPie, isDelta: false);
		return customPie;
	}

	public static CustomPie Deserialize(byte[] buffer)
	{
		CustomPie customPie = Pool.Get<CustomPie>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, customPie, isDelta: false);
		return customPie;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, CustomPie previous)
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

	public static CustomPie Deserialize(BufferStream stream, CustomPie instance, bool isDelta)
	{
		if (!isDelta && instance.menus == null)
		{
			instance.menus = Pool.Get<List<CustomPieMenu>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.CustomPie");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CustomPie");
				instance.closeCommand = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.CustomPie");
				stream.ConsumeRepeatedElement();
				instance.menus.Add(CustomPieMenu.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CustomPie");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.CustomPie");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CustomPie");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static CustomPie DeserializeLengthDelimited(BufferStream stream, CustomPie instance, bool isDelta)
	{
		if (!isDelta && instance.menus == null)
		{
			instance.menus = Pool.Get<List<CustomPieMenu>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.CustomPie");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CustomPie");
				instance.closeCommand = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.CustomPie");
				stream.ConsumeRepeatedElement();
				instance.menus.Add(CustomPieMenu.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CustomPie");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.CustomPie");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CustomPie");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static CustomPie DeserializeLength(BufferStream stream, int length, CustomPie instance, bool isDelta)
	{
		if (!isDelta && instance.menus == null)
		{
			instance.menus = Pool.Get<List<CustomPieMenu>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.CustomPie");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CustomPie");
				instance.closeCommand = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.CustomPie");
				stream.ConsumeRepeatedElement();
				instance.menus.Add(CustomPieMenu.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CustomPie");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.CustomPie");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CustomPie");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, CustomPie instance, CustomPie previous)
	{
		if (instance.closeCommand != null && instance.closeCommand != previous.closeCommand)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.closeCommand);
		}
		if (instance.menus == null)
		{
			return;
		}
		for (int i = 0; i < instance.menus.Count; i++)
		{
			CustomPieMenu customPieMenu = instance.menus[i];
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			CustomPieMenu.SerializeDelta(stream, customPieMenu, customPieMenu);
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

	public static void Serialize(BufferStream stream, CustomPie instance)
	{
		if (instance.closeCommand != null)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.closeCommand);
		}
		if (instance.menus == null)
		{
			return;
		}
		for (int i = 0; i < instance.menus.Count; i++)
		{
			CustomPieMenu instance2 = instance.menus[i];
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			CustomPieMenu.Serialize(stream, instance2);
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
		if (menus != null)
		{
			for (int i = 0; i < menus.Count; i++)
			{
				menus[i]?.InspectUids(action);
			}
		}
	}
}
