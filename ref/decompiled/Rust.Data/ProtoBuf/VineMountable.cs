using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class VineMountable : IDisposable, Pool.IPooled, IProto<VineMountable>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public Vector3 anchorPoint;

	[NonSerialized]
	public VineDestination originPoint;

	[NonSerialized]
	public List<VineDestination> destinations;

	[NonSerialized]
	public VineDestination currentLocation;

	public static void ResetToPool(VineMountable instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.anchorPoint = default(Vector3);
		if (instance.originPoint != null)
		{
			instance.originPoint.ResetToPool();
			instance.originPoint = null;
		}
		if (instance.destinations != null)
		{
			for (int i = 0; i < instance.destinations.Count; i++)
			{
				if (instance.destinations[i] != null)
				{
					instance.destinations[i].ResetToPool();
					instance.destinations[i] = null;
				}
			}
			List<VineDestination> obj = instance.destinations;
			Pool.Free(ref obj, freeElements: false);
			instance.destinations = obj;
		}
		if (instance.currentLocation != null)
		{
			instance.currentLocation.ResetToPool();
			instance.currentLocation = null;
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
			throw new Exception("Trying to dispose VineMountable with ShouldPool set to false!");
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

	public void CopyTo(VineMountable instance)
	{
		instance.anchorPoint = anchorPoint;
		if (originPoint != null)
		{
			if (instance.originPoint == null)
			{
				instance.originPoint = originPoint.Copy();
			}
			else
			{
				originPoint.CopyTo(instance.originPoint);
			}
		}
		else
		{
			instance.originPoint = null;
		}
		if (destinations != null)
		{
			instance.destinations = Pool.Get<List<VineDestination>>();
			for (int i = 0; i < destinations.Count; i++)
			{
				VineDestination item = destinations[i].Copy();
				instance.destinations.Add(item);
			}
		}
		else
		{
			instance.destinations = null;
		}
		if (currentLocation != null)
		{
			if (instance.currentLocation == null)
			{
				instance.currentLocation = currentLocation.Copy();
			}
			else
			{
				currentLocation.CopyTo(instance.currentLocation);
			}
		}
		else
		{
			instance.currentLocation = null;
		}
	}

	public VineMountable Copy()
	{
		VineMountable vineMountable = Pool.Get<VineMountable>();
		CopyTo(vineMountable);
		return vineMountable;
	}

	public static VineMountable Deserialize(BufferStream stream)
	{
		VineMountable vineMountable = Pool.Get<VineMountable>();
		Deserialize(stream, vineMountable, isDelta: false);
		return vineMountable;
	}

	public static VineMountable DeserializeLengthDelimited(BufferStream stream)
	{
		VineMountable vineMountable = Pool.Get<VineMountable>();
		DeserializeLengthDelimited(stream, vineMountable, isDelta: false);
		return vineMountable;
	}

	public static VineMountable DeserializeLength(BufferStream stream, int length)
	{
		VineMountable vineMountable = Pool.Get<VineMountable>();
		DeserializeLength(stream, length, vineMountable, isDelta: false);
		return vineMountable;
	}

	public static VineMountable Deserialize(byte[] buffer)
	{
		VineMountable vineMountable = Pool.Get<VineMountable>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, vineMountable, isDelta: false);
		return vineMountable;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, VineMountable previous)
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

	public static VineMountable Deserialize(BufferStream stream, VineMountable instance, bool isDelta)
	{
		if (!isDelta && instance.destinations == null)
		{
			instance.destinations = Pool.Get<List<VineDestination>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.VineMountable");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VineMountable");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.anchorPoint, isDelta);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VineMountable");
				if (instance.originPoint == null)
				{
					instance.originPoint = VineDestination.DeserializeLengthDelimited(stream);
				}
				else
				{
					VineDestination.DeserializeLengthDelimited(stream, instance.originPoint, isDelta);
				}
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.VineMountable");
				stream.ConsumeRepeatedElement();
				instance.destinations.Add(VineDestination.DeserializeLengthDelimited(stream));
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VineMountable");
				if (instance.currentLocation == null)
				{
					instance.currentLocation = VineDestination.DeserializeLengthDelimited(stream);
				}
				else
				{
					VineDestination.DeserializeLengthDelimited(stream, instance.currentLocation, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VineMountable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VineMountable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.VineMountable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VineMountable");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VineMountable");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static VineMountable DeserializeLengthDelimited(BufferStream stream, VineMountable instance, bool isDelta)
	{
		if (!isDelta && instance.destinations == null)
		{
			instance.destinations = Pool.Get<List<VineDestination>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.VineMountable");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VineMountable");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.anchorPoint, isDelta);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VineMountable");
				if (instance.originPoint == null)
				{
					instance.originPoint = VineDestination.DeserializeLengthDelimited(stream);
				}
				else
				{
					VineDestination.DeserializeLengthDelimited(stream, instance.originPoint, isDelta);
				}
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.VineMountable");
				stream.ConsumeRepeatedElement();
				instance.destinations.Add(VineDestination.DeserializeLengthDelimited(stream));
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VineMountable");
				if (instance.currentLocation == null)
				{
					instance.currentLocation = VineDestination.DeserializeLengthDelimited(stream);
				}
				else
				{
					VineDestination.DeserializeLengthDelimited(stream, instance.currentLocation, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VineMountable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VineMountable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.VineMountable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VineMountable");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VineMountable");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static VineMountable DeserializeLength(BufferStream stream, int length, VineMountable instance, bool isDelta)
	{
		if (!isDelta && instance.destinations == null)
		{
			instance.destinations = Pool.Get<List<VineDestination>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.VineMountable");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VineMountable");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.anchorPoint, isDelta);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VineMountable");
				if (instance.originPoint == null)
				{
					instance.originPoint = VineDestination.DeserializeLengthDelimited(stream);
				}
				else
				{
					VineDestination.DeserializeLengthDelimited(stream, instance.originPoint, isDelta);
				}
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.VineMountable");
				stream.ConsumeRepeatedElement();
				instance.destinations.Add(VineDestination.DeserializeLengthDelimited(stream));
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VineMountable");
				if (instance.currentLocation == null)
				{
					instance.currentLocation = VineDestination.DeserializeLengthDelimited(stream);
				}
				else
				{
					VineDestination.DeserializeLengthDelimited(stream, instance.currentLocation, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VineMountable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VineMountable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.VineMountable");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VineMountable");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VineMountable");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, VineMountable instance, VineMountable previous)
	{
		if (instance.anchorPoint != previous.anchorPoint)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.anchorPoint, previous.anchorPoint);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field anchorPoint (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.originPoint != null)
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			VineDestination.SerializeDelta(stream, instance.originPoint, previous.originPoint);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field originPoint (ProtoBuf.VineDestination)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.destinations != null)
		{
			for (int i = 0; i < instance.destinations.Count; i++)
			{
				VineDestination vineDestination = instance.destinations[i];
				stream.WriteByte(26);
				BufferStream.RangeHandle range3 = stream.GetRange(1);
				int position3 = stream.Position;
				VineDestination.SerializeDelta(stream, vineDestination, vineDestination);
				int num3 = stream.Position - position3;
				if (num3 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field destinations (ProtoBuf.VineDestination)");
				}
				Span<byte> span3 = range3.GetSpan();
				ProtocolParser.WriteUInt32((uint)num3, span3, 0);
			}
		}
		if (instance.currentLocation != null)
		{
			stream.WriteByte(34);
			BufferStream.RangeHandle range4 = stream.GetRange(1);
			int position4 = stream.Position;
			VineDestination.SerializeDelta(stream, instance.currentLocation, previous.currentLocation);
			int num4 = stream.Position - position4;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field currentLocation (ProtoBuf.VineDestination)");
			}
			Span<byte> span4 = range4.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span4, 0);
		}
	}

	public static void Serialize(BufferStream stream, VineMountable instance)
	{
		if (instance.anchorPoint != default(Vector3))
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.Serialize(stream, instance.anchorPoint);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field anchorPoint (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.originPoint != null)
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			VineDestination.Serialize(stream, instance.originPoint);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field originPoint (ProtoBuf.VineDestination)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.destinations != null)
		{
			for (int i = 0; i < instance.destinations.Count; i++)
			{
				VineDestination instance2 = instance.destinations[i];
				stream.WriteByte(26);
				BufferStream.RangeHandle range3 = stream.GetRange(1);
				int position3 = stream.Position;
				VineDestination.Serialize(stream, instance2);
				int num3 = stream.Position - position3;
				if (num3 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field destinations (ProtoBuf.VineDestination)");
				}
				Span<byte> span3 = range3.GetSpan();
				ProtocolParser.WriteUInt32((uint)num3, span3, 0);
			}
		}
		if (instance.currentLocation != null)
		{
			stream.WriteByte(34);
			BufferStream.RangeHandle range4 = stream.GetRange(1);
			int position4 = stream.Position;
			VineDestination.Serialize(stream, instance.currentLocation);
			int num4 = stream.Position - position4;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field currentLocation (ProtoBuf.VineDestination)");
			}
			Span<byte> span4 = range4.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span4, 0);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		originPoint?.InspectUids(action);
		if (destinations != null)
		{
			for (int i = 0; i < destinations.Count; i++)
			{
				destinations[i]?.InspectUids(action);
			}
		}
		currentLocation?.InspectUids(action);
	}
}
