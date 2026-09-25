using UnityEngine;
using UnityEngine.Events;

namespace Rust.Components.Utility;

internal class OnObjectEnable : MonoBehaviour
{
	public UnityEvent Action;

	private void OnEnable()
	{
		Action.Invoke();
	}
}
