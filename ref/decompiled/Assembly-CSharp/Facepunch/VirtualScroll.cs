using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Facepunch;

public class VirtualScroll : MonoBehaviour
{
	public interface IDataSource
	{
		int GetItemCount();

		float GetItemSize(int i);

		void SetItemData(int i, GameObject obj);
	}

	public interface IVisualUpdate
	{
		void OnVisualUpdate(int i, GameObject obj);
	}

	public int ItemHeight = 40;

	public int ItemSpacing = 10;

	public RectOffset Padding;

	[Tooltip("Optional, we'll try to GetComponent IDataSource from this object on awake")]
	public GameObject DataSourceObject;

	public GameObject SourceObject;

	public ScrollRect ScrollRect;

	public RectTransform OverrideContentRoot;

	private IDataSource dataSource;

	private Dictionary<int, GameObject> ActivePool = new Dictionary<int, GameObject>();

	private Stack<GameObject> InactivePool = new Stack<GameObject>();

	private int BlockHeight => ItemHeight + ItemSpacing;

	public void Awake()
	{
		ScrollRect.onValueChanged.AddListener(OnScrollChanged);
		if (DataSourceObject != null)
		{
			SetDataSource(DataSourceObject.GetComponent<IDataSource>());
		}
	}

	public void OnDestroy()
	{
		ScrollRect.onValueChanged.RemoveListener(OnScrollChanged);
	}

	[UnityEvent]
	private void OnScrollChanged(Vector2 pos)
	{
		Rebuild();
	}

	public void SetDataSource(IDataSource source, bool forceRebuild = false)
	{
		if (dataSource != source || forceRebuild)
		{
			dataSource = source;
			FullRebuild();
		}
	}

	private float GetItemHeight(int i)
	{
		if (dataSource != null)
		{
			float itemSize = dataSource.GetItemSize(i);
			if (itemSize > 0f)
			{
				return itemSize;
			}
		}
		return BlockHeight;
	}

	public void FullRebuild()
	{
		int[] array = ActivePool.Keys.ToArray();
		foreach (int key in array)
		{
			Recycle(key);
		}
		Rebuild();
	}

	public void DataChanged()
	{
		foreach (KeyValuePair<int, GameObject> item in ActivePool)
		{
			dataSource.SetItemData(item.Key, item.Value);
		}
		Rebuild();
	}

	protected virtual float GetContentHeight(int itemCount)
	{
		float result = BlockHeight * itemCount - ItemSpacing + Padding.top + Padding.bottom;
		if (dataSource == null)
		{
			return result;
		}
		float num = Padding.top + Padding.bottom - ItemSpacing;
		for (int i = 0; i < itemCount; i++)
		{
			num += GetItemHeight(i) + (float)ItemSpacing;
		}
		return num;
	}

	protected virtual float SetCanvasSize(int items)
	{
		RectTransform obj = ((OverrideContentRoot != null) ? OverrideContentRoot : (ScrollRect.viewport.GetChild(0) as RectTransform));
		obj.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, GetContentHeight(items));
		return obj.anchoredPosition.y;
	}

	public void Rebuild()
	{
		if (dataSource == null)
		{
			return;
		}
		int itemCount = dataSource.GetItemCount();
		if (itemCount <= 0)
		{
			return;
		}
		float num = SetCanvasSize(itemCount);
		int num2 = Mathf.Max(2, Mathf.CeilToInt(ScrollRect.viewport.rect.height / (float)BlockHeight));
		int num3 = Mathf.FloorToInt((num - (float)Padding.top) / (float)BlockHeight);
		int num4 = num3 + num2;
		RecycleOutOfRange(num3, num4);
		for (int i = num3; i <= num4; i++)
		{
			if (i >= 0 && i < itemCount)
			{
				BuildItem(i);
			}
		}
	}

	public void Update()
	{
		if (!(dataSource is IVisualUpdate visualUpdate))
		{
			return;
		}
		foreach (KeyValuePair<int, GameObject> item in ActivePool)
		{
			visualUpdate.OnVisualUpdate(item.Key, item.Value);
		}
	}

	private void RecycleOutOfRange(int startVisible, float endVisible)
	{
		int[] array = (from x in ActivePool.Keys
			where x < startVisible || (float)x > endVisible
			select (x)).ToArray();
		foreach (int key in array)
		{
			Recycle(key);
		}
	}

	private void Recycle(int key)
	{
		GameObject gameObject = ActivePool[key];
		gameObject.SetActive(value: false);
		ActivePool.Remove(key);
		InactivePool.Push(gameObject);
	}

	private void BuildItem(int i)
	{
		if (i >= 0 && !ActivePool.ContainsKey(i))
		{
			GameObject item = GetItem();
			item.SetActive(value: true);
			dataSource.SetItemData(i, item);
			RectTransform obj = item.transform as RectTransform;
			obj.anchorMin = new Vector2(0f, 1f);
			obj.anchorMax = new Vector2(1f, 1f);
			obj.pivot = new Vector2(0.5f, 1f);
			obj.offsetMin = new Vector2(0f, 0f);
			float itemHeight = GetItemHeight(i);
			obj.offsetMax = new Vector2(0f, itemHeight);
			obj.sizeDelta = new Vector2((Padding.left + Padding.right) * -1, itemHeight);
			obj.anchoredPosition = new Vector2((float)(Padding.left - Padding.right) * 0.5f, -1 * (i * BlockHeight + Padding.top));
			ActivePool[i] = item;
		}
	}

	private GameObject GetItem()
	{
		if (InactivePool.Count == 0)
		{
			GameObject gameObject = Object.Instantiate(SourceObject);
			gameObject.transform.SetParent((OverrideContentRoot != null) ? OverrideContentRoot : ScrollRect.viewport.GetChild(0), worldPositionStays: false);
			gameObject.transform.localScale = Vector3.one;
			gameObject.SetActive(value: false);
			InactivePool.Push(gameObject);
		}
		return InactivePool.Pop();
	}
}
