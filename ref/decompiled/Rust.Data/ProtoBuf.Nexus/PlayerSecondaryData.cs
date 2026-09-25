using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf.Nexus;

public class PlayerSecondaryData : IDisposable, Pool.IPooled, IProto<PlayerSecondaryData>, IProto
{
	public class RelationshipData : IDisposable, Pool.IPooled, IProto<RelationshipData>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public RelationshipManager.PlayerRelationshipInfo info;

		[NonSerialized]
		public ArraySegment<byte> mugshotData;

		public static void ResetToPool(RelationshipData instance)
		{
			if (instance.ShouldPool)
			{
				if (instance.info != null)
				{
					instance.info.ResetToPool();
					instance.info = null;
				}
				if (instance.mugshotData.Array != null)
				{
					BufferStream.Shared.ArrayPool.Return(instance.mugshotData.Array);
				}
				instance.mugshotData = default(ArraySegment<byte>);
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
				throw new Exception("Trying to dispose RelationshipData with ShouldPool set to false!");
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

		public void CopyTo(RelationshipData instance)
		{
			if (info != null)
			{
				if (instance.info == null)
				{
					instance.info = info.Copy();
				}
				else
				{
					info.CopyTo(instance.info);
				}
			}
			else
			{
				instance.info = null;
			}
			if (mugshotData.Array == null)
			{
				instance.mugshotData = default(ArraySegment<byte>);
				return;
			}
			byte[] array = BufferStream.Shared.ArrayPool.Rent(mugshotData.Count);
			Array.Copy(mugshotData.Array, 0, array, 0, mugshotData.Count);
			instance.mugshotData = new ArraySegment<byte>(array, 0, mugshotData.Count);
		}

		public RelationshipData Copy()
		{
			RelationshipData relationshipData = Pool.Get<RelationshipData>();
			CopyTo(relationshipData);
			return relationshipData;
		}

		public static RelationshipData Deserialize(BufferStream stream)
		{
			RelationshipData relationshipData = Pool.Get<RelationshipData>();
			Deserialize(stream, relationshipData, isDelta: false);
			return relationshipData;
		}

		public static RelationshipData DeserializeLengthDelimited(BufferStream stream)
		{
			RelationshipData relationshipData = Pool.Get<RelationshipData>();
			DeserializeLengthDelimited(stream, relationshipData, isDelta: false);
			return relationshipData;
		}

		public static RelationshipData DeserializeLength(BufferStream stream, int length)
		{
			RelationshipData relationshipData = Pool.Get<RelationshipData>();
			DeserializeLength(stream, length, relationshipData, isDelta: false);
			return relationshipData;
		}

		public static RelationshipData Deserialize(byte[] buffer)
		{
			RelationshipData relationshipData = Pool.Get<RelationshipData>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, relationshipData, isDelta: false);
			return relationshipData;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, RelationshipData previous)
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

		public static RelationshipData Deserialize(BufferStream stream, RelationshipData instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.Nexus.PlayerSecondaryData.RelationshipData");
				switch (num)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData.RelationshipData");
					if (instance.info == null)
					{
						instance.info = RelationshipManager.PlayerRelationshipInfo.DeserializeLengthDelimited(stream);
					}
					else
					{
						RelationshipManager.PlayerRelationshipInfo.DeserializeLengthDelimited(stream, instance.info, isDelta);
					}
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData.RelationshipData");
					instance.mugshotData = ProtocolParser.ReadPooledBytes(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData.RelationshipData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData.RelationshipData");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.PlayerSecondaryData.RelationshipData");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static RelationshipData DeserializeLengthDelimited(BufferStream stream, RelationshipData instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.Nexus.PlayerSecondaryData.RelationshipData");
				switch (num2)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData.RelationshipData");
					if (instance.info == null)
					{
						instance.info = RelationshipManager.PlayerRelationshipInfo.DeserializeLengthDelimited(stream);
					}
					else
					{
						RelationshipManager.PlayerRelationshipInfo.DeserializeLengthDelimited(stream, instance.info, isDelta);
					}
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData.RelationshipData");
					instance.mugshotData = ProtocolParser.ReadPooledBytes(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData.RelationshipData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData.RelationshipData");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.PlayerSecondaryData.RelationshipData");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static RelationshipData DeserializeLength(BufferStream stream, int length, RelationshipData instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.Nexus.PlayerSecondaryData.RelationshipData");
				switch (num2)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData.RelationshipData");
					if (instance.info == null)
					{
						instance.info = RelationshipManager.PlayerRelationshipInfo.DeserializeLengthDelimited(stream);
					}
					else
					{
						RelationshipManager.PlayerRelationshipInfo.DeserializeLengthDelimited(stream, instance.info, isDelta);
					}
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData.RelationshipData");
					instance.mugshotData = ProtocolParser.ReadPooledBytes(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData.RelationshipData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData.RelationshipData");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.PlayerSecondaryData.RelationshipData");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, RelationshipData instance, RelationshipData previous)
		{
			if (instance.info == null)
			{
				throw new ArgumentNullException("info", "Required by proto specification.");
			}
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			RelationshipManager.PlayerRelationshipInfo.SerializeDelta(stream, instance.info, previous.info);
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
			if (instance.mugshotData.Array != null)
			{
				stream.WriteByte(18);
				ProtocolParser.WritePooledBytes(stream, instance.mugshotData);
			}
		}

		public static void Serialize(BufferStream stream, RelationshipData instance)
		{
			if (instance.info == null)
			{
				throw new ArgumentNullException("info", "Required by proto specification.");
			}
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			RelationshipManager.PlayerRelationshipInfo.Serialize(stream, instance.info);
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
			if (instance.mugshotData.Array != null)
			{
				stream.WriteByte(18);
				ProtocolParser.WritePooledBytes(stream, instance.mugshotData);
			}
		}

		public void ToProto(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public void InspectUids(UidInspector<ulong> action)
		{
			info?.InspectUids(action);
		}
	}

	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public ulong userId;

	[NonSerialized]
	public PlayerState playerState;

	[NonSerialized]
	public List<RelationshipData> relationships;

	[NonSerialized]
	public ulong teamId;

	[NonSerialized]
	public bool isTeamLeader;

	public static void ResetToPool(PlayerSecondaryData instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.userId = 0uL;
		if (instance.playerState != null)
		{
			instance.playerState.ResetToPool();
			instance.playerState = null;
		}
		if (instance.relationships != null)
		{
			for (int i = 0; i < instance.relationships.Count; i++)
			{
				if (instance.relationships[i] != null)
				{
					instance.relationships[i].ResetToPool();
					instance.relationships[i] = null;
				}
			}
			List<RelationshipData> obj = instance.relationships;
			Pool.Free(ref obj, freeElements: false);
			instance.relationships = obj;
		}
		instance.teamId = 0uL;
		instance.isTeamLeader = false;
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
			throw new Exception("Trying to dispose PlayerSecondaryData with ShouldPool set to false!");
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

	public void CopyTo(PlayerSecondaryData instance)
	{
		instance.userId = userId;
		if (playerState != null)
		{
			if (instance.playerState == null)
			{
				instance.playerState = playerState.Copy();
			}
			else
			{
				playerState.CopyTo(instance.playerState);
			}
		}
		else
		{
			instance.playerState = null;
		}
		if (relationships != null)
		{
			instance.relationships = Pool.Get<List<RelationshipData>>();
			for (int i = 0; i < relationships.Count; i++)
			{
				RelationshipData item = relationships[i].Copy();
				instance.relationships.Add(item);
			}
		}
		else
		{
			instance.relationships = null;
		}
		instance.teamId = teamId;
		instance.isTeamLeader = isTeamLeader;
	}

	public PlayerSecondaryData Copy()
	{
		PlayerSecondaryData playerSecondaryData = Pool.Get<PlayerSecondaryData>();
		CopyTo(playerSecondaryData);
		return playerSecondaryData;
	}

	public static PlayerSecondaryData Deserialize(BufferStream stream)
	{
		PlayerSecondaryData playerSecondaryData = Pool.Get<PlayerSecondaryData>();
		Deserialize(stream, playerSecondaryData, isDelta: false);
		return playerSecondaryData;
	}

	public static PlayerSecondaryData DeserializeLengthDelimited(BufferStream stream)
	{
		PlayerSecondaryData playerSecondaryData = Pool.Get<PlayerSecondaryData>();
		DeserializeLengthDelimited(stream, playerSecondaryData, isDelta: false);
		return playerSecondaryData;
	}

	public static PlayerSecondaryData DeserializeLength(BufferStream stream, int length)
	{
		PlayerSecondaryData playerSecondaryData = Pool.Get<PlayerSecondaryData>();
		DeserializeLength(stream, length, playerSecondaryData, isDelta: false);
		return playerSecondaryData;
	}

	public static PlayerSecondaryData Deserialize(byte[] buffer)
	{
		PlayerSecondaryData playerSecondaryData = Pool.Get<PlayerSecondaryData>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, playerSecondaryData, isDelta: false);
		return playerSecondaryData;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, PlayerSecondaryData previous)
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

	public static PlayerSecondaryData Deserialize(BufferStream stream, PlayerSecondaryData instance, bool isDelta)
	{
		if (!isDelta && instance.relationships == null)
		{
			instance.relationships = Pool.Get<List<RelationshipData>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.PlayerSecondaryData");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData");
				instance.userId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData");
				if (instance.playerState == null)
				{
					instance.playerState = PlayerState.DeserializeLengthDelimited(stream);
				}
				else
				{
					PlayerState.DeserializeLengthDelimited(stream, instance.playerState, isDelta);
				}
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.Nexus.PlayerSecondaryData");
				stream.ConsumeRepeatedElement();
				instance.relationships.Add(RelationshipData.DeserializeLengthDelimited(stream));
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData");
				instance.teamId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData");
				instance.isTeamLeader = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.Nexus.PlayerSecondaryData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.PlayerSecondaryData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PlayerSecondaryData DeserializeLengthDelimited(BufferStream stream, PlayerSecondaryData instance, bool isDelta)
	{
		if (!isDelta && instance.relationships == null)
		{
			instance.relationships = Pool.Get<List<RelationshipData>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.PlayerSecondaryData");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData");
				instance.userId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData");
				if (instance.playerState == null)
				{
					instance.playerState = PlayerState.DeserializeLengthDelimited(stream);
				}
				else
				{
					PlayerState.DeserializeLengthDelimited(stream, instance.playerState, isDelta);
				}
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.Nexus.PlayerSecondaryData");
				stream.ConsumeRepeatedElement();
				instance.relationships.Add(RelationshipData.DeserializeLengthDelimited(stream));
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData");
				instance.teamId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData");
				instance.isTeamLeader = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.Nexus.PlayerSecondaryData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.PlayerSecondaryData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PlayerSecondaryData DeserializeLength(BufferStream stream, int length, PlayerSecondaryData instance, bool isDelta)
	{
		if (!isDelta && instance.relationships == null)
		{
			instance.relationships = Pool.Get<List<RelationshipData>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.PlayerSecondaryData");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData");
				instance.userId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData");
				if (instance.playerState == null)
				{
					instance.playerState = PlayerState.DeserializeLengthDelimited(stream);
				}
				else
				{
					PlayerState.DeserializeLengthDelimited(stream, instance.playerState, isDelta);
				}
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.Nexus.PlayerSecondaryData");
				stream.ConsumeRepeatedElement();
				instance.relationships.Add(RelationshipData.DeserializeLengthDelimited(stream));
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData");
				instance.teamId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData");
				instance.isTeamLeader = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.Nexus.PlayerSecondaryData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Nexus.PlayerSecondaryData");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.PlayerSecondaryData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, PlayerSecondaryData instance, PlayerSecondaryData previous)
	{
		if (instance.userId != previous.userId)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.userId);
		}
		if (instance.playerState != null)
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			PlayerState.SerializeDelta(stream, instance.playerState, previous.playerState);
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
		if (instance.relationships != null)
		{
			for (int i = 0; i < instance.relationships.Count; i++)
			{
				RelationshipData relationshipData = instance.relationships[i];
				stream.WriteByte(26);
				BufferStream.RangeHandle range2 = stream.GetRange(5);
				int position2 = stream.Position;
				RelationshipData.SerializeDelta(stream, relationshipData, relationshipData);
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
		}
		if (instance.teamId != previous.teamId)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, instance.teamId);
		}
		stream.WriteByte(40);
		ProtocolParser.WriteBool(stream, instance.isTeamLeader);
	}

	public static void Serialize(BufferStream stream, PlayerSecondaryData instance)
	{
		if (instance.userId != 0L)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.userId);
		}
		if (instance.playerState != null)
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			PlayerState.Serialize(stream, instance.playerState);
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
		if (instance.relationships != null)
		{
			for (int i = 0; i < instance.relationships.Count; i++)
			{
				RelationshipData instance2 = instance.relationships[i];
				stream.WriteByte(26);
				BufferStream.RangeHandle range2 = stream.GetRange(5);
				int position2 = stream.Position;
				RelationshipData.Serialize(stream, instance2);
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
		}
		if (instance.teamId != 0L)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, instance.teamId);
		}
		if (instance.isTeamLeader)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteBool(stream, instance.isTeamLeader);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		playerState?.InspectUids(action);
		if (relationships != null)
		{
			for (int i = 0; i < relationships.Count; i++)
			{
				relationships[i]?.InspectUids(action);
			}
		}
	}
}
