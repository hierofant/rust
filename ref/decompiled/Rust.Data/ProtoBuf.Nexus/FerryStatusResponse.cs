using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf.Nexus;

public class FerryStatusResponse : IDisposable, Pool.IPooled, IProto<FerryStatusResponse>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<FerryStatus> statuses;

	public static void ResetToPool(FerryStatusResponse instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.statuses != null)
		{
			for (int i = 0; i < instance.statuses.Count; i++)
			{
				if (instance.statuses[i] != null)
				{
					instance.statuses[i].ResetToPool();
					instance.statuses[i] = null;
				}
			}
			List<FerryStatus> obj = instance.statuses;
			Pool.Free(ref obj, freeElements: false);
			instance.statuses = obj;
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
			throw new Exception("Trying to dispose FerryStatusResponse with ShouldPool set to false!");
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

	public void CopyTo(FerryStatusResponse instance)
	{
		if (statuses != null)
		{
			instance.statuses = Pool.Get<List<FerryStatus>>();
			for (int i = 0; i < statuses.Count; i++)
			{
				FerryStatus item = statuses[i].Copy();
				instance.statuses.Add(item);
			}
		}
		else
		{
			instance.statuses = null;
		}
	}

	public FerryStatusResponse Copy()
	{
		FerryStatusResponse ferryStatusResponse = Pool.Get<FerryStatusResponse>();
		CopyTo(ferryStatusResponse);
		return ferryStatusResponse;
	}

	public static FerryStatusResponse Deserialize(BufferStream stream)
	{
		FerryStatusResponse ferryStatusResponse = Pool.Get<FerryStatusResponse>();
		Deserialize(stream, ferryStatusResponse, isDelta: false);
		return ferryStatusResponse;
	}

	public static FerryStatusResponse DeserializeLengthDelimited(BufferStream stream)
	{
		FerryStatusResponse ferryStatusResponse = Pool.Get<FerryStatusResponse>();
		DeserializeLengthDelimited(stream, ferryStatusResponse, isDelta: false);
		return ferryStatusResponse;
	}

	public static FerryStatusResponse DeserializeLength(BufferStream stream, int length)
	{
		FerryStatusResponse ferryStatusResponse = Pool.Get<FerryStatusResponse>();
		DeserializeLength(stream, length, ferryStatusResponse, isDelta: false);
		return ferryStatusResponse;
	}

	public static FerryStatusResponse Deserialize(byte[] buffer)
	{
		FerryStatusResponse ferryStatusResponse = Pool.Get<FerryStatusResponse>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, ferryStatusResponse, isDelta: false);
		return ferryStatusResponse;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, FerryStatusResponse previous)
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

	public static FerryStatusResponse Deserialize(BufferStream stream, FerryStatusResponse instance, bool isDelta)
	{
		if (!isDelta && instance.statuses == null)
		{
			instance.statuses = Pool.Get<List<FerryStatus>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.FerryStatusResponse");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryStatusResponse");
				stream.ConsumeRepeatedElement();
				instance.statuses.Add(FerryStatus.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryStatusResponse");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryStatusResponse");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static FerryStatusResponse DeserializeLengthDelimited(BufferStream stream, FerryStatusResponse instance, bool isDelta)
	{
		if (!isDelta && instance.statuses == null)
		{
			instance.statuses = Pool.Get<List<FerryStatus>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.FerryStatusResponse");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryStatusResponse");
				stream.ConsumeRepeatedElement();
				instance.statuses.Add(FerryStatus.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryStatusResponse");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryStatusResponse");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static FerryStatusResponse DeserializeLength(BufferStream stream, int length, FerryStatusResponse instance, bool isDelta)
	{
		if (!isDelta && instance.statuses == null)
		{
			instance.statuses = Pool.Get<List<FerryStatus>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.FerryStatusResponse");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryStatusResponse");
				stream.ConsumeRepeatedElement();
				instance.statuses.Add(FerryStatus.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryStatusResponse");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryStatusResponse");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, FerryStatusResponse instance, FerryStatusResponse previous)
	{
		if (instance.statuses == null)
		{
			return;
		}
		for (int i = 0; i < instance.statuses.Count; i++)
		{
			FerryStatus ferryStatus = instance.statuses[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			FerryStatus.SerializeDelta(stream, ferryStatus, ferryStatus);
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

	public static void Serialize(BufferStream stream, FerryStatusResponse instance)
	{
		if (instance.statuses == null)
		{
			return;
		}
		for (int i = 0; i < instance.statuses.Count; i++)
		{
			FerryStatus instance2 = instance.statuses[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			FerryStatus.Serialize(stream, instance2);
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
		if (statuses != null)
		{
			for (int i = 0; i < statuses.Count; i++)
			{
				statuses[i]?.InspectUids(action);
			}
		}
	}
}
