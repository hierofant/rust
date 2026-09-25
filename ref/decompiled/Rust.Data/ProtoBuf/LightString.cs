using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class LightString : IDisposable, Pool.IPooled, IProto<LightString>, IProto
{
	public class StringPoint : IDisposable, Pool.IPooled, IProto<StringPoint>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public Vector3 point;

		[NonSerialized]
		public Vector3 normal;

		[NonSerialized]
		public float slack;

		public static void ResetToPool(StringPoint instance)
		{
			if (instance.ShouldPool)
			{
				instance.point = default(Vector3);
				instance.normal = default(Vector3);
				instance.slack = 0f;
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
				throw new Exception("Trying to dispose StringPoint with ShouldPool set to false!");
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

		public void CopyTo(StringPoint instance)
		{
			instance.point = point;
			instance.normal = normal;
			instance.slack = slack;
		}

		public StringPoint Copy()
		{
			StringPoint stringPoint = Pool.Get<StringPoint>();
			CopyTo(stringPoint);
			return stringPoint;
		}

		public static StringPoint Deserialize(BufferStream stream)
		{
			StringPoint stringPoint = Pool.Get<StringPoint>();
			Deserialize(stream, stringPoint, isDelta: false);
			return stringPoint;
		}

		public static StringPoint DeserializeLengthDelimited(BufferStream stream)
		{
			StringPoint stringPoint = Pool.Get<StringPoint>();
			DeserializeLengthDelimited(stream, stringPoint, isDelta: false);
			return stringPoint;
		}

		public static StringPoint DeserializeLength(BufferStream stream, int length)
		{
			StringPoint stringPoint = Pool.Get<StringPoint>();
			DeserializeLength(stream, length, stringPoint, isDelta: false);
			return stringPoint;
		}

		public static StringPoint Deserialize(byte[] buffer)
		{
			StringPoint stringPoint = Pool.Get<StringPoint>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, stringPoint, isDelta: false);
			return stringPoint;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, StringPoint previous)
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

		public static StringPoint Deserialize(BufferStream stream, StringPoint instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.LightString.StringPoint");
				switch (num)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.LightString.StringPoint");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.point, isDelta);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.LightString.StringPoint");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.normal, isDelta);
					continue;
				case 29:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.LightString.StringPoint");
					instance.slack = ProtocolParser.ReadSingle(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.LightString.StringPoint");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.LightString.StringPoint");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.LightString.StringPoint");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.LightString.StringPoint");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static StringPoint DeserializeLengthDelimited(BufferStream stream, StringPoint instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.LightString.StringPoint");
				switch (num2)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.LightString.StringPoint");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.point, isDelta);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.LightString.StringPoint");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.normal, isDelta);
					continue;
				case 29:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.LightString.StringPoint");
					instance.slack = ProtocolParser.ReadSingle(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.LightString.StringPoint");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.LightString.StringPoint");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.LightString.StringPoint");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.LightString.StringPoint");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static StringPoint DeserializeLength(BufferStream stream, int length, StringPoint instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.LightString.StringPoint");
				switch (num2)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.LightString.StringPoint");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.point, isDelta);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.LightString.StringPoint");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.normal, isDelta);
					continue;
				case 29:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.LightString.StringPoint");
					instance.slack = ProtocolParser.ReadSingle(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.LightString.StringPoint");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.LightString.StringPoint");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.LightString.StringPoint");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.LightString.StringPoint");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, StringPoint instance, StringPoint previous)
		{
			if (instance.point != previous.point)
			{
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				Vector3Serialized.SerializeDelta(stream, instance.point, previous.point);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field point (UnityEngine.Vector3)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
			if (instance.normal != previous.normal)
			{
				stream.WriteByte(18);
				BufferStream.RangeHandle range2 = stream.GetRange(1);
				int position2 = stream.Position;
				Vector3Serialized.SerializeDelta(stream, instance.normal, previous.normal);
				int num2 = stream.Position - position2;
				if (num2 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field normal (UnityEngine.Vector3)");
				}
				Span<byte> span2 = range2.GetSpan();
				ProtocolParser.WriteUInt32((uint)num2, span2, 0);
			}
			if (instance.slack != previous.slack)
			{
				stream.WriteByte(29);
				ProtocolParser.WriteSingle(stream, instance.slack);
			}
		}

		public static void Serialize(BufferStream stream, StringPoint instance)
		{
			if (instance.point != default(Vector3))
			{
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				Vector3Serialized.Serialize(stream, instance.point);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field point (UnityEngine.Vector3)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
			if (instance.normal != default(Vector3))
			{
				stream.WriteByte(18);
				BufferStream.RangeHandle range2 = stream.GetRange(1);
				int position2 = stream.Position;
				Vector3Serialized.Serialize(stream, instance.normal);
				int num2 = stream.Position - position2;
				if (num2 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field normal (UnityEngine.Vector3)");
				}
				Span<byte> span2 = range2.GetSpan();
				ProtocolParser.WriteUInt32((uint)num2, span2, 0);
			}
			if (instance.slack != 0f)
			{
				stream.WriteByte(29);
				ProtocolParser.WriteSingle(stream, instance.slack);
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

	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<StringPoint> points;

	[NonSerialized]
	public int lengthUsed;

	[NonSerialized]
	public int animationStyle;

	public static void ResetToPool(LightString instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.points != null)
		{
			for (int i = 0; i < instance.points.Count; i++)
			{
				if (instance.points[i] != null)
				{
					instance.points[i].ResetToPool();
					instance.points[i] = null;
				}
			}
			List<StringPoint> obj = instance.points;
			Pool.Free(ref obj, freeElements: false);
			instance.points = obj;
		}
		instance.lengthUsed = 0;
		instance.animationStyle = 0;
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
			throw new Exception("Trying to dispose LightString with ShouldPool set to false!");
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

	public void CopyTo(LightString instance)
	{
		if (points != null)
		{
			instance.points = Pool.Get<List<StringPoint>>();
			for (int i = 0; i < points.Count; i++)
			{
				StringPoint item = points[i].Copy();
				instance.points.Add(item);
			}
		}
		else
		{
			instance.points = null;
		}
		instance.lengthUsed = lengthUsed;
		instance.animationStyle = animationStyle;
	}

	public LightString Copy()
	{
		LightString lightString = Pool.Get<LightString>();
		CopyTo(lightString);
		return lightString;
	}

	public static LightString Deserialize(BufferStream stream)
	{
		LightString lightString = Pool.Get<LightString>();
		Deserialize(stream, lightString, isDelta: false);
		return lightString;
	}

	public static LightString DeserializeLengthDelimited(BufferStream stream)
	{
		LightString lightString = Pool.Get<LightString>();
		DeserializeLengthDelimited(stream, lightString, isDelta: false);
		return lightString;
	}

	public static LightString DeserializeLength(BufferStream stream, int length)
	{
		LightString lightString = Pool.Get<LightString>();
		DeserializeLength(stream, length, lightString, isDelta: false);
		return lightString;
	}

	public static LightString Deserialize(byte[] buffer)
	{
		LightString lightString = Pool.Get<LightString>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, lightString, isDelta: false);
		return lightString;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, LightString previous)
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

	public static LightString Deserialize(BufferStream stream, LightString instance, bool isDelta)
	{
		if (!isDelta && instance.points == null)
		{
			instance.points = Pool.Get<List<StringPoint>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.LightString");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.LightString");
				stream.ConsumeRepeatedElement();
				instance.points.Add(StringPoint.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.LightString");
				instance.lengthUsed = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.LightString");
				instance.animationStyle = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.LightString");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.LightString");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.LightString");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.LightString");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static LightString DeserializeLengthDelimited(BufferStream stream, LightString instance, bool isDelta)
	{
		if (!isDelta && instance.points == null)
		{
			instance.points = Pool.Get<List<StringPoint>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.LightString");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.LightString");
				stream.ConsumeRepeatedElement();
				instance.points.Add(StringPoint.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.LightString");
				instance.lengthUsed = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.LightString");
				instance.animationStyle = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.LightString");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.LightString");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.LightString");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.LightString");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static LightString DeserializeLength(BufferStream stream, int length, LightString instance, bool isDelta)
	{
		if (!isDelta && instance.points == null)
		{
			instance.points = Pool.Get<List<StringPoint>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.LightString");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.LightString");
				stream.ConsumeRepeatedElement();
				instance.points.Add(StringPoint.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.LightString");
				instance.lengthUsed = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.LightString");
				instance.animationStyle = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.LightString");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.LightString");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.LightString");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.LightString");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, LightString instance, LightString previous)
	{
		if (instance.points != null)
		{
			for (int i = 0; i < instance.points.Count; i++)
			{
				StringPoint stringPoint = instance.points[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				StringPoint.SerializeDelta(stream, stringPoint, stringPoint);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field points (ProtoBuf.LightString.StringPoint)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.lengthUsed != previous.lengthUsed)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.lengthUsed);
		}
		if (instance.animationStyle != previous.animationStyle)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.animationStyle);
		}
	}

	public static void Serialize(BufferStream stream, LightString instance)
	{
		if (instance.points != null)
		{
			for (int i = 0; i < instance.points.Count; i++)
			{
				StringPoint instance2 = instance.points[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				StringPoint.Serialize(stream, instance2);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field points (ProtoBuf.LightString.StringPoint)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.lengthUsed != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.lengthUsed);
		}
		if (instance.animationStyle != 0)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.animationStyle);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (points != null)
		{
			for (int i = 0; i < points.Count; i++)
			{
				points[i]?.InspectUids(action);
			}
		}
	}
}
