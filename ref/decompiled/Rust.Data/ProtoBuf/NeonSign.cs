using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class NeonSign : IDisposable, Pool.IPooled, IProto<NeonSign>, IProto
{
	public class Lights : IDisposable, Pool.IPooled, IProto<Lights>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public Color topLeft;

		[NonSerialized]
		public Color topRight;

		[NonSerialized]
		public Color bottomLeft;

		[NonSerialized]
		public Color bottomRight;

		public static void ResetToPool(Lights instance)
		{
			if (instance.ShouldPool)
			{
				instance.topLeft = default(Color);
				instance.topRight = default(Color);
				instance.bottomLeft = default(Color);
				instance.bottomRight = default(Color);
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
				throw new Exception("Trying to dispose Lights with ShouldPool set to false!");
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

		public void CopyTo(Lights instance)
		{
			instance.topLeft = topLeft;
			instance.topRight = topRight;
			instance.bottomLeft = bottomLeft;
			instance.bottomRight = bottomRight;
		}

		public Lights Copy()
		{
			Lights lights = Pool.Get<Lights>();
			CopyTo(lights);
			return lights;
		}

		public static Lights Deserialize(BufferStream stream)
		{
			Lights lights = Pool.Get<Lights>();
			Deserialize(stream, lights, isDelta: false);
			return lights;
		}

		public static Lights DeserializeLengthDelimited(BufferStream stream)
		{
			Lights lights = Pool.Get<Lights>();
			DeserializeLengthDelimited(stream, lights, isDelta: false);
			return lights;
		}

		public static Lights DeserializeLength(BufferStream stream, int length)
		{
			Lights lights = Pool.Get<Lights>();
			DeserializeLength(stream, length, lights, isDelta: false);
			return lights;
		}

		public static Lights Deserialize(byte[] buffer)
		{
			Lights lights = Pool.Get<Lights>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, lights, isDelta: false);
			return lights;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, Lights previous)
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

		public static Lights Deserialize(BufferStream stream, Lights instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.NeonSign.Lights");
				switch (num)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NeonSign.Lights");
					ColorSerialized.DeserializeLengthDelimited(stream, ref instance.topLeft, isDelta);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.NeonSign.Lights");
					ColorSerialized.DeserializeLengthDelimited(stream, ref instance.topRight, isDelta);
					continue;
				case 26:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.NeonSign.Lights");
					ColorSerialized.DeserializeLengthDelimited(stream, ref instance.bottomLeft, isDelta);
					continue;
				case 34:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.NeonSign.Lights");
					ColorSerialized.DeserializeLengthDelimited(stream, ref instance.bottomRight, isDelta);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NeonSign.Lights");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.NeonSign.Lights");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.NeonSign.Lights");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.NeonSign.Lights");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.NeonSign.Lights");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Lights DeserializeLengthDelimited(BufferStream stream, Lights instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.NeonSign.Lights");
				switch (num2)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NeonSign.Lights");
					ColorSerialized.DeserializeLengthDelimited(stream, ref instance.topLeft, isDelta);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.NeonSign.Lights");
					ColorSerialized.DeserializeLengthDelimited(stream, ref instance.topRight, isDelta);
					continue;
				case 26:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.NeonSign.Lights");
					ColorSerialized.DeserializeLengthDelimited(stream, ref instance.bottomLeft, isDelta);
					continue;
				case 34:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.NeonSign.Lights");
					ColorSerialized.DeserializeLengthDelimited(stream, ref instance.bottomRight, isDelta);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NeonSign.Lights");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.NeonSign.Lights");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.NeonSign.Lights");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.NeonSign.Lights");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.NeonSign.Lights");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Lights DeserializeLength(BufferStream stream, int length, Lights instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.NeonSign.Lights");
				switch (num2)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NeonSign.Lights");
					ColorSerialized.DeserializeLengthDelimited(stream, ref instance.topLeft, isDelta);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.NeonSign.Lights");
					ColorSerialized.DeserializeLengthDelimited(stream, ref instance.topRight, isDelta);
					continue;
				case 26:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.NeonSign.Lights");
					ColorSerialized.DeserializeLengthDelimited(stream, ref instance.bottomLeft, isDelta);
					continue;
				case 34:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.NeonSign.Lights");
					ColorSerialized.DeserializeLengthDelimited(stream, ref instance.bottomRight, isDelta);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NeonSign.Lights");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.NeonSign.Lights");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.NeonSign.Lights");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.NeonSign.Lights");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.NeonSign.Lights");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, Lights instance, Lights previous)
		{
			if (instance.topLeft != previous.topLeft)
			{
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				ColorSerialized.SerializeDelta(stream, instance.topLeft, previous.topLeft);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field topLeft (UnityEngine.Color)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
			if (instance.topRight != previous.topRight)
			{
				stream.WriteByte(18);
				BufferStream.RangeHandle range2 = stream.GetRange(1);
				int position2 = stream.Position;
				ColorSerialized.SerializeDelta(stream, instance.topRight, previous.topRight);
				int num2 = stream.Position - position2;
				if (num2 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field topRight (UnityEngine.Color)");
				}
				Span<byte> span2 = range2.GetSpan();
				ProtocolParser.WriteUInt32((uint)num2, span2, 0);
			}
			if (instance.bottomLeft != previous.bottomLeft)
			{
				stream.WriteByte(26);
				BufferStream.RangeHandle range3 = stream.GetRange(1);
				int position3 = stream.Position;
				ColorSerialized.SerializeDelta(stream, instance.bottomLeft, previous.bottomLeft);
				int num3 = stream.Position - position3;
				if (num3 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field bottomLeft (UnityEngine.Color)");
				}
				Span<byte> span3 = range3.GetSpan();
				ProtocolParser.WriteUInt32((uint)num3, span3, 0);
			}
			if (instance.bottomRight != previous.bottomRight)
			{
				stream.WriteByte(34);
				BufferStream.RangeHandle range4 = stream.GetRange(1);
				int position4 = stream.Position;
				ColorSerialized.SerializeDelta(stream, instance.bottomRight, previous.bottomRight);
				int num4 = stream.Position - position4;
				if (num4 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field bottomRight (UnityEngine.Color)");
				}
				Span<byte> span4 = range4.GetSpan();
				ProtocolParser.WriteUInt32((uint)num4, span4, 0);
			}
		}

		public static void Serialize(BufferStream stream, Lights instance)
		{
			if (instance.topLeft != default(Color))
			{
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				ColorSerialized.Serialize(stream, instance.topLeft);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field topLeft (UnityEngine.Color)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
			if (instance.topRight != default(Color))
			{
				stream.WriteByte(18);
				BufferStream.RangeHandle range2 = stream.GetRange(1);
				int position2 = stream.Position;
				ColorSerialized.Serialize(stream, instance.topRight);
				int num2 = stream.Position - position2;
				if (num2 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field topRight (UnityEngine.Color)");
				}
				Span<byte> span2 = range2.GetSpan();
				ProtocolParser.WriteUInt32((uint)num2, span2, 0);
			}
			if (instance.bottomLeft != default(Color))
			{
				stream.WriteByte(26);
				BufferStream.RangeHandle range3 = stream.GetRange(1);
				int position3 = stream.Position;
				ColorSerialized.Serialize(stream, instance.bottomLeft);
				int num3 = stream.Position - position3;
				if (num3 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field bottomLeft (UnityEngine.Color)");
				}
				Span<byte> span3 = range3.GetSpan();
				ProtocolParser.WriteUInt32((uint)num3, span3, 0);
			}
			if (instance.bottomRight != default(Color))
			{
				stream.WriteByte(34);
				BufferStream.RangeHandle range4 = stream.GetRange(1);
				int position4 = stream.Position;
				ColorSerialized.Serialize(stream, instance.bottomRight);
				int num4 = stream.Position - position4;
				if (num4 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field bottomRight (UnityEngine.Color)");
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
		}
	}

	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<Lights> frameLighting;

	[NonSerialized]
	public int currentFrame;

	[NonSerialized]
	public float animationSpeed;

	public static void ResetToPool(NeonSign instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.frameLighting != null)
		{
			for (int i = 0; i < instance.frameLighting.Count; i++)
			{
				if (instance.frameLighting[i] != null)
				{
					instance.frameLighting[i].ResetToPool();
					instance.frameLighting[i] = null;
				}
			}
			List<Lights> obj = instance.frameLighting;
			Pool.Free(ref obj, freeElements: false);
			instance.frameLighting = obj;
		}
		instance.currentFrame = 0;
		instance.animationSpeed = 0f;
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
			throw new Exception("Trying to dispose NeonSign with ShouldPool set to false!");
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

	public void CopyTo(NeonSign instance)
	{
		if (frameLighting != null)
		{
			instance.frameLighting = Pool.Get<List<Lights>>();
			for (int i = 0; i < frameLighting.Count; i++)
			{
				Lights item = frameLighting[i].Copy();
				instance.frameLighting.Add(item);
			}
		}
		else
		{
			instance.frameLighting = null;
		}
		instance.currentFrame = currentFrame;
		instance.animationSpeed = animationSpeed;
	}

	public NeonSign Copy()
	{
		NeonSign neonSign = Pool.Get<NeonSign>();
		CopyTo(neonSign);
		return neonSign;
	}

	public static NeonSign Deserialize(BufferStream stream)
	{
		NeonSign neonSign = Pool.Get<NeonSign>();
		Deserialize(stream, neonSign, isDelta: false);
		return neonSign;
	}

	public static NeonSign DeserializeLengthDelimited(BufferStream stream)
	{
		NeonSign neonSign = Pool.Get<NeonSign>();
		DeserializeLengthDelimited(stream, neonSign, isDelta: false);
		return neonSign;
	}

	public static NeonSign DeserializeLength(BufferStream stream, int length)
	{
		NeonSign neonSign = Pool.Get<NeonSign>();
		DeserializeLength(stream, length, neonSign, isDelta: false);
		return neonSign;
	}

	public static NeonSign Deserialize(byte[] buffer)
	{
		NeonSign neonSign = Pool.Get<NeonSign>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, neonSign, isDelta: false);
		return neonSign;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, NeonSign previous)
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

	public static NeonSign Deserialize(BufferStream stream, NeonSign instance, bool isDelta)
	{
		if (!isDelta && instance.frameLighting == null)
		{
			instance.frameLighting = Pool.Get<List<Lights>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.NeonSign");
			switch (num)
			{
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.NeonSign");
				stream.ConsumeRepeatedElement();
				instance.frameLighting.Add(Lights.DeserializeLengthDelimited(stream));
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.NeonSign");
				instance.currentFrame = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.NeonSign");
				instance.animationSpeed = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.NeonSign");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.NeonSign");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.NeonSign");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.NeonSign");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static NeonSign DeserializeLengthDelimited(BufferStream stream, NeonSign instance, bool isDelta)
	{
		if (!isDelta && instance.frameLighting == null)
		{
			instance.frameLighting = Pool.Get<List<Lights>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.NeonSign");
			switch (num2)
			{
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.NeonSign");
				stream.ConsumeRepeatedElement();
				instance.frameLighting.Add(Lights.DeserializeLengthDelimited(stream));
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.NeonSign");
				instance.currentFrame = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.NeonSign");
				instance.animationSpeed = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.NeonSign");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.NeonSign");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.NeonSign");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.NeonSign");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static NeonSign DeserializeLength(BufferStream stream, int length, NeonSign instance, bool isDelta)
	{
		if (!isDelta && instance.frameLighting == null)
		{
			instance.frameLighting = Pool.Get<List<Lights>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.NeonSign");
			switch (num2)
			{
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.NeonSign");
				stream.ConsumeRepeatedElement();
				instance.frameLighting.Add(Lights.DeserializeLengthDelimited(stream));
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.NeonSign");
				instance.currentFrame = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.NeonSign");
				instance.animationSpeed = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.NeonSign");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.NeonSign");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.NeonSign");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.NeonSign");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, NeonSign instance, NeonSign previous)
	{
		if (instance.frameLighting != null)
		{
			for (int i = 0; i < instance.frameLighting.Count; i++)
			{
				Lights lights = instance.frameLighting[i];
				stream.WriteByte(42);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				Lights.SerializeDelta(stream, lights, lights);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field frameLighting (ProtoBuf.NeonSign.Lights)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.currentFrame != previous.currentFrame)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.currentFrame);
		}
		if (instance.animationSpeed != previous.animationSpeed)
		{
			stream.WriteByte(61);
			ProtocolParser.WriteSingle(stream, instance.animationSpeed);
		}
	}

	public static void Serialize(BufferStream stream, NeonSign instance)
	{
		if (instance.frameLighting != null)
		{
			for (int i = 0; i < instance.frameLighting.Count; i++)
			{
				Lights instance2 = instance.frameLighting[i];
				stream.WriteByte(42);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				Lights.Serialize(stream, instance2);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field frameLighting (ProtoBuf.NeonSign.Lights)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.currentFrame != 0)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.currentFrame);
		}
		if (instance.animationSpeed != 0f)
		{
			stream.WriteByte(61);
			ProtocolParser.WriteSingle(stream, instance.animationSpeed);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (frameLighting != null)
		{
			for (int i = 0; i < frameLighting.Count; i++)
			{
				frameLighting[i]?.InspectUids(action);
			}
		}
	}
}
