using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class TerritoryZones : IDisposable, Pool.IPooled, IProto<TerritoryZones>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public float hexSize;

	[NonSerialized]
	public byte[] cellFactions;

	[NonSerialized]
	public List<uint> factionColors;

	[NonSerialized]
	public List<string> factionNames;

	[NonSerialized]
	public float offsetX;

	[NonSerialized]
	public float offsetZ;

	public static void ResetToPool(TerritoryZones instance)
	{
		if (instance.ShouldPool)
		{
			instance.hexSize = 0f;
			instance.cellFactions = null;
			if (instance.factionColors != null)
			{
				List<uint> obj = instance.factionColors;
				Pool.FreeUnmanaged(ref obj);
				instance.factionColors = obj;
			}
			if (instance.factionNames != null)
			{
				List<string> obj2 = instance.factionNames;
				Pool.FreeUnmanaged(ref obj2);
				instance.factionNames = obj2;
			}
			instance.offsetX = 0f;
			instance.offsetZ = 0f;
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
			throw new Exception("Trying to dispose TerritoryZones with ShouldPool set to false!");
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

	public void CopyTo(TerritoryZones instance)
	{
		instance.hexSize = hexSize;
		if (cellFactions == null)
		{
			instance.cellFactions = null;
		}
		else
		{
			instance.cellFactions = new byte[cellFactions.Length];
			Array.Copy(cellFactions, instance.cellFactions, instance.cellFactions.Length);
		}
		if (factionColors != null)
		{
			instance.factionColors = Pool.Get<List<uint>>();
			for (int i = 0; i < factionColors.Count; i++)
			{
				uint item = factionColors[i];
				instance.factionColors.Add(item);
			}
		}
		else
		{
			instance.factionColors = null;
		}
		if (factionNames != null)
		{
			instance.factionNames = Pool.Get<List<string>>();
			for (int j = 0; j < factionNames.Count; j++)
			{
				string item2 = factionNames[j];
				instance.factionNames.Add(item2);
			}
		}
		else
		{
			instance.factionNames = null;
		}
		instance.offsetX = offsetX;
		instance.offsetZ = offsetZ;
	}

	public TerritoryZones Copy()
	{
		TerritoryZones territoryZones = Pool.Get<TerritoryZones>();
		CopyTo(territoryZones);
		return territoryZones;
	}

	public static TerritoryZones Deserialize(BufferStream stream)
	{
		TerritoryZones territoryZones = Pool.Get<TerritoryZones>();
		Deserialize(stream, territoryZones, isDelta: false);
		return territoryZones;
	}

	public static TerritoryZones DeserializeLengthDelimited(BufferStream stream)
	{
		TerritoryZones territoryZones = Pool.Get<TerritoryZones>();
		DeserializeLengthDelimited(stream, territoryZones, isDelta: false);
		return territoryZones;
	}

	public static TerritoryZones DeserializeLength(BufferStream stream, int length)
	{
		TerritoryZones territoryZones = Pool.Get<TerritoryZones>();
		DeserializeLength(stream, length, territoryZones, isDelta: false);
		return territoryZones;
	}

	public static TerritoryZones Deserialize(byte[] buffer)
	{
		TerritoryZones territoryZones = Pool.Get<TerritoryZones>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, territoryZones, isDelta: false);
		return territoryZones;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, TerritoryZones previous)
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

	public static TerritoryZones Deserialize(BufferStream stream, TerritoryZones instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.factionColors == null)
			{
				instance.factionColors = Pool.Get<List<uint>>();
			}
			if (instance.factionNames == null)
			{
				instance.factionNames = Pool.Get<List<string>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.TerritoryZones");
			switch (num)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TerritoryZones");
				instance.hexSize = ProtocolParser.ReadSingle(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TerritoryZones");
				instance.cellFactions = ProtocolParser.ReadBytes(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.TerritoryZones");
				stream.ConsumeRepeatedElement();
				instance.factionColors.Add(ProtocolParser.ReadUInt32(stream));
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.TerritoryZones");
				stream.ConsumeRepeatedElement();
				instance.factionNames.Add(ProtocolParser.ReadString(stream));
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.TerritoryZones");
				instance.offsetX = ProtocolParser.ReadSingle(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.TerritoryZones");
				instance.offsetZ = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TerritoryZones");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TerritoryZones");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.TerritoryZones");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.TerritoryZones");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.TerritoryZones");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.TerritoryZones");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.TerritoryZones");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static TerritoryZones DeserializeLengthDelimited(BufferStream stream, TerritoryZones instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.factionColors == null)
			{
				instance.factionColors = Pool.Get<List<uint>>();
			}
			if (instance.factionNames == null)
			{
				instance.factionNames = Pool.Get<List<string>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.TerritoryZones");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TerritoryZones");
				instance.hexSize = ProtocolParser.ReadSingle(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TerritoryZones");
				instance.cellFactions = ProtocolParser.ReadBytes(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.TerritoryZones");
				stream.ConsumeRepeatedElement();
				instance.factionColors.Add(ProtocolParser.ReadUInt32(stream));
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.TerritoryZones");
				stream.ConsumeRepeatedElement();
				instance.factionNames.Add(ProtocolParser.ReadString(stream));
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.TerritoryZones");
				instance.offsetX = ProtocolParser.ReadSingle(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.TerritoryZones");
				instance.offsetZ = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TerritoryZones");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TerritoryZones");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.TerritoryZones");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.TerritoryZones");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.TerritoryZones");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.TerritoryZones");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.TerritoryZones");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static TerritoryZones DeserializeLength(BufferStream stream, int length, TerritoryZones instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.factionColors == null)
			{
				instance.factionColors = Pool.Get<List<uint>>();
			}
			if (instance.factionNames == null)
			{
				instance.factionNames = Pool.Get<List<string>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.TerritoryZones");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TerritoryZones");
				instance.hexSize = ProtocolParser.ReadSingle(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TerritoryZones");
				instance.cellFactions = ProtocolParser.ReadBytes(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.TerritoryZones");
				stream.ConsumeRepeatedElement();
				instance.factionColors.Add(ProtocolParser.ReadUInt32(stream));
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.TerritoryZones");
				stream.ConsumeRepeatedElement();
				instance.factionNames.Add(ProtocolParser.ReadString(stream));
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.TerritoryZones");
				instance.offsetX = ProtocolParser.ReadSingle(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.TerritoryZones");
				instance.offsetZ = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TerritoryZones");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TerritoryZones");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.TerritoryZones");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.TerritoryZones");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.TerritoryZones");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.TerritoryZones");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.TerritoryZones");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, TerritoryZones instance, TerritoryZones previous)
	{
		if (instance.hexSize != previous.hexSize)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.hexSize);
		}
		if (instance.cellFactions != null)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteBytes(stream, instance.cellFactions);
		}
		if (instance.factionColors != null)
		{
			for (int i = 0; i < instance.factionColors.Count; i++)
			{
				uint val = instance.factionColors[i];
				stream.WriteByte(24);
				ProtocolParser.WriteUInt32(stream, val);
			}
		}
		if (instance.factionNames != null)
		{
			for (int j = 0; j < instance.factionNames.Count; j++)
			{
				string val2 = instance.factionNames[j];
				stream.WriteByte(34);
				ProtocolParser.WriteString(stream, val2);
			}
		}
		if (instance.offsetX != previous.offsetX)
		{
			stream.WriteByte(45);
			ProtocolParser.WriteSingle(stream, instance.offsetX);
		}
		if (instance.offsetZ != previous.offsetZ)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.offsetZ);
		}
	}

	public static void Serialize(BufferStream stream, TerritoryZones instance)
	{
		if (instance.hexSize != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.hexSize);
		}
		if (instance.cellFactions != null)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteBytes(stream, instance.cellFactions);
		}
		if (instance.factionColors != null)
		{
			for (int i = 0; i < instance.factionColors.Count; i++)
			{
				uint val = instance.factionColors[i];
				stream.WriteByte(24);
				ProtocolParser.WriteUInt32(stream, val);
			}
		}
		if (instance.factionNames != null)
		{
			for (int j = 0; j < instance.factionNames.Count; j++)
			{
				string val2 = instance.factionNames[j];
				stream.WriteByte(34);
				ProtocolParser.WriteString(stream, val2);
			}
		}
		if (instance.offsetX != 0f)
		{
			stream.WriteByte(45);
			ProtocolParser.WriteSingle(stream, instance.offsetX);
		}
		if (instance.offsetZ != 0f)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.offsetZ);
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
