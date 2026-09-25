using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class AutoTurret : IDisposable, Pool.IPooled, IProto<AutoTurret>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public Vector3 aimPos;

	[NonSerialized]
	public Vector3 aimDir;

	[NonSerialized]
	public uint targetID;

	[NonSerialized]
	public List<PlayerNameID> users;

	[NonSerialized]
	public int powerOrder;

	[NonSerialized]
	public List<NetworkableId> interferenceList;

	public static void ResetToPool(AutoTurret instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.aimPos = default(Vector3);
		instance.aimDir = default(Vector3);
		instance.targetID = 0u;
		if (instance.users != null)
		{
			for (int i = 0; i < instance.users.Count; i++)
			{
				if (instance.users[i] != null)
				{
					instance.users[i].ResetToPool();
					instance.users[i] = null;
				}
			}
			List<PlayerNameID> obj = instance.users;
			Pool.Free(ref obj, freeElements: false);
			instance.users = obj;
		}
		instance.powerOrder = 0;
		if (instance.interferenceList != null)
		{
			List<NetworkableId> obj2 = instance.interferenceList;
			Pool.FreeUnmanaged(ref obj2);
			instance.interferenceList = obj2;
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
			throw new Exception("Trying to dispose AutoTurret with ShouldPool set to false!");
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

	public void CopyTo(AutoTurret instance)
	{
		instance.aimPos = aimPos;
		instance.aimDir = aimDir;
		instance.targetID = targetID;
		if (users != null)
		{
			instance.users = Pool.Get<List<PlayerNameID>>();
			for (int i = 0; i < users.Count; i++)
			{
				PlayerNameID item = users[i].Copy();
				instance.users.Add(item);
			}
		}
		else
		{
			instance.users = null;
		}
		instance.powerOrder = powerOrder;
		if (interferenceList != null)
		{
			instance.interferenceList = Pool.Get<List<NetworkableId>>();
			for (int j = 0; j < interferenceList.Count; j++)
			{
				NetworkableId item2 = interferenceList[j];
				instance.interferenceList.Add(item2);
			}
		}
		else
		{
			instance.interferenceList = null;
		}
	}

	public AutoTurret Copy()
	{
		AutoTurret autoTurret = Pool.Get<AutoTurret>();
		CopyTo(autoTurret);
		return autoTurret;
	}

	public static AutoTurret Deserialize(BufferStream stream)
	{
		AutoTurret autoTurret = Pool.Get<AutoTurret>();
		Deserialize(stream, autoTurret, isDelta: false);
		return autoTurret;
	}

	public static AutoTurret DeserializeLengthDelimited(BufferStream stream)
	{
		AutoTurret autoTurret = Pool.Get<AutoTurret>();
		DeserializeLengthDelimited(stream, autoTurret, isDelta: false);
		return autoTurret;
	}

	public static AutoTurret DeserializeLength(BufferStream stream, int length)
	{
		AutoTurret autoTurret = Pool.Get<AutoTurret>();
		DeserializeLength(stream, length, autoTurret, isDelta: false);
		return autoTurret;
	}

	public static AutoTurret Deserialize(byte[] buffer)
	{
		AutoTurret autoTurret = Pool.Get<AutoTurret>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, autoTurret, isDelta: false);
		return autoTurret;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, AutoTurret previous)
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

	public static AutoTurret Deserialize(BufferStream stream, AutoTurret instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.users == null)
			{
				instance.users = Pool.Get<List<PlayerNameID>>();
			}
			if (instance.interferenceList == null)
			{
				instance.interferenceList = Pool.Get<List<NetworkableId>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.AutoTurret");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AutoTurret");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.aimPos, isDelta);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AutoTurret");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.aimDir, isDelta);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AutoTurret");
				instance.targetID = ProtocolParser.ReadUInt32(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.AutoTurret");
				stream.ConsumeRepeatedElement();
				instance.users.Add(PlayerNameID.DeserializeLengthDelimited(stream));
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AutoTurret");
				instance.powerOrder = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: true, "ProtoBuf.AutoTurret");
				stream.ConsumeRepeatedElement();
				instance.interferenceList.Add(new NetworkableId(ProtocolParser.ReadUInt64(stream)));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AutoTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AutoTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AutoTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.AutoTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AutoTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: true, "ProtoBuf.AutoTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AutoTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AutoTurret DeserializeLengthDelimited(BufferStream stream, AutoTurret instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.users == null)
			{
				instance.users = Pool.Get<List<PlayerNameID>>();
			}
			if (instance.interferenceList == null)
			{
				instance.interferenceList = Pool.Get<List<NetworkableId>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.AutoTurret");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AutoTurret");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.aimPos, isDelta);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AutoTurret");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.aimDir, isDelta);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AutoTurret");
				instance.targetID = ProtocolParser.ReadUInt32(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.AutoTurret");
				stream.ConsumeRepeatedElement();
				instance.users.Add(PlayerNameID.DeserializeLengthDelimited(stream));
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AutoTurret");
				instance.powerOrder = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: true, "ProtoBuf.AutoTurret");
				stream.ConsumeRepeatedElement();
				instance.interferenceList.Add(new NetworkableId(ProtocolParser.ReadUInt64(stream)));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AutoTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AutoTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AutoTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.AutoTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AutoTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: true, "ProtoBuf.AutoTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AutoTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AutoTurret DeserializeLength(BufferStream stream, int length, AutoTurret instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.users == null)
			{
				instance.users = Pool.Get<List<PlayerNameID>>();
			}
			if (instance.interferenceList == null)
			{
				instance.interferenceList = Pool.Get<List<NetworkableId>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.AutoTurret");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AutoTurret");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.aimPos, isDelta);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AutoTurret");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.aimDir, isDelta);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AutoTurret");
				instance.targetID = ProtocolParser.ReadUInt32(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.AutoTurret");
				stream.ConsumeRepeatedElement();
				instance.users.Add(PlayerNameID.DeserializeLengthDelimited(stream));
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AutoTurret");
				instance.powerOrder = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: true, "ProtoBuf.AutoTurret");
				stream.ConsumeRepeatedElement();
				instance.interferenceList.Add(new NetworkableId(ProtocolParser.ReadUInt64(stream)));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AutoTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AutoTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AutoTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.AutoTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AutoTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: true, "ProtoBuf.AutoTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AutoTurret");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, AutoTurret instance, AutoTurret previous)
	{
		if (instance.aimPos != previous.aimPos)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.aimPos, previous.aimPos);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field aimPos (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.aimDir != previous.aimDir)
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.aimDir, previous.aimDir);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field aimDir (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.targetID != previous.targetID)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt32(stream, instance.targetID);
		}
		if (instance.users != null)
		{
			for (int i = 0; i < instance.users.Count; i++)
			{
				PlayerNameID playerNameID = instance.users[i];
				stream.WriteByte(34);
				BufferStream.RangeHandle range3 = stream.GetRange(5);
				int position3 = stream.Position;
				PlayerNameID.SerializeDelta(stream, playerNameID, playerNameID);
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
		if (instance.powerOrder != previous.powerOrder)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.powerOrder);
		}
		if (instance.interferenceList != null)
		{
			for (int j = 0; j < instance.interferenceList.Count; j++)
			{
				NetworkableId networkableId = instance.interferenceList[j];
				stream.WriteByte(48);
				ProtocolParser.WriteUInt64(stream, networkableId.Value);
			}
		}
	}

	public static void Serialize(BufferStream stream, AutoTurret instance)
	{
		if (instance.aimPos != default(Vector3))
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.Serialize(stream, instance.aimPos);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field aimPos (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.aimDir != default(Vector3))
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.aimDir);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field aimDir (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.targetID != 0)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt32(stream, instance.targetID);
		}
		if (instance.users != null)
		{
			for (int i = 0; i < instance.users.Count; i++)
			{
				PlayerNameID instance2 = instance.users[i];
				stream.WriteByte(34);
				BufferStream.RangeHandle range3 = stream.GetRange(5);
				int position3 = stream.Position;
				PlayerNameID.Serialize(stream, instance2);
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
		if (instance.powerOrder != 0)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.powerOrder);
		}
		if (instance.interferenceList != null)
		{
			for (int j = 0; j < instance.interferenceList.Count; j++)
			{
				NetworkableId networkableId = instance.interferenceList[j];
				stream.WriteByte(48);
				ProtocolParser.WriteUInt64(stream, networkableId.Value);
			}
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (users != null)
		{
			for (int i = 0; i < users.Count; i++)
			{
				users[i]?.InspectUids(action);
			}
		}
		if (interferenceList != null)
		{
			for (int j = 0; j < interferenceList.Count; j++)
			{
				NetworkableId value = interferenceList[j];
				action(UidType.NetworkableId, ref value.Value);
				interferenceList[j] = value;
			}
		}
	}
}
