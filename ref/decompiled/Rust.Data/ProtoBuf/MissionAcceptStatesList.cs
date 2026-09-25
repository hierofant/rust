using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class MissionAcceptStatesList : IDisposable, Pool.IPooled, IProto<MissionAcceptStatesList>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<MissionAcceptState> missionAcceptStates;

	public static void ResetToPool(MissionAcceptStatesList instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.missionAcceptStates != null)
		{
			for (int i = 0; i < instance.missionAcceptStates.Count; i++)
			{
				if (instance.missionAcceptStates[i] != null)
				{
					instance.missionAcceptStates[i].ResetToPool();
					instance.missionAcceptStates[i] = null;
				}
			}
			List<MissionAcceptState> obj = instance.missionAcceptStates;
			Pool.Free(ref obj, freeElements: false);
			instance.missionAcceptStates = obj;
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
			throw new Exception("Trying to dispose MissionAcceptStatesList with ShouldPool set to false!");
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

	public void CopyTo(MissionAcceptStatesList instance)
	{
		if (missionAcceptStates != null)
		{
			instance.missionAcceptStates = Pool.Get<List<MissionAcceptState>>();
			for (int i = 0; i < missionAcceptStates.Count; i++)
			{
				MissionAcceptState item = missionAcceptStates[i].Copy();
				instance.missionAcceptStates.Add(item);
			}
		}
		else
		{
			instance.missionAcceptStates = null;
		}
	}

	public MissionAcceptStatesList Copy()
	{
		MissionAcceptStatesList missionAcceptStatesList = Pool.Get<MissionAcceptStatesList>();
		CopyTo(missionAcceptStatesList);
		return missionAcceptStatesList;
	}

	public static MissionAcceptStatesList Deserialize(BufferStream stream)
	{
		MissionAcceptStatesList missionAcceptStatesList = Pool.Get<MissionAcceptStatesList>();
		Deserialize(stream, missionAcceptStatesList, isDelta: false);
		return missionAcceptStatesList;
	}

	public static MissionAcceptStatesList DeserializeLengthDelimited(BufferStream stream)
	{
		MissionAcceptStatesList missionAcceptStatesList = Pool.Get<MissionAcceptStatesList>();
		DeserializeLengthDelimited(stream, missionAcceptStatesList, isDelta: false);
		return missionAcceptStatesList;
	}

	public static MissionAcceptStatesList DeserializeLength(BufferStream stream, int length)
	{
		MissionAcceptStatesList missionAcceptStatesList = Pool.Get<MissionAcceptStatesList>();
		DeserializeLength(stream, length, missionAcceptStatesList, isDelta: false);
		return missionAcceptStatesList;
	}

	public static MissionAcceptStatesList Deserialize(byte[] buffer)
	{
		MissionAcceptStatesList missionAcceptStatesList = Pool.Get<MissionAcceptStatesList>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, missionAcceptStatesList, isDelta: false);
		return missionAcceptStatesList;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, MissionAcceptStatesList previous)
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

	public static MissionAcceptStatesList Deserialize(BufferStream stream, MissionAcceptStatesList instance, bool isDelta)
	{
		if (!isDelta && instance.missionAcceptStates == null)
		{
			instance.missionAcceptStates = Pool.Get<List<MissionAcceptState>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.MissionAcceptStatesList");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.MissionAcceptStatesList");
				stream.ConsumeRepeatedElement();
				instance.missionAcceptStates.Add(MissionAcceptState.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.MissionAcceptStatesList");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MissionAcceptStatesList");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static MissionAcceptStatesList DeserializeLengthDelimited(BufferStream stream, MissionAcceptStatesList instance, bool isDelta)
	{
		if (!isDelta && instance.missionAcceptStates == null)
		{
			instance.missionAcceptStates = Pool.Get<List<MissionAcceptState>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.MissionAcceptStatesList");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.MissionAcceptStatesList");
				stream.ConsumeRepeatedElement();
				instance.missionAcceptStates.Add(MissionAcceptState.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.MissionAcceptStatesList");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MissionAcceptStatesList");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static MissionAcceptStatesList DeserializeLength(BufferStream stream, int length, MissionAcceptStatesList instance, bool isDelta)
	{
		if (!isDelta && instance.missionAcceptStates == null)
		{
			instance.missionAcceptStates = Pool.Get<List<MissionAcceptState>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.MissionAcceptStatesList");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.MissionAcceptStatesList");
				stream.ConsumeRepeatedElement();
				instance.missionAcceptStates.Add(MissionAcceptState.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.MissionAcceptStatesList");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MissionAcceptStatesList");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, MissionAcceptStatesList instance, MissionAcceptStatesList previous)
	{
		if (instance.missionAcceptStates == null)
		{
			return;
		}
		for (int i = 0; i < instance.missionAcceptStates.Count; i++)
		{
			MissionAcceptState missionAcceptState = instance.missionAcceptStates[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			MissionAcceptState.SerializeDelta(stream, missionAcceptState, missionAcceptState);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field missionAcceptStates (ProtoBuf.MissionAcceptState)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public static void Serialize(BufferStream stream, MissionAcceptStatesList instance)
	{
		if (instance.missionAcceptStates == null)
		{
			return;
		}
		for (int i = 0; i < instance.missionAcceptStates.Count; i++)
		{
			MissionAcceptState instance2 = instance.missionAcceptStates[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			MissionAcceptState.Serialize(stream, instance2);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field missionAcceptStates (ProtoBuf.MissionAcceptState)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (missionAcceptStates != null)
		{
			for (int i = 0; i < missionAcceptStates.Count; i++)
			{
				missionAcceptStates[i]?.InspectUids(action);
			}
		}
	}
}
