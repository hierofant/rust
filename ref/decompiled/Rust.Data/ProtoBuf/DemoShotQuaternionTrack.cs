using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class DemoShotQuaternionTrack : IDisposable, Pool.IPooled, IProto<DemoShotQuaternionTrack>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public long trackType;

	[NonSerialized]
	public List<DemoShotQuaternionKeyframe> keyframes;

	public static void ResetToPool(DemoShotQuaternionTrack instance)
	{
		if (instance.ShouldPool)
		{
			instance.trackType = 0L;
			if (instance.keyframes != null)
			{
				List<DemoShotQuaternionKeyframe> obj = instance.keyframes;
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
			throw new Exception("Trying to dispose DemoShotQuaternionTrack with ShouldPool set to false!");
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

	public void CopyTo(DemoShotQuaternionTrack instance)
	{
		instance.trackType = trackType;
		if (keyframes != null)
		{
			instance.keyframes = Pool.Get<List<DemoShotQuaternionKeyframe>>();
			for (int i = 0; i < keyframes.Count; i++)
			{
				DemoShotQuaternionKeyframe item = keyframes[i];
				instance.keyframes.Add(item);
			}
		}
		else
		{
			instance.keyframes = null;
		}
	}

	public DemoShotQuaternionTrack Copy()
	{
		DemoShotQuaternionTrack demoShotQuaternionTrack = Pool.Get<DemoShotQuaternionTrack>();
		CopyTo(demoShotQuaternionTrack);
		return demoShotQuaternionTrack;
	}

	public static DemoShotQuaternionTrack Deserialize(BufferStream stream)
	{
		DemoShotQuaternionTrack demoShotQuaternionTrack = Pool.Get<DemoShotQuaternionTrack>();
		Deserialize(stream, demoShotQuaternionTrack, isDelta: false);
		return demoShotQuaternionTrack;
	}

	public static DemoShotQuaternionTrack DeserializeLengthDelimited(BufferStream stream)
	{
		DemoShotQuaternionTrack demoShotQuaternionTrack = Pool.Get<DemoShotQuaternionTrack>();
		DeserializeLengthDelimited(stream, demoShotQuaternionTrack, isDelta: false);
		return demoShotQuaternionTrack;
	}

	public static DemoShotQuaternionTrack DeserializeLength(BufferStream stream, int length)
	{
		DemoShotQuaternionTrack demoShotQuaternionTrack = Pool.Get<DemoShotQuaternionTrack>();
		DeserializeLength(stream, length, demoShotQuaternionTrack, isDelta: false);
		return demoShotQuaternionTrack;
	}

	public static DemoShotQuaternionTrack Deserialize(byte[] buffer)
	{
		DemoShotQuaternionTrack demoShotQuaternionTrack = Pool.Get<DemoShotQuaternionTrack>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, demoShotQuaternionTrack, isDelta: false);
		return demoShotQuaternionTrack;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, DemoShotQuaternionTrack previous)
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

	public static DemoShotQuaternionTrack Deserialize(BufferStream stream, DemoShotQuaternionTrack instance, bool isDelta)
	{
		if (!isDelta && instance.keyframes == null)
		{
			instance.keyframes = Pool.Get<List<DemoShotQuaternionKeyframe>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.DemoShotQuaternionTrack");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionTrack");
				instance.trackType = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.DemoShotQuaternionTrack");
				stream.ConsumeRepeatedElement();
				DemoShotQuaternionKeyframe instance2 = default(DemoShotQuaternionKeyframe);
				DemoShotQuaternionKeyframe.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.keyframes.Add(instance2);
				continue;
			}
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionTrack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.DemoShotQuaternionTrack");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DemoShotQuaternionTrack");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DemoShotQuaternionTrack DeserializeLengthDelimited(BufferStream stream, DemoShotQuaternionTrack instance, bool isDelta)
	{
		if (!isDelta && instance.keyframes == null)
		{
			instance.keyframes = Pool.Get<List<DemoShotQuaternionKeyframe>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.DemoShotQuaternionTrack");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionTrack");
				instance.trackType = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.DemoShotQuaternionTrack");
				stream.ConsumeRepeatedElement();
				DemoShotQuaternionKeyframe instance2 = default(DemoShotQuaternionKeyframe);
				DemoShotQuaternionKeyframe.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.keyframes.Add(instance2);
				continue;
			}
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionTrack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.DemoShotQuaternionTrack");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DemoShotQuaternionTrack");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DemoShotQuaternionTrack DeserializeLength(BufferStream stream, int length, DemoShotQuaternionTrack instance, bool isDelta)
	{
		if (!isDelta && instance.keyframes == null)
		{
			instance.keyframes = Pool.Get<List<DemoShotQuaternionKeyframe>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.DemoShotQuaternionTrack");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionTrack");
				instance.trackType = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.DemoShotQuaternionTrack");
				stream.ConsumeRepeatedElement();
				DemoShotQuaternionKeyframe instance2 = default(DemoShotQuaternionKeyframe);
				DemoShotQuaternionKeyframe.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.keyframes.Add(instance2);
				continue;
			}
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionTrack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.DemoShotQuaternionTrack");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DemoShotQuaternionTrack");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, DemoShotQuaternionTrack instance, DemoShotQuaternionTrack previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, (ulong)instance.trackType);
		if (instance.keyframes == null)
		{
			return;
		}
		for (int i = 0; i < instance.keyframes.Count; i++)
		{
			DemoShotQuaternionKeyframe demoShotQuaternionKeyframe = instance.keyframes[i];
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			DemoShotQuaternionKeyframe.SerializeDelta(stream, demoShotQuaternionKeyframe, demoShotQuaternionKeyframe);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field keyframes (ProtoBuf.DemoShotQuaternionKeyframe)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public static void Serialize(BufferStream stream, DemoShotQuaternionTrack instance)
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
			DemoShotQuaternionKeyframe instance2 = instance.keyframes[i];
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			DemoShotQuaternionKeyframe.Serialize(stream, instance2);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field keyframes (ProtoBuf.DemoShotQuaternionKeyframe)");
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
