using UnityEngine;
using UnityEngine.UI;

namespace Rust.UI.MainMenu;

[RequireComponent(typeof(RectTransform))]
public class UI_FoilShaderMouseFollow : MonoBehaviour
{
	[Header("Foil Shader Settings")]
	[SerializeField]
	private RawImage _rawImage;

	[SerializeField]
	private Image _image;

	[SerializeField]
	private float _rotationSpeed = 10f;

	[SerializeField]
	private float _tiltSensitivity = 3f;

	[SerializeField]
	private float _maxTilt = 10f;

	[Header("Global Settings")]
	[SerializeField]
	private bool _useGlobal;

	[SerializeField]
	[Range(0f, 1f)]
	private float _normalisedMaxGlobalDistance;
}
