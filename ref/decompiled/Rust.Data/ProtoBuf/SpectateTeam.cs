using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class SpectateTeam : IDisposable, Pool.IPooled, IProto<SpectateTeam>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public ulong teamId;

	[NonSerialized]
	public List<PlayerTeam.TeamMember> teamMembers;

	public static void ResetToPool(SpectateTeam instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.teamId = 0uL;
		if (instance.teamMembers != null)
		{
			for (int i = 0; i < instance.teamMembers.Count; i++)
			{
				if (instance.teamMembers[i] != null)
				{
					instance.teamMembers[i].ResetToPool();
					instance.teamMembers[i] = null;
				}
			}
			List<PlayerTeam.TeamMember> obj = instance.teamMembers;
			Pool.Free(ref obj, freeElements: false);
			instance.teamMembers = obj;
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
			throw new Exception("Trying to dispose SpectateTeam with ShouldPool set to false!");
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

	public void CopyTo(SpectateTeam instance)
	{
		instance.teamId = teamId;
		if (teamMembers != null)
		{
			instance.teamMembers = Pool.Get<List<PlayerTeam.TeamMember>>();
			for (int i = 0; i < teamMembers.Count; i++)
			{
				PlayerTeam.TeamMember item = teamMembers[i].Copy();
				instance.teamMembers.Add(item);
			}
		}
		else
		{
			instance.teamMembers = null;
		}
	}

	public SpectateTeam Copy()
	{
		SpectateTeam spectateTeam = Pool.Get<SpectateTeam>();
		CopyTo(spectateTeam);
		return spectateTeam;
	}

	public static SpectateTeam Deserialize(BufferStream stream)
	{
		SpectateTeam spectateTeam = Pool.Get<SpectateTeam>();
		Deserialize(stream, spectateTeam, isDelta: false);
		return spectateTeam;
	}

	public static SpectateTeam DeserializeLengthDelimited(BufferStream stream)
	{
		SpectateTeam spectateTeam = Pool.Get<SpectateTeam>();
		DeserializeLengthDelimited(stream, spectateTeam, isDelta: false);
		return spectateTeam;
	}

	public static SpectateTeam DeserializeLength(BufferStream stream, int length)
	{
		SpectateTeam spectateTeam = Pool.Get<SpectateTeam>();
		DeserializeLength(stream, length, spectateTeam, isDelta: false);
		return spectateTeam;
	}

	public static SpectateTeam Deserialize(byte[] buffer)
	{
		SpectateTeam spectateTeam = Pool.Get<SpectateTeam>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, spectateTeam, isDelta: false);
		return spectateTeam;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, SpectateTeam previous)
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

	public static SpectateTeam Deserialize(BufferStream stream, SpectateTeam instance, bool isDelta)
	{
		if (!isDelta && instance.teamMembers == null)
		{
			instance.teamMembers = Pool.Get<List<PlayerTeam.TeamMember>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.SpectateTeam");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SpectateTeam");
				instance.teamId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.SpectateTeam");
				stream.ConsumeRepeatedElement();
				instance.teamMembers.Add(PlayerTeam.TeamMember.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SpectateTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.SpectateTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SpectateTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static SpectateTeam DeserializeLengthDelimited(BufferStream stream, SpectateTeam instance, bool isDelta)
	{
		if (!isDelta && instance.teamMembers == null)
		{
			instance.teamMembers = Pool.Get<List<PlayerTeam.TeamMember>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.SpectateTeam");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SpectateTeam");
				instance.teamId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.SpectateTeam");
				stream.ConsumeRepeatedElement();
				instance.teamMembers.Add(PlayerTeam.TeamMember.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SpectateTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.SpectateTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SpectateTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static SpectateTeam DeserializeLength(BufferStream stream, int length, SpectateTeam instance, bool isDelta)
	{
		if (!isDelta && instance.teamMembers == null)
		{
			instance.teamMembers = Pool.Get<List<PlayerTeam.TeamMember>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.SpectateTeam");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SpectateTeam");
				instance.teamId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.SpectateTeam");
				stream.ConsumeRepeatedElement();
				instance.teamMembers.Add(PlayerTeam.TeamMember.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SpectateTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.SpectateTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SpectateTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, SpectateTeam instance, SpectateTeam previous)
	{
		if (instance.teamId != previous.teamId)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.teamId);
		}
		if (instance.teamMembers == null)
		{
			return;
		}
		for (int i = 0; i < instance.teamMembers.Count; i++)
		{
			PlayerTeam.TeamMember teamMember = instance.teamMembers[i];
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			PlayerTeam.TeamMember.SerializeDelta(stream, teamMember, teamMember);
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

	public static void Serialize(BufferStream stream, SpectateTeam instance)
	{
		if (instance.teamId != 0L)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.teamId);
		}
		if (instance.teamMembers == null)
		{
			return;
		}
		for (int i = 0; i < instance.teamMembers.Count; i++)
		{
			PlayerTeam.TeamMember instance2 = instance.teamMembers[i];
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			PlayerTeam.TeamMember.Serialize(stream, instance2);
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
		if (teamMembers != null)
		{
			for (int i = 0; i < teamMembers.Count; i++)
			{
				teamMembers[i]?.InspectUids(action);
			}
		}
	}
}
