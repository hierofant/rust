using UnityEngine;
using UnityEngine.EventSystems;

namespace FIMSpace.FTail;

[AddComponentMenu("FImpossible Creations/Tail Animator Utilities/Tail Animator Wind")]
public class TailAnimatorWind : MonoBehaviour, IDropHandler, IEventSystemHandler, IFHierarchyIcon
{
	[Header("In playmode you will find this object in DontDestroyOnLoad")]
	[FPD_Header("Main Wind Setings", 2f, 4f, 2)]
	public float power = 1f;

	public float additionalTurbulence = 1f;

	public float additionalTurbSpeed = 1f;

	[Space(7f)]
	public WindZone SyncWithUnityWindZone;

	public float UnityWindZonePowerMul = 2f;

	public float UnityWindZoneTurbMul = 1f;

	[Header("Overriding wind if value below different than 0,0,0")]
	public Vector3 overrideWind = Vector3.zero;

	[FPD_Header("Procedural Wind Settings (if not syncing and not overriding)", 6f, 4f, 2)]
	[Range(0.1f, 1f)]
	public float rapidness = 0.95f;

	[FPD_Suffix(0f, 360f, FPD_SuffixAttribute.SuffixMode.FromMinToMaxRounded, "°", true, 0)]
	public float changesPower = 90f;

	[Range(0f, 10f)]
	[Header("Extra")]
	public float turbulenceSpeed = 1f;

	[Tooltip("Increase to make objects next to each other wave in slightly different way")]
	[FPD_Header("World Position Turbulence", 6f, 4f, 2)]
	public float worldTurb = 1f;

	[Tooltip("If higher no performance cost, it is just a number")]
	public float worldTurbScale = 512f;

	public float worldTurbSpeed = 5f;

	[FPD_Header("Tail Compoenents Related", 6f, 4f, 2)]
	[Tooltip("When tail is longer then power of wind should be higher")]
	public bool powerDependOnTailLength = true;

	[Tooltip("Don't destroy on load")]
	public bool persistThroughAllScenes;

	private Vector3 targetWind = Vector3.zero;

	private Vector3 smoothWind = Vector3.zero;

	private Vector3 windVeloHelper = Vector3.zero;

	private Quaternion windOrientation = Quaternion.identity;

	private Quaternion smoothWindOrient = Quaternion.identity;

	private Quaternion smoothWindOrientHelper = Quaternion.identity;

	private float[] randNumbers;

	private float[] randTimes;

	private float[] randSpeeds;

	private int frameOffset = 2;

	private Vector3 finalAddTurbulence = Vector3.zero;

	private Vector3 addTurbHelper = Vector3.zero;

	private Vector3 smoothAddTurbulence = Vector3.zero;

	public string EditorIconPath => "Tail Animator/TailAnimatorWindIconSmall";

	public static TailAnimatorWind Instance { get; private set; }

	public void OnDrop(PointerEventData data)
	{
	}

	private void Awake()
	{
		if (Application.isPlaying)
		{
			Instance = this;
			if (persistThroughAllScenes)
			{
				Object.DontDestroyOnLoad(base.gameObject);
			}
		}
	}

	public void OnValidate()
	{
		Instance = this;
	}

	private void Update()
	{
		if (frameOffset > 0)
		{
			frameOffset--;
		}
		else
		{
			ComputeWind();
		}
	}

	public static void Refresh()
	{
		if (Instance == null)
		{
			Debug.Log("[Tail Animator Wind] No Tail Animator Wind component on the scene!");
			Debug.LogWarning("[Tail Animator Wind] No Tail Animator Wind component on the scene!");
		}
	}

	public void AffectTailWithWind(TailAnimator2 t)
	{
		if (!t.UseWind || t.WindEffectPower <= 0f || t.TailSegments.Count <= 0)
		{
			return;
		}
		float num = 1f;
		if (powerDependOnTailLength)
		{
			num = t._TC_TailLength * t.TailSegments[0].transform.lossyScale.z / 5f;
			if (t.TailSegments.Count > 3)
			{
				num *= Mathf.Lerp(0.7f, 3f, (float)t.TailSegments.Count / 14f);
			}
		}
		if (t.WindWorldNoisePower > 0f)
		{
			float num2 = worldTurbSpeed;
			if ((bool)SyncWithUnityWindZone)
			{
				num2 *= SyncWithUnityWindZone.windTurbulence * UnityWindZoneTurbMul;
			}
			float num3 = 0.5f + Mathf.Sin(Time.time * num2 + t.TailSegments[0].ProceduralPosition.x * worldTurbScale) / 2f + (0.5f + Mathf.Cos(Time.time * num2 + t.TailSegments[0].ProceduralPosition.z * worldTurbScale) / 2f);
			num += num3 * worldTurb * t.WindWorldNoisePower;
		}
		num *= t.WindEffectPower;
		if (t.WindTurbulencePower > 0f)
		{
			t.WindEffect = new Vector3(targetWind.x * num + finalAddTurbulence.x * t.WindTurbulencePower, targetWind.y * num + finalAddTurbulence.y * t.WindTurbulencePower, targetWind.z * num + finalAddTurbulence.z * t.WindTurbulencePower);
		}
		else
		{
			t.WindEffect = new Vector3(targetWind.x * num, targetWind.y * num, targetWind.z * num);
		}
	}

	private void Start()
	{
		int num = 10;
		randNumbers = new float[num];
		randTimes = new float[num];
		randSpeeds = new float[num];
		for (int i = 0; i < 10; i++)
		{
			randNumbers[i] = Random.Range(-1000f, 1000f);
			randTimes[i] = Random.Range(-1000f, 1000f);
			randSpeeds[i] = Random.Range(0.18f, 0.7f);
		}
	}

	private void ComputeWind()
	{
		Vector3 target;
		if ((bool)SyncWithUnityWindZone)
		{
			target = SyncWithUnityWindZone.transform.forward * SyncWithUnityWindZone.windMain * UnityWindZonePowerMul;
			base.transform.rotation = SyncWithUnityWindZone.transform.rotation;
		}
		else if (overrideWind != Vector3.zero)
		{
			target = overrideWind;
		}
		else
		{
			for (int i = 0; i < 4; i++)
			{
				randTimes[i] += Time.deltaTime * randSpeeds[i] * turbulenceSpeed;
			}
			Quaternion quaternion = windOrientation;
			float num = -1f + Mathf.PerlinNoise(randTimes[0], 256f + randTimes[1]) * 2f;
			float y = -1f + Mathf.PerlinNoise(0f - randTimes[1], 55f + randTimes[2]) * 2f;
			float num2 = -1f + Mathf.PerlinNoise(0f - randTimes[3], 55f + randTimes[0]) * 2f;
			quaternion *= Quaternion.Euler(new Vector3(0f, y, 0f) * changesPower);
			quaternion = Quaternion.Euler(num * (changesPower / 6f), quaternion.eulerAngles.y, num2 * (changesPower / 6f));
			smoothWindOrient = FEngineering.SmoothDampRotation(smoothWindOrient, quaternion, ref smoothWindOrientHelper, 1f - rapidness, Time.deltaTime);
			base.transform.rotation = smoothWindOrient;
			target = smoothWindOrient * Vector3.forward;
		}
		smoothAddTurbulence = Vector3.SmoothDamp(smoothAddTurbulence, GetAddTurbulence() * additionalTurbulence, ref addTurbHelper, 0.05f, float.PositiveInfinity, Time.deltaTime);
		smoothWind = Vector3.SmoothDamp(smoothWind, target, ref windVeloHelper, 0.1f, float.PositiveInfinity, Time.deltaTime);
		for (int j = 7; j < 10; j++)
		{
			randTimes[j] += Time.deltaTime * randSpeeds[j] * turbulenceSpeed;
		}
		float num3 = power * 0.015f;
		num3 *= 0.5f + Mathf.PerlinNoise(randTimes[7] * 2f, 25f + randTimes[8] * 0.5f);
		finalAddTurbulence = smoothAddTurbulence * num3;
		targetWind = smoothWind * num3;
	}

	private Vector3 GetAddTurbulence()
	{
		float num = additionalTurbSpeed;
		if ((bool)SyncWithUnityWindZone)
		{
			num *= SyncWithUnityWindZone.windTurbulence * UnityWindZoneTurbMul;
		}
		for (int i = 4; i < 7; i++)
		{
			randTimes[i] += Time.deltaTime * randSpeeds[i] * num;
		}
		float x = -1f + Mathf.PerlinNoise(randTimes[4] + 7.123f, -2.324f + Time.time * 0.24f) * 2f;
		float y = -1f + Mathf.PerlinNoise(randTimes[5] - 4.7523f, -25.324f + Time.time * 0.54f) * 2f;
		float z = -1f + Mathf.PerlinNoise(randTimes[6] + 1.123f, -63.324f + Time.time * -0.49f) * 2f;
		return new Vector3(x, y, z);
	}
}
