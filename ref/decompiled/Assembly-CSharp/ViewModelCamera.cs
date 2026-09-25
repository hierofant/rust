using UnityEngine;

public class ViewModelCamera : MonoBehaviour
{
	[SerializeField]
	private Camera overrideCamera;

	[SerializeField]
	private bool stencilCutoutAware;

	public bool StencilCutoutAware => stencilCutoutAware;
}
