using System.Collections.Generic;
using System.Threading.Tasks;
using ConVar;
using Facepunch;
using ProtoBuf;
using UnityEngine;

namespace CompanionServer.Handlers;

public class MapMarkers : BasePlayerHandler<AppEmpty>
{
	public override ValueTask Execute()
	{
		if (!ConVar.Server.mapenabled || ConVar.Server.fogofwar)
		{
			SendError("no_map");
			return default(ValueTask);
		}
		AppMapMarkers appMapMarkers = Facepunch.Pool.Get<AppMapMarkers>();
		appMapMarkers.markers = Facepunch.Pool.Get<List<AppMarker>>();
		RelationshipManager.PlayerTeam playerTeam = RelationshipManager.ServerInstance.FindPlayersTeam(base.UserId);
		if (playerTeam != null)
		{
			foreach (ulong member in playerTeam.members)
			{
				BasePlayer basePlayer = RelationshipManager.FindByID(member);
				if (!(basePlayer == null))
				{
					appMapMarkers.markers.Add(GetPlayerMarker(basePlayer));
				}
			}
		}
		else if (base.Player != null)
		{
			appMapMarkers.markers.Add(GetPlayerMarker(base.Player));
		}
		foreach (MapMarker serverMapMarker in MapMarker.serverMapMarkers)
		{
			if (serverMapMarker.appType != 0)
			{
				appMapMarkers.markers.Add(serverMapMarker.GetAppMarkerData());
			}
		}
		AppResponse appResponse = Facepunch.Pool.Get<AppResponse>();
		appResponse.mapMarkers = appMapMarkers;
		Send(appResponse);
		return default(ValueTask);
	}

	private static AppMarker GetPlayerMarker(BasePlayer player)
	{
		AppMarker appMarker = Facepunch.Pool.Get<AppMarker>();
		Vector2 vector = Util.WorldToMap(player.transform.position);
		appMarker.id = player.net.ID;
		appMarker.type = AppMarkerType.Player;
		appMarker.x = vector.x;
		appMarker.y = vector.y;
		appMarker.steamId = player.userID;
		return appMarker;
	}
}
