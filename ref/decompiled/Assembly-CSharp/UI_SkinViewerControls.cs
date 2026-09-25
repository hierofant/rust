using UnityEngine;

public class UI_SkinViewerControls : MonoBehaviour
{
	public Canvas canvas;

	[SerializeField]
	private CoverImage coverImage;

	[SerializeField]
	[Header("Parallax")]
	private float maxYaw = 8f;

	[SerializeField]
	private float maxPitch = 4f;

	[SerializeField]
	private float lerpSpeed = 6f;

	[SerializeField]
	private float responseCurve = 1.5f;

	[Space]
	[SerializeField]
	private bool fullScreenOnly;
}
