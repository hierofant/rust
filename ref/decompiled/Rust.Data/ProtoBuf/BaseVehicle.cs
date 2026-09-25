using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class BaseVehicle : IDisposable, Pool.IPooled, IProto<BaseVehicle>, IProto
{
	public class MountPoint : IDisposable, Pool.IPooled, IProto<MountPoint>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public int index;

		[NonSerialized]
		public NetworkableId mountableId;

		public static void ResetToPool(MountPoint instance)
		{
			if (instance.ShouldPool)
			{
				instance.index = 0;
				instance.mountableId = default(NetworkableId);
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
				throw new Exception("Trying to dispose MountPoint with ShouldPool set to false!");
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

		public void CopyTo(MountPoint instance)
		{
			instance.index = index;
			instance.mountableId = mountableId;
		}

		public MountPoint Copy()
		{
			MountPoint mountPoint = Pool.Get<MountPoint>();
			CopyTo(mountPoint);
			return mountPoint;
		}

		public static MountPoint Deserialize(BufferStream stream)
		{
			MountPoint mountPoint = Pool.Get<MountPoint>();
			Deserialize(stream, mountPoint, isDelta: false);
			return mountPoint;
		}

		public static MountPoint DeserializeLengthDelimited(BufferStream stream)
		{
			MountPoint mountPoint = Pool.Get<MountPoint>();
			DeserializeLengthDelimited(stream, mountPoint, isDelta: false);
			return mountPoint;
		}

		public static MountPoint DeserializeLength(BufferStream stream, int length)
		{
			MountPoint mountPoint = Pool.Get<MountPoint>();
			DeserializeLength(stream, length, mountPoint, isDelta: false);
			return mountPoint;
		}

		public static MountPoint Deserialize(byte[] buffer)
		{
			MountPoint mountPoint = Pool.Get<MountPoint>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, mountPoint, isDelta: false);
			return mountPoint;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, MountPoint previous)
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

		public static MountPoint Deserialize(BufferStream stream, MountPoint instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.BaseVehicle.MountPoint");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseVehicle.MountPoint");
					instance.index = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BaseVehicle.MountPoint");
					instance.mountableId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseVehicle.MountPoint");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BaseVehicle.MountPoint");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BaseVehicle.MountPoint");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static MountPoint DeserializeLengthDelimited(BufferStream stream, MountPoint instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.BaseVehicle.MountPoint");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseVehicle.MountPoint");
					instance.index = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BaseVehicle.MountPoint");
					instance.mountableId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseVehicle.MountPoint");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BaseVehicle.MountPoint");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BaseVehicle.MountPoint");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static MountPoint DeserializeLength(BufferStream stream, int length, MountPoint instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.BaseVehicle.MountPoint");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseVehicle.MountPoint");
					instance.index = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BaseVehicle.MountPoint");
					instance.mountableId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseVehicle.MountPoint");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BaseVehicle.MountPoint");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BaseVehicle.MountPoint");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, MountPoint instance, MountPoint previous)
		{
			if (instance.index != previous.index)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.index);
			}
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, instance.mountableId.Value);
		}

		public static void Serialize(BufferStream stream, MountPoint instance)
		{
			if (instance.index != 0)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.index);
			}
			if (instance.mountableId != default(NetworkableId))
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, instance.mountableId.Value);
			}
		}

		public void ToProto(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public void InspectUids(UidInspector<ulong> action)
		{
			action(UidType.NetworkableId, ref mountableId.Value);
		}
	}

	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<MountPoint> mountPoints;

	public static void ResetToPool(BaseVehicle instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.mountPoints != null)
		{
			for (int i = 0; i < instance.mountPoints.Count; i++)
			{
				if (instance.mountPoints[i] != null)
				{
					instance.mountPoints[i].ResetToPool();
					instance.mountPoints[i] = null;
				}
			}
			List<MountPoint> obj = instance.mountPoints;
			Pool.Free(ref obj, freeElements: false);
			instance.mountPoints = obj;
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
			throw new Exception("Trying to dispose BaseVehicle with ShouldPool set to false!");
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

	public void CopyTo(BaseVehicle instance)
	{
		if (mountPoints != null)
		{
			instance.mountPoints = Pool.Get<List<MountPoint>>();
			for (int i = 0; i < mountPoints.Count; i++)
			{
				MountPoint item = mountPoints[i].Copy();
				instance.mountPoints.Add(item);
			}
		}
		else
		{
			instance.mountPoints = null;
		}
	}

	public BaseVehicle Copy()
	{
		BaseVehicle baseVehicle = Pool.Get<BaseVehicle>();
		CopyTo(baseVehicle);
		return baseVehicle;
	}

	public static BaseVehicle Deserialize(BufferStream stream)
	{
		BaseVehicle baseVehicle = Pool.Get<BaseVehicle>();
		Deserialize(stream, baseVehicle, isDelta: false);
		return baseVehicle;
	}

	public static BaseVehicle DeserializeLengthDelimited(BufferStream stream)
	{
		BaseVehicle baseVehicle = Pool.Get<BaseVehicle>();
		DeserializeLengthDelimited(stream, baseVehicle, isDelta: false);
		return baseVehicle;
	}

	public static BaseVehicle DeserializeLength(BufferStream stream, int length)
	{
		BaseVehicle baseVehicle = Pool.Get<BaseVehicle>();
		DeserializeLength(stream, length, baseVehicle, isDelta: false);
		return baseVehicle;
	}

	public static BaseVehicle Deserialize(byte[] buffer)
	{
		BaseVehicle baseVehicle = Pool.Get<BaseVehicle>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, baseVehicle, isDelta: false);
		return baseVehicle;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, BaseVehicle previous)
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

	public static BaseVehicle Deserialize(BufferStream stream, BaseVehicle instance, bool isDelta)
	{
		if (!isDelta && instance.mountPoints == null)
		{
			instance.mountPoints = Pool.Get<List<MountPoint>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.BaseVehicle");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.BaseVehicle");
				stream.ConsumeRepeatedElement();
				instance.mountPoints.Add(MountPoint.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.BaseVehicle");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BaseVehicle");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static BaseVehicle DeserializeLengthDelimited(BufferStream stream, BaseVehicle instance, bool isDelta)
	{
		if (!isDelta && instance.mountPoints == null)
		{
			instance.mountPoints = Pool.Get<List<MountPoint>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.BaseVehicle");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.BaseVehicle");
				stream.ConsumeRepeatedElement();
				instance.mountPoints.Add(MountPoint.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.BaseVehicle");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BaseVehicle");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static BaseVehicle DeserializeLength(BufferStream stream, int length, BaseVehicle instance, bool isDelta)
	{
		if (!isDelta && instance.mountPoints == null)
		{
			instance.mountPoints = Pool.Get<List<MountPoint>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.BaseVehicle");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.BaseVehicle");
				stream.ConsumeRepeatedElement();
				instance.mountPoints.Add(MountPoint.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.BaseVehicle");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BaseVehicle");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, BaseVehicle instance, BaseVehicle previous)
	{
		if (instance.mountPoints == null)
		{
			return;
		}
		for (int i = 0; i < instance.mountPoints.Count; i++)
		{
			MountPoint mountPoint = instance.mountPoints[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			MountPoint.SerializeDelta(stream, mountPoint, mountPoint);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field mountPoints (ProtoBuf.BaseVehicle.MountPoint)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public static void Serialize(BufferStream stream, BaseVehicle instance)
	{
		if (instance.mountPoints == null)
		{
			return;
		}
		for (int i = 0; i < instance.mountPoints.Count; i++)
		{
			MountPoint instance2 = instance.mountPoints[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			MountPoint.Serialize(stream, instance2);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field mountPoints (ProtoBuf.BaseVehicle.MountPoint)");
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
		if (mountPoints != null)
		{
			for (int i = 0; i < mountPoints.Count; i++)
			{
				mountPoints[i]?.InspectUids(action);
			}
		}
	}
}
