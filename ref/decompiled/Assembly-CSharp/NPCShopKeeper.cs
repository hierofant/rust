using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;

public class NPCShopKeeper : NPCPlayer
{
	public enum ShopkeeperEvent
	{
		SaleSuccess,
		EndConvoBoughtNothing,
		Talk,
		EndConvoBoughtSomething,
		Wave
	}

	public EntityRef invisibleVendingMachineRef;

	public InvisibleVendingMachine machine;

	public bool canBeHurt;

	public ChildAnimatorSubSystem ChildAnimator;

	public int TotalNoAnimations = 2;

	public int TotalYesAnimations = 2;

	public int TotalGreetAnimations = 2;

	public int TotalByeAnimations = 2;

	public int TotalWaveAnimations = 4;

	public float greetDir;

	public Vector3 initialFacingDir;

	public BasePlayer lastWavedAtPlayer;

	protected override string OverrideCorpseName => "Shopkeeper";

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("NPCShopKeeper.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public InvisibleVendingMachine GetVendingMachine()
	{
		if (!invisibleVendingMachineRef.IsValid(base.isServer))
		{
			return null;
		}
		return invisibleVendingMachineRef.Get(base.isServer).GetComponent<InvisibleVendingMachine>();
	}

	public override void UpdateProtectionFromClothing()
	{
	}

	protected override bool AllowRagdoll()
	{
		return canBeHurt;
	}

	public override void OnDied(HitInfo info)
	{
		base.OnDied(info);
		if (invisibleVendingMachineRef.Get(base.isServer) != null && invisibleVendingMachineRef.Get(base.isServer) is InvisibleVendingMachine invisibleVendingMachine)
		{
			invisibleVendingMachine.SetAttachedNPC(null);
		}
	}

	public override void Hurt(HitInfo info)
	{
		if (canBeHurt)
		{
			base.Hurt(info);
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		Quaternion quaternion = base.transform.rotation;
		if (GetParentEntity() != null)
		{
			quaternion = base.transform.localRotation;
		}
		initialFacingDir = quaternion * Vector3.forward;
		Invoke(DelayedSleepEnd, 3f);
		SetAimDirection(quaternion * Vector3.forward);
		InvokeRandomized(Greeting, Random.Range(5f, 10f), 5f, Random.Range(0f, 2f));
	}

	public override void PostInitShared()
	{
		base.PostInitShared();
		if (base.isServer)
		{
			if (machine == null)
			{
				machine = InvisibleVendingMachine.GetMachineAtPosition(1f, base.transform.position);
			}
			if (invisibleVendingMachineRef.IsValid(serverside: true) && machine == null)
			{
				machine = GetVendingMachine();
				machine.SetAttachedNPC(this);
			}
			else if (machine != null && !invisibleVendingMachineRef.IsValid(serverside: true))
			{
				invisibleVendingMachineRef.Set(machine);
				machine.SetAttachedNPC(this);
			}
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.shopKeeper = Pool.Get<ShopKeeper>();
		info.msg.shopKeeper.vendingRef = invisibleVendingMachineRef.uid;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.shopKeeper != null)
		{
			invisibleVendingMachineRef.uid = info.msg.shopKeeper.vendingRef;
		}
	}

	public void DelayedSleepEnd()
	{
		EndSleeping();
	}

	public virtual void Greeting()
	{
		if (base.eyes == null)
		{
			return;
		}
		using (TimeWarning.New("WaveCheck"))
		{
			if (!BaseNetworkable.HasCloseConnections(net.group, base.transform.position, 10f))
			{
				return;
			}
			using PooledList<BasePlayer> pooledList = Pool.Get<PooledList<BasePlayer>>();
			foreach (Connection subscriber in net.group.subscribers)
			{
				if (subscriber.player is BasePlayer basePlayer && basePlayer.Distance(this) <= 10f)
				{
					pooledList.Add(basePlayer);
				}
			}
			BasePlayer basePlayer2 = null;
			foreach (BasePlayer item in pooledList)
			{
				if (!item.isClient && !item.IsNpc && !(item == this) && item.IsVisible(base.eyes.position) && !(item == lastWavedAtPlayer) && !(Vector3.Dot(Vector3Ex.Direction2D(item.eyes.position, base.eyes.position), initialFacingDir) < 0.2f))
				{
					basePlayer2 = item;
					break;
				}
			}
			if (basePlayer2 == null && !pooledList.Contains(lastWavedAtPlayer))
			{
				lastWavedAtPlayer = null;
			}
			if (basePlayer2 != null)
			{
				ClientRPC(RpcTarget.NetworkGroup("ClientNotifyShopEvent"), 4);
				SetAimDirection(Vector3Ex.Direction2D(basePlayer2.eyes.position, base.eyes.position));
				lastWavedAtPlayer = basePlayer2;
			}
			else
			{
				SetAimDirection(initialFacingDir);
			}
		}
	}

	public void NotifyEvent(ShopkeeperEvent shopEvent)
	{
		ClientRPC(RpcTarget.NetworkGroup("ClientNotifyShopEvent"), (int)shopEvent);
	}
}
