using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class PasteRequest : IDisposable, Pool.IPooled, IProto<PasteRequest>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public CopyPasteEntityInfo pasteData;

	[NonSerialized]
	public Vector3 origin;

	[NonSerialized]
	public Vector3 playerRotation;

	[NonSerialized]
	public Vector3 heightOffset;

	[NonSerialized]
	public bool resources;

	[NonSerialized]
	public bool npcs;

	[NonSerialized]
	public bool vehicles;

	[NonSerialized]
	public bool deployables;

	[NonSerialized]
	public bool foundationsOnly;

	[NonSerialized]
	public bool buildingBlocksOnly;

	[NonSerialized]
	public bool snapToTerrain;

	[NonSerialized]
	public bool players;

	[NonSerialized]
	public bool snapToZero;

	[NonSerialized]
	public List<Vector3> pasteOffsets;

	[NonSerialized]
	public bool autoAuth;

	public static void ResetToPool(PasteRequest instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.pasteData != null)
			{
				instance.pasteData.ResetToPool();
				instance.pasteData = null;
			}
			instance.origin = default(Vector3);
			instance.playerRotation = default(Vector3);
			instance.heightOffset = default(Vector3);
			instance.resources = false;
			instance.npcs = false;
			instance.vehicles = false;
			instance.deployables = false;
			instance.foundationsOnly = false;
			instance.buildingBlocksOnly = false;
			instance.snapToTerrain = false;
			instance.players = false;
			instance.snapToZero = false;
			if (instance.pasteOffsets != null)
			{
				List<Vector3> obj = instance.pasteOffsets;
				Pool.FreeUnmanaged(ref obj);
				instance.pasteOffsets = obj;
			}
			instance.autoAuth = false;
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
			throw new Exception("Trying to dispose PasteRequest with ShouldPool set to false!");
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

	public void CopyTo(PasteRequest instance)
	{
		if (pasteData != null)
		{
			if (instance.pasteData == null)
			{
				instance.pasteData = pasteData.Copy();
			}
			else
			{
				pasteData.CopyTo(instance.pasteData);
			}
		}
		else
		{
			instance.pasteData = null;
		}
		instance.origin = origin;
		instance.playerRotation = playerRotation;
		instance.heightOffset = heightOffset;
		instance.resources = resources;
		instance.npcs = npcs;
		instance.vehicles = vehicles;
		instance.deployables = deployables;
		instance.foundationsOnly = foundationsOnly;
		instance.buildingBlocksOnly = buildingBlocksOnly;
		instance.snapToTerrain = snapToTerrain;
		instance.players = players;
		instance.snapToZero = snapToZero;
		if (pasteOffsets != null)
		{
			instance.pasteOffsets = Pool.Get<List<Vector3>>();
			for (int i = 0; i < pasteOffsets.Count; i++)
			{
				Vector3 item = pasteOffsets[i];
				instance.pasteOffsets.Add(item);
			}
		}
		else
		{
			instance.pasteOffsets = null;
		}
		instance.autoAuth = autoAuth;
	}

	public PasteRequest Copy()
	{
		PasteRequest pasteRequest = Pool.Get<PasteRequest>();
		CopyTo(pasteRequest);
		return pasteRequest;
	}

	public static PasteRequest Deserialize(BufferStream stream)
	{
		PasteRequest pasteRequest = Pool.Get<PasteRequest>();
		Deserialize(stream, pasteRequest, isDelta: false);
		return pasteRequest;
	}

	public static PasteRequest DeserializeLengthDelimited(BufferStream stream)
	{
		PasteRequest pasteRequest = Pool.Get<PasteRequest>();
		DeserializeLengthDelimited(stream, pasteRequest, isDelta: false);
		return pasteRequest;
	}

	public static PasteRequest DeserializeLength(BufferStream stream, int length)
	{
		PasteRequest pasteRequest = Pool.Get<PasteRequest>();
		DeserializeLength(stream, length, pasteRequest, isDelta: false);
		return pasteRequest;
	}

	public static PasteRequest Deserialize(byte[] buffer)
	{
		PasteRequest pasteRequest = Pool.Get<PasteRequest>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, pasteRequest, isDelta: false);
		return pasteRequest;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, PasteRequest previous)
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

	public static PasteRequest Deserialize(BufferStream stream, PasteRequest instance, bool isDelta)
	{
		if (!isDelta && instance.pasteOffsets == null)
		{
			instance.pasteOffsets = Pool.Get<List<Vector3>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.PasteRequest");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				if (instance.pasteData == null)
				{
					instance.pasteData = CopyPasteEntityInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					CopyPasteEntityInfo.DeserializeLengthDelimited(stream, instance.pasteData, isDelta);
				}
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.origin, isDelta);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.playerRotation, isDelta);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.heightOffset, isDelta);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				instance.resources = ProtocolParser.ReadBool(stream);
				continue;
			case 88:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				instance.npcs = ProtocolParser.ReadBool(stream);
				continue;
			case 96:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				instance.vehicles = ProtocolParser.ReadBool(stream);
				continue;
			case 104:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				instance.deployables = ProtocolParser.ReadBool(stream);
				continue;
			case 112:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				instance.foundationsOnly = ProtocolParser.ReadBool(stream);
				continue;
			case 120:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				instance.buildingBlocksOnly = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				if (key.WireType == Wire.Varint)
				{
					instance.snapToTerrain = ProtocolParser.ReadBool(stream);
				}
				break;
			case 17u:
				stream.ValidateFieldOrder(ref lastFieldId, 17u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				if (key.WireType == Wire.Varint)
				{
					instance.players = ProtocolParser.ReadBool(stream);
				}
				break;
			case 18u:
				stream.ValidateFieldOrder(ref lastFieldId, 18u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				if (key.WireType == Wire.Varint)
				{
					instance.snapToZero = ProtocolParser.ReadBool(stream);
				}
				break;
			case 19u:
				stream.ValidateFieldOrder(ref lastFieldId, 19u, fieldIsRepeated: true, "ProtoBuf.PasteRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					stream.ConsumeRepeatedElement();
					Vector3 instance2 = default(Vector3);
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance2, isDelta);
					instance.pasteOffsets.Add(instance2);
				}
				break;
			case 20u:
				stream.ValidateFieldOrder(ref lastFieldId, 20u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				if (key.WireType == Wire.Varint)
				{
					instance.autoAuth = ProtocolParser.ReadBool(stream);
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PasteRequest DeserializeLengthDelimited(BufferStream stream, PasteRequest instance, bool isDelta)
	{
		if (!isDelta && instance.pasteOffsets == null)
		{
			instance.pasteOffsets = Pool.Get<List<Vector3>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.PasteRequest");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				if (instance.pasteData == null)
				{
					instance.pasteData = CopyPasteEntityInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					CopyPasteEntityInfo.DeserializeLengthDelimited(stream, instance.pasteData, isDelta);
				}
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.origin, isDelta);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.playerRotation, isDelta);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.heightOffset, isDelta);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				instance.resources = ProtocolParser.ReadBool(stream);
				continue;
			case 88:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				instance.npcs = ProtocolParser.ReadBool(stream);
				continue;
			case 96:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				instance.vehicles = ProtocolParser.ReadBool(stream);
				continue;
			case 104:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				instance.deployables = ProtocolParser.ReadBool(stream);
				continue;
			case 112:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				instance.foundationsOnly = ProtocolParser.ReadBool(stream);
				continue;
			case 120:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				instance.buildingBlocksOnly = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				if (key.WireType == Wire.Varint)
				{
					instance.snapToTerrain = ProtocolParser.ReadBool(stream);
				}
				break;
			case 17u:
				stream.ValidateFieldOrder(ref lastFieldId, 17u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				if (key.WireType == Wire.Varint)
				{
					instance.players = ProtocolParser.ReadBool(stream);
				}
				break;
			case 18u:
				stream.ValidateFieldOrder(ref lastFieldId, 18u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				if (key.WireType == Wire.Varint)
				{
					instance.snapToZero = ProtocolParser.ReadBool(stream);
				}
				break;
			case 19u:
				stream.ValidateFieldOrder(ref lastFieldId, 19u, fieldIsRepeated: true, "ProtoBuf.PasteRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					stream.ConsumeRepeatedElement();
					Vector3 instance2 = default(Vector3);
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance2, isDelta);
					instance.pasteOffsets.Add(instance2);
				}
				break;
			case 20u:
				stream.ValidateFieldOrder(ref lastFieldId, 20u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				if (key.WireType == Wire.Varint)
				{
					instance.autoAuth = ProtocolParser.ReadBool(stream);
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PasteRequest DeserializeLength(BufferStream stream, int length, PasteRequest instance, bool isDelta)
	{
		if (!isDelta && instance.pasteOffsets == null)
		{
			instance.pasteOffsets = Pool.Get<List<Vector3>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.PasteRequest");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				if (instance.pasteData == null)
				{
					instance.pasteData = CopyPasteEntityInfo.DeserializeLengthDelimited(stream);
				}
				else
				{
					CopyPasteEntityInfo.DeserializeLengthDelimited(stream, instance.pasteData, isDelta);
				}
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.origin, isDelta);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.playerRotation, isDelta);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.heightOffset, isDelta);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				instance.resources = ProtocolParser.ReadBool(stream);
				continue;
			case 88:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				instance.npcs = ProtocolParser.ReadBool(stream);
				continue;
			case 96:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				instance.vehicles = ProtocolParser.ReadBool(stream);
				continue;
			case 104:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				instance.deployables = ProtocolParser.ReadBool(stream);
				continue;
			case 112:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				instance.foundationsOnly = ProtocolParser.ReadBool(stream);
				continue;
			case 120:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				instance.buildingBlocksOnly = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				if (key.WireType == Wire.Varint)
				{
					instance.snapToTerrain = ProtocolParser.ReadBool(stream);
				}
				break;
			case 17u:
				stream.ValidateFieldOrder(ref lastFieldId, 17u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				if (key.WireType == Wire.Varint)
				{
					instance.players = ProtocolParser.ReadBool(stream);
				}
				break;
			case 18u:
				stream.ValidateFieldOrder(ref lastFieldId, 18u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				if (key.WireType == Wire.Varint)
				{
					instance.snapToZero = ProtocolParser.ReadBool(stream);
				}
				break;
			case 19u:
				stream.ValidateFieldOrder(ref lastFieldId, 19u, fieldIsRepeated: true, "ProtoBuf.PasteRequest");
				if (key.WireType == Wire.LengthDelimited)
				{
					stream.ConsumeRepeatedElement();
					Vector3 instance2 = default(Vector3);
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance2, isDelta);
					instance.pasteOffsets.Add(instance2);
				}
				break;
			case 20u:
				stream.ValidateFieldOrder(ref lastFieldId, 20u, fieldIsRepeated: false, "ProtoBuf.PasteRequest");
				if (key.WireType == Wire.Varint)
				{
					instance.autoAuth = ProtocolParser.ReadBool(stream);
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PasteRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, PasteRequest instance, PasteRequest previous)
	{
		if (instance.pasteData != null)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			CopyPasteEntityInfo.SerializeDelta(stream, instance.pasteData, previous.pasteData);
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
		if (instance.origin != previous.origin)
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.origin, previous.origin);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field origin (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.playerRotation != previous.playerRotation)
		{
			stream.WriteByte(26);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.playerRotation, previous.playerRotation);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field playerRotation (UnityEngine.Vector3)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
		if (instance.heightOffset != previous.heightOffset)
		{
			stream.WriteByte(34);
			BufferStream.RangeHandle range4 = stream.GetRange(1);
			int position4 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.heightOffset, previous.heightOffset);
			int num4 = stream.Position - position4;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field heightOffset (UnityEngine.Vector3)");
			}
			Span<byte> span4 = range4.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span4, 0);
		}
		stream.WriteByte(80);
		ProtocolParser.WriteBool(stream, instance.resources);
		stream.WriteByte(88);
		ProtocolParser.WriteBool(stream, instance.npcs);
		stream.WriteByte(96);
		ProtocolParser.WriteBool(stream, instance.vehicles);
		stream.WriteByte(104);
		ProtocolParser.WriteBool(stream, instance.deployables);
		stream.WriteByte(112);
		ProtocolParser.WriteBool(stream, instance.foundationsOnly);
		stream.WriteByte(120);
		ProtocolParser.WriteBool(stream, instance.buildingBlocksOnly);
		stream.WriteByte(128);
		stream.WriteByte(1);
		ProtocolParser.WriteBool(stream, instance.snapToTerrain);
		stream.WriteByte(136);
		stream.WriteByte(1);
		ProtocolParser.WriteBool(stream, instance.players);
		stream.WriteByte(144);
		stream.WriteByte(1);
		ProtocolParser.WriteBool(stream, instance.snapToZero);
		if (instance.pasteOffsets != null)
		{
			for (int i = 0; i < instance.pasteOffsets.Count; i++)
			{
				Vector3 vector = instance.pasteOffsets[i];
				stream.WriteByte(154);
				stream.WriteByte(1);
				BufferStream.RangeHandle range5 = stream.GetRange(1);
				int position5 = stream.Position;
				Vector3Serialized.SerializeDelta(stream, vector, vector);
				int num5 = stream.Position - position5;
				if (num5 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field pasteOffsets (UnityEngine.Vector3)");
				}
				Span<byte> span5 = range5.GetSpan();
				ProtocolParser.WriteUInt32((uint)num5, span5, 0);
			}
		}
		stream.WriteByte(160);
		stream.WriteByte(1);
		ProtocolParser.WriteBool(stream, instance.autoAuth);
	}

	public static void Serialize(BufferStream stream, PasteRequest instance)
	{
		if (instance.pasteData != null)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			CopyPasteEntityInfo.Serialize(stream, instance.pasteData);
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
		if (instance.origin != default(Vector3))
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.origin);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field origin (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.playerRotation != default(Vector3))
		{
			stream.WriteByte(26);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.playerRotation);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field playerRotation (UnityEngine.Vector3)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
		if (instance.heightOffset != default(Vector3))
		{
			stream.WriteByte(34);
			BufferStream.RangeHandle range4 = stream.GetRange(1);
			int position4 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.heightOffset);
			int num4 = stream.Position - position4;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field heightOffset (UnityEngine.Vector3)");
			}
			Span<byte> span4 = range4.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span4, 0);
		}
		if (instance.resources)
		{
			stream.WriteByte(80);
			ProtocolParser.WriteBool(stream, instance.resources);
		}
		if (instance.npcs)
		{
			stream.WriteByte(88);
			ProtocolParser.WriteBool(stream, instance.npcs);
		}
		if (instance.vehicles)
		{
			stream.WriteByte(96);
			ProtocolParser.WriteBool(stream, instance.vehicles);
		}
		if (instance.deployables)
		{
			stream.WriteByte(104);
			ProtocolParser.WriteBool(stream, instance.deployables);
		}
		if (instance.foundationsOnly)
		{
			stream.WriteByte(112);
			ProtocolParser.WriteBool(stream, instance.foundationsOnly);
		}
		if (instance.buildingBlocksOnly)
		{
			stream.WriteByte(120);
			ProtocolParser.WriteBool(stream, instance.buildingBlocksOnly);
		}
		if (instance.snapToTerrain)
		{
			stream.WriteByte(128);
			stream.WriteByte(1);
			ProtocolParser.WriteBool(stream, instance.snapToTerrain);
		}
		if (instance.players)
		{
			stream.WriteByte(136);
			stream.WriteByte(1);
			ProtocolParser.WriteBool(stream, instance.players);
		}
		if (instance.snapToZero)
		{
			stream.WriteByte(144);
			stream.WriteByte(1);
			ProtocolParser.WriteBool(stream, instance.snapToZero);
		}
		if (instance.pasteOffsets != null)
		{
			for (int i = 0; i < instance.pasteOffsets.Count; i++)
			{
				Vector3 instance2 = instance.pasteOffsets[i];
				stream.WriteByte(154);
				stream.WriteByte(1);
				BufferStream.RangeHandle range5 = stream.GetRange(1);
				int position5 = stream.Position;
				Vector3Serialized.Serialize(stream, instance2);
				int num5 = stream.Position - position5;
				if (num5 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field pasteOffsets (UnityEngine.Vector3)");
				}
				Span<byte> span5 = range5.GetSpan();
				ProtocolParser.WriteUInt32((uint)num5, span5, 0);
			}
		}
		if (instance.autoAuth)
		{
			stream.WriteByte(160);
			stream.WriteByte(1);
			ProtocolParser.WriteBool(stream, instance.autoAuth);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		pasteData?.InspectUids(action);
	}
}
