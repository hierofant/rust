using System;
using System.IO;
using Facepunch;
using Facepunch.Nexus;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf.Nexus;

public class Response : IDisposable, Pool.IPooled, IProto<Response>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public Uuid id;

	[NonSerialized]
	public Status status;

	[NonSerialized]
	public PingResponse ping;

	[NonSerialized]
	public SpawnOptionsResponse spawnOptions;

	[NonSerialized]
	public FerryStatusResponse ferryStatus;

	public static void ResetToPool(Response instance)
	{
		if (instance.ShouldPool)
		{
			instance.id = default(Uuid);
			if (instance.status != null)
			{
				instance.status.ResetToPool();
				instance.status = null;
			}
			if (instance.ping != null)
			{
				instance.ping.ResetToPool();
				instance.ping = null;
			}
			if (instance.spawnOptions != null)
			{
				instance.spawnOptions.ResetToPool();
				instance.spawnOptions = null;
			}
			if (instance.ferryStatus != null)
			{
				instance.ferryStatus.ResetToPool();
				instance.ferryStatus = null;
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
			throw new Exception("Trying to dispose Response with ShouldPool set to false!");
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

	public void CopyTo(Response instance)
	{
		instance.id = id;
		if (status != null)
		{
			if (instance.status == null)
			{
				instance.status = status.Copy();
			}
			else
			{
				status.CopyTo(instance.status);
			}
		}
		else
		{
			instance.status = null;
		}
		if (ping != null)
		{
			if (instance.ping == null)
			{
				instance.ping = ping.Copy();
			}
			else
			{
				ping.CopyTo(instance.ping);
			}
		}
		else
		{
			instance.ping = null;
		}
		if (spawnOptions != null)
		{
			if (instance.spawnOptions == null)
			{
				instance.spawnOptions = spawnOptions.Copy();
			}
			else
			{
				spawnOptions.CopyTo(instance.spawnOptions);
			}
		}
		else
		{
			instance.spawnOptions = null;
		}
		if (ferryStatus != null)
		{
			if (instance.ferryStatus == null)
			{
				instance.ferryStatus = ferryStatus.Copy();
			}
			else
			{
				ferryStatus.CopyTo(instance.ferryStatus);
			}
		}
		else
		{
			instance.ferryStatus = null;
		}
	}

	public Response Copy()
	{
		Response response = Pool.Get<Response>();
		CopyTo(response);
		return response;
	}

	public static Response Deserialize(BufferStream stream)
	{
		Response response = Pool.Get<Response>();
		Deserialize(stream, response, isDelta: false);
		return response;
	}

	public static Response DeserializeLengthDelimited(BufferStream stream)
	{
		Response response = Pool.Get<Response>();
		DeserializeLengthDelimited(stream, response, isDelta: false);
		return response;
	}

	public static Response DeserializeLength(BufferStream stream, int length)
	{
		Response response = Pool.Get<Response>();
		DeserializeLength(stream, length, response, isDelta: false);
		return response;
	}

	public static Response Deserialize(byte[] buffer)
	{
		Response response = Pool.Get<Response>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, response, isDelta: false);
		return response;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, Response previous)
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

	public static Response Deserialize(BufferStream stream, Response instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.Response");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.Response");
				UuidSerialized.DeserializeLengthDelimited(stream, ref instance.id, isDelta);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.Response");
				if (instance.status == null)
				{
					instance.status = Status.DeserializeLengthDelimited(stream);
				}
				else
				{
					Status.DeserializeLengthDelimited(stream, instance.status, isDelta);
				}
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.Response");
				if (instance.ping == null)
				{
					instance.ping = PingResponse.DeserializeLengthDelimited(stream);
				}
				else
				{
					PingResponse.DeserializeLengthDelimited(stream, instance.ping, isDelta);
				}
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Nexus.Response");
				if (instance.spawnOptions == null)
				{
					instance.spawnOptions = SpawnOptionsResponse.DeserializeLengthDelimited(stream);
				}
				else
				{
					SpawnOptionsResponse.DeserializeLengthDelimited(stream, instance.spawnOptions, isDelta);
				}
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Nexus.Response");
				if (instance.ferryStatus == null)
				{
					instance.ferryStatus = FerryStatusResponse.DeserializeLengthDelimited(stream);
				}
				else
				{
					FerryStatusResponse.DeserializeLengthDelimited(stream, instance.ferryStatus, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.Response");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.Response");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.Response");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Nexus.Response");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Nexus.Response");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.Response");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Response DeserializeLengthDelimited(BufferStream stream, Response instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.Response");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.Response");
				UuidSerialized.DeserializeLengthDelimited(stream, ref instance.id, isDelta);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.Response");
				if (instance.status == null)
				{
					instance.status = Status.DeserializeLengthDelimited(stream);
				}
				else
				{
					Status.DeserializeLengthDelimited(stream, instance.status, isDelta);
				}
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.Response");
				if (instance.ping == null)
				{
					instance.ping = PingResponse.DeserializeLengthDelimited(stream);
				}
				else
				{
					PingResponse.DeserializeLengthDelimited(stream, instance.ping, isDelta);
				}
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Nexus.Response");
				if (instance.spawnOptions == null)
				{
					instance.spawnOptions = SpawnOptionsResponse.DeserializeLengthDelimited(stream);
				}
				else
				{
					SpawnOptionsResponse.DeserializeLengthDelimited(stream, instance.spawnOptions, isDelta);
				}
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Nexus.Response");
				if (instance.ferryStatus == null)
				{
					instance.ferryStatus = FerryStatusResponse.DeserializeLengthDelimited(stream);
				}
				else
				{
					FerryStatusResponse.DeserializeLengthDelimited(stream, instance.ferryStatus, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.Response");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.Response");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.Response");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Nexus.Response");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Nexus.Response");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.Response");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Response DeserializeLength(BufferStream stream, int length, Response instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.Response");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.Response");
				UuidSerialized.DeserializeLengthDelimited(stream, ref instance.id, isDelta);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.Response");
				if (instance.status == null)
				{
					instance.status = Status.DeserializeLengthDelimited(stream);
				}
				else
				{
					Status.DeserializeLengthDelimited(stream, instance.status, isDelta);
				}
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.Response");
				if (instance.ping == null)
				{
					instance.ping = PingResponse.DeserializeLengthDelimited(stream);
				}
				else
				{
					PingResponse.DeserializeLengthDelimited(stream, instance.ping, isDelta);
				}
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Nexus.Response");
				if (instance.spawnOptions == null)
				{
					instance.spawnOptions = SpawnOptionsResponse.DeserializeLengthDelimited(stream);
				}
				else
				{
					SpawnOptionsResponse.DeserializeLengthDelimited(stream, instance.spawnOptions, isDelta);
				}
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Nexus.Response");
				if (instance.ferryStatus == null)
				{
					instance.ferryStatus = FerryStatusResponse.DeserializeLengthDelimited(stream);
				}
				else
				{
					FerryStatusResponse.DeserializeLengthDelimited(stream, instance.ferryStatus, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.Response");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.Response");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.Response");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Nexus.Response");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Nexus.Response");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.Response");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, Response instance, Response previous)
	{
		stream.WriteByte(10);
		BufferStream.RangeHandle range = stream.GetRange(1);
		int position = stream.Position;
		UuidSerialized.SerializeDelta(stream, instance.id, previous.id);
		int num = stream.Position - position;
		if (num > 127)
		{
			throw new InvalidOperationException("Not enough space was reserved for the length prefix of field id (Facepunch.Nexus.Uuid)");
		}
		Span<byte> span = range.GetSpan();
		ProtocolParser.WriteUInt32((uint)num, span, 0);
		if (instance.status == null)
		{
			throw new ArgumentNullException("status", "Required by proto specification.");
		}
		stream.WriteByte(18);
		BufferStream.RangeHandle range2 = stream.GetRange(5);
		int position2 = stream.Position;
		Status.SerializeDelta(stream, instance.status, previous.status);
		int val = stream.Position - position2;
		Span<byte> span2 = range2.GetSpan();
		int num2 = ProtocolParser.WriteUInt32((uint)val, span2, 0);
		if (num2 < 5)
		{
			span2[num2 - 1] |= 128;
			while (num2 < 4)
			{
				span2[num2++] = 128;
			}
			span2[4] = 0;
		}
		if (instance.ping != null)
		{
			stream.WriteByte(26);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			PingResponse.SerializeDelta(stream, instance.ping, previous.ping);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field ping (ProtoBuf.Nexus.PingResponse)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
		if (instance.spawnOptions != null)
		{
			stream.WriteByte(34);
			BufferStream.RangeHandle range4 = stream.GetRange(5);
			int position4 = stream.Position;
			SpawnOptionsResponse.SerializeDelta(stream, instance.spawnOptions, previous.spawnOptions);
			int val2 = stream.Position - position4;
			Span<byte> span4 = range4.GetSpan();
			int num4 = ProtocolParser.WriteUInt32((uint)val2, span4, 0);
			if (num4 < 5)
			{
				span4[num4 - 1] |= 128;
				while (num4 < 4)
				{
					span4[num4++] = 128;
				}
				span4[4] = 0;
			}
		}
		if (instance.ferryStatus == null)
		{
			return;
		}
		stream.WriteByte(42);
		BufferStream.RangeHandle range5 = stream.GetRange(5);
		int position5 = stream.Position;
		FerryStatusResponse.SerializeDelta(stream, instance.ferryStatus, previous.ferryStatus);
		int val3 = stream.Position - position5;
		Span<byte> span5 = range5.GetSpan();
		int num5 = ProtocolParser.WriteUInt32((uint)val3, span5, 0);
		if (num5 < 5)
		{
			span5[num5 - 1] |= 128;
			while (num5 < 4)
			{
				span5[num5++] = 128;
			}
			span5[4] = 0;
		}
	}

	public static void Serialize(BufferStream stream, Response instance)
	{
		if (instance.id != default(Uuid))
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			UuidSerialized.Serialize(stream, instance.id);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field id (Facepunch.Nexus.Uuid)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.status == null)
		{
			throw new ArgumentNullException("status", "Required by proto specification.");
		}
		stream.WriteByte(18);
		BufferStream.RangeHandle range2 = stream.GetRange(5);
		int position2 = stream.Position;
		Status.Serialize(stream, instance.status);
		int val = stream.Position - position2;
		Span<byte> span2 = range2.GetSpan();
		int num2 = ProtocolParser.WriteUInt32((uint)val, span2, 0);
		if (num2 < 5)
		{
			span2[num2 - 1] |= 128;
			while (num2 < 4)
			{
				span2[num2++] = 128;
			}
			span2[4] = 0;
		}
		if (instance.ping != null)
		{
			stream.WriteByte(26);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			PingResponse.Serialize(stream, instance.ping);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field ping (ProtoBuf.Nexus.PingResponse)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
		if (instance.spawnOptions != null)
		{
			stream.WriteByte(34);
			BufferStream.RangeHandle range4 = stream.GetRange(5);
			int position4 = stream.Position;
			SpawnOptionsResponse.Serialize(stream, instance.spawnOptions);
			int val2 = stream.Position - position4;
			Span<byte> span4 = range4.GetSpan();
			int num4 = ProtocolParser.WriteUInt32((uint)val2, span4, 0);
			if (num4 < 5)
			{
				span4[num4 - 1] |= 128;
				while (num4 < 4)
				{
					span4[num4++] = 128;
				}
				span4[4] = 0;
			}
		}
		if (instance.ferryStatus == null)
		{
			return;
		}
		stream.WriteByte(42);
		BufferStream.RangeHandle range5 = stream.GetRange(5);
		int position5 = stream.Position;
		FerryStatusResponse.Serialize(stream, instance.ferryStatus);
		int val3 = stream.Position - position5;
		Span<byte> span5 = range5.GetSpan();
		int num5 = ProtocolParser.WriteUInt32((uint)val3, span5, 0);
		if (num5 < 5)
		{
			span5[num5 - 1] |= 128;
			while (num5 < 4)
			{
				span5[num5++] = 128;
			}
			span5[4] = 0;
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		status?.InspectUids(action);
		ping?.InspectUids(action);
		spawnOptions?.InspectUids(action);
		ferryStatus?.InspectUids(action);
	}
}
