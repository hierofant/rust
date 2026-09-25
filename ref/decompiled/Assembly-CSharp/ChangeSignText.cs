using System;
using Facepunch.Flexbox;
using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class ChangeSignText : UIDialog
{
	public Action<int, Texture2D> onUpdateTexture;

	public Action onClose;

	public GameObject objectContainer;

	public Transform panTransform;

	public Transform rotateTransform;

	public ObjectRotation objectRotator;

	public Transform rotationVisualizerTransform;

	public GameObject currentFrameSection;

	public GameObject[] frameOptions;

	public Canvas canvas;

	public RectTransform rightPanelRect;

	public Camera cameraPreview;

	public Camera camera3D;

	public Light previewLight;

	public Vector3 homeRotation;

	public RectTransform toolsContainer;

	public RectTransform brushesContainer;

	public RustSlider brushSizeSlider;

	public RustSlider brushSpacingSlider;

	public RustSlider brushOpacitySlider;

	public GameObject chatToggleButton;

	public Toggle autoSaveToggle;

	public RustButton easelButton;

	public RustButton hideObjectButton;

	[Header("Layout")]
	public FlexElement rootElement;

	public FlexElement bodyElement;

	public FlexElement controlsElement;

	public FlexElement floatElement;

	[Header("Censor Save Warning")]
	public GameObject censorSaveWarningPopup;
}
