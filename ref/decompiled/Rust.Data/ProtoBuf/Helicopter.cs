using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class Helicopter : IDisposable, Pool.IPooled, IProto<Helicopter>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public Vector3 tiltRot;

	[NonSerialized]
	public Vector3 leftGun;

	[NonSerialized]
	public Vector3 rightGun;

	[NonSerialized]
	public Vector3 spotlightVec;

	[NonSerialized]
	public List<float> weakspothealths;

	public static void ResetToPool(Helicopter instance)
	{
		if (instance.ShouldPool)
		{
			instance.tiltRot = default(Vector3);
			instance.leftGun = default(Vector3);
			instance.rightGun = default(Vector3);
			instance.spotlightVec = default(Vector3);
			if (instance.weakspothealths != null)
			{
				List<float> obj = instance.weakspothealths;
				Pool.FreeUnmanaged(ref obj);
				instance.weakspothealths = obj;
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
			throw new Exception("Trying to dispose Helicopter with ShouldPool set to false!");
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

	public void CopyTo(Helicopter instance)
	{
		instance.tiltRot = tiltRot;
		instance.leftGun = leftGun;
		instance.rightGun = rightGun;
		instance.spotlightVec = spotlightVec;
		if (weakspothealths != null)
		{
			instance.weakspothealths = Pool.Get<List<float>>();
			for (int i = 0; i < weakspothealths.Count; i++)
			{
				float item = weakspothealths[i];
				instance.weakspothealths.Add(item);
			}
		}
		else
		{
			instance.weakspothealths = null;
		}
	}

	public Helicopter Copy()
	{
		Helicopter helicopter = Pool.Get<Helicopter>();
		CopyTo(helicopter);
		return helicopter;
	}

	public static Helicopter Deserialize(BufferStream stream)
	{
		Helicopter helicopter = Pool.Get<Helicopter>();
		Deserialize(stream, helicopter, isDelta: false);
		return helicopter;
	}

	public static Helicopter DeserializeLengthDelimited(BufferStream stream)
	{
		Helicopter helicopter = Pool.Get<Helicopter>();
		DeserializeLengthDelimited(stream, helicopter, isDelta: false);
		return helicopter;
	}

	public static Helicopter DeserializeLength(BufferStream stream, int length)
	{
		Helicopter helicopter = Pool.Get<Helicopter>();
		DeserializeLength(stream, length, helicopter, isDelta: false);
		return helicopter;
	}

	public static Helicopter Deserialize(byte[] buffer)
	{
		Helicopter helicopter = Pool.Get<Helicopter>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, helicopter, isDelta: false);
		return helicopter;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, Helicopter previous)
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

	public static Helicopter Deserialize(BufferStream stream, Helicopter instance, bool isDelta)
	{
		if (!isDelta && instance.weakspothealths == null)
		{
			instance.weakspothealths = Pool.Get<List<float>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Helicopter");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Helicopter");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.tiltRot, isDelta);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Helicopter");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.leftGun, isDelta);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Helicopter");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.rightGun, isDelta);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Helicopter");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.spotlightVec, isDelta);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.Helicopter");
				stream.ConsumeRepeatedElement();
				instance.weakspothealths.Add(ProtocolParser.ReadSingle(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Helicopter");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Helicopter");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Helicopter");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Helicopter");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.Helicopter");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Helicopter");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Helicopter DeserializeLengthDelimited(BufferStream stream, Helicopter instance, bool isDelta)
	{
		if (!isDelta && instance.weakspothealths == null)
		{
			instance.weakspothealths = Pool.Get<List<float>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Helicopter");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Helicopter");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.tiltRot, isDelta);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Helicopter");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.leftGun, isDelta);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Helicopter");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.rightGun, isDelta);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Helicopter");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.spotlightVec, isDelta);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.Helicopter");
				stream.ConsumeRepeatedElement();
				instance.weakspothealths.Add(ProtocolParser.ReadSingle(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Helicopter");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Helicopter");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Helicopter");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Helicopter");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.Helicopter");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Helicopter");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Helicopter DeserializeLength(BufferStream stream, int length, Helicopter instance, bool isDelta)
	{
		if (!isDelta && instance.weakspothealths == null)
		{
			instance.weakspothealths = Pool.Get<List<float>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Helicopter");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Helicopter");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.tiltRot, isDelta);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Helicopter");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.leftGun, isDelta);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Helicopter");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.rightGun, isDelta);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Helicopter");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.spotlightVec, isDelta);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.Helicopter");
				stream.ConsumeRepeatedElement();
				instance.weakspothealths.Add(ProtocolParser.ReadSingle(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Helicopter");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Helicopter");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Helicopter");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Helicopter");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.Helicopter");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Helicopter");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, Helicopter instance, Helicopter previous)
	{
		if (instance.tiltRot != previous.tiltRot)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.tiltRot, previous.tiltRot);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field tiltRot (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.leftGun != previous.leftGun)
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.leftGun, previous.leftGun);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field leftGun (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.rightGun != previous.rightGun)
		{
			stream.WriteByte(26);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.rightGun, previous.rightGun);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field rightGun (UnityEngine.Vector3)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
		if (instance.spotlightVec != previous.spotlightVec)
		{
			stream.WriteByte(34);
			BufferStream.RangeHandle range4 = stream.GetRange(1);
			int position4 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.spotlightVec, previous.spotlightVec);
			int num4 = stream.Position - position4;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field spotlightVec (UnityEngine.Vector3)");
			}
			Span<byte> span4 = range4.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span4, 0);
		}
		if (instance.weakspothealths != null)
		{
			for (int i = 0; i < instance.weakspothealths.Count; i++)
			{
				float f = instance.weakspothealths[i];
				stream.WriteByte(45);
				ProtocolParser.WriteSingle(stream, f);
			}
		}
	}

	public static void Serialize(BufferStream stream, Helicopter instance)
	{
		if (instance.tiltRot != default(Vector3))
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.Serialize(stream, instance.tiltRot);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field tiltRot (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.leftGun != default(Vector3))
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.leftGun);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field leftGun (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.rightGun != default(Vector3))
		{
			stream.WriteByte(26);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.rightGun);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field rightGun (UnityEngine.Vector3)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
		if (instance.spotlightVec != default(Vector3))
		{
			stream.WriteByte(34);
			BufferStream.RangeHandle range4 = stream.GetRange(1);
			int position4 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.spotlightVec);
			int num4 = stream.Position - position4;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field spotlightVec (UnityEngine.Vector3)");
			}
			Span<byte> span4 = range4.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span4, 0);
		}
		if (instance.weakspothealths != null)
		{
			for (int i = 0; i < instance.weakspothealths.Count; i++)
			{
				float f = instance.weakspothealths[i];
				stream.WriteByte(45);
				ProtocolParser.WriteSingle(stream, f);
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
