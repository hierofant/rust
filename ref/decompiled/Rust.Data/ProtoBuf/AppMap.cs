using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class AppMap : IDisposable, Pool.IPooled, IProto<AppMap>, IProto
{
	public class Monument : IDisposable, Pool.IPooled, IProto<Monument>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public string token;

		[NonSerialized]
		public float x;

		[NonSerialized]
		public float y;

		public static void ResetToPool(Monument instance)
		{
			if (instance.ShouldPool)
			{
				instance.token = string.Empty;
				instance.x = 0f;
				instance.y = 0f;
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
				throw new Exception("Trying to dispose Monument with ShouldPool set to false!");
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

		public void CopyTo(Monument instance)
		{
			instance.token = token;
			instance.x = x;
			instance.y = y;
		}

		public Monument Copy()
		{
			Monument monument = Pool.Get<Monument>();
			CopyTo(monument);
			return monument;
		}

		public static Monument Deserialize(BufferStream stream)
		{
			Monument monument = Pool.Get<Monument>();
			Deserialize(stream, monument, isDelta: false);
			return monument;
		}

		public static Monument DeserializeLengthDelimited(BufferStream stream)
		{
			Monument monument = Pool.Get<Monument>();
			DeserializeLengthDelimited(stream, monument, isDelta: false);
			return monument;
		}

		public static Monument DeserializeLength(BufferStream stream, int length)
		{
			Monument monument = Pool.Get<Monument>();
			DeserializeLength(stream, length, monument, isDelta: false);
			return monument;
		}

		public static Monument Deserialize(byte[] buffer)
		{
			Monument monument = Pool.Get<Monument>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, monument, isDelta: false);
			return monument;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, Monument previous)
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

		public static Monument Deserialize(BufferStream stream, Monument instance, bool isDelta)
		{
			if (!isDelta)
			{
				instance.x = 0f;
				instance.y = 0f;
			}
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.AppMap.Monument");
				switch (num)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppMap.Monument");
					instance.token = ProtocolParser.ReadString(stream);
					continue;
				case 21:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppMap.Monument");
					instance.x = ProtocolParser.ReadSingle(stream);
					continue;
				case 29:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppMap.Monument");
					instance.y = ProtocolParser.ReadSingle(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppMap.Monument");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppMap.Monument");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppMap.Monument");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppMap.Monument");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Monument DeserializeLengthDelimited(BufferStream stream, Monument instance, bool isDelta)
		{
			if (!isDelta)
			{
				instance.x = 0f;
				instance.y = 0f;
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
				stream.ConsumeFieldOperation("ProtoBuf.AppMap.Monument");
				switch (num2)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppMap.Monument");
					instance.token = ProtocolParser.ReadString(stream);
					continue;
				case 21:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppMap.Monument");
					instance.x = ProtocolParser.ReadSingle(stream);
					continue;
				case 29:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppMap.Monument");
					instance.y = ProtocolParser.ReadSingle(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppMap.Monument");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppMap.Monument");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppMap.Monument");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppMap.Monument");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Monument DeserializeLength(BufferStream stream, int length, Monument instance, bool isDelta)
		{
			if (!isDelta)
			{
				instance.x = 0f;
				instance.y = 0f;
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
				stream.ConsumeFieldOperation("ProtoBuf.AppMap.Monument");
				switch (num2)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppMap.Monument");
					instance.token = ProtocolParser.ReadString(stream);
					continue;
				case 21:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppMap.Monument");
					instance.x = ProtocolParser.ReadSingle(stream);
					continue;
				case 29:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppMap.Monument");
					instance.y = ProtocolParser.ReadSingle(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppMap.Monument");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppMap.Monument");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppMap.Monument");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppMap.Monument");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, Monument instance, Monument previous)
		{
			if (instance.token != previous.token)
			{
				if (instance.token == null)
				{
					throw new ArgumentNullException("token", "Required by proto specification.");
				}
				stream.WriteByte(10);
				ProtocolParser.WriteString(stream, instance.token);
			}
			if (instance.x != previous.x)
			{
				stream.WriteByte(21);
				ProtocolParser.WriteSingle(stream, instance.x);
			}
			if (instance.y != previous.y)
			{
				stream.WriteByte(29);
				ProtocolParser.WriteSingle(stream, instance.y);
			}
		}

		public static void Serialize(BufferStream stream, Monument instance)
		{
			if (instance.token == null)
			{
				throw new ArgumentNullException("token", "Required by proto specification.");
			}
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.token);
			if (instance.x != 0f)
			{
				stream.WriteByte(21);
				ProtocolParser.WriteSingle(stream, instance.x);
			}
			if (instance.y != 0f)
			{
				stream.WriteByte(29);
				ProtocolParser.WriteSingle(stream, instance.y);
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
	public uint width;

	[NonSerialized]
	public uint height;

	[NonSerialized]
	public byte[] jpgImage;

	[NonSerialized]
	public int oceanMargin;

	[NonSerialized]
	public List<Monument> monuments;

	[NonSerialized]
	public string background;

	public static void ResetToPool(AppMap instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.width = 0u;
		instance.height = 0u;
		instance.jpgImage = null;
		instance.oceanMargin = 0;
		if (instance.monuments != null)
		{
			for (int i = 0; i < instance.monuments.Count; i++)
			{
				if (instance.monuments[i] != null)
				{
					instance.monuments[i].ResetToPool();
					instance.monuments[i] = null;
				}
			}
			List<Monument> obj = instance.monuments;
			Pool.Free(ref obj, freeElements: false);
			instance.monuments = obj;
		}
		instance.background = string.Empty;
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
			throw new Exception("Trying to dispose AppMap with ShouldPool set to false!");
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

	public void CopyTo(AppMap instance)
	{
		instance.width = width;
		instance.height = height;
		if (jpgImage == null)
		{
			instance.jpgImage = null;
		}
		else
		{
			instance.jpgImage = new byte[jpgImage.Length];
			Array.Copy(jpgImage, instance.jpgImage, instance.jpgImage.Length);
		}
		instance.oceanMargin = oceanMargin;
		if (monuments != null)
		{
			instance.monuments = Pool.Get<List<Monument>>();
			for (int i = 0; i < monuments.Count; i++)
			{
				Monument item = monuments[i].Copy();
				instance.monuments.Add(item);
			}
		}
		else
		{
			instance.monuments = null;
		}
		instance.background = background;
	}

	public AppMap Copy()
	{
		AppMap appMap = Pool.Get<AppMap>();
		CopyTo(appMap);
		return appMap;
	}

	public static AppMap Deserialize(BufferStream stream)
	{
		AppMap appMap = Pool.Get<AppMap>();
		Deserialize(stream, appMap, isDelta: false);
		return appMap;
	}

	public static AppMap DeserializeLengthDelimited(BufferStream stream)
	{
		AppMap appMap = Pool.Get<AppMap>();
		DeserializeLengthDelimited(stream, appMap, isDelta: false);
		return appMap;
	}

	public static AppMap DeserializeLength(BufferStream stream, int length)
	{
		AppMap appMap = Pool.Get<AppMap>();
		DeserializeLength(stream, length, appMap, isDelta: false);
		return appMap;
	}

	public static AppMap Deserialize(byte[] buffer)
	{
		AppMap appMap = Pool.Get<AppMap>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, appMap, isDelta: false);
		return appMap;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, AppMap previous)
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

	public static AppMap Deserialize(BufferStream stream, AppMap instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.width = 0u;
			instance.height = 0u;
			instance.oceanMargin = 0;
			if (instance.monuments == null)
			{
				instance.monuments = Pool.Get<List<Monument>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.AppMap");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppMap");
				instance.width = ProtocolParser.ReadUInt32(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppMap");
				instance.height = ProtocolParser.ReadUInt32(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppMap");
				instance.jpgImage = ProtocolParser.ReadBytes(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppMap");
				instance.oceanMargin = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.AppMap");
				stream.ConsumeRepeatedElement();
				instance.monuments.Add(Monument.DeserializeLengthDelimited(stream));
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppMap");
				instance.background = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppMap");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppMap");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppMap");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppMap");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.AppMap");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppMap");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppMap");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AppMap DeserializeLengthDelimited(BufferStream stream, AppMap instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.width = 0u;
			instance.height = 0u;
			instance.oceanMargin = 0;
			if (instance.monuments == null)
			{
				instance.monuments = Pool.Get<List<Monument>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.AppMap");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppMap");
				instance.width = ProtocolParser.ReadUInt32(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppMap");
				instance.height = ProtocolParser.ReadUInt32(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppMap");
				instance.jpgImage = ProtocolParser.ReadBytes(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppMap");
				instance.oceanMargin = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.AppMap");
				stream.ConsumeRepeatedElement();
				instance.monuments.Add(Monument.DeserializeLengthDelimited(stream));
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppMap");
				instance.background = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppMap");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppMap");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppMap");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppMap");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.AppMap");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppMap");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppMap");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AppMap DeserializeLength(BufferStream stream, int length, AppMap instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.width = 0u;
			instance.height = 0u;
			instance.oceanMargin = 0;
			if (instance.monuments == null)
			{
				instance.monuments = Pool.Get<List<Monument>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.AppMap");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppMap");
				instance.width = ProtocolParser.ReadUInt32(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppMap");
				instance.height = ProtocolParser.ReadUInt32(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppMap");
				instance.jpgImage = ProtocolParser.ReadBytes(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppMap");
				instance.oceanMargin = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.AppMap");
				stream.ConsumeRepeatedElement();
				instance.monuments.Add(Monument.DeserializeLengthDelimited(stream));
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppMap");
				instance.background = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppMap");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppMap");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppMap");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppMap");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.AppMap");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppMap");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppMap");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, AppMap instance, AppMap previous)
	{
		if (instance.width != previous.width)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt32(stream, instance.width);
		}
		if (instance.height != previous.height)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt32(stream, instance.height);
		}
		if (instance.jpgImage == null)
		{
			throw new ArgumentNullException("jpgImage", "Required by proto specification.");
		}
		stream.WriteByte(26);
		ProtocolParser.WriteBytes(stream, instance.jpgImage);
		if (instance.oceanMargin != previous.oceanMargin)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.oceanMargin);
		}
		if (instance.monuments != null)
		{
			for (int i = 0; i < instance.monuments.Count; i++)
			{
				Monument monument = instance.monuments[i];
				stream.WriteByte(42);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				Monument.SerializeDelta(stream, monument, monument);
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
		if (instance.background != null && instance.background != previous.background)
		{
			stream.WriteByte(50);
			ProtocolParser.WriteString(stream, instance.background);
		}
	}

	public static void Serialize(BufferStream stream, AppMap instance)
	{
		if (instance.width != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt32(stream, instance.width);
		}
		if (instance.height != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt32(stream, instance.height);
		}
		if (instance.jpgImage == null)
		{
			throw new ArgumentNullException("jpgImage", "Required by proto specification.");
		}
		stream.WriteByte(26);
		ProtocolParser.WriteBytes(stream, instance.jpgImage);
		if (instance.oceanMargin != 0)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.oceanMargin);
		}
		if (instance.monuments != null)
		{
			for (int i = 0; i < instance.monuments.Count; i++)
			{
				Monument instance2 = instance.monuments[i];
				stream.WriteByte(42);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				Monument.Serialize(stream, instance2);
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
		if (instance.background != null)
		{
			stream.WriteByte(50);
			ProtocolParser.WriteString(stream, instance.background);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (monuments != null)
		{
			for (int i = 0; i < monuments.Count; i++)
			{
				monuments[i]?.InspectUids(action);
			}
		}
	}
}
