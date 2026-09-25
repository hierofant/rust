using System;
using System.Collections.Generic;
using Facepunch;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NeedsKeyboard : ListComponent<NeedsKeyboard>
{
	[Flags]
	public enum BypassOption
	{
		Voice = 1,
		Chat = 4,
		Gesture = 8,
		CardGames = 0x10,
		Movement = 0x20,
		Ping = 0x40,
		Inventory = 0x80,
		Crafting = 0x100,
		Clan = 0x200,
		Contacts = 0x400,
		Attack = 0x800,
		Reload = 0x1000,
		Painting = 0x2000,
		Duck = 0x4000,
		Autorun = 0x8000
	}

	public UnityEvent onNoKeysDown;

	public bool ShowEscapeUI;

	public bool blockUnspecifiedInput = true;

	public BypassOption AllowedBinds;

	private List<string> binds = new List<string>();

	private bool watchForNoKeys;

	public static bool ShouldShowUI
	{
		get
		{
			if (ListComponent<NeedsKeyboard>.InstanceList.Count > 0)
			{
				return ListComponent<NeedsKeyboard>.InstanceList[0].ShowEscapeUI;
			}
			return false;
		}
	}

	private static void GetBindString(BypassOption bypassOption, List<string> resultBinds)
	{
		if ((bypassOption & BypassOption.Voice) == BypassOption.Voice)
		{
			resultBinds.Add("+voice");
		}
		if ((bypassOption & BypassOption.Chat) == BypassOption.Chat)
		{
			resultBinds.Add("chat.open");
		}
		if ((bypassOption & BypassOption.Gesture) == BypassOption.Gesture)
		{
			resultBinds.Add("+gestures");
		}
		if ((bypassOption & BypassOption.Movement) == BypassOption.Movement)
		{
			resultBinds.Add("+left");
			resultBinds.Add("+right");
			resultBinds.Add("+backward");
			resultBinds.Add("+forward");
			resultBinds.Add("+sprint");
			resultBinds.Add("+duck");
			resultBinds.Add("+jump");
		}
		if ((bypassOption & BypassOption.Ping) == BypassOption.Ping)
		{
			resultBinds.Add("+ping");
		}
		if ((bypassOption & BypassOption.Inventory) == BypassOption.Inventory)
		{
			resultBinds.Add("inventory.toggle");
		}
		if ((bypassOption & BypassOption.Crafting) == BypassOption.Crafting)
		{
			resultBinds.Add("inventory.togglecrafting");
		}
		if ((bypassOption & BypassOption.Clan) == BypassOption.Clan)
		{
			resultBinds.Add("clan.toggleclan");
		}
		if ((bypassOption & BypassOption.Contacts) == BypassOption.Contacts)
		{
			resultBinds.Add("uicontacts.togglecontacts");
		}
		if ((bypassOption & BypassOption.Attack) == BypassOption.Attack)
		{
			resultBinds.Add("+attack");
			resultBinds.Add("+attack2");
			resultBinds.Add("+attack3");
		}
		if ((bypassOption & BypassOption.Reload) == BypassOption.Reload)
		{
			resultBinds.Add("+reload");
		}
		if ((bypassOption & BypassOption.Painting) == BypassOption.Painting)
		{
			resultBinds.Add("paint.selectedtool");
			resultBinds.Add("paint.selectedbrush");
			resultBinds.Add("paint.brushsize");
			resultBinds.Add("paint.brushopacity");
		}
	}

	public static bool AnyActive(BypassOption forBypass)
	{
		return AnyActive(Key.None, forBypass);
	}

	public static bool AnyActive(Key key = Key.None, BypassOption forBypass = (BypassOption)0)
	{
		if (ListComponent<NeedsKeyboard>.InstanceList.Count == 0)
		{
			return false;
		}
		if ((forBypass & BypassOption.Duck) != BypassOption.Duck && (forBypass & BypassOption.Autorun) != BypassOption.Autorun && AnyTextboxFocused())
		{
			return true;
		}
		if (key != 0 || forBypass != 0)
		{
			foreach (NeedsKeyboard instance in ListComponent<NeedsKeyboard>.InstanceList)
			{
				if (instance.ShouldBlockInput() && !instance.AllowKeyInput(key, forBypass))
				{
					return true;
				}
			}
			return false;
		}
		foreach (NeedsKeyboard instance2 in ListComponent<NeedsKeyboard>.InstanceList)
		{
			if (instance2.blockUnspecifiedInput && instance2.ShouldBlockInput())
			{
				return true;
			}
		}
		return false;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		watchForNoKeys = true;
		if (AllowedBinds == (BypassOption)0)
		{
			return;
		}
		binds.Clear();
		List<string> obj = Pool.Get<List<string>>();
		GetBindString(AllowedBinds, obj);
		foreach (string item in obj)
		{
			binds.AddRange(Input.GetButtonsWithBind(item));
		}
		Pool.FreeUnmanaged(ref obj);
	}

	public void Update()
	{
		if (watchForNoKeys)
		{
			Keyboard current = Keyboard.current;
			Mouse current2 = Mouse.current;
			if ((current == null || !current.anyKey.isPressed) && (current2 == null || (!current2.leftButton.isPressed && !current2.rightButton.isPressed && !current2.middleButton.isPressed)))
			{
				watchForNoKeys = false;
				onNoKeysDown?.Invoke();
			}
		}
	}

	private bool AllowKeyInput(Key k, BypassOption forBypass)
	{
		if (AllowedBinds == (BypassOption)0)
		{
			return false;
		}
		if (forBypass != 0 && (forBypass & AllowedBinds) == forBypass)
		{
			return true;
		}
		if (forBypass == (BypassOption)0 && k == Key.None)
		{
			return false;
		}
		string b = UnityButtons.LocalizeBindToken(k);
		foreach (string bind in binds)
		{
			if (string.Equals(bind, b, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	protected virtual bool ShouldBlockInput()
	{
		return true;
	}

	private static bool AnyTextboxFocused()
	{
		EventSystem current = EventSystem.current;
		if (current != null)
		{
			GameObject currentSelectedGameObject = current.currentSelectedGameObject;
			if (currentSelectedGameObject != null)
			{
				if (currentSelectedGameObject.TryGetComponent<InputField>(out var component) && !component.readOnly && component.isFocused)
				{
					return true;
				}
				if (currentSelectedGameObject.TryGetComponent<TMP_InputField>(out var component2) && !component2.readOnly && component2.isFocused)
				{
					return true;
				}
			}
		}
		return false;
	}
}
