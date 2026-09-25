using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class BasePlayer : IDisposable, Pool.IPooled, IProto<BasePlayer>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public string name;

	[NonSerialized]
	public ulong userid;

	[NonSerialized]
	public PlayerInventory inventory;

	[NonSerialized]
	public PlayerMetabolism metabolism;

	[NonSerialized]
	public float loadingTimeout;

	[NonSerialized]
	public ModelState modelState;

	[NonSerialized]
	public int playerFlags;

	[NonSerialized]
	public ItemId heldEntity;

	[NonSerialized]
	public float health;

	[NonSerialized]
	public PersistantPlayer persistantData;

	[NonSerialized]
	public float skinCol;

	[NonSerialized]
	public float skinTex;

	[NonSerialized]
	public float skinMesh;

	[NonSerialized]
	public PlayerLifeStory currentLife;

	[NonSerialized]
	public PlayerLifeStory previousLife;

	[NonSerialized]
	public NetworkableId mounted;

	[NonSerialized]
	public ulong currentTeam;

	[NonSerialized]
	public uint underwear;

	[NonSerialized]
	public PlayerModifiers modifiers;

	[NonSerialized]
	public int reputation;

	[NonSerialized]
	public uint loopingGesture;

	[NonSerialized]
	public Missions missions;

	[NonSerialized]
	public string respawnId;

	[NonSerialized]
	public int bagCount;

	[NonSerialized]
	public long clanId;

	[NonSerialized]
	public ItemCrafter itemCrafter;

	[NonSerialized]
	public int shelterCount;

	[NonSerialized]
	public int tutorialAllowance;

	[NonSerialized]
	public int paintballColor;

	[NonSerialized]
	public int bbsCount;

	[NonSerialized]
	public float mortarCooldown;

	public static void ResetToPool(BasePlayer instance)
	{
		if (instance.ShouldPool)
		{
			instance.name = string.Empty;
			instance.userid = 0uL;
			if (instance.inventory != null)
			{
				instance.inventory.ResetToPool();
				instance.inventory = null;
			}
			if (instance.metabolism != null)
			{
				instance.metabolism.ResetToPool();
				instance.metabolism = null;
			}
			instance.loadingTimeout = 0f;
			if (instance.modelState != null)
			{
				instance.modelState.ResetToPool();
				instance.modelState = null;
			}
			instance.playerFlags = 0;
			instance.heldEntity = default(ItemId);
			instance.health = 0f;
			if (instance.persistantData != null)
			{
				instance.persistantData.ResetToPool();
				instance.persistantData = null;
			}
			instance.skinCol = 0f;
			instance.skinTex = 0f;
			instance.skinMesh = 0f;
			if (instance.currentLife != null)
			{
				instance.currentLife.ResetToPool();
				instance.currentLife = null;
			}
			if (instance.previousLife != null)
			{
				instance.previousLife.ResetToPool();
				instance.previousLife = null;
			}
			instance.mounted = default(NetworkableId);
			instance.currentTeam = 0uL;
			instance.underwear = 0u;
			if (instance.modifiers != null)
			{
				instance.modifiers.ResetToPool();
				instance.modifiers = null;
			}
			instance.reputation = 0;
			instance.loopingGesture = 0u;
			if (instance.missions != null)
			{
				instance.missions.ResetToPool();
				instance.missions = null;
			}
			instance.respawnId = string.Empty;
			instance.bagCount = 0;
			instance.clanId = 0L;
			if (instance.itemCrafter != null)
			{
				instance.itemCrafter.ResetToPool();
				instance.itemCrafter = null;
			}
			instance.shelterCount = 0;
			instance.tutorialAllowance = 0;
			instance.paintballColor = 0;
			instance.bbsCount = 0;
			instance.mortarCooldown = 0f;
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
			throw new Exception("Trying to dispose BasePlayer with ShouldPool set to false!");
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

	public void CopyTo(BasePlayer instance)
	{
		instance.name = name;
		instance.userid = userid;
		if (inventory != null)
		{
			if (instance.inventory == null)
			{
				instance.inventory = inventory.Copy();
			}
			else
			{
				inventory.CopyTo(instance.inventory);
			}
		}
		else
		{
			instance.inventory = null;
		}
		if (metabolism != null)
		{
			if (instance.metabolism == null)
			{
				instance.metabolism = metabolism.Copy();
			}
			else
			{
				metabolism.CopyTo(instance.metabolism);
			}
		}
		else
		{
			instance.metabolism = null;
		}
		instance.loadingTimeout = loadingTimeout;
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
		instance.playerFlags = playerFlags;
		instance.heldEntity = heldEntity;
		instance.health = health;
		if (persistantData != null)
		{
			if (instance.persistantData == null)
			{
				instance.persistantData = persistantData.Copy();
			}
			else
			{
				persistantData.CopyTo(instance.persistantData);
			}
		}
		else
		{
			instance.persistantData = null;
		}
		instance.skinCol = skinCol;
		instance.skinTex = skinTex;
		instance.skinMesh = skinMesh;
		if (currentLife != null)
		{
			if (instance.currentLife == null)
			{
				instance.currentLife = currentLife.Copy();
			}
			else
			{
				currentLife.CopyTo(instance.currentLife);
			}
		}
		else
		{
			instance.currentLife = null;
		}
		if (previousLife != null)
		{
			if (instance.previousLife == null)
			{
				instance.previousLife = previousLife.Copy();
			}
			else
			{
				previousLife.CopyTo(instance.previousLife);
			}
		}
		else
		{
			instance.previousLife = null;
		}
		instance.mounted = mounted;
		instance.currentTeam = currentTeam;
		instance.underwear = underwear;
		if (modifiers != null)
		{
			if (instance.modifiers == null)
			{
				instance.modifiers = modifiers.Copy();
			}
			else
			{
				modifiers.CopyTo(instance.modifiers);
			}
		}
		else
		{
			instance.modifiers = null;
		}
		instance.reputation = reputation;
		instance.loopingGesture = loopingGesture;
		if (missions != null)
		{
			if (instance.missions == null)
			{
				instance.missions = missions.Copy();
			}
			else
			{
				missions.CopyTo(instance.missions);
			}
		}
		else
		{
			instance.missions = null;
		}
		instance.respawnId = respawnId;
		instance.bagCount = bagCount;
		instance.clanId = clanId;
		if (itemCrafter != null)
		{
			if (instance.itemCrafter == null)
			{
				instance.itemCrafter = itemCrafter.Copy();
			}
			else
			{
				itemCrafter.CopyTo(instance.itemCrafter);
			}
		}
		else
		{
			instance.itemCrafter = null;
		}
		instance.shelterCount = shelterCount;
		instance.tutorialAllowance = tutorialAllowance;
		instance.paintballColor = paintballColor;
		instance.bbsCount = bbsCount;
		instance.mortarCooldown = mortarCooldown;
	}

	public BasePlayer Copy()
	{
		BasePlayer basePlayer = Pool.Get<BasePlayer>();
		CopyTo(basePlayer);
		return basePlayer;
	}

	public static BasePlayer Deserialize(BufferStream stream)
	{
		BasePlayer basePlayer = Pool.Get<BasePlayer>();
		Deserialize(stream, basePlayer, isDelta: false);
		return basePlayer;
	}

	public static BasePlayer DeserializeLengthDelimited(BufferStream stream)
	{
		BasePlayer basePlayer = Pool.Get<BasePlayer>();
		DeserializeLengthDelimited(stream, basePlayer, isDelta: false);
		return basePlayer;
	}

	public static BasePlayer DeserializeLength(BufferStream stream, int length)
	{
		BasePlayer basePlayer = Pool.Get<BasePlayer>();
		DeserializeLength(stream, length, basePlayer, isDelta: false);
		return basePlayer;
	}

	public static BasePlayer Deserialize(byte[] buffer)
	{
		BasePlayer basePlayer = Pool.Get<BasePlayer>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, basePlayer, isDelta: false);
		return basePlayer;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, BasePlayer previous)
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

	public static BasePlayer Deserialize(BufferStream stream, BasePlayer instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.BasePlayer");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				instance.name = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				instance.userid = ProtocolParser.ReadUInt64(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (instance.inventory == null)
				{
					instance.inventory = PlayerInventory.DeserializeLengthDelimited(stream);
				}
				else
				{
					PlayerInventory.DeserializeLengthDelimited(stream, instance.inventory, isDelta);
				}
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (instance.metabolism == null)
				{
					instance.metabolism = PlayerMetabolism.DeserializeLengthDelimited(stream);
				}
				else
				{
					PlayerMetabolism.DeserializeLengthDelimited(stream, instance.metabolism, isDelta);
				}
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				instance.loadingTimeout = ProtocolParser.ReadSingle(stream);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (instance.modelState == null)
				{
					instance.modelState = ModelState.DeserializeLengthDelimited(stream);
				}
				else
				{
					ModelState.DeserializeLengthDelimited(stream, instance.modelState, isDelta);
				}
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				instance.playerFlags = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				instance.heldEntity = new ItemId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 77:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				instance.health = ProtocolParser.ReadSingle(stream);
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (instance.persistantData == null)
				{
					instance.persistantData = PersistantPlayer.DeserializeLengthDelimited(stream);
				}
				else
				{
					PersistantPlayer.DeserializeLengthDelimited(stream, instance.persistantData, isDelta);
				}
				continue;
			case 125:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				instance.skinCol = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Fixed32)
				{
					instance.skinTex = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 17u:
				stream.ValidateFieldOrder(ref lastFieldId, 17u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Fixed32)
				{
					instance.skinMesh = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 20u:
				stream.ValidateFieldOrder(ref lastFieldId, 20u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.currentLife == null)
					{
						instance.currentLife = PlayerLifeStory.DeserializeLengthDelimited(stream);
					}
					else
					{
						PlayerLifeStory.DeserializeLengthDelimited(stream, instance.currentLife, isDelta);
					}
				}
				break;
			case 21u:
				stream.ValidateFieldOrder(ref lastFieldId, 21u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.previousLife == null)
					{
						instance.previousLife = PlayerLifeStory.DeserializeLengthDelimited(stream);
					}
					else
					{
						PlayerLifeStory.DeserializeLengthDelimited(stream, instance.previousLife, isDelta);
					}
				}
				break;
			case 22u:
				stream.ValidateFieldOrder(ref lastFieldId, 22u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.mounted = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				}
				break;
			case 23u:
				stream.ValidateFieldOrder(ref lastFieldId, 23u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.currentTeam = ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 24u:
				stream.ValidateFieldOrder(ref lastFieldId, 24u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.underwear = ProtocolParser.ReadUInt32(stream);
				}
				break;
			case 25u:
				stream.ValidateFieldOrder(ref lastFieldId, 25u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.modifiers == null)
					{
						instance.modifiers = PlayerModifiers.DeserializeLengthDelimited(stream);
					}
					else
					{
						PlayerModifiers.DeserializeLengthDelimited(stream, instance.modifiers, isDelta);
					}
				}
				break;
			case 26u:
				stream.ValidateFieldOrder(ref lastFieldId, 26u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.reputation = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 27u:
				stream.ValidateFieldOrder(ref lastFieldId, 27u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.loopingGesture = ProtocolParser.ReadUInt32(stream);
				}
				break;
			case 28u:
				stream.ValidateFieldOrder(ref lastFieldId, 28u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.missions == null)
					{
						instance.missions = Missions.DeserializeLengthDelimited(stream);
					}
					else
					{
						Missions.DeserializeLengthDelimited(stream, instance.missions, isDelta);
					}
				}
				break;
			case 29u:
				stream.ValidateFieldOrder(ref lastFieldId, 29u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.LengthDelimited)
				{
					instance.respawnId = ProtocolParser.ReadString(stream);
				}
				break;
			case 30u:
				stream.ValidateFieldOrder(ref lastFieldId, 30u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.bagCount = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 31u:
				stream.ValidateFieldOrder(ref lastFieldId, 31u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.clanId = (long)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 32u:
				stream.ValidateFieldOrder(ref lastFieldId, 32u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.itemCrafter == null)
					{
						instance.itemCrafter = ItemCrafter.DeserializeLengthDelimited(stream);
					}
					else
					{
						ItemCrafter.DeserializeLengthDelimited(stream, instance.itemCrafter, isDelta);
					}
				}
				break;
			case 33u:
				stream.ValidateFieldOrder(ref lastFieldId, 33u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.shelterCount = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 34u:
				stream.ValidateFieldOrder(ref lastFieldId, 34u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.tutorialAllowance = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 35u:
				stream.ValidateFieldOrder(ref lastFieldId, 35u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.paintballColor = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 36u:
				stream.ValidateFieldOrder(ref lastFieldId, 36u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.bbsCount = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 40u:
				stream.ValidateFieldOrder(ref lastFieldId, 40u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Fixed32)
				{
					instance.mortarCooldown = ProtocolParser.ReadSingle(stream);
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static BasePlayer DeserializeLengthDelimited(BufferStream stream, BasePlayer instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.BasePlayer");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				instance.name = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				instance.userid = ProtocolParser.ReadUInt64(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (instance.inventory == null)
				{
					instance.inventory = PlayerInventory.DeserializeLengthDelimited(stream);
				}
				else
				{
					PlayerInventory.DeserializeLengthDelimited(stream, instance.inventory, isDelta);
				}
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (instance.metabolism == null)
				{
					instance.metabolism = PlayerMetabolism.DeserializeLengthDelimited(stream);
				}
				else
				{
					PlayerMetabolism.DeserializeLengthDelimited(stream, instance.metabolism, isDelta);
				}
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				instance.loadingTimeout = ProtocolParser.ReadSingle(stream);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (instance.modelState == null)
				{
					instance.modelState = ModelState.DeserializeLengthDelimited(stream);
				}
				else
				{
					ModelState.DeserializeLengthDelimited(stream, instance.modelState, isDelta);
				}
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				instance.playerFlags = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				instance.heldEntity = new ItemId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 77:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				instance.health = ProtocolParser.ReadSingle(stream);
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (instance.persistantData == null)
				{
					instance.persistantData = PersistantPlayer.DeserializeLengthDelimited(stream);
				}
				else
				{
					PersistantPlayer.DeserializeLengthDelimited(stream, instance.persistantData, isDelta);
				}
				continue;
			case 125:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				instance.skinCol = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Fixed32)
				{
					instance.skinTex = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 17u:
				stream.ValidateFieldOrder(ref lastFieldId, 17u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Fixed32)
				{
					instance.skinMesh = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 20u:
				stream.ValidateFieldOrder(ref lastFieldId, 20u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.currentLife == null)
					{
						instance.currentLife = PlayerLifeStory.DeserializeLengthDelimited(stream);
					}
					else
					{
						PlayerLifeStory.DeserializeLengthDelimited(stream, instance.currentLife, isDelta);
					}
				}
				break;
			case 21u:
				stream.ValidateFieldOrder(ref lastFieldId, 21u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.previousLife == null)
					{
						instance.previousLife = PlayerLifeStory.DeserializeLengthDelimited(stream);
					}
					else
					{
						PlayerLifeStory.DeserializeLengthDelimited(stream, instance.previousLife, isDelta);
					}
				}
				break;
			case 22u:
				stream.ValidateFieldOrder(ref lastFieldId, 22u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.mounted = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				}
				break;
			case 23u:
				stream.ValidateFieldOrder(ref lastFieldId, 23u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.currentTeam = ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 24u:
				stream.ValidateFieldOrder(ref lastFieldId, 24u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.underwear = ProtocolParser.ReadUInt32(stream);
				}
				break;
			case 25u:
				stream.ValidateFieldOrder(ref lastFieldId, 25u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.modifiers == null)
					{
						instance.modifiers = PlayerModifiers.DeserializeLengthDelimited(stream);
					}
					else
					{
						PlayerModifiers.DeserializeLengthDelimited(stream, instance.modifiers, isDelta);
					}
				}
				break;
			case 26u:
				stream.ValidateFieldOrder(ref lastFieldId, 26u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.reputation = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 27u:
				stream.ValidateFieldOrder(ref lastFieldId, 27u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.loopingGesture = ProtocolParser.ReadUInt32(stream);
				}
				break;
			case 28u:
				stream.ValidateFieldOrder(ref lastFieldId, 28u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.missions == null)
					{
						instance.missions = Missions.DeserializeLengthDelimited(stream);
					}
					else
					{
						Missions.DeserializeLengthDelimited(stream, instance.missions, isDelta);
					}
				}
				break;
			case 29u:
				stream.ValidateFieldOrder(ref lastFieldId, 29u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.LengthDelimited)
				{
					instance.respawnId = ProtocolParser.ReadString(stream);
				}
				break;
			case 30u:
				stream.ValidateFieldOrder(ref lastFieldId, 30u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.bagCount = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 31u:
				stream.ValidateFieldOrder(ref lastFieldId, 31u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.clanId = (long)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 32u:
				stream.ValidateFieldOrder(ref lastFieldId, 32u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.itemCrafter == null)
					{
						instance.itemCrafter = ItemCrafter.DeserializeLengthDelimited(stream);
					}
					else
					{
						ItemCrafter.DeserializeLengthDelimited(stream, instance.itemCrafter, isDelta);
					}
				}
				break;
			case 33u:
				stream.ValidateFieldOrder(ref lastFieldId, 33u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.shelterCount = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 34u:
				stream.ValidateFieldOrder(ref lastFieldId, 34u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.tutorialAllowance = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 35u:
				stream.ValidateFieldOrder(ref lastFieldId, 35u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.paintballColor = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 36u:
				stream.ValidateFieldOrder(ref lastFieldId, 36u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.bbsCount = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 40u:
				stream.ValidateFieldOrder(ref lastFieldId, 40u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Fixed32)
				{
					instance.mortarCooldown = ProtocolParser.ReadSingle(stream);
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static BasePlayer DeserializeLength(BufferStream stream, int length, BasePlayer instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.BasePlayer");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				instance.name = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				instance.userid = ProtocolParser.ReadUInt64(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (instance.inventory == null)
				{
					instance.inventory = PlayerInventory.DeserializeLengthDelimited(stream);
				}
				else
				{
					PlayerInventory.DeserializeLengthDelimited(stream, instance.inventory, isDelta);
				}
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (instance.metabolism == null)
				{
					instance.metabolism = PlayerMetabolism.DeserializeLengthDelimited(stream);
				}
				else
				{
					PlayerMetabolism.DeserializeLengthDelimited(stream, instance.metabolism, isDelta);
				}
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				instance.loadingTimeout = ProtocolParser.ReadSingle(stream);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (instance.modelState == null)
				{
					instance.modelState = ModelState.DeserializeLengthDelimited(stream);
				}
				else
				{
					ModelState.DeserializeLengthDelimited(stream, instance.modelState, isDelta);
				}
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				instance.playerFlags = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				instance.heldEntity = new ItemId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 77:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				instance.health = ProtocolParser.ReadSingle(stream);
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (instance.persistantData == null)
				{
					instance.persistantData = PersistantPlayer.DeserializeLengthDelimited(stream);
				}
				else
				{
					PersistantPlayer.DeserializeLengthDelimited(stream, instance.persistantData, isDelta);
				}
				continue;
			case 125:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				instance.skinCol = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Fixed32)
				{
					instance.skinTex = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 17u:
				stream.ValidateFieldOrder(ref lastFieldId, 17u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Fixed32)
				{
					instance.skinMesh = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 20u:
				stream.ValidateFieldOrder(ref lastFieldId, 20u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.currentLife == null)
					{
						instance.currentLife = PlayerLifeStory.DeserializeLengthDelimited(stream);
					}
					else
					{
						PlayerLifeStory.DeserializeLengthDelimited(stream, instance.currentLife, isDelta);
					}
				}
				break;
			case 21u:
				stream.ValidateFieldOrder(ref lastFieldId, 21u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.previousLife == null)
					{
						instance.previousLife = PlayerLifeStory.DeserializeLengthDelimited(stream);
					}
					else
					{
						PlayerLifeStory.DeserializeLengthDelimited(stream, instance.previousLife, isDelta);
					}
				}
				break;
			case 22u:
				stream.ValidateFieldOrder(ref lastFieldId, 22u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.mounted = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				}
				break;
			case 23u:
				stream.ValidateFieldOrder(ref lastFieldId, 23u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.currentTeam = ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 24u:
				stream.ValidateFieldOrder(ref lastFieldId, 24u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.underwear = ProtocolParser.ReadUInt32(stream);
				}
				break;
			case 25u:
				stream.ValidateFieldOrder(ref lastFieldId, 25u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.modifiers == null)
					{
						instance.modifiers = PlayerModifiers.DeserializeLengthDelimited(stream);
					}
					else
					{
						PlayerModifiers.DeserializeLengthDelimited(stream, instance.modifiers, isDelta);
					}
				}
				break;
			case 26u:
				stream.ValidateFieldOrder(ref lastFieldId, 26u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.reputation = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 27u:
				stream.ValidateFieldOrder(ref lastFieldId, 27u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.loopingGesture = ProtocolParser.ReadUInt32(stream);
				}
				break;
			case 28u:
				stream.ValidateFieldOrder(ref lastFieldId, 28u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.missions == null)
					{
						instance.missions = Missions.DeserializeLengthDelimited(stream);
					}
					else
					{
						Missions.DeserializeLengthDelimited(stream, instance.missions, isDelta);
					}
				}
				break;
			case 29u:
				stream.ValidateFieldOrder(ref lastFieldId, 29u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.LengthDelimited)
				{
					instance.respawnId = ProtocolParser.ReadString(stream);
				}
				break;
			case 30u:
				stream.ValidateFieldOrder(ref lastFieldId, 30u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.bagCount = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 31u:
				stream.ValidateFieldOrder(ref lastFieldId, 31u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.clanId = (long)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 32u:
				stream.ValidateFieldOrder(ref lastFieldId, 32u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.itemCrafter == null)
					{
						instance.itemCrafter = ItemCrafter.DeserializeLengthDelimited(stream);
					}
					else
					{
						ItemCrafter.DeserializeLengthDelimited(stream, instance.itemCrafter, isDelta);
					}
				}
				break;
			case 33u:
				stream.ValidateFieldOrder(ref lastFieldId, 33u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.shelterCount = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 34u:
				stream.ValidateFieldOrder(ref lastFieldId, 34u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.tutorialAllowance = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 35u:
				stream.ValidateFieldOrder(ref lastFieldId, 35u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.paintballColor = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 36u:
				stream.ValidateFieldOrder(ref lastFieldId, 36u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Varint)
				{
					instance.bbsCount = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 40u:
				stream.ValidateFieldOrder(ref lastFieldId, 40u, fieldIsRepeated: false, "ProtoBuf.BasePlayer");
				if (key.WireType == Wire.Fixed32)
				{
					instance.mortarCooldown = ProtocolParser.ReadSingle(stream);
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BasePlayer");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, BasePlayer instance, BasePlayer previous)
	{
		if (instance.name != null && instance.name != previous.name)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.name);
		}
		if (instance.userid != previous.userid)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, instance.userid);
		}
		if (instance.inventory != null)
		{
			stream.WriteByte(26);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			PlayerInventory.SerializeDelta(stream, instance.inventory, previous.inventory);
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
		if (instance.metabolism != null)
		{
			stream.WriteByte(34);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			PlayerMetabolism.SerializeDelta(stream, instance.metabolism, previous.metabolism);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field metabolism (ProtoBuf.PlayerMetabolism)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.loadingTimeout != previous.loadingTimeout)
		{
			stream.WriteByte(45);
			ProtocolParser.WriteSingle(stream, instance.loadingTimeout);
		}
		if (instance.modelState != null)
		{
			stream.WriteByte(50);
			BufferStream.RangeHandle range3 = stream.GetRange(2);
			int position3 = stream.Position;
			ModelState.SerializeDelta(stream, instance.modelState, previous.modelState);
			int num3 = stream.Position - position3;
			if (num3 > 16383)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field modelState (global::ModelState)");
			}
			Span<byte> span3 = range3.GetSpan();
			if (ProtocolParser.WriteUInt32((uint)num3, span3, 0) < 2)
			{
				span3[0] |= 128;
				span3[1] = 0;
			}
		}
		if (instance.playerFlags != previous.playerFlags)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.playerFlags);
		}
		stream.WriteByte(64);
		ProtocolParser.WriteUInt64(stream, instance.heldEntity.Value);
		if (instance.health != previous.health)
		{
			stream.WriteByte(77);
			ProtocolParser.WriteSingle(stream, instance.health);
		}
		if (instance.persistantData != null)
		{
			stream.WriteByte(82);
			BufferStream.RangeHandle range4 = stream.GetRange(3);
			int position4 = stream.Position;
			PersistantPlayer.SerializeDelta(stream, instance.persistantData, previous.persistantData);
			int num4 = stream.Position - position4;
			if (num4 > 2097151)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field persistantData (ProtoBuf.PersistantPlayer)");
			}
			Span<byte> span4 = range4.GetSpan();
			int num5 = ProtocolParser.WriteUInt32((uint)num4, span4, 0);
			if (num5 < 3)
			{
				span4[num5 - 1] |= 128;
				while (num5 < 2)
				{
					span4[num5++] = 128;
				}
				span4[2] = 0;
			}
		}
		if (instance.skinCol != previous.skinCol)
		{
			stream.WriteByte(125);
			ProtocolParser.WriteSingle(stream, instance.skinCol);
		}
		if (instance.skinTex != previous.skinTex)
		{
			stream.WriteByte(133);
			stream.WriteByte(1);
			ProtocolParser.WriteSingle(stream, instance.skinTex);
		}
		if (instance.skinMesh != previous.skinMesh)
		{
			stream.WriteByte(141);
			stream.WriteByte(1);
			ProtocolParser.WriteSingle(stream, instance.skinMesh);
		}
		if (instance.currentLife != null)
		{
			stream.WriteByte(162);
			stream.WriteByte(1);
			BufferStream.RangeHandle range5 = stream.GetRange(5);
			int position5 = stream.Position;
			PlayerLifeStory.SerializeDelta(stream, instance.currentLife, previous.currentLife);
			int val2 = stream.Position - position5;
			Span<byte> span5 = range5.GetSpan();
			int num6 = ProtocolParser.WriteUInt32((uint)val2, span5, 0);
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
		if (instance.previousLife != null)
		{
			stream.WriteByte(170);
			stream.WriteByte(1);
			BufferStream.RangeHandle range6 = stream.GetRange(5);
			int position6 = stream.Position;
			PlayerLifeStory.SerializeDelta(stream, instance.previousLife, previous.previousLife);
			int val3 = stream.Position - position6;
			Span<byte> span6 = range6.GetSpan();
			int num7 = ProtocolParser.WriteUInt32((uint)val3, span6, 0);
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
		stream.WriteByte(176);
		stream.WriteByte(1);
		ProtocolParser.WriteUInt64(stream, instance.mounted.Value);
		if (instance.currentTeam != previous.currentTeam)
		{
			stream.WriteByte(184);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, instance.currentTeam);
		}
		if (instance.underwear != previous.underwear)
		{
			stream.WriteByte(192);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt32(stream, instance.underwear);
		}
		if (instance.modifiers != null)
		{
			stream.WriteByte(202);
			stream.WriteByte(1);
			BufferStream.RangeHandle range7 = stream.GetRange(3);
			int position7 = stream.Position;
			PlayerModifiers.SerializeDelta(stream, instance.modifiers, previous.modifiers);
			int num8 = stream.Position - position7;
			if (num8 > 2097151)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field modifiers (ProtoBuf.PlayerModifiers)");
			}
			Span<byte> span7 = range7.GetSpan();
			int num9 = ProtocolParser.WriteUInt32((uint)num8, span7, 0);
			if (num9 < 3)
			{
				span7[num9 - 1] |= 128;
				while (num9 < 2)
				{
					span7[num9++] = 128;
				}
				span7[2] = 0;
			}
		}
		if (instance.reputation != previous.reputation)
		{
			stream.WriteByte(208);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.reputation);
		}
		if (instance.loopingGesture != previous.loopingGesture)
		{
			stream.WriteByte(216);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt32(stream, instance.loopingGesture);
		}
		if (instance.missions != null)
		{
			stream.WriteByte(226);
			stream.WriteByte(1);
			BufferStream.RangeHandle range8 = stream.GetRange(5);
			int position8 = stream.Position;
			Missions.SerializeDelta(stream, instance.missions, previous.missions);
			int val4 = stream.Position - position8;
			Span<byte> span8 = range8.GetSpan();
			int num10 = ProtocolParser.WriteUInt32((uint)val4, span8, 0);
			if (num10 < 5)
			{
				span8[num10 - 1] |= 128;
				while (num10 < 4)
				{
					span8[num10++] = 128;
				}
				span8[4] = 0;
			}
		}
		if (instance.respawnId != null && instance.respawnId != previous.respawnId)
		{
			stream.WriteByte(234);
			stream.WriteByte(1);
			ProtocolParser.WriteString(stream, instance.respawnId);
		}
		if (instance.bagCount != previous.bagCount)
		{
			stream.WriteByte(240);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.bagCount);
		}
		stream.WriteByte(248);
		stream.WriteByte(1);
		ProtocolParser.WriteUInt64(stream, (ulong)instance.clanId);
		if (instance.itemCrafter != null)
		{
			stream.WriteByte(130);
			stream.WriteByte(2);
			BufferStream.RangeHandle range9 = stream.GetRange(5);
			int position9 = stream.Position;
			ItemCrafter.SerializeDelta(stream, instance.itemCrafter, previous.itemCrafter);
			int val5 = stream.Position - position9;
			Span<byte> span9 = range9.GetSpan();
			int num11 = ProtocolParser.WriteUInt32((uint)val5, span9, 0);
			if (num11 < 5)
			{
				span9[num11 - 1] |= 128;
				while (num11 < 4)
				{
					span9[num11++] = 128;
				}
				span9[4] = 0;
			}
		}
		if (instance.shelterCount != previous.shelterCount)
		{
			stream.WriteByte(136);
			stream.WriteByte(2);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.shelterCount);
		}
		if (instance.tutorialAllowance != previous.tutorialAllowance)
		{
			stream.WriteByte(144);
			stream.WriteByte(2);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.tutorialAllowance);
		}
		if (instance.paintballColor != previous.paintballColor)
		{
			stream.WriteByte(152);
			stream.WriteByte(2);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.paintballColor);
		}
		if (instance.bbsCount != previous.bbsCount)
		{
			stream.WriteByte(160);
			stream.WriteByte(2);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.bbsCount);
		}
		if (instance.mortarCooldown != previous.mortarCooldown)
		{
			stream.WriteByte(197);
			stream.WriteByte(2);
			ProtocolParser.WriteSingle(stream, instance.mortarCooldown);
		}
	}

	public static void Serialize(BufferStream stream, BasePlayer instance)
	{
		if (instance.name != null)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.name);
		}
		if (instance.userid != 0L)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, instance.userid);
		}
		if (instance.inventory != null)
		{
			stream.WriteByte(26);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			PlayerInventory.Serialize(stream, instance.inventory);
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
		if (instance.metabolism != null)
		{
			stream.WriteByte(34);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			PlayerMetabolism.Serialize(stream, instance.metabolism);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field metabolism (ProtoBuf.PlayerMetabolism)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.loadingTimeout != 0f)
		{
			stream.WriteByte(45);
			ProtocolParser.WriteSingle(stream, instance.loadingTimeout);
		}
		if (instance.modelState != null)
		{
			stream.WriteByte(50);
			BufferStream.RangeHandle range3 = stream.GetRange(2);
			int position3 = stream.Position;
			ModelState.Serialize(stream, instance.modelState);
			int num3 = stream.Position - position3;
			if (num3 > 16383)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field modelState (global::ModelState)");
			}
			Span<byte> span3 = range3.GetSpan();
			if (ProtocolParser.WriteUInt32((uint)num3, span3, 0) < 2)
			{
				span3[0] |= 128;
				span3[1] = 0;
			}
		}
		if (instance.playerFlags != 0)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.playerFlags);
		}
		if (instance.heldEntity != default(ItemId))
		{
			stream.WriteByte(64);
			ProtocolParser.WriteUInt64(stream, instance.heldEntity.Value);
		}
		if (instance.health != 0f)
		{
			stream.WriteByte(77);
			ProtocolParser.WriteSingle(stream, instance.health);
		}
		if (instance.persistantData != null)
		{
			stream.WriteByte(82);
			BufferStream.RangeHandle range4 = stream.GetRange(3);
			int position4 = stream.Position;
			PersistantPlayer.Serialize(stream, instance.persistantData);
			int num4 = stream.Position - position4;
			if (num4 > 2097151)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field persistantData (ProtoBuf.PersistantPlayer)");
			}
			Span<byte> span4 = range4.GetSpan();
			int num5 = ProtocolParser.WriteUInt32((uint)num4, span4, 0);
			if (num5 < 3)
			{
				span4[num5 - 1] |= 128;
				while (num5 < 2)
				{
					span4[num5++] = 128;
				}
				span4[2] = 0;
			}
		}
		if (instance.skinCol != 0f)
		{
			stream.WriteByte(125);
			ProtocolParser.WriteSingle(stream, instance.skinCol);
		}
		if (instance.skinTex != 0f)
		{
			stream.WriteByte(133);
			stream.WriteByte(1);
			ProtocolParser.WriteSingle(stream, instance.skinTex);
		}
		if (instance.skinMesh != 0f)
		{
			stream.WriteByte(141);
			stream.WriteByte(1);
			ProtocolParser.WriteSingle(stream, instance.skinMesh);
		}
		if (instance.currentLife != null)
		{
			stream.WriteByte(162);
			stream.WriteByte(1);
			BufferStream.RangeHandle range5 = stream.GetRange(5);
			int position5 = stream.Position;
			PlayerLifeStory.Serialize(stream, instance.currentLife);
			int val2 = stream.Position - position5;
			Span<byte> span5 = range5.GetSpan();
			int num6 = ProtocolParser.WriteUInt32((uint)val2, span5, 0);
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
		if (instance.previousLife != null)
		{
			stream.WriteByte(170);
			stream.WriteByte(1);
			BufferStream.RangeHandle range6 = stream.GetRange(5);
			int position6 = stream.Position;
			PlayerLifeStory.Serialize(stream, instance.previousLife);
			int val3 = stream.Position - position6;
			Span<byte> span6 = range6.GetSpan();
			int num7 = ProtocolParser.WriteUInt32((uint)val3, span6, 0);
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
		if (instance.mounted != default(NetworkableId))
		{
			stream.WriteByte(176);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, instance.mounted.Value);
		}
		if (instance.currentTeam != 0L)
		{
			stream.WriteByte(184);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, instance.currentTeam);
		}
		if (instance.underwear != 0)
		{
			stream.WriteByte(192);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt32(stream, instance.underwear);
		}
		if (instance.modifiers != null)
		{
			stream.WriteByte(202);
			stream.WriteByte(1);
			BufferStream.RangeHandle range7 = stream.GetRange(3);
			int position7 = stream.Position;
			PlayerModifiers.Serialize(stream, instance.modifiers);
			int num8 = stream.Position - position7;
			if (num8 > 2097151)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field modifiers (ProtoBuf.PlayerModifiers)");
			}
			Span<byte> span7 = range7.GetSpan();
			int num9 = ProtocolParser.WriteUInt32((uint)num8, span7, 0);
			if (num9 < 3)
			{
				span7[num9 - 1] |= 128;
				while (num9 < 2)
				{
					span7[num9++] = 128;
				}
				span7[2] = 0;
			}
		}
		if (instance.reputation != 0)
		{
			stream.WriteByte(208);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.reputation);
		}
		if (instance.loopingGesture != 0)
		{
			stream.WriteByte(216);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt32(stream, instance.loopingGesture);
		}
		if (instance.missions != null)
		{
			stream.WriteByte(226);
			stream.WriteByte(1);
			BufferStream.RangeHandle range8 = stream.GetRange(5);
			int position8 = stream.Position;
			Missions.Serialize(stream, instance.missions);
			int val4 = stream.Position - position8;
			Span<byte> span8 = range8.GetSpan();
			int num10 = ProtocolParser.WriteUInt32((uint)val4, span8, 0);
			if (num10 < 5)
			{
				span8[num10 - 1] |= 128;
				while (num10 < 4)
				{
					span8[num10++] = 128;
				}
				span8[4] = 0;
			}
		}
		if (instance.respawnId != null)
		{
			stream.WriteByte(234);
			stream.WriteByte(1);
			ProtocolParser.WriteString(stream, instance.respawnId);
		}
		if (instance.bagCount != 0)
		{
			stream.WriteByte(240);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.bagCount);
		}
		if (instance.clanId != 0L)
		{
			stream.WriteByte(248);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.clanId);
		}
		if (instance.itemCrafter != null)
		{
			stream.WriteByte(130);
			stream.WriteByte(2);
			BufferStream.RangeHandle range9 = stream.GetRange(5);
			int position9 = stream.Position;
			ItemCrafter.Serialize(stream, instance.itemCrafter);
			int val5 = stream.Position - position9;
			Span<byte> span9 = range9.GetSpan();
			int num11 = ProtocolParser.WriteUInt32((uint)val5, span9, 0);
			if (num11 < 5)
			{
				span9[num11 - 1] |= 128;
				while (num11 < 4)
				{
					span9[num11++] = 128;
				}
				span9[4] = 0;
			}
		}
		if (instance.shelterCount != 0)
		{
			stream.WriteByte(136);
			stream.WriteByte(2);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.shelterCount);
		}
		if (instance.tutorialAllowance != 0)
		{
			stream.WriteByte(144);
			stream.WriteByte(2);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.tutorialAllowance);
		}
		if (instance.paintballColor != 0)
		{
			stream.WriteByte(152);
			stream.WriteByte(2);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.paintballColor);
		}
		if (instance.bbsCount != 0)
		{
			stream.WriteByte(160);
			stream.WriteByte(2);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.bbsCount);
		}
		if (instance.mortarCooldown != 0f)
		{
			stream.WriteByte(197);
			stream.WriteByte(2);
			ProtocolParser.WriteSingle(stream, instance.mortarCooldown);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		inventory?.InspectUids(action);
		metabolism?.InspectUids(action);
		modelState?.InspectUids(action);
		action(UidType.ItemId, ref heldEntity.Value);
		persistantData?.InspectUids(action);
		currentLife?.InspectUids(action);
		previousLife?.InspectUids(action);
		action(UidType.NetworkableId, ref mounted.Value);
		modifiers?.InspectUids(action);
		missions?.InspectUids(action);
		itemCrafter?.InspectUids(action);
	}
}
