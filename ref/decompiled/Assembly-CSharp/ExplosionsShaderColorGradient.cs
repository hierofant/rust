using UnityEngine;

public class ExplosionsShaderColorGradient : MonoBehaviour, IClientComponent
{
	public string ShaderProperty = "_TintColor";

	public int MaterialID;

	public Gradient Color = new Gradient();

	public float TimeMultiplier = 1f;
}
