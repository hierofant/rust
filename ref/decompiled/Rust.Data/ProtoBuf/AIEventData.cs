using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class AIEventData : IDisposable, Pool.IPooled, IProto<AIEventData>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public int eventType;

	[NonSerialized]
	public int triggerStateContainer;

	[NonSerialized]
	public bool inverted;

	[NonSerialized]
	public int inputMemorySlot;

	[NonSerialized]
	public int outputMemorySlot;

	[NonSerialized]
	public int id;

	[NonSerialized]
	public TimerAIEventData timerData;

	[NonSerialized]
	public PlayerDetectedAIEventData playerDetectedData;

	[NonSerialized]
	public HealthBelowAIEventData healthBelowData;

	[NonSerialized]
	public InRangeAIEventData inRangeData;

	[NonSerialized]
	public HungerAboveAIEventData hungerAboveData;

	[NonSerialized]
	public TirednessAboveAIEventData tirednessAboveData;

	[NonSerialized]
	public ThreatDetectedAIEventData threatDetectedData;

	[NonSerialized]
	public TargetDetectedAIEventData targetDetectedData;

	[NonSerialized]
	public AmmoBelowAIEventData ammoBelowData;

	[NonSerialized]
	public ChanceAIEventData chanceData;

	[NonSerialized]
	public TimeSinceThreatAIEventData timeSinceThreatData;

	[NonSerialized]
	public AggressionTimerAIEventData aggressionTimerData;

	[NonSerialized]
	public InRangeOfHomeAIEventData inRangeOfHomeData;

	public static void ResetToPool(AIEventData instance)
	{
		if (instance.ShouldPool)
		{
			instance.eventType = 0;
			instance.triggerStateContainer = 0;
			instance.inverted = false;
			instance.inputMemorySlot = 0;
			instance.outputMemorySlot = 0;
			instance.id = 0;
			if (instance.timerData != null)
			{
				instance.timerData.ResetToPool();
				instance.timerData = null;
			}
			if (instance.playerDetectedData != null)
			{
				instance.playerDetectedData.ResetToPool();
				instance.playerDetectedData = null;
			}
			if (instance.healthBelowData != null)
			{
				instance.healthBelowData.ResetToPool();
				instance.healthBelowData = null;
			}
			if (instance.inRangeData != null)
			{
				instance.inRangeData.ResetToPool();
				instance.inRangeData = null;
			}
			if (instance.hungerAboveData != null)
			{
				instance.hungerAboveData.ResetToPool();
				instance.hungerAboveData = null;
			}
			if (instance.tirednessAboveData != null)
			{
				instance.tirednessAboveData.ResetToPool();
				instance.tirednessAboveData = null;
			}
			if (instance.threatDetectedData != null)
			{
				instance.threatDetectedData.ResetToPool();
				instance.threatDetectedData = null;
			}
			if (instance.targetDetectedData != null)
			{
				instance.targetDetectedData.ResetToPool();
				instance.targetDetectedData = null;
			}
			if (instance.ammoBelowData != null)
			{
				instance.ammoBelowData.ResetToPool();
				instance.ammoBelowData = null;
			}
			if (instance.chanceData != null)
			{
				instance.chanceData.ResetToPool();
				instance.chanceData = null;
			}
			if (instance.timeSinceThreatData != null)
			{
				instance.timeSinceThreatData.ResetToPool();
				instance.timeSinceThreatData = null;
			}
			if (instance.aggressionTimerData != null)
			{
				instance.aggressionTimerData.ResetToPool();
				instance.aggressionTimerData = null;
			}
			if (instance.inRangeOfHomeData != null)
			{
				instance.inRangeOfHomeData.ResetToPool();
				instance.inRangeOfHomeData = null;
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
			throw new Exception("Trying to dispose AIEventData with ShouldPool set to false!");
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

	public void CopyTo(AIEventData instance)
	{
		instance.eventType = eventType;
		instance.triggerStateContainer = triggerStateContainer;
		instance.inverted = inverted;
		instance.inputMemorySlot = inputMemorySlot;
		instance.outputMemorySlot = outputMemorySlot;
		instance.id = id;
		if (timerData != null)
		{
			if (instance.timerData == null)
			{
				instance.timerData = timerData.Copy();
			}
			else
			{
				timerData.CopyTo(instance.timerData);
			}
		}
		else
		{
			instance.timerData = null;
		}
		if (playerDetectedData != null)
		{
			if (instance.playerDetectedData == null)
			{
				instance.playerDetectedData = playerDetectedData.Copy();
			}
			else
			{
				playerDetectedData.CopyTo(instance.playerDetectedData);
			}
		}
		else
		{
			instance.playerDetectedData = null;
		}
		if (healthBelowData != null)
		{
			if (instance.healthBelowData == null)
			{
				instance.healthBelowData = healthBelowData.Copy();
			}
			else
			{
				healthBelowData.CopyTo(instance.healthBelowData);
			}
		}
		else
		{
			instance.healthBelowData = null;
		}
		if (inRangeData != null)
		{
			if (instance.inRangeData == null)
			{
				instance.inRangeData = inRangeData.Copy();
			}
			else
			{
				inRangeData.CopyTo(instance.inRangeData);
			}
		}
		else
		{
			instance.inRangeData = null;
		}
		if (hungerAboveData != null)
		{
			if (instance.hungerAboveData == null)
			{
				instance.hungerAboveData = hungerAboveData.Copy();
			}
			else
			{
				hungerAboveData.CopyTo(instance.hungerAboveData);
			}
		}
		else
		{
			instance.hungerAboveData = null;
		}
		if (tirednessAboveData != null)
		{
			if (instance.tirednessAboveData == null)
			{
				instance.tirednessAboveData = tirednessAboveData.Copy();
			}
			else
			{
				tirednessAboveData.CopyTo(instance.tirednessAboveData);
			}
		}
		else
		{
			instance.tirednessAboveData = null;
		}
		if (threatDetectedData != null)
		{
			if (instance.threatDetectedData == null)
			{
				instance.threatDetectedData = threatDetectedData.Copy();
			}
			else
			{
				threatDetectedData.CopyTo(instance.threatDetectedData);
			}
		}
		else
		{
			instance.threatDetectedData = null;
		}
		if (targetDetectedData != null)
		{
			if (instance.targetDetectedData == null)
			{
				instance.targetDetectedData = targetDetectedData.Copy();
			}
			else
			{
				targetDetectedData.CopyTo(instance.targetDetectedData);
			}
		}
		else
		{
			instance.targetDetectedData = null;
		}
		if (ammoBelowData != null)
		{
			if (instance.ammoBelowData == null)
			{
				instance.ammoBelowData = ammoBelowData.Copy();
			}
			else
			{
				ammoBelowData.CopyTo(instance.ammoBelowData);
			}
		}
		else
		{
			instance.ammoBelowData = null;
		}
		if (chanceData != null)
		{
			if (instance.chanceData == null)
			{
				instance.chanceData = chanceData.Copy();
			}
			else
			{
				chanceData.CopyTo(instance.chanceData);
			}
		}
		else
		{
			instance.chanceData = null;
		}
		if (timeSinceThreatData != null)
		{
			if (instance.timeSinceThreatData == null)
			{
				instance.timeSinceThreatData = timeSinceThreatData.Copy();
			}
			else
			{
				timeSinceThreatData.CopyTo(instance.timeSinceThreatData);
			}
		}
		else
		{
			instance.timeSinceThreatData = null;
		}
		if (aggressionTimerData != null)
		{
			if (instance.aggressionTimerData == null)
			{
				instance.aggressionTimerData = aggressionTimerData.Copy();
			}
			else
			{
				aggressionTimerData.CopyTo(instance.aggressionTimerData);
			}
		}
		else
		{
			instance.aggressionTimerData = null;
		}
		if (inRangeOfHomeData != null)
		{
			if (instance.inRangeOfHomeData == null)
			{
				instance.inRangeOfHomeData = inRangeOfHomeData.Copy();
			}
			else
			{
				inRangeOfHomeData.CopyTo(instance.inRangeOfHomeData);
			}
		}
		else
		{
			instance.inRangeOfHomeData = null;
		}
	}

	public AIEventData Copy()
	{
		AIEventData aIEventData = Pool.Get<AIEventData>();
		CopyTo(aIEventData);
		return aIEventData;
	}

	public static AIEventData Deserialize(BufferStream stream)
	{
		AIEventData aIEventData = Pool.Get<AIEventData>();
		Deserialize(stream, aIEventData, isDelta: false);
		return aIEventData;
	}

	public static AIEventData DeserializeLengthDelimited(BufferStream stream)
	{
		AIEventData aIEventData = Pool.Get<AIEventData>();
		DeserializeLengthDelimited(stream, aIEventData, isDelta: false);
		return aIEventData;
	}

	public static AIEventData DeserializeLength(BufferStream stream, int length)
	{
		AIEventData aIEventData = Pool.Get<AIEventData>();
		DeserializeLength(stream, length, aIEventData, isDelta: false);
		return aIEventData;
	}

	public static AIEventData Deserialize(byte[] buffer)
	{
		AIEventData aIEventData = Pool.Get<AIEventData>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, aIEventData, isDelta: false);
		return aIEventData;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, AIEventData previous)
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

	public static AIEventData Deserialize(BufferStream stream, AIEventData instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.AIEventData");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				instance.eventType = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				instance.triggerStateContainer = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				instance.inverted = ProtocolParser.ReadBool(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				instance.inputMemorySlot = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				instance.outputMemorySlot = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				instance.id = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 100u:
				stream.ValidateFieldOrder(ref lastFieldId, 100u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.timerData == null)
					{
						instance.timerData = TimerAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						TimerAIEventData.DeserializeLengthDelimited(stream, instance.timerData, isDelta);
					}
				}
				break;
			case 101u:
				stream.ValidateFieldOrder(ref lastFieldId, 101u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.playerDetectedData == null)
					{
						instance.playerDetectedData = PlayerDetectedAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						PlayerDetectedAIEventData.DeserializeLengthDelimited(stream, instance.playerDetectedData, isDelta);
					}
				}
				break;
			case 102u:
				stream.ValidateFieldOrder(ref lastFieldId, 102u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.healthBelowData == null)
					{
						instance.healthBelowData = HealthBelowAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						HealthBelowAIEventData.DeserializeLengthDelimited(stream, instance.healthBelowData, isDelta);
					}
				}
				break;
			case 103u:
				stream.ValidateFieldOrder(ref lastFieldId, 103u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.inRangeData == null)
					{
						instance.inRangeData = InRangeAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						InRangeAIEventData.DeserializeLengthDelimited(stream, instance.inRangeData, isDelta);
					}
				}
				break;
			case 104u:
				stream.ValidateFieldOrder(ref lastFieldId, 104u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.hungerAboveData == null)
					{
						instance.hungerAboveData = HungerAboveAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						HungerAboveAIEventData.DeserializeLengthDelimited(stream, instance.hungerAboveData, isDelta);
					}
				}
				break;
			case 105u:
				stream.ValidateFieldOrder(ref lastFieldId, 105u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.tirednessAboveData == null)
					{
						instance.tirednessAboveData = TirednessAboveAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						TirednessAboveAIEventData.DeserializeLengthDelimited(stream, instance.tirednessAboveData, isDelta);
					}
				}
				break;
			case 106u:
				stream.ValidateFieldOrder(ref lastFieldId, 106u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.threatDetectedData == null)
					{
						instance.threatDetectedData = ThreatDetectedAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						ThreatDetectedAIEventData.DeserializeLengthDelimited(stream, instance.threatDetectedData, isDelta);
					}
				}
				break;
			case 107u:
				stream.ValidateFieldOrder(ref lastFieldId, 107u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.targetDetectedData == null)
					{
						instance.targetDetectedData = TargetDetectedAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						TargetDetectedAIEventData.DeserializeLengthDelimited(stream, instance.targetDetectedData, isDelta);
					}
				}
				break;
			case 108u:
				stream.ValidateFieldOrder(ref lastFieldId, 108u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.ammoBelowData == null)
					{
						instance.ammoBelowData = AmmoBelowAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						AmmoBelowAIEventData.DeserializeLengthDelimited(stream, instance.ammoBelowData, isDelta);
					}
				}
				break;
			case 109u:
				stream.ValidateFieldOrder(ref lastFieldId, 109u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.chanceData == null)
					{
						instance.chanceData = ChanceAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						ChanceAIEventData.DeserializeLengthDelimited(stream, instance.chanceData, isDelta);
					}
				}
				break;
			case 110u:
				stream.ValidateFieldOrder(ref lastFieldId, 110u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.timeSinceThreatData == null)
					{
						instance.timeSinceThreatData = TimeSinceThreatAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						TimeSinceThreatAIEventData.DeserializeLengthDelimited(stream, instance.timeSinceThreatData, isDelta);
					}
				}
				break;
			case 111u:
				stream.ValidateFieldOrder(ref lastFieldId, 111u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.aggressionTimerData == null)
					{
						instance.aggressionTimerData = AggressionTimerAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						AggressionTimerAIEventData.DeserializeLengthDelimited(stream, instance.aggressionTimerData, isDelta);
					}
				}
				break;
			case 112u:
				stream.ValidateFieldOrder(ref lastFieldId, 112u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.inRangeOfHomeData == null)
					{
						instance.inRangeOfHomeData = InRangeOfHomeAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						InRangeOfHomeAIEventData.DeserializeLengthDelimited(stream, instance.inRangeOfHomeData, isDelta);
					}
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AIEventData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AIEventData DeserializeLengthDelimited(BufferStream stream, AIEventData instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.AIEventData");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				instance.eventType = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				instance.triggerStateContainer = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				instance.inverted = ProtocolParser.ReadBool(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				instance.inputMemorySlot = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				instance.outputMemorySlot = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				instance.id = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 100u:
				stream.ValidateFieldOrder(ref lastFieldId, 100u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.timerData == null)
					{
						instance.timerData = TimerAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						TimerAIEventData.DeserializeLengthDelimited(stream, instance.timerData, isDelta);
					}
				}
				break;
			case 101u:
				stream.ValidateFieldOrder(ref lastFieldId, 101u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.playerDetectedData == null)
					{
						instance.playerDetectedData = PlayerDetectedAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						PlayerDetectedAIEventData.DeserializeLengthDelimited(stream, instance.playerDetectedData, isDelta);
					}
				}
				break;
			case 102u:
				stream.ValidateFieldOrder(ref lastFieldId, 102u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.healthBelowData == null)
					{
						instance.healthBelowData = HealthBelowAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						HealthBelowAIEventData.DeserializeLengthDelimited(stream, instance.healthBelowData, isDelta);
					}
				}
				break;
			case 103u:
				stream.ValidateFieldOrder(ref lastFieldId, 103u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.inRangeData == null)
					{
						instance.inRangeData = InRangeAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						InRangeAIEventData.DeserializeLengthDelimited(stream, instance.inRangeData, isDelta);
					}
				}
				break;
			case 104u:
				stream.ValidateFieldOrder(ref lastFieldId, 104u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.hungerAboveData == null)
					{
						instance.hungerAboveData = HungerAboveAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						HungerAboveAIEventData.DeserializeLengthDelimited(stream, instance.hungerAboveData, isDelta);
					}
				}
				break;
			case 105u:
				stream.ValidateFieldOrder(ref lastFieldId, 105u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.tirednessAboveData == null)
					{
						instance.tirednessAboveData = TirednessAboveAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						TirednessAboveAIEventData.DeserializeLengthDelimited(stream, instance.tirednessAboveData, isDelta);
					}
				}
				break;
			case 106u:
				stream.ValidateFieldOrder(ref lastFieldId, 106u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.threatDetectedData == null)
					{
						instance.threatDetectedData = ThreatDetectedAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						ThreatDetectedAIEventData.DeserializeLengthDelimited(stream, instance.threatDetectedData, isDelta);
					}
				}
				break;
			case 107u:
				stream.ValidateFieldOrder(ref lastFieldId, 107u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.targetDetectedData == null)
					{
						instance.targetDetectedData = TargetDetectedAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						TargetDetectedAIEventData.DeserializeLengthDelimited(stream, instance.targetDetectedData, isDelta);
					}
				}
				break;
			case 108u:
				stream.ValidateFieldOrder(ref lastFieldId, 108u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.ammoBelowData == null)
					{
						instance.ammoBelowData = AmmoBelowAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						AmmoBelowAIEventData.DeserializeLengthDelimited(stream, instance.ammoBelowData, isDelta);
					}
				}
				break;
			case 109u:
				stream.ValidateFieldOrder(ref lastFieldId, 109u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.chanceData == null)
					{
						instance.chanceData = ChanceAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						ChanceAIEventData.DeserializeLengthDelimited(stream, instance.chanceData, isDelta);
					}
				}
				break;
			case 110u:
				stream.ValidateFieldOrder(ref lastFieldId, 110u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.timeSinceThreatData == null)
					{
						instance.timeSinceThreatData = TimeSinceThreatAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						TimeSinceThreatAIEventData.DeserializeLengthDelimited(stream, instance.timeSinceThreatData, isDelta);
					}
				}
				break;
			case 111u:
				stream.ValidateFieldOrder(ref lastFieldId, 111u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.aggressionTimerData == null)
					{
						instance.aggressionTimerData = AggressionTimerAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						AggressionTimerAIEventData.DeserializeLengthDelimited(stream, instance.aggressionTimerData, isDelta);
					}
				}
				break;
			case 112u:
				stream.ValidateFieldOrder(ref lastFieldId, 112u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.inRangeOfHomeData == null)
					{
						instance.inRangeOfHomeData = InRangeOfHomeAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						InRangeOfHomeAIEventData.DeserializeLengthDelimited(stream, instance.inRangeOfHomeData, isDelta);
					}
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AIEventData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AIEventData DeserializeLength(BufferStream stream, int length, AIEventData instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.AIEventData");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				instance.eventType = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				instance.triggerStateContainer = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				instance.inverted = ProtocolParser.ReadBool(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				instance.inputMemorySlot = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				instance.outputMemorySlot = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				instance.id = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 100u:
				stream.ValidateFieldOrder(ref lastFieldId, 100u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.timerData == null)
					{
						instance.timerData = TimerAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						TimerAIEventData.DeserializeLengthDelimited(stream, instance.timerData, isDelta);
					}
				}
				break;
			case 101u:
				stream.ValidateFieldOrder(ref lastFieldId, 101u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.playerDetectedData == null)
					{
						instance.playerDetectedData = PlayerDetectedAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						PlayerDetectedAIEventData.DeserializeLengthDelimited(stream, instance.playerDetectedData, isDelta);
					}
				}
				break;
			case 102u:
				stream.ValidateFieldOrder(ref lastFieldId, 102u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.healthBelowData == null)
					{
						instance.healthBelowData = HealthBelowAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						HealthBelowAIEventData.DeserializeLengthDelimited(stream, instance.healthBelowData, isDelta);
					}
				}
				break;
			case 103u:
				stream.ValidateFieldOrder(ref lastFieldId, 103u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.inRangeData == null)
					{
						instance.inRangeData = InRangeAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						InRangeAIEventData.DeserializeLengthDelimited(stream, instance.inRangeData, isDelta);
					}
				}
				break;
			case 104u:
				stream.ValidateFieldOrder(ref lastFieldId, 104u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.hungerAboveData == null)
					{
						instance.hungerAboveData = HungerAboveAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						HungerAboveAIEventData.DeserializeLengthDelimited(stream, instance.hungerAboveData, isDelta);
					}
				}
				break;
			case 105u:
				stream.ValidateFieldOrder(ref lastFieldId, 105u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.tirednessAboveData == null)
					{
						instance.tirednessAboveData = TirednessAboveAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						TirednessAboveAIEventData.DeserializeLengthDelimited(stream, instance.tirednessAboveData, isDelta);
					}
				}
				break;
			case 106u:
				stream.ValidateFieldOrder(ref lastFieldId, 106u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.threatDetectedData == null)
					{
						instance.threatDetectedData = ThreatDetectedAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						ThreatDetectedAIEventData.DeserializeLengthDelimited(stream, instance.threatDetectedData, isDelta);
					}
				}
				break;
			case 107u:
				stream.ValidateFieldOrder(ref lastFieldId, 107u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.targetDetectedData == null)
					{
						instance.targetDetectedData = TargetDetectedAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						TargetDetectedAIEventData.DeserializeLengthDelimited(stream, instance.targetDetectedData, isDelta);
					}
				}
				break;
			case 108u:
				stream.ValidateFieldOrder(ref lastFieldId, 108u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.ammoBelowData == null)
					{
						instance.ammoBelowData = AmmoBelowAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						AmmoBelowAIEventData.DeserializeLengthDelimited(stream, instance.ammoBelowData, isDelta);
					}
				}
				break;
			case 109u:
				stream.ValidateFieldOrder(ref lastFieldId, 109u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.chanceData == null)
					{
						instance.chanceData = ChanceAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						ChanceAIEventData.DeserializeLengthDelimited(stream, instance.chanceData, isDelta);
					}
				}
				break;
			case 110u:
				stream.ValidateFieldOrder(ref lastFieldId, 110u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.timeSinceThreatData == null)
					{
						instance.timeSinceThreatData = TimeSinceThreatAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						TimeSinceThreatAIEventData.DeserializeLengthDelimited(stream, instance.timeSinceThreatData, isDelta);
					}
				}
				break;
			case 111u:
				stream.ValidateFieldOrder(ref lastFieldId, 111u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.aggressionTimerData == null)
					{
						instance.aggressionTimerData = AggressionTimerAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						AggressionTimerAIEventData.DeserializeLengthDelimited(stream, instance.aggressionTimerData, isDelta);
					}
				}
				break;
			case 112u:
				stream.ValidateFieldOrder(ref lastFieldId, 112u, fieldIsRepeated: false, "ProtoBuf.AIEventData");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.inRangeOfHomeData == null)
					{
						instance.inRangeOfHomeData = InRangeOfHomeAIEventData.DeserializeLengthDelimited(stream);
					}
					else
					{
						InRangeOfHomeAIEventData.DeserializeLengthDelimited(stream, instance.inRangeOfHomeData, isDelta);
					}
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AIEventData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, AIEventData instance, AIEventData previous)
	{
		if (instance.eventType != previous.eventType)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.eventType);
		}
		if (instance.triggerStateContainer != previous.triggerStateContainer)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.triggerStateContainer);
		}
		stream.WriteByte(24);
		ProtocolParser.WriteBool(stream, instance.inverted);
		if (instance.inputMemorySlot != previous.inputMemorySlot)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.inputMemorySlot);
		}
		if (instance.outputMemorySlot != previous.outputMemorySlot)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.outputMemorySlot);
		}
		if (instance.id != previous.id)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.id);
		}
		if (instance.timerData != null)
		{
			stream.WriteByte(162);
			stream.WriteByte(6);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			TimerAIEventData.SerializeDelta(stream, instance.timerData, previous.timerData);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field timerData (ProtoBuf.TimerAIEventData)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.playerDetectedData != null)
		{
			stream.WriteByte(170);
			stream.WriteByte(6);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			PlayerDetectedAIEventData.SerializeDelta(stream, instance.playerDetectedData, previous.playerDetectedData);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field playerDetectedData (ProtoBuf.PlayerDetectedAIEventData)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.healthBelowData != null)
		{
			stream.WriteByte(178);
			stream.WriteByte(6);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			HealthBelowAIEventData.SerializeDelta(stream, instance.healthBelowData, previous.healthBelowData);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field healthBelowData (ProtoBuf.HealthBelowAIEventData)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
		if (instance.inRangeData != null)
		{
			stream.WriteByte(186);
			stream.WriteByte(6);
			BufferStream.RangeHandle range4 = stream.GetRange(1);
			int position4 = stream.Position;
			InRangeAIEventData.SerializeDelta(stream, instance.inRangeData, previous.inRangeData);
			int num4 = stream.Position - position4;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field inRangeData (ProtoBuf.InRangeAIEventData)");
			}
			Span<byte> span4 = range4.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span4, 0);
		}
		if (instance.hungerAboveData != null)
		{
			stream.WriteByte(194);
			stream.WriteByte(6);
			BufferStream.RangeHandle range5 = stream.GetRange(1);
			int position5 = stream.Position;
			HungerAboveAIEventData.SerializeDelta(stream, instance.hungerAboveData, previous.hungerAboveData);
			int num5 = stream.Position - position5;
			if (num5 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field hungerAboveData (ProtoBuf.HungerAboveAIEventData)");
			}
			Span<byte> span5 = range5.GetSpan();
			ProtocolParser.WriteUInt32((uint)num5, span5, 0);
		}
		if (instance.tirednessAboveData != null)
		{
			stream.WriteByte(202);
			stream.WriteByte(6);
			BufferStream.RangeHandle range6 = stream.GetRange(1);
			int position6 = stream.Position;
			TirednessAboveAIEventData.SerializeDelta(stream, instance.tirednessAboveData, previous.tirednessAboveData);
			int num6 = stream.Position - position6;
			if (num6 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field tirednessAboveData (ProtoBuf.TirednessAboveAIEventData)");
			}
			Span<byte> span6 = range6.GetSpan();
			ProtocolParser.WriteUInt32((uint)num6, span6, 0);
		}
		if (instance.threatDetectedData != null)
		{
			stream.WriteByte(210);
			stream.WriteByte(6);
			BufferStream.RangeHandle range7 = stream.GetRange(1);
			int position7 = stream.Position;
			ThreatDetectedAIEventData.SerializeDelta(stream, instance.threatDetectedData, previous.threatDetectedData);
			int num7 = stream.Position - position7;
			if (num7 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field threatDetectedData (ProtoBuf.ThreatDetectedAIEventData)");
			}
			Span<byte> span7 = range7.GetSpan();
			ProtocolParser.WriteUInt32((uint)num7, span7, 0);
		}
		if (instance.targetDetectedData != null)
		{
			stream.WriteByte(218);
			stream.WriteByte(6);
			BufferStream.RangeHandle range8 = stream.GetRange(1);
			int position8 = stream.Position;
			TargetDetectedAIEventData.SerializeDelta(stream, instance.targetDetectedData, previous.targetDetectedData);
			int num8 = stream.Position - position8;
			if (num8 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field targetDetectedData (ProtoBuf.TargetDetectedAIEventData)");
			}
			Span<byte> span8 = range8.GetSpan();
			ProtocolParser.WriteUInt32((uint)num8, span8, 0);
		}
		if (instance.ammoBelowData != null)
		{
			stream.WriteByte(226);
			stream.WriteByte(6);
			BufferStream.RangeHandle range9 = stream.GetRange(1);
			int position9 = stream.Position;
			AmmoBelowAIEventData.SerializeDelta(stream, instance.ammoBelowData, previous.ammoBelowData);
			int num9 = stream.Position - position9;
			if (num9 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field ammoBelowData (ProtoBuf.AmmoBelowAIEventData)");
			}
			Span<byte> span9 = range9.GetSpan();
			ProtocolParser.WriteUInt32((uint)num9, span9, 0);
		}
		if (instance.chanceData != null)
		{
			stream.WriteByte(234);
			stream.WriteByte(6);
			BufferStream.RangeHandle range10 = stream.GetRange(1);
			int position10 = stream.Position;
			ChanceAIEventData.SerializeDelta(stream, instance.chanceData, previous.chanceData);
			int num10 = stream.Position - position10;
			if (num10 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field chanceData (ProtoBuf.ChanceAIEventData)");
			}
			Span<byte> span10 = range10.GetSpan();
			ProtocolParser.WriteUInt32((uint)num10, span10, 0);
		}
		if (instance.timeSinceThreatData != null)
		{
			stream.WriteByte(242);
			stream.WriteByte(6);
			BufferStream.RangeHandle range11 = stream.GetRange(1);
			int position11 = stream.Position;
			TimeSinceThreatAIEventData.SerializeDelta(stream, instance.timeSinceThreatData, previous.timeSinceThreatData);
			int num11 = stream.Position - position11;
			if (num11 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field timeSinceThreatData (ProtoBuf.TimeSinceThreatAIEventData)");
			}
			Span<byte> span11 = range11.GetSpan();
			ProtocolParser.WriteUInt32((uint)num11, span11, 0);
		}
		if (instance.aggressionTimerData != null)
		{
			stream.WriteByte(250);
			stream.WriteByte(6);
			BufferStream.RangeHandle range12 = stream.GetRange(1);
			int position12 = stream.Position;
			AggressionTimerAIEventData.SerializeDelta(stream, instance.aggressionTimerData, previous.aggressionTimerData);
			int num12 = stream.Position - position12;
			if (num12 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field aggressionTimerData (ProtoBuf.AggressionTimerAIEventData)");
			}
			Span<byte> span12 = range12.GetSpan();
			ProtocolParser.WriteUInt32((uint)num12, span12, 0);
		}
		if (instance.inRangeOfHomeData != null)
		{
			stream.WriteByte(130);
			stream.WriteByte(7);
			BufferStream.RangeHandle range13 = stream.GetRange(1);
			int position13 = stream.Position;
			InRangeOfHomeAIEventData.SerializeDelta(stream, instance.inRangeOfHomeData, previous.inRangeOfHomeData);
			int num13 = stream.Position - position13;
			if (num13 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field inRangeOfHomeData (ProtoBuf.InRangeOfHomeAIEventData)");
			}
			Span<byte> span13 = range13.GetSpan();
			ProtocolParser.WriteUInt32((uint)num13, span13, 0);
		}
	}

	public static void Serialize(BufferStream stream, AIEventData instance)
	{
		if (instance.eventType != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.eventType);
		}
		if (instance.triggerStateContainer != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.triggerStateContainer);
		}
		if (instance.inverted)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteBool(stream, instance.inverted);
		}
		if (instance.inputMemorySlot != 0)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.inputMemorySlot);
		}
		if (instance.outputMemorySlot != 0)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.outputMemorySlot);
		}
		if (instance.id != 0)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.id);
		}
		if (instance.timerData != null)
		{
			stream.WriteByte(162);
			stream.WriteByte(6);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			TimerAIEventData.Serialize(stream, instance.timerData);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field timerData (ProtoBuf.TimerAIEventData)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.playerDetectedData != null)
		{
			stream.WriteByte(170);
			stream.WriteByte(6);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			PlayerDetectedAIEventData.Serialize(stream, instance.playerDetectedData);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field playerDetectedData (ProtoBuf.PlayerDetectedAIEventData)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.healthBelowData != null)
		{
			stream.WriteByte(178);
			stream.WriteByte(6);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			HealthBelowAIEventData.Serialize(stream, instance.healthBelowData);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field healthBelowData (ProtoBuf.HealthBelowAIEventData)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
		if (instance.inRangeData != null)
		{
			stream.WriteByte(186);
			stream.WriteByte(6);
			BufferStream.RangeHandle range4 = stream.GetRange(1);
			int position4 = stream.Position;
			InRangeAIEventData.Serialize(stream, instance.inRangeData);
			int num4 = stream.Position - position4;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field inRangeData (ProtoBuf.InRangeAIEventData)");
			}
			Span<byte> span4 = range4.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span4, 0);
		}
		if (instance.hungerAboveData != null)
		{
			stream.WriteByte(194);
			stream.WriteByte(6);
			BufferStream.RangeHandle range5 = stream.GetRange(1);
			int position5 = stream.Position;
			HungerAboveAIEventData.Serialize(stream, instance.hungerAboveData);
			int num5 = stream.Position - position5;
			if (num5 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field hungerAboveData (ProtoBuf.HungerAboveAIEventData)");
			}
			Span<byte> span5 = range5.GetSpan();
			ProtocolParser.WriteUInt32((uint)num5, span5, 0);
		}
		if (instance.tirednessAboveData != null)
		{
			stream.WriteByte(202);
			stream.WriteByte(6);
			BufferStream.RangeHandle range6 = stream.GetRange(1);
			int position6 = stream.Position;
			TirednessAboveAIEventData.Serialize(stream, instance.tirednessAboveData);
			int num6 = stream.Position - position6;
			if (num6 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field tirednessAboveData (ProtoBuf.TirednessAboveAIEventData)");
			}
			Span<byte> span6 = range6.GetSpan();
			ProtocolParser.WriteUInt32((uint)num6, span6, 0);
		}
		if (instance.threatDetectedData != null)
		{
			stream.WriteByte(210);
			stream.WriteByte(6);
			BufferStream.RangeHandle range7 = stream.GetRange(1);
			int position7 = stream.Position;
			ThreatDetectedAIEventData.Serialize(stream, instance.threatDetectedData);
			int num7 = stream.Position - position7;
			if (num7 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field threatDetectedData (ProtoBuf.ThreatDetectedAIEventData)");
			}
			Span<byte> span7 = range7.GetSpan();
			ProtocolParser.WriteUInt32((uint)num7, span7, 0);
		}
		if (instance.targetDetectedData != null)
		{
			stream.WriteByte(218);
			stream.WriteByte(6);
			BufferStream.RangeHandle range8 = stream.GetRange(1);
			int position8 = stream.Position;
			TargetDetectedAIEventData.Serialize(stream, instance.targetDetectedData);
			int num8 = stream.Position - position8;
			if (num8 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field targetDetectedData (ProtoBuf.TargetDetectedAIEventData)");
			}
			Span<byte> span8 = range8.GetSpan();
			ProtocolParser.WriteUInt32((uint)num8, span8, 0);
		}
		if (instance.ammoBelowData != null)
		{
			stream.WriteByte(226);
			stream.WriteByte(6);
			BufferStream.RangeHandle range9 = stream.GetRange(1);
			int position9 = stream.Position;
			AmmoBelowAIEventData.Serialize(stream, instance.ammoBelowData);
			int num9 = stream.Position - position9;
			if (num9 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field ammoBelowData (ProtoBuf.AmmoBelowAIEventData)");
			}
			Span<byte> span9 = range9.GetSpan();
			ProtocolParser.WriteUInt32((uint)num9, span9, 0);
		}
		if (instance.chanceData != null)
		{
			stream.WriteByte(234);
			stream.WriteByte(6);
			BufferStream.RangeHandle range10 = stream.GetRange(1);
			int position10 = stream.Position;
			ChanceAIEventData.Serialize(stream, instance.chanceData);
			int num10 = stream.Position - position10;
			if (num10 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field chanceData (ProtoBuf.ChanceAIEventData)");
			}
			Span<byte> span10 = range10.GetSpan();
			ProtocolParser.WriteUInt32((uint)num10, span10, 0);
		}
		if (instance.timeSinceThreatData != null)
		{
			stream.WriteByte(242);
			stream.WriteByte(6);
			BufferStream.RangeHandle range11 = stream.GetRange(1);
			int position11 = stream.Position;
			TimeSinceThreatAIEventData.Serialize(stream, instance.timeSinceThreatData);
			int num11 = stream.Position - position11;
			if (num11 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field timeSinceThreatData (ProtoBuf.TimeSinceThreatAIEventData)");
			}
			Span<byte> span11 = range11.GetSpan();
			ProtocolParser.WriteUInt32((uint)num11, span11, 0);
		}
		if (instance.aggressionTimerData != null)
		{
			stream.WriteByte(250);
			stream.WriteByte(6);
			BufferStream.RangeHandle range12 = stream.GetRange(1);
			int position12 = stream.Position;
			AggressionTimerAIEventData.Serialize(stream, instance.aggressionTimerData);
			int num12 = stream.Position - position12;
			if (num12 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field aggressionTimerData (ProtoBuf.AggressionTimerAIEventData)");
			}
			Span<byte> span12 = range12.GetSpan();
			ProtocolParser.WriteUInt32((uint)num12, span12, 0);
		}
		if (instance.inRangeOfHomeData != null)
		{
			stream.WriteByte(130);
			stream.WriteByte(7);
			BufferStream.RangeHandle range13 = stream.GetRange(1);
			int position13 = stream.Position;
			InRangeOfHomeAIEventData.Serialize(stream, instance.inRangeOfHomeData);
			int num13 = stream.Position - position13;
			if (num13 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field inRangeOfHomeData (ProtoBuf.InRangeOfHomeAIEventData)");
			}
			Span<byte> span13 = range13.GetSpan();
			ProtocolParser.WriteUInt32((uint)num13, span13, 0);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		timerData?.InspectUids(action);
		playerDetectedData?.InspectUids(action);
		healthBelowData?.InspectUids(action);
		inRangeData?.InspectUids(action);
		hungerAboveData?.InspectUids(action);
		tirednessAboveData?.InspectUids(action);
		threatDetectedData?.InspectUids(action);
		targetDetectedData?.InspectUids(action);
		ammoBelowData?.InspectUids(action);
		chanceData?.InspectUids(action);
		timeSinceThreatData?.InspectUids(action);
		aggressionTimerData?.InspectUids(action);
		inRangeOfHomeData?.InspectUids(action);
	}
}
