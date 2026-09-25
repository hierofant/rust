using UnityEngine;
using UnityEngine.Serialization;

public class TriggerRadiation : TriggerBase
{
	[Tooltip("Higher the tier, higher the radiations.")]
	public Radiation.Tier radiationTier = Radiation.Tier.LOW;

	[Tooltip("The radiation amount is determined by the radiation tier, setting this will ignore the tier and use that value instead.")]
	public float RadiationAmountOverride;

	public bool BypassArmor;

	[Tooltip("Armor scales the dose instead of subtracting from it, so rad gear always reduces this volume but only full rad protection blocks it entirely. Ignored if BypassArmor is set.")]
	public bool ScaleByArmor;

	[Space]
	[Tooltip("The fraction of the radius where we fade in from 0-1 dosage.")]
	[Min(0f)]
	public float falloff = 0.1f;

	public bool usePerAxisFalloff;

	public Vector3 falloffPerAxis;

	[Space]
	[Tooltip("Use sphere collider size instead of the transform scale. For sphere triggers only (doesn't make sense for boxes)")]
	[FormerlySerializedAs("UseColliderRadius")]
	public bool DontScaleRadiationSize;

	public bool UseLOSCheck;

	[Tooltip("For sphere triggers only. If enabled, player will take more rads when close to the center of the volume.")]
	public bool IncreaseDamageNearCenter = true;

	public bool ApplyLocalHeightCheck;

	public bool IgnoreAboveGroundPlayers;

	[Tooltip("For sprinklers needing an half sphere trigger.")]
	public float MinLocalHeight;

	private SphereCollider sphereCollider;

	private BoxCollider boxCollider;

	private float FalloffFraction => Mathf.Clamp(falloff, 0.01f, 1f);

	public bool UseColliderScale => DontScaleRadiationSize;

	private bool UseSphere
	{
		get
		{
			if (sphereCollider == null)
			{
				sphereCollider = GetComponent<SphereCollider>();
			}
			return sphereCollider != null;
		}
	}

	private bool UseBox
	{
		get
		{
			if (boxCollider == null)
			{
				boxCollider = GetComponent<BoxCollider>();
			}
			return boxCollider != null;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		boxCollider = GetComponent<BoxCollider>();
		sphereCollider = GetComponent<SphereCollider>();
	}

	private float GetRadiationRadius()
	{
		if (sphereCollider != null)
		{
			if (UseColliderScale)
			{
				return sphereCollider.radius;
			}
			return sphereCollider.radius * base.transform.localScale.Max();
		}
		return 0f;
	}

	private (Vector3 center, Vector3 extents) GetRadiationBounds()
	{
		if (boxCollider == null)
		{
			return (center: Vector3.zero, extents: Vector3.zero);
		}
		Vector3 item = base.transform.TransformPoint(boxCollider.center);
		Vector3 item2 = Vector3.Scale(boxCollider.size * 0.5f, base.transform.lossyScale);
		return (center: item, extents: item2);
	}

	private float GetRadiationAmount()
	{
		if (RadiationAmountOverride > 0f)
		{
			return RadiationAmountOverride;
		}
		return Radiation.GetRadiation(radiationTier);
	}

	public float GetRadiationForPosition(Vector3 position, float radProtection, BaseEntity forEntity)
	{
		if (ApplyLocalHeightCheck && base.transform.InverseTransformPoint(position).y < MinLocalHeight)
		{
			return 0f;
		}
		if (IgnoreAboveGroundPlayers && !forEntity.IsUnderground())
		{
			return 0f;
		}
		if (UseLOSCheck && !GamePhysics.LineOfSight(base.gameObject.transform.position, position, 2097152, 0f, 0.25f))
		{
			return 0f;
		}
		float radiationAmount = GetRadiationAmount();
		float num = 0f;
		if (UseSphere)
		{
			float radiationRadius = GetRadiationRadius();
			num = Mathf.InverseLerp(value: IncreaseDamageNearCenter ? Vector3.Distance(base.gameObject.transform.position, position) : 0f, a: radiationRadius, b: radiationRadius * (1f - FalloffFraction));
		}
		else if (UseBox)
		{
			(Vector3, Vector3) radiationBounds = GetRadiationBounds();
			Vector3 vector = Quaternion.Inverse(base.transform.rotation) * (position - radiationBounds.Item1);
			Vector3 vector2 = new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
			Vector3 item = radiationBounds.Item2;
			Vector3 vector3 = radiationBounds.Item2;
			if (usePerAxisFalloff)
			{
				vector3.x = Mathf.Max(0f, item.x - falloffPerAxis.x);
				vector3.y = Mathf.Max(0f, item.y - falloffPerAxis.y);
				vector3.z = Mathf.Max(0f, item.z - falloffPerAxis.z);
			}
			else
			{
				vector3 = item * (1f - FalloffFraction);
			}
			if (vector2.x <= vector3.x && vector2.y <= vector3.y && vector2.z <= vector3.z)
			{
				num = 1f;
			}
			else if (vector2.x <= item.x && vector2.y <= item.y && vector2.z <= item.z)
			{
				float a = ((item.x == vector3.x) ? 1f : Mathf.Clamp01(Mathf.InverseLerp(item.x, vector3.x, vector2.x)));
				float a2 = ((item.y == vector3.y) ? 1f : Mathf.Clamp01(Mathf.InverseLerp(item.y, vector3.y, vector2.y)));
				float b = ((item.z == vector3.z) ? 1f : Mathf.Clamp01(Mathf.InverseLerp(item.z, vector3.z, vector2.z)));
				num = Mathf.Min(a, Mathf.Min(a2, b));
			}
			else
			{
				num = 0f;
			}
		}
		float num2;
		if (BypassArmor)
		{
			num2 = radiationAmount;
		}
		else if (ScaleByArmor)
		{
			float num3 = 1f - Mathf.Clamp01(radProtection / (Radiation.MaxExposureProtection * 100f));
			num2 = radiationAmount * num3;
		}
		else
		{
			num2 = Radiation.GetRadiationAfterProtection(radiationAmount, radProtection);
		}
		return num2 * num;
	}

	public override GameObject InterestedInObject(GameObject obj)
	{
		obj = base.InterestedInObject(obj);
		if (obj == null)
		{
			return null;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
		if (baseEntity == null)
		{
			return null;
		}
		if (baseEntity.isClient)
		{
			return null;
		}
		if (!(baseEntity is BaseCombatEntity))
		{
			return null;
		}
		return baseEntity.gameObject;
	}
}
