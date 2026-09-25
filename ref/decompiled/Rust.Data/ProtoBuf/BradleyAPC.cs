using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class BradleyAPC : IDisposable, Pool.IPooled, IProto<BradleyAPC>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public float engineThrottle;

	[NonSerialized]
	public float throttleLeft;

	[NonSerialized]
	public float throttleRight;

	[NonSerialized]
	public Vector3 mainGunVec;

	[NonSerialized]
	public Vector3 topTurretVec;

	[NonSerialized]
	public Vector3 rearGunVec;

	[NonSerialized]
	public Vector3 leftSideGun1;

	[NonSerialized]
	public Vector3 leftSideGun2;

	[NonSerialized]
	public Vector3 rightSideGun1;

	[NonSerialized]
	public Vector3 rightSideGun2;

	public static void ResetToPool(BradleyAPC instance)
	{
		if (instance.ShouldPool)
		{
			instance.engineThrottle = 0f;
			instance.throttleLeft = 0f;
			instance.throttleRight = 0f;
			instance.mainGunVec = default(Vector3);
			instance.topTurretVec = default(Vector3);
			instance.rearGunVec = default(Vector3);
			instance.leftSideGun1 = default(Vector3);
			instance.leftSideGun2 = default(Vector3);
			instance.rightSideGun1 = default(Vector3);
			instance.rightSideGun2 = default(Vector3);
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
			throw new Exception("Trying to dispose BradleyAPC with ShouldPool set to false!");
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

	public void CopyTo(BradleyAPC instance)
	{
		instance.engineThrottle = engineThrottle;
		instance.throttleLeft = throttleLeft;
		instance.throttleRight = throttleRight;
		instance.mainGunVec = mainGunVec;
		instance.topTurretVec = topTurretVec;
		instance.rearGunVec = rearGunVec;
		instance.leftSideGun1 = leftSideGun1;
		instance.leftSideGun2 = leftSideGun2;
		instance.rightSideGun1 = rightSideGun1;
		instance.rightSideGun2 = rightSideGun2;
	}

	public BradleyAPC Copy()
	{
		BradleyAPC bradleyAPC = Pool.Get<BradleyAPC>();
		CopyTo(bradleyAPC);
		return bradleyAPC;
	}

	public static BradleyAPC Deserialize(BufferStream stream)
	{
		BradleyAPC bradleyAPC = Pool.Get<BradleyAPC>();
		Deserialize(stream, bradleyAPC, isDelta: false);
		return bradleyAPC;
	}

	public static BradleyAPC DeserializeLengthDelimited(BufferStream stream)
	{
		BradleyAPC bradleyAPC = Pool.Get<BradleyAPC>();
		DeserializeLengthDelimited(stream, bradleyAPC, isDelta: false);
		return bradleyAPC;
	}

	public static BradleyAPC DeserializeLength(BufferStream stream, int length)
	{
		BradleyAPC bradleyAPC = Pool.Get<BradleyAPC>();
		DeserializeLength(stream, length, bradleyAPC, isDelta: false);
		return bradleyAPC;
	}

	public static BradleyAPC Deserialize(byte[] buffer)
	{
		BradleyAPC bradleyAPC = Pool.Get<BradleyAPC>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, bradleyAPC, isDelta: false);
		return bradleyAPC;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, BradleyAPC previous)
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

	public static BradleyAPC Deserialize(BufferStream stream, BradleyAPC instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.BradleyAPC");
			switch (num)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				instance.engineThrottle = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				instance.throttleLeft = ProtocolParser.ReadSingle(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				instance.throttleRight = ProtocolParser.ReadSingle(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.mainGunVec, isDelta);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.topTurretVec, isDelta);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.rearGunVec, isDelta);
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.leftSideGun1, isDelta);
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.leftSideGun2, isDelta);
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.rightSideGun1, isDelta);
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.rightSideGun2, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static BradleyAPC DeserializeLengthDelimited(BufferStream stream, BradleyAPC instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.BradleyAPC");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				instance.engineThrottle = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				instance.throttleLeft = ProtocolParser.ReadSingle(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				instance.throttleRight = ProtocolParser.ReadSingle(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.mainGunVec, isDelta);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.topTurretVec, isDelta);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.rearGunVec, isDelta);
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.leftSideGun1, isDelta);
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.leftSideGun2, isDelta);
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.rightSideGun1, isDelta);
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.rightSideGun2, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static BradleyAPC DeserializeLength(BufferStream stream, int length, BradleyAPC instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.BradleyAPC");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				instance.engineThrottle = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				instance.throttleLeft = ProtocolParser.ReadSingle(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				instance.throttleRight = ProtocolParser.ReadSingle(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.mainGunVec, isDelta);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.topTurretVec, isDelta);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.rearGunVec, isDelta);
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.leftSideGun1, isDelta);
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.leftSideGun2, isDelta);
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.rightSideGun1, isDelta);
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.rightSideGun2, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BradleyAPC");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, BradleyAPC instance, BradleyAPC previous)
	{
		if (instance.engineThrottle != previous.engineThrottle)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.engineThrottle);
		}
		if (instance.throttleLeft != previous.throttleLeft)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.throttleLeft);
		}
		if (instance.throttleRight != previous.throttleRight)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.throttleRight);
		}
		if (instance.mainGunVec != previous.mainGunVec)
		{
			stream.WriteByte(34);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.mainGunVec, previous.mainGunVec);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field mainGunVec (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.topTurretVec != previous.topTurretVec)
		{
			stream.WriteByte(42);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.topTurretVec, previous.topTurretVec);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field topTurretVec (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.rearGunVec != previous.rearGunVec)
		{
			stream.WriteByte(50);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.rearGunVec, previous.rearGunVec);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field rearGunVec (UnityEngine.Vector3)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
		if (instance.leftSideGun1 != previous.leftSideGun1)
		{
			stream.WriteByte(58);
			BufferStream.RangeHandle range4 = stream.GetRange(1);
			int position4 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.leftSideGun1, previous.leftSideGun1);
			int num4 = stream.Position - position4;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field leftSideGun1 (UnityEngine.Vector3)");
			}
			Span<byte> span4 = range4.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span4, 0);
		}
		if (instance.leftSideGun2 != previous.leftSideGun2)
		{
			stream.WriteByte(66);
			BufferStream.RangeHandle range5 = stream.GetRange(1);
			int position5 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.leftSideGun2, previous.leftSideGun2);
			int num5 = stream.Position - position5;
			if (num5 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field leftSideGun2 (UnityEngine.Vector3)");
			}
			Span<byte> span5 = range5.GetSpan();
			ProtocolParser.WriteUInt32((uint)num5, span5, 0);
		}
		if (instance.rightSideGun1 != previous.rightSideGun1)
		{
			stream.WriteByte(74);
			BufferStream.RangeHandle range6 = stream.GetRange(1);
			int position6 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.rightSideGun1, previous.rightSideGun1);
			int num6 = stream.Position - position6;
			if (num6 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field rightSideGun1 (UnityEngine.Vector3)");
			}
			Span<byte> span6 = range6.GetSpan();
			ProtocolParser.WriteUInt32((uint)num6, span6, 0);
		}
		if (instance.rightSideGun2 != previous.rightSideGun2)
		{
			stream.WriteByte(82);
			BufferStream.RangeHandle range7 = stream.GetRange(1);
			int position7 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.rightSideGun2, previous.rightSideGun2);
			int num7 = stream.Position - position7;
			if (num7 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field rightSideGun2 (UnityEngine.Vector3)");
			}
			Span<byte> span7 = range7.GetSpan();
			ProtocolParser.WriteUInt32((uint)num7, span7, 0);
		}
	}

	public static void Serialize(BufferStream stream, BradleyAPC instance)
	{
		if (instance.engineThrottle != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.engineThrottle);
		}
		if (instance.throttleLeft != 0f)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.throttleLeft);
		}
		if (instance.throttleRight != 0f)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.throttleRight);
		}
		if (instance.mainGunVec != default(Vector3))
		{
			stream.WriteByte(34);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.Serialize(stream, instance.mainGunVec);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field mainGunVec (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.topTurretVec != default(Vector3))
		{
			stream.WriteByte(42);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.topTurretVec);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field topTurretVec (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.rearGunVec != default(Vector3))
		{
			stream.WriteByte(50);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.rearGunVec);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field rearGunVec (UnityEngine.Vector3)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
		if (instance.leftSideGun1 != default(Vector3))
		{
			stream.WriteByte(58);
			BufferStream.RangeHandle range4 = stream.GetRange(1);
			int position4 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.leftSideGun1);
			int num4 = stream.Position - position4;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field leftSideGun1 (UnityEngine.Vector3)");
			}
			Span<byte> span4 = range4.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span4, 0);
		}
		if (instance.leftSideGun2 != default(Vector3))
		{
			stream.WriteByte(66);
			BufferStream.RangeHandle range5 = stream.GetRange(1);
			int position5 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.leftSideGun2);
			int num5 = stream.Position - position5;
			if (num5 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field leftSideGun2 (UnityEngine.Vector3)");
			}
			Span<byte> span5 = range5.GetSpan();
			ProtocolParser.WriteUInt32((uint)num5, span5, 0);
		}
		if (instance.rightSideGun1 != default(Vector3))
		{
			stream.WriteByte(74);
			BufferStream.RangeHandle range6 = stream.GetRange(1);
			int position6 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.rightSideGun1);
			int num6 = stream.Position - position6;
			if (num6 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field rightSideGun1 (UnityEngine.Vector3)");
			}
			Span<byte> span6 = range6.GetSpan();
			ProtocolParser.WriteUInt32((uint)num6, span6, 0);
		}
		if (instance.rightSideGun2 != default(Vector3))
		{
			stream.WriteByte(82);
			BufferStream.RangeHandle range7 = stream.GetRange(1);
			int position7 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.rightSideGun2);
			int num7 = stream.Position - position7;
			if (num7 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field rightSideGun2 (UnityEngine.Vector3)");
			}
			Span<byte> span7 = range7.GetSpan();
			ProtocolParser.WriteUInt32((uint)num7, span7, 0);
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
