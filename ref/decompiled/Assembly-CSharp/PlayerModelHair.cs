using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class PlayerModelHair : MonoBehaviour
{
	public struct RendererMaterials
	{
		public string[] names;

		public Material[] original;

		public Material[] replacement;

		public RendererMaterials(Renderer r)
		{
			original = r.sharedMaterials;
			replacement = original.Clone() as Material[];
			names = new string[original.Length];
			for (int i = 0; i < original.Length; i++)
			{
				names[i] = original[i].name;
			}
		}
	}

	public HairType type;

	private Dictionary<Renderer, RendererMaterials> materials;

	public Dictionary<Renderer, RendererMaterials> Materials => materials;

	private void CacheOriginalMaterials()
	{
		if (materials != null)
		{
			return;
		}
		List<SkinnedMeshRenderer> obj = Pool.Get<List<SkinnedMeshRenderer>>();
		base.gameObject.GetComponentsInChildren(includeInactive: true, obj);
		materials = new Dictionary<Renderer, RendererMaterials>();
		materials.Clear();
		foreach (SkinnedMeshRenderer item in obj)
		{
			materials.Add(item, new RendererMaterials(item));
		}
		Pool.FreeUnmanaged(ref obj);
	}

	private void Setup(HairType type, HairSetCollection hair, int meshIndex, float typeNum, float dyeNum, MaterialPropertyBlock block)
	{
		CacheOriginalMaterials();
		HairSetCollection.HairSetEntry hairSetEntry = hair.Get(type, typeNum);
		if (hairSetEntry.HairSet == null)
		{
			Debug.LogWarning("Hair.Get returned a NULL hair");
			return;
		}
		int blendShapeIndex = -1;
		if (type == HairType.Facial || type == HairType.Eyebrow)
		{
			blendShapeIndex = meshIndex;
		}
		HairDye dye = null;
		HairDyeCollection hairDyeCollection = hairSetEntry.HairDyeCollection;
		if (hairDyeCollection != null)
		{
			dye = hairDyeCollection.Get(dyeNum);
		}
		hairSetEntry.HairSet.Process(this, hairDyeCollection, dye, block);
		hairSetEntry.HairSet.ProcessMorphs(base.gameObject, blendShapeIndex);
	}

	public void Setup(SkinSetCollection skin, float hairNum, float meshNum, float dyeNum, float meshSubNumber, MaterialPropertyBlock block)
	{
		int index = skin.GetIndex(meshNum);
		SkinSet skinSet = skin.Get(meshNum, meshSubNumber);
		if (skinSet == null)
		{
			Debug.LogError("Skin.Get returned a NULL skin");
		}
		else if (!(skinSet.HairCollection == null))
		{
			int typeIndex = (int)type;
			GetRandomVariation(hairNum, typeIndex, dyeNum, out var typeNum, out var dyeNum2);
			Setup(type, skinSet.HairCollection, index, typeNum, dyeNum2, block);
		}
	}

	public static void GetRandomVariation(float hairNum, int typeIndex, float dyeNumber, out float typeNum, out float dyeNum)
	{
		typeNum = GetRandomHairType(hairNum, typeIndex);
		dyeNum = dyeNumber;
	}

	public static float GetRandomHairType(float hairNum, int typeIndex)
	{
		Random.InitState(Mathf.FloorToInt(hairNum * 100000f) + typeIndex);
		return Random.Range(0f, 1f);
	}
}
