using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Facepunch.Extend;
using Facepunch.Nexus;
using Facepunch.Nexus.Models;
using UnityEngine;

public class NexusClanWrapper : IClan
{
	private const int MaxChatScrollback = 20;

	public readonly NexusClan Internal;

	private readonly NexusClanChatCollector _chatCollector;

	private readonly List<ClanRole> _roles;

	private readonly List<ClanMember> _members;

	private readonly List<ClanInvite> _invites;

	private readonly List<ClanChatEntry> _chatHistory;

	private RealTimeSince _sinceLastRefresh;

	public long ClanId => Internal.ClanId;

	public string Name => Internal.Name;

	public long Created => Internal.Created;

	public ulong Creator => Internal.Creator;

	public string Motd { get; private set; }

	public long MotdTimestamp { get; private set; }

	public ulong MotdAuthor { get; private set; }

	public byte[] Logo { get; private set; }

	public Color32 Color { get; private set; }

	public long Score => Internal.Score;

	public IReadOnlyList<ClanRole> Roles => _roles;

	public IReadOnlyList<ClanMember> Members => _members;

	public int MaxMemberCount { get; private set; }

	public IReadOnlyList<ClanInvite> Invites => _invites;

	public NexusClanWrapper(NexusClan clan, NexusClanChatCollector chatCollector)
	{
		Internal = clan ?? throw new ArgumentNullException("clan");
		_chatCollector = chatCollector ?? throw new ArgumentNullException("chatCollector");
		_roles = new List<ClanRole>();
		_members = new List<ClanMember>();
		_invites = new List<ClanInvite>();
		_chatHistory = new List<ClanChatEntry>(20);
		_sinceLastRefresh = 0f;
		UpdateValuesInternal();
	}

	public void UpdateValuesInternal()
	{
		NexusClanUtil.GetMotd(Internal, out var motd, out var motdTimestamp, out var motdAuthor);
		Motd = motd;
		MotdTimestamp = motdTimestamp;
		MotdAuthor = motdAuthor;
		NexusClanUtil.GetBanner(Internal, out var logo, out var color);
		Logo = logo;
		Color = color;
		_roles.Resize(Internal.Roles.Count);
		for (int i = 0; i < _roles.Count; i++)
		{
			_roles[i] = NexusClanUtil.ToClanRole(Internal.Roles[i]);
		}
		_members.Resize(Internal.Members.Count);
		for (int j = 0; j < _members.Count; j++)
		{
			_members[j] = NexusClanUtil.ToClanMember(Internal.Members[j]);
		}
		MaxMemberCount = Internal.MaxMemberCount;
		_invites.Resize(Internal.Invites.Count);
		for (int k = 0; k < _invites.Count; k++)
		{
			_invites[k] = NexusClanUtil.ToClanInvite(Internal.Invites[k]);
		}
	}

	public long GetLatestScore()
	{
		return Internal.Score;
	}

	public async ValueTask RefreshIfStale()
	{
		if ((float)_sinceLastRefresh > 30f)
		{
			_sinceLastRefresh = 0f;
			try
			{
				await Internal.Refresh();
				UpdateValuesInternal();
			}
			catch (Exception exception)
			{
				Debug.LogError($"Failed to refresh nexus clan ID {ClanId}");
				Debug.LogException(exception);
			}
		}
	}

	public async ValueTask<ClanValueResult<ClanLogs>> GetLogs(int limit, ulong bySteamId)
	{
		try
		{
			NexusClanResult<List<Facepunch.Nexus.Models.ClanLogEntry>> nexusClanResult = await Internal.GetLogs(bySteamId, limit);
			if (nexusClanResult.IsSuccess && nexusClanResult.TryGetResponse(out var response))
			{
				ClanLogs clanLogs = default(ClanLogs);
				clanLogs.ClanId = ClanId;
				clanLogs.Entries = response.Select(delegate(Facepunch.Nexus.Models.ClanLogEntry e)
				{
					ClanLogEntry result = default(ClanLogEntry);
					result.Timestamp = e.Timestamp * 1000;
					result.EventKey = e.EventKey;
					result.Arg1 = e.Arg1;
					result.Arg2 = e.Arg2;
					result.Arg3 = e.Arg3;
					result.Arg4 = e.Arg4;
					return result;
				}).ToList();
				return clanLogs;
			}
			return NexusClanUtil.ToClanResult(nexusClanResult.ResultCode);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return ClanResult.Fail;
		}
	}

	public async ValueTask<ClanResult> UpdateLastSeen(ulong steamId)
	{
		try
		{
			return NexusClanUtil.ToClanResult(await Internal.UpdateLastSeen(steamId));
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return ClanResult.Fail;
		}
	}

	public async ValueTask<ClanResult> SetMotd(string newMotd, ulong bySteamId)
	{
		if (!CheckRole(bySteamId, (ClanRole r) => r.CanSetMotd))
		{
			return ClanResult.NoPermission;
		}
		try
		{
			string text = bySteamId.ToString("G");
			return NexusClanUtil.ToClanResult(await Internal.UpdateVariables(new ClanVariablesUpdate
			{
				Variables = new List<VariableUpdate>(2)
				{
					new VariableUpdate("motd", newMotd),
					new VariableUpdate("motd_author", text)
				},
				EventKey = "set_motd",
				Arg1 = text,
				Arg2 = newMotd
			}));
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return ClanResult.Fail;
		}
	}

	public async ValueTask<ClanResult> SetLogo(byte[] newLogo, ulong bySteamId)
	{
		if (!CheckRole(bySteamId, (ClanRole r) => r.CanSetLogo))
		{
			return ClanResult.NoPermission;
		}
		try
		{
			string arg = bySteamId.ToString("G");
			return NexusClanUtil.ToClanResult(await Internal.UpdateVariables(new ClanVariablesUpdate
			{
				Variables = new List<VariableUpdate>(1)
				{
					new VariableUpdate("logo", newLogo)
				},
				EventKey = "set_logo",
				Arg1 = arg
			}));
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return ClanResult.Fail;
		}
	}

	public async ValueTask<ClanResult> SetColor(Color32 newColor, ulong bySteamId)
	{
		if (!CheckRole(bySteamId, (ClanRole r) => r.CanSetLogo))
		{
			return ClanResult.NoPermission;
		}
		try
		{
			string arg = bySteamId.ToString("G");
			return NexusClanUtil.ToClanResult(await Internal.UpdateVariables(new ClanVariablesUpdate
			{
				Variables = new List<VariableUpdate>(1)
				{
					new VariableUpdate("color", newColor.ToInt32().ToString("G"))
				},
				EventKey = "set_color",
				Arg1 = arg,
				Arg2 = newColor.ToHex()
			}));
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return ClanResult.Fail;
		}
	}

	public async ValueTask<ClanResult> Invite(ulong steamId, ulong bySteamId)
	{
		try
		{
			return NexusClanUtil.ToClanResult(await Internal.Invite(steamId, bySteamId));
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return ClanResult.Fail;
		}
	}

	public async ValueTask<ClanResult> CancelInvite(ulong steamId, ulong bySteamId)
	{
		try
		{
			return NexusClanUtil.ToClanResult(await Internal.CancelInvite(steamId, bySteamId));
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return ClanResult.Fail;
		}
	}

	public async ValueTask<ClanResult> AcceptInvite(ulong steamId)
	{
		try
		{
			return NexusClanUtil.ToClanResult(await Internal.AcceptInvite(steamId));
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return ClanResult.Fail;
		}
	}

	public async ValueTask<ClanResult> Kick(ulong steamId, ulong bySteamId)
	{
		try
		{
			return NexusClanUtil.ToClanResult(await Internal.Kick(steamId, bySteamId));
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return ClanResult.Fail;
		}
	}

	public async ValueTask<ClanResult> SetPlayerRole(ulong steamId, int newRoleId, ulong bySteamId)
	{
		try
		{
			return NexusClanUtil.ToClanResult(await Internal.SetPlayerRole(steamId, newRoleId, bySteamId));
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return ClanResult.Fail;
		}
	}

	public async ValueTask<ClanResult> SetPlayerNotes(ulong steamId, string notes, ulong bySteamId)
	{
		if (!CheckRole(bySteamId, (ClanRole r) => r.CanSetPlayerNotes))
		{
			return ClanResult.NoPermission;
		}
		try
		{
			return NexusClanUtil.ToClanResult(await Internal.UpdatePlayerVariables(steamId, new ClanVariablesUpdate
			{
				Variables = new List<VariableUpdate>(1)
				{
					new VariableUpdate("notes", notes)
				},
				EventKey = "set_notes",
				Arg1 = bySteamId.ToString("G"),
				Arg2 = steamId.ToString("G"),
				Arg3 = notes
			}));
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return ClanResult.Fail;
		}
	}

	public async ValueTask<ClanResult> CreateRole(ClanRole role, ulong bySteamId)
	{
		try
		{
			return NexusClanUtil.ToClanResult(await Internal.CreateRole(NexusClanUtil.ToRoleParameters(role), bySteamId));
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return ClanResult.Fail;
		}
	}

	public async ValueTask<ClanResult> UpdateRole(ClanRole role, ulong bySteamId)
	{
		try
		{
			return NexusClanUtil.ToClanResult(await Internal.UpdateRole(role.RoleId, NexusClanUtil.ToRoleParameters(role), bySteamId));
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return ClanResult.Fail;
		}
	}

	public async ValueTask<ClanResult> SwapRoleRanks(int roleIdA, int roleIdB, ulong bySteamId)
	{
		try
		{
			return NexusClanUtil.ToClanResult(await Internal.SwapRoleRanks(roleIdA, roleIdB, bySteamId));
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return ClanResult.Fail;
		}
	}

	public async ValueTask<ClanResult> DeleteRole(int roleId, ulong bySteamId)
	{
		try
		{
			return NexusClanUtil.ToClanResult(await Internal.DeleteRole(roleId, bySteamId));
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return ClanResult.Fail;
		}
	}

	public async ValueTask<ClanResult> Disband(ulong bySteamId)
	{
		try
		{
			return NexusClanUtil.ToClanResult(await Internal.Disband(bySteamId));
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return ClanResult.Fail;
		}
	}

	public async ValueTask<ClanValueResult<ClanScoreEvents>> GetScoreEvents(int limit, ulong bySteamId)
	{
		try
		{
			NexusClanResult<List<ClanScoreEventEntry>> nexusClanResult = await Internal.GetScoreEvents(bySteamId, limit);
			if (nexusClanResult.IsSuccess && nexusClanResult.TryGetResponse(out var response))
			{
				ClanScoreEvents clanScoreEvents = default(ClanScoreEvents);
				clanScoreEvents.ClanId = ClanId;
				clanScoreEvents.ScoreEvents = response.Select(delegate(ClanScoreEventEntry e)
				{
					ClanScoreEvent result = default(ClanScoreEvent);
					result.Timestamp = e.Timestamp * 1000;
					result.Type = (ClanScoreEventType)e.Type;
					result.Score = e.Score;
					result.Multiplier = e.Multiplier;
					result.SteamId = e.PlayerId;
					result.OtherSteamId = e.OtherPlayerId;
					result.OtherClanId = e.OtherClanId;
					result.Arg1 = e.Arg1;
					result.Arg2 = e.Arg2;
					return result;
				}).ToList();
				return clanScoreEvents;
			}
			return NexusClanUtil.ToClanResult(nexusClanResult.ResultCode);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return ClanResult.Fail;
		}
	}

	public ValueTask<ClanResult> AddScoreEvent(ClanScoreEvent scoreEvent)
	{
		Internal.AddScoreEvent(new NewClanScoreEventEntry
		{
			Type = (int)scoreEvent.Type,
			Score = scoreEvent.Score,
			Multiplier = scoreEvent.Multiplier,
			PlayerId = scoreEvent.SteamId.GetValueOrDefault(),
			OtherPlayerId = scoreEvent.OtherSteamId.GetValueOrDefault(),
			OtherClanId = scoreEvent.OtherClanId,
			Arg1 = scoreEvent.Arg1,
			Arg2 = scoreEvent.Arg2
		});
		return new ValueTask<ClanResult>(ClanResult.Success);
	}

	public ValueTask<ClanValueResult<ClanChatScrollback>> GetChatScrollback()
	{
		lock (_chatHistory)
		{
			ClanChatScrollback clanChatScrollback = default(ClanChatScrollback);
			clanChatScrollback.ClanId = ClanId;
			clanChatScrollback.Entries = _chatHistory.ToList();
			return new ValueTask<ClanValueResult<ClanChatScrollback>>(clanChatScrollback);
		}
	}

	public ValueTask<ClanResult> SendChatMessage(string name, string message, ulong bySteamId)
	{
		if (!_members.TryFindWith((ClanMember m) => m.SteamId, bySteamId).HasValue)
		{
			return new ValueTask<ClanResult>(ClanResult.Fail);
		}
		ClanValidatorResult clanValidatorResult = ClanValidator.ValidateChatMessage(message);
		if (!clanValidatorResult.Success)
		{
			return new ValueTask<ClanResult>(clanValidatorResult.Error.ToClanResult());
		}
		ClanChatEntry clanChatEntry = default(ClanChatEntry);
		clanChatEntry.SteamId = bySteamId;
		clanChatEntry.Name = name;
		clanChatEntry.Message = clanValidatorResult.Value;
		clanChatEntry.Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		ClanChatEntry entry = clanChatEntry;
		AddScrollback(in entry);
		_chatCollector.OnClanChatMessage(ClanId, entry);
		return new ValueTask<ClanResult>(ClanResult.Success);
	}

	public void AddScrollback(in ClanChatEntry entry)
	{
		lock (_chatHistory)
		{
			if (_chatHistory.Count >= 20)
			{
				_chatHistory.RemoveAt(0);
			}
			_chatHistory.Add(entry);
		}
	}

	private bool CheckRole(ulong steamId, Func<ClanRole, bool> roleTest)
	{
		ClanMember? clanMember = _members.TryFindWith((ClanMember m) => m.SteamId, steamId);
		if (!clanMember.HasValue)
		{
			return false;
		}
		ClanRole? clanRole = _roles.TryFindWith((ClanRole r) => r.RoleId, clanMember.Value.RoleId);
		if (!clanRole.HasValue)
		{
			return false;
		}
		if (clanRole.Value.Rank != 1)
		{
			return roleTest(clanRole.Value);
		}
		return true;
	}
}
