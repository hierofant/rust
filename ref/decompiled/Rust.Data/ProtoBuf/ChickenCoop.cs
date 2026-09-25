using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ChickenCoop : IDisposable, Pool.IPooled, IProto<ChickenCoop>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<ChickenStatus> chickens;

	public static void ResetToPool(ChickenCoop instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.chickens != null)
		{
			for (int i = 0; i < instance.chickens.Count; i++)
			{
				if (instance.chickens[i] != null)
				{
					instance.chickens[i].ResetToPool();
					instance.chickens[i] = null;
				}
			}
			List<ChickenStatus> obj = instance.chickens;
			Pool.Free(ref obj, freeElements: false);
			instance.chickens = obj;
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
			throw new Exception("Trying to dispose ChickenCoop with ShouldPool set to false!");
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

	public void CopyTo(ChickenCoop instance)
	{
		if (chickens != null)
		{
			instance.chickens = Pool.Get<List<ChickenStatus>>();
			for (int i = 0; i < chickens.Count; i++)
			{
				ChickenStatus item = chickens[i].Copy();
				instance.chickens.Add(item);
			}
		}
		else
		{
			instance.chickens = null;
		}
	}

	public ChickenCoop Copy()
	{
		ChickenCoop chickenCoop = Pool.Get<ChickenCoop>();
		CopyTo(chickenCoop);
		return chickenCoop;
	}

	public static ChickenCoop Deserialize(BufferStream stream)
	{
		ChickenCoop chickenCoop = Pool.Get<ChickenCoop>();
		Deserialize(stream, chickenCoop, isDelta: false);
		return chickenCoop;
	}

	public static ChickenCoop DeserializeLengthDelimited(BufferStream stream)
	{
		ChickenCoop chickenCoop = Pool.Get<ChickenCoop>();
		DeserializeLengthDelimited(stream, chickenCoop, isDelta: false);
		return chickenCoop;
	}

	public static ChickenCoop DeserializeLength(BufferStream stream, int length)
	{
		ChickenCoop chickenCoop = Pool.Get<ChickenCoop>();
		DeserializeLength(stream, length, chickenCoop, isDelta: false);
		return chickenCoop;
	}

	public static ChickenCoop Deserialize(byte[] buffer)
	{
		ChickenCoop chickenCoop = Pool.Get<ChickenCoop>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, chickenCoop, isDelta: false);
		return chickenCoop;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ChickenCoop previous)
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

	public static ChickenCoop Deserialize(BufferStream stream, ChickenCoop instance, bool isDelta)
	{
		if (!isDelta && instance.chickens == null)
		{
			instance.chickens = Pool.Get<List<ChickenStatus>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ChickenCoop");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ChickenCoop");
				stream.ConsumeRepeatedElement();
				instance.chickens.Add(ChickenStatus.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ChickenCoop");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ChickenCoop");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ChickenCoop DeserializeLengthDelimited(BufferStream stream, ChickenCoop instance, bool isDelta)
	{
		if (!isDelta && instance.chickens == null)
		{
			instance.chickens = Pool.Get<List<ChickenStatus>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ChickenCoop");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ChickenCoop");
				stream.ConsumeRepeatedElement();
				instance.chickens.Add(ChickenStatus.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ChickenCoop");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ChickenCoop");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ChickenCoop DeserializeLength(BufferStream stream, int length, ChickenCoop instance, bool isDelta)
	{
		if (!isDelta && instance.chickens == null)
		{
			instance.chickens = Pool.Get<List<ChickenStatus>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ChickenCoop");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ChickenCoop");
				stream.ConsumeRepeatedElement();
				instance.chickens.Add(ChickenStatus.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ChickenCoop");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ChickenCoop");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ChickenCoop instance, ChickenCoop previous)
	{
		if (instance.chickens == null)
		{
			return;
		}
		for (int i = 0; i < instance.chickens.Count; i++)
		{
			ChickenStatus chickenStatus = instance.chickens[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			ChickenStatus.SerializeDelta(stream, chickenStatus, chickenStatus);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field chickens (ProtoBuf.ChickenStatus)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public static void Serialize(BufferStream stream, ChickenCoop instance)
	{
		if (instance.chickens == null)
		{
			return;
		}
		for (int i = 0; i < instance.chickens.Count; i++)
		{
			ChickenStatus instance2 = instance.chickens[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			ChickenStatus.Serialize(stream, instance2);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field chickens (ProtoBuf.ChickenStatus)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (chickens != null)
		{
			for (int i = 0; i < chickens.Count; i++)
			{
				chickens[i]?.InspectUids(action);
			}
		}
	}
}
