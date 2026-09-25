using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class PatternFirework : IDisposable, Pool.IPooled, IProto<PatternFirework>, IProto
{
	public class Design : IDisposable, Pool.IPooled, IProto<Design>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public List<Star> stars;

		[NonSerialized]
		public ulong editedBy;

		public static void ResetToPool(Design instance)
		{
			if (!instance.ShouldPool)
			{
				return;
			}
			if (instance.stars != null)
			{
				for (int i = 0; i < instance.stars.Count; i++)
				{
					if (instance.stars[i] != null)
					{
						instance.stars[i].ResetToPool();
						instance.stars[i] = null;
					}
				}
				List<Star> obj = instance.stars;
				Pool.Free(ref obj, freeElements: false);
				instance.stars = obj;
			}
			instance.editedBy = 0uL;
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
				throw new Exception("Trying to dispose Design with ShouldPool set to false!");
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

		public void CopyTo(Design instance)
		{
			if (stars != null)
			{
				instance.stars = Pool.Get<List<Star>>();
				for (int i = 0; i < stars.Count; i++)
				{
					Star item = stars[i].Copy();
					instance.stars.Add(item);
				}
			}
			else
			{
				instance.stars = null;
			}
			instance.editedBy = editedBy;
		}

		public Design Copy()
		{
			Design design = Pool.Get<Design>();
			CopyTo(design);
			return design;
		}

		public static Design Deserialize(BufferStream stream)
		{
			Design design = Pool.Get<Design>();
			Deserialize(stream, design, isDelta: false);
			return design;
		}

		public static Design DeserializeLengthDelimited(BufferStream stream)
		{
			Design design = Pool.Get<Design>();
			DeserializeLengthDelimited(stream, design, isDelta: false);
			return design;
		}

		public static Design DeserializeLength(BufferStream stream, int length)
		{
			Design design = Pool.Get<Design>();
			DeserializeLength(stream, length, design, isDelta: false);
			return design;
		}

		public static Design Deserialize(byte[] buffer)
		{
			Design design = Pool.Get<Design>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, design, isDelta: false);
			return design;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, Design previous)
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

		public static Design Deserialize(BufferStream stream, Design instance, bool isDelta)
		{
			if (!isDelta && instance.stars == null)
			{
				instance.stars = Pool.Get<List<Star>>();
			}
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.PatternFirework.Design");
				switch (num)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.PatternFirework.Design");
					stream.ConsumeRepeatedElement();
					instance.stars.Add(Star.DeserializeLengthDelimited(stream));
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.Design");
					instance.editedBy = ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.PatternFirework.Design");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.Design");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PatternFirework.Design");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Design DeserializeLengthDelimited(BufferStream stream, Design instance, bool isDelta)
		{
			if (!isDelta && instance.stars == null)
			{
				instance.stars = Pool.Get<List<Star>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.PatternFirework.Design");
				switch (num2)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.PatternFirework.Design");
					stream.ConsumeRepeatedElement();
					instance.stars.Add(Star.DeserializeLengthDelimited(stream));
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.Design");
					instance.editedBy = ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.PatternFirework.Design");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.Design");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PatternFirework.Design");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Design DeserializeLength(BufferStream stream, int length, Design instance, bool isDelta)
		{
			if (!isDelta && instance.stars == null)
			{
				instance.stars = Pool.Get<List<Star>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.PatternFirework.Design");
				switch (num2)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.PatternFirework.Design");
					stream.ConsumeRepeatedElement();
					instance.stars.Add(Star.DeserializeLengthDelimited(stream));
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.Design");
					instance.editedBy = ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.PatternFirework.Design");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.Design");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PatternFirework.Design");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, Design instance, Design previous)
		{
			if (instance.stars != null)
			{
				for (int i = 0; i < instance.stars.Count; i++)
				{
					Star star = instance.stars[i];
					stream.WriteByte(10);
					BufferStream.RangeHandle range = stream.GetRange(1);
					int position = stream.Position;
					Star.SerializeDelta(stream, star, star);
					int num = stream.Position - position;
					if (num > 127)
					{
						throw new InvalidOperationException("Not enough space was reserved for the length prefix of field stars (ProtoBuf.PatternFirework.Star)");
					}
					Span<byte> span = range.GetSpan();
					ProtocolParser.WriteUInt32((uint)num, span, 0);
				}
			}
			if (instance.editedBy != previous.editedBy)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, instance.editedBy);
			}
		}

		public static void Serialize(BufferStream stream, Design instance)
		{
			if (instance.stars != null)
			{
				for (int i = 0; i < instance.stars.Count; i++)
				{
					Star instance2 = instance.stars[i];
					stream.WriteByte(10);
					BufferStream.RangeHandle range = stream.GetRange(1);
					int position = stream.Position;
					Star.Serialize(stream, instance2);
					int num = stream.Position - position;
					if (num > 127)
					{
						throw new InvalidOperationException("Not enough space was reserved for the length prefix of field stars (ProtoBuf.PatternFirework.Star)");
					}
					Span<byte> span = range.GetSpan();
					ProtocolParser.WriteUInt32((uint)num, span, 0);
				}
			}
			if (instance.editedBy != 0L)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, instance.editedBy);
			}
		}

		public void ToProto(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public void InspectUids(UidInspector<ulong> action)
		{
			if (stars != null)
			{
				for (int i = 0; i < stars.Count; i++)
				{
					stars[i]?.InspectUids(action);
				}
			}
		}
	}

	public class SavedDesign : IDisposable, Pool.IPooled, IProto<SavedDesign>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public int version;

		[NonSerialized]
		public string name;

		[NonSerialized]
		public Design design;

		public static void ResetToPool(SavedDesign instance)
		{
			if (instance.ShouldPool)
			{
				instance.version = 0;
				instance.name = string.Empty;
				if (instance.design != null)
				{
					instance.design.ResetToPool();
					instance.design = null;
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
				throw new Exception("Trying to dispose SavedDesign with ShouldPool set to false!");
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

		public void CopyTo(SavedDesign instance)
		{
			instance.version = version;
			instance.name = name;
			if (design != null)
			{
				if (instance.design == null)
				{
					instance.design = design.Copy();
				}
				else
				{
					design.CopyTo(instance.design);
				}
			}
			else
			{
				instance.design = null;
			}
		}

		public SavedDesign Copy()
		{
			SavedDesign savedDesign = Pool.Get<SavedDesign>();
			CopyTo(savedDesign);
			return savedDesign;
		}

		public static SavedDesign Deserialize(BufferStream stream)
		{
			SavedDesign savedDesign = Pool.Get<SavedDesign>();
			Deserialize(stream, savedDesign, isDelta: false);
			return savedDesign;
		}

		public static SavedDesign DeserializeLengthDelimited(BufferStream stream)
		{
			SavedDesign savedDesign = Pool.Get<SavedDesign>();
			DeserializeLengthDelimited(stream, savedDesign, isDelta: false);
			return savedDesign;
		}

		public static SavedDesign DeserializeLength(BufferStream stream, int length)
		{
			SavedDesign savedDesign = Pool.Get<SavedDesign>();
			DeserializeLength(stream, length, savedDesign, isDelta: false);
			return savedDesign;
		}

		public static SavedDesign Deserialize(byte[] buffer)
		{
			SavedDesign savedDesign = Pool.Get<SavedDesign>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, savedDesign, isDelta: false);
			return savedDesign;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, SavedDesign previous)
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

		public static SavedDesign Deserialize(BufferStream stream, SavedDesign instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.PatternFirework.SavedDesign");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.SavedDesign");
					instance.version = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.SavedDesign");
					instance.name = ProtocolParser.ReadString(stream);
					continue;
				case 26:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.SavedDesign");
					if (instance.design == null)
					{
						instance.design = Design.DeserializeLengthDelimited(stream);
					}
					else
					{
						Design.DeserializeLengthDelimited(stream, instance.design, isDelta);
					}
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.SavedDesign");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.SavedDesign");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.SavedDesign");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PatternFirework.SavedDesign");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static SavedDesign DeserializeLengthDelimited(BufferStream stream, SavedDesign instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.PatternFirework.SavedDesign");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.SavedDesign");
					instance.version = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.SavedDesign");
					instance.name = ProtocolParser.ReadString(stream);
					continue;
				case 26:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.SavedDesign");
					if (instance.design == null)
					{
						instance.design = Design.DeserializeLengthDelimited(stream);
					}
					else
					{
						Design.DeserializeLengthDelimited(stream, instance.design, isDelta);
					}
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.SavedDesign");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.SavedDesign");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.SavedDesign");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PatternFirework.SavedDesign");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static SavedDesign DeserializeLength(BufferStream stream, int length, SavedDesign instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.PatternFirework.SavedDesign");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.SavedDesign");
					instance.version = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.SavedDesign");
					instance.name = ProtocolParser.ReadString(stream);
					continue;
				case 26:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.SavedDesign");
					if (instance.design == null)
					{
						instance.design = Design.DeserializeLengthDelimited(stream);
					}
					else
					{
						Design.DeserializeLengthDelimited(stream, instance.design, isDelta);
					}
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.SavedDesign");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.SavedDesign");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.SavedDesign");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PatternFirework.SavedDesign");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, SavedDesign instance, SavedDesign previous)
		{
			if (instance.version != previous.version)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.version);
			}
			if (instance.name != previous.name)
			{
				if (instance.name == null)
				{
					throw new ArgumentNullException("name", "Required by proto specification.");
				}
				stream.WriteByte(18);
				ProtocolParser.WriteString(stream, instance.name);
			}
			if (instance.design == null)
			{
				throw new ArgumentNullException("design", "Required by proto specification.");
			}
			stream.WriteByte(26);
			BufferStream.RangeHandle range = stream.GetRange(3);
			int position = stream.Position;
			Design.SerializeDelta(stream, instance.design, previous.design);
			int num = stream.Position - position;
			if (num > 2097151)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field design (ProtoBuf.PatternFirework.Design)");
			}
			Span<byte> span = range.GetSpan();
			int num2 = ProtocolParser.WriteUInt32((uint)num, span, 0);
			if (num2 < 3)
			{
				span[num2 - 1] |= 128;
				while (num2 < 2)
				{
					span[num2++] = 128;
				}
				span[2] = 0;
			}
		}

		public static void Serialize(BufferStream stream, SavedDesign instance)
		{
			if (instance.version != 0)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.version);
			}
			if (instance.name == null)
			{
				throw new ArgumentNullException("name", "Required by proto specification.");
			}
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.name);
			if (instance.design == null)
			{
				throw new ArgumentNullException("design", "Required by proto specification.");
			}
			stream.WriteByte(26);
			BufferStream.RangeHandle range = stream.GetRange(3);
			int position = stream.Position;
			Design.Serialize(stream, instance.design);
			int num = stream.Position - position;
			if (num > 2097151)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field design (ProtoBuf.PatternFirework.Design)");
			}
			Span<byte> span = range.GetSpan();
			int num2 = ProtocolParser.WriteUInt32((uint)num, span, 0);
			if (num2 < 3)
			{
				span[num2 - 1] |= 128;
				while (num2 < 2)
				{
					span[num2++] = 128;
				}
				span[2] = 0;
			}
		}

		public void ToProto(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public void InspectUids(UidInspector<ulong> action)
		{
			design?.InspectUids(action);
		}
	}

	public class Star : IDisposable, Pool.IPooled, IProto<Star>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public Vector2 position;

		[NonSerialized]
		public Color color;

		public static void ResetToPool(Star instance)
		{
			if (instance.ShouldPool)
			{
				instance.position = default(Vector2);
				instance.color = default(Color);
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
				throw new Exception("Trying to dispose Star with ShouldPool set to false!");
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

		public void CopyTo(Star instance)
		{
			instance.position = position;
			instance.color = color;
		}

		public Star Copy()
		{
			Star star = Pool.Get<Star>();
			CopyTo(star);
			return star;
		}

		public static Star Deserialize(BufferStream stream)
		{
			Star star = Pool.Get<Star>();
			Deserialize(stream, star, isDelta: false);
			return star;
		}

		public static Star DeserializeLengthDelimited(BufferStream stream)
		{
			Star star = Pool.Get<Star>();
			DeserializeLengthDelimited(stream, star, isDelta: false);
			return star;
		}

		public static Star DeserializeLength(BufferStream stream, int length)
		{
			Star star = Pool.Get<Star>();
			DeserializeLength(stream, length, star, isDelta: false);
			return star;
		}

		public static Star Deserialize(byte[] buffer)
		{
			Star star = Pool.Get<Star>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, star, isDelta: false);
			return star;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, Star previous)
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

		public static Star Deserialize(BufferStream stream, Star instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.PatternFirework.Star");
				switch (num)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.Star");
					Vector2Serialized.DeserializeLengthDelimited(stream, ref instance.position, isDelta);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.Star");
					ColorSerialized.DeserializeLengthDelimited(stream, ref instance.color, isDelta);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.Star");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.Star");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PatternFirework.Star");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Star DeserializeLengthDelimited(BufferStream stream, Star instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.PatternFirework.Star");
				switch (num2)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.Star");
					Vector2Serialized.DeserializeLengthDelimited(stream, ref instance.position, isDelta);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.Star");
					ColorSerialized.DeserializeLengthDelimited(stream, ref instance.color, isDelta);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.Star");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.Star");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PatternFirework.Star");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Star DeserializeLength(BufferStream stream, int length, Star instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.PatternFirework.Star");
				switch (num2)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.Star");
					Vector2Serialized.DeserializeLengthDelimited(stream, ref instance.position, isDelta);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.Star");
					ColorSerialized.DeserializeLengthDelimited(stream, ref instance.color, isDelta);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.Star");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PatternFirework.Star");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PatternFirework.Star");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, Star instance, Star previous)
		{
			if (instance.position != previous.position)
			{
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int num = stream.Position;
				Vector2Serialized.SerializeDelta(stream, instance.position, previous.position);
				int num2 = stream.Position - num;
				if (num2 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field position (UnityEngine.Vector2)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num2, span, 0);
			}
			if (instance.color != previous.color)
			{
				stream.WriteByte(18);
				BufferStream.RangeHandle range2 = stream.GetRange(1);
				int num3 = stream.Position;
				ColorSerialized.SerializeDelta(stream, instance.color, previous.color);
				int num4 = stream.Position - num3;
				if (num4 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field color (UnityEngine.Color)");
				}
				Span<byte> span2 = range2.GetSpan();
				ProtocolParser.WriteUInt32((uint)num4, span2, 0);
			}
		}

		public static void Serialize(BufferStream stream, Star instance)
		{
			if (instance.position != default(Vector2))
			{
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int num = stream.Position;
				Vector2Serialized.Serialize(stream, instance.position);
				int num2 = stream.Position - num;
				if (num2 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field position (UnityEngine.Vector2)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num2, span, 0);
			}
			if (instance.color != default(Color))
			{
				stream.WriteByte(18);
				BufferStream.RangeHandle range2 = stream.GetRange(1);
				int num3 = stream.Position;
				ColorSerialized.Serialize(stream, instance.color);
				int num4 = stream.Position - num3;
				if (num4 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field color (UnityEngine.Color)");
				}
				Span<byte> span2 = range2.GetSpan();
				ProtocolParser.WriteUInt32((uint)num4, span2, 0);
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
	public Design design;

	[NonSerialized]
	public int shellFuseLength;

	public static void ResetToPool(PatternFirework instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.design != null)
			{
				instance.design.ResetToPool();
				instance.design = null;
			}
			instance.shellFuseLength = 0;
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
			throw new Exception("Trying to dispose PatternFirework with ShouldPool set to false!");
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

	public void CopyTo(PatternFirework instance)
	{
		if (design != null)
		{
			if (instance.design == null)
			{
				instance.design = design.Copy();
			}
			else
			{
				design.CopyTo(instance.design);
			}
		}
		else
		{
			instance.design = null;
		}
		instance.shellFuseLength = shellFuseLength;
	}

	public PatternFirework Copy()
	{
		PatternFirework patternFirework = Pool.Get<PatternFirework>();
		CopyTo(patternFirework);
		return patternFirework;
	}

	public static PatternFirework Deserialize(BufferStream stream)
	{
		PatternFirework patternFirework = Pool.Get<PatternFirework>();
		Deserialize(stream, patternFirework, isDelta: false);
		return patternFirework;
	}

	public static PatternFirework DeserializeLengthDelimited(BufferStream stream)
	{
		PatternFirework patternFirework = Pool.Get<PatternFirework>();
		DeserializeLengthDelimited(stream, patternFirework, isDelta: false);
		return patternFirework;
	}

	public static PatternFirework DeserializeLength(BufferStream stream, int length)
	{
		PatternFirework patternFirework = Pool.Get<PatternFirework>();
		DeserializeLength(stream, length, patternFirework, isDelta: false);
		return patternFirework;
	}

	public static PatternFirework Deserialize(byte[] buffer)
	{
		PatternFirework patternFirework = Pool.Get<PatternFirework>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, patternFirework, isDelta: false);
		return patternFirework;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, PatternFirework previous)
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

	public static PatternFirework Deserialize(BufferStream stream, PatternFirework instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.PatternFirework");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PatternFirework");
				if (instance.design == null)
				{
					instance.design = Design.DeserializeLengthDelimited(stream);
				}
				else
				{
					Design.DeserializeLengthDelimited(stream, instance.design, isDelta);
				}
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PatternFirework");
				instance.shellFuseLength = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PatternFirework");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PatternFirework");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PatternFirework");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PatternFirework DeserializeLengthDelimited(BufferStream stream, PatternFirework instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.PatternFirework");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PatternFirework");
				if (instance.design == null)
				{
					instance.design = Design.DeserializeLengthDelimited(stream);
				}
				else
				{
					Design.DeserializeLengthDelimited(stream, instance.design, isDelta);
				}
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PatternFirework");
				instance.shellFuseLength = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PatternFirework");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PatternFirework");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PatternFirework");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PatternFirework DeserializeLength(BufferStream stream, int length, PatternFirework instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.PatternFirework");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PatternFirework");
				if (instance.design == null)
				{
					instance.design = Design.DeserializeLengthDelimited(stream);
				}
				else
				{
					Design.DeserializeLengthDelimited(stream, instance.design, isDelta);
				}
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PatternFirework");
				instance.shellFuseLength = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PatternFirework");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PatternFirework");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PatternFirework");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, PatternFirework instance, PatternFirework previous)
	{
		if (instance.design != null)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(3);
			int position = stream.Position;
			Design.SerializeDelta(stream, instance.design, previous.design);
			int num = stream.Position - position;
			if (num > 2097151)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field design (ProtoBuf.PatternFirework.Design)");
			}
			Span<byte> span = range.GetSpan();
			int num2 = ProtocolParser.WriteUInt32((uint)num, span, 0);
			if (num2 < 3)
			{
				span[num2 - 1] |= 128;
				while (num2 < 2)
				{
					span[num2++] = 128;
				}
				span[2] = 0;
			}
		}
		if (instance.shellFuseLength != previous.shellFuseLength)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.shellFuseLength);
		}
	}

	public static void Serialize(BufferStream stream, PatternFirework instance)
	{
		if (instance.design != null)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(3);
			int position = stream.Position;
			Design.Serialize(stream, instance.design);
			int num = stream.Position - position;
			if (num > 2097151)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field design (ProtoBuf.PatternFirework.Design)");
			}
			Span<byte> span = range.GetSpan();
			int num2 = ProtocolParser.WriteUInt32((uint)num, span, 0);
			if (num2 < 3)
			{
				span[num2 - 1] |= 128;
				while (num2 < 2)
				{
					span[num2++] = 128;
				}
				span[2] = 0;
			}
		}
		if (instance.shellFuseLength != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.shellFuseLength);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		design?.InspectUids(action);
	}
}
