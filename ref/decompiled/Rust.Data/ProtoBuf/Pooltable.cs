using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class Pooltable : IDisposable, Pool.IPooled, IProto<Pooltable>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<PoolBallData> poolBalls;

	[NonSerialized]
	public int state;

	[NonSerialized]
	public ulong player1Id;

	[NonSerialized]
	public ulong player2Id;

	[NonSerialized]
	public int currentPlayer;

	[NonSerialized]
	public int player1Group;

	[NonSerialized]
	public int player2Group;

	[NonSerialized]
	public bool isSoloGame;

	[NonSerialized]
	public uint shotId;

	public static void ResetToPool(Pooltable instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.poolBalls != null)
		{
			for (int i = 0; i < instance.poolBalls.Count; i++)
			{
				if (instance.poolBalls[i] != null)
				{
					instance.poolBalls[i].ResetToPool();
					instance.poolBalls[i] = null;
				}
			}
			List<PoolBallData> obj = instance.poolBalls;
			Pool.Free(ref obj, freeElements: false);
			instance.poolBalls = obj;
		}
		instance.state = 0;
		instance.player1Id = 0uL;
		instance.player2Id = 0uL;
		instance.currentPlayer = 0;
		instance.player1Group = 0;
		instance.player2Group = 0;
		instance.isSoloGame = false;
		instance.shotId = 0u;
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
			throw new Exception("Trying to dispose Pooltable with ShouldPool set to false!");
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

	public void CopyTo(Pooltable instance)
	{
		if (poolBalls != null)
		{
			instance.poolBalls = Pool.Get<List<PoolBallData>>();
			for (int i = 0; i < poolBalls.Count; i++)
			{
				PoolBallData item = poolBalls[i].Copy();
				instance.poolBalls.Add(item);
			}
		}
		else
		{
			instance.poolBalls = null;
		}
		instance.state = state;
		instance.player1Id = player1Id;
		instance.player2Id = player2Id;
		instance.currentPlayer = currentPlayer;
		instance.player1Group = player1Group;
		instance.player2Group = player2Group;
		instance.isSoloGame = isSoloGame;
		instance.shotId = shotId;
	}

	public Pooltable Copy()
	{
		Pooltable pooltable = Pool.Get<Pooltable>();
		CopyTo(pooltable);
		return pooltable;
	}

	public static Pooltable Deserialize(BufferStream stream)
	{
		Pooltable pooltable = Pool.Get<Pooltable>();
		Deserialize(stream, pooltable, isDelta: false);
		return pooltable;
	}

	public static Pooltable DeserializeLengthDelimited(BufferStream stream)
	{
		Pooltable pooltable = Pool.Get<Pooltable>();
		DeserializeLengthDelimited(stream, pooltable, isDelta: false);
		return pooltable;
	}

	public static Pooltable DeserializeLength(BufferStream stream, int length)
	{
		Pooltable pooltable = Pool.Get<Pooltable>();
		DeserializeLength(stream, length, pooltable, isDelta: false);
		return pooltable;
	}

	public static Pooltable Deserialize(byte[] buffer)
	{
		Pooltable pooltable = Pool.Get<Pooltable>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, pooltable, isDelta: false);
		return pooltable;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, Pooltable previous)
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

	public static Pooltable Deserialize(BufferStream stream, Pooltable instance, bool isDelta)
	{
		if (!isDelta && instance.poolBalls == null)
		{
			instance.poolBalls = Pool.Get<List<PoolBallData>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Pooltable");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Pooltable");
				stream.ConsumeRepeatedElement();
				instance.poolBalls.Add(PoolBallData.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				instance.state = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				instance.player1Id = ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				instance.player2Id = ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				instance.currentPlayer = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				instance.player1Group = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				instance.player2Group = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				instance.isSoloGame = ProtocolParser.ReadBool(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				instance.shotId = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Pooltable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Pooltable");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Pooltable DeserializeLengthDelimited(BufferStream stream, Pooltable instance, bool isDelta)
	{
		if (!isDelta && instance.poolBalls == null)
		{
			instance.poolBalls = Pool.Get<List<PoolBallData>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Pooltable");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Pooltable");
				stream.ConsumeRepeatedElement();
				instance.poolBalls.Add(PoolBallData.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				instance.state = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				instance.player1Id = ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				instance.player2Id = ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				instance.currentPlayer = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				instance.player1Group = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				instance.player2Group = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				instance.isSoloGame = ProtocolParser.ReadBool(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				instance.shotId = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Pooltable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Pooltable");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Pooltable DeserializeLength(BufferStream stream, int length, Pooltable instance, bool isDelta)
	{
		if (!isDelta && instance.poolBalls == null)
		{
			instance.poolBalls = Pool.Get<List<PoolBallData>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Pooltable");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Pooltable");
				stream.ConsumeRepeatedElement();
				instance.poolBalls.Add(PoolBallData.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				instance.state = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				instance.player1Id = ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				instance.player2Id = ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				instance.currentPlayer = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				instance.player1Group = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				instance.player2Group = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				instance.isSoloGame = ProtocolParser.ReadBool(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				instance.shotId = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Pooltable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.Pooltable");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Pooltable");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, Pooltable instance, Pooltable previous)
	{
		if (instance.poolBalls != null)
		{
			for (int i = 0; i < instance.poolBalls.Count; i++)
			{
				PoolBallData poolBallData = instance.poolBalls[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				PoolBallData.SerializeDelta(stream, poolBallData, poolBallData);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field poolBalls (ProtoBuf.PoolBallData)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.state != previous.state)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.state);
		}
		if (instance.player1Id != previous.player1Id)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, instance.player1Id);
		}
		if (instance.player2Id != previous.player2Id)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, instance.player2Id);
		}
		if (instance.currentPlayer != previous.currentPlayer)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.currentPlayer);
		}
		if (instance.player1Group != previous.player1Group)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.player1Group);
		}
		if (instance.player2Group != previous.player2Group)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.player2Group);
		}
		stream.WriteByte(64);
		ProtocolParser.WriteBool(stream, instance.isSoloGame);
		if (instance.shotId != previous.shotId)
		{
			stream.WriteByte(72);
			ProtocolParser.WriteUInt32(stream, instance.shotId);
		}
	}

	public static void Serialize(BufferStream stream, Pooltable instance)
	{
		if (instance.poolBalls != null)
		{
			for (int i = 0; i < instance.poolBalls.Count; i++)
			{
				PoolBallData instance2 = instance.poolBalls[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				PoolBallData.Serialize(stream, instance2);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field poolBalls (ProtoBuf.PoolBallData)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.state != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.state);
		}
		if (instance.player1Id != 0L)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, instance.player1Id);
		}
		if (instance.player2Id != 0L)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, instance.player2Id);
		}
		if (instance.currentPlayer != 0)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.currentPlayer);
		}
		if (instance.player1Group != 0)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.player1Group);
		}
		if (instance.player2Group != 0)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.player2Group);
		}
		if (instance.isSoloGame)
		{
			stream.WriteByte(64);
			ProtocolParser.WriteBool(stream, instance.isSoloGame);
		}
		if (instance.shotId != 0)
		{
			stream.WriteByte(72);
			ProtocolParser.WriteUInt32(stream, instance.shotId);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (poolBalls != null)
		{
			for (int i = 0; i < poolBalls.Count; i++)
			{
				poolBalls[i]?.InspectUids(action);
			}
		}
	}
}
