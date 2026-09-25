using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class CustomVitals : IDisposable, Pool.IPooled, IProto<CustomVitals>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<CustomVitalInfo> vitals;

	public static void ResetToPool(CustomVitals instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.vitals != null)
		{
			for (int i = 0; i < instance.vitals.Count; i++)
			{
				if (instance.vitals[i] != null)
				{
					instance.vitals[i].ResetToPool();
					instance.vitals[i] = null;
				}
			}
			List<CustomVitalInfo> obj = instance.vitals;
			Pool.Free(ref obj, freeElements: false);
			instance.vitals = obj;
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
			throw new Exception("Trying to dispose CustomVitals with ShouldPool set to false!");
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

	public void CopyTo(CustomVitals instance)
	{
		if (vitals != null)
		{
			instance.vitals = Pool.Get<List<CustomVitalInfo>>();
			for (int i = 0; i < vitals.Count; i++)
			{
				CustomVitalInfo item = vitals[i].Copy();
				instance.vitals.Add(item);
			}
		}
		else
		{
			instance.vitals = null;
		}
	}

	public CustomVitals Copy()
	{
		CustomVitals customVitals = Pool.Get<CustomVitals>();
		CopyTo(customVitals);
		return customVitals;
	}

	public static CustomVitals Deserialize(BufferStream stream)
	{
		CustomVitals customVitals = Pool.Get<CustomVitals>();
		Deserialize(stream, customVitals, isDelta: false);
		return customVitals;
	}

	public static CustomVitals DeserializeLengthDelimited(BufferStream stream)
	{
		CustomVitals customVitals = Pool.Get<CustomVitals>();
		DeserializeLengthDelimited(stream, customVitals, isDelta: false);
		return customVitals;
	}

	public static CustomVitals DeserializeLength(BufferStream stream, int length)
	{
		CustomVitals customVitals = Pool.Get<CustomVitals>();
		DeserializeLength(stream, length, customVitals, isDelta: false);
		return customVitals;
	}

	public static CustomVitals Deserialize(byte[] buffer)
	{
		CustomVitals customVitals = Pool.Get<CustomVitals>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, customVitals, isDelta: false);
		return customVitals;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, CustomVitals previous)
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

	public static CustomVitals Deserialize(BufferStream stream, CustomVitals instance, bool isDelta)
	{
		if (!isDelta && instance.vitals == null)
		{
			instance.vitals = Pool.Get<List<CustomVitalInfo>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.CustomVitals");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CustomVitals");
				stream.ConsumeRepeatedElement();
				instance.vitals.Add(CustomVitalInfo.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CustomVitals");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CustomVitals");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static CustomVitals DeserializeLengthDelimited(BufferStream stream, CustomVitals instance, bool isDelta)
	{
		if (!isDelta && instance.vitals == null)
		{
			instance.vitals = Pool.Get<List<CustomVitalInfo>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.CustomVitals");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CustomVitals");
				stream.ConsumeRepeatedElement();
				instance.vitals.Add(CustomVitalInfo.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CustomVitals");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CustomVitals");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static CustomVitals DeserializeLength(BufferStream stream, int length, CustomVitals instance, bool isDelta)
	{
		if (!isDelta && instance.vitals == null)
		{
			instance.vitals = Pool.Get<List<CustomVitalInfo>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.CustomVitals");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CustomVitals");
				stream.ConsumeRepeatedElement();
				instance.vitals.Add(CustomVitalInfo.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CustomVitals");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CustomVitals");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, CustomVitals instance, CustomVitals previous)
	{
		if (instance.vitals == null)
		{
			return;
		}
		for (int i = 0; i < instance.vitals.Count; i++)
		{
			CustomVitalInfo customVitalInfo = instance.vitals[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			CustomVitalInfo.SerializeDelta(stream, customVitalInfo, customVitalInfo);
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

	public static void Serialize(BufferStream stream, CustomVitals instance)
	{
		if (instance.vitals == null)
		{
			return;
		}
		for (int i = 0; i < instance.vitals.Count; i++)
		{
			CustomVitalInfo instance2 = instance.vitals[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			CustomVitalInfo.Serialize(stream, instance2);
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
		if (vitals != null)
		{
			for (int i = 0; i < vitals.Count; i++)
			{
				vitals[i]?.InspectUids(action);
			}
		}
	}
}
