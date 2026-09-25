using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class Telephone : IDisposable, Pool.IPooled, IProto<Telephone>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public int lastNumber;

	[NonSerialized]
	public int phoneNumber;

	[NonSerialized]
	public NetworkableId usingPlayer;

	[NonSerialized]
	public string phoneName;

	[NonSerialized]
	public PhoneDirectory savedNumbers;

	[NonSerialized]
	public List<VoicemailEntry> voicemail;

	public static void ResetToPool(Telephone instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.lastNumber = 0;
		instance.phoneNumber = 0;
		instance.usingPlayer = default(NetworkableId);
		instance.phoneName = string.Empty;
		if (instance.savedNumbers != null)
		{
			instance.savedNumbers.ResetToPool();
			instance.savedNumbers = null;
		}
		if (instance.voicemail != null)
		{
			for (int i = 0; i < instance.voicemail.Count; i++)
			{
				if (instance.voicemail[i] != null)
				{
					instance.voicemail[i].ResetToPool();
					instance.voicemail[i] = null;
				}
			}
			List<VoicemailEntry> obj = instance.voicemail;
			Pool.Free(ref obj, freeElements: false);
			instance.voicemail = obj;
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
			throw new Exception("Trying to dispose Telephone with ShouldPool set to false!");
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

	public void CopyTo(Telephone instance)
	{
		instance.lastNumber = lastNumber;
		instance.phoneNumber = phoneNumber;
		instance.usingPlayer = usingPlayer;
		instance.phoneName = phoneName;
		if (savedNumbers != null)
		{
			if (instance.savedNumbers == null)
			{
				instance.savedNumbers = savedNumbers.Copy();
			}
			else
			{
				savedNumbers.CopyTo(instance.savedNumbers);
			}
		}
		else
		{
			instance.savedNumbers = null;
		}
		if (voicemail != null)
		{
			instance.voicemail = Pool.Get<List<VoicemailEntry>>();
			for (int i = 0; i < voicemail.Count; i++)
			{
				VoicemailEntry item = voicemail[i].Copy();
				instance.voicemail.Add(item);
			}
		}
		else
		{
			instance.voicemail = null;
		}
	}

	public Telephone Copy()
	{
		Telephone telephone = Pool.Get<Telephone>();
		CopyTo(telephone);
		return telephone;
	}

	public static Telephone Deserialize(BufferStream stream)
	{
		Telephone telephone = Pool.Get<Telephone>();
		Deserialize(stream, telephone, isDelta: false);
		return telephone;
	}

	public static Telephone DeserializeLengthDelimited(BufferStream stream)
	{
		Telephone telephone = Pool.Get<Telephone>();
		DeserializeLengthDelimited(stream, telephone, isDelta: false);
		return telephone;
	}

	public static Telephone DeserializeLength(BufferStream stream, int length)
	{
		Telephone telephone = Pool.Get<Telephone>();
		DeserializeLength(stream, length, telephone, isDelta: false);
		return telephone;
	}

	public static Telephone Deserialize(byte[] buffer)
	{
		Telephone telephone = Pool.Get<Telephone>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, telephone, isDelta: false);
		return telephone;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, Telephone previous)
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

	public static Telephone Deserialize(BufferStream stream, Telephone instance, bool isDelta)
	{
		if (!isDelta && instance.voicemail == null)
		{
			instance.voicemail = Pool.Get<List<VoicemailEntry>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Telephone");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Telephone");
				instance.lastNumber = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Telephone");
				instance.phoneNumber = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Telephone");
				instance.usingPlayer = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Telephone");
				instance.phoneName = ProtocolParser.ReadString(stream);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Telephone");
				if (instance.savedNumbers == null)
				{
					instance.savedNumbers = PhoneDirectory.DeserializeLengthDelimited(stream);
				}
				else
				{
					PhoneDirectory.DeserializeLengthDelimited(stream, instance.savedNumbers, isDelta);
				}
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.Telephone");
				stream.ConsumeRepeatedElement();
				instance.voicemail.Add(VoicemailEntry.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Telephone");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Telephone");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Telephone");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Telephone");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Telephone");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.Telephone");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Telephone");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Telephone DeserializeLengthDelimited(BufferStream stream, Telephone instance, bool isDelta)
	{
		if (!isDelta && instance.voicemail == null)
		{
			instance.voicemail = Pool.Get<List<VoicemailEntry>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Telephone");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Telephone");
				instance.lastNumber = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Telephone");
				instance.phoneNumber = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Telephone");
				instance.usingPlayer = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Telephone");
				instance.phoneName = ProtocolParser.ReadString(stream);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Telephone");
				if (instance.savedNumbers == null)
				{
					instance.savedNumbers = PhoneDirectory.DeserializeLengthDelimited(stream);
				}
				else
				{
					PhoneDirectory.DeserializeLengthDelimited(stream, instance.savedNumbers, isDelta);
				}
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.Telephone");
				stream.ConsumeRepeatedElement();
				instance.voicemail.Add(VoicemailEntry.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Telephone");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Telephone");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Telephone");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Telephone");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Telephone");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.Telephone");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Telephone");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Telephone DeserializeLength(BufferStream stream, int length, Telephone instance, bool isDelta)
	{
		if (!isDelta && instance.voicemail == null)
		{
			instance.voicemail = Pool.Get<List<VoicemailEntry>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Telephone");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Telephone");
				instance.lastNumber = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Telephone");
				instance.phoneNumber = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Telephone");
				instance.usingPlayer = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Telephone");
				instance.phoneName = ProtocolParser.ReadString(stream);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Telephone");
				if (instance.savedNumbers == null)
				{
					instance.savedNumbers = PhoneDirectory.DeserializeLengthDelimited(stream);
				}
				else
				{
					PhoneDirectory.DeserializeLengthDelimited(stream, instance.savedNumbers, isDelta);
				}
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.Telephone");
				stream.ConsumeRepeatedElement();
				instance.voicemail.Add(VoicemailEntry.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Telephone");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Telephone");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Telephone");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Telephone");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Telephone");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.Telephone");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Telephone");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, Telephone instance, Telephone previous)
	{
		if (instance.lastNumber != previous.lastNumber)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.lastNumber);
		}
		if (instance.phoneNumber != previous.phoneNumber)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.phoneNumber);
		}
		stream.WriteByte(32);
		ProtocolParser.WriteUInt64(stream, instance.usingPlayer.Value);
		if (instance.phoneName != null && instance.phoneName != previous.phoneName)
		{
			stream.WriteByte(42);
			ProtocolParser.WriteString(stream, instance.phoneName);
		}
		if (instance.savedNumbers != null)
		{
			stream.WriteByte(50);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			PhoneDirectory.SerializeDelta(stream, instance.savedNumbers, previous.savedNumbers);
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
		if (instance.voicemail == null)
		{
			return;
		}
		for (int i = 0; i < instance.voicemail.Count; i++)
		{
			VoicemailEntry voicemailEntry = instance.voicemail[i];
			stream.WriteByte(58);
			BufferStream.RangeHandle range2 = stream.GetRange(5);
			int position2 = stream.Position;
			VoicemailEntry.SerializeDelta(stream, voicemailEntry, voicemailEntry);
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

	public static void Serialize(BufferStream stream, Telephone instance)
	{
		if (instance.lastNumber != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.lastNumber);
		}
		if (instance.phoneNumber != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.phoneNumber);
		}
		if (instance.usingPlayer != default(NetworkableId))
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, instance.usingPlayer.Value);
		}
		if (instance.phoneName != null)
		{
			stream.WriteByte(42);
			ProtocolParser.WriteString(stream, instance.phoneName);
		}
		if (instance.savedNumbers != null)
		{
			stream.WriteByte(50);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			PhoneDirectory.Serialize(stream, instance.savedNumbers);
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
		if (instance.voicemail == null)
		{
			return;
		}
		for (int i = 0; i < instance.voicemail.Count; i++)
		{
			VoicemailEntry instance2 = instance.voicemail[i];
			stream.WriteByte(58);
			BufferStream.RangeHandle range2 = stream.GetRange(5);
			int position2 = stream.Position;
			VoicemailEntry.Serialize(stream, instance2);
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

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref usingPlayer.Value);
		savedNumbers?.InspectUids(action);
		if (voicemail != null)
		{
			for (int i = 0; i < voicemail.Count; i++)
			{
				voicemail[i]?.InspectUids(action);
			}
		}
	}
}
