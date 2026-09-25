using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Powergrid Stage Config")]
public class PowergridStageConfig : BaseScriptableObject
{
	[Serializable]
	public class StageData
	{
		public int requiredFuses;
	}

	private static PowergridStageConfig _instance;

	[SerializeField]
	private StageData[] stages;

	public static PowergridStageConfig instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FileSystem.Load<PowergridStageConfig>("assets/content/config/powergridstageconfig.asset");
			}
			if (_instance == null)
			{
				Debug.LogError("Failed to load EntityColorSwapLookup");
			}
			return _instance;
		}
	}

	public int GetStageForFuseCount(int fuseCount)
	{
		int result = 0;
		for (int i = 0; i < stages.Length; i++)
		{
			StageData stageData = stages[i];
			if (fuseCount >= stageData.requiredFuses)
			{
				result = i + 1;
			}
		}
		return result;
	}

	public bool TryGetStageDataForStage(int stage, out StageData stageData)
	{
		stageData = null;
		if (stage < 1 || stage > stages.Length)
		{
			Debug.LogWarning($"No powergrid stage data exists for stage {stage}");
			return false;
		}
		stageData = stages[stage - 1];
		return true;
	}

	public int GetNumberOfStages()
	{
		return stages.Length;
	}
}
