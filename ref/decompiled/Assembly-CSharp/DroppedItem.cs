using System;
using System.Linq;
using ConVar;
using Facepunch.Rust;
using Oxide.Core;
using UnityEngine;

public class DroppedItem : WorldItem, IContainerSounds, Hopper.IHopperTarget
{
	public enum DropReasonEnum
	{
		Unknown,
		Player,
		Death,
		Loot
	}

	public class DroppedItemUnderwaterQueue : PersistentObjectWorkQueue<DroppedItem>
	{
		protected override void RunJob(DroppedItem entity)
		{
			if (entity != null)
			{
				entity.CheckUnderwaterStatus(canSplash: true);
			}
		}
	}

	[Header("DroppedItem")]
	public GameObjectRef itemModel;

	public GameObjectRef splashEffect;

	[ServerVar(Help = "How many milliseconds to spend on updating underwater drag levels")]
	public static float underwater_drag_budget_ms = 0.05f;

	[ServerVar(Help = "Whether Rigidbody components are removed from DroppedItems when sleeping")]
	public static bool remove_rb_on_sleep = false;

	[ServerVar(Help = "Will broadcast debug ddraw information on ALL dropped items to ALL players")]
	public static bool broadcast_debug_ddraw = false;

	private const Flags FLAG_STUCK = Flags.Reserved1;

	private const Flags FLAG_UNDERWATER = Flags.Reserved2;

	public const Flags FLAG_HOPPERANIMATING = Flags.Reserved3;

	private int originalLayer = -1;

	[NonSerialized]
	public DropReasonEnum DropReason;

	[NonSerialized]
	public ulong DroppedBy;

	[NonSerialized]
	public DateTime DroppedTime;

	[NonSerialized]
	public bool NeverCombine;

	private Rigidbody rB;

	private CollisionDetectionMode originalCollisionMode;

	private Vector3 prevLocalPos;

	private const float SLEEP_CHECK_FREQUENCY = 11f;

	private const float ANGULAR_DRAG = 0.1f;

	private const float AIR_DRAG = 0.1f;

	private const float UNDERWATER_DRAG = 7f;

	private bool hasLastPos;

	private bool hadParent;

	private EntityRef parentRef;

	private Vector3 lastGoodColliderCentre;

	private Vector3 lastGoodPos;

	private Quaternion lastGoodRot;

	private Action cachedSleepCheck;

	private float maxBoundsExtent;

	private readonly Vector3 smallVerticalOffset = new Vector3(0f, 0.05f, 0f);

	public static DroppedItemUnderwaterQueue underwaterStatusQueue = new DroppedItemUnderwaterQueue();

	private TimeSince lastUnderwaterFlowImpulse;

	public Collider childCollider { get; private set; }

	private bool StuckInSomething => HasFlag(Flags.Reserved1);

	public SoundDefinition OpenSound
	{
		get
		{
			if (item == null)
			{
				return null;
			}
			ItemModContainer component = item.info.GetComponent<ItemModContainer>();
			if (component == null)
			{
				return null;
			}
			return component.openSound;
		}
	}

	public SoundDefinition CloseSound
	{
		get
		{
			if (item == null)
			{
				return null;
			}
			ItemModContainer component = item.info.GetComponent<ItemModContainer>();
			if (component == null)
			{
				return null;
			}
			return component.closeSound;
		}
	}

	public Rigidbody Rigidbody => rB;

	public int NumberOfItemsToTransfer => 1 + ((item.contents != null) ? item.contents.itemList.Count() : 0);

	public float EndPositionToleranceMultiplier => 1f;

	public bool IsSleeping
	{
		get
		{
			if (rB != null)
			{
				return rB.IsSleeping();
			}
			return false;
		}
	}

	public BaseEntity ToEntity => this;

	protected override bool CanBePickedUp => !HasFlag(Flags.Reserved3);

	public void TransferAllItemsToContainer(ItemContainer itemContainer, Vector3 itemFallbackPosition)
	{
		if (item == null || itemContainer == null)
		{
			return;
		}
		if (item.contents != null && item.IsBackpack())
		{
			int capacity = item.contents.capacity;
			for (int i = 0; i < capacity; i++)
			{
				if (item.contents != null)
				{
					Item slot = item.contents.GetSlot(i);
					if (slot != null && !slot.MoveToContainer(itemContainer))
					{
						slot.DropAndTossUpwards(itemFallbackPosition);
					}
				}
			}
		}
		if (item != null)
		{
			if (item.MoveToContainer(itemContainer))
			{
				RemoveItem();
			}
			else
			{
				CancelHopper();
			}
		}
	}

	public override float GetNetworkTime()
	{
		return UnityEngine.Time.fixedTime;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (GetDespawnDuration() < float.PositiveInfinity)
		{
			Invoke(IdleDestroy, GetDespawnDuration());
		}
		ReceiveCollisionMessages(b: true);
		prevLocalPos = base.transform.localPosition;
		underwaterStatusQueue.Add(this);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		underwaterStatusQueue.Remove(this);
	}

	public virtual float GetDespawnDuration()
	{
		return item?.GetDespawnDuration() ?? Server.itemdespawn;
	}

	public void IdleDestroy()
	{
		Interface.CallHook("OnItemDespawn", item);
		Facepunch.Rust.Analytics.Azure.OnItemDespawn(this, item, (int)DropReason, DroppedBy);
		if (item != null)
		{
			BuriedItems.Instance.Register(item, base.transform.position);
		}
		DestroyItem();
		Kill();
	}

	private void SetupRigidbody()
	{
		UpdateItemMass();
		rB.drag = 0.1f;
		rB.angularDrag = 0.1f;
		rB.interpolation = RigidbodyInterpolation.None;
		rB.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
		originalCollisionMode = rB.collisionDetectionMode;
		rB.sleepThreshold = Mathf.Max(0.05f, UnityEngine.Physics.sleepThreshold);
	}

	private Vector3 GetColliderCentre()
	{
		Vector3 vector = childCollider.bounds.center + smallVerticalOffset;
		if (HasParent())
		{
			vector = GetParentEntity().transform.worldToLocalMatrix.MultiplyPoint3x4(vector);
		}
		return vector;
	}

	public override void OnCollision(Collision collision, BaseEntity hitEntity)
	{
		if (item != null && item.MaxStackable() > 1)
		{
			DroppedItem droppedItem = hitEntity as DroppedItem;
			if (!(droppedItem == null) && droppedItem.item != null)
			{
				droppedItem.OnDroppedOn(this);
			}
		}
	}

	public void OnDroppedOn(DroppedItem di)
	{
		if (item == null || Interface.CallHook("CanCombineDroppedItem", this, di) != null || NeverCombine || di.NeverCombine || !item.CanStack(di.item))
		{
			return;
		}
		Interface.CallHook("OnDroppedItemCombined", this);
		int num = di.item.amount + item.amount;
		if (num <= item.MaxStackable() && num != 0)
		{
			if (di.DropReason == DropReasonEnum.Player)
			{
				DropReason = DropReasonEnum.Player;
			}
			di.item.MigrateItemOwnership(item, di.item.amount);
			di.DestroyItem();
			di.Kill();
			int worldModelIndex = item.info.GetWorldModelIndex(item.amount);
			item.amount = num;
			item.MarkDirty();
			if (GetDespawnDuration() < float.PositiveInfinity)
			{
				Invoke(IdleDestroy, GetDespawnDuration());
			}
			Effect.server.Run("assets/bundled/prefabs/fx/notice/stack.world.fx.prefab", this, 0u, Vector3.zero, Vector3.zero);
			int worldModelIndex2 = item.info.GetWorldModelIndex(item.amount);
			if (worldModelIndex != worldModelIndex2)
			{
				item.Drop(base.transform.position, Vector3.zero, base.transform.rotation);
			}
		}
	}

	public override void OnParentChanging(BaseEntity oldParent, BaseEntity newParent)
	{
		base.OnParentChanging(oldParent, newParent);
		hadParent = false;
		if (newParent != null && newParent != oldParent)
		{
			OnParented();
		}
		else if (newParent == null && oldParent != null)
		{
			OnUnparented();
		}
	}

	internal override void OnParentRemoved()
	{
		if (rB == null)
		{
			base.OnParentRemoved();
			return;
		}
		Vector3 position = base.transform.position;
		Quaternion rotation = base.transform.rotation;
		SetParent(null);
		if (UnityEngine.Physics.Raycast(position + Vector3.up * 2f, Vector3.down, out var hitInfo, 2f, 161546240) && position.y < hitInfo.point.y)
		{
			position += Vector3.up * 1.5f;
		}
		base.transform.position = position;
		base.transform.rotation = rotation;
		Unstick();
		if (GetDespawnDuration() < float.PositiveInfinity)
		{
			Invoke(IdleDestroy, GetDespawnDuration());
		}
	}

	public void StickIn()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved1, b: true);
	}

	public void Unstick()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved1, b: false);
	}

	private void SleepCheck()
	{
		if (!HasParent() || StuckInSomething)
		{
			return;
		}
		if (!rB || rB.isKinematic)
		{
			if (maxBoundsExtent == 0f)
			{
				maxBoundsExtent = ((childCollider != null) ? childCollider.bounds.extents.Max() : bounds.extents.Max());
			}
			Ray ray = new Ray(CenterPoint(), Vector3.down);
			if (!GamePhysics.TraceRealm(GamePhysics.Realm.Server, ray, 0f, out var _, maxBoundsExtent + 0.1f, -928830719, QueryTriggerInteraction.Ignore, this))
			{
				BecomeActive();
			}
		}
		else if (Vector3.SqrMagnitude(base.transform.localPosition - prevLocalPos) < 0.075f)
		{
			BecomeInactive();
		}
		prevLocalPos = base.transform.localPosition;
	}

	public void OnPhysicsNeighbourChanged()
	{
		if (!StuckInSomething)
		{
			BecomeActive();
		}
	}

	public override void OnPositionalNetworkUpdate()
	{
		base.OnPositionalNetworkUpdate();
		if (!HasFlag(Flags.Reserved3))
		{
			CheckValidPosition();
		}
	}

	protected override void TransformChanged()
	{
		base.TransformChanged();
		SingletonComponent<NpcFoodManager>.Instance.Move(this);
	}

	protected override bool ShouldUpdateNetworkPosition()
	{
		if (syncPosition && (bool)rB)
		{
			return !rB.isKinematic;
		}
		return false;
	}

	private void CheckValidPosition()
	{
		if (!rB || !childCollider)
		{
			return;
		}
		using (TimeWarning.New("DroppedItemCheck"))
		{
			Vector3 vector = GetColliderCentre();
			Vector3 vector2 = vector;
			Vector3 vector3 = lastGoodColliderCentre;
			bool flag = HasParent();
			BaseEntity baseEntity = GetParentEntity();
			if (flag != hadParent || baseEntity != parentRef.Get(serverside: true))
			{
				hasLastPos = false;
			}
			hadParent = flag;
			parentRef = parentEntity;
			if (flag)
			{
				Matrix4x4 localToWorldMatrix = GetParentEntity().transform.localToWorldMatrix;
				vector = localToWorldMatrix.MultiplyPoint3x4(vector);
				vector3 = localToWorldMatrix.MultiplyPoint3x4(vector3);
			}
			Vector3 vector4 = vector - vector3;
			Ray ray = new Ray(vector3, vector4.normalized);
			if (broadcast_debug_ddraw)
			{
				UnityEngine.DDraw.BroadcastSphere(base.transform.position, 0.1f, childCollider.enabled ? Color.green : Color.red, 30f, distanceFade: true, zTest: false);
				if (hasLastPos)
				{
					UnityEngine.DDraw.BroadcastLine(vector3, vector3 + vector4, Color.yellow, 30f, distanceFade: true, zTest: false);
				}
			}
			if (hasLastPos && GamePhysics.TraceRealm(GamePhysics.Realm.Server, ray, 0f, out var hitInfo, vector4.magnitude, 1218511105, QueryTriggerInteraction.Ignore, this))
			{
				if (broadcast_debug_ddraw)
				{
					UnityEngine.DDraw.BroadcastLine(base.transform.position, lastGoodPos + smallVerticalOffset, Color.magenta, 30f, distanceFade: true, zTest: false);
				}
				base.transform.localPosition = lastGoodPos + smallVerticalOffset;
				base.transform.localRotation = lastGoodRot;
				if (rB.isKinematic)
				{
					return;
				}
				if (flag && (bool)hitInfo.rigidbody)
				{
					BaseEntity a = GetParentEntity();
					BaseEntity baseEntity2 = GameObjectEx.ToBaseEntity(hitInfo.collider);
					if ((bool)baseEntity2 && (GamePhysics.CompareEntity(a, baseEntity2) || GamePhysics.CompareEntity(a, baseEntity2.GetRootParentEntity())))
					{
						BecomeInactive();
					}
				}
				else
				{
					rB.linearVelocity = Vector3.zero;
					rB.angularVelocity = Vector3.zero;
				}
			}
			else
			{
				lastGoodColliderCentre = vector2;
				lastGoodPos = base.transform.localPosition;
				lastGoodRot = base.transform.localRotation;
				hasLastPos = true;
			}
		}
	}

	public void PrepareForHopper()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved3, b: true);
		}
		if (childCollider != null)
		{
			childCollider.enabled = false;
		}
	}

	public void HopperCancelled()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved3, b: false);
		}
		if (childCollider != null)
		{
			childCollider.enabled = true;
		}
	}

	public void CancelHopper()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved3, b: false);
		}
		if (Rigidbody != null)
		{
			Rigidbody.useGravity = true;
		}
		if (childCollider != null)
		{
			childCollider.enabled = true;
		}
	}

	private void OnUnparented()
	{
		if (cachedSleepCheck != null)
		{
			CancelInvoke(cachedSleepCheck);
		}
		hasLastPos = false;
	}

	private void OnParented()
	{
		if (!(childCollider == null) && base.isServer && !StuckInSomething)
		{
			hasLastPos = false;
			if (cachedSleepCheck == null)
			{
				cachedSleepCheck = SleepCheck;
			}
			InvokeRandomized(cachedSleepCheck, 5.5f, 11f, UnityEngine.Random.Range(-1.1f, 1.1f));
		}
	}

	public override void PostInitShared()
	{
		base.PostInitShared();
		GameObject gameObject = null;
		if (0 == 0)
		{
			if (item != null && item.GetWorldModel().isValid)
			{
				gameObject = base.gameManager.CreatePrefab(item.GetWorldModel().resourcePath, base.transform);
				gameObject.transform.localScale = item.GetWorldModel().Get().transform.localScale;
			}
			else
			{
				gameObject = base.gameManager.CreatePrefab(itemModel.resourcePath, base.transform);
			}
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			TransformEx.SetLayerRecursive(gameObject, base.gameObject.layer);
			childCollider = gameObject.GetComponentInChildren<Collider>();
			if ((bool)childCollider)
			{
				if (HasParent())
				{
					OnParented();
				}
				originalLayer = childCollider.gameObject.layer;
			}
		}
		if (base.isServer)
		{
			rB = base.gameObject.AddComponent<Rigidbody>();
			SetupRigidbody();
			hasLastPos = false;
			CheckValidPosition();
			CheckUnderwaterStatus(canSplash: false);
			UpdateUnderwaterDrag();
		}
		if (item != null)
		{
			PhysicsEffects component = base.gameObject.GetComponent<PhysicsEffects>();
			if (component != null)
			{
				component.entity = this;
				if (item.info.physImpactSoundDef != null)
				{
					component.physImpactSoundDef = item.info.physImpactSoundDef;
				}
			}
			Buoyancy component2 = gameObject.GetComponent<Buoyancy>();
			if (component2 != null && base.isServer)
			{
				component2.rigidBody = rB;
			}
		}
		if (0 == 0)
		{
			gameObject.SetActive(value: true);
		}
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if ((old & Flags.Reserved1) != Flags.Reserved1 && (next & Flags.Reserved1) == Flags.Reserved1)
		{
			BecomeInactive();
		}
		else if ((old & Flags.Reserved1) == Flags.Reserved1 && (next & Flags.Reserved1) != Flags.Reserved1)
		{
			BecomeActive();
		}
		if (base.isServer && (old & Flags.Reserved2) == Flags.Reserved2 != ((next & Flags.Reserved2) == Flags.Reserved2))
		{
			UpdateUnderwaterDrag();
		}
	}

	private void BecomeActive()
	{
		using (TimeWarning.New("DroppedItem.BecomeActive"))
		{
			if (base.isServer)
			{
				if (!rB)
				{
					rB = base.gameObject.AddComponent<Rigidbody>();
					SetupRigidbody();
				}
				rB.isKinematic = false;
				rB.collisionDetectionMode = originalCollisionMode;
				rB.WakeUp();
				if (HasParent())
				{
					Rigidbody component = GetParentEntity().GetComponent<Rigidbody>();
					if (component != null)
					{
						rB.linearVelocity = component.linearVelocity;
						rB.angularVelocity = component.angularVelocity;
					}
				}
				prevLocalPos = base.transform.localPosition;
				hasLastPos = false;
				CheckUnderwaterStatus(canSplash: false);
				UpdateUnderwaterDrag();
			}
			if (childCollider != null)
			{
				childCollider.gameObject.layer = originalLayer;
			}
		}
	}

	private void BecomeInactive()
	{
		using (TimeWarning.New("DroppedItem.BecomeInactive"))
		{
			if (base.isServer)
			{
				if (remove_rb_on_sleep)
				{
					UnityEngine.Object.Destroy(rB);
					rB = null;
					SendNetworkUpdate();
				}
				else
				{
					rB.collisionDetectionMode = CollisionDetectionMode.Discrete;
					rB.isKinematic = true;
				}
			}
			if (childCollider != null)
			{
				childCollider.gameObject.layer = 19;
			}
		}
	}

	public void UpdateItemMass()
	{
		if (rB == null)
		{
			rB = GetComponent<Rigidbody>();
		}
		if (rB == null || item == null || item.contents?.itemList == null)
		{
			return;
		}
		float num = item.info.GetWorldModelMass();
		ItemModContainer component = item.info.GetComponent<ItemModContainer>();
		if (component != null)
		{
			_ = component.worldWeightScale;
		}
		foreach (Item item in item.contents.itemList)
		{
			num += item.info.GetWorldModelMass() * component.worldWeightScale;
		}
		if (component != null && component.maxWeight > 0f)
		{
			num = Mathf.Min(component.maxWeight, num);
		}
		rB.mass = num;
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved3, b: false);
	}

	public override bool ShouldInheritNetworkGroup()
	{
		return false;
	}

	private void CheckUnderwaterStatus(bool canSplash)
	{
		if (!(Rigidbody != null) || !Rigidbody.IsSleeping())
		{
			bool flag = WaterLevel.Test(base.transform.position, waves: false, volumes: true, this);
			if (canSplash && flag && !HasFlag(Flags.Reserved2) && splashEffect.isValid)
			{
				Effect.server.Run(splashEffect.resourcePath, base.transform.position, Vector3.zero);
			}
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved2, flag);
			}
			if (flag && rB != null && !rB.IsSleeping() && (float)lastUnderwaterFlowImpulse > 1f)
			{
				lastUnderwaterFlowImpulse = 0f - UnityEngine.Random.Range(0f, 1f);
				rB.AddForceAtPosition(UnityEngine.Random.onUnitSphere, base.transform.position + UnityEngine.Random.onUnitSphere * 3f, ForceMode.Impulse);
			}
		}
	}

	private void UpdateUnderwaterDrag()
	{
		if (rB != null)
		{
			rB.linearDamping = (HasFlag(Flags.Reserved2) ? 7f : 0.1f);
		}
	}
}
