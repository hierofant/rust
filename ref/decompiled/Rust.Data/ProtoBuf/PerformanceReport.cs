using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class PerformanceReport : IDisposable, Pool.IPooled, IProto<PerformanceReport>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public int request_id;

	[NonSerialized]
	public string user_id;

	[NonSerialized]
	public float fps_average;

	[NonSerialized]
	public int fps;

	[NonSerialized]
	public int frame_id;

	[NonSerialized]
	public float frame_time;

	[NonSerialized]
	public float frame_time_average;

	[NonSerialized]
	public long memory_system;

	[NonSerialized]
	public long memory_collections;

	[NonSerialized]
	public long memory_managed_heap;

	[NonSerialized]
	public float realtime_since_startup;

	[NonSerialized]
	public bool streamer_mode;

	[NonSerialized]
	public int ping;

	[NonSerialized]
	public int tasks_invokes;

	[NonSerialized]
	public int tasks_load_balancer;

	[NonSerialized]
	public int workshop_skins_queued;

	public static void ResetToPool(PerformanceReport instance)
	{
		if (instance.ShouldPool)
		{
			instance.request_id = 0;
			instance.user_id = string.Empty;
			instance.fps_average = 0f;
			instance.fps = 0;
			instance.frame_id = 0;
			instance.frame_time = 0f;
			instance.frame_time_average = 0f;
			instance.memory_system = 0L;
			instance.memory_collections = 0L;
			instance.memory_managed_heap = 0L;
			instance.realtime_since_startup = 0f;
			instance.streamer_mode = false;
			instance.ping = 0;
			instance.tasks_invokes = 0;
			instance.tasks_load_balancer = 0;
			instance.workshop_skins_queued = 0;
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
			throw new Exception("Trying to dispose PerformanceReport with ShouldPool set to false!");
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

	public void CopyTo(PerformanceReport instance)
	{
		instance.request_id = request_id;
		instance.user_id = user_id;
		instance.fps_average = fps_average;
		instance.fps = fps;
		instance.frame_id = frame_id;
		instance.frame_time = frame_time;
		instance.frame_time_average = frame_time_average;
		instance.memory_system = memory_system;
		instance.memory_collections = memory_collections;
		instance.memory_managed_heap = memory_managed_heap;
		instance.realtime_since_startup = realtime_since_startup;
		instance.streamer_mode = streamer_mode;
		instance.ping = ping;
		instance.tasks_invokes = tasks_invokes;
		instance.tasks_load_balancer = tasks_load_balancer;
		instance.workshop_skins_queued = workshop_skins_queued;
	}

	public PerformanceReport Copy()
	{
		PerformanceReport performanceReport = Pool.Get<PerformanceReport>();
		CopyTo(performanceReport);
		return performanceReport;
	}

	public static PerformanceReport Deserialize(BufferStream stream)
	{
		PerformanceReport performanceReport = Pool.Get<PerformanceReport>();
		Deserialize(stream, performanceReport, isDelta: false);
		return performanceReport;
	}

	public static PerformanceReport DeserializeLengthDelimited(BufferStream stream)
	{
		PerformanceReport performanceReport = Pool.Get<PerformanceReport>();
		DeserializeLengthDelimited(stream, performanceReport, isDelta: false);
		return performanceReport;
	}

	public static PerformanceReport DeserializeLength(BufferStream stream, int length)
	{
		PerformanceReport performanceReport = Pool.Get<PerformanceReport>();
		DeserializeLength(stream, length, performanceReport, isDelta: false);
		return performanceReport;
	}

	public static PerformanceReport Deserialize(byte[] buffer)
	{
		PerformanceReport performanceReport = Pool.Get<PerformanceReport>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, performanceReport, isDelta: false);
		return performanceReport;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, PerformanceReport previous)
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

	public static PerformanceReport Deserialize(BufferStream stream, PerformanceReport instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.PerformanceReport");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.request_id = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.user_id = ProtocolParser.ReadString(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.fps_average = ProtocolParser.ReadSingle(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.fps = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.frame_id = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.frame_time = ProtocolParser.ReadSingle(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.frame_time_average = ProtocolParser.ReadSingle(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.memory_system = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.memory_collections = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.memory_managed_heap = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 93:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.realtime_since_startup = ProtocolParser.ReadSingle(stream);
				continue;
			case 96:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.streamer_mode = ProtocolParser.ReadBool(stream);
				continue;
			case 104:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.ping = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 112:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.tasks_invokes = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 120:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.tasks_load_balancer = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				if (key.WireType == Wire.Varint)
				{
					instance.workshop_skins_queued = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PerformanceReport DeserializeLengthDelimited(BufferStream stream, PerformanceReport instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.PerformanceReport");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.request_id = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.user_id = ProtocolParser.ReadString(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.fps_average = ProtocolParser.ReadSingle(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.fps = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.frame_id = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.frame_time = ProtocolParser.ReadSingle(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.frame_time_average = ProtocolParser.ReadSingle(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.memory_system = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.memory_collections = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.memory_managed_heap = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 93:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.realtime_since_startup = ProtocolParser.ReadSingle(stream);
				continue;
			case 96:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.streamer_mode = ProtocolParser.ReadBool(stream);
				continue;
			case 104:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.ping = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 112:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.tasks_invokes = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 120:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.tasks_load_balancer = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				if (key.WireType == Wire.Varint)
				{
					instance.workshop_skins_queued = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PerformanceReport DeserializeLength(BufferStream stream, int length, PerformanceReport instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.PerformanceReport");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.request_id = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.user_id = ProtocolParser.ReadString(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.fps_average = ProtocolParser.ReadSingle(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.fps = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.frame_id = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.frame_time = ProtocolParser.ReadSingle(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.frame_time_average = ProtocolParser.ReadSingle(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.memory_system = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.memory_collections = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.memory_managed_heap = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 93:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.realtime_since_startup = ProtocolParser.ReadSingle(stream);
				continue;
			case 96:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.streamer_mode = ProtocolParser.ReadBool(stream);
				continue;
			case 104:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.ping = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 112:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.tasks_invokes = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 120:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				instance.tasks_load_balancer = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.PerformanceReport");
				if (key.WireType == Wire.Varint)
				{
					instance.workshop_skins_queued = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PerformanceReport");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, PerformanceReport instance, PerformanceReport previous)
	{
		if (instance.request_id != previous.request_id)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.request_id);
		}
		if (instance.user_id != null && instance.user_id != previous.user_id)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.user_id);
		}
		if (instance.fps_average != previous.fps_average)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.fps_average);
		}
		if (instance.fps != previous.fps)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.fps);
		}
		if (instance.frame_id != previous.frame_id)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.frame_id);
		}
		if (instance.frame_time != previous.frame_time)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.frame_time);
		}
		if (instance.frame_time_average != previous.frame_time_average)
		{
			stream.WriteByte(61);
			ProtocolParser.WriteSingle(stream, instance.frame_time_average);
		}
		stream.WriteByte(64);
		ProtocolParser.WriteUInt64(stream, (ulong)instance.memory_system);
		stream.WriteByte(72);
		ProtocolParser.WriteUInt64(stream, (ulong)instance.memory_collections);
		stream.WriteByte(80);
		ProtocolParser.WriteUInt64(stream, (ulong)instance.memory_managed_heap);
		if (instance.realtime_since_startup != previous.realtime_since_startup)
		{
			stream.WriteByte(93);
			ProtocolParser.WriteSingle(stream, instance.realtime_since_startup);
		}
		stream.WriteByte(96);
		ProtocolParser.WriteBool(stream, instance.streamer_mode);
		if (instance.ping != previous.ping)
		{
			stream.WriteByte(104);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ping);
		}
		if (instance.tasks_invokes != previous.tasks_invokes)
		{
			stream.WriteByte(112);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.tasks_invokes);
		}
		if (instance.tasks_load_balancer != previous.tasks_load_balancer)
		{
			stream.WriteByte(120);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.tasks_load_balancer);
		}
		if (instance.workshop_skins_queued != previous.workshop_skins_queued)
		{
			stream.WriteByte(128);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.workshop_skins_queued);
		}
	}

	public static void Serialize(BufferStream stream, PerformanceReport instance)
	{
		if (instance.request_id != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.request_id);
		}
		if (instance.user_id != null)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.user_id);
		}
		if (instance.fps_average != 0f)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.fps_average);
		}
		if (instance.fps != 0)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.fps);
		}
		if (instance.frame_id != 0)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.frame_id);
		}
		if (instance.frame_time != 0f)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.frame_time);
		}
		if (instance.frame_time_average != 0f)
		{
			stream.WriteByte(61);
			ProtocolParser.WriteSingle(stream, instance.frame_time_average);
		}
		if (instance.memory_system != 0L)
		{
			stream.WriteByte(64);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.memory_system);
		}
		if (instance.memory_collections != 0L)
		{
			stream.WriteByte(72);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.memory_collections);
		}
		if (instance.memory_managed_heap != 0L)
		{
			stream.WriteByte(80);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.memory_managed_heap);
		}
		if (instance.realtime_since_startup != 0f)
		{
			stream.WriteByte(93);
			ProtocolParser.WriteSingle(stream, instance.realtime_since_startup);
		}
		if (instance.streamer_mode)
		{
			stream.WriteByte(96);
			ProtocolParser.WriteBool(stream, instance.streamer_mode);
		}
		if (instance.ping != 0)
		{
			stream.WriteByte(104);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ping);
		}
		if (instance.tasks_invokes != 0)
		{
			stream.WriteByte(112);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.tasks_invokes);
		}
		if (instance.tasks_load_balancer != 0)
		{
			stream.WriteByte(120);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.tasks_load_balancer);
		}
		if (instance.workshop_skins_queued != 0)
		{
			stream.WriteByte(128);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.workshop_skins_queued);
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
