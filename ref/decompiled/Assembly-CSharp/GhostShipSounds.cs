using System.Collections.Generic;
using Prefabs.Misc;
using UnityEngine;

public class GhostShipSounds : MonoBehaviour
{
	[SerializeField]
	private GhostShip ghostShip;

	private float soundCullDistanceSq;

	[Header("Ambient")]
	[SerializeField]
	private SoundDefinition hullGroanDef;

	[SerializeField]
	private float hullGroanCooldown = 1f;

	[SerializeField]
	private List<AmbienceDefinitionList> AmbienceStormHullStings;

	[SerializeField]
	private float stormHullStingsCooldown = 2f;

	private float lastHullGroan;

	private float lastStormHullStingGroan;
}
