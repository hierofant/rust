using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf.Nexus;

public class TransferRequest : IDisposable, Pool.IPooled, IProto<TransferRequest>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public string method;

	[NonSerialized]
	public string from;

	[NonSerialized]
	public string to;

	[NonSerialized]
	public List<Entity> entities;

	[NonSerialized]
	public List<PlayerSecondaryData> secondaryData;

	public static void ResetToPool(TransferRequest instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.method = string.Empty;
		instance.from = string.Empty;
		instance.to = string.Empty;
		if (instance.entities != null)
		{
			for (int i = 0; i < instance.entities.Count; i++)
			{
				if (instance.entities[i] != null)
				{
					instance.entities[i].ResetToPool();
					instance.entities[i] = null;
				}
			}
			List<Entity> obj = instance.entities;
			Pool.Free(ref obj, freeElements: false);
			instance.entities = obj;
		}
		if (instance.secondaryData != null)
		{
			for (int j = 0; j < instance.secondaryData.Count; j++)
			{
				if (instance.secondaryData[j] != null)
				{
					instance.secondaryData[j].ResetToPool();
					instance.secondaryData[j] = null;
				}
			}
			List<PlayerSecondaryData> obj2 = instance.secondaryData;
			Pool.Free(ref obj2, freeElements: false);
			instance.secondaryData = obj2;
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
			throw new Exception("Trying to dispose TransferRequest with ShouldPool set to false!");
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

	public void CopyTo(TransferRequest instance)
	{
		instance.method = method;
		instance.from = from;
		instance.to = to;
		if (entities != null)
		{
			instance.entities = Pool.Get<List<Entity>>();
			for (int i = 0; i < entities.Count; i++)
			{
				Entity item = entities[i].Copy();
				instance.entities.Add(item);
			}
		}
		else
		{
			instance.entities = null;
		}
		if (secondaryData != null)
		{
			instance.secondaryData = Pool.Get<List<PlayerSecondaryData>>();
			for (int j = 0; j < secondaryData.Count; j++)
			{
				PlayerSecondaryData item2 = secondaryData[j].Copy();
				instance.secondaryData.Add(item2);
			}
		}
		else
		{
			instance.secondaryData = null;
		}
	}

	public TransferRequest Copy()
	{
		TransferRequest transferRequest = Pool.Get<TransferRequest>();
		CopyTo(transferRequest);
		return transferRequest;
	}

	public static TransferRequest Deserialize(BufferStream stream)
	{
		TransferRequest transferRequest = Pool.Get<TransferRequest>();
		Deserialize(stream, transferRequest, isDelta: false);
		return transferRequest;
	}

	public static TransferRequest DeserializeLengthDelimited(BufferStream stream)
	{
		TransferRequest transferRequest = Pool.Get<TransferRequest>();
		DeserializeLengthDelimited(stream, transferRequest, isDelta: false);
		return transferRequest;
	}

	public static TransferRequest DeserializeLength(BufferStream stream, int length)
	{
		TransferRequest transferRequest = Pool.Get<TransferRequest>();
		DeserializeLength(stream, length, transferRequest, isDelta: false);
		return transferRequest;
	}

	public static TransferRequest Deserialize(byte[] buffer)
	{
		TransferRequest transferRequest = Pool.Get<TransferRequest>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, transferRequest, isDelta: false);
		return transferRequest;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, TransferRequest previous)
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

	public static TransferRequest Deserialize(BufferStream stream, TransferRequest instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.entities == null)
			{
				instance.entities = Pool.Get<List<Entity>>();
			}
			if (instance.secondaryData == null)
			{
				instance.secondaryData = Pool.Get<List<PlayerSecondaryData>>();
			}
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.TransferRequest");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.TransferRequest");
				instance.method = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.TransferRequest");
				instance.from = ProtocolParser.ReadString(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.TransferRequest");
				instance.to = ProtocolParser.ReadString(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.Nexus.TransferRequest");
				stream.ConsumeRepeatedElement();
				instance.entities.Add(Entity.DeserializeLengthDelimited(stream));
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.Nexus.TransferRequest");
				stream.ConsumeRepeatedElement();
				instance.secondaryData.Add(PlayerSecondaryData.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.TransferRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.TransferRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.TransferRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.Nexus.TransferRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.Nexus.TransferRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.TransferRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static TransferRequest DeserializeLengthDelimited(BufferStream stream, TransferRequest instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.entities == null)
			{
				instance.entities = Pool.Get<List<Entity>>();
			}
			if (instance.secondaryData == null)
			{
				instance.secondaryData = Pool.Get<List<PlayerSecondaryData>>();
			}
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.TransferRequest");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.TransferRequest");
				instance.method = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.TransferRequest");
				instance.from = ProtocolParser.ReadString(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.TransferRequest");
				instance.to = ProtocolParser.ReadString(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.Nexus.TransferRequest");
				stream.ConsumeRepeatedElement();
				instance.entities.Add(Entity.DeserializeLengthDelimited(stream));
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.Nexus.TransferRequest");
				stream.ConsumeRepeatedElement();
				instance.secondaryData.Add(PlayerSecondaryData.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.TransferRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.TransferRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.TransferRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.Nexus.TransferRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.Nexus.TransferRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.TransferRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static TransferRequest DeserializeLength(BufferStream stream, int length, TransferRequest instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.entities == null)
			{
				instance.entities = Pool.Get<List<Entity>>();
			}
			if (instance.secondaryData == null)
			{
				instance.secondaryData = Pool.Get<List<PlayerSecondaryData>>();
			}
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.TransferRequest");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.TransferRequest");
				instance.method = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.TransferRequest");
				instance.from = ProtocolParser.ReadString(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.TransferRequest");
				instance.to = ProtocolParser.ReadString(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.Nexus.TransferRequest");
				stream.ConsumeRepeatedElement();
				instance.entities.Add(Entity.DeserializeLengthDelimited(stream));
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.Nexus.TransferRequest");
				stream.ConsumeRepeatedElement();
				instance.secondaryData.Add(PlayerSecondaryData.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.TransferRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.TransferRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.TransferRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.Nexus.TransferRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.Nexus.TransferRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.TransferRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, TransferRequest instance, TransferRequest previous)
	{
		if (instance.method != null && instance.method != previous.method)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.method);
		}
		if (instance.from != null && instance.from != previous.from)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.from);
		}
		if (instance.to != null && instance.to != previous.to)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteString(stream, instance.to);
		}
		if (instance.entities != null)
		{
			for (int i = 0; i < instance.entities.Count; i++)
			{
				Entity entity = instance.entities[i];
				stream.WriteByte(34);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				Entity.SerializeDelta(stream, entity, entity);
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
		if (instance.secondaryData == null)
		{
			return;
		}
		for (int j = 0; j < instance.secondaryData.Count; j++)
		{
			PlayerSecondaryData playerSecondaryData = instance.secondaryData[j];
			stream.WriteByte(42);
			BufferStream.RangeHandle range2 = stream.GetRange(5);
			int position2 = stream.Position;
			PlayerSecondaryData.SerializeDelta(stream, playerSecondaryData, playerSecondaryData);
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
	}

	public static void Serialize(BufferStream stream, TransferRequest instance)
	{
		if (instance.method != null)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.method);
		}
		if (instance.from != null)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.from);
		}
		if (instance.to != null)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteString(stream, instance.to);
		}
		if (instance.entities != null)
		{
			for (int i = 0; i < instance.entities.Count; i++)
			{
				Entity instance2 = instance.entities[i];
				stream.WriteByte(34);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				Entity.Serialize(stream, instance2);
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
		if (instance.secondaryData == null)
		{
			return;
		}
		for (int j = 0; j < instance.secondaryData.Count; j++)
		{
			PlayerSecondaryData instance3 = instance.secondaryData[j];
			stream.WriteByte(42);
			BufferStream.RangeHandle range2 = stream.GetRange(5);
			int position2 = stream.Position;
			PlayerSecondaryData.Serialize(stream, instance3);
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
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (entities != null)
		{
			for (int i = 0; i < entities.Count; i++)
			{
				entities[i]?.InspectUids(action);
			}
		}
		if (secondaryData != null)
		{
			for (int j = 0; j < secondaryData.Count; j++)
			{
				secondaryData[j]?.InspectUids(action);
			}
		}
	}
}
