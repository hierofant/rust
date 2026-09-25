using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class IOEntity : IDisposable, Pool.IPooled, IProto<IOEntity>, IProto
{
	public class IOConnection : IDisposable, Pool.IPooled, IProto<IOConnection>, IProto
	{
		public class LinePointList : IDisposable, Pool.IPooled, IProto<LinePointList>, IProto
		{
			public bool ShouldPool = true;

			private bool _disposed;

			[NonSerialized]
			public Vector4 a;

			[NonSerialized]
			public Vector4 b;

			[NonSerialized]
			public Vector4 c;

			[NonSerialized]
			public Vector4 d;

			[NonSerialized]
			public Vector4 e;

			[NonSerialized]
			public Vector4 f;

			[NonSerialized]
			public Vector4 g;

			[NonSerialized]
			public Vector4 h;

			public static void ResetToPool(LinePointList instance)
			{
				if (instance.ShouldPool)
				{
					instance.a = default(Vector4);
					instance.b = default(Vector4);
					instance.c = default(Vector4);
					instance.d = default(Vector4);
					instance.e = default(Vector4);
					instance.f = default(Vector4);
					instance.g = default(Vector4);
					instance.h = default(Vector4);
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
					throw new Exception("Trying to dispose LinePointList with ShouldPool set to false!");
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

			public void CopyTo(LinePointList instance)
			{
				instance.a = a;
				instance.b = b;
				instance.c = c;
				instance.d = d;
				instance.e = e;
				instance.f = f;
				instance.g = g;
				instance.h = h;
			}

			public LinePointList Copy()
			{
				LinePointList linePointList = Pool.Get<LinePointList>();
				CopyTo(linePointList);
				return linePointList;
			}

			public static LinePointList Deserialize(BufferStream stream)
			{
				LinePointList linePointList = Pool.Get<LinePointList>();
				Deserialize(stream, linePointList, isDelta: false);
				return linePointList;
			}

			public static LinePointList DeserializeLengthDelimited(BufferStream stream)
			{
				LinePointList linePointList = Pool.Get<LinePointList>();
				DeserializeLengthDelimited(stream, linePointList, isDelta: false);
				return linePointList;
			}

			public static LinePointList DeserializeLength(BufferStream stream, int length)
			{
				LinePointList linePointList = Pool.Get<LinePointList>();
				DeserializeLength(stream, length, linePointList, isDelta: false);
				return linePointList;
			}

			public static LinePointList Deserialize(byte[] buffer)
			{
				LinePointList linePointList = Pool.Get<LinePointList>();
				using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
				Deserialize(stream, linePointList, isDelta: false);
				return linePointList;
			}

			public void FromProto(BufferStream stream, bool isDelta = false)
			{
				Deserialize(stream, this, isDelta);
			}

			public virtual void WriteToStream(BufferStream stream)
			{
				Serialize(stream, this);
			}

			public virtual void WriteToStreamDelta(BufferStream stream, LinePointList previous)
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

			public static LinePointList Deserialize(BufferStream stream, LinePointList instance, bool isDelta)
			{
				uint lastFieldId = 0u;
				while (true)
				{
					int num = stream.ReadByte();
					if (num == -1 || num == 0)
					{
						break;
					}
					stream.ConsumeFieldOperation("ProtoBuf.IOEntity.IOConnection.LinePointList");
					switch (num)
					{
					case 10:
						stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.a, isDelta);
						continue;
					case 18:
						stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.b, isDelta);
						continue;
					case 26:
						stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.c, isDelta);
						continue;
					case 34:
						stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.d, isDelta);
						continue;
					case 42:
						stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.e, isDelta);
						continue;
					case 50:
						stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.f, isDelta);
						continue;
					case 58:
						stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.g, isDelta);
						continue;
					case 66:
						stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.h, isDelta);
						continue;
					}
					Key key = ProtocolParser.ReadKey((byte)num, stream);
					switch (key.Field)
					{
					case 1u:
						stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 2u:
						stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 3u:
						stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 4u:
						stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 5u:
						stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 6u:
						stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 7u:
						stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 8u:
						stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						ProtocolParser.SkipKey(stream, key);
						break;
					default:
						stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						ProtocolParser.SkipKey(stream, key);
						break;
					}
				}
				return instance;
			}

			public static LinePointList DeserializeLengthDelimited(BufferStream stream, LinePointList instance, bool isDelta)
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
					stream.ConsumeFieldOperation("ProtoBuf.IOEntity.IOConnection.LinePointList");
					switch (num2)
					{
					case 10:
						stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.a, isDelta);
						continue;
					case 18:
						stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.b, isDelta);
						continue;
					case 26:
						stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.c, isDelta);
						continue;
					case 34:
						stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.d, isDelta);
						continue;
					case 42:
						stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.e, isDelta);
						continue;
					case 50:
						stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.f, isDelta);
						continue;
					case 58:
						stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.g, isDelta);
						continue;
					case 66:
						stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.h, isDelta);
						continue;
					}
					Key key = ProtocolParser.ReadKey((byte)num2, stream);
					switch (key.Field)
					{
					case 1u:
						stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 2u:
						stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 3u:
						stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 4u:
						stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 5u:
						stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 6u:
						stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 7u:
						stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 8u:
						stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						ProtocolParser.SkipKey(stream, key);
						break;
					default:
						stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						ProtocolParser.SkipKey(stream, key);
						break;
					}
				}
				return instance;
			}

			public static LinePointList DeserializeLength(BufferStream stream, int length, LinePointList instance, bool isDelta)
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
					stream.ConsumeFieldOperation("ProtoBuf.IOEntity.IOConnection.LinePointList");
					switch (num2)
					{
					case 10:
						stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.a, isDelta);
						continue;
					case 18:
						stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.b, isDelta);
						continue;
					case 26:
						stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.c, isDelta);
						continue;
					case 34:
						stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.d, isDelta);
						continue;
					case 42:
						stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.e, isDelta);
						continue;
					case 50:
						stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.f, isDelta);
						continue;
					case 58:
						stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.g, isDelta);
						continue;
					case 66:
						stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.h, isDelta);
						continue;
					}
					Key key = ProtocolParser.ReadKey((byte)num2, stream);
					switch (key.Field)
					{
					case 1u:
						stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 2u:
						stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 3u:
						stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 4u:
						stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 5u:
						stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 6u:
						stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 7u:
						stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 8u:
						stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						ProtocolParser.SkipKey(stream, key);
						break;
					default:
						stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.IOEntity.IOConnection.LinePointList");
						ProtocolParser.SkipKey(stream, key);
						break;
					}
				}
				return instance;
			}

			public static void SerializeDelta(BufferStream stream, LinePointList instance, LinePointList previous)
			{
				if (instance.a != previous.a)
				{
					stream.WriteByte(10);
					BufferStream.RangeHandle range = stream.GetRange(1);
					int position = stream.Position;
					Vector4Serialized.SerializeDelta(stream, instance.a, previous.a);
					int num = stream.Position - position;
					if (num > 127)
					{
						throw new InvalidOperationException("Not enough space was reserved for the length prefix of field a (UnityEngine.Vector4)");
					}
					Span<byte> span = range.GetSpan();
					ProtocolParser.WriteUInt32((uint)num, span, 0);
				}
				if (instance.b != previous.b)
				{
					stream.WriteByte(18);
					BufferStream.RangeHandle range2 = stream.GetRange(1);
					int position2 = stream.Position;
					Vector4Serialized.SerializeDelta(stream, instance.b, previous.b);
					int num2 = stream.Position - position2;
					if (num2 > 127)
					{
						throw new InvalidOperationException("Not enough space was reserved for the length prefix of field b (UnityEngine.Vector4)");
					}
					Span<byte> span2 = range2.GetSpan();
					ProtocolParser.WriteUInt32((uint)num2, span2, 0);
				}
				if (instance.c != previous.c)
				{
					stream.WriteByte(26);
					BufferStream.RangeHandle range3 = stream.GetRange(1);
					int position3 = stream.Position;
					Vector4Serialized.SerializeDelta(stream, instance.c, previous.c);
					int num3 = stream.Position - position3;
					if (num3 > 127)
					{
						throw new InvalidOperationException("Not enough space was reserved for the length prefix of field c (UnityEngine.Vector4)");
					}
					Span<byte> span3 = range3.GetSpan();
					ProtocolParser.WriteUInt32((uint)num3, span3, 0);
				}
				if (instance.d != previous.d)
				{
					stream.WriteByte(34);
					BufferStream.RangeHandle range4 = stream.GetRange(1);
					int position4 = stream.Position;
					Vector4Serialized.SerializeDelta(stream, instance.d, previous.d);
					int num4 = stream.Position - position4;
					if (num4 > 127)
					{
						throw new InvalidOperationException("Not enough space was reserved for the length prefix of field d (UnityEngine.Vector4)");
					}
					Span<byte> span4 = range4.GetSpan();
					ProtocolParser.WriteUInt32((uint)num4, span4, 0);
				}
				if (instance.e != previous.e)
				{
					stream.WriteByte(42);
					BufferStream.RangeHandle range5 = stream.GetRange(1);
					int position5 = stream.Position;
					Vector4Serialized.SerializeDelta(stream, instance.e, previous.e);
					int num5 = stream.Position - position5;
					if (num5 > 127)
					{
						throw new InvalidOperationException("Not enough space was reserved for the length prefix of field e (UnityEngine.Vector4)");
					}
					Span<byte> span5 = range5.GetSpan();
					ProtocolParser.WriteUInt32((uint)num5, span5, 0);
				}
				if (instance.f != previous.f)
				{
					stream.WriteByte(50);
					BufferStream.RangeHandle range6 = stream.GetRange(1);
					int position6 = stream.Position;
					Vector4Serialized.SerializeDelta(stream, instance.f, previous.f);
					int num6 = stream.Position - position6;
					if (num6 > 127)
					{
						throw new InvalidOperationException("Not enough space was reserved for the length prefix of field f (UnityEngine.Vector4)");
					}
					Span<byte> span6 = range6.GetSpan();
					ProtocolParser.WriteUInt32((uint)num6, span6, 0);
				}
				if (instance.g != previous.g)
				{
					stream.WriteByte(58);
					BufferStream.RangeHandle range7 = stream.GetRange(1);
					int position7 = stream.Position;
					Vector4Serialized.SerializeDelta(stream, instance.g, previous.g);
					int num7 = stream.Position - position7;
					if (num7 > 127)
					{
						throw new InvalidOperationException("Not enough space was reserved for the length prefix of field g (UnityEngine.Vector4)");
					}
					Span<byte> span7 = range7.GetSpan();
					ProtocolParser.WriteUInt32((uint)num7, span7, 0);
				}
				if (instance.h != previous.h)
				{
					stream.WriteByte(66);
					BufferStream.RangeHandle range8 = stream.GetRange(1);
					int position8 = stream.Position;
					Vector4Serialized.SerializeDelta(stream, instance.h, previous.h);
					int num8 = stream.Position - position8;
					if (num8 > 127)
					{
						throw new InvalidOperationException("Not enough space was reserved for the length prefix of field h (UnityEngine.Vector4)");
					}
					Span<byte> span8 = range8.GetSpan();
					ProtocolParser.WriteUInt32((uint)num8, span8, 0);
				}
			}

			public static void Serialize(BufferStream stream, LinePointList instance)
			{
				if (instance.a != default(Vector4))
				{
					stream.WriteByte(10);
					BufferStream.RangeHandle range = stream.GetRange(1);
					int position = stream.Position;
					Vector4Serialized.Serialize(stream, instance.a);
					int num = stream.Position - position;
					if (num > 127)
					{
						throw new InvalidOperationException("Not enough space was reserved for the length prefix of field a (UnityEngine.Vector4)");
					}
					Span<byte> span = range.GetSpan();
					ProtocolParser.WriteUInt32((uint)num, span, 0);
				}
				if (instance.b != default(Vector4))
				{
					stream.WriteByte(18);
					BufferStream.RangeHandle range2 = stream.GetRange(1);
					int position2 = stream.Position;
					Vector4Serialized.Serialize(stream, instance.b);
					int num2 = stream.Position - position2;
					if (num2 > 127)
					{
						throw new InvalidOperationException("Not enough space was reserved for the length prefix of field b (UnityEngine.Vector4)");
					}
					Span<byte> span2 = range2.GetSpan();
					ProtocolParser.WriteUInt32((uint)num2, span2, 0);
				}
				if (instance.c != default(Vector4))
				{
					stream.WriteByte(26);
					BufferStream.RangeHandle range3 = stream.GetRange(1);
					int position3 = stream.Position;
					Vector4Serialized.Serialize(stream, instance.c);
					int num3 = stream.Position - position3;
					if (num3 > 127)
					{
						throw new InvalidOperationException("Not enough space was reserved for the length prefix of field c (UnityEngine.Vector4)");
					}
					Span<byte> span3 = range3.GetSpan();
					ProtocolParser.WriteUInt32((uint)num3, span3, 0);
				}
				if (instance.d != default(Vector4))
				{
					stream.WriteByte(34);
					BufferStream.RangeHandle range4 = stream.GetRange(1);
					int position4 = stream.Position;
					Vector4Serialized.Serialize(stream, instance.d);
					int num4 = stream.Position - position4;
					if (num4 > 127)
					{
						throw new InvalidOperationException("Not enough space was reserved for the length prefix of field d (UnityEngine.Vector4)");
					}
					Span<byte> span4 = range4.GetSpan();
					ProtocolParser.WriteUInt32((uint)num4, span4, 0);
				}
				if (instance.e != default(Vector4))
				{
					stream.WriteByte(42);
					BufferStream.RangeHandle range5 = stream.GetRange(1);
					int position5 = stream.Position;
					Vector4Serialized.Serialize(stream, instance.e);
					int num5 = stream.Position - position5;
					if (num5 > 127)
					{
						throw new InvalidOperationException("Not enough space was reserved for the length prefix of field e (UnityEngine.Vector4)");
					}
					Span<byte> span5 = range5.GetSpan();
					ProtocolParser.WriteUInt32((uint)num5, span5, 0);
				}
				if (instance.f != default(Vector4))
				{
					stream.WriteByte(50);
					BufferStream.RangeHandle range6 = stream.GetRange(1);
					int position6 = stream.Position;
					Vector4Serialized.Serialize(stream, instance.f);
					int num6 = stream.Position - position6;
					if (num6 > 127)
					{
						throw new InvalidOperationException("Not enough space was reserved for the length prefix of field f (UnityEngine.Vector4)");
					}
					Span<byte> span6 = range6.GetSpan();
					ProtocolParser.WriteUInt32((uint)num6, span6, 0);
				}
				if (instance.g != default(Vector4))
				{
					stream.WriteByte(58);
					BufferStream.RangeHandle range7 = stream.GetRange(1);
					int position7 = stream.Position;
					Vector4Serialized.Serialize(stream, instance.g);
					int num7 = stream.Position - position7;
					if (num7 > 127)
					{
						throw new InvalidOperationException("Not enough space was reserved for the length prefix of field g (UnityEngine.Vector4)");
					}
					Span<byte> span7 = range7.GetSpan();
					ProtocolParser.WriteUInt32((uint)num7, span7, 0);
				}
				if (instance.h != default(Vector4))
				{
					stream.WriteByte(66);
					BufferStream.RangeHandle range8 = stream.GetRange(1);
					int position8 = stream.Position;
					Vector4Serialized.Serialize(stream, instance.h);
					int num8 = stream.Position - position8;
					if (num8 > 127)
					{
						throw new InvalidOperationException("Not enough space was reserved for the length prefix of field h (UnityEngine.Vector4)");
					}
					Span<byte> span8 = range8.GetSpan();
					ProtocolParser.WriteUInt32((uint)num8, span8, 0);
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

		public class LineVec : IDisposable, Pool.IPooled, IProto<LineVec>, IProto
		{
			public bool ShouldPool = true;

			private bool _disposed;

			[NonSerialized]
			public Vector4 vec;

			public static void ResetToPool(LineVec instance)
			{
				if (instance.ShouldPool)
				{
					instance.vec = default(Vector4);
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
					throw new Exception("Trying to dispose LineVec with ShouldPool set to false!");
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

			public void CopyTo(LineVec instance)
			{
				instance.vec = vec;
			}

			public LineVec Copy()
			{
				LineVec lineVec = Pool.Get<LineVec>();
				CopyTo(lineVec);
				return lineVec;
			}

			public static LineVec Deserialize(BufferStream stream)
			{
				LineVec lineVec = Pool.Get<LineVec>();
				Deserialize(stream, lineVec, isDelta: false);
				return lineVec;
			}

			public static LineVec DeserializeLengthDelimited(BufferStream stream)
			{
				LineVec lineVec = Pool.Get<LineVec>();
				DeserializeLengthDelimited(stream, lineVec, isDelta: false);
				return lineVec;
			}

			public static LineVec DeserializeLength(BufferStream stream, int length)
			{
				LineVec lineVec = Pool.Get<LineVec>();
				DeserializeLength(stream, length, lineVec, isDelta: false);
				return lineVec;
			}

			public static LineVec Deserialize(byte[] buffer)
			{
				LineVec lineVec = Pool.Get<LineVec>();
				using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
				Deserialize(stream, lineVec, isDelta: false);
				return lineVec;
			}

			public void FromProto(BufferStream stream, bool isDelta = false)
			{
				Deserialize(stream, this, isDelta);
			}

			public virtual void WriteToStream(BufferStream stream)
			{
				Serialize(stream, this);
			}

			public virtual void WriteToStreamDelta(BufferStream stream, LineVec previous)
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

			public static LineVec Deserialize(BufferStream stream, LineVec instance, bool isDelta)
			{
				uint lastFieldId = 0u;
				while (true)
				{
					int num = stream.ReadByte();
					if (num == -1 || num == 0)
					{
						break;
					}
					stream.ConsumeFieldOperation("ProtoBuf.IOEntity.IOConnection.LineVec");
					if (num == 10)
					{
						stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LineVec");
						Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.vec, isDelta);
						continue;
					}
					Key key = ProtocolParser.ReadKey((byte)num, stream);
					if (key.Field == 1)
					{
						stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LineVec");
						ProtocolParser.SkipKey(stream, key);
					}
					else
					{
						stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.IOEntity.IOConnection.LineVec");
						ProtocolParser.SkipKey(stream, key);
					}
				}
				return instance;
			}

			public static LineVec DeserializeLengthDelimited(BufferStream stream, LineVec instance, bool isDelta)
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
					stream.ConsumeFieldOperation("ProtoBuf.IOEntity.IOConnection.LineVec");
					if (num2 == 10)
					{
						stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LineVec");
						Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.vec, isDelta);
						continue;
					}
					Key key = ProtocolParser.ReadKey((byte)num2, stream);
					if (key.Field == 1)
					{
						stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LineVec");
						ProtocolParser.SkipKey(stream, key);
					}
					else
					{
						stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.IOEntity.IOConnection.LineVec");
						ProtocolParser.SkipKey(stream, key);
					}
				}
				return instance;
			}

			public static LineVec DeserializeLength(BufferStream stream, int length, LineVec instance, bool isDelta)
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
					stream.ConsumeFieldOperation("ProtoBuf.IOEntity.IOConnection.LineVec");
					if (num2 == 10)
					{
						stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LineVec");
						Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.vec, isDelta);
						continue;
					}
					Key key = ProtocolParser.ReadKey((byte)num2, stream);
					if (key.Field == 1)
					{
						stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection.LineVec");
						ProtocolParser.SkipKey(stream, key);
					}
					else
					{
						stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.IOEntity.IOConnection.LineVec");
						ProtocolParser.SkipKey(stream, key);
					}
				}
				return instance;
			}

			public static void SerializeDelta(BufferStream stream, LineVec instance, LineVec previous)
			{
				if (instance.vec != previous.vec)
				{
					stream.WriteByte(10);
					BufferStream.RangeHandle range = stream.GetRange(1);
					int position = stream.Position;
					Vector4Serialized.SerializeDelta(stream, instance.vec, previous.vec);
					int num = stream.Position - position;
					if (num > 127)
					{
						throw new InvalidOperationException("Not enough space was reserved for the length prefix of field vec (UnityEngine.Vector4)");
					}
					Span<byte> span = range.GetSpan();
					ProtocolParser.WriteUInt32((uint)num, span, 0);
				}
			}

			public static void Serialize(BufferStream stream, LineVec instance)
			{
				if (instance.vec != default(Vector4))
				{
					stream.WriteByte(10);
					BufferStream.RangeHandle range = stream.GetRange(1);
					int position = stream.Position;
					Vector4Serialized.Serialize(stream, instance.vec);
					int num = stream.Position - position;
					if (num > 127)
					{
						throw new InvalidOperationException("Not enough space was reserved for the length prefix of field vec (UnityEngine.Vector4)");
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
			}
		}

		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public string niceName;

		[NonSerialized]
		public int type;

		[NonSerialized]
		public NetworkableId connectedID;

		[NonSerialized]
		public int connectedToSlot;

		[NonSerialized]
		public bool inUse;

		[NonSerialized]
		public List<LineVec> linePointList;

		[NonSerialized]
		public int colour;

		[NonSerialized]
		public Vector3 worldSpaceRotation;

		[NonSerialized]
		public float lineThickness;

		[NonSerialized]
		public List<WireLineAnchorInfo> lineAnchorList;

		[NonSerialized]
		public Vector3 originPosition;

		[NonSerialized]
		public Vector3 originRotation;

		[NonSerialized]
		public List<float> slackLevels;

		public static void ResetToPool(IOConnection instance)
		{
			if (!instance.ShouldPool)
			{
				return;
			}
			instance.niceName = string.Empty;
			instance.type = 0;
			instance.connectedID = default(NetworkableId);
			instance.connectedToSlot = 0;
			instance.inUse = false;
			if (instance.linePointList != null)
			{
				for (int i = 0; i < instance.linePointList.Count; i++)
				{
					if (instance.linePointList[i] != null)
					{
						instance.linePointList[i].ResetToPool();
						instance.linePointList[i] = null;
					}
				}
				List<LineVec> obj = instance.linePointList;
				Pool.Free(ref obj, freeElements: false);
				instance.linePointList = obj;
			}
			instance.colour = 0;
			instance.worldSpaceRotation = default(Vector3);
			instance.lineThickness = 0f;
			if (instance.lineAnchorList != null)
			{
				for (int j = 0; j < instance.lineAnchorList.Count; j++)
				{
					if (instance.lineAnchorList[j] != null)
					{
						instance.lineAnchorList[j].ResetToPool();
						instance.lineAnchorList[j] = null;
					}
				}
				List<WireLineAnchorInfo> obj2 = instance.lineAnchorList;
				Pool.Free(ref obj2, freeElements: false);
				instance.lineAnchorList = obj2;
			}
			instance.originPosition = default(Vector3);
			instance.originRotation = default(Vector3);
			if (instance.slackLevels != null)
			{
				List<float> obj3 = instance.slackLevels;
				Pool.FreeUnmanaged(ref obj3);
				instance.slackLevels = obj3;
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
				throw new Exception("Trying to dispose IOConnection with ShouldPool set to false!");
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

		public void CopyTo(IOConnection instance)
		{
			instance.niceName = niceName;
			instance.type = type;
			instance.connectedID = connectedID;
			instance.connectedToSlot = connectedToSlot;
			instance.inUse = inUse;
			if (linePointList != null)
			{
				instance.linePointList = Pool.Get<List<LineVec>>();
				for (int i = 0; i < linePointList.Count; i++)
				{
					LineVec item = linePointList[i].Copy();
					instance.linePointList.Add(item);
				}
			}
			else
			{
				instance.linePointList = null;
			}
			instance.colour = colour;
			instance.worldSpaceRotation = worldSpaceRotation;
			instance.lineThickness = lineThickness;
			if (lineAnchorList != null)
			{
				instance.lineAnchorList = Pool.Get<List<WireLineAnchorInfo>>();
				for (int j = 0; j < lineAnchorList.Count; j++)
				{
					WireLineAnchorInfo item2 = lineAnchorList[j].Copy();
					instance.lineAnchorList.Add(item2);
				}
			}
			else
			{
				instance.lineAnchorList = null;
			}
			instance.originPosition = originPosition;
			instance.originRotation = originRotation;
			if (slackLevels != null)
			{
				instance.slackLevels = Pool.Get<List<float>>();
				for (int k = 0; k < slackLevels.Count; k++)
				{
					float item3 = slackLevels[k];
					instance.slackLevels.Add(item3);
				}
			}
			else
			{
				instance.slackLevels = null;
			}
		}

		public IOConnection Copy()
		{
			IOConnection iOConnection = Pool.Get<IOConnection>();
			CopyTo(iOConnection);
			return iOConnection;
		}

		public static IOConnection Deserialize(BufferStream stream)
		{
			IOConnection iOConnection = Pool.Get<IOConnection>();
			Deserialize(stream, iOConnection, isDelta: false);
			return iOConnection;
		}

		public static IOConnection DeserializeLengthDelimited(BufferStream stream)
		{
			IOConnection iOConnection = Pool.Get<IOConnection>();
			DeserializeLengthDelimited(stream, iOConnection, isDelta: false);
			return iOConnection;
		}

		public static IOConnection DeserializeLength(BufferStream stream, int length)
		{
			IOConnection iOConnection = Pool.Get<IOConnection>();
			DeserializeLength(stream, length, iOConnection, isDelta: false);
			return iOConnection;
		}

		public static IOConnection Deserialize(byte[] buffer)
		{
			IOConnection iOConnection = Pool.Get<IOConnection>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, iOConnection, isDelta: false);
			return iOConnection;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, IOConnection previous)
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

		public static IOConnection Deserialize(BufferStream stream, IOConnection instance, bool isDelta)
		{
			if (!isDelta)
			{
				if (instance.linePointList == null)
				{
					instance.linePointList = Pool.Get<List<LineVec>>();
				}
				if (instance.lineAnchorList == null)
				{
					instance.lineAnchorList = Pool.Get<List<WireLineAnchorInfo>>();
				}
				if (instance.slackLevels == null)
				{
					instance.slackLevels = Pool.Get<List<float>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.IOEntity.IOConnection");
				switch (num)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					instance.niceName = ProtocolParser.ReadString(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					instance.type = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					instance.connectedID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					instance.connectedToSlot = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					instance.inUse = ProtocolParser.ReadBool(stream);
					continue;
				case 50:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: true, "ProtoBuf.IOEntity.IOConnection");
					stream.ConsumeRepeatedElement();
					instance.linePointList.Add(LineVec.DeserializeLengthDelimited(stream));
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					instance.colour = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 66:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.worldSpaceRotation, isDelta);
					continue;
				case 77:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					instance.lineThickness = ProtocolParser.ReadSingle(stream);
					continue;
				case 82:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: true, "ProtoBuf.IOEntity.IOConnection");
					stream.ConsumeRepeatedElement();
					instance.lineAnchorList.Add(WireLineAnchorInfo.DeserializeLengthDelimited(stream));
					continue;
				case 90:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.originPosition, isDelta);
					continue;
				case 98:
					stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.originRotation, isDelta);
					continue;
				case 109:
					stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: true, "ProtoBuf.IOEntity.IOConnection");
					stream.ConsumeRepeatedElement();
					instance.slackLevels.Add(ProtocolParser.ReadSingle(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: true, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 9u:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 10u:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: true, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 11u:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 12u:
					stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 13u:
					stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: true, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static IOConnection DeserializeLengthDelimited(BufferStream stream, IOConnection instance, bool isDelta)
		{
			if (!isDelta)
			{
				if (instance.linePointList == null)
				{
					instance.linePointList = Pool.Get<List<LineVec>>();
				}
				if (instance.lineAnchorList == null)
				{
					instance.lineAnchorList = Pool.Get<List<WireLineAnchorInfo>>();
				}
				if (instance.slackLevels == null)
				{
					instance.slackLevels = Pool.Get<List<float>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.IOEntity.IOConnection");
				switch (num2)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					instance.niceName = ProtocolParser.ReadString(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					instance.type = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					instance.connectedID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					instance.connectedToSlot = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					instance.inUse = ProtocolParser.ReadBool(stream);
					continue;
				case 50:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: true, "ProtoBuf.IOEntity.IOConnection");
					stream.ConsumeRepeatedElement();
					instance.linePointList.Add(LineVec.DeserializeLengthDelimited(stream));
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					instance.colour = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 66:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.worldSpaceRotation, isDelta);
					continue;
				case 77:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					instance.lineThickness = ProtocolParser.ReadSingle(stream);
					continue;
				case 82:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: true, "ProtoBuf.IOEntity.IOConnection");
					stream.ConsumeRepeatedElement();
					instance.lineAnchorList.Add(WireLineAnchorInfo.DeserializeLengthDelimited(stream));
					continue;
				case 90:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.originPosition, isDelta);
					continue;
				case 98:
					stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.originRotation, isDelta);
					continue;
				case 109:
					stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: true, "ProtoBuf.IOEntity.IOConnection");
					stream.ConsumeRepeatedElement();
					instance.slackLevels.Add(ProtocolParser.ReadSingle(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: true, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 9u:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 10u:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: true, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 11u:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 12u:
					stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 13u:
					stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: true, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static IOConnection DeserializeLength(BufferStream stream, int length, IOConnection instance, bool isDelta)
		{
			if (!isDelta)
			{
				if (instance.linePointList == null)
				{
					instance.linePointList = Pool.Get<List<LineVec>>();
				}
				if (instance.lineAnchorList == null)
				{
					instance.lineAnchorList = Pool.Get<List<WireLineAnchorInfo>>();
				}
				if (instance.slackLevels == null)
				{
					instance.slackLevels = Pool.Get<List<float>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.IOEntity.IOConnection");
				switch (num2)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					instance.niceName = ProtocolParser.ReadString(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					instance.type = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					instance.connectedID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					instance.connectedToSlot = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					instance.inUse = ProtocolParser.ReadBool(stream);
					continue;
				case 50:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: true, "ProtoBuf.IOEntity.IOConnection");
					stream.ConsumeRepeatedElement();
					instance.linePointList.Add(LineVec.DeserializeLengthDelimited(stream));
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					instance.colour = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 66:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.worldSpaceRotation, isDelta);
					continue;
				case 77:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					instance.lineThickness = ProtocolParser.ReadSingle(stream);
					continue;
				case 82:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: true, "ProtoBuf.IOEntity.IOConnection");
					stream.ConsumeRepeatedElement();
					instance.lineAnchorList.Add(WireLineAnchorInfo.DeserializeLengthDelimited(stream));
					continue;
				case 90:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.originPosition, isDelta);
					continue;
				case 98:
					stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.originRotation, isDelta);
					continue;
				case 109:
					stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: true, "ProtoBuf.IOEntity.IOConnection");
					stream.ConsumeRepeatedElement();
					instance.slackLevels.Add(ProtocolParser.ReadSingle(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: true, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 9u:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 10u:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: true, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 11u:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 12u:
					stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 13u:
					stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: true, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.IOEntity.IOConnection");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, IOConnection instance, IOConnection previous)
		{
			if (instance.niceName != null && instance.niceName != previous.niceName)
			{
				stream.WriteByte(10);
				ProtocolParser.WriteString(stream, instance.niceName);
			}
			if (instance.type != previous.type)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.type);
			}
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, instance.connectedID.Value);
			if (instance.connectedToSlot != previous.connectedToSlot)
			{
				stream.WriteByte(32);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.connectedToSlot);
			}
			stream.WriteByte(40);
			ProtocolParser.WriteBool(stream, instance.inUse);
			if (instance.linePointList != null)
			{
				for (int i = 0; i < instance.linePointList.Count; i++)
				{
					LineVec lineVec = instance.linePointList[i];
					stream.WriteByte(50);
					BufferStream.RangeHandle range = stream.GetRange(1);
					int position = stream.Position;
					LineVec.SerializeDelta(stream, lineVec, lineVec);
					int num = stream.Position - position;
					if (num > 127)
					{
						throw new InvalidOperationException("Not enough space was reserved for the length prefix of field linePointList (ProtoBuf.IOEntity.IOConnection.LineVec)");
					}
					Span<byte> span = range.GetSpan();
					ProtocolParser.WriteUInt32((uint)num, span, 0);
				}
			}
			if (instance.colour != previous.colour)
			{
				stream.WriteByte(56);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.colour);
			}
			if (instance.worldSpaceRotation != previous.worldSpaceRotation)
			{
				stream.WriteByte(66);
				BufferStream.RangeHandle range2 = stream.GetRange(1);
				int position2 = stream.Position;
				Vector3Serialized.SerializeDelta(stream, instance.worldSpaceRotation, previous.worldSpaceRotation);
				int num2 = stream.Position - position2;
				if (num2 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field worldSpaceRotation (UnityEngine.Vector3)");
				}
				Span<byte> span2 = range2.GetSpan();
				ProtocolParser.WriteUInt32((uint)num2, span2, 0);
			}
			if (instance.lineThickness != previous.lineThickness)
			{
				stream.WriteByte(77);
				ProtocolParser.WriteSingle(stream, instance.lineThickness);
			}
			if (instance.lineAnchorList != null)
			{
				for (int j = 0; j < instance.lineAnchorList.Count; j++)
				{
					WireLineAnchorInfo wireLineAnchorInfo = instance.lineAnchorList[j];
					stream.WriteByte(82);
					BufferStream.RangeHandle range3 = stream.GetRange(5);
					int position3 = stream.Position;
					WireLineAnchorInfo.SerializeDelta(stream, wireLineAnchorInfo, wireLineAnchorInfo);
					int val = stream.Position - position3;
					Span<byte> span3 = range3.GetSpan();
					int num3 = ProtocolParser.WriteUInt32((uint)val, span3, 0);
					if (num3 < 5)
					{
						span3[num3 - 1] |= 128;
						while (num3 < 4)
						{
							span3[num3++] = 128;
						}
						span3[4] = 0;
					}
				}
			}
			if (instance.originPosition != previous.originPosition)
			{
				stream.WriteByte(90);
				BufferStream.RangeHandle range4 = stream.GetRange(1);
				int position4 = stream.Position;
				Vector3Serialized.SerializeDelta(stream, instance.originPosition, previous.originPosition);
				int num4 = stream.Position - position4;
				if (num4 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field originPosition (UnityEngine.Vector3)");
				}
				Span<byte> span4 = range4.GetSpan();
				ProtocolParser.WriteUInt32((uint)num4, span4, 0);
			}
			if (instance.originRotation != previous.originRotation)
			{
				stream.WriteByte(98);
				BufferStream.RangeHandle range5 = stream.GetRange(1);
				int position5 = stream.Position;
				Vector3Serialized.SerializeDelta(stream, instance.originRotation, previous.originRotation);
				int num5 = stream.Position - position5;
				if (num5 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field originRotation (UnityEngine.Vector3)");
				}
				Span<byte> span5 = range5.GetSpan();
				ProtocolParser.WriteUInt32((uint)num5, span5, 0);
			}
			if (instance.slackLevels != null)
			{
				for (int k = 0; k < instance.slackLevels.Count; k++)
				{
					float f = instance.slackLevels[k];
					stream.WriteByte(109);
					ProtocolParser.WriteSingle(stream, f);
				}
			}
		}

		public static void Serialize(BufferStream stream, IOConnection instance)
		{
			if (instance.niceName != null)
			{
				stream.WriteByte(10);
				ProtocolParser.WriteString(stream, instance.niceName);
			}
			if (instance.type != 0)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.type);
			}
			if (instance.connectedID != default(NetworkableId))
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, instance.connectedID.Value);
			}
			if (instance.connectedToSlot != 0)
			{
				stream.WriteByte(32);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.connectedToSlot);
			}
			if (instance.inUse)
			{
				stream.WriteByte(40);
				ProtocolParser.WriteBool(stream, instance.inUse);
			}
			if (instance.linePointList != null)
			{
				for (int i = 0; i < instance.linePointList.Count; i++)
				{
					LineVec instance2 = instance.linePointList[i];
					stream.WriteByte(50);
					BufferStream.RangeHandle range = stream.GetRange(1);
					int position = stream.Position;
					LineVec.Serialize(stream, instance2);
					int num = stream.Position - position;
					if (num > 127)
					{
						throw new InvalidOperationException("Not enough space was reserved for the length prefix of field linePointList (ProtoBuf.IOEntity.IOConnection.LineVec)");
					}
					Span<byte> span = range.GetSpan();
					ProtocolParser.WriteUInt32((uint)num, span, 0);
				}
			}
			if (instance.colour != 0)
			{
				stream.WriteByte(56);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.colour);
			}
			if (instance.worldSpaceRotation != default(Vector3))
			{
				stream.WriteByte(66);
				BufferStream.RangeHandle range2 = stream.GetRange(1);
				int position2 = stream.Position;
				Vector3Serialized.Serialize(stream, instance.worldSpaceRotation);
				int num2 = stream.Position - position2;
				if (num2 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field worldSpaceRotation (UnityEngine.Vector3)");
				}
				Span<byte> span2 = range2.GetSpan();
				ProtocolParser.WriteUInt32((uint)num2, span2, 0);
			}
			if (instance.lineThickness != 0f)
			{
				stream.WriteByte(77);
				ProtocolParser.WriteSingle(stream, instance.lineThickness);
			}
			if (instance.lineAnchorList != null)
			{
				for (int j = 0; j < instance.lineAnchorList.Count; j++)
				{
					WireLineAnchorInfo instance3 = instance.lineAnchorList[j];
					stream.WriteByte(82);
					BufferStream.RangeHandle range3 = stream.GetRange(5);
					int position3 = stream.Position;
					WireLineAnchorInfo.Serialize(stream, instance3);
					int val = stream.Position - position3;
					Span<byte> span3 = range3.GetSpan();
					int num3 = ProtocolParser.WriteUInt32((uint)val, span3, 0);
					if (num3 < 5)
					{
						span3[num3 - 1] |= 128;
						while (num3 < 4)
						{
							span3[num3++] = 128;
						}
						span3[4] = 0;
					}
				}
			}
			if (instance.originPosition != default(Vector3))
			{
				stream.WriteByte(90);
				BufferStream.RangeHandle range4 = stream.GetRange(1);
				int position4 = stream.Position;
				Vector3Serialized.Serialize(stream, instance.originPosition);
				int num4 = stream.Position - position4;
				if (num4 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field originPosition (UnityEngine.Vector3)");
				}
				Span<byte> span4 = range4.GetSpan();
				ProtocolParser.WriteUInt32((uint)num4, span4, 0);
			}
			if (instance.originRotation != default(Vector3))
			{
				stream.WriteByte(98);
				BufferStream.RangeHandle range5 = stream.GetRange(1);
				int position5 = stream.Position;
				Vector3Serialized.Serialize(stream, instance.originRotation);
				int num5 = stream.Position - position5;
				if (num5 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field originRotation (UnityEngine.Vector3)");
				}
				Span<byte> span5 = range5.GetSpan();
				ProtocolParser.WriteUInt32((uint)num5, span5, 0);
			}
			if (instance.slackLevels != null)
			{
				for (int k = 0; k < instance.slackLevels.Count; k++)
				{
					float f = instance.slackLevels[k];
					stream.WriteByte(109);
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
			action(UidType.NetworkableId, ref connectedID.Value);
			if (linePointList != null)
			{
				for (int i = 0; i < linePointList.Count; i++)
				{
					linePointList[i]?.InspectUids(action);
				}
			}
			if (lineAnchorList != null)
			{
				for (int j = 0; j < lineAnchorList.Count; j++)
				{
					lineAnchorList[j]?.InspectUids(action);
				}
			}
		}
	}

	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<IOConnection> inputs;

	[NonSerialized]
	public List<IOConnection> outputs;

	[NonSerialized]
	public NetworkableId genericEntRef1;

	[NonSerialized]
	public NetworkableId genericEntRef2;

	[NonSerialized]
	public NetworkableId genericEntRef3;

	[NonSerialized]
	public int genericInt1;

	[NonSerialized]
	public int genericInt2;

	[NonSerialized]
	public float genericFloat1;

	[NonSerialized]
	public float genericFloat2;

	[NonSerialized]
	public int genericInt3;

	public static void ResetToPool(IOEntity instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.inputs != null)
		{
			for (int i = 0; i < instance.inputs.Count; i++)
			{
				if (instance.inputs[i] != null)
				{
					instance.inputs[i].ResetToPool();
					instance.inputs[i] = null;
				}
			}
			List<IOConnection> obj = instance.inputs;
			Pool.Free(ref obj, freeElements: false);
			instance.inputs = obj;
		}
		if (instance.outputs != null)
		{
			for (int j = 0; j < instance.outputs.Count; j++)
			{
				if (instance.outputs[j] != null)
				{
					instance.outputs[j].ResetToPool();
					instance.outputs[j] = null;
				}
			}
			List<IOConnection> obj2 = instance.outputs;
			Pool.Free(ref obj2, freeElements: false);
			instance.outputs = obj2;
		}
		instance.genericEntRef1 = default(NetworkableId);
		instance.genericEntRef2 = default(NetworkableId);
		instance.genericEntRef3 = default(NetworkableId);
		instance.genericInt1 = 0;
		instance.genericInt2 = 0;
		instance.genericFloat1 = 0f;
		instance.genericFloat2 = 0f;
		instance.genericInt3 = 0;
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
			throw new Exception("Trying to dispose IOEntity with ShouldPool set to false!");
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

	public void CopyTo(IOEntity instance)
	{
		if (inputs != null)
		{
			instance.inputs = Pool.Get<List<IOConnection>>();
			for (int i = 0; i < inputs.Count; i++)
			{
				IOConnection item = inputs[i].Copy();
				instance.inputs.Add(item);
			}
		}
		else
		{
			instance.inputs = null;
		}
		if (outputs != null)
		{
			instance.outputs = Pool.Get<List<IOConnection>>();
			for (int j = 0; j < outputs.Count; j++)
			{
				IOConnection item2 = outputs[j].Copy();
				instance.outputs.Add(item2);
			}
		}
		else
		{
			instance.outputs = null;
		}
		instance.genericEntRef1 = genericEntRef1;
		instance.genericEntRef2 = genericEntRef2;
		instance.genericEntRef3 = genericEntRef3;
		instance.genericInt1 = genericInt1;
		instance.genericInt2 = genericInt2;
		instance.genericFloat1 = genericFloat1;
		instance.genericFloat2 = genericFloat2;
		instance.genericInt3 = genericInt3;
	}

	public IOEntity Copy()
	{
		IOEntity iOEntity = Pool.Get<IOEntity>();
		CopyTo(iOEntity);
		return iOEntity;
	}

	public static IOEntity Deserialize(BufferStream stream)
	{
		IOEntity iOEntity = Pool.Get<IOEntity>();
		Deserialize(stream, iOEntity, isDelta: false);
		return iOEntity;
	}

	public static IOEntity DeserializeLengthDelimited(BufferStream stream)
	{
		IOEntity iOEntity = Pool.Get<IOEntity>();
		DeserializeLengthDelimited(stream, iOEntity, isDelta: false);
		return iOEntity;
	}

	public static IOEntity DeserializeLength(BufferStream stream, int length)
	{
		IOEntity iOEntity = Pool.Get<IOEntity>();
		DeserializeLength(stream, length, iOEntity, isDelta: false);
		return iOEntity;
	}

	public static IOEntity Deserialize(byte[] buffer)
	{
		IOEntity iOEntity = Pool.Get<IOEntity>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, iOEntity, isDelta: false);
		return iOEntity;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, IOEntity previous)
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

	public static IOEntity Deserialize(BufferStream stream, IOEntity instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.inputs == null)
			{
				instance.inputs = Pool.Get<List<IOConnection>>();
			}
			if (instance.outputs == null)
			{
				instance.outputs = Pool.Get<List<IOConnection>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.IOEntity");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.IOEntity");
				stream.ConsumeRepeatedElement();
				instance.inputs.Add(IOConnection.DeserializeLengthDelimited(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.IOEntity");
				stream.ConsumeRepeatedElement();
				instance.outputs.Add(IOConnection.DeserializeLengthDelimited(stream));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				instance.genericEntRef1 = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				instance.genericEntRef2 = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				instance.genericEntRef3 = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				instance.genericInt1 = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				instance.genericInt2 = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 69:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				instance.genericFloat1 = ProtocolParser.ReadSingle(stream);
				continue;
			case 77:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				instance.genericFloat2 = ProtocolParser.ReadSingle(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				instance.genericInt3 = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static IOEntity DeserializeLengthDelimited(BufferStream stream, IOEntity instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.inputs == null)
			{
				instance.inputs = Pool.Get<List<IOConnection>>();
			}
			if (instance.outputs == null)
			{
				instance.outputs = Pool.Get<List<IOConnection>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.IOEntity");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.IOEntity");
				stream.ConsumeRepeatedElement();
				instance.inputs.Add(IOConnection.DeserializeLengthDelimited(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.IOEntity");
				stream.ConsumeRepeatedElement();
				instance.outputs.Add(IOConnection.DeserializeLengthDelimited(stream));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				instance.genericEntRef1 = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				instance.genericEntRef2 = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				instance.genericEntRef3 = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				instance.genericInt1 = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				instance.genericInt2 = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 69:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				instance.genericFloat1 = ProtocolParser.ReadSingle(stream);
				continue;
			case 77:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				instance.genericFloat2 = ProtocolParser.ReadSingle(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				instance.genericInt3 = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static IOEntity DeserializeLength(BufferStream stream, int length, IOEntity instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.inputs == null)
			{
				instance.inputs = Pool.Get<List<IOConnection>>();
			}
			if (instance.outputs == null)
			{
				instance.outputs = Pool.Get<List<IOConnection>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.IOEntity");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.IOEntity");
				stream.ConsumeRepeatedElement();
				instance.inputs.Add(IOConnection.DeserializeLengthDelimited(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.IOEntity");
				stream.ConsumeRepeatedElement();
				instance.outputs.Add(IOConnection.DeserializeLengthDelimited(stream));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				instance.genericEntRef1 = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				instance.genericEntRef2 = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				instance.genericEntRef3 = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				instance.genericInt1 = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				instance.genericInt2 = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 69:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				instance.genericFloat1 = ProtocolParser.ReadSingle(stream);
				continue;
			case 77:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				instance.genericFloat2 = ProtocolParser.ReadSingle(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				instance.genericInt3 = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.IOEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, IOEntity instance, IOEntity previous)
	{
		if (instance.inputs != null)
		{
			for (int i = 0; i < instance.inputs.Count; i++)
			{
				IOConnection iOConnection = instance.inputs[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				IOConnection.SerializeDelta(stream, iOConnection, iOConnection);
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
		}
		if (instance.outputs != null)
		{
			for (int j = 0; j < instance.outputs.Count; j++)
			{
				IOConnection iOConnection2 = instance.outputs[j];
				stream.WriteByte(18);
				BufferStream.RangeHandle range2 = stream.GetRange(5);
				int position2 = stream.Position;
				IOConnection.SerializeDelta(stream, iOConnection2, iOConnection2);
				int val2 = stream.Position - position2;
				Span<byte> span2 = range2.GetSpan();
				int num2 = ProtocolParser.WriteUInt32((uint)val2, span2, 0);
				if (num2 < 5)
				{
					span2[num2 - 1] |= 128;
					while (num2 < 4)
					{
						span2[num2++] = 128;
					}
					span2[4] = 0;
				}
			}
		}
		stream.WriteByte(24);
		ProtocolParser.WriteUInt64(stream, instance.genericEntRef1.Value);
		stream.WriteByte(32);
		ProtocolParser.WriteUInt64(stream, instance.genericEntRef2.Value);
		stream.WriteByte(40);
		ProtocolParser.WriteUInt64(stream, instance.genericEntRef3.Value);
		if (instance.genericInt1 != previous.genericInt1)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.genericInt1);
		}
		if (instance.genericInt2 != previous.genericInt2)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.genericInt2);
		}
		if (instance.genericFloat1 != previous.genericFloat1)
		{
			stream.WriteByte(69);
			ProtocolParser.WriteSingle(stream, instance.genericFloat1);
		}
		if (instance.genericFloat2 != previous.genericFloat2)
		{
			stream.WriteByte(77);
			ProtocolParser.WriteSingle(stream, instance.genericFloat2);
		}
		if (instance.genericInt3 != previous.genericInt3)
		{
			stream.WriteByte(80);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.genericInt3);
		}
	}

	public static void Serialize(BufferStream stream, IOEntity instance)
	{
		if (instance.inputs != null)
		{
			for (int i = 0; i < instance.inputs.Count; i++)
			{
				IOConnection instance2 = instance.inputs[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				IOConnection.Serialize(stream, instance2);
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
		}
		if (instance.outputs != null)
		{
			for (int j = 0; j < instance.outputs.Count; j++)
			{
				IOConnection instance3 = instance.outputs[j];
				stream.WriteByte(18);
				BufferStream.RangeHandle range2 = stream.GetRange(5);
				int position2 = stream.Position;
				IOConnection.Serialize(stream, instance3);
				int val2 = stream.Position - position2;
				Span<byte> span2 = range2.GetSpan();
				int num2 = ProtocolParser.WriteUInt32((uint)val2, span2, 0);
				if (num2 < 5)
				{
					span2[num2 - 1] |= 128;
					while (num2 < 4)
					{
						span2[num2++] = 128;
					}
					span2[4] = 0;
				}
			}
		}
		if (instance.genericEntRef1 != default(NetworkableId))
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, instance.genericEntRef1.Value);
		}
		if (instance.genericEntRef2 != default(NetworkableId))
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, instance.genericEntRef2.Value);
		}
		if (instance.genericEntRef3 != default(NetworkableId))
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, instance.genericEntRef3.Value);
		}
		if (instance.genericInt1 != 0)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.genericInt1);
		}
		if (instance.genericInt2 != 0)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.genericInt2);
		}
		if (instance.genericFloat1 != 0f)
		{
			stream.WriteByte(69);
			ProtocolParser.WriteSingle(stream, instance.genericFloat1);
		}
		if (instance.genericFloat2 != 0f)
		{
			stream.WriteByte(77);
			ProtocolParser.WriteSingle(stream, instance.genericFloat2);
		}
		if (instance.genericInt3 != 0)
		{
			stream.WriteByte(80);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.genericInt3);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (inputs != null)
		{
			for (int i = 0; i < inputs.Count; i++)
			{
				inputs[i]?.InspectUids(action);
			}
		}
		if (outputs != null)
		{
			for (int j = 0; j < outputs.Count; j++)
			{
				outputs[j]?.InspectUids(action);
			}
		}
		action(UidType.NetworkableId, ref genericEntRef1.Value);
		action(UidType.NetworkableId, ref genericEntRef2.Value);
		action(UidType.NetworkableId, ref genericEntRef3.Value);
	}
}
