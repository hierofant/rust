using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class HotAirBalloon : IDisposable, Pool.IPooled, IProto<HotAirBalloon>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public float inflationAmount;

	[NonSerialized]
	public Vector3 velocity;

	[NonSerialized]
	public float sinceLastBlast;

	public static void ResetToPool(HotAirBalloon instance)
	{
		if (instance.ShouldPool)
		{
			instance.inflationAmount = 0f;
			instance.velocity = default(Vector3);
			instance.sinceLastBlast = 0f;
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
			throw new Exception("Trying to dispose HotAirBalloon with ShouldPool set to false!");
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

	public void CopyTo(HotAirBalloon instance)
	{
		instance.inflationAmount = inflationAmount;
		instance.velocity = velocity;
		instance.sinceLastBlast = sinceLastBlast;
	}

	public HotAirBalloon Copy()
	{
		HotAirBalloon hotAirBalloon = Pool.Get<HotAirBalloon>();
		CopyTo(hotAirBalloon);
		return hotAirBalloon;
	}

	public static HotAirBalloon Deserialize(BufferStream stream)
	{
		HotAirBalloon hotAirBalloon = Pool.Get<HotAirBalloon>();
		Deserialize(stream, hotAirBalloon, isDelta: false);
		return hotAirBalloon;
	}

	public static HotAirBalloon DeserializeLengthDelimited(BufferStream stream)
	{
		HotAirBalloon hotAirBalloon = Pool.Get<HotAirBalloon>();
		DeserializeLengthDelimited(stream, hotAirBalloon, isDelta: false);
		return hotAirBalloon;
	}

	public static HotAirBalloon DeserializeLength(BufferStream stream, int length)
	{
		HotAirBalloon hotAirBalloon = Pool.Get<HotAirBalloon>();
		DeserializeLength(stream, length, hotAirBalloon, isDelta: false);
		return hotAirBalloon;
	}

	public static HotAirBalloon Deserialize(byte[] buffer)
	{
		HotAirBalloon hotAirBalloon = Pool.Get<HotAirBalloon>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, hotAirBalloon, isDelta: false);
		return hotAirBalloon;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, HotAirBalloon previous)
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

	public static HotAirBalloon Deserialize(BufferStream stream, HotAirBalloon instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.HotAirBalloon");
			switch (num)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.HotAirBalloon");
				instance.inflationAmount = ProtocolParser.ReadSingle(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.HotAirBalloon");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.velocity, isDelta);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.HotAirBalloon");
				instance.sinceLastBlast = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.HotAirBalloon");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.HotAirBalloon");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.HotAirBalloon");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.HotAirBalloon");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static HotAirBalloon DeserializeLengthDelimited(BufferStream stream, HotAirBalloon instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.HotAirBalloon");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.HotAirBalloon");
				instance.inflationAmount = ProtocolParser.ReadSingle(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.HotAirBalloon");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.velocity, isDelta);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.HotAirBalloon");
				instance.sinceLastBlast = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.HotAirBalloon");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.HotAirBalloon");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.HotAirBalloon");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.HotAirBalloon");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static HotAirBalloon DeserializeLength(BufferStream stream, int length, HotAirBalloon instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.HotAirBalloon");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.HotAirBalloon");
				instance.inflationAmount = ProtocolParser.ReadSingle(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.HotAirBalloon");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.velocity, isDelta);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.HotAirBalloon");
				instance.sinceLastBlast = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.HotAirBalloon");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.HotAirBalloon");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.HotAirBalloon");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.HotAirBalloon");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, HotAirBalloon instance, HotAirBalloon previous)
	{
		if (instance.inflationAmount != previous.inflationAmount)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.inflationAmount);
		}
		if (instance.velocity != previous.velocity)
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.velocity, previous.velocity);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field velocity (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.sinceLastBlast != previous.sinceLastBlast)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.sinceLastBlast);
		}
	}

	public static void Serialize(BufferStream stream, HotAirBalloon instance)
	{
		if (instance.inflationAmount != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.inflationAmount);
		}
		if (instance.velocity != default(Vector3))
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.Serialize(stream, instance.velocity);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field velocity (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.sinceLastBlast != 0f)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.sinceLastBlast);
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
