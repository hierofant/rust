using System.Collections.Generic;
using Facepunch.Flexbox;
using UnityEngine;
using UnityEngine.UI;

public class FlexVirtualScroll : MonoBehaviour
{
	public interface IVisualUpdate
	{
		void OnVisualUpdate(int i, GameObject obj);
	}

	[Tooltip("Align content to the bottom of the viewport when it doesn't fill the full height")]
	public bool BottomUp;

	[Tooltip("Optional, we'll try to GetComponent IDataSource from this object on awake")]
	public GameObject DataSourceObject;

	public GameObject SourceObject;

	public GameObjectRef PrefabRef;

	public ScrollRect ScrollRect;

	public FlexElement FlexContentRoot;

	public FlexGridsElement FlexGrid;

	public FlexColumnsElement FlexColumns;

	public int ItemHeight;

	public int ItemsPerLine = 1;

	public int Gap;

	[Tooltip("Extra items to keep loaded above/below the viewport, as a fraction of viewport height. 0.3 = 30% of viewport.")]
	public float OverscanFactor = 0.3f;

	public FlexElement topSpacer;

	public FlexElement bottomSpacer;

	[Tooltip("Objects that are already spawned and in editor rather than being instantiated at runtime")]
	public List<GameObject> PreloadObjects = new List<GameObject>();

	[Tooltip("Instead of disabling gameobjects, hide them via CanvasGroup and disable their FlexElement")]
	public bool HideInsteadOfDisabling;
}
