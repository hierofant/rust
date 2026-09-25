using Facepunch.Flexbox;
using Rust.UI;
using UnityEngine;

public class SculptingDialogue : UIDialog
{
	[Header("Sculpting Dialogue")]
	public GameObject objectContainer;

	public Transform panTransform;

	public Transform rotateTransform;

	public Canvas canvas;

	public RectTransform rightPanelRect;

	public Camera cameraPreview;

	public Light previewLight;

	public Vector3 homeRotation;

	public RectTransform toolsContainer;

	public RectTransform brushesContainer;

	public RustSlider brushSizeSlider;

	public RustSlider brushSpacingSlider;

	public RustSlider brushOpacitySlider;

	public RustButton baseplateViewButton;

	public SculptMeshPaintController sculptMeshPaintController;

	[Header("Layout")]
	public FlexElement rootElement;

	public FlexElement bodyElement;

	public FlexElement controlsElement;

	public FlexElement floatElement;

	public FlexElement shapeSelectionElement;
}
