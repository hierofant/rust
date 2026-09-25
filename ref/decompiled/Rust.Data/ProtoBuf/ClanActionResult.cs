using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ClanActionResult : IDisposable, Pool.IPooled, IProto<ClanActionResult>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public int requestId;

	[NonSerialized]
	public int result;

	[NonSerialized]
	public bool hasClanInfo;

	[NonSerialized]
	public ClanInfo clanInfo;

	public static void ResetToPool(ClanActionResult instance)
	{
		if (instance.ShouldPool)
		{
			instance.requestId = 0;
			instance.result = 0;
			instance.hasClanInfo = false;
			if (instance.clanInfo != null)
			{
				instance.clanInfo.ResetToPool();
				instance.clanInfo = null;
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
			throw new Exception("Trying to dispose ClanActionResult with ShouldPool set to false!");
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

	public void CopyTo(ClanActionResult instance)
	{
		instance.requestId = requestId;
		instance.result = result;
		instance.hasClanInfo = hasClanInfo;
		if (clanInfo != null)
		{
			if (instance.clanInfo == null)
			{
				instance.clanInfo = clanInfo.Copy();
			}
			else
			{
				clanInfo.CopyTo(instance.clanInfo);
			}
		}
		else
		{
			instance.clanInfo = null;
		}
	}

	public ClanActionResult Copy()
	{
		ClanActionResult instance = Pool.Get<ClanActionResult>();
		CopyTo(instance);
		return instance;
	}

	public static ClanActionResult Deserialize(BufferStream stream)
	{
		ClanActionResult instance = Pool.Get<ClanActionResult>();
		Deserialize(stream, instance, isDelta: false);
		return instance;
	}

	public static ClanActionResult DeserializeLengthDelimited(BufferStream stream)
	{
		ClanActionResult instance = Pool.Get<ClanActionResult>();
		DeserializeLengthDelimited(stream, instance, isDelta: false);
		return instance;
	}

	public static ClanActionResult DeserializeLength(BufferStream stream, int length)
	{
		ClanActionResult instance = Pool.Get<ClanActionResult>();
		DeserializeLength(stream, length, instance, isDelta: false);
		return instance;
	}

	public static ClanActionResult Deserialize(byte[] buffer)
	{
		ClanActionResult instance = Pool.Get<ClanActionResult>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, instance, isDelta: false);
		return instance;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ClanActionResult previous)
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

	public static ClanActionResult Deserialize(BufferStream stream, ClanActionResult instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.requestId = 0;
			instance.result = 0;
			instance.hasClanInfo = false;
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ClanActionResult");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanActionResult");
				instance.requestId = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanActionResult");
				instance.result = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanActionResult");
				instance.hasClanInfo = ProtocolParser.ReadBool(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanActionResult");
				if (instance.clanInfo == null)
				{
					instance.clanInfo = ClanInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					ClanInfo.DeserializeLengthDelimited(stream, instance.clanInfo, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanActionResult");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanActionResult");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanActionResult");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanActionResult");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanActionResult");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ClanActionResult DeserializeLengthDelimited(BufferStream stream, ClanActionResult instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.requestId = 0;
			instance.result = 0;
			instance.hasClanInfo = false;
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
			stream.ConsumeFieldOperation("ProtoBuf.ClanActionResult");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanActionResult");
				instance.requestId = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanActionResult");
				instance.result = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanActionResult");
				instance.hasClanInfo = ProtocolParser.ReadBool(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanActionResult");
				if (instance.clanInfo == null)
				{
					instance.clanInfo = ClanInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					ClanInfo.DeserializeLengthDelimited(stream, instance.clanInfo, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanActionResult");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanActionResult");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanActionResult");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanActionResult");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanActionResult");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ClanActionResult DeserializeLength(BufferStream stream, int length, ClanActionResult instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.requestId = 0;
			instance.result = 0;
			instance.hasClanInfo = false;
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
			stream.ConsumeFieldOperation("ProtoBuf.ClanActionResult");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanActionResult");
				instance.requestId = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanActionResult");
				instance.result = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanActionResult");
				instance.hasClanInfo = ProtocolParser.ReadBool(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanActionResult");
				if (instance.clanInfo == null)
				{
					instance.clanInfo = ClanInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					ClanInfo.DeserializeLengthDelimited(stream, instance.clanInfo, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanActionResult");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanActionResult");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanActionResult");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanActionResult");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanActionResult");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ClanActionResult instance, ClanActionResult previous)
	{
		if (instance.requestId != previous.requestId)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.requestId);
		}
		if (instance.result != previous.result)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.result);
		}
		stream.WriteByte(24);
		ProtocolParser.WriteBool(stream, instance.hasClanInfo);
		if (instance.clanInfo == null)
		{
			return;
		}
		stream.WriteByte(34);
		BufferStream.RangeHandle range = stream.GetRange(5);
		int position = stream.Position;
		ClanInfo.SerializeDelta(stream, instance.clanInfo, previous.clanInfo);
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

	public static void Serialize(BufferStream stream, ClanActionResult instance)
	{
		if (instance.requestId != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.requestId);
		}
		if (instance.result != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.result);
		}
		if (instance.hasClanInfo)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteBool(stream, instance.hasClanInfo);
		}
		if (instance.clanInfo == null)
		{
			return;
		}
		stream.WriteByte(34);
		BufferStream.RangeHandle range = stream.GetRange(5);
		int position = stream.Position;
		ClanInfo.Serialize(stream, instance.clanInfo);
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
		clanInfo?.InspectUids(action);
	}
}
