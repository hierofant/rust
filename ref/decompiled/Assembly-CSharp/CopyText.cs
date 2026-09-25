using Rust.UI;
using UnityEngine;

public class CopyText : MonoBehaviour
{
	public RustText TargetText;

	[UnityEvent]
	public void TriggerCopy()
	{
		if (TargetText != null)
		{
			GUIUtility.systemCopyBuffer = TargetText.text;
		}
	}
}
