using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using ProtoBuf;
using UnityEngine;

public class ConstructableEntity : StorageContainer
{
	[Header("Constructable Entity")]
	public List<ItemAmount> ingredients = new List<ItemAmount>();

	private int[] currentMaterials;

	public GameObjectRef entityToSpawn;

	public PlayerDetectionTrigger trigger;

	public bool onlyRunTriggerCheckOnLastHit;

	public bool onlyBlockWhenStanding;

	public GameObject[] states;

	public SoundDefinition[] stateSounds;

	public GameObjectRef spawnEffect;

	public bool additiveMode;

	private int currentState;

	public override bool ValidateMeleeColliderAntihack => false;

	private void SetState(int index)
	{
		if (index < 0 || index >= states.Length)
		{
			return;
		}
		if (additiveMode)
		{
			for (int i = 0; i < states.Length; i++)
			{
				states[i].SetActive(i <= index);
			}
		}
		else
		{
			GameObject[] array = states;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].SetActive(value: false);
			}
			states[index].SetActive(value: true);
		}
		currentState = index;
	}

	private void UpdateState()
	{
		int value = Mathf.FloorToInt(GetTotalMaterialFraction() * (float)states.Length);
		value = Mathf.Clamp(value, 0, states.Length - 1);
		if (value != currentState)
		{
			if (base.isServer)
			{
				timePlaced = GetNetworkTime();
			}
			SetState(value);
		}
	}

	private float GetTotalMaterialFraction()
	{
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < ingredients.Count; i++)
		{
			num += Mathf.Min(currentMaterials[i], ingredients[i].amount);
			num2 += ingredients[i].amount;
		}
		if (!(num2 > 0f))
		{
			return 0f;
		}
		return num / num2;
	}

	public bool IsNearlyBuilt()
	{
		for (int i = 0; i < ingredients.Count; i++)
		{
			float num = Mathf.Max(0f, ingredients[i].amount - (float)currentMaterials[i]);
			if (!(num <= 0f))
			{
				int num2 = Mathf.CeilToInt(ingredients[i].amount / 10f);
				if (num > (float)num2)
				{
					return false;
				}
			}
		}
		return true;
	}

	public override void ServerInit()
	{
		SetState(0);
		base.ServerInit();
	}

	public override void OnRepairFinished(BasePlayer player)
	{
		base.OnRepairFinished(player);
		ulong ownerID = base.OwnerID;
		Kill();
		BaseEntity baseEntity = GameManager.server.CreateEntity(entityToSpawn.resourcePath, base.transform.position, base.transform.rotation);
		baseEntity.OwnerID = ownerID;
		baseEntity.Spawn();
		if (spawnEffect.isValid)
		{
			Effect.server.Run(spawnEffect.resourcePath, base.transform.position, Vector3.up);
		}
	}

	public override float GetDamageRepairCooldown()
	{
		return 5f;
	}

	public override void DoRepair(BasePlayer player)
	{
		if (!CanRepair(player))
		{
			return;
		}
		bool flag = false;
		float num = 0f;
		for (int i = 0; i < ingredients.Count; i++)
		{
			ItemAmount itemAmount = ingredients[i];
			float num2 = Mathf.Max(0f, itemAmount.amount - (float)currentMaterials[i]);
			if (num2 != 0f)
			{
				int num3 = Mathf.CeilToInt(itemAmount.amount / 10f);
				int num4 = player.inventory.GetAmount(itemAmount.itemid);
				if (player.IsInCreativeMode && Creative.freeRepair)
				{
					num4 = int.MaxValue;
				}
				int num5 = Mathf.Min(num3, num4, Mathf.FloorToInt(num2));
				if (num5 > 0)
				{
					currentMaterials[i] += num5;
					player.inventory.Take(null, itemAmount.itemid, num5);
					player.Command("note.inv", itemAmount.itemid, num5 * -1);
					flag = true;
					float num6 = MaxHealth() * (itemAmount.amount / GetTotalRequiredMaterials());
					num += num6 * ((float)num5 / itemAmount.amount);
				}
				else
				{
					OnBuildFailedResources(player, itemAmount.itemDef.itemid);
				}
			}
		}
		if (num != 0f)
		{
			SetHealth(Mathf.Min(Health() + num, MaxHealth()));
		}
		if (flag)
		{
			SendNetworkUpdate();
			UpdateState();
			if (IsFullyBuilt())
			{
				OnRepairFinished(player);
			}
			else
			{
				OnRepair();
			}
		}
	}

	public void OnBuildFailedResources(BasePlayer player, int itemID)
	{
		if (player != null)
		{
			player.ClientRPC(RpcTarget.Player("Client_OnConstructableBuildFailed", player), itemID);
		}
	}

	private bool IsFullyBuilt()
	{
		for (int i = 0; i < ingredients.Count; i++)
		{
			if ((float)currentMaterials[i] < ingredients[i].amount)
			{
				return false;
			}
		}
		return true;
	}

	private float GetTotalRequiredMaterials()
	{
		float num = 0f;
		for (int i = 0; i < ingredients.Count; i++)
		{
			num += ingredients[i].amount;
		}
		return num;
	}

	protected virtual bool CanRepair(BasePlayer player)
	{
		if (trigger != null && trigger.entityContents != null)
		{
			if (onlyRunTriggerCheckOnLastHit && !IsNearlyBuilt())
			{
				return true;
			}
			foreach (BaseEntity entityContent in trigger.entityContents)
			{
				if (entityContent is BaseVehicle baseVehicle)
				{
					if (!baseVehicle.IsDead() && !baseVehicle.isClient && !(baseVehicle.transform.root == base.transform))
					{
						player.ShowToast(GameTip.Styles.Error, ConstructionErrors.BlockedByVehicle, false);
						return false;
					}
				}
				else if (entityContent is BasePlayer { isClient: false } basePlayer && !basePlayer.IsDead() && (!onlyBlockWhenStanding || basePlayer.IsStandingOnEntity(this, 134226176)))
				{
					player.ShowToast(GameTip.Styles.Error, ConstructionErrors.BlockedByPlayer, overlay: false, (string[])null);
					return false;
				}
			}
		}
		return true;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.constructableEntity = Facepunch.Pool.Get<ProtoBuf.ConstructableEntity>();
		if (currentMaterials == null || CollectionEx.IsEmpty(currentMaterials))
		{
			currentMaterials = new int[ingredients.Count];
			for (int i = 0; i < ingredients.Count; i++)
			{
				currentMaterials[i] = 0;
			}
		}
		info.msg.constructableEntity.addedResources = currentMaterials.ToList();
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.constructableEntity != null)
		{
			currentMaterials = info.msg.constructableEntity.addedResources.ToArray();
		}
		UpdateState();
	}
}
