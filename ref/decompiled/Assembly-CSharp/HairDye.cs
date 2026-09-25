using System;
using UnityEngine;

[Serializable]
public class HairDye
{
	public enum CopyProperty
	{
		Color,
		Metallic,
		Smoothness,
		Occlusion,
		DetailLayer,
		DetailBlendType,
		DetailAdditionalBlendSettings,
		DetailBlendFactor,
		DetailBlendFalloff,
		DetailColor,
		ApplyVertexColorStrength,
		ApplyVertexAlphaStrength,
		Count
	}

	[Flags]
	public enum CopyPropertyMask
	{
		Color = 1,
		Metallic = 2,
		Smoothness = 4,
		Occlusion = 8,
		DetailLayer = 0x10,
		DetailBlendType = 0x20,
		DetailAdditionalBlendSettings = 0x40,
		DetailBlendFactor = 0x80,
		DetailBlendFalloff = 0x100,
		DetailColor = 0x200,
		ApplyVertexColorStrength = 0x400,
		ApplyVertexAlphaStrength = 0x800
	}

	[ColorUsage(true, false)]
	public Color capBaseColor;

	public Material sourceMaterial;

	[InspectorFlags]
	public CopyPropertyMask copyProperties;

	private static MaterialPropertyDesc[] transferableProps = new MaterialPropertyDesc[12]
	{
		new MaterialPropertyDesc("_Color", typeof(Color)),
		new MaterialPropertyDesc("_Metallic", typeof(float)),
		new MaterialPropertyDesc("_Glossiness", typeof(float)),
		new MaterialPropertyDesc("_Occlusion", typeof(float)),
		new MaterialPropertyDesc("_DetailLayer", typeof(float)),
		new MaterialPropertyDesc("_DetailBlendType", typeof(int)),
		new MaterialPropertyDesc("_DetailAdditionalBlendSettings", typeof(float)),
		new MaterialPropertyDesc("_DetailBlendFactor", typeof(float)),
		new MaterialPropertyDesc("_DetailBlendFalloff", typeof(float)),
		new MaterialPropertyDesc("_DetailColor", typeof(Color)),
		new MaterialPropertyDesc("_ApplyVertexColorStrength", typeof(float)),
		new MaterialPropertyDesc("__ApplyVertexAlphaStrength", typeof(float))
	};

	public static int _HairBaseColorUV1 = Shader.PropertyToID("_HairBaseColorUV1");

	public static int _HairBaseColorUV2 = Shader.PropertyToID("_HairBaseColorUV2");

	public static int _HairPackedMapUV1 = Shader.PropertyToID("_HairPackedMapUV1");

	public static int _HairPackedMapUV2 = Shader.PropertyToID("_HairPackedMapUV2");

	public void Apply(HairDyeCollection collection, MaterialPropertyBlock block)
	{
		if (!(sourceMaterial != null))
		{
			return;
		}
		for (int i = 0; i < 12; i++)
		{
			if (((uint)copyProperties & (uint)(1 << i)) == 0)
			{
				continue;
			}
			MaterialPropertyDesc materialPropertyDesc = transferableProps[i];
			if (sourceMaterial.HasProperty(materialPropertyDesc.nameID))
			{
				if (materialPropertyDesc.type == typeof(Color))
				{
					block.SetColor(materialPropertyDesc.nameID, sourceMaterial.GetColor(materialPropertyDesc.nameID));
				}
				else if (materialPropertyDesc.type == typeof(float))
				{
					block.SetFloat(materialPropertyDesc.nameID, sourceMaterial.GetFloat(materialPropertyDesc.nameID));
				}
			}
		}
	}

	public void ApplyCap(HairDyeCollection collection, HairType type, MaterialPropertyBlock block)
	{
		if (collection.applyCap)
		{
			switch (type)
			{
			case HairType.Head:
			case HairType.Armpit:
			case HairType.Pubic:
				block.SetColor(_HairBaseColorUV1, capBaseColor);
				block.SetTexture(_HairPackedMapUV1, (collection.capMask != null) ? collection.capMask : Texture2D.blackTexture);
				break;
			case HairType.Facial:
				block.SetColor(_HairBaseColorUV2, capBaseColor);
				block.SetTexture(_HairPackedMapUV2, (collection.capMask != null) ? collection.capMask : Texture2D.blackTexture);
				break;
			}
		}
	}
}
