using System;
using UnityEngine;

public class HealthThresholdToggle : FacepunchBehaviour, IPrefabPreProcess
{
	[Serializable]
	public struct ThresholdEntry
	{
		[Tooltip("Active while health fraction is above this. At or below, the objects are disabled.")]
		[Range(0f, 1f)]
		public float disableAtHealthFraction;

		public GameObject[] targets;
	}

	[SerializeField]
	private ThresholdEntry[] entries;

	private float lastAppliedFraction = -1f;

	public bool CanRunDuringBundling => true;

	public void UpdateHealth(float healthFraction)
	{
		if (healthFraction == lastAppliedFraction)
		{
			return;
		}
		if (lastAppliedFraction >= 0f)
		{
			_ = healthFraction < lastAppliedFraction;
		}
		else
			_ = 0;
		lastAppliedFraction = healthFraction;
		ThresholdEntry[] array = entries;
		for (int i = 0; i < array.Length; i++)
		{
			ThresholdEntry thresholdEntry = array[i];
			bool flag = healthFraction > thresholdEntry.disableAtHealthFraction;
			GameObject[] targets = thresholdEntry.targets;
			foreach (GameObject gameObject in targets)
			{
				if (!(gameObject == null) && gameObject.activeSelf != flag)
				{
					gameObject.SetActive(flag);
				}
			}
		}
	}

	public void ResetState()
	{
		lastAppliedFraction = -1f;
		ThresholdEntry[] array = entries;
		for (int i = 0; i < array.Length; i++)
		{
			GameObject[] targets = array[i].targets;
			foreach (GameObject gameObject in targets)
			{
				if (gameObject != null && !gameObject.activeSelf)
				{
					gameObject.SetActive(value: true);
				}
			}
		}
	}

	public void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		ThresholdEntry[] array = entries;
		for (int i = 0; i < array.Length; i++)
		{
			GameObject[] targets = array[i].targets;
			foreach (GameObject gameObject in targets)
			{
				if (!(gameObject == null))
				{
					Gibbable component = gameObject.GetComponent<Gibbable>();
					if (component != null)
					{
						component.isConditional = true;
					}
				}
			}
		}
	}
}
