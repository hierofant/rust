using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class DemoShotParentTrack : IDisposable, Pool.IPooled, IProto<DemoShotParentTrack>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public ulong parentId;

	[NonSerialized]
	public List<DemoShotParentKeyframe> keyframes;

	public static void ResetToPool(DemoShotParentTrack instance)
	{
		if (instance.ShouldPool)
		{
			instance.parentId = 0uL;
			if (instance.keyframes != null)
			{
				List<DemoShotParentKeyframe> obj = instance.keyframes;
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
			throw new Exception("Trying to dispose DemoShotParentTrack with ShouldPool set to false!");
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

	public void CopyTo(DemoShotParentTrack instance)
	{
		instance.parentId = parentId;
		if (keyframes != null)
		{
			instance.keyframes = Pool.Get<List<DemoShotParentKeyframe>>();
			for (int i = 0; i < keyframes.Count; i++)
			{
				DemoShotParentKeyframe item = keyframes[i];
				instance.keyframes.Add(item);
			}
		}
		else
		{
			instance.keyframes = null;
		}
	}

	public DemoShotParentTrack Copy()
	{
		DemoShotParentTrack demoShotParentTrack = Pool.Get<DemoShotParentTrack>();
		CopyTo(demoShotParentTrack);
		return demoShotParentTrack;
	}

	public static DemoShotParentTrack Deserialize(BufferStream stream)
	{
		DemoShotParentTrack demoShotParentTrack = Pool.Get<DemoShotParentTrack>();
		Deserialize(stream, demoShotParentTrack, isDelta: false);
		return demoShotParentTrack;
	}

	public static DemoShotParentTrack DeserializeLengthDelimited(BufferStream stream)
	{
		DemoShotParentTrack demoShotParentTrack = Pool.Get<DemoShotParentTrack>();
		DeserializeLengthDelimited(stream, demoShotParentTrack, isDelta: false);
		return demoShotParentTrack;
	}

	public static DemoShotParentTrack DeserializeLength(BufferStream stream, int length)
	{
		DemoShotParentTrack demoShotParentTrack = Pool.Get<DemoShotParentTrack>();
		DeserializeLength(stream, length, demoShotParentTrack, isDelta: false);
		return demoShotParentTrack;
	}

	public static DemoShotParentTrack Deserialize(byte[] buffer)
	{
		DemoShotParentTrack demoShotParentTrack = Pool.Get<DemoShotParentTrack>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, demoShotParentTrack, isDelta: false);
		return demoShotParentTrack;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, DemoShotParentTrack previous)
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

	public static DemoShotParentTrack Deserialize(BufferStream stream, DemoShotParentTrack instance, bool isDelta)
	{
		if (!isDelta && instance.keyframes == null)
		{
			instance.keyframes = Pool.Get<List<DemoShotParentKeyframe>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.DemoShotParentTrack");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotParentTrack");
				instance.parentId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.DemoShotParentTrack");
				stream.ConsumeRepeatedElement();
				DemoShotParentKeyframe instance2 = default(DemoShotParentKeyframe);
				DemoShotParentKeyframe.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.keyframes.Add(instance2);
				continue;
			}
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotParentTrack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.DemoShotParentTrack");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DemoShotParentTrack");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DemoShotParentTrack DeserializeLengthDelimited(BufferStream stream, DemoShotParentTrack instance, bool isDelta)
	{
		if (!isDelta && instance.keyframes == null)
		{
			instance.keyframes = Pool.Get<List<DemoShotParentKeyframe>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.DemoShotParentTrack");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotParentTrack");
				instance.parentId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.DemoShotParentTrack");
				stream.ConsumeRepeatedElement();
				DemoShotParentKeyframe instance2 = default(DemoShotParentKeyframe);
				DemoShotParentKeyframe.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.keyframes.Add(instance2);
				continue;
			}
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotParentTrack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.DemoShotParentTrack");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DemoShotParentTrack");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DemoShotParentTrack DeserializeLength(BufferStream stream, int length, DemoShotParentTrack instance, bool isDelta)
	{
		if (!isDelta && instance.keyframes == null)
		{
			instance.keyframes = Pool.Get<List<DemoShotParentKeyframe>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.DemoShotParentTrack");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotParentTrack");
				instance.parentId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.DemoShotParentTrack");
				stream.ConsumeRepeatedElement();
				DemoShotParentKeyframe instance2 = default(DemoShotParentKeyframe);
				DemoShotParentKeyframe.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.keyframes.Add(instance2);
				continue;
			}
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotParentTrack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.DemoShotParentTrack");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DemoShotParentTrack");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, DemoShotParentTrack instance, DemoShotParentTrack previous)
	{
		if (instance.parentId != previous.parentId)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.parentId);
		}
		if (instance.keyframes == null)
		{
			return;
		}
		for (int i = 0; i < instance.keyframes.Count; i++)
		{
			DemoShotParentKeyframe demoShotParentKeyframe = instance.keyframes[i];
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			DemoShotParentKeyframe.SerializeDelta(stream, demoShotParentKeyframe, demoShotParentKeyframe);
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

	public static void Serialize(BufferStream stream, DemoShotParentTrack instance)
	{
		if (instance.parentId != 0L)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.parentId);
		}
		if (instance.keyframes == null)
		{
			return;
		}
		for (int i = 0; i < instance.keyframes.Count; i++)
		{
			DemoShotParentKeyframe instance2 = instance.keyframes[i];
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			DemoShotParentKeyframe.Serialize(stream, instance2);
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
		if (keyframes != null)
		{
			for (int i = 0; i < keyframes.Count; i++)
			{
				keyframes[i].InspectUids(action);
			}
		}
	}
}
