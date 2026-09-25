using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class SpectateTeamInfo : IDisposable, Pool.IPooled, IProto<SpectateTeamInfo>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<SpectateTeam> teams;

	public static void ResetToPool(SpectateTeamInfo instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.teams != null)
		{
			for (int i = 0; i < instance.teams.Count; i++)
			{
				if (instance.teams[i] != null)
				{
					instance.teams[i].ResetToPool();
					instance.teams[i] = null;
				}
			}
			List<SpectateTeam> obj = instance.teams;
			Pool.Free(ref obj, freeElements: false);
			instance.teams = obj;
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
			throw new Exception("Trying to dispose SpectateTeamInfo with ShouldPool set to false!");
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

	public void CopyTo(SpectateTeamInfo instance)
	{
		if (teams != null)
		{
			instance.teams = Pool.Get<List<SpectateTeam>>();
			for (int i = 0; i < teams.Count; i++)
			{
				SpectateTeam item = teams[i].Copy();
				instance.teams.Add(item);
			}
		}
		else
		{
			instance.teams = null;
		}
	}

	public SpectateTeamInfo Copy()
	{
		SpectateTeamInfo spectateTeamInfo = Pool.Get<SpectateTeamInfo>();
		CopyTo(spectateTeamInfo);
		return spectateTeamInfo;
	}

	public static SpectateTeamInfo Deserialize(BufferStream stream)
	{
		SpectateTeamInfo spectateTeamInfo = Pool.Get<SpectateTeamInfo>();
		Deserialize(stream, spectateTeamInfo, isDelta: false);
		return spectateTeamInfo;
	}

	public static SpectateTeamInfo DeserializeLengthDelimited(BufferStream stream)
	{
		SpectateTeamInfo spectateTeamInfo = Pool.Get<SpectateTeamInfo>();
		DeserializeLengthDelimited(stream, spectateTeamInfo, isDelta: false);
		return spectateTeamInfo;
	}

	public static SpectateTeamInfo DeserializeLength(BufferStream stream, int length)
	{
		SpectateTeamInfo spectateTeamInfo = Pool.Get<SpectateTeamInfo>();
		DeserializeLength(stream, length, spectateTeamInfo, isDelta: false);
		return spectateTeamInfo;
	}

	public static SpectateTeamInfo Deserialize(byte[] buffer)
	{
		SpectateTeamInfo spectateTeamInfo = Pool.Get<SpectateTeamInfo>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, spectateTeamInfo, isDelta: false);
		return spectateTeamInfo;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, SpectateTeamInfo previous)
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

	public static SpectateTeamInfo Deserialize(BufferStream stream, SpectateTeamInfo instance, bool isDelta)
	{
		if (!isDelta && instance.teams == null)
		{
			instance.teams = Pool.Get<List<SpectateTeam>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.SpectateTeamInfo");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.SpectateTeamInfo");
				stream.ConsumeRepeatedElement();
				instance.teams.Add(SpectateTeam.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.SpectateTeamInfo");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SpectateTeamInfo");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static SpectateTeamInfo DeserializeLengthDelimited(BufferStream stream, SpectateTeamInfo instance, bool isDelta)
	{
		if (!isDelta && instance.teams == null)
		{
			instance.teams = Pool.Get<List<SpectateTeam>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.SpectateTeamInfo");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.SpectateTeamInfo");
				stream.ConsumeRepeatedElement();
				instance.teams.Add(SpectateTeam.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.SpectateTeamInfo");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SpectateTeamInfo");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static SpectateTeamInfo DeserializeLength(BufferStream stream, int length, SpectateTeamInfo instance, bool isDelta)
	{
		if (!isDelta && instance.teams == null)
		{
			instance.teams = Pool.Get<List<SpectateTeam>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.SpectateTeamInfo");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.SpectateTeamInfo");
				stream.ConsumeRepeatedElement();
				instance.teams.Add(SpectateTeam.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.SpectateTeamInfo");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SpectateTeamInfo");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, SpectateTeamInfo instance, SpectateTeamInfo previous)
	{
		if (instance.teams == null)
		{
			return;
		}
		for (int i = 0; i < instance.teams.Count; i++)
		{
			SpectateTeam spectateTeam = instance.teams[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			SpectateTeam.SerializeDelta(stream, spectateTeam, spectateTeam);
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

	public static void Serialize(BufferStream stream, SpectateTeamInfo instance)
	{
		if (instance.teams == null)
		{
			return;
		}
		for (int i = 0; i < instance.teams.Count; i++)
		{
			SpectateTeam instance2 = instance.teams[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			SpectateTeam.Serialize(stream, instance2);
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
		if (teams != null)
		{
			for (int i = 0; i < teams.Count; i++)
			{
				teams[i]?.InspectUids(action);
			}
		}
	}
}
