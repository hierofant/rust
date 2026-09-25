using System;
using System.Collections;
using ConVar;
using Facepunch;
using Network;
using UnityEngine;
using UnityEngine.Networking;

public static class Auth_EAC
{
	public static IEnumerator Run(Connection connection)
	{
		connection.authStatusEAC = string.Empty;
		if (!connection.active || connection.rejected)
		{
			yield break;
		}
		EAC.SystemConfig requiredSystemConfig = (ConVar.Server.useServerWideRequiredSystemConfig ? EAC.SystemConfig.Default : EAC.SystemConfig.None);
		if (ConVar.Server.usePerPlayerRequiredSystemConfig)
		{
			using UnityWebRequest request = UnityWebRequest.Get($"{Facepunch.Application.Integration.ApiUrl}rust/playerReqSysConfig/{connection.userid}");
			request.timeout = 10;
			yield return request.SendWebRequest();
			if (request.result == UnityWebRequest.Result.Success)
			{
				try
				{
					PlayerRequiredSystemConfigPayload instance = PlayerRequiredSystemConfigPayload.Instance;
					instance.Flags = 0;
					JsonUtility.FromJsonOverwrite(request.downloadHandler.text, instance);
					requiredSystemConfig = (EAC.SystemConfig)((int)requiredSystemConfig | instance.Flags);
				}
				catch (Exception arg)
				{
					DebugEx.LogWarning($"Failed to parse required system config for {connection.userid}: {arg}");
				}
			}
			else
			{
				DebugEx.LogWarning($"Failed to fetch required system config for {connection.userid} ({request.result}: {request.error})");
			}
		}
		EACServer.OnJoinGame(connection, requiredSystemConfig);
		while (connection.active && !connection.rejected && connection.authStatusEAC == string.Empty)
		{
			yield return null;
		}
	}
}
