using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

public class ModelState : IDisposable, Pool.IPooled, IProto<ModelState>, IProto
{
	public enum Flag
	{
		Ducked = 1,
		Jumped = 2,
		OnGround = 4,
		Sleeping = 8,
		Sprinting = 0x10,
		OnLadder = 0x20,
		Flying = 0x40,
		Aiming = 0x80,
		Prone = 0x100,
		Mounted = 0x200,
		Relaxed = 0x400,
		Unused = 0x800,
		Crawling = 0x1000,
		Loading = 0x2000,
		HeadLook = 0x4000,
		HasParachute = 0x8000,
		Blocking = 0x10000,
		Ragdolling = 0x20000,
		Catching = 0x40000
	}

	public const Flag DefaultFlags = Flag.OnGround;

	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public float waterLevel;

	[NonSerialized]
	public Vector3 lookDir;

	[NonSerialized]
	public int flags;

	[NonSerialized]
	public int poseType;

	[NonSerialized]
	public Vector3 inheritedVelocity;

	[NonSerialized]
	public int ladderType;

	[NonSerialized]
	public Vector3 guidePosition;

	[NonSerialized]
	public Vector3 guideRotation;

	[NonSerialized]
	public uint guidePrefab;

	[NonSerialized]
	public bool guideValid;

	[NonSerialized]
	public int guideVersion;

	[NonSerialized]
	public float ducking;

	[NonSerialized]
	public Vector3 localShieldPos;

	[NonSerialized]
	public Vector3 localShieldRot;

	public bool ducked
	{
		get
		{
			return HasFlag(Flag.Ducked);
		}
		set
		{
			SetFlag(Flag.Ducked, value);
		}
	}

	public bool jumped
	{
		get
		{
			return HasFlag(Flag.Jumped);
		}
		set
		{
			SetFlag(Flag.Jumped, value);
		}
	}

	public bool onground
	{
		get
		{
			return HasFlag(Flag.OnGround);
		}
		set
		{
			SetFlag(Flag.OnGround, value);
		}
	}

	public bool sleeping
	{
		get
		{
			return HasFlag(Flag.Sleeping);
		}
		set
		{
			SetFlag(Flag.Sleeping, value);
		}
	}

	public bool sprinting
	{
		get
		{
			return HasFlag(Flag.Sprinting);
		}
		set
		{
			SetFlag(Flag.Sprinting, value);
		}
	}

	public bool onLadder
	{
		get
		{
			return HasFlag(Flag.OnLadder);
		}
		set
		{
			SetFlag(Flag.OnLadder, value);
		}
	}

	public bool flying
	{
		get
		{
			return HasFlag(Flag.Flying);
		}
		set
		{
			SetFlag(Flag.Flying, value);
		}
	}

	public bool aiming
	{
		get
		{
			return HasFlag(Flag.Aiming);
		}
		set
		{
			SetFlag(Flag.Aiming, value);
		}
	}

	public bool prone
	{
		get
		{
			return HasFlag(Flag.Prone);
		}
		set
		{
			SetFlag(Flag.Prone, value);
		}
	}

	public bool mounted
	{
		get
		{
			return HasFlag(Flag.Mounted);
		}
		set
		{
			SetFlag(Flag.Mounted, value);
		}
	}

	public bool relaxed
	{
		get
		{
			return HasFlag(Flag.Relaxed);
		}
		set
		{
			SetFlag(Flag.Relaxed, value);
		}
	}

	public bool crawling
	{
		get
		{
			return HasFlag(Flag.Crawling);
		}
		set
		{
			SetFlag(Flag.Crawling, value);
		}
	}

	public bool catching
	{
		get
		{
			return HasFlag(Flag.Catching);
		}
		set
		{
			SetFlag(Flag.Catching, value);
		}
	}

	public bool hasParachute
	{
		get
		{
			return HasFlag(Flag.HasParachute);
		}
		set
		{
			SetFlag(Flag.HasParachute, value);
		}
	}

	public bool ragdolling
	{
		get
		{
			return HasFlag(Flag.Ragdolling);
		}
		set
		{
			SetFlag(Flag.Ragdolling, value);
		}
	}

	public bool blocking
	{
		get
		{
			return HasFlag(Flag.Blocking);
		}
		set
		{
			SetFlag(Flag.Blocking, value);
		}
	}

	public bool headLook
	{
		get
		{
			return HasFlag(Flag.HeadLook);
		}
		set
		{
			SetFlag(Flag.HeadLook, value);
		}
	}

	public bool loading
	{
		get
		{
			return HasFlag(Flag.Loading);
		}
		set
		{
			SetFlag(Flag.Loading, value);
		}
	}

	public ModelState()
	{
		flags = 4;
		waterLevel = 0f;
		poseType = 0;
		ducking = 0f;
	}

	public bool HasFlag(Flag f)
	{
		return ((uint)flags & (uint)f) == (uint)f;
	}

	public void SetFlag(Flag f, bool b)
	{
		if (b)
		{
			flags |= (int)f;
		}
		else
		{
			flags &= (int)(~f);
		}
	}

	public static bool Equal(ModelState a, ModelState b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (a.flags != b.flags)
		{
			return false;
		}
		if (Math.Abs(a.waterLevel - b.waterLevel) > 0.001f)
		{
			return false;
		}
		if (a.lookDir != b.lookDir)
		{
			return false;
		}
		if (a.poseType != b.poseType)
		{
			return false;
		}
		if (a.guidePrefab != b.guidePrefab)
		{
			return false;
		}
		if (a.guidePosition != b.guidePosition)
		{
			return false;
		}
		if (a.guideRotation != b.guideRotation)
		{
			return false;
		}
		if (a.guideValid != b.guideValid)
		{
			return false;
		}
		if (Math.Abs(a.ducking - b.ducking) > 0.001f)
		{
			return false;
		}
		return true;
	}

	public static void ResetToPool(ModelState instance)
	{
		if (instance.ShouldPool)
		{
			instance.waterLevel = 0f;
			instance.lookDir = default(Vector3);
			instance.flags = 0;
			instance.poseType = 0;
			instance.inheritedVelocity = default(Vector3);
			instance.ladderType = 0;
			instance.guidePosition = default(Vector3);
			instance.guideRotation = default(Vector3);
			instance.guidePrefab = 0u;
			instance.guideValid = false;
			instance.guideVersion = 0;
			instance.ducking = 0f;
			instance.localShieldPos = default(Vector3);
			instance.localShieldRot = default(Vector3);
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
			throw new Exception("Trying to dispose ModelState with ShouldPool set to false!");
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

	public void CopyTo(ModelState instance)
	{
		instance.waterLevel = waterLevel;
		instance.lookDir = lookDir;
		instance.flags = flags;
		instance.poseType = poseType;
		instance.inheritedVelocity = inheritedVelocity;
		instance.ladderType = ladderType;
		instance.guidePosition = guidePosition;
		instance.guideRotation = guideRotation;
		instance.guidePrefab = guidePrefab;
		instance.guideValid = guideValid;
		instance.guideVersion = guideVersion;
		instance.ducking = ducking;
		instance.localShieldPos = localShieldPos;
		instance.localShieldRot = localShieldRot;
	}

	public ModelState Copy()
	{
		ModelState modelState = Pool.Get<ModelState>();
		CopyTo(modelState);
		return modelState;
	}

	public static ModelState Deserialize(BufferStream stream)
	{
		ModelState modelState = Pool.Get<ModelState>();
		Deserialize(stream, modelState, isDelta: false);
		return modelState;
	}

	public static ModelState DeserializeLengthDelimited(BufferStream stream)
	{
		ModelState modelState = Pool.Get<ModelState>();
		DeserializeLengthDelimited(stream, modelState, isDelta: false);
		return modelState;
	}

	public static ModelState DeserializeLength(BufferStream stream, int length)
	{
		ModelState modelState = Pool.Get<ModelState>();
		DeserializeLength(stream, length, modelState, isDelta: false);
		return modelState;
	}

	public static ModelState Deserialize(byte[] buffer)
	{
		ModelState modelState = Pool.Get<ModelState>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, modelState, isDelta: false);
		return modelState;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ModelState previous)
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

	public static ModelState Deserialize(BufferStream stream, ModelState instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("global::ModelState");
			switch (num)
			{
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "global::ModelState");
				instance.waterLevel = ProtocolParser.ReadSingle(stream);
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "global::ModelState");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.lookDir, isDelta);
				continue;
			case 88:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "global::ModelState");
				instance.flags = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 96:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "global::ModelState");
				instance.poseType = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 106:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "global::ModelState");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.inheritedVelocity, isDelta);
				continue;
			case 112:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "global::ModelState");
				instance.ladderType = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 122:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "global::ModelState");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.guidePosition, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "global::ModelState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "global::ModelState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "global::ModelState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "global::ModelState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "global::ModelState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "global::ModelState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "global::ModelState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "global::ModelState");
				if (key.WireType == Wire.LengthDelimited)
				{
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.guideRotation, isDelta);
				}
				break;
			case 17u:
				stream.ValidateFieldOrder(ref lastFieldId, 17u, fieldIsRepeated: false, "global::ModelState");
				if (key.WireType == Wire.Varint)
				{
					instance.guidePrefab = ProtocolParser.ReadUInt32(stream);
				}
				break;
			case 18u:
				stream.ValidateFieldOrder(ref lastFieldId, 18u, fieldIsRepeated: false, "global::ModelState");
				if (key.WireType == Wire.Varint)
				{
					instance.guideValid = ProtocolParser.ReadBool(stream);
				}
				break;
			case 19u:
				stream.ValidateFieldOrder(ref lastFieldId, 19u, fieldIsRepeated: false, "global::ModelState");
				if (key.WireType == Wire.Varint)
				{
					instance.guideVersion = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 20u:
				stream.ValidateFieldOrder(ref lastFieldId, 20u, fieldIsRepeated: false, "global::ModelState");
				if (key.WireType == Wire.Fixed32)
				{
					instance.ducking = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 21u:
				stream.ValidateFieldOrder(ref lastFieldId, 21u, fieldIsRepeated: false, "global::ModelState");
				if (key.WireType == Wire.LengthDelimited)
				{
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.localShieldPos, isDelta);
				}
				break;
			case 22u:
				stream.ValidateFieldOrder(ref lastFieldId, 22u, fieldIsRepeated: false, "global::ModelState");
				if (key.WireType == Wire.LengthDelimited)
				{
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.localShieldRot, isDelta);
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "global::ModelState");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ModelState DeserializeLengthDelimited(BufferStream stream, ModelState instance, bool isDelta)
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
			stream.ConsumeFieldOperation("global::ModelState");
			switch (num2)
			{
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "global::ModelState");
				instance.waterLevel = ProtocolParser.ReadSingle(stream);
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "global::ModelState");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.lookDir, isDelta);
				continue;
			case 88:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "global::ModelState");
				instance.flags = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 96:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "global::ModelState");
				instance.poseType = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 106:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "global::ModelState");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.inheritedVelocity, isDelta);
				continue;
			case 112:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "global::ModelState");
				instance.ladderType = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 122:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "global::ModelState");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.guidePosition, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "global::ModelState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "global::ModelState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "global::ModelState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "global::ModelState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "global::ModelState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "global::ModelState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "global::ModelState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "global::ModelState");
				if (key.WireType == Wire.LengthDelimited)
				{
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.guideRotation, isDelta);
				}
				break;
			case 17u:
				stream.ValidateFieldOrder(ref lastFieldId, 17u, fieldIsRepeated: false, "global::ModelState");
				if (key.WireType == Wire.Varint)
				{
					instance.guidePrefab = ProtocolParser.ReadUInt32(stream);
				}
				break;
			case 18u:
				stream.ValidateFieldOrder(ref lastFieldId, 18u, fieldIsRepeated: false, "global::ModelState");
				if (key.WireType == Wire.Varint)
				{
					instance.guideValid = ProtocolParser.ReadBool(stream);
				}
				break;
			case 19u:
				stream.ValidateFieldOrder(ref lastFieldId, 19u, fieldIsRepeated: false, "global::ModelState");
				if (key.WireType == Wire.Varint)
				{
					instance.guideVersion = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 20u:
				stream.ValidateFieldOrder(ref lastFieldId, 20u, fieldIsRepeated: false, "global::ModelState");
				if (key.WireType == Wire.Fixed32)
				{
					instance.ducking = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 21u:
				stream.ValidateFieldOrder(ref lastFieldId, 21u, fieldIsRepeated: false, "global::ModelState");
				if (key.WireType == Wire.LengthDelimited)
				{
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.localShieldPos, isDelta);
				}
				break;
			case 22u:
				stream.ValidateFieldOrder(ref lastFieldId, 22u, fieldIsRepeated: false, "global::ModelState");
				if (key.WireType == Wire.LengthDelimited)
				{
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.localShieldRot, isDelta);
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "global::ModelState");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ModelState DeserializeLength(BufferStream stream, int length, ModelState instance, bool isDelta)
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
			stream.ConsumeFieldOperation("global::ModelState");
			switch (num2)
			{
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "global::ModelState");
				instance.waterLevel = ProtocolParser.ReadSingle(stream);
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "global::ModelState");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.lookDir, isDelta);
				continue;
			case 88:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "global::ModelState");
				instance.flags = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 96:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "global::ModelState");
				instance.poseType = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 106:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "global::ModelState");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.inheritedVelocity, isDelta);
				continue;
			case 112:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "global::ModelState");
				instance.ladderType = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 122:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "global::ModelState");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.guidePosition, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "global::ModelState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "global::ModelState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "global::ModelState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "global::ModelState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "global::ModelState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "global::ModelState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "global::ModelState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "global::ModelState");
				if (key.WireType == Wire.LengthDelimited)
				{
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.guideRotation, isDelta);
				}
				break;
			case 17u:
				stream.ValidateFieldOrder(ref lastFieldId, 17u, fieldIsRepeated: false, "global::ModelState");
				if (key.WireType == Wire.Varint)
				{
					instance.guidePrefab = ProtocolParser.ReadUInt32(stream);
				}
				break;
			case 18u:
				stream.ValidateFieldOrder(ref lastFieldId, 18u, fieldIsRepeated: false, "global::ModelState");
				if (key.WireType == Wire.Varint)
				{
					instance.guideValid = ProtocolParser.ReadBool(stream);
				}
				break;
			case 19u:
				stream.ValidateFieldOrder(ref lastFieldId, 19u, fieldIsRepeated: false, "global::ModelState");
				if (key.WireType == Wire.Varint)
				{
					instance.guideVersion = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 20u:
				stream.ValidateFieldOrder(ref lastFieldId, 20u, fieldIsRepeated: false, "global::ModelState");
				if (key.WireType == Wire.Fixed32)
				{
					instance.ducking = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 21u:
				stream.ValidateFieldOrder(ref lastFieldId, 21u, fieldIsRepeated: false, "global::ModelState");
				if (key.WireType == Wire.LengthDelimited)
				{
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.localShieldPos, isDelta);
				}
				break;
			case 22u:
				stream.ValidateFieldOrder(ref lastFieldId, 22u, fieldIsRepeated: false, "global::ModelState");
				if (key.WireType == Wire.LengthDelimited)
				{
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.localShieldRot, isDelta);
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "global::ModelState");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ModelState instance, ModelState previous)
	{
		if (instance.waterLevel != previous.waterLevel)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.waterLevel);
		}
		if (instance.lookDir != previous.lookDir)
		{
			stream.WriteByte(82);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.lookDir, previous.lookDir);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field lookDir (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.flags != previous.flags)
		{
			stream.WriteByte(88);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.flags);
		}
		if (instance.poseType != previous.poseType)
		{
			stream.WriteByte(96);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.poseType);
		}
		if (instance.inheritedVelocity != previous.inheritedVelocity)
		{
			stream.WriteByte(106);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.inheritedVelocity, previous.inheritedVelocity);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field inheritedVelocity (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.ladderType != previous.ladderType)
		{
			stream.WriteByte(112);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ladderType);
		}
		if (instance.guidePosition != previous.guidePosition)
		{
			stream.WriteByte(122);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.guidePosition, previous.guidePosition);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field guidePosition (UnityEngine.Vector3)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
		if (instance.guideRotation != previous.guideRotation)
		{
			stream.WriteByte(130);
			stream.WriteByte(1);
			BufferStream.RangeHandle range4 = stream.GetRange(1);
			int position4 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.guideRotation, previous.guideRotation);
			int num4 = stream.Position - position4;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field guideRotation (UnityEngine.Vector3)");
			}
			Span<byte> span4 = range4.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span4, 0);
		}
		if (instance.guidePrefab != previous.guidePrefab)
		{
			stream.WriteByte(136);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt32(stream, instance.guidePrefab);
		}
		stream.WriteByte(144);
		stream.WriteByte(1);
		ProtocolParser.WriteBool(stream, instance.guideValid);
		if (instance.guideVersion != previous.guideVersion)
		{
			stream.WriteByte(152);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.guideVersion);
		}
		if (instance.ducking != previous.ducking)
		{
			stream.WriteByte(165);
			stream.WriteByte(1);
			ProtocolParser.WriteSingle(stream, instance.ducking);
		}
		if (instance.localShieldPos != previous.localShieldPos)
		{
			stream.WriteByte(170);
			stream.WriteByte(1);
			BufferStream.RangeHandle range5 = stream.GetRange(1);
			int position5 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.localShieldPos, previous.localShieldPos);
			int num5 = stream.Position - position5;
			if (num5 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field localShieldPos (UnityEngine.Vector3)");
			}
			Span<byte> span5 = range5.GetSpan();
			ProtocolParser.WriteUInt32((uint)num5, span5, 0);
		}
		if (instance.localShieldRot != previous.localShieldRot)
		{
			stream.WriteByte(178);
			stream.WriteByte(1);
			BufferStream.RangeHandle range6 = stream.GetRange(1);
			int position6 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.localShieldRot, previous.localShieldRot);
			int num6 = stream.Position - position6;
			if (num6 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field localShieldRot (UnityEngine.Vector3)");
			}
			Span<byte> span6 = range6.GetSpan();
			ProtocolParser.WriteUInt32((uint)num6, span6, 0);
		}
	}

	public static void Serialize(BufferStream stream, ModelState instance)
	{
		if (instance.waterLevel != 0f)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.waterLevel);
		}
		if (instance.lookDir != default(Vector3))
		{
			stream.WriteByte(82);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.Serialize(stream, instance.lookDir);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field lookDir (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.flags != 0)
		{
			stream.WriteByte(88);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.flags);
		}
		if (instance.poseType != 0)
		{
			stream.WriteByte(96);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.poseType);
		}
		if (instance.inheritedVelocity != default(Vector3))
		{
			stream.WriteByte(106);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.inheritedVelocity);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field inheritedVelocity (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.ladderType != 0)
		{
			stream.WriteByte(112);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ladderType);
		}
		if (instance.guidePosition != default(Vector3))
		{
			stream.WriteByte(122);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.guidePosition);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field guidePosition (UnityEngine.Vector3)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
		if (instance.guideRotation != default(Vector3))
		{
			stream.WriteByte(130);
			stream.WriteByte(1);
			BufferStream.RangeHandle range4 = stream.GetRange(1);
			int position4 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.guideRotation);
			int num4 = stream.Position - position4;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field guideRotation (UnityEngine.Vector3)");
			}
			Span<byte> span4 = range4.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span4, 0);
		}
		if (instance.guidePrefab != 0)
		{
			stream.WriteByte(136);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt32(stream, instance.guidePrefab);
		}
		if (instance.guideValid)
		{
			stream.WriteByte(144);
			stream.WriteByte(1);
			ProtocolParser.WriteBool(stream, instance.guideValid);
		}
		if (instance.guideVersion != 0)
		{
			stream.WriteByte(152);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.guideVersion);
		}
		if (instance.ducking != 0f)
		{
			stream.WriteByte(165);
			stream.WriteByte(1);
			ProtocolParser.WriteSingle(stream, instance.ducking);
		}
		if (instance.localShieldPos != default(Vector3))
		{
			stream.WriteByte(170);
			stream.WriteByte(1);
			BufferStream.RangeHandle range5 = stream.GetRange(1);
			int position5 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.localShieldPos);
			int num5 = stream.Position - position5;
			if (num5 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field localShieldPos (UnityEngine.Vector3)");
			}
			Span<byte> span5 = range5.GetSpan();
			ProtocolParser.WriteUInt32((uint)num5, span5, 0);
		}
		if (instance.localShieldRot != default(Vector3))
		{
			stream.WriteByte(178);
			stream.WriteByte(1);
			BufferStream.RangeHandle range6 = stream.GetRange(1);
			int position6 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.localShieldRot);
			int num6 = stream.Position - position6;
			if (num6 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field localShieldRot (UnityEngine.Vector3)");
			}
			Span<byte> span6 = range6.GetSpan();
			ProtocolParser.WriteUInt32((uint)num6, span6, 0);
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
