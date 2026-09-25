using System.Collections.Generic;
using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_ServerBrowser_ModifyComponentsOnCategory : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> _disableComponentsWhileActive = new List<GameObject>();

	[SerializeField]
	private List<GameObject> _enableComponentsWhileActive = new List<GameObject>();
}
