using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class MissionMapMarker : IDisposable, Pool.IPooled, IProto<MissionMapMarker>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public NetworkableId missionProviderNetId;

	[NonSerialized]
	public List<uint> missionIds;

	[NonSerialized]
	public string nameToken;

	public static void ResetToPool(MissionMapMarker instance)
	{
		if (instance.ShouldPool)
		{
			instance.missionProviderNetId = default(NetworkableId);
			if (instance.missionIds != null)
			{
				List<uint> obj = instance.missionIds;
				Pool.FreeUnmanaged(ref obj);
				instance.missionIds = obj;
			}
			instance.nameToken = string.Empty;
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
			throw new Exception("Trying to dispose MissionMapMarker with ShouldPool set to false!");
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

	public void CopyTo(MissionMapMarker instance)
	{
		instance.missionProviderNetId = missionProviderNetId;
		if (missionIds != null)
		{
			instance.missionIds = Pool.Get<List<uint>>();
			for (int i = 0; i < missionIds.Count; i++)
			{
				uint item = missionIds[i];
				instance.missionIds.Add(item);
			}
		}
		else
		{
			instance.missionIds = null;
		}
		instance.nameToken = nameToken;
	}

	public MissionMapMarker Copy()
	{
		MissionMapMarker missionMapMarker = Pool.Get<MissionMapMarker>();
		CopyTo(missionMapMarker);
		return missionMapMarker;
	}

	public static MissionMapMarker Deserialize(BufferStream stream)
	{
		MissionMapMarker missionMapMarker = Pool.Get<MissionMapMarker>();
		Deserialize(stream, missionMapMarker, isDelta: false);
		return missionMapMarker;
	}

	public static MissionMapMarker DeserializeLengthDelimited(BufferStream stream)
	{
		MissionMapMarker missionMapMarker = Pool.Get<MissionMapMarker>();
		DeserializeLengthDelimited(stream, missionMapMarker, isDelta: false);
		return missionMapMarker;
	}

	public static MissionMapMarker DeserializeLength(BufferStream stream, int length)
	{
		MissionMapMarker missionMapMarker = Pool.Get<MissionMapMarker>();
		DeserializeLength(stream, length, missionMapMarker, isDelta: false);
		return missionMapMarker;
	}

	public static MissionMapMarker Deserialize(byte[] buffer)
	{
		MissionMapMarker missionMapMarker = Pool.Get<MissionMapMarker>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, missionMapMarker, isDelta: false);
		return missionMapMarker;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, MissionMapMarker previous)
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

	public static MissionMapMarker Deserialize(BufferStream stream, MissionMapMarker instance, bool isDelta)
	{
		if (!isDelta && instance.missionIds == null)
		{
			instance.missionIds = Pool.Get<List<uint>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.MissionMapMarker");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MissionMapMarker");
				instance.missionProviderNetId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.MissionMapMarker");
				stream.ConsumeRepeatedElement();
				instance.missionIds.Add(ProtocolParser.ReadUInt32(stream));
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MissionMapMarker");
				instance.nameToken = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MissionMapMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.MissionMapMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MissionMapMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MissionMapMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static MissionMapMarker DeserializeLengthDelimited(BufferStream stream, MissionMapMarker instance, bool isDelta)
	{
		if (!isDelta && instance.missionIds == null)
		{
			instance.missionIds = Pool.Get<List<uint>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.MissionMapMarker");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MissionMapMarker");
				instance.missionProviderNetId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.MissionMapMarker");
				stream.ConsumeRepeatedElement();
				instance.missionIds.Add(ProtocolParser.ReadUInt32(stream));
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MissionMapMarker");
				instance.nameToken = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MissionMapMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.MissionMapMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MissionMapMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MissionMapMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static MissionMapMarker DeserializeLength(BufferStream stream, int length, MissionMapMarker instance, bool isDelta)
	{
		if (!isDelta && instance.missionIds == null)
		{
			instance.missionIds = Pool.Get<List<uint>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.MissionMapMarker");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MissionMapMarker");
				instance.missionProviderNetId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.MissionMapMarker");
				stream.ConsumeRepeatedElement();
				instance.missionIds.Add(ProtocolParser.ReadUInt32(stream));
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MissionMapMarker");
				instance.nameToken = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MissionMapMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.MissionMapMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MissionMapMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MissionMapMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, MissionMapMarker instance, MissionMapMarker previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, instance.missionProviderNetId.Value);
		if (instance.missionIds != null)
		{
			for (int i = 0; i < instance.missionIds.Count; i++)
			{
				uint val = instance.missionIds[i];
				stream.WriteByte(16);
				ProtocolParser.WriteUInt32(stream, val);
			}
		}
		if (instance.nameToken != previous.nameToken)
		{
			if (instance.nameToken == null)
			{
				throw new ArgumentNullException("nameToken", "Required by proto specification.");
			}
			stream.WriteByte(26);
			ProtocolParser.WriteString(stream, instance.nameToken);
		}
	}

	public static void Serialize(BufferStream stream, MissionMapMarker instance)
	{
		if (instance.missionProviderNetId != default(NetworkableId))
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.missionProviderNetId.Value);
		}
		if (instance.missionIds != null)
		{
			for (int i = 0; i < instance.missionIds.Count; i++)
			{
				uint val = instance.missionIds[i];
				stream.WriteByte(16);
				ProtocolParser.WriteUInt32(stream, val);
			}
		}
		if (instance.nameToken == null)
		{
			throw new ArgumentNullException("nameToken", "Required by proto specification.");
		}
		stream.WriteByte(26);
		ProtocolParser.WriteString(stream, instance.nameToken);
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref missionProviderNetId.Value);
	}
}
