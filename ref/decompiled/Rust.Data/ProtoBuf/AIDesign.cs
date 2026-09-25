using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class AIDesign : IDisposable, Pool.IPooled, IProto<AIDesign>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<int> availableStates;

	[NonSerialized]
	public List<AIStateContainer> stateContainers;

	[NonSerialized]
	public int defaultStateContainer;

	[NonSerialized]
	public string description;

	[NonSerialized]
	public int scope;

	[NonSerialized]
	public int intialViewStateID;

	public static void ResetToPool(AIDesign instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.availableStates != null)
		{
			List<int> obj = instance.availableStates;
			Pool.FreeUnmanaged(ref obj);
			instance.availableStates = obj;
		}
		if (instance.stateContainers != null)
		{
			for (int i = 0; i < instance.stateContainers.Count; i++)
			{
				if (instance.stateContainers[i] != null)
				{
					instance.stateContainers[i].ResetToPool();
					instance.stateContainers[i] = null;
				}
			}
			List<AIStateContainer> obj2 = instance.stateContainers;
			Pool.Free(ref obj2, freeElements: false);
			instance.stateContainers = obj2;
		}
		instance.defaultStateContainer = 0;
		instance.description = string.Empty;
		instance.scope = 0;
		instance.intialViewStateID = 0;
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
			throw new Exception("Trying to dispose AIDesign with ShouldPool set to false!");
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

	public void CopyTo(AIDesign instance)
	{
		if (availableStates != null)
		{
			instance.availableStates = Pool.Get<List<int>>();
			for (int i = 0; i < availableStates.Count; i++)
			{
				int item = availableStates[i];
				instance.availableStates.Add(item);
			}
		}
		else
		{
			instance.availableStates = null;
		}
		if (stateContainers != null)
		{
			instance.stateContainers = Pool.Get<List<AIStateContainer>>();
			for (int j = 0; j < stateContainers.Count; j++)
			{
				AIStateContainer item2 = stateContainers[j].Copy();
				instance.stateContainers.Add(item2);
			}
		}
		else
		{
			instance.stateContainers = null;
		}
		instance.defaultStateContainer = defaultStateContainer;
		instance.description = description;
		instance.scope = scope;
		instance.intialViewStateID = intialViewStateID;
	}

	public AIDesign Copy()
	{
		AIDesign aIDesign = Pool.Get<AIDesign>();
		CopyTo(aIDesign);
		return aIDesign;
	}

	public static AIDesign Deserialize(BufferStream stream)
	{
		AIDesign aIDesign = Pool.Get<AIDesign>();
		Deserialize(stream, aIDesign, isDelta: false);
		return aIDesign;
	}

	public static AIDesign DeserializeLengthDelimited(BufferStream stream)
	{
		AIDesign aIDesign = Pool.Get<AIDesign>();
		DeserializeLengthDelimited(stream, aIDesign, isDelta: false);
		return aIDesign;
	}

	public static AIDesign DeserializeLength(BufferStream stream, int length)
	{
		AIDesign aIDesign = Pool.Get<AIDesign>();
		DeserializeLength(stream, length, aIDesign, isDelta: false);
		return aIDesign;
	}

	public static AIDesign Deserialize(byte[] buffer)
	{
		AIDesign aIDesign = Pool.Get<AIDesign>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, aIDesign, isDelta: false);
		return aIDesign;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, AIDesign previous)
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

	public static AIDesign Deserialize(BufferStream stream, AIDesign instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.availableStates == null)
			{
				instance.availableStates = Pool.Get<List<int>>();
			}
			if (instance.stateContainers == null)
			{
				instance.stateContainers = Pool.Get<List<AIStateContainer>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.AIDesign");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.AIDesign");
				stream.ConsumeRepeatedElement();
				instance.availableStates.Add((int)ProtocolParser.ReadUInt64(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.AIDesign");
				stream.ConsumeRepeatedElement();
				instance.stateContainers.Add(AIStateContainer.DeserializeLengthDelimited(stream));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AIDesign");
				instance.defaultStateContainer = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AIDesign");
				instance.description = ProtocolParser.ReadString(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AIDesign");
				instance.scope = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AIDesign");
				instance.intialViewStateID = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.AIDesign");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.AIDesign");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AIDesign");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AIDesign");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AIDesign");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AIDesign");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AIDesign");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AIDesign DeserializeLengthDelimited(BufferStream stream, AIDesign instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.availableStates == null)
			{
				instance.availableStates = Pool.Get<List<int>>();
			}
			if (instance.stateContainers == null)
			{
				instance.stateContainers = Pool.Get<List<AIStateContainer>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.AIDesign");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.AIDesign");
				stream.ConsumeRepeatedElement();
				instance.availableStates.Add((int)ProtocolParser.ReadUInt64(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.AIDesign");
				stream.ConsumeRepeatedElement();
				instance.stateContainers.Add(AIStateContainer.DeserializeLengthDelimited(stream));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AIDesign");
				instance.defaultStateContainer = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AIDesign");
				instance.description = ProtocolParser.ReadString(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AIDesign");
				instance.scope = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AIDesign");
				instance.intialViewStateID = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.AIDesign");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.AIDesign");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AIDesign");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AIDesign");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AIDesign");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AIDesign");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AIDesign");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AIDesign DeserializeLength(BufferStream stream, int length, AIDesign instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.availableStates == null)
			{
				instance.availableStates = Pool.Get<List<int>>();
			}
			if (instance.stateContainers == null)
			{
				instance.stateContainers = Pool.Get<List<AIStateContainer>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.AIDesign");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.AIDesign");
				stream.ConsumeRepeatedElement();
				instance.availableStates.Add((int)ProtocolParser.ReadUInt64(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.AIDesign");
				stream.ConsumeRepeatedElement();
				instance.stateContainers.Add(AIStateContainer.DeserializeLengthDelimited(stream));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AIDesign");
				instance.defaultStateContainer = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AIDesign");
				instance.description = ProtocolParser.ReadString(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AIDesign");
				instance.scope = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AIDesign");
				instance.intialViewStateID = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.AIDesign");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.AIDesign");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AIDesign");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AIDesign");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AIDesign");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AIDesign");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AIDesign");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, AIDesign instance, AIDesign previous)
	{
		if (instance.availableStates != null)
		{
			for (int i = 0; i < instance.availableStates.Count; i++)
			{
				int num = instance.availableStates[i];
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)num);
			}
		}
		if (instance.stateContainers != null)
		{
			for (int j = 0; j < instance.stateContainers.Count; j++)
			{
				AIStateContainer aIStateContainer = instance.stateContainers[j];
				stream.WriteByte(18);
				BufferStream.RangeHandle range = stream.GetRange(3);
				int position = stream.Position;
				AIStateContainer.SerializeDelta(stream, aIStateContainer, aIStateContainer);
				int num2 = stream.Position - position;
				if (num2 > 2097151)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field stateContainers (ProtoBuf.AIStateContainer)");
				}
				Span<byte> span = range.GetSpan();
				int num3 = ProtocolParser.WriteUInt32((uint)num2, span, 0);
				if (num3 < 3)
				{
					span[num3 - 1] |= 128;
					while (num3 < 2)
					{
						span[num3++] = 128;
					}
					span[2] = 0;
				}
			}
		}
		if (instance.defaultStateContainer != previous.defaultStateContainer)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.defaultStateContainer);
		}
		if (instance.description != null && instance.description != previous.description)
		{
			stream.WriteByte(34);
			ProtocolParser.WriteString(stream, instance.description);
		}
		if (instance.scope != previous.scope)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.scope);
		}
		if (instance.intialViewStateID != previous.intialViewStateID)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.intialViewStateID);
		}
	}

	public static void Serialize(BufferStream stream, AIDesign instance)
	{
		if (instance.availableStates != null)
		{
			for (int i = 0; i < instance.availableStates.Count; i++)
			{
				int num = instance.availableStates[i];
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)num);
			}
		}
		if (instance.stateContainers != null)
		{
			for (int j = 0; j < instance.stateContainers.Count; j++)
			{
				AIStateContainer instance2 = instance.stateContainers[j];
				stream.WriteByte(18);
				BufferStream.RangeHandle range = stream.GetRange(3);
				int position = stream.Position;
				AIStateContainer.Serialize(stream, instance2);
				int num2 = stream.Position - position;
				if (num2 > 2097151)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field stateContainers (ProtoBuf.AIStateContainer)");
				}
				Span<byte> span = range.GetSpan();
				int num3 = ProtocolParser.WriteUInt32((uint)num2, span, 0);
				if (num3 < 3)
				{
					span[num3 - 1] |= 128;
					while (num3 < 2)
					{
						span[num3++] = 128;
					}
					span[2] = 0;
				}
			}
		}
		if (instance.defaultStateContainer != 0)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.defaultStateContainer);
		}
		if (instance.description != null)
		{
			stream.WriteByte(34);
			ProtocolParser.WriteString(stream, instance.description);
		}
		if (instance.scope != 0)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.scope);
		}
		if (instance.intialViewStateID != 0)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.intialViewStateID);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (stateContainers != null)
		{
			for (int i = 0; i < stateContainers.Count; i++)
			{
				stateContainers[i]?.InspectUids(action);
			}
		}
	}
}
