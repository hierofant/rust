using UnityEngine;

public class UISleepingScreen : SingletonComponent<UISleepingScreen>, IUIScreen
{
	protected Canvas canvas;

	protected CanvasGroup canvasGroup;

	private bool visible;

	protected override void Awake()
	{
		base.Awake();
		canvasGroup = GetComponent<CanvasGroup>();
		canvas = GetComponent<Canvas>();
		visible = true;
	}

	public void SetVisible(bool b)
	{
		if (visible != b)
		{
			visible = b;
			if (canvas != null)
			{
				canvas.enabled = b;
			}
			canvasGroup.alpha = (visible ? 1f : 0f);
		}
	}
}
