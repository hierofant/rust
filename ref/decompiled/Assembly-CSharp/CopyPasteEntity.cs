#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ConVar;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class CopyPasteEntity : PointEntity
{
	public static CopyPasteEntity ServerInstance;

	public const string ClientDirectory = "copypaste";

	public const string FileExtension = ".data";

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("CopyPasteEntity.OnRpcMessage"))
		{
			if (rpc == 2913956655u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Paste");
				}
				using (TimeWarning.New("Paste"))
				{
					using (msg.read.SuspendProtoFieldOrderValidation())
					{
						using (msg.read.SuspendProtoFieldOperationLimit())
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
									Paste(rpc2);
								}
							}
							catch (Exception exception)
							{
								Debug.LogException(exception);
								player.Kick("RPC Error in Paste");
							}
						}
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public static string MakeFilenameSafe(string input)
	{
		return Regex.Replace(input, "[^a-zA-Z0-9_\\- ]", "");
	}

	public static string GetClientPath(string fileName)
	{
		return Path.Combine("copypaste", MakeFilenameSafe(fileName) + ".data");
	}

	public static byte[] LoadScreenshot(string fileName)
	{
		string clientPath = GetClientPath(fileName);
		if (!File.Exists(clientPath))
		{
			return null;
		}
		using CopyPasteEntityInfo copyPasteEntityInfo = CopyPasteEntityInfo.Deserialize(File.ReadAllBytes(clientPath));
		return copyPasteEntityInfo.screenshot;
	}

	public static string[] GetLocalPasteNames()
	{
		return (from x in Directory.GetFiles("copypaste", "*.data")
			select Path.GetFileNameWithoutExtension(x)).ToArray();
	}

	[RPC_Server.IgnoreProtoFieldOperationLimit]
	[RPC_Server.IgnoreProtoFieldOrder]
	[RPC_Server]
	public void Paste(RPCMessage rpc)
	{
		if (!rpc.player.IsAdmin)
		{
			return;
		}
		PasteRequest pasteRequest = rpc.read.Proto<PasteRequest>();
		CopyPasteEntityInfo pasteData = pasteRequest.pasteData;
		CopyPaste.PasteOptions pasteOptions = new CopyPaste.PasteOptions(pasteRequest);
		List<BaseEntity> list = new List<BaseEntity>();
		List<Vector3> list2 = new List<Vector3>();
		if (pasteRequest.pasteOffsets == null || pasteRequest.pasteOffsets.Count == 0)
		{
			list2.Add(Vector3.zero);
		}
		else
		{
			list2.AddRange(pasteRequest.pasteOffsets);
		}
		foreach (Vector3 item in list2)
		{
			pasteOptions.HeightOffset = new Vector3(0f, pasteOptions.HeightOffset.y, 0f) + item;
			List<BaseEntity> list3 = CopyPaste.PasteEntities(pasteData, pasteOptions, rpc.player.userID);
			if (pasteOptions.Players)
			{
				foreach (BaseEntity item2 in list3)
				{
					if (item2 is BasePlayer)
					{
						item2.ForceUpdateTriggers(enter: true, exit: false, invoke: false);
					}
				}
			}
			list.AddRange(list3);
		}
		if (list.Count > 0)
		{
			CopyPaste.playerHistory.AddToHistory(rpc.player.userID, list);
		}
		if (list2.Count == 1)
		{
			rpc.player.ConsoleMessage($"Pasted {list.Count} entities");
		}
		else
		{
			rpc.player.ConsoleMessage($"Pasted {list.Count} entities ({list2.Count} groups)");
		}
	}

	public void OnEnable()
	{
		if (base.isServer)
		{
			if (ServerInstance != null)
			{
				Debug.LogError("Major fuckup! CopyPasteEntity spawned twice, Contact Developers!");
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else
			{
				ServerInstance = this;
			}
		}
	}

	public void OnDestroy()
	{
		if (base.isServer)
		{
			ServerInstance = null;
		}
	}
}
