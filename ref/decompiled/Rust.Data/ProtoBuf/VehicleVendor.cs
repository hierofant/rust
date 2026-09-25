using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class VehicleVendor : IDisposable, Pool.IPooled, IProto<VehicleVendor>, IProto
{
	public class PlayerStorage : IDisposable, Pool.IPooled, IProto<PlayerStorage>, IProto
	{
		public class PlayerStoredVehicle : IDisposable, Pool.IPooled, IProto<PlayerStoredVehicle>, IProto
		{
			public bool ShouldPool = true;

			private bool _disposed;

			[NonSerialized]
			public string shortname;

			[NonSerialized]
			public string resourcePath;

			[NonSerialized]
			public float health;

			public static void ResetToPool(PlayerStoredVehicle instance)
			{
				if (instance.ShouldPool)
				{
					instance.shortname = string.Empty;
					instance.resourcePath = string.Empty;
					instance.health = 0f;
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
					throw new Exception("Trying to dispose PlayerStoredVehicle with ShouldPool set to false!");
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

			public void CopyTo(PlayerStoredVehicle instance)
			{
				instance.shortname = shortname;
				instance.resourcePath = resourcePath;
				instance.health = health;
			}

			public PlayerStoredVehicle Copy()
			{
				PlayerStoredVehicle playerStoredVehicle = Pool.Get<PlayerStoredVehicle>();
				CopyTo(playerStoredVehicle);
				return playerStoredVehicle;
			}

			public static PlayerStoredVehicle Deserialize(BufferStream stream)
			{
				PlayerStoredVehicle playerStoredVehicle = Pool.Get<PlayerStoredVehicle>();
				Deserialize(stream, playerStoredVehicle, isDelta: false);
				return playerStoredVehicle;
			}

			public static PlayerStoredVehicle DeserializeLengthDelimited(BufferStream stream)
			{
				PlayerStoredVehicle playerStoredVehicle = Pool.Get<PlayerStoredVehicle>();
				DeserializeLengthDelimited(stream, playerStoredVehicle, isDelta: false);
				return playerStoredVehicle;
			}

			public static PlayerStoredVehicle DeserializeLength(BufferStream stream, int length)
			{
				PlayerStoredVehicle playerStoredVehicle = Pool.Get<PlayerStoredVehicle>();
				DeserializeLength(stream, length, playerStoredVehicle, isDelta: false);
				return playerStoredVehicle;
			}

			public static PlayerStoredVehicle Deserialize(byte[] buffer)
			{
				PlayerStoredVehicle playerStoredVehicle = Pool.Get<PlayerStoredVehicle>();
				using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
				Deserialize(stream, playerStoredVehicle, isDelta: false);
				return playerStoredVehicle;
			}

			public void FromProto(BufferStream stream, bool isDelta = false)
			{
				Deserialize(stream, this, isDelta);
			}

			public virtual void WriteToStream(BufferStream stream)
			{
				Serialize(stream, this);
			}

			public virtual void WriteToStreamDelta(BufferStream stream, PlayerStoredVehicle previous)
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

			public static PlayerStoredVehicle Deserialize(BufferStream stream, PlayerStoredVehicle instance, bool isDelta)
			{
				uint lastFieldId = 0u;
				while (true)
				{
					int num = stream.ReadByte();
					if (num == -1 || num == 0)
					{
						break;
					}
					stream.ConsumeFieldOperation("ProtoBuf.VehicleVendor.PlayerStorage.PlayerStoredVehicle");
					switch (num)
					{
					case 10:
						stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VehicleVendor.PlayerStorage.PlayerStoredVehicle");
						instance.shortname = ProtocolParser.ReadString(stream);
						continue;
					case 18:
						stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VehicleVendor.PlayerStorage.PlayerStoredVehicle");
						instance.resourcePath = ProtocolParser.ReadString(stream);
						continue;
					case 29:
						stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VehicleVendor.PlayerStorage.PlayerStoredVehicle");
						instance.health = ProtocolParser.ReadSingle(stream);
						continue;
					}
					Key key = ProtocolParser.ReadKey((byte)num, stream);
					switch (key.Field)
					{
					case 1u:
						stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VehicleVendor.PlayerStorage.PlayerStoredVehicle");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 2u:
						stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VehicleVendor.PlayerStorage.PlayerStoredVehicle");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 3u:
						stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VehicleVendor.PlayerStorage.PlayerStoredVehicle");
						ProtocolParser.SkipKey(stream, key);
						break;
					default:
						stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VehicleVendor.PlayerStorage.PlayerStoredVehicle");
						ProtocolParser.SkipKey(stream, key);
						break;
					}
				}
				return instance;
			}

			public static PlayerStoredVehicle DeserializeLengthDelimited(BufferStream stream, PlayerStoredVehicle instance, bool isDelta)
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
					stream.ConsumeFieldOperation("ProtoBuf.VehicleVendor.PlayerStorage.PlayerStoredVehicle");
					switch (num2)
					{
					case 10:
						stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VehicleVendor.PlayerStorage.PlayerStoredVehicle");
						instance.shortname = ProtocolParser.ReadString(stream);
						continue;
					case 18:
						stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VehicleVendor.PlayerStorage.PlayerStoredVehicle");
						instance.resourcePath = ProtocolParser.ReadString(stream);
						continue;
					case 29:
						stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VehicleVendor.PlayerStorage.PlayerStoredVehicle");
						instance.health = ProtocolParser.ReadSingle(stream);
						continue;
					}
					Key key = ProtocolParser.ReadKey((byte)num2, stream);
					switch (key.Field)
					{
					case 1u:
						stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VehicleVendor.PlayerStorage.PlayerStoredVehicle");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 2u:
						stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VehicleVendor.PlayerStorage.PlayerStoredVehicle");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 3u:
						stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VehicleVendor.PlayerStorage.PlayerStoredVehicle");
						ProtocolParser.SkipKey(stream, key);
						break;
					default:
						stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VehicleVendor.PlayerStorage.PlayerStoredVehicle");
						ProtocolParser.SkipKey(stream, key);
						break;
					}
				}
				return instance;
			}

			public static PlayerStoredVehicle DeserializeLength(BufferStream stream, int length, PlayerStoredVehicle instance, bool isDelta)
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
					stream.ConsumeFieldOperation("ProtoBuf.VehicleVendor.PlayerStorage.PlayerStoredVehicle");
					switch (num2)
					{
					case 10:
						stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VehicleVendor.PlayerStorage.PlayerStoredVehicle");
						instance.shortname = ProtocolParser.ReadString(stream);
						continue;
					case 18:
						stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VehicleVendor.PlayerStorage.PlayerStoredVehicle");
						instance.resourcePath = ProtocolParser.ReadString(stream);
						continue;
					case 29:
						stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VehicleVendor.PlayerStorage.PlayerStoredVehicle");
						instance.health = ProtocolParser.ReadSingle(stream);
						continue;
					}
					Key key = ProtocolParser.ReadKey((byte)num2, stream);
					switch (key.Field)
					{
					case 1u:
						stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VehicleVendor.PlayerStorage.PlayerStoredVehicle");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 2u:
						stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VehicleVendor.PlayerStorage.PlayerStoredVehicle");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 3u:
						stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VehicleVendor.PlayerStorage.PlayerStoredVehicle");
						ProtocolParser.SkipKey(stream, key);
						break;
					default:
						stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VehicleVendor.PlayerStorage.PlayerStoredVehicle");
						ProtocolParser.SkipKey(stream, key);
						break;
					}
				}
				return instance;
			}

			public static void SerializeDelta(BufferStream stream, PlayerStoredVehicle instance, PlayerStoredVehicle previous)
			{
				if (instance.shortname != null && instance.shortname != previous.shortname)
				{
					stream.WriteByte(10);
					ProtocolParser.WriteString(stream, instance.shortname);
				}
				if (instance.resourcePath != null && instance.resourcePath != previous.resourcePath)
				{
					stream.WriteByte(18);
					ProtocolParser.WriteString(stream, instance.resourcePath);
				}
				if (instance.health != previous.health)
				{
					stream.WriteByte(29);
					ProtocolParser.WriteSingle(stream, instance.health);
				}
			}

			public static void Serialize(BufferStream stream, PlayerStoredVehicle instance)
			{
				if (instance.shortname != null)
				{
					stream.WriteByte(10);
					ProtocolParser.WriteString(stream, instance.shortname);
				}
				if (instance.resourcePath != null)
				{
					stream.WriteByte(18);
					ProtocolParser.WriteString(stream, instance.resourcePath);
				}
				if (instance.health != 0f)
				{
					stream.WriteByte(29);
					ProtocolParser.WriteSingle(stream, instance.health);
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
		public ulong userid;

		[NonSerialized]
		public List<PlayerStoredVehicle> storedVehicles;

		public static void ResetToPool(PlayerStorage instance)
		{
			if (!instance.ShouldPool)
			{
				return;
			}
			instance.userid = 0uL;
			if (instance.storedVehicles != null)
			{
				for (int i = 0; i < instance.storedVehicles.Count; i++)
				{
					if (instance.storedVehicles[i] != null)
					{
						instance.storedVehicles[i].ResetToPool();
						instance.storedVehicles[i] = null;
					}
				}
				List<PlayerStoredVehicle> obj = instance.storedVehicles;
				Pool.Free(ref obj, freeElements: false);
				instance.storedVehicles = obj;
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
				throw new Exception("Trying to dispose PlayerStorage with ShouldPool set to false!");
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

		public void CopyTo(PlayerStorage instance)
		{
			instance.userid = userid;
			if (storedVehicles != null)
			{
				instance.storedVehicles = Pool.Get<List<PlayerStoredVehicle>>();
				for (int i = 0; i < storedVehicles.Count; i++)
				{
					PlayerStoredVehicle item = storedVehicles[i].Copy();
					instance.storedVehicles.Add(item);
				}
			}
			else
			{
				instance.storedVehicles = null;
			}
		}

		public PlayerStorage Copy()
		{
			PlayerStorage playerStorage = Pool.Get<PlayerStorage>();
			CopyTo(playerStorage);
			return playerStorage;
		}

		public static PlayerStorage Deserialize(BufferStream stream)
		{
			PlayerStorage playerStorage = Pool.Get<PlayerStorage>();
			Deserialize(stream, playerStorage, isDelta: false);
			return playerStorage;
		}

		public static PlayerStorage DeserializeLengthDelimited(BufferStream stream)
		{
			PlayerStorage playerStorage = Pool.Get<PlayerStorage>();
			DeserializeLengthDelimited(stream, playerStorage, isDelta: false);
			return playerStorage;
		}

		public static PlayerStorage DeserializeLength(BufferStream stream, int length)
		{
			PlayerStorage playerStorage = Pool.Get<PlayerStorage>();
			DeserializeLength(stream, length, playerStorage, isDelta: false);
			return playerStorage;
		}

		public static PlayerStorage Deserialize(byte[] buffer)
		{
			PlayerStorage playerStorage = Pool.Get<PlayerStorage>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, playerStorage, isDelta: false);
			return playerStorage;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, PlayerStorage previous)
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

		public static PlayerStorage Deserialize(BufferStream stream, PlayerStorage instance, bool isDelta)
		{
			if (!isDelta && instance.storedVehicles == null)
			{
				instance.storedVehicles = Pool.Get<List<PlayerStoredVehicle>>();
			}
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.VehicleVendor.PlayerStorage");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VehicleVendor.PlayerStorage");
					instance.userid = ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.VehicleVendor.PlayerStorage");
					stream.ConsumeRepeatedElement();
					instance.storedVehicles.Add(PlayerStoredVehicle.DeserializeLengthDelimited(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VehicleVendor.PlayerStorage");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.VehicleVendor.PlayerStorage");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VehicleVendor.PlayerStorage");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static PlayerStorage DeserializeLengthDelimited(BufferStream stream, PlayerStorage instance, bool isDelta)
		{
			if (!isDelta && instance.storedVehicles == null)
			{
				instance.storedVehicles = Pool.Get<List<PlayerStoredVehicle>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.VehicleVendor.PlayerStorage");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VehicleVendor.PlayerStorage");
					instance.userid = ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.VehicleVendor.PlayerStorage");
					stream.ConsumeRepeatedElement();
					instance.storedVehicles.Add(PlayerStoredVehicle.DeserializeLengthDelimited(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VehicleVendor.PlayerStorage");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.VehicleVendor.PlayerStorage");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VehicleVendor.PlayerStorage");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static PlayerStorage DeserializeLength(BufferStream stream, int length, PlayerStorage instance, bool isDelta)
		{
			if (!isDelta && instance.storedVehicles == null)
			{
				instance.storedVehicles = Pool.Get<List<PlayerStoredVehicle>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.VehicleVendor.PlayerStorage");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VehicleVendor.PlayerStorage");
					instance.userid = ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.VehicleVendor.PlayerStorage");
					stream.ConsumeRepeatedElement();
					instance.storedVehicles.Add(PlayerStoredVehicle.DeserializeLengthDelimited(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VehicleVendor.PlayerStorage");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.VehicleVendor.PlayerStorage");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VehicleVendor.PlayerStorage");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, PlayerStorage instance, PlayerStorage previous)
		{
			if (instance.userid != previous.userid)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, instance.userid);
			}
			if (instance.storedVehicles == null)
			{
				return;
			}
			for (int i = 0; i < instance.storedVehicles.Count; i++)
			{
				PlayerStoredVehicle playerStoredVehicle = instance.storedVehicles[i];
				stream.WriteByte(18);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				PlayerStoredVehicle.SerializeDelta(stream, playerStoredVehicle, playerStoredVehicle);
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

		public static void Serialize(BufferStream stream, PlayerStorage instance)
		{
			if (instance.userid != 0L)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, instance.userid);
			}
			if (instance.storedVehicles == null)
			{
				return;
			}
			for (int i = 0; i < instance.storedVehicles.Count; i++)
			{
				PlayerStoredVehicle instance2 = instance.storedVehicles[i];
				stream.WriteByte(18);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				PlayerStoredVehicle.Serialize(stream, instance2);
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

		public void ToProto(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public void InspectUids(UidInspector<ulong> action)
		{
			if (storedVehicles != null)
			{
				for (int i = 0; i < storedVehicles.Count; i++)
				{
					storedVehicles[i]?.InspectUids(action);
				}
			}
		}
	}

	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<PlayerStorage> playerStorage;

	[NonSerialized]
	public NetworkableId spawnerRef;

	public static void ResetToPool(VehicleVendor instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.playerStorage != null)
		{
			for (int i = 0; i < instance.playerStorage.Count; i++)
			{
				if (instance.playerStorage[i] != null)
				{
					instance.playerStorage[i].ResetToPool();
					instance.playerStorage[i] = null;
				}
			}
			List<PlayerStorage> obj = instance.playerStorage;
			Pool.Free(ref obj, freeElements: false);
			instance.playerStorage = obj;
		}
		instance.spawnerRef = default(NetworkableId);
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
			throw new Exception("Trying to dispose VehicleVendor with ShouldPool set to false!");
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

	public void CopyTo(VehicleVendor instance)
	{
		if (playerStorage != null)
		{
			instance.playerStorage = Pool.Get<List<PlayerStorage>>();
			for (int i = 0; i < playerStorage.Count; i++)
			{
				PlayerStorage item = playerStorage[i].Copy();
				instance.playerStorage.Add(item);
			}
		}
		else
		{
			instance.playerStorage = null;
		}
		instance.spawnerRef = spawnerRef;
	}

	public VehicleVendor Copy()
	{
		VehicleVendor vehicleVendor = Pool.Get<VehicleVendor>();
		CopyTo(vehicleVendor);
		return vehicleVendor;
	}

	public static VehicleVendor Deserialize(BufferStream stream)
	{
		VehicleVendor vehicleVendor = Pool.Get<VehicleVendor>();
		Deserialize(stream, vehicleVendor, isDelta: false);
		return vehicleVendor;
	}

	public static VehicleVendor DeserializeLengthDelimited(BufferStream stream)
	{
		VehicleVendor vehicleVendor = Pool.Get<VehicleVendor>();
		DeserializeLengthDelimited(stream, vehicleVendor, isDelta: false);
		return vehicleVendor;
	}

	public static VehicleVendor DeserializeLength(BufferStream stream, int length)
	{
		VehicleVendor vehicleVendor = Pool.Get<VehicleVendor>();
		DeserializeLength(stream, length, vehicleVendor, isDelta: false);
		return vehicleVendor;
	}

	public static VehicleVendor Deserialize(byte[] buffer)
	{
		VehicleVendor vehicleVendor = Pool.Get<VehicleVendor>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, vehicleVendor, isDelta: false);
		return vehicleVendor;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, VehicleVendor previous)
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

	public static VehicleVendor Deserialize(BufferStream stream, VehicleVendor instance, bool isDelta)
	{
		if (!isDelta && instance.playerStorage == null)
		{
			instance.playerStorage = Pool.Get<List<PlayerStorage>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.VehicleVendor");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VehicleVendor");
				stream.ConsumeRepeatedElement();
				instance.playerStorage.Add(PlayerStorage.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VehicleVendor");
				instance.spawnerRef = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VehicleVendor");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VehicleVendor");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VehicleVendor");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static VehicleVendor DeserializeLengthDelimited(BufferStream stream, VehicleVendor instance, bool isDelta)
	{
		if (!isDelta && instance.playerStorage == null)
		{
			instance.playerStorage = Pool.Get<List<PlayerStorage>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.VehicleVendor");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VehicleVendor");
				stream.ConsumeRepeatedElement();
				instance.playerStorage.Add(PlayerStorage.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VehicleVendor");
				instance.spawnerRef = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VehicleVendor");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VehicleVendor");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VehicleVendor");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static VehicleVendor DeserializeLength(BufferStream stream, int length, VehicleVendor instance, bool isDelta)
	{
		if (!isDelta && instance.playerStorage == null)
		{
			instance.playerStorage = Pool.Get<List<PlayerStorage>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.VehicleVendor");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VehicleVendor");
				stream.ConsumeRepeatedElement();
				instance.playerStorage.Add(PlayerStorage.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VehicleVendor");
				instance.spawnerRef = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VehicleVendor");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VehicleVendor");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VehicleVendor");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, VehicleVendor instance, VehicleVendor previous)
	{
		if (instance.playerStorage != null)
		{
			for (int i = 0; i < instance.playerStorage.Count; i++)
			{
				PlayerStorage playerStorage = instance.playerStorage[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				PlayerStorage.SerializeDelta(stream, playerStorage, playerStorage);
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
		stream.WriteByte(16);
		ProtocolParser.WriteUInt64(stream, instance.spawnerRef.Value);
	}

	public static void Serialize(BufferStream stream, VehicleVendor instance)
	{
		if (instance.playerStorage != null)
		{
			for (int i = 0; i < instance.playerStorage.Count; i++)
			{
				PlayerStorage instance2 = instance.playerStorage[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				PlayerStorage.Serialize(stream, instance2);
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
		if (instance.spawnerRef != default(NetworkableId))
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, instance.spawnerRef.Value);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (playerStorage != null)
		{
			for (int i = 0; i < playerStorage.Count; i++)
			{
				playerStorage[i]?.InspectUids(action);
			}
		}
		action(UidType.NetworkableId, ref spawnerRef.Value);
	}
}
