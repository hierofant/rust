using System;
using UnityEngine;

[DisallowMultipleComponent]
public class ProjectileModPartVisibility : EntityComponent<BaseProjectile>, IClientComponent, IViewModelUpdated, IViewModeChanged
{
	public enum ModMatchMode
	{
		Any,
		All
	}

	public enum RuleVisibilityMode
	{
		HideWhenMatched,
		ShowWhenMatched
	}

	[Serializable]
	public class Rule
	{
		[Tooltip("Optional label to make this easier to identify.")]
		public string label;

		[Tooltip("ProjectileWeaponMod entity prefabs that drive this rule.")]
		public GameObjectRef[] projectileWeaponMods;

		[Tooltip("How projectileWeaponMods are matched.")]
		public ModMatchMode matchMode;

		[Tooltip("Controls whether targets are hidden or shown while this rule matches.")]
		public RuleVisibilityMode visibilityMode;

		[Tooltip("When true, only non-broken and logically active mods are considered.")]
		public bool requireActiveMod = true;

		[Tooltip("Renderers on this entity affected by this rule.")]
		public Renderer[] entityRenderers;
	}

	public Rule[] rules = Array.Empty<Rule>();

	public bool isViewmodel;
}
