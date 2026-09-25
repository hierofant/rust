using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class AppMapMarkers : IDisposable, Pool.IPooled, IProto<AppMapMarkers>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<AppMarker> markers;

	public static void ResetToPool(AppMapMarkers instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.markers != null)
		{
			for (int i = 0; i < instance.markers.Count; i++)
			{
				if (instance.markers[i] != null)
				{
					instance.markers[i].ResetToPool();
					instance.markers[i] = null;
				}
			}
			List<AppMarker> obj = instance.markers;
			Pool.Free(ref obj, freeElements: false);
			instance.markers = obj;
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
			throw new Exception("Trying to dispose AppMapMarkers with ShouldPool set to false!");
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

	public void CopyTo(AppMapMarkers instance)
	{
		if (markers != null)
		{
			instance.markers = Pool.Get<List<AppMarker>>();
			for (int i = 0; i < markers.Count; i++)
			{
				AppMarker item = markers[i].Copy();
				instance.markers.Add(item);
			}
		}
		else
		{
			instance.markers = null;
		}
	}

	public AppMapMarkers Copy()
	{
		AppMapMarkers appMapMarkers = Pool.Get<AppMapMarkers>();
		CopyTo(appMapMarkers);
		return appMapMarkers;
	}

	public static AppMapMarkers Deserialize(BufferStream stream)
	{
		AppMapMarkers appMapMarkers = Pool.Get<AppMapMarkers>();
		Deserialize(stream, appMapMarkers, isDelta: false);
		return appMapMarkers;
	}

	public static AppMapMarkers DeserializeLengthDelimited(BufferStream stream)
	{
		AppMapMarkers appMapMarkers = Pool.Get<AppMapMarkers>();
		DeserializeLengthDelimited(stream, appMapMarkers, isDelta: false);
		return appMapMarkers;
	}

	public static AppMapMarkers DeserializeLength(BufferStream stream, int length)
	{
		AppMapMarkers appMapMarkers = Pool.Get<AppMapMarkers>();
		DeserializeLength(stream, length, appMapMarkers, isDelta: false);
		return appMapMarkers;
	}

	public static AppMapMarkers Deserialize(byte[] buffer)
	{
		AppMapMarkers appMapMarkers = Pool.Get<AppMapMarkers>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, appMapMarkers, isDelta: false);
		return appMapMarkers;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, AppMapMarkers previous)
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

	public static AppMapMarkers Deserialize(BufferStream stream, AppMapMarkers instance, bool isDelta)
	{
		if (!isDelta && instance.markers == null)
		{
			instance.markers = Pool.Get<List<AppMarker>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.AppMapMarkers");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.AppMapMarkers");
				stream.ConsumeRepeatedElement();
				instance.markers.Add(AppMarker.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.AppMapMarkers");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppMapMarkers");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static AppMapMarkers DeserializeLengthDelimited(BufferStream stream, AppMapMarkers instance, bool isDelta)
	{
		if (!isDelta && instance.markers == null)
		{
			instance.markers = Pool.Get<List<AppMarker>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.AppMapMarkers");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.AppMapMarkers");
				stream.ConsumeRepeatedElement();
				instance.markers.Add(AppMarker.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.AppMapMarkers");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppMapMarkers");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static AppMapMarkers DeserializeLength(BufferStream stream, int length, AppMapMarkers instance, bool isDelta)
	{
		if (!isDelta && instance.markers == null)
		{
			instance.markers = Pool.Get<List<AppMarker>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.AppMapMarkers");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.AppMapMarkers");
				stream.ConsumeRepeatedElement();
				instance.markers.Add(AppMarker.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.AppMapMarkers");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppMapMarkers");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, AppMapMarkers instance, AppMapMarkers previous)
	{
		if (instance.markers == null)
		{
			return;
		}
		for (int i = 0; i < instance.markers.Count; i++)
		{
			AppMarker appMarker = instance.markers[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			AppMarker.SerializeDelta(stream, appMarker, appMarker);
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

	public static void Serialize(BufferStream stream, AppMapMarkers instance)
	{
		if (instance.markers == null)
		{
			return;
		}
		for (int i = 0; i < instance.markers.Count; i++)
		{
			AppMarker instance2 = instance.markers[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			AppMarker.Serialize(stream, instance2);
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
		if (markers != null)
		{
			for (int i = 0; i < markers.Count; i++)
			{
				markers[i]?.InspectUids(action);
			}
		}
	}
}
