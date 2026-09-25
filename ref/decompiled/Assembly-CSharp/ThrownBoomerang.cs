using System;
using Network;
using Rust.Demo;
using UnityEngine;

public class ThrownBoomerang : BaseEntity
{
	[Header("References")]
	public ItemDefinition boomerangItem;

	[Header("Settings")]
	public float timeToReturnOnArc = 3f;

	public float secondsUntilStartArc = 0.9f;

	public float lerpSpeed = 20f;

	private const float CATCH_DISTANCE = 1.5f;

	private const float HOMING_TO_PLAYER_DISTANCE = 6f;

	private Vector3 lastMoveDirection;

	private Vector3 gravityVelocity = Vector3.zero;

	private bool calculated;

	private float returnTimer;

	private float timeToReturn;

	private Vector3 startLocation;

	private Vector3 midLocation;

	private Vector3 endLocation;

	private Vector3 spawnLocation = Vector3.zero;

	private ThrownBoomerangServerProjectile projectile;

	private BasePlayer creatorPlayer;

	private Boomerang originEntityItem;

	[NonSerialized]
	public ItemOwnershipShare ItemOwnership;

	[NonSerialized]
	public float Condition;

	public override bool PositionTickFixedTime
	{
		protected get
		{
			return true;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ThrownBoomerang.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	private void DoBoomerangMove()
	{
		if (Reader.IsActive && Reader.Active.IsScrubbing)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		float num = 15f;
		if (!calculated)
		{
			returnTimer = 0f;
			startLocation = base.transform.position;
			endLocation = spawnLocation;
			endLocation += Vector3.up * 1.2f;
			Vector3 normalized = (endLocation - startLocation).normalized;
			Vector3 vector = Vector3.Cross(Vector3.up, normalized);
			midLocation = (startLocation + endLocation) / 2f;
			midLocation += vector * num;
			if (base.isServer)
			{
				projectile.ProjectileHandleMovement(state: false);
			}
			calculated = true;
		}
		BasePlayer basePlayer = null;
		if (base.isServer)
		{
			basePlayer = creatorPlayer;
		}
		if (basePlayer != null && Vector3.Distance(basePlayer.transform.position, spawnLocation) <= 6f && IsValidPlayer(basePlayer))
		{
			endLocation = basePlayer.transform.position;
			endLocation += Vector3.up * 1.5f;
			Vector3 normalized2 = (endLocation - startLocation).normalized;
			Vector3 vector2 = Vector3.Cross(Vector3.up, normalized2);
			midLocation = (startLocation + endLocation) / 2f;
			midLocation += vector2 * num;
		}
		float num2 = returnTimer / timeToReturnOnArc;
		Vector3 vector3 = FakePhysicsRope.GetRationalBezierPoint(startLocation, midLocation, endLocation, Mathf.Clamp01(num2));
		if (num2 >= 1f)
		{
			gravityVelocity += Vector3.down * 9.81f * deltaTime;
			lastMoveDirection += gravityVelocity * deltaTime;
			vector3 = base.transform.position + lastMoveDirection;
		}
		else if (num2 > 0.95f)
		{
			vector3 += Vector3.down * 0.03f;
		}
		Vector3 vector4 = vector3 - base.transform.position;
		if (vector4 != Vector3.zero && base.isServer)
		{
			projectile.SetVelocity(vector4);
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, Quaternion.LookRotation(vector4.normalized), deltaTime * 2f);
		}
		base.transform.position = Vector3.MoveTowards(base.transform.position, vector3, deltaTime * lerpSpeed);
		if (num2 <= 1f)
		{
			lastMoveDirection = vector4;
		}
		returnTimer += deltaTime;
	}

	private bool IsValidPlayer(BasePlayer ply)
	{
		if (ply == null)
		{
			return false;
		}
		if (ply.IsDead())
		{
			return false;
		}
		if (ply.IsSleeping())
		{
			return false;
		}
		return true;
	}

	public override float GetNetworkTime()
	{
		return Time.fixedTime;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (base.isClient)
		{
			return;
		}
		spawnLocation = base.transform.position;
		projectile = GetComponent<ThrownBoomerangServerProjectile>();
		if ((bool)projectile)
		{
			projectile.InitializeVelocity(base.transform.forward * projectile.speed);
			projectile.ProjectileHandleMovement(state: true);
			projectile.SetStartPosition(spawnLocation);
			InvokeRepeating(DoBoomerangMove, secondsUntilStartArc, 0f);
			if (!(creatorEntity is BasePlayer basePlayer))
			{
				return;
			}
			creatorPlayer = basePlayer;
			base.OwnerID = creatorPlayer.userID;
			creatorEntity = creatorPlayer;
			Item activeItem = creatorPlayer.GetActiveItem();
			if (activeItem != null)
			{
				if (activeItem.GetHeldEntity() is Boomerang boomerang)
				{
					originEntityItem = boomerang;
				}
				ItemOwnership = activeItem.TakeOwnershipShare();
			}
			Invoke(LateRPC, 0.1f);
			InvokeRepeating(CheckReturnToHand, secondsUntilStartArc, 0f);
		}
		else
		{
			KillThrownBoomerang();
		}
	}

	private void LateRPC()
	{
		Item activeItem = creatorPlayer.GetActiveItem();
		if (activeItem != null && activeItem.GetHeldEntity() is Boomerang)
		{
			ClientRPC(RpcTarget.Player("SetClientPlayer", creatorPlayer), activeItem.uid.Value);
		}
	}

	public void CreateWorldModel(HitInfo info, Vector3 attackDir)
	{
		float num = Mathf.Max(projectile.scanRange, projectile.radius);
		num /= 2f;
		Item item = ItemManager.Create(boomerangItem, 1, 0uL, isServerSide: true, 0uL);
		BaseEntity baseEntity = null;
		bool flag = false;
		if (info.HitEntity == null || !info.HitEntity.IsValid())
		{
			baseEntity = item.CreateWorldObject(info.HitPositionWorld + -attackDir * num * 0.3f, Quaternion.LookRotation(-attackDir));
			flag = info.HitMaterial != Projectile.WaterMaterialID();
			if (!info.HitEntity.IsValid())
			{
				flag = false;
			}
		}
		else if (info.HitBone == 0)
		{
			Vector3 hitPositionLocal = info.HitPositionLocal;
			baseEntity = item.CreateWorldObject(hitPositionLocal, Quaternion.LookRotation(info.HitEntity.transform.InverseTransformDirection(attackDir.normalized)), info.HitEntity);
			flag = false;
		}
		else
		{
			baseEntity = item.CreateWorldObject(info.HitPositionWorld, Quaternion.LookRotation(info.HitEntity.transform.InverseTransformDirection(attackDir.normalized)));
			flag = false;
		}
		if (flag)
		{
			DroppedItem droppedItem = baseEntity as DroppedItem;
			if (droppedItem != null)
			{
				droppedItem.StickIn();
			}
			else
			{
				baseEntity.GetComponent<Rigidbody>().isKinematic = true;
			}
		}
		else
		{
			baseEntity.GetComponent<Rigidbody>().AddTorque(attackDir.normalized * UnityEngine.Random.Range(5f, 10f), ForceMode.Impulse);
		}
		item.condition = Condition;
		item.SetItemOwnership(ItemOwnership);
		baseEntity.OwnerID = base.OwnerID;
		baseEntity.creatorEntity = creatorEntity;
	}

	public void OnHit()
	{
		if (originEntityItem != null)
		{
			Item item = originEntityItem.GetItem();
			if (item != null)
			{
				float num = item.maxCondition * 0.1f;
				Condition -= num;
				item.UseItem();
				item.SetParent(null);
			}
		}
	}

	private void KillThrownBoomerang()
	{
		CancelInvoke(CheckReturnToHand);
		Kill();
	}

	public void CheckReturnToHand()
	{
		if (!(creatorPlayer == null) && !creatorPlayer.IsDead() && !creatorPlayer.IsSleeping() && Vector3.Distance(creatorPlayer.transform.position, base.transform.position) <= 1.5f)
		{
			Item activeItem = creatorPlayer.GetActiveItem();
			if (activeItem != null && activeItem.GetHeldEntity() is Boomerang { HasThrown: not false } boomerang && !(boomerang != originEntityItem))
			{
				boomerang.SetHasThrown(thrown: false);
				KillThrownBoomerang();
			}
		}
	}
}
