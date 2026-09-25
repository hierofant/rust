using System;
using Rust.UI;
using UnityEngine;

public class SatelliteMenuUI : UIDialog
{
	[Header("Screen Panels")]
	public GameObject panelOffline;

	public GameObject panelList;

	public GameObject panelControl;

	public GameObject panelLocked;

	public GameObject panelCooldown;

	[Header("Satellite List Screen")]
	public RectTransform satelliteListContainer;

	public GameObject satelliteEntryPrefab;

	[Header("Control Screen")]
	public RectTransform thrusterButtonContainer;

	public GameObject thrusterButtonPrefab;

	public RustText textTimeRemaining;

	public SatelliteMapUI mapUI;

	public RectTransform fuelBlockContainer;

	public GameObject fuelBlockPrefab;

	[Tooltip("Column captions above the thruster and fuel containers, shown and hidden with the containers they label.")]
	public GameObject thrusterHeader;

	public GameObject fuelHeader;

	[Header("Locked Screen")]
	public RustText textLockedStatus;

	public GameObject lockButtonPrefab;

	[Header("Cooldown Screen")]
	public RustText textCooldownStatus;

	[Header("Audio")]
	public SoundDefinition buttonPressSoundDef;

	public SoundDefinition thrusterFireSoundDef;

	[NonSerialized]
	public EntityRef ownerComputer;
}
