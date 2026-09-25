using Rust.UI;
using UnityEngine;

public class UI_ServerAdminUGCEntryStub : MonoBehaviour
{
	[SerializeField]
	private RustText prefabNameText;

	[SerializeField]
	[Header("Widgets")]
	private UI_ServerAdminUGCEntryImage imageWidget;

	[SerializeField]
	private UI_ServerAdminUGCEntryAudio audioWidget;

	[SerializeField]
	private UI_ServerAdminUGCEntryPattern patternWidget;

	[SerializeField]
	private UI_ServerAdminUGCEntryVendingMachine vendingMachineWidget;

	[SerializeField]
	private UI_ServerAdminUGCEntrySculpture sculptureWidget;

	[Space]
	[SerializeField]
	private RustButton editHistoryButton;

	[SerializeField]
	private GameObjectRef historyPlayerIdPrefab;

	[SerializeField]
	private RectTransform historyPlayerIdParent;
}
