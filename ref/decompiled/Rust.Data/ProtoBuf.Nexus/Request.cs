using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf.Nexus;

public class Request : IDisposable, Pool.IPooled, IProto<Request>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public bool isFireAndForget;

	[NonSerialized]
	public TransferRequest transfer;

	[NonSerialized]
	public PingRequest ping;

	[NonSerialized]
	public SpawnOptionsRequest spawnOptions;

	[NonSerialized]
	public SleepingBagRespawnRequest respawnAtBag;

	[NonSerialized]
	public SleepingBagDestroyRequest destroyBag;

	[NonSerialized]
	public FerryStatusRequest ferryStatus;

	[NonSerialized]
	public FerryRetireRequest ferryRetire;

	[NonSerialized]
	public FerryUpdateScheduleRequest ferryUpdateSchedule;

	[NonSerialized]
	public ClanChatBatchRequest clanChatBatch;

	[NonSerialized]
	public PlayerManifestRequest playerManifest;

	public static void ResetToPool(Request instance)
	{
		if (instance.ShouldPool)
		{
			instance.isFireAndForget = false;
			if (instance.transfer != null)
			{
				instance.transfer.ResetToPool();
				instance.transfer = null;
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
			if (instance.respawnAtBag != null)
			{
				instance.respawnAtBag.ResetToPool();
				instance.respawnAtBag = null;
			}
			if (instance.destroyBag != null)
			{
				instance.destroyBag.ResetToPool();
				instance.destroyBag = null;
			}
			if (instance.ferryStatus != null)
			{
				instance.ferryStatus.ResetToPool();
				instance.ferryStatus = null;
			}
			if (instance.ferryRetire != null)
			{
				instance.ferryRetire.ResetToPool();
				instance.ferryRetire = null;
			}
			if (instance.ferryUpdateSchedule != null)
			{
				instance.ferryUpdateSchedule.ResetToPool();
				instance.ferryUpdateSchedule = null;
			}
			if (instance.clanChatBatch != null)
			{
				instance.clanChatBatch.ResetToPool();
				instance.clanChatBatch = null;
			}
			if (instance.playerManifest != null)
			{
				instance.playerManifest.ResetToPool();
				instance.playerManifest = null;
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
			throw new Exception("Trying to dispose Request with ShouldPool set to false!");
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

	public void CopyTo(Request instance)
	{
		instance.isFireAndForget = isFireAndForget;
		if (transfer != null)
		{
			if (instance.transfer == null)
			{
				instance.transfer = transfer.Copy();
			}
			else
			{
				transfer.CopyTo(instance.transfer);
			}
		}
		else
		{
			instance.transfer = null;
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
		if (respawnAtBag != null)
		{
			if (instance.respawnAtBag == null)
			{
				instance.respawnAtBag = respawnAtBag.Copy();
			}
			else
			{
				respawnAtBag.CopyTo(instance.respawnAtBag);
			}
		}
		else
		{
			instance.respawnAtBag = null;
		}
		if (destroyBag != null)
		{
			if (instance.destroyBag == null)
			{
				instance.destroyBag = destroyBag.Copy();
			}
			else
			{
				destroyBag.CopyTo(instance.destroyBag);
			}
		}
		else
		{
			instance.destroyBag = null;
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
		if (ferryRetire != null)
		{
			if (instance.ferryRetire == null)
			{
				instance.ferryRetire = ferryRetire.Copy();
			}
			else
			{
				ferryRetire.CopyTo(instance.ferryRetire);
			}
		}
		else
		{
			instance.ferryRetire = null;
		}
		if (ferryUpdateSchedule != null)
		{
			if (instance.ferryUpdateSchedule == null)
			{
				instance.ferryUpdateSchedule = ferryUpdateSchedule.Copy();
			}
			else
			{
				ferryUpdateSchedule.CopyTo(instance.ferryUpdateSchedule);
			}
		}
		else
		{
			instance.ferryUpdateSchedule = null;
		}
		if (clanChatBatch != null)
		{
			if (instance.clanChatBatch == null)
			{
				instance.clanChatBatch = clanChatBatch.Copy();
			}
			else
			{
				clanChatBatch.CopyTo(instance.clanChatBatch);
			}
		}
		else
		{
			instance.clanChatBatch = null;
		}
		if (playerManifest != null)
		{
			if (instance.playerManifest == null)
			{
				instance.playerManifest = playerManifest.Copy();
			}
			else
			{
				playerManifest.CopyTo(instance.playerManifest);
			}
		}
		else
		{
			instance.playerManifest = null;
		}
	}

	public Request Copy()
	{
		Request request = Pool.Get<Request>();
		CopyTo(request);
		return request;
	}

	public static Request Deserialize(BufferStream stream)
	{
		Request request = Pool.Get<Request>();
		Deserialize(stream, request, isDelta: false);
		return request;
	}

	public static Request DeserializeLengthDelimited(BufferStream stream)
	{
		Request request = Pool.Get<Request>();
		DeserializeLengthDelimited(stream, request, isDelta: false);
		return request;
	}

	public static Request DeserializeLength(BufferStream stream, int length)
	{
		Request request = Pool.Get<Request>();
		DeserializeLength(stream, length, request, isDelta: false);
		return request;
	}

	public static Request Deserialize(byte[] buffer)
	{
		Request request = Pool.Get<Request>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, request, isDelta: false);
		return request;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, Request previous)
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

	public static Request Deserialize(BufferStream stream, Request instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.Request");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				instance.isFireAndForget = ProtocolParser.ReadBool(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				if (instance.transfer == null)
				{
					instance.transfer = TransferRequest.DeserializeLengthDelimited(stream);
				}
				else
				{
					TransferRequest.DeserializeLengthDelimited(stream, instance.transfer, isDelta);
				}
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				if (instance.ping == null)
				{
					instance.ping = PingRequest.DeserializeLengthDelimited(stream);
				}
				else
				{
					PingRequest.DeserializeLengthDelimited(stream, instance.ping, isDelta);
				}
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				if (instance.spawnOptions == null)
				{
					instance.spawnOptions = SpawnOptionsRequest.DeserializeLengthDelimited(stream);
				}
				else
				{
					SpawnOptionsRequest.DeserializeLengthDelimited(stream, instance.spawnOptions, isDelta);
				}
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				if (instance.respawnAtBag == null)
				{
					instance.respawnAtBag = SleepingBagRespawnRequest.DeserializeLengthDelimited(stream);
				}
				else
				{
					SleepingBagRespawnRequest.DeserializeLengthDelimited(stream, instance.respawnAtBag, isDelta);
				}
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				if (instance.destroyBag == null)
				{
					instance.destroyBag = SleepingBagDestroyRequest.DeserializeLengthDelimited(stream);
				}
				else
				{
					SleepingBagDestroyRequest.DeserializeLengthDelimited(stream, instance.destroyBag, isDelta);
				}
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				if (instance.ferryStatus == null)
				{
					instance.ferryStatus = FerryStatusRequest.DeserializeLengthDelimited(stream);
				}
				else
				{
					FerryStatusRequest.DeserializeLengthDelimited(stream, instance.ferryStatus, isDelta);
				}
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				if (instance.ferryRetire == null)
				{
					instance.ferryRetire = FerryRetireRequest.DeserializeLengthDelimited(stream);
				}
				else
				{
					FerryRetireRequest.DeserializeLengthDelimited(stream, instance.ferryRetire, isDelta);
				}
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				if (instance.ferryUpdateSchedule == null)
				{
					instance.ferryUpdateSchedule = FerryUpdateScheduleRequest.DeserializeLengthDelimited(stream);
				}
				else
				{
					FerryUpdateScheduleRequest.DeserializeLengthDelimited(stream, instance.ferryUpdateSchedule, isDelta);
				}
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				if (instance.clanChatBatch == null)
				{
					instance.clanChatBatch = ClanChatBatchRequest.DeserializeLengthDelimited(stream);
				}
				else
				{
					ClanChatBatchRequest.DeserializeLengthDelimited(stream, instance.clanChatBatch, isDelta);
				}
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				if (instance.playerManifest == null)
				{
					instance.playerManifest = PlayerManifestRequest.DeserializeLengthDelimited(stream);
				}
				else
				{
					PlayerManifestRequest.DeserializeLengthDelimited(stream, instance.playerManifest, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Request DeserializeLengthDelimited(BufferStream stream, Request instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.Request");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				instance.isFireAndForget = ProtocolParser.ReadBool(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				if (instance.transfer == null)
				{
					instance.transfer = TransferRequest.DeserializeLengthDelimited(stream);
				}
				else
				{
					TransferRequest.DeserializeLengthDelimited(stream, instance.transfer, isDelta);
				}
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				if (instance.ping == null)
				{
					instance.ping = PingRequest.DeserializeLengthDelimited(stream);
				}
				else
				{
					PingRequest.DeserializeLengthDelimited(stream, instance.ping, isDelta);
				}
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				if (instance.spawnOptions == null)
				{
					instance.spawnOptions = SpawnOptionsRequest.DeserializeLengthDelimited(stream);
				}
				else
				{
					SpawnOptionsRequest.DeserializeLengthDelimited(stream, instance.spawnOptions, isDelta);
				}
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				if (instance.respawnAtBag == null)
				{
					instance.respawnAtBag = SleepingBagRespawnRequest.DeserializeLengthDelimited(stream);
				}
				else
				{
					SleepingBagRespawnRequest.DeserializeLengthDelimited(stream, instance.respawnAtBag, isDelta);
				}
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				if (instance.destroyBag == null)
				{
					instance.destroyBag = SleepingBagDestroyRequest.DeserializeLengthDelimited(stream);
				}
				else
				{
					SleepingBagDestroyRequest.DeserializeLengthDelimited(stream, instance.destroyBag, isDelta);
				}
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				if (instance.ferryStatus == null)
				{
					instance.ferryStatus = FerryStatusRequest.DeserializeLengthDelimited(stream);
				}
				else
				{
					FerryStatusRequest.DeserializeLengthDelimited(stream, instance.ferryStatus, isDelta);
				}
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				if (instance.ferryRetire == null)
				{
					instance.ferryRetire = FerryRetireRequest.DeserializeLengthDelimited(stream);
				}
				else
				{
					FerryRetireRequest.DeserializeLengthDelimited(stream, instance.ferryRetire, isDelta);
				}
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				if (instance.ferryUpdateSchedule == null)
				{
					instance.ferryUpdateSchedule = FerryUpdateScheduleRequest.DeserializeLengthDelimited(stream);
				}
				else
				{
					FerryUpdateScheduleRequest.DeserializeLengthDelimited(stream, instance.ferryUpdateSchedule, isDelta);
				}
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				if (instance.clanChatBatch == null)
				{
					instance.clanChatBatch = ClanChatBatchRequest.DeserializeLengthDelimited(stream);
				}
				else
				{
					ClanChatBatchRequest.DeserializeLengthDelimited(stream, instance.clanChatBatch, isDelta);
				}
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				if (instance.playerManifest == null)
				{
					instance.playerManifest = PlayerManifestRequest.DeserializeLengthDelimited(stream);
				}
				else
				{
					PlayerManifestRequest.DeserializeLengthDelimited(stream, instance.playerManifest, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Request DeserializeLength(BufferStream stream, int length, Request instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.Request");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				instance.isFireAndForget = ProtocolParser.ReadBool(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				if (instance.transfer == null)
				{
					instance.transfer = TransferRequest.DeserializeLengthDelimited(stream);
				}
				else
				{
					TransferRequest.DeserializeLengthDelimited(stream, instance.transfer, isDelta);
				}
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				if (instance.ping == null)
				{
					instance.ping = PingRequest.DeserializeLengthDelimited(stream);
				}
				else
				{
					PingRequest.DeserializeLengthDelimited(stream, instance.ping, isDelta);
				}
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				if (instance.spawnOptions == null)
				{
					instance.spawnOptions = SpawnOptionsRequest.DeserializeLengthDelimited(stream);
				}
				else
				{
					SpawnOptionsRequest.DeserializeLengthDelimited(stream, instance.spawnOptions, isDelta);
				}
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				if (instance.respawnAtBag == null)
				{
					instance.respawnAtBag = SleepingBagRespawnRequest.DeserializeLengthDelimited(stream);
				}
				else
				{
					SleepingBagRespawnRequest.DeserializeLengthDelimited(stream, instance.respawnAtBag, isDelta);
				}
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				if (instance.destroyBag == null)
				{
					instance.destroyBag = SleepingBagDestroyRequest.DeserializeLengthDelimited(stream);
				}
				else
				{
					SleepingBagDestroyRequest.DeserializeLengthDelimited(stream, instance.destroyBag, isDelta);
				}
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				if (instance.ferryStatus == null)
				{
					instance.ferryStatus = FerryStatusRequest.DeserializeLengthDelimited(stream);
				}
				else
				{
					FerryStatusRequest.DeserializeLengthDelimited(stream, instance.ferryStatus, isDelta);
				}
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				if (instance.ferryRetire == null)
				{
					instance.ferryRetire = FerryRetireRequest.DeserializeLengthDelimited(stream);
				}
				else
				{
					FerryRetireRequest.DeserializeLengthDelimited(stream, instance.ferryRetire, isDelta);
				}
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				if (instance.ferryUpdateSchedule == null)
				{
					instance.ferryUpdateSchedule = FerryUpdateScheduleRequest.DeserializeLengthDelimited(stream);
				}
				else
				{
					FerryUpdateScheduleRequest.DeserializeLengthDelimited(stream, instance.ferryUpdateSchedule, isDelta);
				}
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				if (instance.clanChatBatch == null)
				{
					instance.clanChatBatch = ClanChatBatchRequest.DeserializeLengthDelimited(stream);
				}
				else
				{
					ClanChatBatchRequest.DeserializeLengthDelimited(stream, instance.clanChatBatch, isDelta);
				}
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				if (instance.playerManifest == null)
				{
					instance.playerManifest = PlayerManifestRequest.DeserializeLengthDelimited(stream);
				}
				else
				{
					PlayerManifestRequest.DeserializeLengthDelimited(stream, instance.playerManifest, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.Request");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, Request instance, Request previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteBool(stream, instance.isFireAndForget);
		if (instance.transfer != null)
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			TransferRequest.SerializeDelta(stream, instance.transfer, previous.transfer);
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
		if (instance.ping != null)
		{
			stream.WriteByte(26);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			PingRequest.SerializeDelta(stream, instance.ping, previous.ping);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field ping (ProtoBuf.Nexus.PingRequest)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.spawnOptions != null)
		{
			stream.WriteByte(34);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			SpawnOptionsRequest.SerializeDelta(stream, instance.spawnOptions, previous.spawnOptions);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field spawnOptions (ProtoBuf.Nexus.SpawnOptionsRequest)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
		if (instance.respawnAtBag != null)
		{
			stream.WriteByte(42);
			BufferStream.RangeHandle range4 = stream.GetRange(5);
			int position4 = stream.Position;
			SleepingBagRespawnRequest.SerializeDelta(stream, instance.respawnAtBag, previous.respawnAtBag);
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
		if (instance.destroyBag != null)
		{
			stream.WriteByte(50);
			BufferStream.RangeHandle range5 = stream.GetRange(1);
			int position5 = stream.Position;
			SleepingBagDestroyRequest.SerializeDelta(stream, instance.destroyBag, previous.destroyBag);
			int num5 = stream.Position - position5;
			if (num5 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field destroyBag (ProtoBuf.Nexus.SleepingBagDestroyRequest)");
			}
			Span<byte> span5 = range5.GetSpan();
			ProtocolParser.WriteUInt32((uint)num5, span5, 0);
		}
		if (instance.ferryStatus != null)
		{
			stream.WriteByte(58);
			BufferStream.RangeHandle range6 = stream.GetRange(1);
			int position6 = stream.Position;
			FerryStatusRequest.SerializeDelta(stream, instance.ferryStatus, previous.ferryStatus);
			int num6 = stream.Position - position6;
			if (num6 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field ferryStatus (ProtoBuf.Nexus.FerryStatusRequest)");
			}
			Span<byte> span6 = range6.GetSpan();
			ProtocolParser.WriteUInt32((uint)num6, span6, 0);
		}
		if (instance.ferryRetire != null)
		{
			stream.WriteByte(66);
			BufferStream.RangeHandle range7 = stream.GetRange(1);
			int position7 = stream.Position;
			FerryRetireRequest.SerializeDelta(stream, instance.ferryRetire, previous.ferryRetire);
			int num7 = stream.Position - position7;
			if (num7 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field ferryRetire (ProtoBuf.Nexus.FerryRetireRequest)");
			}
			Span<byte> span7 = range7.GetSpan();
			ProtocolParser.WriteUInt32((uint)num7, span7, 0);
		}
		if (instance.ferryUpdateSchedule != null)
		{
			stream.WriteByte(74);
			BufferStream.RangeHandle range8 = stream.GetRange(5);
			int position8 = stream.Position;
			FerryUpdateScheduleRequest.SerializeDelta(stream, instance.ferryUpdateSchedule, previous.ferryUpdateSchedule);
			int val3 = stream.Position - position8;
			Span<byte> span8 = range8.GetSpan();
			int num8 = ProtocolParser.WriteUInt32((uint)val3, span8, 0);
			if (num8 < 5)
			{
				span8[num8 - 1] |= 128;
				while (num8 < 4)
				{
					span8[num8++] = 128;
				}
				span8[4] = 0;
			}
		}
		if (instance.clanChatBatch != null)
		{
			stream.WriteByte(82);
			BufferStream.RangeHandle range9 = stream.GetRange(5);
			int position9 = stream.Position;
			ClanChatBatchRequest.SerializeDelta(stream, instance.clanChatBatch, previous.clanChatBatch);
			int val4 = stream.Position - position9;
			Span<byte> span9 = range9.GetSpan();
			int num9 = ProtocolParser.WriteUInt32((uint)val4, span9, 0);
			if (num9 < 5)
			{
				span9[num9 - 1] |= 128;
				while (num9 < 4)
				{
					span9[num9++] = 128;
				}
				span9[4] = 0;
			}
		}
		if (instance.playerManifest == null)
		{
			return;
		}
		stream.WriteByte(90);
		BufferStream.RangeHandle range10 = stream.GetRange(3);
		int position10 = stream.Position;
		PlayerManifestRequest.SerializeDelta(stream, instance.playerManifest, previous.playerManifest);
		int num10 = stream.Position - position10;
		if (num10 > 2097151)
		{
			throw new InvalidOperationException("Not enough space was reserved for the length prefix of field playerManifest (ProtoBuf.Nexus.PlayerManifestRequest)");
		}
		Span<byte> span10 = range10.GetSpan();
		int num11 = ProtocolParser.WriteUInt32((uint)num10, span10, 0);
		if (num11 < 3)
		{
			span10[num11 - 1] |= 128;
			while (num11 < 2)
			{
				span10[num11++] = 128;
			}
			span10[2] = 0;
		}
	}

	public static void Serialize(BufferStream stream, Request instance)
	{
		if (instance.isFireAndForget)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteBool(stream, instance.isFireAndForget);
		}
		if (instance.transfer != null)
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			TransferRequest.Serialize(stream, instance.transfer);
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
		if (instance.ping != null)
		{
			stream.WriteByte(26);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			PingRequest.Serialize(stream, instance.ping);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field ping (ProtoBuf.Nexus.PingRequest)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.spawnOptions != null)
		{
			stream.WriteByte(34);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			SpawnOptionsRequest.Serialize(stream, instance.spawnOptions);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field spawnOptions (ProtoBuf.Nexus.SpawnOptionsRequest)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
		if (instance.respawnAtBag != null)
		{
			stream.WriteByte(42);
			BufferStream.RangeHandle range4 = stream.GetRange(5);
			int position4 = stream.Position;
			SleepingBagRespawnRequest.Serialize(stream, instance.respawnAtBag);
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
		if (instance.destroyBag != null)
		{
			stream.WriteByte(50);
			BufferStream.RangeHandle range5 = stream.GetRange(1);
			int position5 = stream.Position;
			SleepingBagDestroyRequest.Serialize(stream, instance.destroyBag);
			int num5 = stream.Position - position5;
			if (num5 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field destroyBag (ProtoBuf.Nexus.SleepingBagDestroyRequest)");
			}
			Span<byte> span5 = range5.GetSpan();
			ProtocolParser.WriteUInt32((uint)num5, span5, 0);
		}
		if (instance.ferryStatus != null)
		{
			stream.WriteByte(58);
			BufferStream.RangeHandle range6 = stream.GetRange(1);
			int position6 = stream.Position;
			FerryStatusRequest.Serialize(stream, instance.ferryStatus);
			int num6 = stream.Position - position6;
			if (num6 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field ferryStatus (ProtoBuf.Nexus.FerryStatusRequest)");
			}
			Span<byte> span6 = range6.GetSpan();
			ProtocolParser.WriteUInt32((uint)num6, span6, 0);
		}
		if (instance.ferryRetire != null)
		{
			stream.WriteByte(66);
			BufferStream.RangeHandle range7 = stream.GetRange(1);
			int position7 = stream.Position;
			FerryRetireRequest.Serialize(stream, instance.ferryRetire);
			int num7 = stream.Position - position7;
			if (num7 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field ferryRetire (ProtoBuf.Nexus.FerryRetireRequest)");
			}
			Span<byte> span7 = range7.GetSpan();
			ProtocolParser.WriteUInt32((uint)num7, span7, 0);
		}
		if (instance.ferryUpdateSchedule != null)
		{
			stream.WriteByte(74);
			BufferStream.RangeHandle range8 = stream.GetRange(5);
			int position8 = stream.Position;
			FerryUpdateScheduleRequest.Serialize(stream, instance.ferryUpdateSchedule);
			int val3 = stream.Position - position8;
			Span<byte> span8 = range8.GetSpan();
			int num8 = ProtocolParser.WriteUInt32((uint)val3, span8, 0);
			if (num8 < 5)
			{
				span8[num8 - 1] |= 128;
				while (num8 < 4)
				{
					span8[num8++] = 128;
				}
				span8[4] = 0;
			}
		}
		if (instance.clanChatBatch != null)
		{
			stream.WriteByte(82);
			BufferStream.RangeHandle range9 = stream.GetRange(5);
			int position9 = stream.Position;
			ClanChatBatchRequest.Serialize(stream, instance.clanChatBatch);
			int val4 = stream.Position - position9;
			Span<byte> span9 = range9.GetSpan();
			int num9 = ProtocolParser.WriteUInt32((uint)val4, span9, 0);
			if (num9 < 5)
			{
				span9[num9 - 1] |= 128;
				while (num9 < 4)
				{
					span9[num9++] = 128;
				}
				span9[4] = 0;
			}
		}
		if (instance.playerManifest == null)
		{
			return;
		}
		stream.WriteByte(90);
		BufferStream.RangeHandle range10 = stream.GetRange(3);
		int position10 = stream.Position;
		PlayerManifestRequest.Serialize(stream, instance.playerManifest);
		int num10 = stream.Position - position10;
		if (num10 > 2097151)
		{
			throw new InvalidOperationException("Not enough space was reserved for the length prefix of field playerManifest (ProtoBuf.Nexus.PlayerManifestRequest)");
		}
		Span<byte> span10 = range10.GetSpan();
		int num11 = ProtocolParser.WriteUInt32((uint)num10, span10, 0);
		if (num11 < 3)
		{
			span10[num11 - 1] |= 128;
			while (num11 < 2)
			{
				span10[num11++] = 128;
			}
			span10[2] = 0;
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		transfer?.InspectUids(action);
		ping?.InspectUids(action);
		spawnOptions?.InspectUids(action);
		respawnAtBag?.InspectUids(action);
		destroyBag?.InspectUids(action);
		ferryStatus?.InspectUids(action);
		ferryRetire?.InspectUids(action);
		ferryUpdateSchedule?.InspectUids(action);
		clanChatBatch?.InspectUids(action);
		playerManifest?.InspectUids(action);
	}
}
