using UnityEngine;

public class LightOccludee : MonoBehaviour, IClientComponent
{
	public float RadiusScale = 0.5f;

	public float MinTimeVisible = 0.1f;

	public bool IsDynamic;
}
