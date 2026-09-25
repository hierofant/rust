using System;
using UnityEngine;

public class VisualFoodSettings : PrefabAttribute, IClientComponent
{
	[Serializable]
	public class VisualFoodSetting
	{
		public GameObjectRef model;

		public GameObjectRef effectPrefab;

		public ItemDefinition[] items;

		public Transform[] parents;

		public Transform[] effects;

		[HideInInspector]
		public Vector3[] parentPositions;

		[HideInInspector]
		public Quaternion[] parentRotations;

		[HideInInspector]
		public Vector3[] parentScales;

		[HideInInspector]
		public Vector3[] effectParentPositions;

		[HideInInspector]
		public Quaternion[] effectParentRotations;

		[HideInInspector]
		public Vector3[] effectParentScales;

		public void ProcessSpawnPos(Transform rootTransform)
		{
			if (parents != null && parents.Length != 0)
			{
				parentPositions = new Vector3[parents.Length];
				parentRotations = new Quaternion[parents.Length];
				parentScales = new Vector3[parents.Length];
				for (int i = 0; i < parents.Length; i++)
				{
					if (parents[i] != null)
					{
						parentPositions[i] = parents[i].position;
						parentRotations[i] = parents[i].rotation;
						parentScales[i] = parents[i].localScale;
					}
				}
			}
			if (effects == null || effects.Length == 0)
			{
				return;
			}
			effectParentPositions = new Vector3[effects.Length];
			effectParentRotations = new Quaternion[effects.Length];
			effectParentScales = new Vector3[effects.Length];
			for (int j = 0; j < effects.Length; j++)
			{
				if (effects[j] != null)
				{
					effectParentPositions[j] = effects[j].position;
					effectParentRotations[j] = effects[j].rotation;
					effectParentScales[j] = effects[j].localScale;
				}
			}
		}
	}

	public Transform strippedParent;

	public VisualFoodSetting[] settings;

	public GameObjectRef[] slotPanModels;

	public Transform[] slotPanParents;

	[HideInInspector]
	public Vector3[] slotPanPositions;

	[HideInInspector]
	public Quaternion[] slotPanRotations;

	[HideInInspector]
	public Vector3[] slotPanScales;

	protected override Type GetIndexedType()
	{
		return typeof(VisualFoodSettings);
	}
}
