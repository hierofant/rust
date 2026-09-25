using UnityEngine;
using UnityEngine.UI;

public class CharmPicker : MonoBehaviour, IClientComponent
{
	public GameObjectRef AccessoryPrefab;

	public Transform AccessoryParent;

	public GameObject ActiveRoot;

	public ScrollRect scroller;

	public SearchFilterInput searchFilter;

	public SoundDefinition CharmEquipSound;

	public SoundDefinition CharmUnequipSound;
}
