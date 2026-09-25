#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class CommunityEntity : PointEntity
{
	private class Countdown : MonoBehaviour
	{
		public enum TimerFormat
		{
			None,
			SecondsHundreth,
			MinutesSeconds,
			MinutesSecondsHundreth,
			HoursMinutes,
			HoursMinutesSeconds,
			HoursMinutesSecondsMilliseconds,
			HoursMinutesSecondsTenths,
			DaysHoursMinutes,
			DaysHoursMinutesSeconds,
			Custom
		}

		public string command = "";

		public float endTime;

		public float startTime;

		public float step = 1f;

		public float interval = 1f;

		public TimerFormat timerFormat;

		public string numberFormat = "0.####";

		public bool destroyIfDone = true;
	}

	public enum DraggablePositionSendType
	{
		NormalizedScreen,
		NormalizedParent,
		Relative,
		RelativeAnchor
	}

	private class FadeOut : MonoBehaviour
	{
		public float duration;
	}

	public enum TooltipType
	{
		Default,
		AlwaysOnTop,
		AlwaysOnTopEmoji
	}

	public GameObject TooltipRef;

	public GameObject TooltipAlwaysOnTopRef;

	public GameObject TooltipAlwaysOnTopEmojiRef;

	public static CommunityEntity ServerInstance;

	public static CommunityEntity ClientInstance;

	public GameObject[] OverallPanels;

	public Canvas[] AllCanvases;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("CommunityEntity.OnRpcMessage"))
		{
			if (rpc == 2271099967u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - DragRPC");
				}
				using (TimeWarning.New("DragRPC"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage rpc2 = rPCMessage;
							DragRPC(rpc2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in DragRPC");
					}
				}
				return true;
			}
			if (rpc == 3687934507u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - DropRPC");
				}
				using (TimeWarning.New("DropRPC"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage rpc3 = rPCMessage;
							DropRPC(rpc3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in DropRPC");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[ServerVar]
	public static void pietest(ConsoleSystem.Arg arg)
	{
		using CustomPie customPie = Facepunch.Pool.Get<CustomPie>();
		customPie.menus = Facepunch.Pool.Get<List<CustomPieMenu>>();
		AddPieMenu(customPie, "Switch Night", "Switch to night mode", "env.time 0", "assets/icons/device_add.png", disabled: false, selected: false, "", "");
		AddPieMenu(customPie, "Switch Day", "Switch to day mode", "env.time 12", "assets/icons/device_add.png", disabled: false, selected: false, "", "");
		AddPieMenu(customPie, "Rain", "Make it rain on the server", "sv weather.load storm", "assets/icons/embrella.png", disabled: false, selected: false, "", "");
		AddPieMenu(customPie, "Be Malicious", "This attempts to open your player inventory.", "inventory.toggle", "assets/icons/explosion_sprite.png", disabled: false, selected: false, "", "");
		AddPieMenu(customPie, "Left/Right Test", "Pick one or the other", "pietest_prev", "assets/icons/facepunch.png", disabled: false, selected: false, "pietest_prev", "pietest_next");
		AddPieMenu(customPie, "Exit", "Close this context menu", "", "assets/icons/close.png", disabled: false, selected: false, "", "");
		ServerInstance.SendPie(ArgEx.Player(arg), customPie);
	}

	[ServerVar]
	public static void pietest_prev(ConsoleSystem.Arg arg)
	{
		using CustomPie customPie = Facepunch.Pool.Get<CustomPie>();
		customPie.menus = Facepunch.Pool.Get<List<CustomPieMenu>>();
		AddPieMenu(customPie, "Go Back", null, "pietest", "assets/icons/fun.png", disabled: false, selected: false, "", "");
		AddPieMenu(customPie, "Or Don't", "You went previous", "pietest", "assets/icons/fun.png", disabled: true, selected: false, "", "");
		ServerInstance.SendPie(ArgEx.Player(arg), customPie);
	}

	[ServerVar]
	public static void pietest_next(ConsoleSystem.Arg arg)
	{
		using CustomPie customPie = Facepunch.Pool.Get<CustomPie>();
		customPie.menus = Facepunch.Pool.Get<List<CustomPieMenu>>();
		AddPieMenu(customPie, "Go Back", null, "pietest", "assets/icons/fun.png", disabled: false, selected: false, "", "");
		AddPieMenu(customPie, "Or Don't", "You went next", "pietest", "assets/icons/fun.png", disabled: true, selected: false, "", "");
		ServerInstance.SendPie(ArgEx.Player(arg), customPie);
	}

	private static void AddPieMenu(CustomPie pie, string name, string description, string command, string sprite, bool disabled, bool selected, string next, string prev)
	{
		CustomPieMenu customPieMenu = Facepunch.Pool.Get<CustomPieMenu>();
		customPieMenu.name = name;
		customPieMenu.description = description;
		customPieMenu.command = command;
		customPieMenu.sprite = sprite;
		customPieMenu.disabled = disabled;
		customPieMenu.selected = selected;
		customPieMenu.nextCommand = next;
		customPieMenu.prevCommand = prev;
		pie.menus.Add(customPieMenu);
	}

	public void SendPie(BasePlayer player, CustomPie pie)
	{
		ClientRPC(RpcTarget.Player("OpenPie", player), pie);
	}

	[RPC_Server]
	public void DragRPC(RPCMessage rpc)
	{
		string text = rpc.read.String();
		Vector3 position = rpc.read.Vector3();
		DraggablePositionSendType type = (DraggablePositionSendType)rpc.read.Int32();
		Hook_DragRPC(rpc.player, text, position, type);
	}

	private void Hook_DragRPC(BasePlayer player, string name, Vector3 position, DraggablePositionSendType type)
	{
		Interface.CallHook("OnCuiDraggableDrag", player, name, position, type);
	}

	[RPC_Server]
	public void DropRPC(RPCMessage rpc)
	{
		string draggedName = rpc.read.String();
		string draggedSlot = rpc.read.String();
		string swappedName = rpc.read.String();
		string swappedSlot = rpc.read.String();
		Hook_DropRPC(rpc.player, draggedName, draggedSlot, swappedName, swappedSlot);
	}

	private void Hook_DropRPC(BasePlayer player, string draggedName, string draggedSlot, string swappedName, string swappedSlot)
	{
		Interface.CallHook("OnCuiDraggableDrop", player, draggedName, draggedSlot, swappedName, swappedSlot);
	}

	public void SendCustomVitals(BasePlayer player, CustomVitals vitals)
	{
		ClientRPC(RpcTarget.Player("RPC_UpdateVitals", player), vitals);
	}

	public override void InitShared()
	{
		if (base.isServer)
		{
			ServerInstance = this;
		}
		else
		{
			ClientInstance = this;
		}
		base.InitShared();
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
		if (base.isServer)
		{
			ServerInstance = null;
		}
		else
		{
			ClientInstance = null;
		}
	}

	public void SendDestroyUIs(BasePlayer player, List<string> uiPanels)
	{
		using CommunityEntity_DestroyUIs communityEntity_DestroyUIs = Facepunch.Pool.Get<CommunityEntity_DestroyUIs>();
		communityEntity_DestroyUIs.list = Facepunch.Pool.Get<List<string>>();
		for (int i = 0; i < uiPanels.Count; i++)
		{
			communityEntity_DestroyUIs.list.Add(uiPanels[i]);
		}
		ClientRPC(RpcTarget.Player("DestroyUIs", player), communityEntity_DestroyUIs);
	}

	public void SendDestroyUIs(BasePlayer player, string[] uiPanels)
	{
		using CommunityEntity_DestroyUIs communityEntity_DestroyUIs = Facepunch.Pool.Get<CommunityEntity_DestroyUIs>();
		communityEntity_DestroyUIs.list = Facepunch.Pool.Get<List<string>>();
		for (int i = 0; i < uiPanels.Length; i++)
		{
			communityEntity_DestroyUIs.list.Add(uiPanels[i]);
		}
		ClientRPC(RpcTarget.Player("DestroyUIs", player), communityEntity_DestroyUIs);
	}
}
