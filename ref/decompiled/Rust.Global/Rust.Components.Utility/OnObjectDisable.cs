using UnityEngine;
using UnityEngine.Events;

namespace Rust.Components.Utility;

internal class OnObjectDisable : MonoBehaviour
{
	public UnityEvent Action;

	private void OnDisable()
	{
		if (!Application.isQuitting)
		{
			Action.Invoke();
		}
	}
}
