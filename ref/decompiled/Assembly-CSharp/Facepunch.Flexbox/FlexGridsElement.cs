using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Facepunch.Flexbox;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class FlexGridsElement : FlexElementBase
{
	private struct GridSlot
	{
		public IFlexNode Child;

		public int X;

		public int Y;

		public int SpanX;

		public int SpanY;
	}

	[Tooltip("Spacing to add from this elements borders to where children are laid out.")]
	public FlexPadding Padding;

	[Min(0f)]
	[Tooltip("Spacing to add between each child flex item.")]
	public float Gap;

	[Min(1f)]
	[Tooltip("The number of columns to use when using a fixed number of columns.")]
	public int ColumnCount = 1;

	[Min(1f)]
	[Tooltip("The minimum width of each column when not using a fixed number of columns.")]
	[FormerlySerializedAs("ColumnWidth")]
	public int ColumnMinWidth = 100;

	public bool FixedRowCount;

	[Min(1f)]
	public int RowCount = 1;

	[Min(1f)]
	public int RowMinHeight = 100;

	public bool BestFitOrdering;

	private bool[,] _gridMapBuffer = new bool[32, 32];

	private int _gridMapColumns = 32;

	private int _gridMapRows = 32;

	public bool[,] GridMapBuffer => _gridMapBuffer;

	public void EnsureGridMapSize(int columns, int rows)
	{
		if (_gridMapBuffer == null || _gridMapColumns < columns || _gridMapRows < rows)
		{
			_gridMapBuffer = new bool[columns, rows];
			_gridMapColumns = columns;
			_gridMapRows = rows;
		}
		else
		{
			Array.Clear(_gridMapBuffer, 0, _gridMapBuffer.Length);
		}
	}

	protected override void MeasureHorizontalImpl()
	{
		float num = 0f;
		int num2 = Mathf.Min(ColumnCount, Mathf.Max(1, Children.Count));
		num = (float)(num2 * ColumnMinWidth) + (float)Mathf.Max(num2 - 1, 0) * Gap;
		foreach (IFlexNode child in Children)
		{
			if (child.IsDirty)
			{
				child.MeasureHorizontal();
			}
		}
		float b = ((Basis.HasValue && Basis.Unit == FlexUnit.Pixels) ? Basis.Value : 0f);
		float a = ((MinWidth.HasValue && MinWidth.Unit == FlexUnit.Pixels) ? MinWidth.Value : 0f);
		float max = ((MaxWidth.HasValue && MaxWidth.Unit == FlexUnit.Pixels) ? MaxWidth.Value : float.PositiveInfinity);
		float num3 = Padding.left + Padding.right;
		PrefWidth = Mathf.Clamp(num + num3, Mathf.Max(a, b), max);
	}

	protected override void LayoutHorizontalImpl(float maxWidth, float maxHeight)
	{
		float num = maxWidth - Padding.left - Padding.right;
		float fillValue = maxHeight - Padding.top - Padding.bottom;
		float num2 = (num - Gap * (float)(ColumnCount - 1)) / (float)ColumnCount;
		float num3 = RowMinHeight;
		if (BestFitOrdering)
		{
			ReOrderChildren(num, num2);
		}
		int num4 = (FixedRowCount ? RowCount : 100);
		EnsureGridMapSize(ColumnCount, num4);
		Array.Clear(_gridMapBuffer, 0, _gridMapBuffer.Length);
		List<GridSlot> obj = Pool.Get<List<GridSlot>>();
		foreach (IFlexNode child in Children)
		{
			FlexLength length = child.MinWidth;
			int num5 = Mathf.Clamp(Mathf.CeilToInt(FlexElementBase.CalculateLengthValue(in length, num, 0f) / (num2 + Gap)), 1, ColumnCount);
			length = child.MinHeight;
			int num6 = Mathf.Clamp(Mathf.CeilToInt(FlexElementBase.CalculateLengthValue(in length, fillValue, 0f) / (num3 + Gap)), 1, num4);
			bool flag = false;
			for (int i = 0; i < num4; i++)
			{
				for (int j = 0; j <= ColumnCount - num5; j++)
				{
					if (CanPlaceAt(j, i, num5, num6, _gridMapBuffer))
					{
						MarkUsed(j, i, num5, num6, _gridMapBuffer);
						obj.Add(new GridSlot
						{
							Child = child,
							X = j,
							Y = i,
							SpanX = num5,
							SpanY = num6
						});
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
		}
		foreach (GridSlot item in obj)
		{
			RectTransform rectTransform = item.Child.Transform;
			float x = Padding.left + (float)item.X * (num2 + Gap);
			float num7 = Padding.top + (float)item.Y * (num3 + Gap);
			float num8 = (float)item.SpanX * num2 + (float)(item.SpanX - 1) * Gap;
			float num9 = (float)item.SpanY * num3 + (float)(item.SpanY - 1) * Gap;
			item.Child.LayoutHorizontal(num8, float.PositiveInfinity);
			item.Child.LayoutVertical(float.PositiveInfinity, num9);
			rectTransform.anchoredPosition = new Vector2(x, 0f - num7);
			rectTransform.sizeDelta = new Vector2(num8, num9);
		}
		Pool.FreeUnmanaged(ref obj);
	}

	public static bool CanPlaceAt(int xPos, int yPos, int sizeX, int sizeY, bool[,] grid)
	{
		for (int i = 0; i < sizeY; i++)
		{
			for (int j = 0; j < sizeX; j++)
			{
				if (grid[xPos + j, yPos + i])
				{
					return false;
				}
			}
		}
		return true;
	}

	public static void MarkUsed(int xPos, int yPos, int sizeX, int sizeY, bool[,] grid)
	{
		for (int i = 0; i < sizeY; i++)
		{
			for (int j = 0; j < sizeX; j++)
			{
				grid[xPos + j, yPos + i] = true;
			}
		}
	}

	private void ReOrderChildren(float innerWidth, float columnWidth)
	{
		List<GridSlot> list = Children.Select(delegate(IFlexNode child)
		{
			FlexLength length = child.MinWidth;
			float min = FlexElementBase.CalculateLengthValue(in length, innerWidth, 0f);
			length = child.MaxWidth;
			float max = FlexElementBase.CalculateLengthValue(in length, innerWidth, float.PositiveInfinity);
			float num3 = Mathf.Clamp(columnWidth, min, max);
			length = child.MinHeight;
			float num4 = Mathf.Clamp(min: FlexElementBase.CalculateLengthValue(in length, float.PositiveInfinity, 0f), value: RowMinHeight, max: float.PositiveInfinity);
			int spanX = Mathf.Clamp(Mathf.CeilToInt((num3 + Gap) / (columnWidth + Gap)), 1, ColumnCount);
			int spanY = Mathf.Clamp(Mathf.CeilToInt((num4 + Gap) / ((float)RowMinHeight + Gap)), 1, 64);
			GridSlot result = default(GridSlot);
			result.Child = child;
			result.SpanX = spanX;
			result.SpanY = spanY;
			return result;
		}).ToList();
		list.Sort(delegate(GridSlot a, GridSlot b)
		{
			int value = a.SpanX * a.SpanY;
			return (b.SpanX * b.SpanY).CompareTo(value);
		});
		List<IFlexNode> obj = Pool.Get<List<IFlexNode>>();
		while (list.Count > 0)
		{
			int num = ColumnCount;
			int num2 = 0;
			while (num2 < list.Count)
			{
				if (list[num2].SpanX <= num)
				{
					obj.Add(list[num2].Child);
					num -= list[num2].SpanX;
					list.RemoveAt(num2);
				}
				else
				{
					num2++;
				}
			}
		}
		for (int i = 0; i < obj.Count; i++)
		{
			obj[i].Transform.SetSiblingIndex(i);
		}
		Pool.FreeUnmanaged(ref obj);
	}

	protected override void MeasureVerticalImpl()
	{
		float num;
		if (FixedRowCount)
		{
			num = (float)(RowCount * RowMinHeight) + (float)Mathf.Max(RowCount - 1, 0) * Gap;
		}
		else
		{
			int[] array = new int[ColumnCount];
			int num2 = 0;
			foreach (IFlexNode child in Children)
			{
				if (child.IsDirty)
				{
					child.MeasureVertical();
				}
				FlexLength length = child.MinWidth;
				float num3 = FlexElementBase.CalculateLengthValue(in length, 0f, 0f);
				length = child.MinHeight;
				float num4 = FlexElementBase.CalculateLengthValue(in length, 0f, 0f);
				int num5 = Mathf.Clamp(Mathf.CeilToInt((num3 + Gap) / ((float)ColumnMinWidth + Gap)), 1, ColumnCount);
				int num6 = Mathf.Clamp(Mathf.CeilToInt((num4 + Gap) / ((float)RowMinHeight + Gap)), 1, 64);
				int num7 = -1;
				int num8 = int.MaxValue;
				for (int i = 0; i <= ColumnCount - num5; i++)
				{
					int num9 = 0;
					for (int j = 0; j < num5; j++)
					{
						num9 = Mathf.Max(num9, array[i + j]);
					}
					if (num9 < num8)
					{
						num8 = num9;
						num7 = i;
					}
				}
				if (num7 >= 0)
				{
					for (int k = 0; k < num5; k++)
					{
						array[num7 + k] = num8 + num6;
					}
					num2 = Mathf.Max(num2, num8 + num6);
				}
			}
			num = (float)(num2 * RowMinHeight) + (float)Mathf.Max(num2 - 1, 0) * Gap;
		}
		float b = ((Basis.HasValue && Basis.Unit == FlexUnit.Pixels) ? Basis.Value : 0f);
		float a = ((MinHeight.HasValue && MinHeight.Unit == FlexUnit.Pixels) ? MinHeight.Value : 0f);
		float max = ((MaxHeight.HasValue && MaxHeight.Unit == FlexUnit.Pixels) ? MaxHeight.Value : float.PositiveInfinity);
		float num10 = Padding.top + Padding.bottom;
		PrefHeight = Mathf.Clamp(num + num10, Mathf.Max(a, b), max);
	}

	protected override void LayoutVerticalImpl(float maxWidth, float maxHeight)
	{
	}
}
