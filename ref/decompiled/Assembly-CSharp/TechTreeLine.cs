using UnityEngine;
using UnityEngine.UI;

public class TechTreeLine : TechTreeWidget
{
	public RawImage centerSelected;

	public RawImage topLeftSelected;

	public RawImage topRightSelected;

	public RawImage bottomLeftSelected;

	public RawImage bottomRightSelected;

	public RawImage centerLocked;

	public RawImage topLeftLocked;

	public RawImage topRightLocked;

	public RawImage bottomLeftLocked;

	public RawImage bottomRightLocked;

	public int from;

	public int to;

	public Color selectedColor;

	public Color lockedColor;
}
