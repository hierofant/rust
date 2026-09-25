using UnityEngine;

public class DigitController : MonoBehaviour
{
	public MeshRenderer[] AllDigits;

	public void SetDigitActive(int digit)
	{
		for (int i = 0; i < AllDigits.Length; i++)
		{
			AllDigits[i].gameObject.SetActive(i == digit);
		}
	}

	public void SetAllDigitsTransparency(float normalizedAlpha, MaterialPropertyBlock materialPropertyBlock, int colorProperty)
	{
		for (int i = 0; i < AllDigits.Length; i++)
		{
			SetDigitTransparency(i, normalizedAlpha, materialPropertyBlock, colorProperty);
		}
	}

	public void SetDigitTransparency(int digit, float normalizedAlpha, MaterialPropertyBlock materialPropertyBlock, int colorProperty)
	{
		MeshRenderer meshRenderer = AllDigits[digit];
		meshRenderer.GetPropertyBlock(materialPropertyBlock);
		Color color = materialPropertyBlock.GetColor(colorProperty);
		color.a = Mathf.Lerp(0f, meshRenderer.sharedMaterial.color.a, normalizedAlpha);
		materialPropertyBlock.SetColor(colorProperty, color);
		meshRenderer.SetPropertyBlock(materialPropertyBlock);
	}
}
