using UnityEngine;

public class UIBackgroundBlur : ListComponent<UIBackgroundBlur>, IClientComponent
{
	[SerializeField]
	[Range(0f, 1f)]
	private float amount = 1f;

	[Range(0f, 10f)]
	[SerializeField]
	private float blurSize;

	public float Amount
	{
		get
		{
			return amount;
		}
		set
		{
			amount = Mathf.Clamp(value, 0f, 1f);
			base.enabled = amount > 0f;
		}
	}

	public static float GetCurrentMax(out float blurSize)
	{
		blurSize = 0f;
		if (ListComponent<UIBackgroundBlur>.InstanceList.Count == 0)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < ListComponent<UIBackgroundBlur>.InstanceList.Count; i++)
		{
			UIBackgroundBlur uIBackgroundBlur = ListComponent<UIBackgroundBlur>.InstanceList[i];
			num = Mathf.Max(uIBackgroundBlur.amount, num);
			blurSize = Mathf.Max(uIBackgroundBlur.blurSize, blurSize);
		}
		return num;
	}
}
