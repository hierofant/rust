using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
[ExecuteAlways]
public class UIBorder : MonoBehaviour
{
	private const string ChildName = "_UIBorder";

	[SerializeField]
	private float top;

	[SerializeField]
	private float right;

	[SerializeField]
	private float bottom;

	[SerializeField]
	private float left;

	[SerializeField]
	private Color color = Color.white;

	[SerializeField]
	private float topLeftRadius;

	[SerializeField]
	private float topRightRadius;

	[SerializeField]
	private float bottomRightRadius;

	[SerializeField]
	private float bottomLeftRadius;

	[SerializeField]
	[Range(1f, 32f)]
	private int segmentsPerCorner = 8;

	[SerializeField]
	[HideInInspector]
	private BorderGraphic graphic;

	public float Top
	{
		get
		{
			return top;
		}
		set
		{
			if (top != value)
			{
				top = value;
				Sync();
			}
		}
	}

	public float Right
	{
		get
		{
			return right;
		}
		set
		{
			if (right != value)
			{
				right = value;
				Sync();
			}
		}
	}

	public float Bottom
	{
		get
		{
			return bottom;
		}
		set
		{
			if (bottom != value)
			{
				bottom = value;
				Sync();
			}
		}
	}

	public float Left
	{
		get
		{
			return left;
		}
		set
		{
			if (left != value)
			{
				left = value;
				Sync();
			}
		}
	}

	public Color Color
	{
		get
		{
			return color;
		}
		set
		{
			if (color != value)
			{
				color = value;
				Sync();
			}
		}
	}

	public float TopLeftRadius
	{
		get
		{
			return topLeftRadius;
		}
		set
		{
			if (topLeftRadius != value)
			{
				topLeftRadius = value;
				Sync();
			}
		}
	}

	public float TopRightRadius
	{
		get
		{
			return topRightRadius;
		}
		set
		{
			if (topRightRadius != value)
			{
				topRightRadius = value;
				Sync();
			}
		}
	}

	public float BottomRightRadius
	{
		get
		{
			return bottomRightRadius;
		}
		set
		{
			if (bottomRightRadius != value)
			{
				bottomRightRadius = value;
				Sync();
			}
		}
	}

	public float BottomLeftRadius
	{
		get
		{
			return bottomLeftRadius;
		}
		set
		{
			if (bottomLeftRadius != value)
			{
				bottomLeftRadius = value;
				Sync();
			}
		}
	}

	public int SegmentsPerCorner
	{
		get
		{
			return segmentsPerCorner;
		}
		set
		{
			if (segmentsPerCorner != value)
			{
				segmentsPerCorner = value;
				Sync();
			}
		}
	}

	private void OnEnable()
	{
		EnsureGraphic();
		Sync();
	}

	private void OnTransformChildrenChanged()
	{
		if (graphic != null)
		{
			graphic.transform.SetAsLastSibling();
		}
	}

	private void EnsureGraphic()
	{
		if (graphic == null || graphic.transform.parent != base.transform)
		{
			Transform transform = null;
			for (int i = 0; i < base.transform.childCount; i++)
			{
				Transform child = base.transform.GetChild(i);
				if (child.name == "_UIBorder")
				{
					transform = child;
					break;
				}
			}
			if (transform != null)
			{
				graphic = transform.GetComponent<BorderGraphic>();
				if (graphic == null)
				{
					graphic = transform.gameObject.AddComponent<BorderGraphic>();
				}
			}
			else
			{
				GameObject gameObject = new GameObject("_UIBorder", typeof(RectTransform), typeof(BorderGraphic));
				RectTransform obj = (RectTransform)gameObject.transform;
				obj.SetParent(base.transform, worldPositionStays: false);
				obj.anchorMin = Vector2.zero;
				obj.anchorMax = Vector2.one;
				obj.offsetMin = Vector2.zero;
				obj.offsetMax = Vector2.zero;
				graphic = gameObject.GetComponent<BorderGraphic>();
			}
		}
		graphic.gameObject.hideFlags = HideFlags.HideInHierarchy;
		graphic.raycastTarget = false;
		graphic.transform.SetAsLastSibling();
	}

	private void Sync()
	{
		if (!(graphic == null))
		{
			graphic.SetSides(top, right, bottom, left, color);
			graphic.SetCorners(topLeftRadius, topRightRadius, bottomRightRadius, bottomLeftRadius, segmentsPerCorner);
		}
	}
}
