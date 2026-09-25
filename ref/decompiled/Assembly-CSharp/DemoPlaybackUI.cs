using UnityEngine;

public class DemoPlaybackUI : SingletonComponent<DemoPlaybackUI>
{
	private const float ToolkitSortingOrder = 100f;

	public Canvas canvas;

	public GameObject Root;
}
