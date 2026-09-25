using ConVar;
using Rust.UI;
using UnityEngine;

public class UI_ServerAdminConvarInfo : MonoBehaviour
{
	[SerializeField]
	private RustText convarName;

	[SerializeField]
	private RustText convarValue;

	[SerializeField]
	private Tooltip tooltipComponent;

	private Admin.ServerConvarInfo cachedConvar;
}
