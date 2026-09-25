using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class MissionInstance : IDisposable, Pool.IPooled, IProto<MissionInstance>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public uint missionID;

	[NonSerialized]
	public uint missionStatus;

	[NonSerialized]
	public MissionInstanceData instanceData;

	public static void ResetToPool(MissionInstance instance)
	{
		if (instance.ShouldPool)
		{
			instance.missionID = 0u;
			instance.missionStatus = 0u;
			if (instance.instanceData != null)
			{
				instance.instanceData.ResetToPool();
				instance.instanceData = null;
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
			throw new Exception("Trying to dispose MissionInstance with ShouldPool set to false!");
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

	public void CopyTo(MissionInstance instance)
	{
		instance.missionID = missionID;
		instance.missionStatus = missionStatus;
		if (instanceData != null)
		{
			if (instance.instanceData == null)
			{
				instance.instanceData = instanceData.Copy();
			}
			else
			{
				instanceData.CopyTo(instance.instanceData);
			}
		}
		else
		{
			instance.instanceData = null;
		}
	}

	public MissionInstance Copy()
	{
		MissionInstance missionInstance = Pool.Get<MissionInstance>();
		CopyTo(missionInstance);
		return missionInstance;
	}

	public static MissionInstance Deserialize(BufferStream stream)
	{
		MissionInstance missionInstance = Pool.Get<MissionInstance>();
		Deserialize(stream, missionInstance, isDelta: false);
		return missionInstance;
	}

	public static MissionInstance DeserializeLengthDelimited(BufferStream stream)
	{
		MissionInstance missionInstance = Pool.Get<MissionInstance>();
		DeserializeLengthDelimited(stream, missionInstance, isDelta: false);
		return missionInstance;
	}

	public static MissionInstance DeserializeLength(BufferStream stream, int length)
	{
		MissionInstance missionInstance = Pool.Get<MissionInstance>();
		DeserializeLength(stream, length, missionInstance, isDelta: false);
		return missionInstance;
	}

	public static MissionInstance Deserialize(byte[] buffer)
	{
		MissionInstance missionInstance = Pool.Get<MissionInstance>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, missionInstance, isDelta: false);
		return missionInstance;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, MissionInstance previous)
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

	public static MissionInstance Deserialize(BufferStream stream, MissionInstance instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.MissionInstance");
			switch (num)
			{
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MissionInstance");
				instance.missionID = ProtocolParser.ReadUInt32(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MissionInstance");
				instance.missionStatus = ProtocolParser.ReadUInt32(stream);
				continue;
			case 98:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.MissionInstance");
				if (instance.instanceData == null)
				{
					instance.instanceData = MissionInstanceData.DeserializeLengthDelimited(stream);
				}
				else
				{
					MissionInstanceData.DeserializeLengthDelimited(stream, instance.instanceData, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MissionInstance");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MissionInstance");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.MissionInstance");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MissionInstance");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static MissionInstance DeserializeLengthDelimited(BufferStream stream, MissionInstance instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.MissionInstance");
			switch (num2)
			{
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MissionInstance");
				instance.missionID = ProtocolParser.ReadUInt32(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MissionInstance");
				instance.missionStatus = ProtocolParser.ReadUInt32(stream);
				continue;
			case 98:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.MissionInstance");
				if (instance.instanceData == null)
				{
					instance.instanceData = MissionInstanceData.DeserializeLengthDelimited(stream);
				}
				else
				{
					MissionInstanceData.DeserializeLengthDelimited(stream, instance.instanceData, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MissionInstance");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MissionInstance");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.MissionInstance");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MissionInstance");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static MissionInstance DeserializeLength(BufferStream stream, int length, MissionInstance instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.MissionInstance");
			switch (num2)
			{
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MissionInstance");
				instance.missionID = ProtocolParser.ReadUInt32(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MissionInstance");
				instance.missionStatus = ProtocolParser.ReadUInt32(stream);
				continue;
			case 98:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.MissionInstance");
				if (instance.instanceData == null)
				{
					instance.instanceData = MissionInstanceData.DeserializeLengthDelimited(stream);
				}
				else
				{
					MissionInstanceData.DeserializeLengthDelimited(stream, instance.instanceData, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MissionInstance");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MissionInstance");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.MissionInstance");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MissionInstance");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, MissionInstance instance, MissionInstance previous)
	{
		if (instance.missionID != previous.missionID)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt32(stream, instance.missionID);
		}
		if (instance.missionStatus != previous.missionStatus)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt32(stream, instance.missionStatus);
		}
		if (instance.instanceData == null)
		{
			return;
		}
		stream.WriteByte(98);
		BufferStream.RangeHandle range = stream.GetRange(5);
		int position = stream.Position;
		MissionInstanceData.SerializeDelta(stream, instance.instanceData, previous.instanceData);
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

	public static void Serialize(BufferStream stream, MissionInstance instance)
	{
		if (instance.missionID != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt32(stream, instance.missionID);
		}
		if (instance.missionStatus != 0)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt32(stream, instance.missionStatus);
		}
		if (instance.instanceData == null)
		{
			return;
		}
		stream.WriteByte(98);
		BufferStream.RangeHandle range = stream.GetRange(5);
		int position = stream.Position;
		MissionInstanceData.Serialize(stream, instance.instanceData);
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

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		instanceData?.InspectUids(action);
	}
}
