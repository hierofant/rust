using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ArcadeMachine : IDisposable, Pool.IPooled, IProto<ArcadeMachine>, IProto
{
	public class ScoreEntry : IDisposable, Pool.IPooled, IProto<ScoreEntry>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public ulong playerID;

		[NonSerialized]
		public string displayName;

		[NonSerialized]
		public int score;

		public static void ResetToPool(ScoreEntry instance)
		{
			if (instance.ShouldPool)
			{
				instance.playerID = 0uL;
				instance.displayName = string.Empty;
				instance.score = 0;
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
				throw new Exception("Trying to dispose ScoreEntry with ShouldPool set to false!");
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

		public void CopyTo(ScoreEntry instance)
		{
			instance.playerID = playerID;
			instance.displayName = displayName;
			instance.score = score;
		}

		public ScoreEntry Copy()
		{
			ScoreEntry scoreEntry = Pool.Get<ScoreEntry>();
			CopyTo(scoreEntry);
			return scoreEntry;
		}

		public static ScoreEntry Deserialize(BufferStream stream)
		{
			ScoreEntry scoreEntry = Pool.Get<ScoreEntry>();
			Deserialize(stream, scoreEntry, isDelta: false);
			return scoreEntry;
		}

		public static ScoreEntry DeserializeLengthDelimited(BufferStream stream)
		{
			ScoreEntry scoreEntry = Pool.Get<ScoreEntry>();
			DeserializeLengthDelimited(stream, scoreEntry, isDelta: false);
			return scoreEntry;
		}

		public static ScoreEntry DeserializeLength(BufferStream stream, int length)
		{
			ScoreEntry scoreEntry = Pool.Get<ScoreEntry>();
			DeserializeLength(stream, length, scoreEntry, isDelta: false);
			return scoreEntry;
		}

		public static ScoreEntry Deserialize(byte[] buffer)
		{
			ScoreEntry scoreEntry = Pool.Get<ScoreEntry>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, scoreEntry, isDelta: false);
			return scoreEntry;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, ScoreEntry previous)
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

		public static ScoreEntry Deserialize(BufferStream stream, ScoreEntry instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.ArcadeMachine.ScoreEntry");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine.ScoreEntry");
					instance.playerID = ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine.ScoreEntry");
					instance.displayName = ProtocolParser.ReadString(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine.ScoreEntry");
					instance.score = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine.ScoreEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine.ScoreEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine.ScoreEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ArcadeMachine.ScoreEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static ScoreEntry DeserializeLengthDelimited(BufferStream stream, ScoreEntry instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.ArcadeMachine.ScoreEntry");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine.ScoreEntry");
					instance.playerID = ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine.ScoreEntry");
					instance.displayName = ProtocolParser.ReadString(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine.ScoreEntry");
					instance.score = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine.ScoreEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine.ScoreEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine.ScoreEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ArcadeMachine.ScoreEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static ScoreEntry DeserializeLength(BufferStream stream, int length, ScoreEntry instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.ArcadeMachine.ScoreEntry");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine.ScoreEntry");
					instance.playerID = ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine.ScoreEntry");
					instance.displayName = ProtocolParser.ReadString(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine.ScoreEntry");
					instance.score = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine.ScoreEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine.ScoreEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine.ScoreEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ArcadeMachine.ScoreEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, ScoreEntry instance, ScoreEntry previous)
		{
			if (instance.playerID != previous.playerID)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, instance.playerID);
			}
			if (instance.displayName != null && instance.displayName != previous.displayName)
			{
				stream.WriteByte(18);
				ProtocolParser.WriteString(stream, instance.displayName);
			}
			if (instance.score != previous.score)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.score);
			}
		}

		public static void Serialize(BufferStream stream, ScoreEntry instance)
		{
			if (instance.playerID != 0L)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, instance.playerID);
			}
			if (instance.displayName != null)
			{
				stream.WriteByte(18);
				ProtocolParser.WriteString(stream, instance.displayName);
			}
			if (instance.score != 0)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.score);
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

	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<ScoreEntry> scores;

	[NonSerialized]
	public int genericInt1;

	[NonSerialized]
	public int genericInt2;

	[NonSerialized]
	public int genericInt3;

	[NonSerialized]
	public int genericInt4;

	[NonSerialized]
	public float genericFloat1;

	[NonSerialized]
	public float genericFloat2;

	[NonSerialized]
	public float genericFloat3;

	[NonSerialized]
	public float genericFloat4;

	public static void ResetToPool(ArcadeMachine instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.scores != null)
		{
			for (int i = 0; i < instance.scores.Count; i++)
			{
				if (instance.scores[i] != null)
				{
					instance.scores[i].ResetToPool();
					instance.scores[i] = null;
				}
			}
			List<ScoreEntry> obj = instance.scores;
			Pool.Free(ref obj, freeElements: false);
			instance.scores = obj;
		}
		instance.genericInt1 = 0;
		instance.genericInt2 = 0;
		instance.genericInt3 = 0;
		instance.genericInt4 = 0;
		instance.genericFloat1 = 0f;
		instance.genericFloat2 = 0f;
		instance.genericFloat3 = 0f;
		instance.genericFloat4 = 0f;
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
			throw new Exception("Trying to dispose ArcadeMachine with ShouldPool set to false!");
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

	public void CopyTo(ArcadeMachine instance)
	{
		if (scores != null)
		{
			instance.scores = Pool.Get<List<ScoreEntry>>();
			for (int i = 0; i < scores.Count; i++)
			{
				ScoreEntry item = scores[i].Copy();
				instance.scores.Add(item);
			}
		}
		else
		{
			instance.scores = null;
		}
		instance.genericInt1 = genericInt1;
		instance.genericInt2 = genericInt2;
		instance.genericInt3 = genericInt3;
		instance.genericInt4 = genericInt4;
		instance.genericFloat1 = genericFloat1;
		instance.genericFloat2 = genericFloat2;
		instance.genericFloat3 = genericFloat3;
		instance.genericFloat4 = genericFloat4;
	}

	public ArcadeMachine Copy()
	{
		ArcadeMachine arcadeMachine = Pool.Get<ArcadeMachine>();
		CopyTo(arcadeMachine);
		return arcadeMachine;
	}

	public static ArcadeMachine Deserialize(BufferStream stream)
	{
		ArcadeMachine arcadeMachine = Pool.Get<ArcadeMachine>();
		Deserialize(stream, arcadeMachine, isDelta: false);
		return arcadeMachine;
	}

	public static ArcadeMachine DeserializeLengthDelimited(BufferStream stream)
	{
		ArcadeMachine arcadeMachine = Pool.Get<ArcadeMachine>();
		DeserializeLengthDelimited(stream, arcadeMachine, isDelta: false);
		return arcadeMachine;
	}

	public static ArcadeMachine DeserializeLength(BufferStream stream, int length)
	{
		ArcadeMachine arcadeMachine = Pool.Get<ArcadeMachine>();
		DeserializeLength(stream, length, arcadeMachine, isDelta: false);
		return arcadeMachine;
	}

	public static ArcadeMachine Deserialize(byte[] buffer)
	{
		ArcadeMachine arcadeMachine = Pool.Get<ArcadeMachine>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, arcadeMachine, isDelta: false);
		return arcadeMachine;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ArcadeMachine previous)
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

	public static ArcadeMachine Deserialize(BufferStream stream, ArcadeMachine instance, bool isDelta)
	{
		if (!isDelta && instance.scores == null)
		{
			instance.scores = Pool.Get<List<ScoreEntry>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ArcadeMachine");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ArcadeMachine");
				stream.ConsumeRepeatedElement();
				instance.scores.Add(ScoreEntry.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				instance.genericInt1 = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				instance.genericInt2 = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				instance.genericInt3 = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				instance.genericInt4 = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				instance.genericFloat1 = ProtocolParser.ReadSingle(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				instance.genericFloat2 = ProtocolParser.ReadSingle(stream);
				continue;
			case 69:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				instance.genericFloat3 = ProtocolParser.ReadSingle(stream);
				continue;
			case 77:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				instance.genericFloat4 = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ArcadeMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ArcadeMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ArcadeMachine DeserializeLengthDelimited(BufferStream stream, ArcadeMachine instance, bool isDelta)
	{
		if (!isDelta && instance.scores == null)
		{
			instance.scores = Pool.Get<List<ScoreEntry>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ArcadeMachine");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ArcadeMachine");
				stream.ConsumeRepeatedElement();
				instance.scores.Add(ScoreEntry.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				instance.genericInt1 = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				instance.genericInt2 = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				instance.genericInt3 = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				instance.genericInt4 = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				instance.genericFloat1 = ProtocolParser.ReadSingle(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				instance.genericFloat2 = ProtocolParser.ReadSingle(stream);
				continue;
			case 69:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				instance.genericFloat3 = ProtocolParser.ReadSingle(stream);
				continue;
			case 77:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				instance.genericFloat4 = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ArcadeMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ArcadeMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ArcadeMachine DeserializeLength(BufferStream stream, int length, ArcadeMachine instance, bool isDelta)
	{
		if (!isDelta && instance.scores == null)
		{
			instance.scores = Pool.Get<List<ScoreEntry>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ArcadeMachine");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ArcadeMachine");
				stream.ConsumeRepeatedElement();
				instance.scores.Add(ScoreEntry.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				instance.genericInt1 = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				instance.genericInt2 = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				instance.genericInt3 = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				instance.genericInt4 = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				instance.genericFloat1 = ProtocolParser.ReadSingle(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				instance.genericFloat2 = ProtocolParser.ReadSingle(stream);
				continue;
			case 69:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				instance.genericFloat3 = ProtocolParser.ReadSingle(stream);
				continue;
			case 77:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				instance.genericFloat4 = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ArcadeMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ArcadeMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ArcadeMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ArcadeMachine instance, ArcadeMachine previous)
	{
		if (instance.scores != null)
		{
			for (int i = 0; i < instance.scores.Count; i++)
			{
				ScoreEntry scoreEntry = instance.scores[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				ScoreEntry.SerializeDelta(stream, scoreEntry, scoreEntry);
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
		if (instance.genericInt1 != previous.genericInt1)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.genericInt1);
		}
		if (instance.genericInt2 != previous.genericInt2)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.genericInt2);
		}
		if (instance.genericInt3 != previous.genericInt3)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.genericInt3);
		}
		if (instance.genericInt4 != previous.genericInt4)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.genericInt4);
		}
		if (instance.genericFloat1 != previous.genericFloat1)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.genericFloat1);
		}
		if (instance.genericFloat2 != previous.genericFloat2)
		{
			stream.WriteByte(61);
			ProtocolParser.WriteSingle(stream, instance.genericFloat2);
		}
		if (instance.genericFloat3 != previous.genericFloat3)
		{
			stream.WriteByte(69);
			ProtocolParser.WriteSingle(stream, instance.genericFloat3);
		}
		if (instance.genericFloat4 != previous.genericFloat4)
		{
			stream.WriteByte(77);
			ProtocolParser.WriteSingle(stream, instance.genericFloat4);
		}
	}

	public static void Serialize(BufferStream stream, ArcadeMachine instance)
	{
		if (instance.scores != null)
		{
			for (int i = 0; i < instance.scores.Count; i++)
			{
				ScoreEntry instance2 = instance.scores[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				ScoreEntry.Serialize(stream, instance2);
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
		if (instance.genericInt1 != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.genericInt1);
		}
		if (instance.genericInt2 != 0)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.genericInt2);
		}
		if (instance.genericInt3 != 0)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.genericInt3);
		}
		if (instance.genericInt4 != 0)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.genericInt4);
		}
		if (instance.genericFloat1 != 0f)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.genericFloat1);
		}
		if (instance.genericFloat2 != 0f)
		{
			stream.WriteByte(61);
			ProtocolParser.WriteSingle(stream, instance.genericFloat2);
		}
		if (instance.genericFloat3 != 0f)
		{
			stream.WriteByte(69);
			ProtocolParser.WriteSingle(stream, instance.genericFloat3);
		}
		if (instance.genericFloat4 != 0f)
		{
			stream.WriteByte(77);
			ProtocolParser.WriteSingle(stream, instance.genericFloat4);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (scores != null)
		{
			for (int i = 0; i < scores.Count; i++)
			{
				scores[i]?.InspectUids(action);
			}
		}
	}
}
