using System;
using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class SculptMeshPaintController : MonoBehaviour, IClientComponent
{
	[Serializable]
	public struct HitGuideVisuals
	{
		public GameObject CarveSphere;

		public GameObject CarveBox;

		public GameObject CarveCylinder;

		public GameObject CarveCapsule;

		public GameObject CarveCone;

		public GameObject CarveHexPrism;

		public GameObject AdditiveSphere;

		public GameObject AdditiveBox;

		public GameObject AdditiveCylinder;

		public GameObject AdditiveCapsule;

		public GameObject AdditiveCone;

		public GameObject AdditiveHexPrism;

		public GameObject CurrentGuide { get; set; }

		public void UpdateGuide(SculptingToolData.CarvingMode mode, SculptingToolData.CarvingShapeType shape, bool invert)
		{
			GameObject guide = GetGuide(mode, shape, invert);
			if (guide != CurrentGuide && (bool)CurrentGuide)
			{
				CurrentGuide.SetActive(value: false);
			}
			CurrentGuide = guide;
		}

		public GameObject GetGuide(SculptingToolData.CarvingMode mode, SculptingToolData.CarvingShapeType shape, bool invert)
		{
			return mode switch
			{
				SculptingToolData.CarvingMode.Carve => shape switch
				{
					SculptingToolData.CarvingShapeType.Sphere => CarveSphere, 
					SculptingToolData.CarvingShapeType.Rectangle => CarveBox, 
					SculptingToolData.CarvingShapeType.Cylinder => CarveCylinder, 
					SculptingToolData.CarvingShapeType.Capsule => CarveCapsule, 
					SculptingToolData.CarvingShapeType.Cone => CarveCone, 
					SculptingToolData.CarvingShapeType.HexagonalPrism => CarveHexPrism, 
					_ => CarveSphere, 
				}, 
				SculptingToolData.CarvingMode.Additive => shape switch
				{
					SculptingToolData.CarvingShapeType.Sphere => AdditiveSphere, 
					SculptingToolData.CarvingShapeType.Rectangle => AdditiveBox, 
					SculptingToolData.CarvingShapeType.Cylinder => AdditiveCylinder, 
					SculptingToolData.CarvingShapeType.Capsule => AdditiveCapsule, 
					SculptingToolData.CarvingShapeType.Cone => AdditiveCone, 
					SculptingToolData.CarvingShapeType.HexagonalPrism => AdditiveHexPrism, 
					_ => AdditiveSphere, 
				}, 
				SculptingToolData.CarvingMode.PushPull => invert ? CarveSphere : AdditiveSphere, 
				SculptingToolData.CarvingMode.Slab => CarveBox, 
				SculptingToolData.CarvingMode.ClipCurve => CarveBox, 
				SculptingToolData.CarvingMode.Smooth => AdditiveSphere, 
				_ => CarveSphere, 
			};
		}
	}

	public SculptingToolData.CarvingMode carvingMode;

	public SculptingToolData.CarvingShapeType carvingShapeType;

	public Camera pickerCamera;

	public float brushSpacing = 4f;

	public MeshPaintController.RotateMode brushRotation;

	public bool applyDefaults;

	public float defaultBrushSize = 16f;

	public float pushPullRepeatInterval = 0.025f;

	public RustButton UndoButton;

	public RustButton RedoButton;

	public RustButton vertexColorsButton;

	public GameObject BackgroundBlocker;

	public Toggle CarveToggle;

	public Toggle AdditiveToggle;

	public Toggle PushPullToggle;

	public Toggle SlabToggle;

	public Toggle ClipCurveToggle;

	public Toggle SmoothToggle;

	public float AutoSaveTimeInterval = 30f;

	public HitGuideVisuals HitGuide;
}
