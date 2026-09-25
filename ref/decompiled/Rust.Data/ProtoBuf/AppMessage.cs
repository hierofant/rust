using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class AppMessage : IDisposable, Pool.IPooled, IProto<AppMessage>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public AppResponse response;

	[NonSerialized]
	public AppBroadcast broadcast;

	public static void ResetToPool(AppMessage instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.response != null)
			{
				instance.response.ResetToPool();
				instance.response = null;
			}
			if (instance.broadcast != null)
			{
				instance.broadcast.ResetToPool();
				instance.broadcast = null;
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
			throw new Exception("Trying to dispose AppMessage with ShouldPool set to false!");
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

	public void CopyTo(AppMessage instance)
	{
		if (response != null)
		{
			if (instance.response == null)
			{
				instance.response = response.Copy();
			}
			else
			{
				response.CopyTo(instance.response);
			}
		}
		else
		{
			instance.response = null;
		}
		if (broadcast != null)
		{
			if (instance.broadcast == null)
			{
				instance.broadcast = broadcast.Copy();
			}
			else
			{
				broadcast.CopyTo(instance.broadcast);
			}
		}
		else
		{
			instance.broadcast = null;
		}
	}

	public AppMessage Copy()
	{
		AppMessage appMessage = Pool.Get<AppMessage>();
		CopyTo(appMessage);
		return appMessage;
	}

	public static AppMessage Deserialize(BufferStream stream)
	{
		AppMessage appMessage = Pool.Get<AppMessage>();
		Deserialize(stream, appMessage, isDelta: false);
		return appMessage;
	}

	public static AppMessage DeserializeLengthDelimited(BufferStream stream)
	{
		AppMessage appMessage = Pool.Get<AppMessage>();
		DeserializeLengthDelimited(stream, appMessage, isDelta: false);
		return appMessage;
	}

	public static AppMessage DeserializeLength(BufferStream stream, int length)
	{
		AppMessage appMessage = Pool.Get<AppMessage>();
		DeserializeLength(stream, length, appMessage, isDelta: false);
		return appMessage;
	}

	public static AppMessage Deserialize(byte[] buffer)
	{
		AppMessage appMessage = Pool.Get<AppMessage>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, appMessage, isDelta: false);
		return appMessage;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, AppMessage previous)
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

	public static AppMessage Deserialize(BufferStream stream, AppMessage instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.AppMessage");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppMessage");
				if (instance.response == null)
				{
					instance.response = AppResponse.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppResponse.DeserializeLengthDelimited(stream, instance.response, isDelta);
				}
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppMessage");
				if (instance.broadcast == null)
				{
					instance.broadcast = AppBroadcast.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppBroadcast.DeserializeLengthDelimited(stream, instance.broadcast, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AppMessage DeserializeLengthDelimited(BufferStream stream, AppMessage instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.AppMessage");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppMessage");
				if (instance.response == null)
				{
					instance.response = AppResponse.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppResponse.DeserializeLengthDelimited(stream, instance.response, isDelta);
				}
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppMessage");
				if (instance.broadcast == null)
				{
					instance.broadcast = AppBroadcast.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppBroadcast.DeserializeLengthDelimited(stream, instance.broadcast, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AppMessage DeserializeLength(BufferStream stream, int length, AppMessage instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.AppMessage");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppMessage");
				if (instance.response == null)
				{
					instance.response = AppResponse.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppResponse.DeserializeLengthDelimited(stream, instance.response, isDelta);
				}
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppMessage");
				if (instance.broadcast == null)
				{
					instance.broadcast = AppBroadcast.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppBroadcast.DeserializeLengthDelimited(stream, instance.broadcast, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, AppMessage instance, AppMessage previous)
	{
		if (instance.response != null)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			AppResponse.SerializeDelta(stream, instance.response, previous.response);
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
		if (instance.broadcast == null)
		{
			return;
		}
		stream.WriteByte(18);
		BufferStream.RangeHandle range2 = stream.GetRange(5);
		int position2 = stream.Position;
		AppBroadcast.SerializeDelta(stream, instance.broadcast, previous.broadcast);
		int val2 = stream.Position - position2;
		Span<byte> span2 = range2.GetSpan();
		int num2 = ProtocolParser.WriteUInt32((uint)val2, span2, 0);
		if (num2 < 5)
		{
			span2[num2 - 1] |= 128;
			while (num2 < 4)
			{
				span2[num2++] = 128;
			}
			span2[4] = 0;
		}
	}

	public static void Serialize(BufferStream stream, AppMessage instance)
	{
		if (instance.response != null)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			AppResponse.Serialize(stream, instance.response);
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
		if (instance.broadcast == null)
		{
			return;
		}
		stream.WriteByte(18);
		BufferStream.RangeHandle range2 = stream.GetRange(5);
		int position2 = stream.Position;
		AppBroadcast.Serialize(stream, instance.broadcast);
		int val2 = stream.Position - position2;
		Span<byte> span2 = range2.GetSpan();
		int num2 = ProtocolParser.WriteUInt32((uint)val2, span2, 0);
		if (num2 < 5)
		{
			span2[num2 - 1] |= 128;
			while (num2 < 4)
			{
				span2[num2++] = 128;
			}
			span2[4] = 0;
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		response?.InspectUids(action);
		broadcast?.InspectUids(action);
	}
}
