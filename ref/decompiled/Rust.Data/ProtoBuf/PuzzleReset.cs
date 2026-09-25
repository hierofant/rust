using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class PuzzleReset : IDisposable, Pool.IPooled, IProto<PuzzleReset>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public bool playerBlocksReset;

	[NonSerialized]
	public float playerDetectionRadius;

	[NonSerialized]
	public Vector3 playerDetectionOrigin;

	[NonSerialized]
	public float timeBetweenResets;

	[NonSerialized]
	public bool scaleWithServerPopulation;

	[NonSerialized]
	public bool checkSleepingAIZForPlayers;

	[NonSerialized]
	public bool ignoreAboveGroundPlayers;

	[NonSerialized]
	public bool broadcastResetMessage;

	[NonSerialized]
	public string resetPhrase;

	[NonSerialized]
	public bool radiationReset;

	[NonSerialized]
	public bool pauseUntilLooted;

	[NonSerialized]
	public List<NetworkableId> danglingSpawnedInstances;

	[NonSerialized]
	public List<Vector3> resetPositions;

	public static void ResetToPool(PuzzleReset instance)
	{
		if (instance.ShouldPool)
		{
			instance.playerBlocksReset = false;
			instance.playerDetectionRadius = 0f;
			instance.playerDetectionOrigin = default(Vector3);
			instance.timeBetweenResets = 0f;
			instance.scaleWithServerPopulation = false;
			instance.checkSleepingAIZForPlayers = false;
			instance.ignoreAboveGroundPlayers = false;
			instance.broadcastResetMessage = false;
			instance.resetPhrase = string.Empty;
			instance.radiationReset = false;
			instance.pauseUntilLooted = false;
			if (instance.danglingSpawnedInstances != null)
			{
				List<NetworkableId> obj = instance.danglingSpawnedInstances;
				Pool.FreeUnmanaged(ref obj);
				instance.danglingSpawnedInstances = obj;
			}
			if (instance.resetPositions != null)
			{
				List<Vector3> obj2 = instance.resetPositions;
				Pool.FreeUnmanaged(ref obj2);
				instance.resetPositions = obj2;
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
			throw new Exception("Trying to dispose PuzzleReset with ShouldPool set to false!");
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

	public void CopyTo(PuzzleReset instance)
	{
		instance.playerBlocksReset = playerBlocksReset;
		instance.playerDetectionRadius = playerDetectionRadius;
		instance.playerDetectionOrigin = playerDetectionOrigin;
		instance.timeBetweenResets = timeBetweenResets;
		instance.scaleWithServerPopulation = scaleWithServerPopulation;
		instance.checkSleepingAIZForPlayers = checkSleepingAIZForPlayers;
		instance.ignoreAboveGroundPlayers = ignoreAboveGroundPlayers;
		instance.broadcastResetMessage = broadcastResetMessage;
		instance.resetPhrase = resetPhrase;
		instance.radiationReset = radiationReset;
		instance.pauseUntilLooted = pauseUntilLooted;
		if (danglingSpawnedInstances != null)
		{
			instance.danglingSpawnedInstances = Pool.Get<List<NetworkableId>>();
			for (int i = 0; i < danglingSpawnedInstances.Count; i++)
			{
				NetworkableId item = danglingSpawnedInstances[i];
				instance.danglingSpawnedInstances.Add(item);
			}
		}
		else
		{
			instance.danglingSpawnedInstances = null;
		}
		if (resetPositions != null)
		{
			instance.resetPositions = Pool.Get<List<Vector3>>();
			for (int j = 0; j < resetPositions.Count; j++)
			{
				Vector3 item2 = resetPositions[j];
				instance.resetPositions.Add(item2);
			}
		}
		else
		{
			instance.resetPositions = null;
		}
	}

	public PuzzleReset Copy()
	{
		PuzzleReset puzzleReset = Pool.Get<PuzzleReset>();
		CopyTo(puzzleReset);
		return puzzleReset;
	}

	public static PuzzleReset Deserialize(BufferStream stream)
	{
		PuzzleReset puzzleReset = Pool.Get<PuzzleReset>();
		Deserialize(stream, puzzleReset, isDelta: false);
		return puzzleReset;
	}

	public static PuzzleReset DeserializeLengthDelimited(BufferStream stream)
	{
		PuzzleReset puzzleReset = Pool.Get<PuzzleReset>();
		DeserializeLengthDelimited(stream, puzzleReset, isDelta: false);
		return puzzleReset;
	}

	public static PuzzleReset DeserializeLength(BufferStream stream, int length)
	{
		PuzzleReset puzzleReset = Pool.Get<PuzzleReset>();
		DeserializeLength(stream, length, puzzleReset, isDelta: false);
		return puzzleReset;
	}

	public static PuzzleReset Deserialize(byte[] buffer)
	{
		PuzzleReset puzzleReset = Pool.Get<PuzzleReset>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, puzzleReset, isDelta: false);
		return puzzleReset;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, PuzzleReset previous)
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

	public static PuzzleReset Deserialize(BufferStream stream, PuzzleReset instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.danglingSpawnedInstances == null)
			{
				instance.danglingSpawnedInstances = Pool.Get<List<NetworkableId>>();
			}
			if (instance.resetPositions == null)
			{
				instance.resetPositions = Pool.Get<List<Vector3>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.PuzzleReset");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				instance.playerBlocksReset = ProtocolParser.ReadBool(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				instance.playerDetectionRadius = ProtocolParser.ReadSingle(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.playerDetectionOrigin, isDelta);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				instance.timeBetweenResets = ProtocolParser.ReadSingle(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				instance.scaleWithServerPopulation = ProtocolParser.ReadBool(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				instance.checkSleepingAIZForPlayers = ProtocolParser.ReadBool(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				instance.ignoreAboveGroundPlayers = ProtocolParser.ReadBool(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				instance.broadcastResetMessage = ProtocolParser.ReadBool(stream);
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				instance.resetPhrase = ProtocolParser.ReadString(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				instance.radiationReset = ProtocolParser.ReadBool(stream);
				continue;
			case 88:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				instance.pauseUntilLooted = ProtocolParser.ReadBool(stream);
				continue;
			case 96:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: true, "ProtoBuf.PuzzleReset");
				stream.ConsumeRepeatedElement();
				instance.danglingSpawnedInstances.Add(new NetworkableId(ProtocolParser.ReadUInt64(stream)));
				continue;
			case 106:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: true, "ProtoBuf.PuzzleReset");
				stream.ConsumeRepeatedElement();
				Vector3 instance2 = default(Vector3);
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.resetPositions.Add(instance2);
				continue;
			}
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: true, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: true, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PuzzleReset DeserializeLengthDelimited(BufferStream stream, PuzzleReset instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.danglingSpawnedInstances == null)
			{
				instance.danglingSpawnedInstances = Pool.Get<List<NetworkableId>>();
			}
			if (instance.resetPositions == null)
			{
				instance.resetPositions = Pool.Get<List<Vector3>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.PuzzleReset");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				instance.playerBlocksReset = ProtocolParser.ReadBool(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				instance.playerDetectionRadius = ProtocolParser.ReadSingle(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.playerDetectionOrigin, isDelta);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				instance.timeBetweenResets = ProtocolParser.ReadSingle(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				instance.scaleWithServerPopulation = ProtocolParser.ReadBool(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				instance.checkSleepingAIZForPlayers = ProtocolParser.ReadBool(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				instance.ignoreAboveGroundPlayers = ProtocolParser.ReadBool(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				instance.broadcastResetMessage = ProtocolParser.ReadBool(stream);
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				instance.resetPhrase = ProtocolParser.ReadString(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				instance.radiationReset = ProtocolParser.ReadBool(stream);
				continue;
			case 88:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				instance.pauseUntilLooted = ProtocolParser.ReadBool(stream);
				continue;
			case 96:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: true, "ProtoBuf.PuzzleReset");
				stream.ConsumeRepeatedElement();
				instance.danglingSpawnedInstances.Add(new NetworkableId(ProtocolParser.ReadUInt64(stream)));
				continue;
			case 106:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: true, "ProtoBuf.PuzzleReset");
				stream.ConsumeRepeatedElement();
				Vector3 instance2 = default(Vector3);
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.resetPositions.Add(instance2);
				continue;
			}
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: true, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: true, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PuzzleReset DeserializeLength(BufferStream stream, int length, PuzzleReset instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.danglingSpawnedInstances == null)
			{
				instance.danglingSpawnedInstances = Pool.Get<List<NetworkableId>>();
			}
			if (instance.resetPositions == null)
			{
				instance.resetPositions = Pool.Get<List<Vector3>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.PuzzleReset");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				instance.playerBlocksReset = ProtocolParser.ReadBool(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				instance.playerDetectionRadius = ProtocolParser.ReadSingle(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.playerDetectionOrigin, isDelta);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				instance.timeBetweenResets = ProtocolParser.ReadSingle(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				instance.scaleWithServerPopulation = ProtocolParser.ReadBool(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				instance.checkSleepingAIZForPlayers = ProtocolParser.ReadBool(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				instance.ignoreAboveGroundPlayers = ProtocolParser.ReadBool(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				instance.broadcastResetMessage = ProtocolParser.ReadBool(stream);
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				instance.resetPhrase = ProtocolParser.ReadString(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				instance.radiationReset = ProtocolParser.ReadBool(stream);
				continue;
			case 88:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				instance.pauseUntilLooted = ProtocolParser.ReadBool(stream);
				continue;
			case 96:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: true, "ProtoBuf.PuzzleReset");
				stream.ConsumeRepeatedElement();
				instance.danglingSpawnedInstances.Add(new NetworkableId(ProtocolParser.ReadUInt64(stream)));
				continue;
			case 106:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: true, "ProtoBuf.PuzzleReset");
				stream.ConsumeRepeatedElement();
				Vector3 instance2 = default(Vector3);
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.resetPositions.Add(instance2);
				continue;
			}
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: true, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: true, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PuzzleReset");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, PuzzleReset instance, PuzzleReset previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteBool(stream, instance.playerBlocksReset);
		if (instance.playerDetectionRadius != previous.playerDetectionRadius)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.playerDetectionRadius);
		}
		if (instance.playerDetectionOrigin != previous.playerDetectionOrigin)
		{
			stream.WriteByte(26);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.playerDetectionOrigin, previous.playerDetectionOrigin);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field playerDetectionOrigin (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.timeBetweenResets != previous.timeBetweenResets)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.timeBetweenResets);
		}
		stream.WriteByte(40);
		ProtocolParser.WriteBool(stream, instance.scaleWithServerPopulation);
		stream.WriteByte(48);
		ProtocolParser.WriteBool(stream, instance.checkSleepingAIZForPlayers);
		stream.WriteByte(56);
		ProtocolParser.WriteBool(stream, instance.ignoreAboveGroundPlayers);
		stream.WriteByte(64);
		ProtocolParser.WriteBool(stream, instance.broadcastResetMessage);
		if (instance.resetPhrase != null && instance.resetPhrase != previous.resetPhrase)
		{
			stream.WriteByte(74);
			ProtocolParser.WriteString(stream, instance.resetPhrase);
		}
		stream.WriteByte(80);
		ProtocolParser.WriteBool(stream, instance.radiationReset);
		stream.WriteByte(88);
		ProtocolParser.WriteBool(stream, instance.pauseUntilLooted);
		if (instance.danglingSpawnedInstances != null)
		{
			for (int i = 0; i < instance.danglingSpawnedInstances.Count; i++)
			{
				NetworkableId networkableId = instance.danglingSpawnedInstances[i];
				stream.WriteByte(96);
				ProtocolParser.WriteUInt64(stream, networkableId.Value);
			}
		}
		if (instance.resetPositions == null)
		{
			return;
		}
		for (int j = 0; j < instance.resetPositions.Count; j++)
		{
			Vector3 vector = instance.resetPositions[j];
			stream.WriteByte(106);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, vector, vector);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field resetPositions (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
	}

	public static void Serialize(BufferStream stream, PuzzleReset instance)
	{
		if (instance.playerBlocksReset)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteBool(stream, instance.playerBlocksReset);
		}
		if (instance.playerDetectionRadius != 0f)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.playerDetectionRadius);
		}
		if (instance.playerDetectionOrigin != default(Vector3))
		{
			stream.WriteByte(26);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.Serialize(stream, instance.playerDetectionOrigin);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field playerDetectionOrigin (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.timeBetweenResets != 0f)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.timeBetweenResets);
		}
		if (instance.scaleWithServerPopulation)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteBool(stream, instance.scaleWithServerPopulation);
		}
		if (instance.checkSleepingAIZForPlayers)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteBool(stream, instance.checkSleepingAIZForPlayers);
		}
		if (instance.ignoreAboveGroundPlayers)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteBool(stream, instance.ignoreAboveGroundPlayers);
		}
		if (instance.broadcastResetMessage)
		{
			stream.WriteByte(64);
			ProtocolParser.WriteBool(stream, instance.broadcastResetMessage);
		}
		if (instance.resetPhrase != null)
		{
			stream.WriteByte(74);
			ProtocolParser.WriteString(stream, instance.resetPhrase);
		}
		if (instance.radiationReset)
		{
			stream.WriteByte(80);
			ProtocolParser.WriteBool(stream, instance.radiationReset);
		}
		if (instance.pauseUntilLooted)
		{
			stream.WriteByte(88);
			ProtocolParser.WriteBool(stream, instance.pauseUntilLooted);
		}
		if (instance.danglingSpawnedInstances != null)
		{
			for (int i = 0; i < instance.danglingSpawnedInstances.Count; i++)
			{
				NetworkableId networkableId = instance.danglingSpawnedInstances[i];
				stream.WriteByte(96);
				ProtocolParser.WriteUInt64(stream, networkableId.Value);
			}
		}
		if (instance.resetPositions == null)
		{
			return;
		}
		for (int j = 0; j < instance.resetPositions.Count; j++)
		{
			Vector3 instance2 = instance.resetPositions[j];
			stream.WriteByte(106);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.Serialize(stream, instance2);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field resetPositions (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (danglingSpawnedInstances != null)
		{
			for (int i = 0; i < danglingSpawnedInstances.Count; i++)
			{
				NetworkableId value = danglingSpawnedInstances[i];
				action(UidType.NetworkableId, ref value.Value);
				danglingSpawnedInstances[i] = value;
			}
		}
	}
}
