using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf.Nexus;

public class SpawnOptionsResponse : IDisposable, Pool.IPooled, IProto<SpawnOptionsResponse>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<RespawnInformation.SpawnOptions> spawnOptions;

	public static void ResetToPool(SpawnOptionsResponse instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.spawnOptions != null)
		{
			for (int i = 0; i < instance.spawnOptions.Count; i++)
			{
				if (instance.spawnOptions[i] != null)
				{
					instance.spawnOptions[i].ResetToPool();
					instance.spawnOptions[i] = null;
				}
			}
			List<RespawnInformation.SpawnOptions> obj = instance.spawnOptions;
			Pool.Free(ref obj, freeElements: false);
			instance.spawnOptions = obj;
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
			throw new Exception("Trying to dispose SpawnOptionsResponse with ShouldPool set to false!");
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

	public void CopyTo(SpawnOptionsResponse instance)
	{
		if (spawnOptions != null)
		{
			instance.spawnOptions = Pool.Get<List<RespawnInformation.SpawnOptions>>();
			for (int i = 0; i < spawnOptions.Count; i++)
			{
				RespawnInformation.SpawnOptions item = spawnOptions[i].Copy();
				instance.spawnOptions.Add(item);
			}
		}
		else
		{
			instance.spawnOptions = null;
		}
	}

	public SpawnOptionsResponse Copy()
	{
		SpawnOptionsResponse spawnOptionsResponse = Pool.Get<SpawnOptionsResponse>();
		CopyTo(spawnOptionsResponse);
		return spawnOptionsResponse;
	}

	public static SpawnOptionsResponse Deserialize(BufferStream stream)
	{
		SpawnOptionsResponse spawnOptionsResponse = Pool.Get<SpawnOptionsResponse>();
		Deserialize(stream, spawnOptionsResponse, isDelta: false);
		return spawnOptionsResponse;
	}

	public static SpawnOptionsResponse DeserializeLengthDelimited(BufferStream stream)
	{
		SpawnOptionsResponse spawnOptionsResponse = Pool.Get<SpawnOptionsResponse>();
		DeserializeLengthDelimited(stream, spawnOptionsResponse, isDelta: false);
		return spawnOptionsResponse;
	}

	public static SpawnOptionsResponse DeserializeLength(BufferStream stream, int length)
	{
		SpawnOptionsResponse spawnOptionsResponse = Pool.Get<SpawnOptionsResponse>();
		DeserializeLength(stream, length, spawnOptionsResponse, isDelta: false);
		return spawnOptionsResponse;
	}

	public static SpawnOptionsResponse Deserialize(byte[] buffer)
	{
		SpawnOptionsResponse spawnOptionsResponse = Pool.Get<SpawnOptionsResponse>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, spawnOptionsResponse, isDelta: false);
		return spawnOptionsResponse;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, SpawnOptionsResponse previous)
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

	public static SpawnOptionsResponse Deserialize(BufferStream stream, SpawnOptionsResponse instance, bool isDelta)
	{
		if (!isDelta && instance.spawnOptions == null)
		{
			instance.spawnOptions = Pool.Get<List<RespawnInformation.SpawnOptions>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.SpawnOptionsResponse");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Nexus.SpawnOptionsResponse");
				stream.ConsumeRepeatedElement();
				instance.spawnOptions.Add(RespawnInformation.SpawnOptions.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Nexus.SpawnOptionsResponse");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.SpawnOptionsResponse");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static SpawnOptionsResponse DeserializeLengthDelimited(BufferStream stream, SpawnOptionsResponse instance, bool isDelta)
	{
		if (!isDelta && instance.spawnOptions == null)
		{
			instance.spawnOptions = Pool.Get<List<RespawnInformation.SpawnOptions>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.SpawnOptionsResponse");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Nexus.SpawnOptionsResponse");
				stream.ConsumeRepeatedElement();
				instance.spawnOptions.Add(RespawnInformation.SpawnOptions.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Nexus.SpawnOptionsResponse");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.SpawnOptionsResponse");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static SpawnOptionsResponse DeserializeLength(BufferStream stream, int length, SpawnOptionsResponse instance, bool isDelta)
	{
		if (!isDelta && instance.spawnOptions == null)
		{
			instance.spawnOptions = Pool.Get<List<RespawnInformation.SpawnOptions>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.SpawnOptionsResponse");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Nexus.SpawnOptionsResponse");
				stream.ConsumeRepeatedElement();
				instance.spawnOptions.Add(RespawnInformation.SpawnOptions.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Nexus.SpawnOptionsResponse");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.SpawnOptionsResponse");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, SpawnOptionsResponse instance, SpawnOptionsResponse previous)
	{
		if (instance.spawnOptions == null)
		{
			return;
		}
		for (int i = 0; i < instance.spawnOptions.Count; i++)
		{
			RespawnInformation.SpawnOptions spawnOptions = instance.spawnOptions[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			RespawnInformation.SpawnOptions.SerializeDelta(stream, spawnOptions, spawnOptions);
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

	public static void Serialize(BufferStream stream, SpawnOptionsResponse instance)
	{
		if (instance.spawnOptions == null)
		{
			return;
		}
		for (int i = 0; i < instance.spawnOptions.Count; i++)
		{
			RespawnInformation.SpawnOptions instance2 = instance.spawnOptions[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			RespawnInformation.SpawnOptions.Serialize(stream, instance2);
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
		if (spawnOptions != null)
		{
			for (int i = 0; i < spawnOptions.Count; i++)
			{
				spawnOptions[i]?.InspectUids(action);
			}
		}
	}
}
