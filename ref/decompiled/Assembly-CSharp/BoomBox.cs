using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Newtonsoft.Json.Linq;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;

public class BoomBox : EntityComponent<BaseEntity>, INotifyLOD
{
	public static Dictionary<string, string> ValidStations;

	public static Dictionary<string, string> ServerValidStations;

	[ReplicatedVar(Saved = true, Help = "A list of radio stations that are valid on this server. Format: NAME,URL,NAME,URL,etc", ShowInAdminUI = true)]
	public static string ServerUrlList = string.Empty;

	public static string lastParsedServerList;

	public ShoutcastStreamer ShoutcastStreamer;

	public GameObjectRef RadioIpDialog;

	public ulong AssignedRadioBy;

	public AudioSource SoundSource;

	public float ConditionLossRate = 0.25f;

	public ItemDefinition[] ValidCassettes;

	public SoundDefinition PlaySfx;

	public SoundDefinition StopSfx;

	public const BaseEntity.Flags HasCassette = BaseEntity.Flags.Reserved1;

	[ServerVar(Saved = true, Help = "(Generated) Number of seconds of audio backtrack buffer maintained by the boombox for streaming synchronisation; default 30s")]
	public static int BacktrackLength = 30;

	public Action<float> HurtCallback;

	public string CurrentRadioIp { get; set; } = "rustradio.facepunch.com";


	public BaseEntity BaseEntity => base.baseEntity;

	private bool isClient
	{
		get
		{
			if (base.baseEntity != null)
			{
				return base.baseEntity.isClient;
			}
			return false;
		}
	}

	[ServerVar(Help = "(Generated) Clears all radio station data set by the given Steam64 ID from all deployed and held boomboxes on the server")]
	public static void ClearRadioByUser(ConsoleSystem.Arg arg)
	{
		ulong uInt = arg.GetUInt64(0, 0uL);
		int num = 0;
		foreach (BaseNetworkable serverEntity in BaseNetworkable.serverEntities)
		{
			if (serverEntity is DeployableBoomBox deployableBoomBox)
			{
				if (deployableBoomBox.ClearRadioByUserId(uInt))
				{
					num++;
				}
			}
			else if (serverEntity is HeldBoomBox heldBoomBox && heldBoomBox.ClearRadioByUserId(uInt))
			{
				num++;
			}
		}
		arg.ReplyWith($"Stopped and cleared saved URL of {num} boom boxes");
	}

	public static void LoadStations()
	{
		if (ValidStations == null)
		{
			ValidStations = GetStationData() ?? new Dictionary<string, string>();
			ParseServerUrlList();
		}
	}

	public static Dictionary<string, string> GetStationData()
	{
		if ((Facepunch.Application.Manifest?.Metadata)?["RadioStations"] is JArray { Count: >0 } jArray)
		{
			string[] array = new string[2];
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			{
				foreach (string item in jArray.Values<string>())
				{
					array = item.Split(',');
					if (!dictionary.ContainsKey(array[0]) && !array[1].Contains("facepunch"))
					{
						dictionary.Add(array[0], array[1]);
					}
				}
				return dictionary;
			}
		}
		return null;
	}

	public static bool IsStationValid(string url)
	{
		ParseServerUrlList();
		object obj = Interface.CallHook("OnBoomboxStationValidate", url);
		if (obj is bool)
		{
			return (bool)obj;
		}
		ShoutcastStreamer.CheckBuiltInRadios();
		if (ValidStations == null || !ValidStations.ContainsValue(url))
		{
			if (ServerValidStations == null || !ServerValidStations.ContainsValue(url))
			{
				if (ShoutcastStreamer.ParsedLocalRadioList != null)
				{
					return ShoutcastStreamer.ParsedLocalRadioList.ContainsValue(url);
				}
				return false;
			}
			return true;
		}
		return true;
	}

	public static void ParseServerUrlList()
	{
		if (ServerValidStations == null)
		{
			ServerValidStations = new Dictionary<string, string>();
		}
		if (lastParsedServerList == ServerUrlList)
		{
			return;
		}
		ServerValidStations.Clear();
		if (!string.IsNullOrEmpty(ServerUrlList))
		{
			string[] array = ServerUrlList.Split(',');
			if (array.Length % 2 != 0)
			{
				Debug.Log("Invalid number of stations in BoomBox.ServerUrlList, ensure you always have a name and a url");
				return;
			}
			for (int i = 0; i < array.Length; i += 2)
			{
				if (ServerValidStations.ContainsKey(array[i]))
				{
					Debug.Log("Duplicate station name detected in BoomBox.ServerUrlList, all station names must be unique: " + array[i]);
				}
				else
				{
					ServerValidStations.Add(array[i], array[i + 1]);
				}
			}
		}
		lastParsedServerList = ServerUrlList;
	}

	public void Server_UpdateRadioIP(BaseEntity.RPCMessage msg)
	{
		string text = msg.read.String();
		if (Interface.CallHook("OnBoomboxStationUpdate", this, text, msg.player) == null && IsStationValid(text))
		{
			if (msg.player != null)
			{
				ulong assignedRadioBy = msg.player.userID.Get();
				AssignedRadioBy = assignedRadioBy;
			}
			CurrentRadioIp = text;
			base.baseEntity.ClientRPC(RpcTarget.NetworkGroup("OnRadioIPChanged"), CurrentRadioIp);
			Interface.CallHook("OnBoomboxStationUpdated", this, text, msg.player);
			if (IsOn())
			{
				ServerTogglePlay(play: false);
			}
		}
	}

	public void Save(BaseNetworkable.SaveInfo info)
	{
		if (info.msg.boomBox == null)
		{
			info.msg.boomBox = Facepunch.Pool.Get<ProtoBuf.BoomBox>();
		}
		info.msg.boomBox.radioIp = CurrentRadioIp;
		info.msg.boomBox.assignedRadioBy = AssignedRadioBy;
	}

	public bool ClearRadioByUserId(ulong id)
	{
		if (AssignedRadioBy == id)
		{
			CurrentRadioIp = string.Empty;
			AssignedRadioBy = 0uL;
			if (HasFlag(BaseEntity.Flags.On))
			{
				ServerTogglePlay(play: false);
			}
			return true;
		}
		return false;
	}

	public void Load(BaseNetworkable.LoadInfo info)
	{
		if (info.msg.boomBox != null)
		{
			CurrentRadioIp = info.msg.boomBox.radioIp;
			AssignedRadioBy = info.msg.boomBox.assignedRadioBy;
		}
	}

	public void ServerTogglePlay(BaseEntity.RPCMessage msg, bool bypassPower = false)
	{
		if (!IsPowered() && !bypassPower)
		{
			return;
		}
		BasePlayer player = msg.player;
		bool flag = msg.read.ReadByte() == 1;
		if (Interface.CallHook("OnBoomboxToggle", this, msg.player, flag) == null)
		{
			if (flag && player != null && player.IsConnected && !player.IsNpc && !player.IsBot && base.baseEntity is DeployableBoomBox deployableBoomBox)
			{
				BaseMission.MissionEventPayload missionEventPayload = default(BaseMission.MissionEventPayload);
				missionEventPayload.NetworkIdentifier = deployableBoomBox.net.ID;
				missionEventPayload.WorldPosition = base.transform.position;
				missionEventPayload.StringIdentifier = CurrentRadioIp;
				BaseMission.MissionEventPayload payload = missionEventPayload;
				player.ProcessMissionEvent(BaseMission.MissionEventType.PLAY_BOOMBOX, payload, deployableBoomBox.IsStatic ? 1f : 0f);
			}
			ServerTogglePlay(flag);
		}
	}

	public void DeductCondition()
	{
		HurtCallback?.Invoke(ConditionLossRate * ConVar.Decay.scale);
	}

	public void ServerTogglePlay(bool play)
	{
		if (!(base.baseEntity == null) && HasFlag(BaseEntity.Flags.On) != play)
		{
			SetFlag(BaseEntity.Flags.On, play);
			if (base.baseEntity is IOEntity iOEntity)
			{
				iOEntity.MarkDirtyForceUpdateOutputs();
			}
			if (play && !IsInvoking(DeductCondition) && ConditionLossRate > 0f)
			{
				InvokeRepeating(DeductCondition, 1f, 1f);
			}
			else if (IsInvoking(DeductCondition))
			{
				CancelInvoke(DeductCondition);
			}
		}
	}

	public void OnCassetteInserted(Cassette c)
	{
		if (!(base.baseEntity == null))
		{
			base.baseEntity.ClientRPC(RpcTarget.NetworkGroup("Client_OnCassetteInserted"), c.net.ID);
			ServerTogglePlay(play: false);
			SetFlag(BaseEntity.Flags.Reserved1, state: true);
			base.baseEntity.SendNetworkUpdate();
		}
	}

	public void OnCassetteRemoved(Cassette c)
	{
		if (!(base.baseEntity == null))
		{
			base.baseEntity.ClientRPC(RpcTarget.NetworkGroup("Client_OnCassetteRemoved"));
			ServerTogglePlay(play: false);
			SetFlag(BaseEntity.Flags.Reserved1, state: false);
		}
	}

	public void SetFlag(BaseEntity.Flags f, bool state)
	{
		if (base.baseEntity != null)
		{
			using (BaseEntity.FlagsUpdateScope flagsUpdateScope = base.baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(f, state);
			}
		}
	}

	public bool IsPowered()
	{
		if (base.baseEntity == null)
		{
			return false;
		}
		if (!base.baseEntity.HasFlag(BaseEntity.Flags.Reserved8))
		{
			return base.baseEntity is HeldBoomBox;
		}
		return true;
	}

	public bool IsOn()
	{
		if (base.baseEntity == null)
		{
			return false;
		}
		return base.baseEntity.IsOn();
	}

	public bool HasFlag(BaseEntity.Flags f)
	{
		if (base.baseEntity == null)
		{
			return false;
		}
		return base.baseEntity.HasFlag(f);
	}
}
