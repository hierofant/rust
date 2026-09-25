using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class DemoShot : IDisposable, Pool.IPooled, IProto<DemoShot>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public string shotName;

	[NonSerialized]
	public string demoName;

	[NonSerialized]
	public float shotStart;

	[NonSerialized]
	public float shotLength;

	[NonSerialized]
	public DemoShotVectorTrack camPos;

	[NonSerialized]
	public DemoShotQuaternionTrack camRot;

	[NonSerialized]
	public DemoShotFloatTrack camFov;

	[NonSerialized]
	public DemoShotFloatTrack camDofDist;

	[NonSerialized]
	public DemoShotFloatTrack camDofFocalSize;

	[NonSerialized]
	public DemoShotFloatTrack camDofAperture;

	[NonSerialized]
	public DemoShotParentTrack camParent;

	[NonSerialized]
	public string folderName;

	public static void ResetToPool(DemoShot instance)
	{
		if (instance.ShouldPool)
		{
			instance.shotName = string.Empty;
			instance.demoName = string.Empty;
			instance.shotStart = 0f;
			instance.shotLength = 0f;
			if (instance.camPos != null)
			{
				instance.camPos.ResetToPool();
				instance.camPos = null;
			}
			if (instance.camRot != null)
			{
				instance.camRot.ResetToPool();
				instance.camRot = null;
			}
			if (instance.camFov != null)
			{
				instance.camFov.ResetToPool();
				instance.camFov = null;
			}
			if (instance.camDofDist != null)
			{
				instance.camDofDist.ResetToPool();
				instance.camDofDist = null;
			}
			if (instance.camDofFocalSize != null)
			{
				instance.camDofFocalSize.ResetToPool();
				instance.camDofFocalSize = null;
			}
			if (instance.camDofAperture != null)
			{
				instance.camDofAperture.ResetToPool();
				instance.camDofAperture = null;
			}
			if (instance.camParent != null)
			{
				instance.camParent.ResetToPool();
				instance.camParent = null;
			}
			instance.folderName = string.Empty;
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
			throw new Exception("Trying to dispose DemoShot with ShouldPool set to false!");
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

	public void CopyTo(DemoShot instance)
	{
		instance.shotName = shotName;
		instance.demoName = demoName;
		instance.shotStart = shotStart;
		instance.shotLength = shotLength;
		if (camPos != null)
		{
			if (instance.camPos == null)
			{
				instance.camPos = camPos.Copy();
			}
			else
			{
				camPos.CopyTo(instance.camPos);
			}
		}
		else
		{
			instance.camPos = null;
		}
		if (camRot != null)
		{
			if (instance.camRot == null)
			{
				instance.camRot = camRot.Copy();
			}
			else
			{
				camRot.CopyTo(instance.camRot);
			}
		}
		else
		{
			instance.camRot = null;
		}
		if (camFov != null)
		{
			if (instance.camFov == null)
			{
				instance.camFov = camFov.Copy();
			}
			else
			{
				camFov.CopyTo(instance.camFov);
			}
		}
		else
		{
			instance.camFov = null;
		}
		if (camDofDist != null)
		{
			if (instance.camDofDist == null)
			{
				instance.camDofDist = camDofDist.Copy();
			}
			else
			{
				camDofDist.CopyTo(instance.camDofDist);
			}
		}
		else
		{
			instance.camDofDist = null;
		}
		if (camDofFocalSize != null)
		{
			if (instance.camDofFocalSize == null)
			{
				instance.camDofFocalSize = camDofFocalSize.Copy();
			}
			else
			{
				camDofFocalSize.CopyTo(instance.camDofFocalSize);
			}
		}
		else
		{
			instance.camDofFocalSize = null;
		}
		if (camDofAperture != null)
		{
			if (instance.camDofAperture == null)
			{
				instance.camDofAperture = camDofAperture.Copy();
			}
			else
			{
				camDofAperture.CopyTo(instance.camDofAperture);
			}
		}
		else
		{
			instance.camDofAperture = null;
		}
		if (camParent != null)
		{
			if (instance.camParent == null)
			{
				instance.camParent = camParent.Copy();
			}
			else
			{
				camParent.CopyTo(instance.camParent);
			}
		}
		else
		{
			instance.camParent = null;
		}
		instance.folderName = folderName;
	}

	public DemoShot Copy()
	{
		DemoShot demoShot = Pool.Get<DemoShot>();
		CopyTo(demoShot);
		return demoShot;
	}

	public static DemoShot Deserialize(BufferStream stream)
	{
		DemoShot demoShot = Pool.Get<DemoShot>();
		Deserialize(stream, demoShot, isDelta: false);
		return demoShot;
	}

	public static DemoShot DeserializeLengthDelimited(BufferStream stream)
	{
		DemoShot demoShot = Pool.Get<DemoShot>();
		DeserializeLengthDelimited(stream, demoShot, isDelta: false);
		return demoShot;
	}

	public static DemoShot DeserializeLength(BufferStream stream, int length)
	{
		DemoShot demoShot = Pool.Get<DemoShot>();
		DeserializeLength(stream, length, demoShot, isDelta: false);
		return demoShot;
	}

	public static DemoShot Deserialize(byte[] buffer)
	{
		DemoShot demoShot = Pool.Get<DemoShot>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, demoShot, isDelta: false);
		return demoShot;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, DemoShot previous)
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

	public static DemoShot Deserialize(BufferStream stream, DemoShot instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.DemoShot");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				instance.shotName = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				instance.demoName = ProtocolParser.ReadString(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				instance.shotStart = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				instance.shotLength = ProtocolParser.ReadSingle(stream);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				if (instance.camPos == null)
				{
					instance.camPos = DemoShotVectorTrack.DeserializeLengthDelimited(stream);
				}
				else
				{
					DemoShotVectorTrack.DeserializeLengthDelimited(stream, instance.camPos, isDelta);
				}
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				if (instance.camRot == null)
				{
					instance.camRot = DemoShotQuaternionTrack.DeserializeLengthDelimited(stream);
				}
				else
				{
					DemoShotQuaternionTrack.DeserializeLengthDelimited(stream, instance.camRot, isDelta);
				}
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				if (instance.camFov == null)
				{
					instance.camFov = DemoShotFloatTrack.DeserializeLengthDelimited(stream);
				}
				else
				{
					DemoShotFloatTrack.DeserializeLengthDelimited(stream, instance.camFov, isDelta);
				}
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				if (instance.camDofDist == null)
				{
					instance.camDofDist = DemoShotFloatTrack.DeserializeLengthDelimited(stream);
				}
				else
				{
					DemoShotFloatTrack.DeserializeLengthDelimited(stream, instance.camDofDist, isDelta);
				}
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				if (instance.camDofFocalSize == null)
				{
					instance.camDofFocalSize = DemoShotFloatTrack.DeserializeLengthDelimited(stream);
				}
				else
				{
					DemoShotFloatTrack.DeserializeLengthDelimited(stream, instance.camDofFocalSize, isDelta);
				}
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				if (instance.camDofAperture == null)
				{
					instance.camDofAperture = DemoShotFloatTrack.DeserializeLengthDelimited(stream);
				}
				else
				{
					DemoShotFloatTrack.DeserializeLengthDelimited(stream, instance.camDofAperture, isDelta);
				}
				continue;
			case 98:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				if (instance.camParent == null)
				{
					instance.camParent = DemoShotParentTrack.DeserializeLengthDelimited(stream);
				}
				else
				{
					DemoShotParentTrack.DeserializeLengthDelimited(stream, instance.camParent, isDelta);
				}
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				instance.folderName = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DemoShot DeserializeLengthDelimited(BufferStream stream, DemoShot instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.DemoShot");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				instance.shotName = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				instance.demoName = ProtocolParser.ReadString(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				instance.shotStart = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				instance.shotLength = ProtocolParser.ReadSingle(stream);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				if (instance.camPos == null)
				{
					instance.camPos = DemoShotVectorTrack.DeserializeLengthDelimited(stream);
				}
				else
				{
					DemoShotVectorTrack.DeserializeLengthDelimited(stream, instance.camPos, isDelta);
				}
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				if (instance.camRot == null)
				{
					instance.camRot = DemoShotQuaternionTrack.DeserializeLengthDelimited(stream);
				}
				else
				{
					DemoShotQuaternionTrack.DeserializeLengthDelimited(stream, instance.camRot, isDelta);
				}
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				if (instance.camFov == null)
				{
					instance.camFov = DemoShotFloatTrack.DeserializeLengthDelimited(stream);
				}
				else
				{
					DemoShotFloatTrack.DeserializeLengthDelimited(stream, instance.camFov, isDelta);
				}
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				if (instance.camDofDist == null)
				{
					instance.camDofDist = DemoShotFloatTrack.DeserializeLengthDelimited(stream);
				}
				else
				{
					DemoShotFloatTrack.DeserializeLengthDelimited(stream, instance.camDofDist, isDelta);
				}
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				if (instance.camDofFocalSize == null)
				{
					instance.camDofFocalSize = DemoShotFloatTrack.DeserializeLengthDelimited(stream);
				}
				else
				{
					DemoShotFloatTrack.DeserializeLengthDelimited(stream, instance.camDofFocalSize, isDelta);
				}
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				if (instance.camDofAperture == null)
				{
					instance.camDofAperture = DemoShotFloatTrack.DeserializeLengthDelimited(stream);
				}
				else
				{
					DemoShotFloatTrack.DeserializeLengthDelimited(stream, instance.camDofAperture, isDelta);
				}
				continue;
			case 98:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				if (instance.camParent == null)
				{
					instance.camParent = DemoShotParentTrack.DeserializeLengthDelimited(stream);
				}
				else
				{
					DemoShotParentTrack.DeserializeLengthDelimited(stream, instance.camParent, isDelta);
				}
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				instance.folderName = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DemoShot DeserializeLength(BufferStream stream, int length, DemoShot instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.DemoShot");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				instance.shotName = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				instance.demoName = ProtocolParser.ReadString(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				instance.shotStart = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				instance.shotLength = ProtocolParser.ReadSingle(stream);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				if (instance.camPos == null)
				{
					instance.camPos = DemoShotVectorTrack.DeserializeLengthDelimited(stream);
				}
				else
				{
					DemoShotVectorTrack.DeserializeLengthDelimited(stream, instance.camPos, isDelta);
				}
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				if (instance.camRot == null)
				{
					instance.camRot = DemoShotQuaternionTrack.DeserializeLengthDelimited(stream);
				}
				else
				{
					DemoShotQuaternionTrack.DeserializeLengthDelimited(stream, instance.camRot, isDelta);
				}
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				if (instance.camFov == null)
				{
					instance.camFov = DemoShotFloatTrack.DeserializeLengthDelimited(stream);
				}
				else
				{
					DemoShotFloatTrack.DeserializeLengthDelimited(stream, instance.camFov, isDelta);
				}
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				if (instance.camDofDist == null)
				{
					instance.camDofDist = DemoShotFloatTrack.DeserializeLengthDelimited(stream);
				}
				else
				{
					DemoShotFloatTrack.DeserializeLengthDelimited(stream, instance.camDofDist, isDelta);
				}
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				if (instance.camDofFocalSize == null)
				{
					instance.camDofFocalSize = DemoShotFloatTrack.DeserializeLengthDelimited(stream);
				}
				else
				{
					DemoShotFloatTrack.DeserializeLengthDelimited(stream, instance.camDofFocalSize, isDelta);
				}
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				if (instance.camDofAperture == null)
				{
					instance.camDofAperture = DemoShotFloatTrack.DeserializeLengthDelimited(stream);
				}
				else
				{
					DemoShotFloatTrack.DeserializeLengthDelimited(stream, instance.camDofAperture, isDelta);
				}
				continue;
			case 98:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				if (instance.camParent == null)
				{
					instance.camParent = DemoShotParentTrack.DeserializeLengthDelimited(stream);
				}
				else
				{
					DemoShotParentTrack.DeserializeLengthDelimited(stream, instance.camParent, isDelta);
				}
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				instance.folderName = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DemoShot");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, DemoShot instance, DemoShot previous)
	{
		if (instance.shotName != previous.shotName)
		{
			if (instance.shotName == null)
			{
				throw new ArgumentNullException("shotName", "Required by proto specification.");
			}
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.shotName);
		}
		if (instance.demoName != previous.demoName)
		{
			if (instance.demoName == null)
			{
				throw new ArgumentNullException("demoName", "Required by proto specification.");
			}
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.demoName);
		}
		if (instance.shotStart != previous.shotStart)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.shotStart);
		}
		if (instance.shotLength != previous.shotLength)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.shotLength);
		}
		if (instance.camPos == null)
		{
			throw new ArgumentNullException("camPos", "Required by proto specification.");
		}
		stream.WriteByte(42);
		BufferStream.RangeHandle range = stream.GetRange(3);
		int position = stream.Position;
		DemoShotVectorTrack.SerializeDelta(stream, instance.camPos, previous.camPos);
		int num = stream.Position - position;
		if (num > 2097151)
		{
			throw new InvalidOperationException("Not enough space was reserved for the length prefix of field camPos (ProtoBuf.DemoShotVectorTrack)");
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
		if (instance.camRot == null)
		{
			throw new ArgumentNullException("camRot", "Required by proto specification.");
		}
		stream.WriteByte(50);
		BufferStream.RangeHandle range2 = stream.GetRange(3);
		int position2 = stream.Position;
		DemoShotQuaternionTrack.SerializeDelta(stream, instance.camRot, previous.camRot);
		int num3 = stream.Position - position2;
		if (num3 > 2097151)
		{
			throw new InvalidOperationException("Not enough space was reserved for the length prefix of field camRot (ProtoBuf.DemoShotQuaternionTrack)");
		}
		Span<byte> span2 = range2.GetSpan();
		int num4 = ProtocolParser.WriteUInt32((uint)num3, span2, 0);
		if (num4 < 3)
		{
			span2[num4 - 1] |= 128;
			while (num4 < 2)
			{
				span2[num4++] = 128;
			}
			span2[2] = 0;
		}
		if (instance.camFov == null)
		{
			throw new ArgumentNullException("camFov", "Required by proto specification.");
		}
		stream.WriteByte(58);
		BufferStream.RangeHandle range3 = stream.GetRange(3);
		int position3 = stream.Position;
		DemoShotFloatTrack.SerializeDelta(stream, instance.camFov, previous.camFov);
		int num5 = stream.Position - position3;
		if (num5 > 2097151)
		{
			throw new InvalidOperationException("Not enough space was reserved for the length prefix of field camFov (ProtoBuf.DemoShotFloatTrack)");
		}
		Span<byte> span3 = range3.GetSpan();
		int num6 = ProtocolParser.WriteUInt32((uint)num5, span3, 0);
		if (num6 < 3)
		{
			span3[num6 - 1] |= 128;
			while (num6 < 2)
			{
				span3[num6++] = 128;
			}
			span3[2] = 0;
		}
		if (instance.camDofDist != null)
		{
			stream.WriteByte(74);
			BufferStream.RangeHandle range4 = stream.GetRange(3);
			int position4 = stream.Position;
			DemoShotFloatTrack.SerializeDelta(stream, instance.camDofDist, previous.camDofDist);
			int num7 = stream.Position - position4;
			if (num7 > 2097151)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field camDofDist (ProtoBuf.DemoShotFloatTrack)");
			}
			Span<byte> span4 = range4.GetSpan();
			int num8 = ProtocolParser.WriteUInt32((uint)num7, span4, 0);
			if (num8 < 3)
			{
				span4[num8 - 1] |= 128;
				while (num8 < 2)
				{
					span4[num8++] = 128;
				}
				span4[2] = 0;
			}
		}
		if (instance.camDofFocalSize != null)
		{
			stream.WriteByte(82);
			BufferStream.RangeHandle range5 = stream.GetRange(3);
			int position5 = stream.Position;
			DemoShotFloatTrack.SerializeDelta(stream, instance.camDofFocalSize, previous.camDofFocalSize);
			int num9 = stream.Position - position5;
			if (num9 > 2097151)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field camDofFocalSize (ProtoBuf.DemoShotFloatTrack)");
			}
			Span<byte> span5 = range5.GetSpan();
			int num10 = ProtocolParser.WriteUInt32((uint)num9, span5, 0);
			if (num10 < 3)
			{
				span5[num10 - 1] |= 128;
				while (num10 < 2)
				{
					span5[num10++] = 128;
				}
				span5[2] = 0;
			}
		}
		if (instance.camDofAperture != null)
		{
			stream.WriteByte(90);
			BufferStream.RangeHandle range6 = stream.GetRange(3);
			int position6 = stream.Position;
			DemoShotFloatTrack.SerializeDelta(stream, instance.camDofAperture, previous.camDofAperture);
			int num11 = stream.Position - position6;
			if (num11 > 2097151)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field camDofAperture (ProtoBuf.DemoShotFloatTrack)");
			}
			Span<byte> span6 = range6.GetSpan();
			int num12 = ProtocolParser.WriteUInt32((uint)num11, span6, 0);
			if (num12 < 3)
			{
				span6[num12 - 1] |= 128;
				while (num12 < 2)
				{
					span6[num12++] = 128;
				}
				span6[2] = 0;
			}
		}
		if (instance.camParent != null)
		{
			stream.WriteByte(98);
			BufferStream.RangeHandle range7 = stream.GetRange(5);
			int position7 = stream.Position;
			DemoShotParentTrack.SerializeDelta(stream, instance.camParent, previous.camParent);
			int val = stream.Position - position7;
			Span<byte> span7 = range7.GetSpan();
			int num13 = ProtocolParser.WriteUInt32((uint)val, span7, 0);
			if (num13 < 5)
			{
				span7[num13 - 1] |= 128;
				while (num13 < 4)
				{
					span7[num13++] = 128;
				}
				span7[4] = 0;
			}
		}
		if (instance.folderName != null && instance.folderName != previous.folderName)
		{
			stream.WriteByte(66);
			ProtocolParser.WriteString(stream, instance.folderName);
		}
	}

	public static void Serialize(BufferStream stream, DemoShot instance)
	{
		if (instance.shotName == null)
		{
			throw new ArgumentNullException("shotName", "Required by proto specification.");
		}
		stream.WriteByte(10);
		ProtocolParser.WriteString(stream, instance.shotName);
		if (instance.demoName == null)
		{
			throw new ArgumentNullException("demoName", "Required by proto specification.");
		}
		stream.WriteByte(18);
		ProtocolParser.WriteString(stream, instance.demoName);
		if (instance.shotStart != 0f)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.shotStart);
		}
		if (instance.shotLength != 0f)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.shotLength);
		}
		if (instance.camPos == null)
		{
			throw new ArgumentNullException("camPos", "Required by proto specification.");
		}
		stream.WriteByte(42);
		BufferStream.RangeHandle range = stream.GetRange(3);
		int position = stream.Position;
		DemoShotVectorTrack.Serialize(stream, instance.camPos);
		int num = stream.Position - position;
		if (num > 2097151)
		{
			throw new InvalidOperationException("Not enough space was reserved for the length prefix of field camPos (ProtoBuf.DemoShotVectorTrack)");
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
		if (instance.camRot == null)
		{
			throw new ArgumentNullException("camRot", "Required by proto specification.");
		}
		stream.WriteByte(50);
		BufferStream.RangeHandle range2 = stream.GetRange(3);
		int position2 = stream.Position;
		DemoShotQuaternionTrack.Serialize(stream, instance.camRot);
		int num3 = stream.Position - position2;
		if (num3 > 2097151)
		{
			throw new InvalidOperationException("Not enough space was reserved for the length prefix of field camRot (ProtoBuf.DemoShotQuaternionTrack)");
		}
		Span<byte> span2 = range2.GetSpan();
		int num4 = ProtocolParser.WriteUInt32((uint)num3, span2, 0);
		if (num4 < 3)
		{
			span2[num4 - 1] |= 128;
			while (num4 < 2)
			{
				span2[num4++] = 128;
			}
			span2[2] = 0;
		}
		if (instance.camFov == null)
		{
			throw new ArgumentNullException("camFov", "Required by proto specification.");
		}
		stream.WriteByte(58);
		BufferStream.RangeHandle range3 = stream.GetRange(3);
		int position3 = stream.Position;
		DemoShotFloatTrack.Serialize(stream, instance.camFov);
		int num5 = stream.Position - position3;
		if (num5 > 2097151)
		{
			throw new InvalidOperationException("Not enough space was reserved for the length prefix of field camFov (ProtoBuf.DemoShotFloatTrack)");
		}
		Span<byte> span3 = range3.GetSpan();
		int num6 = ProtocolParser.WriteUInt32((uint)num5, span3, 0);
		if (num6 < 3)
		{
			span3[num6 - 1] |= 128;
			while (num6 < 2)
			{
				span3[num6++] = 128;
			}
			span3[2] = 0;
		}
		if (instance.camDofDist != null)
		{
			stream.WriteByte(74);
			BufferStream.RangeHandle range4 = stream.GetRange(3);
			int position4 = stream.Position;
			DemoShotFloatTrack.Serialize(stream, instance.camDofDist);
			int num7 = stream.Position - position4;
			if (num7 > 2097151)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field camDofDist (ProtoBuf.DemoShotFloatTrack)");
			}
			Span<byte> span4 = range4.GetSpan();
			int num8 = ProtocolParser.WriteUInt32((uint)num7, span4, 0);
			if (num8 < 3)
			{
				span4[num8 - 1] |= 128;
				while (num8 < 2)
				{
					span4[num8++] = 128;
				}
				span4[2] = 0;
			}
		}
		if (instance.camDofFocalSize != null)
		{
			stream.WriteByte(82);
			BufferStream.RangeHandle range5 = stream.GetRange(3);
			int position5 = stream.Position;
			DemoShotFloatTrack.Serialize(stream, instance.camDofFocalSize);
			int num9 = stream.Position - position5;
			if (num9 > 2097151)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field camDofFocalSize (ProtoBuf.DemoShotFloatTrack)");
			}
			Span<byte> span5 = range5.GetSpan();
			int num10 = ProtocolParser.WriteUInt32((uint)num9, span5, 0);
			if (num10 < 3)
			{
				span5[num10 - 1] |= 128;
				while (num10 < 2)
				{
					span5[num10++] = 128;
				}
				span5[2] = 0;
			}
		}
		if (instance.camDofAperture != null)
		{
			stream.WriteByte(90);
			BufferStream.RangeHandle range6 = stream.GetRange(3);
			int position6 = stream.Position;
			DemoShotFloatTrack.Serialize(stream, instance.camDofAperture);
			int num11 = stream.Position - position6;
			if (num11 > 2097151)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field camDofAperture (ProtoBuf.DemoShotFloatTrack)");
			}
			Span<byte> span6 = range6.GetSpan();
			int num12 = ProtocolParser.WriteUInt32((uint)num11, span6, 0);
			if (num12 < 3)
			{
				span6[num12 - 1] |= 128;
				while (num12 < 2)
				{
					span6[num12++] = 128;
				}
				span6[2] = 0;
			}
		}
		if (instance.camParent != null)
		{
			stream.WriteByte(98);
			BufferStream.RangeHandle range7 = stream.GetRange(5);
			int position7 = stream.Position;
			DemoShotParentTrack.Serialize(stream, instance.camParent);
			int val = stream.Position - position7;
			Span<byte> span7 = range7.GetSpan();
			int num13 = ProtocolParser.WriteUInt32((uint)val, span7, 0);
			if (num13 < 5)
			{
				span7[num13 - 1] |= 128;
				while (num13 < 4)
				{
					span7[num13++] = 128;
				}
				span7[4] = 0;
			}
		}
		if (instance.folderName != null)
		{
			stream.WriteByte(66);
			ProtocolParser.WriteString(stream, instance.folderName);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		camPos?.InspectUids(action);
		camRot?.InspectUids(action);
		camFov?.InspectUids(action);
		camDofDist?.InspectUids(action);
		camDofFocalSize?.InspectUids(action);
		camDofAperture?.InspectUids(action);
		camParent?.InspectUids(action);
	}
}
