using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class TechTreeSelectedNodeUI : FacepunchBehaviour
{
	public RustText selectedTitle;

	public RawImage selectedIcon;

	public RustText selectedDescription;

	public RustText costText;

	public RustText craftingCostText;

	public GameObject costObject;

	public GameObject cantAffordObject;

	public GameObject unlockedObject;

	public GameObject unlockButton;

	public GameObject unlockPathButton;

	public TechTreeDialog dialog;

	public Color ColorAfford;

	public Color ColorCantAfford;

	public GameObject singleCostRoot;

	public RustText singleCostText;

	public ItemInformationPanel[] informationPanels;

	public GameObject workbenchTaxRoot;

	public RustText workbenchTaxText;

	public Tooltip workbenchTaxTooltip;

	[Header("Prototype Bypass")]
	public RustText prototypeCostText;

	public GameObject prototypeRoot;

	public GameObject prototypeFailRoot;

	public GameObject cantAffordPrototypeRoot;
}
