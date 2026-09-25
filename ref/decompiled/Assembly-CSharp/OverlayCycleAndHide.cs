using UnityEngine;
using UnityEngine.UI;

public class OverlayCycleAndHide : MonoBehaviour
{
	[Tooltip("All overlay GameObjects, in the order you want to cycle through them.")]
	public GameObject[] overlays;

	[Tooltip("Optional label to show which overlay (or 'Off') is currently active.")]
	public Text label;

	private int currentIndex;

	private void Start()
	{
		currentIndex = overlays.Length;
		ApplyState();
	}

	public void CycleNext()
	{
		currentIndex = (currentIndex + 1) % (overlays.Length + 1);
		ApplyState();
	}

	private void ApplyState()
	{
		for (int i = 0; i < overlays.Length; i++)
		{
			if (overlays[i] != null)
			{
				overlays[i].SetActive(i == currentIndex);
			}
		}
		UpdateLabel();
	}

	private void UpdateLabel()
	{
		if (!(label == null))
		{
			label.text = ((currentIndex < overlays.Length && overlays[currentIndex] != null) ? overlays[currentIndex].name : "Off");
		}
	}
}
