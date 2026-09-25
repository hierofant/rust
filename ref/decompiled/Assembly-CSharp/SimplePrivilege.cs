using System.Collections.Generic;
using Facepunch;
using ProtoBuf;

public class SimplePrivilege : BaseEntity, IPrivilege
{
	public HashSet<ulong> authorizedPlayers = new HashSet<ulong>();

	public const Flags Flag_MaxAuths = Flags.Reserved5;

	public override void ResetState()
	{
		base.ResetState();
		authorizedPlayers.Clear();
	}

	public bool IsAuthed(BasePlayer player)
	{
		return IsAuthed(player.userID);
	}

	public bool IsAuthed(ulong userID)
	{
		return authorizedPlayers.Contains(userID);
	}

	public bool AnyAuthed()
	{
		return authorizedPlayers.Count > 0;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.buildingPrivilege = Pool.Get<BuildingPrivilege>();
		if (authorizedPlayers == null)
		{
			return;
		}
		info.msg.buildingPrivilege.users = Pool.Get<List<PlayerNameID>>();
		foreach (ulong authorizedPlayer in authorizedPlayers)
		{
			PlayerNameID playerNameID = Pool.Get<PlayerNameID>();
			playerNameID.userid = authorizedPlayer;
			info.msg.buildingPrivilege.users.Add(playerNameID);
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		authorizedPlayers.Clear();
		if (info.msg.buildingPrivilege == null || info.msg.buildingPrivilege.users == null)
		{
			return;
		}
		foreach (PlayerNameID user in info.msg.buildingPrivilege.users)
		{
			authorizedPlayers.Add(user.userid);
		}
	}

	public bool AtMaxAuthCapacity()
	{
		return HasFlag(Flags.Reserved5);
	}

	public void UpdateMaxAuthCapacity()
	{
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if ((bool)activeGameMode && activeGameMode.limitTeamAuths)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved5, authorizedPlayers.Count >= activeGameMode.GetMaxRelationshipTeamSize());
			}
		}
	}
}
