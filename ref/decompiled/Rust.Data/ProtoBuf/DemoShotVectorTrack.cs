using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class DemoShotVectorTrack : IDisposable, Pool.IPooled, IProto<DemoShotVectorTrack>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public long trackType;

	[NonSerialized]
	public List<DemoShotVectorKeyframe> keyframes;

	public static void ResetToPool(DemoShotVectorTrack instance)
	{
		if (instance.ShouldPool)
		{
			instance.trackType = 0L;
			if (instance.keyframes != null)
			{
				List<DemoShotVectorKeyframe> obj = instance.keyframes;
				Pool.FreeUnmanaged(ref obj);
				instance.keyframes = obj;
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
			throw new Exception("Trying to dispose DemoShotVectorTrack with ShouldPool set to false!");
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

	public void CopyTo(DemoShotVectorTrack instance)
	{
		instance.trackType = trackType;
		if (keyframes != null)
		{
			instance.keyframes = Pool.Get<List<DemoShotVectorKeyframe>>();
			for (int i = 0; i < keyframes.Count; i++)
			{
				DemoShotVectorKeyframe item = keyframes[i];
				instance.keyframes.Add(item);
			}
		}
		else
		{
			instance.keyframes = null;
		}
	}

	public DemoShotVectorTrack Copy()
	{
		DemoShotVectorTrack demoShotVectorTrack = Pool.Get<DemoShotVectorTrack>();
		CopyTo(demoShotVectorTrack);
		return demoShotVectorTrack;
	}

	public static DemoShotVectorTrack Deserialize(BufferStream stream)
	{
		DemoShotVectorTrack demoShotVectorTrack = Pool.Get<DemoShotVectorTrack>();
		Deserialize(stream, demoShotVectorTrack, isDelta: false);
		return demoShotVectorTrack;
	}

	public static DemoShotVectorTrack DeserializeLengthDelimited(BufferStream stream)
	{
		DemoShotVectorTrack demoShotVectorTrack = Pool.Get<DemoShotVectorTrack>();
		DeserializeLengthDelimited(stream, demoShotVectorTrack, isDelta: false);
		return demoShotVectorTrack;
	}

	public static DemoShotVectorTrack DeserializeLength(BufferStream stream, int length)
	{
		DemoShotVectorTrack demoShotVectorTrack = Pool.Get<DemoShotVectorTrack>();
		DeserializeLength(stream, length, demoShotVectorTrack, isDelta: false);
		return demoShotVectorTrack;
	}

	public static DemoShotVectorTrack Deserialize(byte[] buffer)
	{
		DemoShotVectorTrack demoShotVectorTrack = Pool.Get<DemoShotVectorTrack>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, demoShotVectorTrack, isDelta: false);
		return demoShotVectorTrack;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, DemoShotVectorTrack previous)
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

	public static DemoShotVectorTrack Deserialize(BufferStream stream, DemoShotVectorTrack instance, bool isDelta)
	{
		if (!isDelta && instance.keyframes == null)
		{
			instance.keyframes = Pool.Get<List<DemoShotVectorKeyframe>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.DemoShotVectorTrack");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotVectorTrack");
				instance.trackType = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.DemoShotVectorTrack");
				stream.ConsumeRepeatedElement();
				DemoShotVectorKeyframe instance2 = default(DemoShotVectorKeyframe);
				DemoShotVectorKeyframe.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.keyframes.Add(instance2);
				continue;
			}
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotVectorTrack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.DemoShotVectorTrack");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DemoShotVectorTrack");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DemoShotVectorTrack DeserializeLengthDelimited(BufferStream stream, DemoShotVectorTrack instance, bool isDelta)
	{
		if (!isDelta && instance.keyframes == null)
		{
			instance.keyframes = Pool.Get<List<DemoShotVectorKeyframe>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.DemoShotVectorTrack");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotVectorTrack");
				instance.trackType = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.DemoShotVectorTrack");
				stream.ConsumeRepeatedElement();
				DemoShotVectorKeyframe instance2 = default(DemoShotVectorKeyframe);
				DemoShotVectorKeyframe.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.keyframes.Add(instance2);
				continue;
			}
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotVectorTrack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.DemoShotVectorTrack");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DemoShotVectorTrack");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DemoShotVectorTrack DeserializeLength(BufferStream stream, int length, DemoShotVectorTrack instance, bool isDelta)
	{
		if (!isDelta && instance.keyframes == null)
		{
			instance.keyframes = Pool.Get<List<DemoShotVectorKeyframe>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.DemoShotVectorTrack");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotVectorTrack");
				instance.trackType = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.DemoShotVectorTrack");
				stream.ConsumeRepeatedElement();
				DemoShotVectorKeyframe instance2 = default(DemoShotVectorKeyframe);
				DemoShotVectorKeyframe.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.keyframes.Add(instance2);
				continue;
			}
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotVectorTrack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.DemoShotVectorTrack");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DemoShotVectorTrack");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, DemoShotVectorTrack instance, DemoShotVectorTrack previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, (ulong)instance.trackType);
		if (instance.keyframes == null)
		{
			return;
		}
		for (int i = 0; i < instance.keyframes.Count; i++)
		{
			DemoShotVectorKeyframe demoShotVectorKeyframe = instance.keyframes[i];
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			DemoShotVectorKeyframe.SerializeDelta(stream, demoShotVectorKeyframe, demoShotVectorKeyframe);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field keyframes (ProtoBuf.DemoShotVectorKeyframe)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public static void Serialize(BufferStream stream, DemoShotVectorTrack instance)
	{
		if (instance.trackType != 0L)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.trackType);
		}
		if (instance.keyframes == null)
		{
			return;
		}
		for (int i = 0; i < instance.keyframes.Count; i++)
		{
			DemoShotVectorKeyframe instance2 = instance.keyframes[i];
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			DemoShotVectorKeyframe.Serialize(stream, instance2);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field keyframes (ProtoBuf.DemoShotVectorKeyframe)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (keyframes != null)
		{
			for (int i = 0; i < keyframes.Count; i++)
			{
				keyframes[i].InspectUids(action);
			}
		}
	}
}
