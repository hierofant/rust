using Facepunch;
using ProtoBuf;
using UnityEngine;

public class ElectricWaterWheel : IOEntity
{
	public class WaterWheelUpdateWorkQueue : PersistentObjectWorkQueue<ElectricWaterWheel>
	{
		protected override void RunJob(ElectricWaterWheel wheel)
		{
			wheel.PowerUpdate();
			if (wheel.isInWater)
			{
				wheel.WaterUpdate();
			}
		}
	}

	[Header("Power Output")]
	public int maxPowerGenerationFromWater = 100;

	public int maxPowerGenerationFromManualUse = 100;

	[Tooltip("How fast the power output ramps up to max during manual use. Also used for visual wheel rotation lerp speed.")]
	public float manualPowerLerpSpeed = 0.25f;

	[Tooltip("How much to multiply the Perlin noise of the ocean water speeds. Higher values mean more power in the ocean.")]
	public float oceanPowerScaling = 1.2f;

	[Header("Water Wheel Speed Settings")]
	public Transform wheelRotationTransform;

	public float maxWheelWaterRotationSpeed = 30f;

	public float maxWheelManualRotationSpeed = 30f;

	[Tooltip("How fast the wheel changes direction when rocking back and forth when there's no water flow")]
	public float rockingSpeed = 0.75f;

	[Tooltip("How far the wheel spins in each direction when rocking.")]
	public float rockingAmount = 2f;

	[Header("Water Wheel dependencies")]
	public Transform waterSamplePoint;

	public Transform alignWithWaterDirectionAnchor;

	public GameObjectRef waterWheelMountablePrefab;

	[Header("Wheel Audio")]
	public SoundDefinition wheelMovementStartSoundDef;

	public SoundDefinition wheelMovementLoopSoundDef;

	public SoundDefinition wheelMovementStopSoundDef;

	public SoundDefinition wheelMovementAccentSoundDef;

	public SoundDefinition wheelWaterLoopSoundDef;

	[Header("Wheel Spawn Sounds")]
	public GameObjectRef deployEffect;

	public GameObjectRef deployEffectWater;

	[Header("Wheel Audio Settings")]
	public float movementThreshold = 10f;

	public float wheelLoopMinPitch = 0.8f;

	public float wheelLoopMaxPitch = 1.2f;

	public float wheelMinVolumeGain = 0.2f;

	public float wheelMaxVolumeGain = 1f;

	public float wheelMovementAccentAngle = 90f;

	public float wheelMovementAccentCooldown = 0.2f;

	public float wheelWaterPitchMin = 0.7f;

	public float wheelWaterPitchMax = 1.2f;

	public float wheelWaterGainMin = 0.4f;

	public float wheelWaterGainMax = 1f;

	public SoundPlayer ManualUseWaterSounds;

	[Header("Wheel VFX")]
	public GameObject RunningFX;

	public GameObject RunningDamagedFX;

	public GameObject StoppedFX;

	public GameObject StoppedDamagedFX;

	public ParticleSystem ManualUseWaterFX;

	public Transform manualWaterFXBottomAnchor;

	public Transform manualWaterFXTopAnchor;

	private EntityRef<WaterWheelMountable> waterWheelMountableRef;

	private WaterWheelMountable wwm;

	private bool fetchedWaterInfo;

	private bool isInWater;

	private float serverWaterSpeed;

	private Vector3 waterFlowDirection;

	private int topology;

	private float _waterAlignment;

	private bool _waterAlignmentCached;

	private TimeSince timeSinceWaterUpdate;

	private TimeSince timeSincePowerUpdate;

	private bool isInOpenWater = true;

	public static WaterWheelUpdateWorkQueue UpdateWorkQueue = new WaterWheelUpdateWorkQueue();

	private WaterWheelMountable waterWheelMountable
	{
		get
		{
			if (wwm == null)
			{
				wwm = waterWheelMountableRef.Get(base.isServer);
			}
			return wwm;
		}
	}

	private bool IsInOcean
	{
		get
		{
			if ((topology & 0x80) == 0 && (topology & 0x10) == 0 && (topology & 0x100) == 0)
			{
				return (topology & 8) != 0;
			}
			return true;
		}
	}

	private bool IsInRiver
	{
		get
		{
			if ((topology & 0x4000) == 0)
			{
				return (topology & 0x8000) != 0;
			}
			return true;
		}
	}

	private float waterFlowAlignment
	{
		get
		{
			if (!_waterAlignmentCached)
			{
				_waterAlignment = GetWaterFlowAlignment();
				_waterAlignmentCached = true;
			}
			return _waterAlignment;
		}
	}

	public override int MaximalPowerOutput()
	{
		return maxPowerGenerationFromWater;
	}

	public override bool IsRootEntity()
	{
		return true;
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		if (child.prefabID == waterWheelMountablePrefab.GetEntity().prefabID)
		{
			WaterWheelMountable entity = (WaterWheelMountable)child;
			waterWheelMountableRef.Set(entity);
		}
	}

	public void GetWaterInfo()
	{
		if (!fetchedWaterInfo)
		{
			isInWater = WaterLevel.GetWaterInfo(waterSamplePoint.position, waves: false, volumes: true, this).isValid;
			if (isInWater)
			{
				topology = TerrainMeta.TopologyMap.GetTopology(waterSamplePoint.position);
				waterFlowDirection = WaterLevel.GetWaterFlowDirection(alignWithWaterDirectionAnchor.position);
			}
			fetchedWaterInfo = true;
		}
	}

	public float GetWaterFlowAlignment()
	{
		if (waterFlowDirection == Vector3.zero)
		{
			return 0f;
		}
		if (!IsInRiver && !IsInOcean)
		{
			return 0f;
		}
		Vector3 forward = alignWithWaterDirectionAnchor.forward;
		return Vector3.Dot(waterFlowDirection, forward);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		GetWaterInfo();
		serverWaterSpeed = 1f;
		UpdateWorkQueue.Add(this);
	}

	internal override void DoServerDestroy()
	{
		UpdateWorkQueue.Remove(this);
		base.DoServerDestroy();
	}

	public bool IsInOpenWater()
	{
		float num = 10f;
		Vector3 position = waterSamplePoint.position - waterFlowDirection.normalized * num;
		if (!IsVisible(position, num + 1f))
		{
			return false;
		}
		return true;
	}

	public float GetOceanNoiseSpeed()
	{
		using (TimeWarning.New("ElectricWaterWheel.GetOceanNoiseSpeed"))
		{
			float num = Time.time / 60f;
			float num2 = base.transform.position.x / 512f;
			float num3 = base.transform.position.z / 512f;
			return (Mathf.PerlinNoise(num2 + num, num3 + num * 0.1f) - 0.5f) * oceanPowerScaling;
		}
	}

	public void WaterUpdate()
	{
		using (TimeWarning.New("ElectricWaterWheel.WaterUpdate"))
		{
			if (!((float)timeSinceWaterUpdate < 15f))
			{
				isInOpenWater = IsInOpenWater();
				serverWaterSpeed = (isInOpenWater ? 1f : 0.1f);
				if (IsInOcean && !IsInRiver)
				{
					serverWaterSpeed *= GetOceanNoiseSpeed();
				}
				if (serverWaterSpeed < 1f)
				{
					SendNetworkUpdate();
				}
				timeSinceWaterUpdate = 0f;
			}
		}
	}

	public void PowerUpdate()
	{
		if ((float)timeSincePowerUpdate < 1f)
		{
			return;
		}
		if (!fetchedWaterInfo)
		{
			GetWaterInfo();
		}
		int num = 0;
		bool flag = waterWheelMountable != null && waterWheelMountable.AnyMounted();
		bool flag2 = waterWheelMountable != null && waterWheelMountable.HasFlag(Flags.Reserved11);
		if (flag && flag2)
		{
			float t = Time.deltaTime * manualPowerLerpSpeed;
			num = Mathf.CeilToInt(Mathf.Lerp(currentEnergy, maxPowerGenerationFromManualUse, t));
		}
		else if (!flag && isInWater)
		{
			float num2 = Mathf.Abs(waterFlowAlignment) * (float)maxPowerGenerationFromWater;
			if (!isInOpenWater)
			{
				num2 *= 0.1f;
			}
			if (IsInOcean)
			{
				num2 *= Mathf.Abs(serverWaterSpeed);
			}
			else if (!IsInRiver)
			{
				num2 = 0f;
			}
			num = Mathf.CeilToInt(Mathf.Clamp(num2, 0f, maxPowerGenerationFromWater));
		}
		else
		{
			num = Mathf.FloorToInt(Mathf.Lerp(currentEnergy, 0f, Time.deltaTime * 5f));
		}
		bool num3 = currentEnergy != num;
		currentEnergy = num;
		if (num3)
		{
			MarkDirty();
			SendNetworkUpdate();
		}
		timeSincePowerUpdate = 0f;
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		if (outputSlot != 0)
		{
			return 0;
		}
		return currentEnergy;
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		if (!fetchedWaterInfo)
		{
			GetWaterInfo();
		}
		GameObjectRef gameObjectRef = (isInWater ? deployEffectWater : deployEffect);
		if (gameObjectRef.isValid)
		{
			Effect.server.Run(gameObjectRef.resourcePath, base.transform.position, Vector3.up);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (!info.forDisk)
		{
			if (info.msg.ioEntity == null)
			{
				info.msg.ioEntity = Pool.Get<ProtoBuf.IOEntity>();
			}
			info.msg.ioEntity.genericFloat1 = serverWaterSpeed;
			info.msg.ioEntity.genericEntRef1 = waterWheelMountableRef.uid;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
	}
}
