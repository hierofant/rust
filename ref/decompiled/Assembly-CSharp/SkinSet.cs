using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Skin Set")]
public class SkinSet : ScriptableObject
{
	public string Label;

	public Gradient SkinColour;

	public float MinSkinBrightness = 1f;

	public float MaxSkinBrightness = 1f;

	public float MinEyeBrightness = 1f;

	public float MaxEyeBrightness = 1f;

	public HairSetCollection HairCollection;

	[Header("Models")]
	public GameObjectRef Head;

	public GameObjectRef Torso;

	public GameObjectRef Legs;

	public GameObjectRef Feet;

	public GameObjectRef Hands;

	[Header("Bone Positions")]
	public Transform SkeletonRoot;

	public Vector3 LeftEyePosition;

	public Vector3 RightEyePosition;

	public Vector3 JawPosition;

	[Header("Censored Variants")]
	public GameObjectRef CensoredTorso;

	public GameObjectRef CensoredLegs;

	[Header("Materials")]
	public Material HeadMaterial;

	public Material BodyMaterial;

	public Material EyeMaterial;

	internal Color GetSkinColor(float skinNumber)
	{
		return SkinColour.Evaluate(skinNumber);
	}

	internal float GetSkinBrightness(float skinNumber)
	{
		return Mathf.Lerp(MinSkinBrightness, MaxSkinBrightness, skinNumber);
	}

	internal float GetEyeBrightness(float skinNumber)
	{
		return Mathf.Lerp(MinEyeBrightness, MaxEyeBrightness, skinNumber);
	}
}
