using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class AppBroadcast : IDisposable, Pool.IPooled, IProto<AppBroadcast>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public AppTeamChanged teamChanged;

	[NonSerialized]
	public AppNewTeamMessage teamMessage;

	[NonSerialized]
	public AppEntityChanged entityChanged;

	[NonSerialized]
	public AppClanChanged clanChanged;

	[NonSerialized]
	public AppNewClanMessage clanMessage;

	[NonSerialized]
	public AppCameraRays cameraRays;

	public static void ResetToPool(AppBroadcast instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.teamChanged != null)
			{
				instance.teamChanged.ResetToPool();
				instance.teamChanged = null;
			}
			if (instance.teamMessage != null)
			{
				instance.teamMessage.ResetToPool();
				instance.teamMessage = null;
			}
			if (instance.entityChanged != null)
			{
				instance.entityChanged.ResetToPool();
				instance.entityChanged = null;
			}
			if (instance.clanChanged != null)
			{
				instance.clanChanged.ResetToPool();
				instance.clanChanged = null;
			}
			if (instance.clanMessage != null)
			{
				instance.clanMessage.ResetToPool();
				instance.clanMessage = null;
			}
			if (instance.cameraRays != null)
			{
				instance.cameraRays.ResetToPool();
				instance.cameraRays = null;
			}
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
			throw new Exception("Trying to dispose AppBroadcast with ShouldPool set to false!");
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

	public void CopyTo(AppBroadcast instance)
	{
		if (teamChanged != null)
		{
			if (instance.teamChanged == null)
			{
				instance.teamChanged = teamChanged.Copy();
			}
			else
			{
				teamChanged.CopyTo(instance.teamChanged);
			}
		}
		else
		{
			instance.teamChanged = null;
		}
		if (teamMessage != null)
		{
			if (instance.teamMessage == null)
			{
				instance.teamMessage = teamMessage.Copy();
			}
			else
			{
				teamMessage.CopyTo(instance.teamMessage);
			}
		}
		else
		{
			instance.teamMessage = null;
		}
		if (entityChanged != null)
		{
			if (instance.entityChanged == null)
			{
				instance.entityChanged = entityChanged.Copy();
			}
			else
			{
				entityChanged.CopyTo(instance.entityChanged);
			}
		}
		else
		{
			instance.entityChanged = null;
		}
		if (clanChanged != null)
		{
			if (instance.clanChanged == null)
			{
				instance.clanChanged = clanChanged.Copy();
			}
			else
			{
				clanChanged.CopyTo(instance.clanChanged);
			}
		}
		else
		{
			instance.clanChanged = null;
		}
		if (clanMessage != null)
		{
			if (instance.clanMessage == null)
			{
				instance.clanMessage = clanMessage.Copy();
			}
			else
			{
				clanMessage.CopyTo(instance.clanMessage);
			}
		}
		else
		{
			instance.clanMessage = null;
		}
		if (cameraRays != null)
		{
			if (instance.cameraRays == null)
			{
				instance.cameraRays = cameraRays.Copy();
			}
			else
			{
				cameraRays.CopyTo(instance.cameraRays);
			}
		}
		else
		{
			instance.cameraRays = null;
		}
	}

	public AppBroadcast Copy()
	{
		AppBroadcast appBroadcast = Pool.Get<AppBroadcast>();
		CopyTo(appBroadcast);
		return appBroadcast;
	}

	public static AppBroadcast Deserialize(BufferStream stream)
	{
		AppBroadcast appBroadcast = Pool.Get<AppBroadcast>();
		Deserialize(stream, appBroadcast, isDelta: false);
		return appBroadcast;
	}

	public static AppBroadcast DeserializeLengthDelimited(BufferStream stream)
	{
		AppBroadcast appBroadcast = Pool.Get<AppBroadcast>();
		DeserializeLengthDelimited(stream, appBroadcast, isDelta: false);
		return appBroadcast;
	}

	public static AppBroadcast DeserializeLength(BufferStream stream, int length)
	{
		AppBroadcast appBroadcast = Pool.Get<AppBroadcast>();
		DeserializeLength(stream, length, appBroadcast, isDelta: false);
		return appBroadcast;
	}

	public static AppBroadcast Deserialize(byte[] buffer)
	{
		AppBroadcast appBroadcast = Pool.Get<AppBroadcast>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, appBroadcast, isDelta: false);
		return appBroadcast;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, AppBroadcast previous)
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

	public static AppBroadcast Deserialize(BufferStream stream, AppBroadcast instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.AppBroadcast");
			switch (num)
			{
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				if (instance.teamChanged == null)
				{
					instance.teamChanged = AppTeamChanged.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppTeamChanged.DeserializeLengthDelimited(stream, instance.teamChanged, isDelta);
				}
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				if (instance.teamMessage == null)
				{
					instance.teamMessage = AppNewTeamMessage.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppNewTeamMessage.DeserializeLengthDelimited(stream, instance.teamMessage, isDelta);
				}
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				if (instance.entityChanged == null)
				{
					instance.entityChanged = AppEntityChanged.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppEntityChanged.DeserializeLengthDelimited(stream, instance.entityChanged, isDelta);
				}
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				if (instance.clanChanged == null)
				{
					instance.clanChanged = AppClanChanged.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppClanChanged.DeserializeLengthDelimited(stream, instance.clanChanged, isDelta);
				}
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				if (instance.clanMessage == null)
				{
					instance.clanMessage = AppNewClanMessage.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppNewClanMessage.DeserializeLengthDelimited(stream, instance.clanMessage, isDelta);
				}
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				if (instance.cameraRays == null)
				{
					instance.cameraRays = AppCameraRays.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppCameraRays.DeserializeLengthDelimited(stream, instance.cameraRays, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppBroadcast");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AppBroadcast DeserializeLengthDelimited(BufferStream stream, AppBroadcast instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.AppBroadcast");
			switch (num2)
			{
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				if (instance.teamChanged == null)
				{
					instance.teamChanged = AppTeamChanged.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppTeamChanged.DeserializeLengthDelimited(stream, instance.teamChanged, isDelta);
				}
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				if (instance.teamMessage == null)
				{
					instance.teamMessage = AppNewTeamMessage.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppNewTeamMessage.DeserializeLengthDelimited(stream, instance.teamMessage, isDelta);
				}
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				if (instance.entityChanged == null)
				{
					instance.entityChanged = AppEntityChanged.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppEntityChanged.DeserializeLengthDelimited(stream, instance.entityChanged, isDelta);
				}
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				if (instance.clanChanged == null)
				{
					instance.clanChanged = AppClanChanged.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppClanChanged.DeserializeLengthDelimited(stream, instance.clanChanged, isDelta);
				}
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				if (instance.clanMessage == null)
				{
					instance.clanMessage = AppNewClanMessage.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppNewClanMessage.DeserializeLengthDelimited(stream, instance.clanMessage, isDelta);
				}
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				if (instance.cameraRays == null)
				{
					instance.cameraRays = AppCameraRays.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppCameraRays.DeserializeLengthDelimited(stream, instance.cameraRays, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppBroadcast");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AppBroadcast DeserializeLength(BufferStream stream, int length, AppBroadcast instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.AppBroadcast");
			switch (num2)
			{
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				if (instance.teamChanged == null)
				{
					instance.teamChanged = AppTeamChanged.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppTeamChanged.DeserializeLengthDelimited(stream, instance.teamChanged, isDelta);
				}
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				if (instance.teamMessage == null)
				{
					instance.teamMessage = AppNewTeamMessage.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppNewTeamMessage.DeserializeLengthDelimited(stream, instance.teamMessage, isDelta);
				}
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				if (instance.entityChanged == null)
				{
					instance.entityChanged = AppEntityChanged.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppEntityChanged.DeserializeLengthDelimited(stream, instance.entityChanged, isDelta);
				}
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				if (instance.clanChanged == null)
				{
					instance.clanChanged = AppClanChanged.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppClanChanged.DeserializeLengthDelimited(stream, instance.clanChanged, isDelta);
				}
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				if (instance.clanMessage == null)
				{
					instance.clanMessage = AppNewClanMessage.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppNewClanMessage.DeserializeLengthDelimited(stream, instance.clanMessage, isDelta);
				}
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				if (instance.cameraRays == null)
				{
					instance.cameraRays = AppCameraRays.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppCameraRays.DeserializeLengthDelimited(stream, instance.cameraRays, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppBroadcast");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppBroadcast");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, AppBroadcast instance, AppBroadcast previous)
	{
		if (instance.teamChanged != null)
		{
			stream.WriteByte(34);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			AppTeamChanged.SerializeDelta(stream, instance.teamChanged, previous.teamChanged);
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
		if (instance.teamMessage != null)
		{
			stream.WriteByte(42);
			BufferStream.RangeHandle range2 = stream.GetRange(5);
			int position2 = stream.Position;
			AppNewTeamMessage.SerializeDelta(stream, instance.teamMessage, previous.teamMessage);
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
		if (instance.entityChanged != null)
		{
			stream.WriteByte(50);
			BufferStream.RangeHandle range3 = stream.GetRange(3);
			int position3 = stream.Position;
			AppEntityChanged.SerializeDelta(stream, instance.entityChanged, previous.entityChanged);
			int num3 = stream.Position - position3;
			if (num3 > 2097151)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field entityChanged (ProtoBuf.AppEntityChanged)");
			}
			Span<byte> span3 = range3.GetSpan();
			int num4 = ProtocolParser.WriteUInt32((uint)num3, span3, 0);
			if (num4 < 3)
			{
				span3[num4 - 1] |= 128;
				while (num4 < 2)
				{
					span3[num4++] = 128;
				}
				span3[2] = 0;
			}
		}
		if (instance.clanChanged != null)
		{
			stream.WriteByte(58);
			BufferStream.RangeHandle range4 = stream.GetRange(5);
			int position4 = stream.Position;
			AppClanChanged.SerializeDelta(stream, instance.clanChanged, previous.clanChanged);
			int val3 = stream.Position - position4;
			Span<byte> span4 = range4.GetSpan();
			int num5 = ProtocolParser.WriteUInt32((uint)val3, span4, 0);
			if (num5 < 5)
			{
				span4[num5 - 1] |= 128;
				while (num5 < 4)
				{
					span4[num5++] = 128;
				}
				span4[4] = 0;
			}
		}
		if (instance.clanMessage != null)
		{
			stream.WriteByte(66);
			BufferStream.RangeHandle range5 = stream.GetRange(5);
			int position5 = stream.Position;
			AppNewClanMessage.SerializeDelta(stream, instance.clanMessage, previous.clanMessage);
			int val4 = stream.Position - position5;
			Span<byte> span5 = range5.GetSpan();
			int num6 = ProtocolParser.WriteUInt32((uint)val4, span5, 0);
			if (num6 < 5)
			{
				span5[num6 - 1] |= 128;
				while (num6 < 4)
				{
					span5[num6++] = 128;
				}
				span5[4] = 0;
			}
		}
		if (instance.cameraRays == null)
		{
			return;
		}
		stream.WriteByte(82);
		BufferStream.RangeHandle range6 = stream.GetRange(5);
		int position6 = stream.Position;
		AppCameraRays.SerializeDelta(stream, instance.cameraRays, previous.cameraRays);
		int val5 = stream.Position - position6;
		Span<byte> span6 = range6.GetSpan();
		int num7 = ProtocolParser.WriteUInt32((uint)val5, span6, 0);
		if (num7 < 5)
		{
			span6[num7 - 1] |= 128;
			while (num7 < 4)
			{
				span6[num7++] = 128;
			}
			span6[4] = 0;
		}
	}

	public static void Serialize(BufferStream stream, AppBroadcast instance)
	{
		if (instance.teamChanged != null)
		{
			stream.WriteByte(34);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			AppTeamChanged.Serialize(stream, instance.teamChanged);
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
		if (instance.teamMessage != null)
		{
			stream.WriteByte(42);
			BufferStream.RangeHandle range2 = stream.GetRange(5);
			int position2 = stream.Position;
			AppNewTeamMessage.Serialize(stream, instance.teamMessage);
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
		if (instance.entityChanged != null)
		{
			stream.WriteByte(50);
			BufferStream.RangeHandle range3 = stream.GetRange(3);
			int position3 = stream.Position;
			AppEntityChanged.Serialize(stream, instance.entityChanged);
			int num3 = stream.Position - position3;
			if (num3 > 2097151)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field entityChanged (ProtoBuf.AppEntityChanged)");
			}
			Span<byte> span3 = range3.GetSpan();
			int num4 = ProtocolParser.WriteUInt32((uint)num3, span3, 0);
			if (num4 < 3)
			{
				span3[num4 - 1] |= 128;
				while (num4 < 2)
				{
					span3[num4++] = 128;
				}
				span3[2] = 0;
			}
		}
		if (instance.clanChanged != null)
		{
			stream.WriteByte(58);
			BufferStream.RangeHandle range4 = stream.GetRange(5);
			int position4 = stream.Position;
			AppClanChanged.Serialize(stream, instance.clanChanged);
			int val3 = stream.Position - position4;
			Span<byte> span4 = range4.GetSpan();
			int num5 = ProtocolParser.WriteUInt32((uint)val3, span4, 0);
			if (num5 < 5)
			{
				span4[num5 - 1] |= 128;
				while (num5 < 4)
				{
					span4[num5++] = 128;
				}
				span4[4] = 0;
			}
		}
		if (instance.clanMessage != null)
		{
			stream.WriteByte(66);
			BufferStream.RangeHandle range5 = stream.GetRange(5);
			int position5 = stream.Position;
			AppNewClanMessage.Serialize(stream, instance.clanMessage);
			int val4 = stream.Position - position5;
			Span<byte> span5 = range5.GetSpan();
			int num6 = ProtocolParser.WriteUInt32((uint)val4, span5, 0);
			if (num6 < 5)
			{
				span5[num6 - 1] |= 128;
				while (num6 < 4)
				{
					span5[num6++] = 128;
				}
				span5[4] = 0;
			}
		}
		if (instance.cameraRays == null)
		{
			return;
		}
		stream.WriteByte(82);
		BufferStream.RangeHandle range6 = stream.GetRange(5);
		int position6 = stream.Position;
		AppCameraRays.Serialize(stream, instance.cameraRays);
		int val5 = stream.Position - position6;
		Span<byte> span6 = range6.GetSpan();
		int num7 = ProtocolParser.WriteUInt32((uint)val5, span6, 0);
		if (num7 < 5)
		{
			span6[num7 - 1] |= 128;
			while (num7 < 4)
			{
				span6[num7++] = 128;
			}
			span6[4] = 0;
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		teamChanged?.InspectUids(action);
		teamMessage?.InspectUids(action);
		entityChanged?.InspectUids(action);
		clanChanged?.InspectUids(action);
		clanMessage?.InspectUids(action);
		cameraRays?.InspectUids(action);
	}
}
