using System;
using UnityEngine;

public class BiomeVisuals : MonoBehaviour
{
	[Serializable]
	public class EnvironmentVolumeOverride
	{
		public EnvironmentType Environment;

		public TerrainBiome.Enum Biome;
	}

	public GameObject Arid;

	public GameObject Temperate;

	public GameObject Tundra;

	public GameObject Arctic;

	public bool OverrideBiome;

	public TerrainBiome.Enum ToOverride;

	[Horizontal(2, -1)]
	public EnvironmentVolumeOverride[] EnvironmentVolumeOverrides;

	private bool _supportsPooling;

	private GameObject _defaultSelection;

	protected void Awake()
	{
		_supportsPooling = PoolableEx.SupportsPoolingInParent(base.gameObject);
		if ((bool)Arid && Arid.activeSelf)
		{
			_defaultSelection = Arid;
		}
		else if ((bool)Temperate && Temperate.activeSelf)
		{
			_defaultSelection = Temperate;
		}
		else if ((bool)Tundra && Tundra.activeSelf)
		{
			_defaultSelection = Tundra;
		}
		else if ((bool)Arctic && Arctic.activeSelf)
		{
			_defaultSelection = Arctic;
		}
	}

	protected void OnEnable()
	{
		int num = ((TerrainMeta.BiomeMap != null) ? TerrainMeta.BiomeMap.GetBiomeMaxType(base.transform.position) : 2);
		if (OverrideBiome)
		{
			num = (int)ToOverride;
		}
		else if (EnvironmentVolumeOverrides.Length != 0)
		{
			EnvironmentType environmentType = EnvironmentManager.Get(base.transform.position);
			EnvironmentVolumeOverride[] environmentVolumeOverrides = EnvironmentVolumeOverrides;
			foreach (EnvironmentVolumeOverride environmentVolumeOverride in environmentVolumeOverrides)
			{
				if ((environmentType & environmentVolumeOverride.Environment) != 0)
				{
					num = (int)environmentVolumeOverride.Biome;
					break;
				}
			}
		}
		switch (num)
		{
		case 1:
			SetChoice(Arid);
			break;
		case 2:
			SetChoice(Temperate);
			break;
		case 4:
			SetChoice(Tundra);
			break;
		case 8:
			SetChoice(Arctic);
			break;
		default:
			SetChoice(_defaultSelection);
			break;
		}
	}

	private void SetChoice(GameObject selection)
	{
		bool flag = !_supportsPooling;
		ApplyChoice(selection, Arid, flag);
		ApplyChoice(selection, Temperate, flag);
		ApplyChoice(selection, Tundra, flag);
		ApplyChoice(selection, Arctic, flag);
		if (selection != null)
		{
			selection.SetActive(value: true);
		}
		if (flag)
		{
			GameManager.Destroy(this);
		}
	}

	private void ApplyChoice(GameObject selection, GameObject target, bool shouldDestroy)
	{
		if (target != null && target != selection)
		{
			if (shouldDestroy)
			{
				GameManager.Destroy(target);
			}
			else
			{
				target.SetActive(value: false);
			}
		}
	}
}
