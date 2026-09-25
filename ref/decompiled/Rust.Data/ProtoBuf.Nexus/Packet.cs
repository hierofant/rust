using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf.Nexus;

public class Packet : IDisposable, Pool.IPooled, IProto<Packet>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public uint protocol;

	[NonSerialized]
	public int sourceZone;

	[NonSerialized]
	public Request request;

	[NonSerialized]
	public Response response;

	public static void ResetToPool(Packet instance)
	{
		if (instance.ShouldPool)
		{
			instance.protocol = 0u;
			instance.sourceZone = 0;
			if (instance.request != null)
			{
				instance.request.ResetToPool();
				instance.request = null;
			}
			if (instance.response != null)
			{
				instance.response.ResetToPool();
				instance.response = null;
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
			throw new Exception("Trying to dispose Packet with ShouldPool set to false!");
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

	public void CopyTo(Packet instance)
	{
		instance.protocol = protocol;
		instance.sourceZone = sourceZone;
		if (request != null)
		{
			if (instance.request == null)
			{
				instance.request = request.Copy();
			}
			else
			{
				request.CopyTo(instance.request);
			}
		}
		else
		{
			instance.request = null;
		}
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
	}

	public Packet Copy()
	{
		Packet packet = Pool.Get<Packet>();
		CopyTo(packet);
		return packet;
	}

	public static Packet Deserialize(BufferStream stream)
	{
		Packet packet = Pool.Get<Packet>();
		Deserialize(stream, packet, isDelta: false);
		return packet;
	}

	public static Packet DeserializeLengthDelimited(BufferStream stream)
	{
		Packet packet = Pool.Get<Packet>();
		DeserializeLengthDelimited(stream, packet, isDelta: false);
		return packet;
	}

	public static Packet DeserializeLength(BufferStream stream, int length)
	{
		Packet packet = Pool.Get<Packet>();
		DeserializeLength(stream, length, packet, isDelta: false);
		return packet;
	}

	public static Packet Deserialize(byte[] buffer)
	{
		Packet packet = Pool.Get<Packet>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, packet, isDelta: false);
		return packet;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, Packet previous)
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

	public static Packet Deserialize(BufferStream stream, Packet instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.Packet");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.Packet");
				instance.protocol = ProtocolParser.ReadUInt32(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.Packet");
				instance.sourceZone = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.Packet");
				if (instance.request == null)
				{
					instance.request = Request.DeserializeLengthDelimited(stream);
				}
				else
				{
					Request.DeserializeLengthDelimited(stream, instance.request, isDelta);
				}
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Nexus.Packet");
				if (instance.response == null)
				{
					instance.response = Response.DeserializeLengthDelimited(stream);
				}
				else
				{
					Response.DeserializeLengthDelimited(stream, instance.response, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.Packet");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.Packet");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.Packet");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Nexus.Packet");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.Packet");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Packet DeserializeLengthDelimited(BufferStream stream, Packet instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.Packet");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.Packet");
				instance.protocol = ProtocolParser.ReadUInt32(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.Packet");
				instance.sourceZone = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.Packet");
				if (instance.request == null)
				{
					instance.request = Request.DeserializeLengthDelimited(stream);
				}
				else
				{
					Request.DeserializeLengthDelimited(stream, instance.request, isDelta);
				}
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Nexus.Packet");
				if (instance.response == null)
				{
					instance.response = Response.DeserializeLengthDelimited(stream);
				}
				else
				{
					Response.DeserializeLengthDelimited(stream, instance.response, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.Packet");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.Packet");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.Packet");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Nexus.Packet");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.Packet");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Packet DeserializeLength(BufferStream stream, int length, Packet instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.Packet");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.Packet");
				instance.protocol = ProtocolParser.ReadUInt32(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.Packet");
				instance.sourceZone = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.Packet");
				if (instance.request == null)
				{
					instance.request = Request.DeserializeLengthDelimited(stream);
				}
				else
				{
					Request.DeserializeLengthDelimited(stream, instance.request, isDelta);
				}
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Nexus.Packet");
				if (instance.response == null)
				{
					instance.response = Response.DeserializeLengthDelimited(stream);
				}
				else
				{
					Response.DeserializeLengthDelimited(stream, instance.response, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.Packet");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.Packet");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.Packet");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Nexus.Packet");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.Packet");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, Packet instance, Packet previous)
	{
		if (instance.protocol != previous.protocol)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt32(stream, instance.protocol);
		}
		if (instance.sourceZone != previous.sourceZone)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.sourceZone);
		}
		if (instance.request != null)
		{
			stream.WriteByte(26);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			Request.SerializeDelta(stream, instance.request, previous.request);
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
		if (instance.response == null)
		{
			return;
		}
		stream.WriteByte(34);
		BufferStream.RangeHandle range2 = stream.GetRange(5);
		int position2 = stream.Position;
		Response.SerializeDelta(stream, instance.response, previous.response);
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

	public static void Serialize(BufferStream stream, Packet instance)
	{
		if (instance.protocol != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt32(stream, instance.protocol);
		}
		if (instance.sourceZone != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.sourceZone);
		}
		if (instance.request != null)
		{
			stream.WriteByte(26);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			Request.Serialize(stream, instance.request);
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
		if (instance.response == null)
		{
			return;
		}
		stream.WriteByte(34);
		BufferStream.RangeHandle range2 = stream.GetRange(5);
		int position2 = stream.Position;
		Response.Serialize(stream, instance.response);
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
		request?.InspectUids(action);
		response?.InspectUids(action);
	}
}
