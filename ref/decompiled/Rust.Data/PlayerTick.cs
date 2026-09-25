using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

public class PlayerTick : IDisposable, Pool.IPooled, IProto<PlayerTick>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public InputMessage inputState;

	[NonSerialized]
	public Vector3 position;

	[NonSerialized]
	public ModelState modelState;

	[NonSerialized]
	public ItemId activeItem;

	[NonSerialized]
	public Vector3 eyePos;

	[NonSerialized]
	public NetworkableId parentID;

	[NonSerialized]
	public uint deltaMs;

	public static void ResetToPool(PlayerTick instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.inputState != null)
			{
				instance.inputState.ResetToPool();
				instance.inputState = null;
			}
			instance.position = default(Vector3);
			if (instance.modelState != null)
			{
				instance.modelState.ResetToPool();
				instance.modelState = null;
			}
			instance.activeItem = default(ItemId);
			instance.eyePos = default(Vector3);
			instance.parentID = default(NetworkableId);
			instance.deltaMs = 0u;
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
			throw new Exception("Trying to dispose PlayerTick with ShouldPool set to false!");
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

	public void CopyTo(PlayerTick instance)
	{
		if (inputState != null)
		{
			if (instance.inputState == null)
			{
				instance.inputState = inputState.Copy();
			}
			else
			{
				inputState.CopyTo(instance.inputState);
			}
		}
		else
		{
			instance.inputState = null;
		}
		instance.position = position;
		if (modelState != null)
		{
			if (instance.modelState == null)
			{
				instance.modelState = modelState.Copy();
			}
			else
			{
				modelState.CopyTo(instance.modelState);
			}
		}
		else
		{
			instance.modelState = null;
		}
		instance.activeItem = activeItem;
		instance.eyePos = eyePos;
		instance.parentID = parentID;
		instance.deltaMs = deltaMs;
	}

	public PlayerTick Copy()
	{
		PlayerTick playerTick = Pool.Get<PlayerTick>();
		CopyTo(playerTick);
		return playerTick;
	}

	public static PlayerTick Deserialize(BufferStream stream)
	{
		PlayerTick playerTick = Pool.Get<PlayerTick>();
		Deserialize(stream, playerTick, isDelta: false);
		return playerTick;
	}

	public static PlayerTick DeserializeLengthDelimited(BufferStream stream)
	{
		PlayerTick playerTick = Pool.Get<PlayerTick>();
		DeserializeLengthDelimited(stream, playerTick, isDelta: false);
		return playerTick;
	}

	public static PlayerTick DeserializeLength(BufferStream stream, int length)
	{
		PlayerTick playerTick = Pool.Get<PlayerTick>();
		DeserializeLength(stream, length, playerTick, isDelta: false);
		return playerTick;
	}

	public static PlayerTick Deserialize(byte[] buffer)
	{
		PlayerTick playerTick = Pool.Get<PlayerTick>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, playerTick, isDelta: false);
		return playerTick;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, PlayerTick previous)
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

	public static PlayerTick Deserialize(BufferStream stream, PlayerTick instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("global::PlayerTick");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "global::PlayerTick");
				if (instance.inputState == null)
				{
					instance.inputState = InputMessage.DeserializeLengthDelimited(stream);
				}
				else
				{
					InputMessage.DeserializeLengthDelimited(stream, instance.inputState, isDelta);
				}
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "global::PlayerTick");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.position, isDelta);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "global::PlayerTick");
				if (instance.modelState == null)
				{
					instance.modelState = ModelState.DeserializeLengthDelimited(stream);
				}
				else
				{
					ModelState.DeserializeLengthDelimited(stream, instance.modelState, isDelta);
				}
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "global::PlayerTick");
				instance.activeItem = new ItemId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "global::PlayerTick");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.eyePos, isDelta);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "global::PlayerTick");
				instance.parentID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "global::PlayerTick");
				instance.deltaMs = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "global::PlayerTick");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "global::PlayerTick");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "global::PlayerTick");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "global::PlayerTick");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "global::PlayerTick");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "global::PlayerTick");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "global::PlayerTick");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "global::PlayerTick");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PlayerTick DeserializeLengthDelimited(BufferStream stream, PlayerTick instance, bool isDelta)
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
			stream.ConsumeFieldOperation("global::PlayerTick");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "global::PlayerTick");
				if (instance.inputState == null)
				{
					instance.inputState = InputMessage.DeserializeLengthDelimited(stream);
				}
				else
				{
					InputMessage.DeserializeLengthDelimited(stream, instance.inputState, isDelta);
				}
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "global::PlayerTick");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.position, isDelta);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "global::PlayerTick");
				if (instance.modelState == null)
				{
					instance.modelState = ModelState.DeserializeLengthDelimited(stream);
				}
				else
				{
					ModelState.DeserializeLengthDelimited(stream, instance.modelState, isDelta);
				}
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "global::PlayerTick");
				instance.activeItem = new ItemId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "global::PlayerTick");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.eyePos, isDelta);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "global::PlayerTick");
				instance.parentID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "global::PlayerTick");
				instance.deltaMs = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "global::PlayerTick");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "global::PlayerTick");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "global::PlayerTick");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "global::PlayerTick");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "global::PlayerTick");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "global::PlayerTick");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "global::PlayerTick");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "global::PlayerTick");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PlayerTick DeserializeLength(BufferStream stream, int length, PlayerTick instance, bool isDelta)
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
			stream.ConsumeFieldOperation("global::PlayerTick");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "global::PlayerTick");
				if (instance.inputState == null)
				{
					instance.inputState = InputMessage.DeserializeLengthDelimited(stream);
				}
				else
				{
					InputMessage.DeserializeLengthDelimited(stream, instance.inputState, isDelta);
				}
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "global::PlayerTick");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.position, isDelta);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "global::PlayerTick");
				if (instance.modelState == null)
				{
					instance.modelState = ModelState.DeserializeLengthDelimited(stream);
				}
				else
				{
					ModelState.DeserializeLengthDelimited(stream, instance.modelState, isDelta);
				}
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "global::PlayerTick");
				instance.activeItem = new ItemId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "global::PlayerTick");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.eyePos, isDelta);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "global::PlayerTick");
				instance.parentID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "global::PlayerTick");
				instance.deltaMs = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "global::PlayerTick");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "global::PlayerTick");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "global::PlayerTick");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "global::PlayerTick");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "global::PlayerTick");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "global::PlayerTick");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "global::PlayerTick");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "global::PlayerTick");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, PlayerTick instance, PlayerTick previous)
	{
		if (instance.inputState == null)
		{
			throw new ArgumentNullException("inputState", "Required by proto specification.");
		}
		stream.WriteByte(10);
		BufferStream.RangeHandle range = stream.GetRange(1);
		int num = stream.Position;
		InputMessage.SerializeDelta(stream, instance.inputState, previous.inputState);
		int num2 = stream.Position - num;
		if (num2 > 127)
		{
			throw new InvalidOperationException("Not enough space was reserved for the length prefix of field inputState (global::InputMessage)");
		}
		Span<byte> span = range.GetSpan();
		ProtocolParser.WriteUInt32((uint)num2, span, 0);
		if (instance.position != previous.position)
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int num3 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.position, previous.position);
			int num4 = stream.Position - num3;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field position (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span2, 0);
		}
		if (instance.modelState == null)
		{
			throw new ArgumentNullException("modelState", "Required by proto specification.");
		}
		stream.WriteByte(34);
		BufferStream.RangeHandle range3 = stream.GetRange(2);
		int num5 = stream.Position;
		ModelState.SerializeDelta(stream, instance.modelState, previous.modelState);
		int num6 = stream.Position - num5;
		if (num6 > 16383)
		{
			throw new InvalidOperationException("Not enough space was reserved for the length prefix of field modelState (global::ModelState)");
		}
		Span<byte> span3 = range3.GetSpan();
		if (ProtocolParser.WriteUInt32((uint)num6, span3, 0) < 2)
		{
			span3[0] |= 128;
			span3[1] = 0;
		}
		stream.WriteByte(40);
		ProtocolParser.WriteUInt64(stream, instance.activeItem.Value);
		if (instance.eyePos != previous.eyePos)
		{
			stream.WriteByte(50);
			BufferStream.RangeHandle range4 = stream.GetRange(1);
			int num7 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.eyePos, previous.eyePos);
			int num8 = stream.Position - num7;
			if (num8 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field eyePos (UnityEngine.Vector3)");
			}
			Span<byte> span4 = range4.GetSpan();
			ProtocolParser.WriteUInt32((uint)num8, span4, 0);
		}
		stream.WriteByte(56);
		ProtocolParser.WriteUInt64(stream, instance.parentID.Value);
		if (instance.deltaMs != previous.deltaMs)
		{
			stream.WriteByte(64);
			ProtocolParser.WriteUInt32(stream, instance.deltaMs);
		}
	}

	public static void Serialize(BufferStream stream, PlayerTick instance)
	{
		if (instance.inputState == null)
		{
			throw new ArgumentNullException("inputState", "Required by proto specification.");
		}
		stream.WriteByte(10);
		BufferStream.RangeHandle range = stream.GetRange(1);
		int num = stream.Position;
		InputMessage.Serialize(stream, instance.inputState);
		int num2 = stream.Position - num;
		if (num2 > 127)
		{
			throw new InvalidOperationException("Not enough space was reserved for the length prefix of field inputState (global::InputMessage)");
		}
		Span<byte> span = range.GetSpan();
		ProtocolParser.WriteUInt32((uint)num2, span, 0);
		if (instance.position != default(Vector3))
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int num3 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.position);
			int num4 = stream.Position - num3;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field position (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span2, 0);
		}
		if (instance.modelState == null)
		{
			throw new ArgumentNullException("modelState", "Required by proto specification.");
		}
		stream.WriteByte(34);
		BufferStream.RangeHandle range3 = stream.GetRange(2);
		int num5 = stream.Position;
		ModelState.Serialize(stream, instance.modelState);
		int num6 = stream.Position - num5;
		if (num6 > 16383)
		{
			throw new InvalidOperationException("Not enough space was reserved for the length prefix of field modelState (global::ModelState)");
		}
		Span<byte> span3 = range3.GetSpan();
		if (ProtocolParser.WriteUInt32((uint)num6, span3, 0) < 2)
		{
			span3[0] |= 128;
			span3[1] = 0;
		}
		if (instance.activeItem != default(ItemId))
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, instance.activeItem.Value);
		}
		if (instance.eyePos != default(Vector3))
		{
			stream.WriteByte(50);
			BufferStream.RangeHandle range4 = stream.GetRange(1);
			int num7 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.eyePos);
			int num8 = stream.Position - num7;
			if (num8 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field eyePos (UnityEngine.Vector3)");
			}
			Span<byte> span4 = range4.GetSpan();
			ProtocolParser.WriteUInt32((uint)num8, span4, 0);
		}
		if (instance.parentID != default(NetworkableId))
		{
			stream.WriteByte(56);
			ProtocolParser.WriteUInt64(stream, instance.parentID.Value);
		}
		if (instance.deltaMs != 0)
		{
			stream.WriteByte(64);
			ProtocolParser.WriteUInt32(stream, instance.deltaMs);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		inputState?.InspectUids(action);
		modelState?.InspectUids(action);
		action(UidType.ItemId, ref activeItem.Value);
		action(UidType.NetworkableId, ref parentID.Value);
	}
}
