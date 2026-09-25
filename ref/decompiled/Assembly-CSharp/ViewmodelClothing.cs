using System;
using Facepunch;
using UnityEngine;

public class ViewmodelClothing : MonoBehaviour
{
	[Flags]
	public enum SkinArea
	{
		Arms = 1,
		Hands = 2
	}

	public enum Slot
	{
		Under,
		Over
	}

	public SkeletonSkin[] SkeletonSkins;

	public bool DisableHandsEntirely;

	[Header("Conditional Logic")]
	public bool isConditional;

	[Tooltip("This is the slot the clothing will fit into. Over clothing will use MaxSkins when on top of under clothing.")]
	public Slot ClothingSlot;

	[Tooltip("A mask to allow multiple over clothing on the same model, but affecting different areas.")]
	public SkinArea AreasCovered = SkinArea.Arms | SkinArea.Hands;

	[Tooltip("Max Skins are the skins displayed if clothing is on top.")]
	public SkeletonSkin[] MaxSkin;
}
