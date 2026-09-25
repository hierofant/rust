using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class SatelliteControlComputer : IDisposable, Pool.IPooled, IProto<SatelliteControlComputer>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public float cooldownRemaining;

	[NonSerialized]
	public float controlRemaining;

	[NonSerialized]
	public int selectedSatelliteIndex;

	[NonSerialized]
	public int fuelRemaining;

	[NonSerialized]
	public bool isDescending;

	[NonSerialized]
	public float descentRemaining;

	[NonSerialized]
	public Vector3 finalCrashPos;

	[NonSerialized]
	public float finalCrashRadius;

	[NonSerialized]
	public ulong controllingPlayerId;

	[NonSerialized]
	public Vector3 targetingCenter;

	[NonSerialized]
	public float targetingRadius;

	[NonSerialized]
	public int satelliteSeed;

	[NonSerialized]
	public List<int> resolvedPowerCost;

	public static void ResetToPool(SatelliteControlComputer instance)
	{
		if (instance.ShouldPool)
		{
			instance.cooldownRemaining = 0f;
			instance.controlRemaining = 0f;
			instance.selectedSatelliteIndex = 0;
			instance.fuelRemaining = 0;
			instance.isDescending = false;
			instance.descentRemaining = 0f;
			instance.finalCrashPos = default(Vector3);
			instance.finalCrashRadius = 0f;
			instance.controllingPlayerId = 0uL;
			instance.targetingCenter = default(Vector3);
			instance.targetingRadius = 0f;
			instance.satelliteSeed = 0;
			if (instance.resolvedPowerCost != null)
			{
				List<int> obj = instance.resolvedPowerCost;
				Pool.FreeUnmanaged(ref obj);
				instance.resolvedPowerCost = obj;
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
			throw new Exception("Trying to dispose SatelliteControlComputer with ShouldPool set to false!");
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

	public void CopyTo(SatelliteControlComputer instance)
	{
		instance.cooldownRemaining = cooldownRemaining;
		instance.controlRemaining = controlRemaining;
		instance.selectedSatelliteIndex = selectedSatelliteIndex;
		instance.fuelRemaining = fuelRemaining;
		instance.isDescending = isDescending;
		instance.descentRemaining = descentRemaining;
		instance.finalCrashPos = finalCrashPos;
		instance.finalCrashRadius = finalCrashRadius;
		instance.controllingPlayerId = controllingPlayerId;
		instance.targetingCenter = targetingCenter;
		instance.targetingRadius = targetingRadius;
		instance.satelliteSeed = satelliteSeed;
		if (resolvedPowerCost != null)
		{
			instance.resolvedPowerCost = Pool.Get<List<int>>();
			for (int i = 0; i < resolvedPowerCost.Count; i++)
			{
				int item = resolvedPowerCost[i];
				instance.resolvedPowerCost.Add(item);
			}
		}
		else
		{
			instance.resolvedPowerCost = null;
		}
	}

	public SatelliteControlComputer Copy()
	{
		SatelliteControlComputer satelliteControlComputer = Pool.Get<SatelliteControlComputer>();
		CopyTo(satelliteControlComputer);
		return satelliteControlComputer;
	}

	public static SatelliteControlComputer Deserialize(BufferStream stream)
	{
		SatelliteControlComputer satelliteControlComputer = Pool.Get<SatelliteControlComputer>();
		Deserialize(stream, satelliteControlComputer, isDelta: false);
		return satelliteControlComputer;
	}

	public static SatelliteControlComputer DeserializeLengthDelimited(BufferStream stream)
	{
		SatelliteControlComputer satelliteControlComputer = Pool.Get<SatelliteControlComputer>();
		DeserializeLengthDelimited(stream, satelliteControlComputer, isDelta: false);
		return satelliteControlComputer;
	}

	public static SatelliteControlComputer DeserializeLength(BufferStream stream, int length)
	{
		SatelliteControlComputer satelliteControlComputer = Pool.Get<SatelliteControlComputer>();
		DeserializeLength(stream, length, satelliteControlComputer, isDelta: false);
		return satelliteControlComputer;
	}

	public static SatelliteControlComputer Deserialize(byte[] buffer)
	{
		SatelliteControlComputer satelliteControlComputer = Pool.Get<SatelliteControlComputer>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, satelliteControlComputer, isDelta: false);
		return satelliteControlComputer;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, SatelliteControlComputer previous)
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

	public static SatelliteControlComputer Deserialize(BufferStream stream, SatelliteControlComputer instance, bool isDelta)
	{
		if (!isDelta && instance.resolvedPowerCost == null)
		{
			instance.resolvedPowerCost = Pool.Get<List<int>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.SatelliteControlComputer");
			switch (num)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				instance.cooldownRemaining = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				instance.controlRemaining = ProtocolParser.ReadSingle(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				instance.selectedSatelliteIndex = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				instance.fuelRemaining = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				instance.isDescending = ProtocolParser.ReadBool(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				instance.descentRemaining = ProtocolParser.ReadSingle(stream);
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.finalCrashPos, isDelta);
				continue;
			case 77:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				instance.finalCrashRadius = ProtocolParser.ReadSingle(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				instance.controllingPlayerId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.targetingCenter, isDelta);
				continue;
			case 109:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				instance.targetingRadius = ProtocolParser.ReadSingle(stream);
				continue;
			case 112:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				instance.satelliteSeed = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 120:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: true, "ProtoBuf.SatelliteControlComputer");
				stream.ConsumeRepeatedElement();
				instance.resolvedPowerCost.Add((int)ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: true, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static SatelliteControlComputer DeserializeLengthDelimited(BufferStream stream, SatelliteControlComputer instance, bool isDelta)
	{
		if (!isDelta && instance.resolvedPowerCost == null)
		{
			instance.resolvedPowerCost = Pool.Get<List<int>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.SatelliteControlComputer");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				instance.cooldownRemaining = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				instance.controlRemaining = ProtocolParser.ReadSingle(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				instance.selectedSatelliteIndex = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				instance.fuelRemaining = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				instance.isDescending = ProtocolParser.ReadBool(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				instance.descentRemaining = ProtocolParser.ReadSingle(stream);
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.finalCrashPos, isDelta);
				continue;
			case 77:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				instance.finalCrashRadius = ProtocolParser.ReadSingle(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				instance.controllingPlayerId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.targetingCenter, isDelta);
				continue;
			case 109:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				instance.targetingRadius = ProtocolParser.ReadSingle(stream);
				continue;
			case 112:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				instance.satelliteSeed = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 120:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: true, "ProtoBuf.SatelliteControlComputer");
				stream.ConsumeRepeatedElement();
				instance.resolvedPowerCost.Add((int)ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: true, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static SatelliteControlComputer DeserializeLength(BufferStream stream, int length, SatelliteControlComputer instance, bool isDelta)
	{
		if (!isDelta && instance.resolvedPowerCost == null)
		{
			instance.resolvedPowerCost = Pool.Get<List<int>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.SatelliteControlComputer");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				instance.cooldownRemaining = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				instance.controlRemaining = ProtocolParser.ReadSingle(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				instance.selectedSatelliteIndex = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				instance.fuelRemaining = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				instance.isDescending = ProtocolParser.ReadBool(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				instance.descentRemaining = ProtocolParser.ReadSingle(stream);
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.finalCrashPos, isDelta);
				continue;
			case 77:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				instance.finalCrashRadius = ProtocolParser.ReadSingle(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				instance.controllingPlayerId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.targetingCenter, isDelta);
				continue;
			case 109:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				instance.targetingRadius = ProtocolParser.ReadSingle(stream);
				continue;
			case 112:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				instance.satelliteSeed = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 120:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: true, "ProtoBuf.SatelliteControlComputer");
				stream.ConsumeRepeatedElement();
				instance.resolvedPowerCost.Add((int)ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: true, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SatelliteControlComputer");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, SatelliteControlComputer instance, SatelliteControlComputer previous)
	{
		if (instance.cooldownRemaining != previous.cooldownRemaining)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.cooldownRemaining);
		}
		if (instance.controlRemaining != previous.controlRemaining)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.controlRemaining);
		}
		if (instance.selectedSatelliteIndex != previous.selectedSatelliteIndex)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.selectedSatelliteIndex);
		}
		if (instance.fuelRemaining != previous.fuelRemaining)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.fuelRemaining);
		}
		stream.WriteByte(40);
		ProtocolParser.WriteBool(stream, instance.isDescending);
		if (instance.descentRemaining != previous.descentRemaining)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.descentRemaining);
		}
		if (instance.finalCrashPos != previous.finalCrashPos)
		{
			stream.WriteByte(58);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.finalCrashPos, previous.finalCrashPos);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field finalCrashPos (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.finalCrashRadius != previous.finalCrashRadius)
		{
			stream.WriteByte(77);
			ProtocolParser.WriteSingle(stream, instance.finalCrashRadius);
		}
		if (instance.controllingPlayerId != previous.controllingPlayerId)
		{
			stream.WriteByte(80);
			ProtocolParser.WriteUInt64(stream, instance.controllingPlayerId);
		}
		if (instance.targetingCenter != previous.targetingCenter)
		{
			stream.WriteByte(90);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.targetingCenter, previous.targetingCenter);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field targetingCenter (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.targetingRadius != previous.targetingRadius)
		{
			stream.WriteByte(109);
			ProtocolParser.WriteSingle(stream, instance.targetingRadius);
		}
		if (instance.satelliteSeed != previous.satelliteSeed)
		{
			stream.WriteByte(112);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.satelliteSeed);
		}
		if (instance.resolvedPowerCost != null)
		{
			for (int i = 0; i < instance.resolvedPowerCost.Count; i++)
			{
				int num3 = instance.resolvedPowerCost[i];
				stream.WriteByte(120);
				ProtocolParser.WriteUInt64(stream, (ulong)num3);
			}
		}
	}

	public static void Serialize(BufferStream stream, SatelliteControlComputer instance)
	{
		if (instance.cooldownRemaining != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.cooldownRemaining);
		}
		if (instance.controlRemaining != 0f)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.controlRemaining);
		}
		if (instance.selectedSatelliteIndex != 0)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.selectedSatelliteIndex);
		}
		if (instance.fuelRemaining != 0)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.fuelRemaining);
		}
		if (instance.isDescending)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteBool(stream, instance.isDescending);
		}
		if (instance.descentRemaining != 0f)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.descentRemaining);
		}
		if (instance.finalCrashPos != default(Vector3))
		{
			stream.WriteByte(58);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.Serialize(stream, instance.finalCrashPos);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field finalCrashPos (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.finalCrashRadius != 0f)
		{
			stream.WriteByte(77);
			ProtocolParser.WriteSingle(stream, instance.finalCrashRadius);
		}
		if (instance.controllingPlayerId != 0L)
		{
			stream.WriteByte(80);
			ProtocolParser.WriteUInt64(stream, instance.controllingPlayerId);
		}
		if (instance.targetingCenter != default(Vector3))
		{
			stream.WriteByte(90);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.targetingCenter);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field targetingCenter (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.targetingRadius != 0f)
		{
			stream.WriteByte(109);
			ProtocolParser.WriteSingle(stream, instance.targetingRadius);
		}
		if (instance.satelliteSeed != 0)
		{
			stream.WriteByte(112);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.satelliteSeed);
		}
		if (instance.resolvedPowerCost != null)
		{
			for (int i = 0; i < instance.resolvedPowerCost.Count; i++)
			{
				int num3 = instance.resolvedPowerCost[i];
				stream.WriteByte(120);
				ProtocolParser.WriteUInt64(stream, (ulong)num3);
			}
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
