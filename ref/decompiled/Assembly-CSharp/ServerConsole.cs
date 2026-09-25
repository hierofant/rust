using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ConVar;
using Facepunch;
using Facepunch.Extend;
using Network;
using UnityEngine;
using Windows;

public class ServerConsole : SingletonComponent<ServerConsole>
{
	private struct ConsoleMessage
	{
		public string Message;

		public string StackTrace;

		public LogType Type;

		public ConsoleColor? Color;

		public List<string> StatusUpdate;

		public ConsoleMessage(string message, string stackTrace, LogType type)
		{
			Message = message;
			StackTrace = stackTrace;
			Type = type;
			StatusUpdate = null;
			Color = null;
		}
	}

	public ConsoleWindow console;

	public ConsoleInput input;

	private CancellationTokenSource logThreadCancellation;

	private static bool ignoreLogs;

	private ConcurrentQueue<ConsoleMessage> queuedMessages = new ConcurrentQueue<ConsoleMessage>();

	private ConcurrentQueue<string> queuedCommands = new ConcurrentQueue<string>();

	private float nextUpdate;

	private static bool consoleEnabled => !CommandLine.HasSwitch("-noconsole");

	private DateTime currentGameTime
	{
		get
		{
			if (!TOD_Sky.Instance)
			{
				return DateTime.Now;
			}
			return TOD_Sky.Instance.Cycle.DateTime;
		}
	}

	private int currentPlayerCount => BasePlayer.activePlayerList.Count;

	private int maxPlayerCount => ConVar.Server.maxplayers;

	private int currentEntityCount => BaseNetworkable.serverEntities.Count;

	private int currentSleeperCount => BasePlayer.sleepingPlayerList.Count;

	public void OnEnable()
	{
		if (!consoleEnabled)
		{
			base.enabled = false;
			return;
		}
		console = new ConsoleWindow();
		input = new ConsoleInput();
		console.Initialize();
		input.OnInputText += OnInputText;
		Output.OnMessage += HandleLog;
		input.ClearLine(System.Console.WindowHeight);
		for (int i = 0; i < System.Console.WindowHeight; i++)
		{
			System.Console.WriteLine("");
		}
		if (logThreadCancellation != null)
		{
			logThreadCancellation.Cancel();
			logThreadCancellation.Dispose();
		}
		logThreadCancellation = new CancellationTokenSource();
		Task.Run(async delegate
		{
			await LogThread(logThreadCancellation.Token);
		});
	}

	private void OnDisable()
	{
		if (logThreadCancellation != null)
		{
			logThreadCancellation.Cancel();
			logThreadCancellation.Dispose();
			logThreadCancellation = null;
		}
		Output.OnMessage -= HandleLog;
		if (input != null)
		{
			input.OnInputText -= OnInputText;
		}
		console?.Shutdown();
	}

	private void OnInputText(string obj)
	{
		queuedCommands.Enqueue(obj);
	}

	public static void PrintColoured(string text, ConsoleColor color)
	{
		ignoreLogs = true;
		DebugEx.Log(text);
		ignoreLogs = false;
		if (!(SingletonComponent<ServerConsole>.Instance == null) && SingletonComponent<ServerConsole>.Instance.input != null)
		{
			SingletonComponent<ServerConsole>.Instance.queuedMessages.Enqueue(new ConsoleMessage(text, null, LogType.Log)
			{
				Color = color
			});
		}
	}

	private void HandleLog(string message, string stackTrace, LogType type)
	{
		if (!ignoreLogs)
		{
			queuedMessages.Enqueue(new ConsoleMessage(message, stackTrace, type));
		}
	}

	private async Task LogThread(CancellationToken cancellationToken)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			try
			{
				if (queuedMessages.Count > 0)
				{
					bool flag = false;
					ConsoleMessage result;
					while (queuedMessages.TryDequeue(out result))
					{
						if (result.StatusUpdate != null)
						{
							for (int i = 0; i < result.StatusUpdate.Count; i++)
							{
								input.statusText[i] = result.StatusUpdate[i];
							}
							Facepunch.Pool.FreeUnmanaged(ref result.StatusUpdate);
							continue;
						}
						if (!flag)
						{
							input.ClearLine(input.statusText.Length + 1);
							flag = true;
						}
						PrintMessage(result.Message, result.Type, result.Color);
					}
					if (System.Console.CursorTop == System.Console.BufferHeight - 1)
					{
						System.Console.WriteLine();
					}
					if (flag)
					{
						input.RedrawInputLine(clear: false);
					}
					System.Console.CursorVisible = false;
					input.RedrawStatusText();
					System.Console.CursorVisible = true;
					input.FixBottomOfBuffer();
				}
				input?.Update();
			}
			catch (Exception arg)
			{
				System.Console.WriteLine($"Console Thread Error: {arg}");
			}
			await Task.Delay(20);
		}
	}

	private void PrintMessage(string message, LogType type, ConsoleColor? colorOverride)
	{
		if (message == null || message.StartsWith("[CHAT]") || message.StartsWith("[TEAM CHAT]") || message.StartsWith("[CARDS CHAT]"))
		{
			return;
		}
		ConsoleColor foregroundColor = System.Console.ForegroundColor;
		if (colorOverride.HasValue)
		{
			try
			{
				System.Console.ForegroundColor = colorOverride.Value;
			}
			catch
			{
				System.Console.ForegroundColor = ConsoleColor.Gray;
			}
		}
		else
		{
			switch (type)
			{
			case LogType.Warning:
				if (message.StartsWith("HDR RenderTexture format is not") || message.StartsWith("The image effect") || message.StartsWith("Image Effects are not supported on this platform") || message.StartsWith("[AmplifyColor]") || message.StartsWith("Skipping profile frame.") || message.StartsWith("Kinematic body only supports Speculative Continuous collision detection"))
				{
					return;
				}
				System.Console.ForegroundColor = ConsoleColor.Yellow;
				break;
			case LogType.Error:
				System.Console.ForegroundColor = ConsoleColor.Red;
				break;
			case LogType.Exception:
				System.Console.ForegroundColor = ConsoleColor.Red;
				break;
			case LogType.Assert:
				System.Console.ForegroundColor = ConsoleColor.Red;
				break;
			default:
				System.Console.ForegroundColor = ConsoleColor.Gray;
				break;
			}
		}
		if (input != null)
		{
			System.Console.WriteLine(message);
		}
		System.Console.ForegroundColor = foregroundColor;
	}

	private void Update()
	{
		UpdateStatus();
		string result;
		while (queuedCommands.TryDequeue(out result))
		{
			ConsoleSystem.Run(ConsoleSystem.Option.Server.FromServerConsole(), result);
		}
	}

	private void UpdateStatus()
	{
		if (!(nextUpdate > UnityEngine.Time.realtimeSinceStartup) && Network.Net.sv != null && Network.Net.sv.IsConnected())
		{
			nextUpdate = UnityEngine.Time.realtimeSinceStartup + 0.33f;
			if (input != null && input.valid)
			{
				string text = ((long)UnityEngine.Time.realtimeSinceStartup).FormatSeconds();
				string text2 = currentGameTime.ToString("[H:mm]");
				string text3 = " " + text2 + " [" + currentPlayerCount + "/" + maxPlayerCount + "] " + ConVar.Server.hostname + " [" + ConVar.Server.level + "]";
				string text4 = Performance.current.frameRate + "fps " + Performance.current.memoryCollections + "gc " + text;
				string text5 = Network.Net.sv.GetStat(null, BaseNetwork.StatTypeLong.BytesReceived_LastSecond).FormatBytes(shortFormat: true) + "/s in, " + Network.Net.sv.GetStat(null, BaseNetwork.StatTypeLong.BytesSent_LastSecond).FormatBytes(shortFormat: true) + "/s out";
				string text6 = text4.PadLeft(input.lineWidth - 1);
				text6 = text3 + ((text3.Length < text6.Length) ? text6.Substring(text3.Length) : "");
				string text7 = " " + currentEntityCount.ToString("n0") + " ents, " + currentSleeperCount.ToString("n0") + " slprs";
				string text8 = text5.PadLeft(input.lineWidth - 1);
				text8 = text7 + ((text7.Length < text8.Length) ? text8.Substring(text7.Length) : "");
				ConsoleMessage consoleMessage = default(ConsoleMessage);
				consoleMessage.StatusUpdate = Facepunch.Pool.Get<List<string>>();
				ConsoleMessage item = consoleMessage;
				item.StatusUpdate.Add("");
				item.StatusUpdate.Add(text6);
				item.StatusUpdate.Add(text8);
				queuedMessages.Enqueue(item);
			}
		}
	}
}
