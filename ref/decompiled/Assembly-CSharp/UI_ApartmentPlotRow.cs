using Rust.UI;
using UnityEngine;

public class UI_ApartmentPlotRow : BaseMonoBehaviour
{
	[SerializeField]
	private RustText indexText;

	[SerializeField]
	private RustText nameText;

	[SerializeField]
	private RustText storageText;

	[SerializeField]
	private RustText roomsText;

	[SerializeField]
	private RustText valueText;

	[SerializeField]
	[Space]
	private GameObject[] occupiedVisuals;

	[SerializeField]
	private GameObject[] unoccupiedVisuals;

	private static readonly Translate.Phrase lowPhrase = new Translate.Phrase("apartment.value.low", "Low");

	private static readonly Translate.Phrase mediumPhrase = new Translate.Phrase("apartment.value.medium", "Medium");

	private static readonly Translate.Phrase highPhrase = new Translate.Phrase("apartment.value.high", "High");

	private static readonly Translate.Phrase nullPhrase = new Translate.Phrase("apartment.value.null", "Null");

	private static readonly Translate.Phrase slotsPhrase = new Translate.Phrase("apartment.slots", "{0} Slots");

	private static readonly Translate.Phrase shopPhrase = new Translate.Phrase("apartment.shop", "Shop");

	public NetworkableId RoomId { get; private set; }
}
