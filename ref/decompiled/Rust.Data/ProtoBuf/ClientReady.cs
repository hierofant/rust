using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ClientReady : IDisposable, Pool.IPooled, IProto<ClientReady>, IProto
{
	public class ClientInfo : IDisposable, Pool.IPooled, IProto<ClientInfo>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public string name;

		[NonSerialized]
		public string value;

		public static void ResetToPool(ClientInfo instance)
		{
			if (instance.ShouldPool)
			{
				instance.name = string.Empty;
				instance.value = string.Empty;
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
				throw new Exception("Trying to dispose ClientInfo with ShouldPool set to false!");
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

		public void CopyTo(ClientInfo instance)
		{
			instance.name = name;
			instance.value = value;
		}

		public ClientInfo Copy()
		{
			ClientInfo clientInfo = Pool.Get<ClientInfo>();
			CopyTo(clientInfo);
			return clientInfo;
		}

		public static ClientInfo Deserialize(BufferStream stream)
		{
			ClientInfo clientInfo = Pool.Get<ClientInfo>();
			Deserialize(stream, clientInfo, isDelta: false);
			return clientInfo;
		}

		public static ClientInfo DeserializeLengthDelimited(BufferStream stream)
		{
			ClientInfo clientInfo = Pool.Get<ClientInfo>();
			DeserializeLengthDelimited(stream, clientInfo, isDelta: false);
			return clientInfo;
		}

		public static ClientInfo DeserializeLength(BufferStream stream, int length)
		{
			ClientInfo clientInfo = Pool.Get<ClientInfo>();
			DeserializeLength(stream, length, clientInfo, isDelta: false);
			return clientInfo;
		}

		public static ClientInfo Deserialize(byte[] buffer)
		{
			ClientInfo clientInfo = Pool.Get<ClientInfo>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, clientInfo, isDelta: false);
			return clientInfo;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, ClientInfo previous)
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

		public static ClientInfo Deserialize(BufferStream stream, ClientInfo instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.ClientReady.ClientInfo");
				switch (num)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClientReady.ClientInfo");
					instance.name = ProtocolParser.ReadString(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClientReady.ClientInfo");
					instance.value = ProtocolParser.ReadString(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClientReady.ClientInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClientReady.ClientInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClientReady.ClientInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static ClientInfo DeserializeLengthDelimited(BufferStream stream, ClientInfo instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.ClientReady.ClientInfo");
				switch (num2)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClientReady.ClientInfo");
					instance.name = ProtocolParser.ReadString(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClientReady.ClientInfo");
					instance.value = ProtocolParser.ReadString(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClientReady.ClientInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClientReady.ClientInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClientReady.ClientInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static ClientInfo DeserializeLength(BufferStream stream, int length, ClientInfo instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.ClientReady.ClientInfo");
				switch (num2)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClientReady.ClientInfo");
					instance.name = ProtocolParser.ReadString(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClientReady.ClientInfo");
					instance.value = ProtocolParser.ReadString(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClientReady.ClientInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClientReady.ClientInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClientReady.ClientInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, ClientInfo instance, ClientInfo previous)
		{
			if (instance.name != previous.name)
			{
				if (instance.name == null)
				{
					throw new ArgumentNullException("name", "Required by proto specification.");
				}
				stream.WriteByte(10);
				ProtocolParser.WriteString(stream, instance.name);
			}
			if (instance.value != previous.value)
			{
				if (instance.value == null)
				{
					throw new ArgumentNullException("value", "Required by proto specification.");
				}
				stream.WriteByte(18);
				ProtocolParser.WriteString(stream, instance.value);
			}
		}

		public static void Serialize(BufferStream stream, ClientInfo instance)
		{
			if (instance.name == null)
			{
				throw new ArgumentNullException("name", "Required by proto specification.");
			}
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.name);
			if (instance.value == null)
			{
				throw new ArgumentNullException("value", "Required by proto specification.");
			}
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.value);
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
	public List<ClientInfo> clientInfo;

	[NonSerialized]
	public bool globalNetworking;

	[NonSerialized]
	public PartyData party;

	public static void ResetToPool(ClientReady instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.clientInfo != null)
		{
			for (int i = 0; i < instance.clientInfo.Count; i++)
			{
				if (instance.clientInfo[i] != null)
				{
					instance.clientInfo[i].ResetToPool();
					instance.clientInfo[i] = null;
				}
			}
			List<ClientInfo> obj = instance.clientInfo;
			Pool.Free(ref obj, freeElements: false);
			instance.clientInfo = obj;
		}
		instance.globalNetworking = false;
		if (instance.party != null)
		{
			instance.party.ResetToPool();
			instance.party = null;
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
			throw new Exception("Trying to dispose ClientReady with ShouldPool set to false!");
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

	public void CopyTo(ClientReady instance)
	{
		if (clientInfo != null)
		{
			instance.clientInfo = Pool.Get<List<ClientInfo>>();
			for (int i = 0; i < clientInfo.Count; i++)
			{
				ClientInfo item = clientInfo[i].Copy();
				instance.clientInfo.Add(item);
			}
		}
		else
		{
			instance.clientInfo = null;
		}
		instance.globalNetworking = globalNetworking;
		if (party != null)
		{
			if (instance.party == null)
			{
				instance.party = party.Copy();
			}
			else
			{
				party.CopyTo(instance.party);
			}
		}
		else
		{
			instance.party = null;
		}
	}

	public ClientReady Copy()
	{
		ClientReady clientReady = Pool.Get<ClientReady>();
		CopyTo(clientReady);
		return clientReady;
	}

	public static ClientReady Deserialize(BufferStream stream)
	{
		ClientReady clientReady = Pool.Get<ClientReady>();
		Deserialize(stream, clientReady, isDelta: false);
		return clientReady;
	}

	public static ClientReady DeserializeLengthDelimited(BufferStream stream)
	{
		ClientReady clientReady = Pool.Get<ClientReady>();
		DeserializeLengthDelimited(stream, clientReady, isDelta: false);
		return clientReady;
	}

	public static ClientReady DeserializeLength(BufferStream stream, int length)
	{
		ClientReady clientReady = Pool.Get<ClientReady>();
		DeserializeLength(stream, length, clientReady, isDelta: false);
		return clientReady;
	}

	public static ClientReady Deserialize(byte[] buffer)
	{
		ClientReady clientReady = Pool.Get<ClientReady>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, clientReady, isDelta: false);
		return clientReady;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ClientReady previous)
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

	public static ClientReady Deserialize(BufferStream stream, ClientReady instance, bool isDelta)
	{
		if (!isDelta && instance.clientInfo == null)
		{
			instance.clientInfo = Pool.Get<List<ClientInfo>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ClientReady");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ClientReady");
				stream.ConsumeRepeatedElement();
				instance.clientInfo.Add(ClientInfo.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClientReady");
				instance.globalNetworking = ProtocolParser.ReadBool(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClientReady");
				if (instance.party == null)
				{
					instance.party = PartyData.DeserializeLengthDelimited(stream);
				}
				else
				{
					PartyData.DeserializeLengthDelimited(stream, instance.party, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ClientReady");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClientReady");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClientReady");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClientReady");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ClientReady DeserializeLengthDelimited(BufferStream stream, ClientReady instance, bool isDelta)
	{
		if (!isDelta && instance.clientInfo == null)
		{
			instance.clientInfo = Pool.Get<List<ClientInfo>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ClientReady");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ClientReady");
				stream.ConsumeRepeatedElement();
				instance.clientInfo.Add(ClientInfo.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClientReady");
				instance.globalNetworking = ProtocolParser.ReadBool(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClientReady");
				if (instance.party == null)
				{
					instance.party = PartyData.DeserializeLengthDelimited(stream);
				}
				else
				{
					PartyData.DeserializeLengthDelimited(stream, instance.party, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ClientReady");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClientReady");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClientReady");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClientReady");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ClientReady DeserializeLength(BufferStream stream, int length, ClientReady instance, bool isDelta)
	{
		if (!isDelta && instance.clientInfo == null)
		{
			instance.clientInfo = Pool.Get<List<ClientInfo>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ClientReady");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ClientReady");
				stream.ConsumeRepeatedElement();
				instance.clientInfo.Add(ClientInfo.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClientReady");
				instance.globalNetworking = ProtocolParser.ReadBool(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClientReady");
				if (instance.party == null)
				{
					instance.party = PartyData.DeserializeLengthDelimited(stream);
				}
				else
				{
					PartyData.DeserializeLengthDelimited(stream, instance.party, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ClientReady");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClientReady");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClientReady");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClientReady");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ClientReady instance, ClientReady previous)
	{
		if (instance.clientInfo != null)
		{
			for (int i = 0; i < instance.clientInfo.Count; i++)
			{
				ClientInfo clientInfo = instance.clientInfo[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				ClientInfo.SerializeDelta(stream, clientInfo, clientInfo);
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
		ProtocolParser.WriteBool(stream, instance.globalNetworking);
		if (instance.party == null)
		{
			return;
		}
		stream.WriteByte(26);
		BufferStream.RangeHandle range2 = stream.GetRange(5);
		int position2 = stream.Position;
		PartyData.SerializeDelta(stream, instance.party, previous.party);
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

	public static void Serialize(BufferStream stream, ClientReady instance)
	{
		if (instance.clientInfo != null)
		{
			for (int i = 0; i < instance.clientInfo.Count; i++)
			{
				ClientInfo instance2 = instance.clientInfo[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				ClientInfo.Serialize(stream, instance2);
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
		if (instance.globalNetworking)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteBool(stream, instance.globalNetworking);
		}
		if (instance.party == null)
		{
			return;
		}
		stream.WriteByte(26);
		BufferStream.RangeHandle range2 = stream.GetRange(5);
		int position2 = stream.Position;
		PartyData.Serialize(stream, instance.party);
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

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (clientInfo != null)
		{
			for (int i = 0; i < clientInfo.Count; i++)
			{
				clientInfo[i]?.InspectUids(action);
			}
		}
		party?.InspectUids(action);
	}
}
