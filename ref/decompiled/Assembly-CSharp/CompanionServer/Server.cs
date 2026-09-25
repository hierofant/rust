using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CompanionServer.Handlers;
using ConVar;
using Development.Attributes;
using Facepunch;
using Newtonsoft.Json;
using ProtoBuf;
using UnityEngine;

namespace CompanionServer;

public static class Server
{
	private class RegisterResponse
	{
		public string ServerId;

		public string ServerToken;

		public string Secret;
	}

	private class TestConnectionResponse
	{
		public List<string> Messages;
	}

	public static readonly ChatLog TeamChat = new ChatLog();

	internal static string Token;

	internal static string Secret;

	public static Listener Listener { get; private set; }

	public static bool IsEnabled
	{
		get
		{
			if (App.port >= 0 && !string.IsNullOrWhiteSpace(App.serverid))
			{
				return Listener != null;
			}
			return false;
		}
	}

	public static void Initialize(bool minimal = false)
	{
		if (App.port < 0)
		{
			return;
		}
		if (IsEnabled)
		{
			Debug.LogWarning("Rust+ is already started up! Skipping second startup");
			return;
		}
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if (!(activeGameMode != null) || activeGameMode.rustPlus)
		{
			Shutdown();
			Map.PopulateCache();
			if (App.port == 0)
			{
				App.port = Math.Max(ConVar.Server.port, RCon.Port) + 67;
			}
			try
			{
				Listener = new Listener(App.GetListenIP(), App.port);
			}
			catch (Exception arg)
			{
				Debug.LogError($"Companion server failed to start: {arg}");
			}
			if (!minimal)
			{
				PostInitializeServer();
			}
		}
	}

	public static void Shutdown()
	{
		SetServerId(null);
		Listener?.Dispose();
		Listener = null;
	}

	public static bool Update()
	{
		return Listener?.Update() ?? false;
	}

	[PoolAnalyzerNonCaching]
	public static void Broadcast(PlayerTarget target, AppBroadcast broadcast)
	{
		Listener?.PlayerSubscribers?.Send(target, broadcast);
	}

	[PoolAnalyzerNonCaching]
	public static void Broadcast(EntityTarget target, AppBroadcast broadcast)
	{
		Listener?.EntitySubscribers?.Send(target, broadcast);
	}

	[PoolAnalyzerNonCaching]
	public static void Broadcast(ClanTarget target, AppBroadcast broadcast)
	{
		Listener?.ClanSubscribers?.Send(target, broadcast);
	}

	[PoolAnalyzerNonCaching]
	public static void Broadcast(CameraTarget target, AppBroadcast broadcast)
	{
		Listener?.CameraSubscribers?.Send(target, broadcast);
	}

	public static bool HasAnySubscribers(CameraTarget target)
	{
		return (Listener?.CameraSubscribers?.HasAnySubscribers(target)).GetValueOrDefault();
	}

	public static bool CanSendPairingNotification(ulong playerId)
	{
		return Listener?.CanSendPairingNotification(playerId) ?? false;
	}

	private static async void PostInitializeServer()
	{
		string text = await App.GetPublicIPAsync();
		if (string.IsNullOrEmpty(text))
		{
			Debug.LogError("Failed to determine public IP address for Rust+ while setting up registration. Disabling Rust+ features because we wouldn't know what IP to tell Rust+ to connect to.");
			SetServerId(null);
		}
		else
		{
			await SetupServerRegistration(text, App.port);
			await CheckConnectivity();
		}
	}

	private static async Task SetupServerRegistration(string address, int port)
	{
		string arg = Uri.EscapeDataString(ConVar.Server.hostname ?? string.Empty);
		string query = $"?address={address}&port={port}&name={arg}";
		try
		{
			if (TryLoadServerRegistration(out var _, out var serverToken))
			{
				StringContent refreshContent = new StringContent(serverToken, Encoding.UTF8, "text/plain");
				HttpResponseMessage httpResponseMessage = await AutoRetry(() => WebUtil.HttpClient.PostAsync(App.endpoint + "/server/refresh" + query, refreshContent));
				if (httpResponseMessage.IsSuccessStatusCode)
				{
					SetServerRegistration(await httpResponseMessage.Content.ReadAsStringAsync());
					return;
				}
				Debug.LogWarning("Failed to refresh server ID - registering a new one");
			}
			HttpResponseMessage obj = await AutoRetry(() => WebUtil.HttpClient.GetAsync(App.endpoint + "/server/register" + query));
			obj.EnsureSuccessStatusCode();
			SetServerRegistration(await obj.Content.ReadAsStringAsync());
		}
		catch (Exception arg2)
		{
			Debug.LogError($"Failed to setup companion server registration: {arg2}");
		}
	}

	private static bool TryLoadServerRegistration(out string serverId, out string serverToken)
	{
		serverId = null;
		serverToken = null;
		string serverIdPath = GetServerIdPath();
		if (!File.Exists(serverIdPath))
		{
			return false;
		}
		try
		{
			RegisterResponse registerResponse = JsonConvert.DeserializeObject<RegisterResponse>(File.ReadAllText(serverIdPath));
			serverId = registerResponse.ServerId;
			serverToken = registerResponse.ServerToken;
			return true;
		}
		catch (Exception arg)
		{
			Debug.LogError($"Failed to load companion server registration: {arg}");
			return false;
		}
	}

	private static void SetServerRegistration(string responseJson)
	{
		RegisterResponse registerResponse = null;
		try
		{
			registerResponse = JsonConvert.DeserializeObject<RegisterResponse>(responseJson);
		}
		catch (Exception arg)
		{
			Debug.LogError($"Failed to parse registration response JSON: {responseJson}\n\n{arg}");
		}
		SetServerId(registerResponse?.ServerId);
		Token = registerResponse?.ServerToken;
		Secret = registerResponse?.Secret;
		if (registerResponse == null)
		{
			return;
		}
		try
		{
			File.WriteAllText(GetServerIdPath(), responseJson);
		}
		catch (Exception arg2)
		{
			Debug.LogError($"Unable to save companion app server registration - server ID may be different after restart: {arg2}");
		}
	}

	private static async Task CheckConnectivity()
	{
		if (!IsEnabled)
		{
			Shutdown();
			return;
		}
		try
		{
			if (string.IsNullOrEmpty(Token))
			{
				Debug.LogWarning("Skipping Rust+ connectivity test because the server is not registered.");
				return;
			}
			StringContent testContent = new StringContent(Token, Encoding.UTF8, "text/plain");
			HttpResponseMessage testResponse = await AutoRetry(() => WebUtil.HttpClient.PostAsync(App.endpoint + "/server/test_connection", testContent));
			string text = await testResponse.Content.ReadAsStringAsync();
			TestConnectionResponse testConnectionResponse = null;
			try
			{
				testConnectionResponse = JsonConvert.DeserializeObject<TestConnectionResponse>(text);
			}
			catch (Exception arg)
			{
				Debug.LogError($"Failed to parse connectivity test response JSON: {text}\n\n{arg}");
			}
			if (testConnectionResponse == null)
			{
				return;
			}
			IEnumerable<string> messages = testConnectionResponse.Messages;
			string text2 = string.Join("\n", messages ?? Enumerable.Empty<string>());
			if (testResponse.StatusCode == (HttpStatusCode)555)
			{
				Debug.LogError("Rust+ companion server connectivity test failed! Disabling Rust+ features.\n\n" + text2);
				SetServerId(null);
				return;
			}
			testResponse.EnsureSuccessStatusCode();
			if (!string.IsNullOrWhiteSpace(text2))
			{
				Debug.LogWarning("Rust+ companion server connectivity test has warnings:\n" + text2);
			}
		}
		catch (Exception arg2)
		{
			Debug.LogError($"Failed to check connectivity to the companion server: {arg2}");
		}
	}

	private static async Task<HttpResponseMessage> AutoRetry(Func<Task<HttpResponseMessage>> action)
	{
		Exception lastException = null;
		for (int i = 0; i < 5; i++)
		{
			try
			{
				HttpResponseMessage httpResponseMessage = await action();
				int statusCode = (int)httpResponseMessage.StatusCode;
				if (statusCode != 555 && statusCode >= 500 && statusCode <= 599 && i < 4)
				{
					httpResponseMessage.EnsureSuccessStatusCode();
				}
				return httpResponseMessage;
			}
			catch (Exception ex)
			{
				lastException = ex;
			}
			await Task.Delay(30000);
		}
		throw lastException ?? new Exception("Exceeded maximum number of retries");
	}

	private static void SetServerId(string serverId)
	{
		ConsoleSystem.Index.Server.Find("app.serverid")?.Set(serverId ?? "");
	}

	private static string GetServerIdPath()
	{
		return Path.Combine(ConVar.Server.rootFolder, "companion.id");
	}
}
