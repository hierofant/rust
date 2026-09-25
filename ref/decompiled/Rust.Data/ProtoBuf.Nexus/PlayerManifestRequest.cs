using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf.Nexus;

public class PlayerManifestRequest : IDisposable, Pool.IPooled, IProto<PlayerManifestRequest>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<ulong> userIds;

	public static void ResetToPool(PlayerManifestRequest instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.userIds != null)
			{
				List<ulong> obj = instance.userIds;
				Pool.FreeUnmanaged(ref obj);
				instance.userIds = obj;
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
			throw new Exception("Trying to dispose PlayerManifestRequest with ShouldPool set to false!");
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

	public void CopyTo(PlayerManifestRequest instance)
	{
		if (userIds != null)
		{
			instance.userIds = Pool.Get<List<ulong>>();
			for (int i = 0; i < userIds.Count; i++)
			{
				ulong item = userIds[i];
				instance.userIds.Add(item);
			}
		}
		else
		{
			instance.userIds = null;
		}
	}

	public PlayerManifestRequest Copy()
	{
		PlayerManifestRequest playerManifestRequest = Pool.Get<PlayerManifestRequest>();
		CopyTo(playerManifestRequest);
		return playerManifestRequest;
	}

	public static PlayerManifestRequest Deserialize(BufferStream stream)
	{
		PlayerManifestRequest playerManifestRequest = Pool.Get<PlayerManifestRequest>();
		Deserialize(stream, playerManifestRequest, isDelta: false);
		return playerManifestRequest;
	}

	public static PlayerManifestRequest DeserializeLengthDelimited(BufferStream stream)
	{
		PlayerManifestRequest playerManifestRequest = Pool.Get<PlayerManifestRequest>();
		DeserializeLengthDelimited(stream, playerManifestRequest, isDelta: false);
		return playerManifestRequest;
	}

	public static PlayerManifestRequest DeserializeLength(BufferStream stream, int length)
	{
		PlayerManifestRequest playerManifestRequest = Pool.Get<PlayerManifestRequest>();
		DeserializeLength(stream, length, playerManifestRequest, isDelta: false);
		return playerManifestRequest;
	}

	public static PlayerManifestRequest Deserialize(byte[] buffer)
	{
		PlayerManifestRequest playerManifestRequest = Pool.Get<PlayerManifestRequest>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, playerManifestRequest, isDelta: false);
		return playerManifestRequest;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, PlayerManifestRequest previous)
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

	public static PlayerManifestRequest Deserialize(BufferStream stream, PlayerManifestRequest instance, bool isDelta)
	{
		if (!isDelta && instance.userIds == null)
		{
			instance.userIds = Pool.Get<List<ulong>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.PlayerManifestRequest");
			if (num == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Nexus.PlayerManifestRequest");
				stream.ConsumeRepeatedElement();
				instance.userIds.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Nexus.PlayerManifestRequest");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.PlayerManifestRequest");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static PlayerManifestRequest DeserializeLengthDelimited(BufferStream stream, PlayerManifestRequest instance, bool isDelta)
	{
		if (!isDelta && instance.userIds == null)
		{
			instance.userIds = Pool.Get<List<ulong>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.PlayerManifestRequest");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Nexus.PlayerManifestRequest");
				stream.ConsumeRepeatedElement();
				instance.userIds.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Nexus.PlayerManifestRequest");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.PlayerManifestRequest");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static PlayerManifestRequest DeserializeLength(BufferStream stream, int length, PlayerManifestRequest instance, bool isDelta)
	{
		if (!isDelta && instance.userIds == null)
		{
			instance.userIds = Pool.Get<List<ulong>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.PlayerManifestRequest");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Nexus.PlayerManifestRequest");
				stream.ConsumeRepeatedElement();
				instance.userIds.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Nexus.PlayerManifestRequest");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.PlayerManifestRequest");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, PlayerManifestRequest instance, PlayerManifestRequest previous)
	{
		if (instance.userIds != null)
		{
			for (int i = 0; i < instance.userIds.Count; i++)
			{
				ulong val = instance.userIds[i];
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, val);
			}
		}
	}

	public static void Serialize(BufferStream stream, PlayerManifestRequest instance)
	{
		if (instance.userIds != null)
		{
			for (int i = 0; i < instance.userIds.Count; i++)
			{
				ulong val = instance.userIds[i];
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, val);
			}
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
	}
}
