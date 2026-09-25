using Development.Attributes;
using UnityEngine;

[ResetStaticFields]
public class LightLOD : MonoBehaviour, ILOD, IClientComponent
{
	public float DistanceBias;

	public bool ToggleLight;

	public bool ToggleShadows = true;

	[SerializeField]
	private ShadowCacher shadowCacher = new ShadowCacher();

	protected void OnValidate()
	{
		LightEx.CheckConflict(base.gameObject);
	}
}
