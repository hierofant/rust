using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ClanInvitations : IDisposable, Pool.IPooled, IProto<ClanInvitations>, IProto
{
	public class Invitation : IDisposable, Pool.IPooled, IProto<Invitation>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public long clanId;

		[NonSerialized]
		public ulong recruiter;

		[NonSerialized]
		public long timestamp;

		public static void ResetToPool(Invitation instance)
		{
			if (instance.ShouldPool)
			{
				instance.clanId = 0L;
				instance.recruiter = 0uL;
				instance.timestamp = 0L;
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
				throw new Exception("Trying to dispose Invitation with ShouldPool set to false!");
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

		public void CopyTo(Invitation instance)
		{
			instance.clanId = clanId;
			instance.recruiter = recruiter;
			instance.timestamp = timestamp;
		}

		public Invitation Copy()
		{
			Invitation invitation = Pool.Get<Invitation>();
			CopyTo(invitation);
			return invitation;
		}

		public static Invitation Deserialize(BufferStream stream)
		{
			Invitation invitation = Pool.Get<Invitation>();
			Deserialize(stream, invitation, isDelta: false);
			return invitation;
		}

		public static Invitation DeserializeLengthDelimited(BufferStream stream)
		{
			Invitation invitation = Pool.Get<Invitation>();
			DeserializeLengthDelimited(stream, invitation, isDelta: false);
			return invitation;
		}

		public static Invitation DeserializeLength(BufferStream stream, int length)
		{
			Invitation invitation = Pool.Get<Invitation>();
			DeserializeLength(stream, length, invitation, isDelta: false);
			return invitation;
		}

		public static Invitation Deserialize(byte[] buffer)
		{
			Invitation invitation = Pool.Get<Invitation>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, invitation, isDelta: false);
			return invitation;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, Invitation previous)
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

		public static Invitation Deserialize(BufferStream stream, Invitation instance, bool isDelta)
		{
			if (!isDelta)
			{
				instance.clanId = 0L;
				instance.recruiter = 0uL;
				instance.timestamp = 0L;
			}
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.ClanInvitations.Invitation");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanInvitations.Invitation");
					instance.clanId = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanInvitations.Invitation");
					instance.recruiter = ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanInvitations.Invitation");
					instance.timestamp = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanInvitations.Invitation");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanInvitations.Invitation");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanInvitations.Invitation");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanInvitations.Invitation");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Invitation DeserializeLengthDelimited(BufferStream stream, Invitation instance, bool isDelta)
		{
			if (!isDelta)
			{
				instance.clanId = 0L;
				instance.recruiter = 0uL;
				instance.timestamp = 0L;
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
				stream.ConsumeFieldOperation("ProtoBuf.ClanInvitations.Invitation");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanInvitations.Invitation");
					instance.clanId = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanInvitations.Invitation");
					instance.recruiter = ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanInvitations.Invitation");
					instance.timestamp = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanInvitations.Invitation");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanInvitations.Invitation");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanInvitations.Invitation");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanInvitations.Invitation");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Invitation DeserializeLength(BufferStream stream, int length, Invitation instance, bool isDelta)
		{
			if (!isDelta)
			{
				instance.clanId = 0L;
				instance.recruiter = 0uL;
				instance.timestamp = 0L;
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
				stream.ConsumeFieldOperation("ProtoBuf.ClanInvitations.Invitation");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanInvitations.Invitation");
					instance.clanId = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanInvitations.Invitation");
					instance.recruiter = ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanInvitations.Invitation");
					instance.timestamp = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanInvitations.Invitation");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanInvitations.Invitation");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanInvitations.Invitation");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanInvitations.Invitation");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, Invitation instance, Invitation previous)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.clanId);
			if (instance.recruiter != previous.recruiter)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, instance.recruiter);
			}
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.timestamp);
		}

		public static void Serialize(BufferStream stream, Invitation instance)
		{
			if (instance.clanId != 0L)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.clanId);
			}
			if (instance.recruiter != 0L)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, instance.recruiter);
			}
			if (instance.timestamp != 0L)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.timestamp);
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
	public List<Invitation> invitations;

	public static void ResetToPool(ClanInvitations instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.invitations != null)
		{
			for (int i = 0; i < instance.invitations.Count; i++)
			{
				if (instance.invitations[i] != null)
				{
					instance.invitations[i].ResetToPool();
					instance.invitations[i] = null;
				}
			}
			List<Invitation> obj = instance.invitations;
			Pool.Free(ref obj, freeElements: false);
			instance.invitations = obj;
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
			throw new Exception("Trying to dispose ClanInvitations with ShouldPool set to false!");
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

	public void CopyTo(ClanInvitations instance)
	{
		if (invitations != null)
		{
			instance.invitations = Pool.Get<List<Invitation>>();
			for (int i = 0; i < invitations.Count; i++)
			{
				Invitation item = invitations[i].Copy();
				instance.invitations.Add(item);
			}
		}
		else
		{
			instance.invitations = null;
		}
	}

	public ClanInvitations Copy()
	{
		ClanInvitations clanInvitations = Pool.Get<ClanInvitations>();
		CopyTo(clanInvitations);
		return clanInvitations;
	}

	public static ClanInvitations Deserialize(BufferStream stream)
	{
		ClanInvitations clanInvitations = Pool.Get<ClanInvitations>();
		Deserialize(stream, clanInvitations, isDelta: false);
		return clanInvitations;
	}

	public static ClanInvitations DeserializeLengthDelimited(BufferStream stream)
	{
		ClanInvitations clanInvitations = Pool.Get<ClanInvitations>();
		DeserializeLengthDelimited(stream, clanInvitations, isDelta: false);
		return clanInvitations;
	}

	public static ClanInvitations DeserializeLength(BufferStream stream, int length)
	{
		ClanInvitations clanInvitations = Pool.Get<ClanInvitations>();
		DeserializeLength(stream, length, clanInvitations, isDelta: false);
		return clanInvitations;
	}

	public static ClanInvitations Deserialize(byte[] buffer)
	{
		ClanInvitations clanInvitations = Pool.Get<ClanInvitations>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, clanInvitations, isDelta: false);
		return clanInvitations;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ClanInvitations previous)
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

	public static ClanInvitations Deserialize(BufferStream stream, ClanInvitations instance, bool isDelta)
	{
		if (!isDelta && instance.invitations == null)
		{
			instance.invitations = Pool.Get<List<Invitation>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ClanInvitations");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ClanInvitations");
				stream.ConsumeRepeatedElement();
				instance.invitations.Add(Invitation.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ClanInvitations");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanInvitations");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ClanInvitations DeserializeLengthDelimited(BufferStream stream, ClanInvitations instance, bool isDelta)
	{
		if (!isDelta && instance.invitations == null)
		{
			instance.invitations = Pool.Get<List<Invitation>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ClanInvitations");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ClanInvitations");
				stream.ConsumeRepeatedElement();
				instance.invitations.Add(Invitation.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ClanInvitations");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanInvitations");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ClanInvitations DeserializeLength(BufferStream stream, int length, ClanInvitations instance, bool isDelta)
	{
		if (!isDelta && instance.invitations == null)
		{
			instance.invitations = Pool.Get<List<Invitation>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ClanInvitations");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ClanInvitations");
				stream.ConsumeRepeatedElement();
				instance.invitations.Add(Invitation.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ClanInvitations");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanInvitations");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ClanInvitations instance, ClanInvitations previous)
	{
		if (instance.invitations == null)
		{
			return;
		}
		for (int i = 0; i < instance.invitations.Count; i++)
		{
			Invitation invitation = instance.invitations[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Invitation.SerializeDelta(stream, invitation, invitation);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field invitations (ProtoBuf.ClanInvitations.Invitation)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public static void Serialize(BufferStream stream, ClanInvitations instance)
	{
		if (instance.invitations == null)
		{
			return;
		}
		for (int i = 0; i < instance.invitations.Count; i++)
		{
			Invitation instance2 = instance.invitations[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Invitation.Serialize(stream, instance2);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field invitations (ProtoBuf.ClanInvitations.Invitation)");
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
		if (invitations != null)
		{
			for (int i = 0; i < invitations.Count; i++)
			{
				invitations[i]?.InspectUids(action);
			}
		}
	}
}
