using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ChickenCoopStatusUpdate : IDisposable, Pool.IPooled, IProto<ChickenCoopStatusUpdate>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<FarmableAnimalStatus> animals;

	public static void ResetToPool(ChickenCoopStatusUpdate instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.animals != null)
		{
			for (int i = 0; i < instance.animals.Count; i++)
			{
				if (instance.animals[i] != null)
				{
					instance.animals[i].ResetToPool();
					instance.animals[i] = null;
				}
			}
			List<FarmableAnimalStatus> obj = instance.animals;
			Pool.Free(ref obj, freeElements: false);
			instance.animals = obj;
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
			throw new Exception("Trying to dispose ChickenCoopStatusUpdate with ShouldPool set to false!");
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

	public void CopyTo(ChickenCoopStatusUpdate instance)
	{
		if (animals != null)
		{
			instance.animals = Pool.Get<List<FarmableAnimalStatus>>();
			for (int i = 0; i < animals.Count; i++)
			{
				FarmableAnimalStatus item = animals[i].Copy();
				instance.animals.Add(item);
			}
		}
		else
		{
			instance.animals = null;
		}
	}

	public ChickenCoopStatusUpdate Copy()
	{
		ChickenCoopStatusUpdate chickenCoopStatusUpdate = Pool.Get<ChickenCoopStatusUpdate>();
		CopyTo(chickenCoopStatusUpdate);
		return chickenCoopStatusUpdate;
	}

	public static ChickenCoopStatusUpdate Deserialize(BufferStream stream)
	{
		ChickenCoopStatusUpdate chickenCoopStatusUpdate = Pool.Get<ChickenCoopStatusUpdate>();
		Deserialize(stream, chickenCoopStatusUpdate, isDelta: false);
		return chickenCoopStatusUpdate;
	}

	public static ChickenCoopStatusUpdate DeserializeLengthDelimited(BufferStream stream)
	{
		ChickenCoopStatusUpdate chickenCoopStatusUpdate = Pool.Get<ChickenCoopStatusUpdate>();
		DeserializeLengthDelimited(stream, chickenCoopStatusUpdate, isDelta: false);
		return chickenCoopStatusUpdate;
	}

	public static ChickenCoopStatusUpdate DeserializeLength(BufferStream stream, int length)
	{
		ChickenCoopStatusUpdate chickenCoopStatusUpdate = Pool.Get<ChickenCoopStatusUpdate>();
		DeserializeLength(stream, length, chickenCoopStatusUpdate, isDelta: false);
		return chickenCoopStatusUpdate;
	}

	public static ChickenCoopStatusUpdate Deserialize(byte[] buffer)
	{
		ChickenCoopStatusUpdate chickenCoopStatusUpdate = Pool.Get<ChickenCoopStatusUpdate>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, chickenCoopStatusUpdate, isDelta: false);
		return chickenCoopStatusUpdate;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ChickenCoopStatusUpdate previous)
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

	public static ChickenCoopStatusUpdate Deserialize(BufferStream stream, ChickenCoopStatusUpdate instance, bool isDelta)
	{
		if (!isDelta && instance.animals == null)
		{
			instance.animals = Pool.Get<List<FarmableAnimalStatus>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ChickenCoopStatusUpdate");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ChickenCoopStatusUpdate");
				stream.ConsumeRepeatedElement();
				instance.animals.Add(FarmableAnimalStatus.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ChickenCoopStatusUpdate");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ChickenCoopStatusUpdate");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ChickenCoopStatusUpdate DeserializeLengthDelimited(BufferStream stream, ChickenCoopStatusUpdate instance, bool isDelta)
	{
		if (!isDelta && instance.animals == null)
		{
			instance.animals = Pool.Get<List<FarmableAnimalStatus>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ChickenCoopStatusUpdate");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ChickenCoopStatusUpdate");
				stream.ConsumeRepeatedElement();
				instance.animals.Add(FarmableAnimalStatus.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ChickenCoopStatusUpdate");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ChickenCoopStatusUpdate");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ChickenCoopStatusUpdate DeserializeLength(BufferStream stream, int length, ChickenCoopStatusUpdate instance, bool isDelta)
	{
		if (!isDelta && instance.animals == null)
		{
			instance.animals = Pool.Get<List<FarmableAnimalStatus>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ChickenCoopStatusUpdate");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ChickenCoopStatusUpdate");
				stream.ConsumeRepeatedElement();
				instance.animals.Add(FarmableAnimalStatus.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ChickenCoopStatusUpdate");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ChickenCoopStatusUpdate");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ChickenCoopStatusUpdate instance, ChickenCoopStatusUpdate previous)
	{
		if (instance.animals == null)
		{
			return;
		}
		for (int i = 0; i < instance.animals.Count; i++)
		{
			FarmableAnimalStatus farmableAnimalStatus = instance.animals[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			FarmableAnimalStatus.SerializeDelta(stream, farmableAnimalStatus, farmableAnimalStatus);
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

	public static void Serialize(BufferStream stream, ChickenCoopStatusUpdate instance)
	{
		if (instance.animals == null)
		{
			return;
		}
		for (int i = 0; i < instance.animals.Count; i++)
		{
			FarmableAnimalStatus instance2 = instance.animals[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			FarmableAnimalStatus.Serialize(stream, instance2);
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
		if (animals != null)
		{
			for (int i = 0; i < animals.Count; i++)
			{
				animals[i]?.InspectUids(action);
			}
		}
	}
}
