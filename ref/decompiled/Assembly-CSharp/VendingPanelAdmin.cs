using TMPro;
using UnityEngine;

public class VendingPanelAdmin : UIDialog
{
	public GameObjectRef statsPanelRef;

	public AddSellOrderManager sellOrderManager;

	public EmojiGallery emojiGallery;

	public GameObject sellOrderAdminContainer;

	public GameObject sellOrderAdminPrefab;

	public TMP_InputField storeNameInputField;

	[Header("Drone Prediction")]
	public DeliveryDroneConfig predictionConfig;

	public GameObject droneAccessible;

	public GameObject droneInaccessible;
}
