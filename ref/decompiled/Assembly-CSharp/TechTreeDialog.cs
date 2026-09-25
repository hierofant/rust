using Rust.UI;
using UnityEngine;

public class TechTreeDialog : UIDialog, IInventoryChanged
{
	private const string techTreeLevelPrefKey = "techTreeLevel";

	private TechTreeData[] dataOptions;

	public RustButton[] tierButtons;

	public GameObjectRef entryPrefab;

	public GameObjectRef groupPrefab;

	public GameObjectRef linePrefab;

	public RectTransform contents;

	public TechTreeSelectedNodeUI selectedNodeUI;

	public const float nodeSize = 128f;

	public const float gridSize = 64f;

	public GameObjectRef unlockEffect;

	public GameObjectRef multiUnlockEffect;

	public RustText scrapCount;

	private Vector2 startPos = Vector2.zero;

	public ScrollRectZoom zoom;

	public TechTreeData data
	{
		get
		{
			if (dataOptions == null)
			{
				return null;
			}
			return dataOptions[selectedDataIndex];
		}
	}

	private int selectedDataIndex
	{
		get
		{
			return PlayerPrefs.GetInt("techTreeLevel", 0);
		}
		set
		{
			PlayerPrefs.SetInt("techTreeLevel", value);
		}
	}
}
