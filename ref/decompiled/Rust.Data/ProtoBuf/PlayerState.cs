using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class PlayerState : IDisposable, Pool.IPooled, IProto<PlayerState>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public double unHostileTimestamp;

	[NonSerialized]
	public List<MapNote> pointsOfInterest;

	[NonSerialized]
	public MapNote deathMarker;

	[NonSerialized]
	public Missions missions;

	[NonSerialized]
	public List<MapNote> pings;

	[NonSerialized]
	public bool chatMuted;

	[NonSerialized]
	public double chatMuteExpiryTimestamp;

	[NonSerialized]
	public int numberOfTimesReported;

	[NonSerialized]
	public List<uint> fogImagesMainland;

	[NonSerialized]
	public NetworkableId fogImageNetId;

	[NonSerialized]
	public List<uint> fogImagesDeepSea;

	[NonSerialized]
	public int paintballColor;

	[NonSerialized]
	public List<ReconnectToast> toastOnReconnect;

	[NonSerialized]
	public int protocol;

	[NonSerialized]
	public uint seed;

	[NonSerialized]
	public int saveCreatedTime;

	public static void ResetToPool(PlayerState instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.unHostileTimestamp = 0.0;
		if (instance.pointsOfInterest != null)
		{
			for (int i = 0; i < instance.pointsOfInterest.Count; i++)
			{
				if (instance.pointsOfInterest[i] != null)
				{
					instance.pointsOfInterest[i].ResetToPool();
					instance.pointsOfInterest[i] = null;
				}
			}
			List<MapNote> obj = instance.pointsOfInterest;
			Pool.Free(ref obj, freeElements: false);
			instance.pointsOfInterest = obj;
		}
		if (instance.deathMarker != null)
		{
			instance.deathMarker.ResetToPool();
			instance.deathMarker = null;
		}
		if (instance.missions != null)
		{
			instance.missions.ResetToPool();
			instance.missions = null;
		}
		if (instance.pings != null)
		{
			for (int j = 0; j < instance.pings.Count; j++)
			{
				if (instance.pings[j] != null)
				{
					instance.pings[j].ResetToPool();
					instance.pings[j] = null;
				}
			}
			List<MapNote> obj2 = instance.pings;
			Pool.Free(ref obj2, freeElements: false);
			instance.pings = obj2;
		}
		instance.chatMuted = false;
		instance.chatMuteExpiryTimestamp = 0.0;
		instance.numberOfTimesReported = 0;
		if (instance.fogImagesMainland != null)
		{
			List<uint> obj3 = instance.fogImagesMainland;
			Pool.FreeUnmanaged(ref obj3);
			instance.fogImagesMainland = obj3;
		}
		instance.fogImageNetId = default(NetworkableId);
		if (instance.fogImagesDeepSea != null)
		{
			List<uint> obj4 = instance.fogImagesDeepSea;
			Pool.FreeUnmanaged(ref obj4);
			instance.fogImagesDeepSea = obj4;
		}
		instance.paintballColor = 0;
		if (instance.toastOnReconnect != null)
		{
			for (int k = 0; k < instance.toastOnReconnect.Count; k++)
			{
				if (instance.toastOnReconnect[k] != null)
				{
					instance.toastOnReconnect[k].ResetToPool();
					instance.toastOnReconnect[k] = null;
				}
			}
			List<ReconnectToast> obj5 = instance.toastOnReconnect;
			Pool.Free(ref obj5, freeElements: false);
			instance.toastOnReconnect = obj5;
		}
		instance.protocol = 0;
		instance.seed = 0u;
		instance.saveCreatedTime = 0;
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
			throw new Exception("Trying to dispose PlayerState with ShouldPool set to false!");
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

	public void CopyTo(PlayerState instance)
	{
		instance.unHostileTimestamp = unHostileTimestamp;
		if (pointsOfInterest != null)
		{
			instance.pointsOfInterest = Pool.Get<List<MapNote>>();
			for (int i = 0; i < pointsOfInterest.Count; i++)
			{
				MapNote item = pointsOfInterest[i].Copy();
				instance.pointsOfInterest.Add(item);
			}
		}
		else
		{
			instance.pointsOfInterest = null;
		}
		if (deathMarker != null)
		{
			if (instance.deathMarker == null)
			{
				instance.deathMarker = deathMarker.Copy();
			}
			else
			{
				deathMarker.CopyTo(instance.deathMarker);
			}
		}
		else
		{
			instance.deathMarker = null;
		}
		if (missions != null)
		{
			if (instance.missions == null)
			{
				instance.missions = missions.Copy();
			}
			else
			{
				missions.CopyTo(instance.missions);
			}
		}
		else
		{
			instance.missions = null;
		}
		if (pings != null)
		{
			instance.pings = Pool.Get<List<MapNote>>();
			for (int j = 0; j < pings.Count; j++)
			{
				MapNote item2 = pings[j].Copy();
				instance.pings.Add(item2);
			}
		}
		else
		{
			instance.pings = null;
		}
		instance.chatMuted = chatMuted;
		instance.chatMuteExpiryTimestamp = chatMuteExpiryTimestamp;
		instance.numberOfTimesReported = numberOfTimesReported;
		if (fogImagesMainland != null)
		{
			instance.fogImagesMainland = Pool.Get<List<uint>>();
			for (int k = 0; k < fogImagesMainland.Count; k++)
			{
				uint item3 = fogImagesMainland[k];
				instance.fogImagesMainland.Add(item3);
			}
		}
		else
		{
			instance.fogImagesMainland = null;
		}
		instance.fogImageNetId = fogImageNetId;
		if (fogImagesDeepSea != null)
		{
			instance.fogImagesDeepSea = Pool.Get<List<uint>>();
			for (int l = 0; l < fogImagesDeepSea.Count; l++)
			{
				uint item4 = fogImagesDeepSea[l];
				instance.fogImagesDeepSea.Add(item4);
			}
		}
		else
		{
			instance.fogImagesDeepSea = null;
		}
		instance.paintballColor = paintballColor;
		if (toastOnReconnect != null)
		{
			instance.toastOnReconnect = Pool.Get<List<ReconnectToast>>();
			for (int m = 0; m < toastOnReconnect.Count; m++)
			{
				ReconnectToast item5 = toastOnReconnect[m].Copy();
				instance.toastOnReconnect.Add(item5);
			}
		}
		else
		{
			instance.toastOnReconnect = null;
		}
		instance.protocol = protocol;
		instance.seed = seed;
		instance.saveCreatedTime = saveCreatedTime;
	}

	public PlayerState Copy()
	{
		PlayerState playerState = Pool.Get<PlayerState>();
		CopyTo(playerState);
		return playerState;
	}

	public static PlayerState Deserialize(BufferStream stream)
	{
		PlayerState playerState = Pool.Get<PlayerState>();
		Deserialize(stream, playerState, isDelta: false);
		return playerState;
	}

	public static PlayerState DeserializeLengthDelimited(BufferStream stream)
	{
		PlayerState playerState = Pool.Get<PlayerState>();
		DeserializeLengthDelimited(stream, playerState, isDelta: false);
		return playerState;
	}

	public static PlayerState DeserializeLength(BufferStream stream, int length)
	{
		PlayerState playerState = Pool.Get<PlayerState>();
		DeserializeLength(stream, length, playerState, isDelta: false);
		return playerState;
	}

	public static PlayerState Deserialize(byte[] buffer)
	{
		PlayerState playerState = Pool.Get<PlayerState>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, playerState, isDelta: false);
		return playerState;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, PlayerState previous)
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

	public static PlayerState Deserialize(BufferStream stream, PlayerState instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.pointsOfInterest == null)
			{
				instance.pointsOfInterest = Pool.Get<List<MapNote>>();
			}
			if (instance.pings == null)
			{
				instance.pings = Pool.Get<List<MapNote>>();
			}
			if (instance.fogImagesMainland == null)
			{
				instance.fogImagesMainland = Pool.Get<List<uint>>();
			}
			if (instance.fogImagesDeepSea == null)
			{
				instance.fogImagesDeepSea = Pool.Get<List<uint>>();
			}
			if (instance.toastOnReconnect == null)
			{
				instance.toastOnReconnect = Pool.Get<List<ReconnectToast>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.PlayerState");
			switch (num)
			{
			case 9:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				instance.unHostileTimestamp = ProtocolParser.ReadDouble(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				stream.ConsumeRepeatedElement();
				instance.pointsOfInterest.Add(MapNote.DeserializeLengthDelimited(stream));
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				if (instance.deathMarker == null)
				{
					instance.deathMarker = MapNote.DeserializeLengthDelimited(stream);
				}
				else
				{
					MapNote.DeserializeLengthDelimited(stream, instance.deathMarker, isDelta);
				}
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				if (instance.missions == null)
				{
					instance.missions = Missions.DeserializeLengthDelimited(stream);
				}
				else
				{
					Missions.DeserializeLengthDelimited(stream, instance.missions, isDelta);
				}
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				stream.ConsumeRepeatedElement();
				instance.pings.Add(MapNote.DeserializeLengthDelimited(stream));
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				instance.chatMuted = ProtocolParser.ReadBool(stream);
				continue;
			case 65:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				instance.chatMuteExpiryTimestamp = ProtocolParser.ReadDouble(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				instance.numberOfTimesReported = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				stream.ConsumeRepeatedElement();
				instance.fogImagesMainland.Add(ProtocolParser.ReadUInt32(stream));
				continue;
			case 88:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				instance.fogImageNetId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 96:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				stream.ConsumeRepeatedElement();
				instance.fogImagesDeepSea.Add(ProtocolParser.ReadUInt32(stream));
				continue;
			case 104:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				instance.paintballColor = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 114:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				stream.ConsumeRepeatedElement();
				instance.toastOnReconnect.Add(ReconnectToast.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 50u:
				stream.ValidateFieldOrder(ref lastFieldId, 50u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				if (key.WireType == Wire.Varint)
				{
					instance.protocol = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 51u:
				stream.ValidateFieldOrder(ref lastFieldId, 51u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				if (key.WireType == Wire.Varint)
				{
					instance.seed = ProtocolParser.ReadUInt32(stream);
				}
				break;
			case 52u:
				stream.ValidateFieldOrder(ref lastFieldId, 52u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				if (key.WireType == Wire.Varint)
				{
					instance.saveCreatedTime = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PlayerState DeserializeLengthDelimited(BufferStream stream, PlayerState instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.pointsOfInterest == null)
			{
				instance.pointsOfInterest = Pool.Get<List<MapNote>>();
			}
			if (instance.pings == null)
			{
				instance.pings = Pool.Get<List<MapNote>>();
			}
			if (instance.fogImagesMainland == null)
			{
				instance.fogImagesMainland = Pool.Get<List<uint>>();
			}
			if (instance.fogImagesDeepSea == null)
			{
				instance.fogImagesDeepSea = Pool.Get<List<uint>>();
			}
			if (instance.toastOnReconnect == null)
			{
				instance.toastOnReconnect = Pool.Get<List<ReconnectToast>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.PlayerState");
			switch (num2)
			{
			case 9:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				instance.unHostileTimestamp = ProtocolParser.ReadDouble(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				stream.ConsumeRepeatedElement();
				instance.pointsOfInterest.Add(MapNote.DeserializeLengthDelimited(stream));
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				if (instance.deathMarker == null)
				{
					instance.deathMarker = MapNote.DeserializeLengthDelimited(stream);
				}
				else
				{
					MapNote.DeserializeLengthDelimited(stream, instance.deathMarker, isDelta);
				}
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				if (instance.missions == null)
				{
					instance.missions = Missions.DeserializeLengthDelimited(stream);
				}
				else
				{
					Missions.DeserializeLengthDelimited(stream, instance.missions, isDelta);
				}
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				stream.ConsumeRepeatedElement();
				instance.pings.Add(MapNote.DeserializeLengthDelimited(stream));
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				instance.chatMuted = ProtocolParser.ReadBool(stream);
				continue;
			case 65:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				instance.chatMuteExpiryTimestamp = ProtocolParser.ReadDouble(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				instance.numberOfTimesReported = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				stream.ConsumeRepeatedElement();
				instance.fogImagesMainland.Add(ProtocolParser.ReadUInt32(stream));
				continue;
			case 88:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				instance.fogImageNetId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 96:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				stream.ConsumeRepeatedElement();
				instance.fogImagesDeepSea.Add(ProtocolParser.ReadUInt32(stream));
				continue;
			case 104:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				instance.paintballColor = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 114:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				stream.ConsumeRepeatedElement();
				instance.toastOnReconnect.Add(ReconnectToast.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 50u:
				stream.ValidateFieldOrder(ref lastFieldId, 50u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				if (key.WireType == Wire.Varint)
				{
					instance.protocol = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 51u:
				stream.ValidateFieldOrder(ref lastFieldId, 51u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				if (key.WireType == Wire.Varint)
				{
					instance.seed = ProtocolParser.ReadUInt32(stream);
				}
				break;
			case 52u:
				stream.ValidateFieldOrder(ref lastFieldId, 52u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				if (key.WireType == Wire.Varint)
				{
					instance.saveCreatedTime = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PlayerState DeserializeLength(BufferStream stream, int length, PlayerState instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.pointsOfInterest == null)
			{
				instance.pointsOfInterest = Pool.Get<List<MapNote>>();
			}
			if (instance.pings == null)
			{
				instance.pings = Pool.Get<List<MapNote>>();
			}
			if (instance.fogImagesMainland == null)
			{
				instance.fogImagesMainland = Pool.Get<List<uint>>();
			}
			if (instance.fogImagesDeepSea == null)
			{
				instance.fogImagesDeepSea = Pool.Get<List<uint>>();
			}
			if (instance.toastOnReconnect == null)
			{
				instance.toastOnReconnect = Pool.Get<List<ReconnectToast>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.PlayerState");
			switch (num2)
			{
			case 9:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				instance.unHostileTimestamp = ProtocolParser.ReadDouble(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				stream.ConsumeRepeatedElement();
				instance.pointsOfInterest.Add(MapNote.DeserializeLengthDelimited(stream));
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				if (instance.deathMarker == null)
				{
					instance.deathMarker = MapNote.DeserializeLengthDelimited(stream);
				}
				else
				{
					MapNote.DeserializeLengthDelimited(stream, instance.deathMarker, isDelta);
				}
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				if (instance.missions == null)
				{
					instance.missions = Missions.DeserializeLengthDelimited(stream);
				}
				else
				{
					Missions.DeserializeLengthDelimited(stream, instance.missions, isDelta);
				}
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				stream.ConsumeRepeatedElement();
				instance.pings.Add(MapNote.DeserializeLengthDelimited(stream));
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				instance.chatMuted = ProtocolParser.ReadBool(stream);
				continue;
			case 65:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				instance.chatMuteExpiryTimestamp = ProtocolParser.ReadDouble(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				instance.numberOfTimesReported = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				stream.ConsumeRepeatedElement();
				instance.fogImagesMainland.Add(ProtocolParser.ReadUInt32(stream));
				continue;
			case 88:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				instance.fogImageNetId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 96:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				stream.ConsumeRepeatedElement();
				instance.fogImagesDeepSea.Add(ProtocolParser.ReadUInt32(stream));
				continue;
			case 104:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				instance.paintballColor = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 114:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				stream.ConsumeRepeatedElement();
				instance.toastOnReconnect.Add(ReconnectToast.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 50u:
				stream.ValidateFieldOrder(ref lastFieldId, 50u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				if (key.WireType == Wire.Varint)
				{
					instance.protocol = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 51u:
				stream.ValidateFieldOrder(ref lastFieldId, 51u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				if (key.WireType == Wire.Varint)
				{
					instance.seed = ProtocolParser.ReadUInt32(stream);
				}
				break;
			case 52u:
				stream.ValidateFieldOrder(ref lastFieldId, 52u, fieldIsRepeated: false, "ProtoBuf.PlayerState");
				if (key.WireType == Wire.Varint)
				{
					instance.saveCreatedTime = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerState");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, PlayerState instance, PlayerState previous)
	{
		if (instance.unHostileTimestamp != previous.unHostileTimestamp)
		{
			stream.WriteByte(9);
			ProtocolParser.WriteDouble(stream, instance.unHostileTimestamp);
		}
		if (instance.pointsOfInterest != null)
		{
			for (int i = 0; i < instance.pointsOfInterest.Count; i++)
			{
				MapNote mapNote = instance.pointsOfInterest[i];
				stream.WriteByte(26);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				MapNote.SerializeDelta(stream, mapNote, mapNote);
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
		if (instance.deathMarker != null)
		{
			stream.WriteByte(34);
			BufferStream.RangeHandle range2 = stream.GetRange(5);
			int position2 = stream.Position;
			MapNote.SerializeDelta(stream, instance.deathMarker, previous.deathMarker);
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
		if (instance.missions != null)
		{
			stream.WriteByte(42);
			BufferStream.RangeHandle range3 = stream.GetRange(5);
			int position3 = stream.Position;
			Missions.SerializeDelta(stream, instance.missions, previous.missions);
			int val3 = stream.Position - position3;
			Span<byte> span3 = range3.GetSpan();
			int num3 = ProtocolParser.WriteUInt32((uint)val3, span3, 0);
			if (num3 < 5)
			{
				span3[num3 - 1] |= 128;
				while (num3 < 4)
				{
					span3[num3++] = 128;
				}
				span3[4] = 0;
			}
		}
		if (instance.pings != null)
		{
			for (int j = 0; j < instance.pings.Count; j++)
			{
				MapNote mapNote2 = instance.pings[j];
				stream.WriteByte(50);
				BufferStream.RangeHandle range4 = stream.GetRange(5);
				int position4 = stream.Position;
				MapNote.SerializeDelta(stream, mapNote2, mapNote2);
				int val4 = stream.Position - position4;
				Span<byte> span4 = range4.GetSpan();
				int num4 = ProtocolParser.WriteUInt32((uint)val4, span4, 0);
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
		}
		stream.WriteByte(56);
		ProtocolParser.WriteBool(stream, instance.chatMuted);
		if (instance.chatMuteExpiryTimestamp != previous.chatMuteExpiryTimestamp)
		{
			stream.WriteByte(65);
			ProtocolParser.WriteDouble(stream, instance.chatMuteExpiryTimestamp);
		}
		if (instance.numberOfTimesReported != previous.numberOfTimesReported)
		{
			stream.WriteByte(72);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.numberOfTimesReported);
		}
		if (instance.fogImagesMainland != null)
		{
			for (int k = 0; k < instance.fogImagesMainland.Count; k++)
			{
				uint val5 = instance.fogImagesMainland[k];
				stream.WriteByte(80);
				ProtocolParser.WriteUInt32(stream, val5);
			}
		}
		stream.WriteByte(88);
		ProtocolParser.WriteUInt64(stream, instance.fogImageNetId.Value);
		if (instance.fogImagesDeepSea != null)
		{
			for (int l = 0; l < instance.fogImagesDeepSea.Count; l++)
			{
				uint val6 = instance.fogImagesDeepSea[l];
				stream.WriteByte(96);
				ProtocolParser.WriteUInt32(stream, val6);
			}
		}
		if (instance.paintballColor != previous.paintballColor)
		{
			stream.WriteByte(104);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.paintballColor);
		}
		if (instance.toastOnReconnect != null)
		{
			for (int m = 0; m < instance.toastOnReconnect.Count; m++)
			{
				ReconnectToast reconnectToast = instance.toastOnReconnect[m];
				stream.WriteByte(114);
				BufferStream.RangeHandle range5 = stream.GetRange(5);
				int position5 = stream.Position;
				ReconnectToast.SerializeDelta(stream, reconnectToast, reconnectToast);
				int val7 = stream.Position - position5;
				Span<byte> span5 = range5.GetSpan();
				int num5 = ProtocolParser.WriteUInt32((uint)val7, span5, 0);
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
		}
		if (instance.protocol != previous.protocol)
		{
			stream.WriteByte(144);
			stream.WriteByte(3);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.protocol);
		}
		if (instance.seed != previous.seed)
		{
			stream.WriteByte(152);
			stream.WriteByte(3);
			ProtocolParser.WriteUInt32(stream, instance.seed);
		}
		if (instance.saveCreatedTime != previous.saveCreatedTime)
		{
			stream.WriteByte(160);
			stream.WriteByte(3);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.saveCreatedTime);
		}
	}

	public static void Serialize(BufferStream stream, PlayerState instance)
	{
		if (instance.unHostileTimestamp != 0.0)
		{
			stream.WriteByte(9);
			ProtocolParser.WriteDouble(stream, instance.unHostileTimestamp);
		}
		if (instance.pointsOfInterest != null)
		{
			for (int i = 0; i < instance.pointsOfInterest.Count; i++)
			{
				MapNote instance2 = instance.pointsOfInterest[i];
				stream.WriteByte(26);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				MapNote.Serialize(stream, instance2);
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
		if (instance.deathMarker != null)
		{
			stream.WriteByte(34);
			BufferStream.RangeHandle range2 = stream.GetRange(5);
			int position2 = stream.Position;
			MapNote.Serialize(stream, instance.deathMarker);
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
		if (instance.missions != null)
		{
			stream.WriteByte(42);
			BufferStream.RangeHandle range3 = stream.GetRange(5);
			int position3 = stream.Position;
			Missions.Serialize(stream, instance.missions);
			int val3 = stream.Position - position3;
			Span<byte> span3 = range3.GetSpan();
			int num3 = ProtocolParser.WriteUInt32((uint)val3, span3, 0);
			if (num3 < 5)
			{
				span3[num3 - 1] |= 128;
				while (num3 < 4)
				{
					span3[num3++] = 128;
				}
				span3[4] = 0;
			}
		}
		if (instance.pings != null)
		{
			for (int j = 0; j < instance.pings.Count; j++)
			{
				MapNote instance3 = instance.pings[j];
				stream.WriteByte(50);
				BufferStream.RangeHandle range4 = stream.GetRange(5);
				int position4 = stream.Position;
				MapNote.Serialize(stream, instance3);
				int val4 = stream.Position - position4;
				Span<byte> span4 = range4.GetSpan();
				int num4 = ProtocolParser.WriteUInt32((uint)val4, span4, 0);
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
		}
		if (instance.chatMuted)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteBool(stream, instance.chatMuted);
		}
		if (instance.chatMuteExpiryTimestamp != 0.0)
		{
			stream.WriteByte(65);
			ProtocolParser.WriteDouble(stream, instance.chatMuteExpiryTimestamp);
		}
		if (instance.numberOfTimesReported != 0)
		{
			stream.WriteByte(72);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.numberOfTimesReported);
		}
		if (instance.fogImagesMainland != null)
		{
			for (int k = 0; k < instance.fogImagesMainland.Count; k++)
			{
				uint val5 = instance.fogImagesMainland[k];
				stream.WriteByte(80);
				ProtocolParser.WriteUInt32(stream, val5);
			}
		}
		if (instance.fogImageNetId != default(NetworkableId))
		{
			stream.WriteByte(88);
			ProtocolParser.WriteUInt64(stream, instance.fogImageNetId.Value);
		}
		if (instance.fogImagesDeepSea != null)
		{
			for (int l = 0; l < instance.fogImagesDeepSea.Count; l++)
			{
				uint val6 = instance.fogImagesDeepSea[l];
				stream.WriteByte(96);
				ProtocolParser.WriteUInt32(stream, val6);
			}
		}
		if (instance.paintballColor != 0)
		{
			stream.WriteByte(104);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.paintballColor);
		}
		if (instance.toastOnReconnect != null)
		{
			for (int m = 0; m < instance.toastOnReconnect.Count; m++)
			{
				ReconnectToast instance4 = instance.toastOnReconnect[m];
				stream.WriteByte(114);
				BufferStream.RangeHandle range5 = stream.GetRange(5);
				int position5 = stream.Position;
				ReconnectToast.Serialize(stream, instance4);
				int val7 = stream.Position - position5;
				Span<byte> span5 = range5.GetSpan();
				int num5 = ProtocolParser.WriteUInt32((uint)val7, span5, 0);
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
		}
		if (instance.protocol != 0)
		{
			stream.WriteByte(144);
			stream.WriteByte(3);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.protocol);
		}
		if (instance.seed != 0)
		{
			stream.WriteByte(152);
			stream.WriteByte(3);
			ProtocolParser.WriteUInt32(stream, instance.seed);
		}
		if (instance.saveCreatedTime != 0)
		{
			stream.WriteByte(160);
			stream.WriteByte(3);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.saveCreatedTime);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (pointsOfInterest != null)
		{
			for (int i = 0; i < pointsOfInterest.Count; i++)
			{
				pointsOfInterest[i]?.InspectUids(action);
			}
		}
		deathMarker?.InspectUids(action);
		missions?.InspectUids(action);
		if (pings != null)
		{
			for (int j = 0; j < pings.Count; j++)
			{
				pings[j]?.InspectUids(action);
			}
		}
		action(UidType.NetworkableId, ref fogImageNetId.Value);
		if (toastOnReconnect != null)
		{
			for (int k = 0; k < toastOnReconnect.Count; k++)
			{
				toastOnReconnect[k]?.InspectUids(action);
			}
		}
	}
}
