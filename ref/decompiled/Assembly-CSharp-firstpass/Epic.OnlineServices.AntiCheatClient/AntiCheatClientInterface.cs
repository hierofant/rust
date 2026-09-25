using System;

namespace Epic.OnlineServices.AntiCheatClient;

public sealed class AntiCheatClientInterface : Handle
{
	private static byte[] PollStatusStaticBuffer = new byte[256];

	public const int ADDEXTERNALINTEGRITYCATALOG_API_LATEST = 1;

	public const int ADDNOTIFYCLIENTINTEGRITYVIOLATED_API_LATEST = 1;

	public const int ADDNOTIFYMESSAGETOPEER_API_LATEST = 1;

	public const int ADDNOTIFYMESSAGETOSERVER_API_LATEST = 1;

	public const int ADDNOTIFYPEERACTIONREQUIRED_API_LATEST = 1;

	public const int ADDNOTIFYPEERAUTHSTATUSCHANGED_API_LATEST = 1;

	public const int BEGINSESSION_API_LATEST = 3;

	public const int ENDSESSION_API_LATEST = 1;

	public const int GETPROTECTMESSAGEOUTPUTLENGTH_API_LATEST = 1;

	public const int ONMESSAGETOPEERCALLBACK_MAX_MESSAGE_SIZE = 512;

	public const int ONMESSAGETOSERVERCALLBACK_MAX_MESSAGE_SIZE = 512;

	public static readonly IntPtr PEER_SELF = (IntPtr)(-1);

	public const int POLLSTATUS_API_LATEST = 1;

	public const int PROTECTMESSAGE_API_LATEST = 1;

	public const int RECEIVEMESSAGEFROMPEER_API_LATEST = 1;

	public const int RECEIVEMESSAGEFROMSERVER_API_LATEST = 1;

	public const int REGISTERPEER_API_LATEST = 3;

	public const int REGISTERPEER_MAX_AUTHENTICATIONTIMEOUT = 120;

	public const int REGISTERPEER_MIN_AUTHENTICATIONTIMEOUT = 40;

	public const int RESERVED01_API_LATEST = 1;

	public const int UNPROTECTMESSAGE_API_LATEST = 1;

	public const int UNREGISTERPEER_API_LATEST = 1;

	public AntiCheatClientInterface()
	{
	}

	public AntiCheatClientInterface(IntPtr innerHandle)
		: base(innerHandle)
	{
	}

	public Result AddExternalIntegrityCatalog(ref AddExternalIntegrityCatalogOptions options)
	{
		AddExternalIntegrityCatalogOptionsInternal options2 = default(AddExternalIntegrityCatalogOptionsInternal);
		options2.Set(ref options);
		Result result = Bindings.EOS_AntiCheatClient_AddExternalIntegrityCatalog(base.InnerHandle, ref options2);
		Helper.Dispose(ref options2);
		return result;
	}

	public ulong AddNotifyClientIntegrityViolated(ref AddNotifyClientIntegrityViolatedOptions options, object clientData, OnClientIntegrityViolatedCallback notificationFn)
	{
		if (notificationFn == null)
		{
			throw new ArgumentNullException("notificationFn");
		}
		AddNotifyClientIntegrityViolatedOptionsInternal options2 = default(AddNotifyClientIntegrityViolatedOptionsInternal);
		options2.Set(ref options);
		IntPtr clientDataPointer = IntPtr.Zero;
		Helper.AddCallback(out clientDataPointer, clientData, notificationFn);
		ulong num = Bindings.EOS_AntiCheatClient_AddNotifyClientIntegrityViolated(base.InnerHandle, ref options2, clientDataPointer, OnClientIntegrityViolatedCallbackInternalImplementation.Delegate);
		Helper.Dispose(ref options2);
		Helper.AssignNotificationIdToCallback(clientDataPointer, num);
		return num;
	}

	public ulong AddNotifyMessageToPeer(ref AddNotifyMessageToPeerOptions options, object clientData, OnMessageToPeerCallback notificationFn)
	{
		if (notificationFn == null)
		{
			throw new ArgumentNullException("notificationFn");
		}
		AddNotifyMessageToPeerOptionsInternal options2 = default(AddNotifyMessageToPeerOptionsInternal);
		options2.Set(ref options);
		IntPtr clientDataPointer = IntPtr.Zero;
		Helper.AddCallback(out clientDataPointer, clientData, notificationFn);
		ulong num = Bindings.EOS_AntiCheatClient_AddNotifyMessageToPeer(base.InnerHandle, ref options2, clientDataPointer, OnMessageToPeerCallbackInternalImplementation.Delegate);
		Helper.Dispose(ref options2);
		Helper.AssignNotificationIdToCallback(clientDataPointer, num);
		return num;
	}

	public ulong AddNotifyMessageToServer(ref AddNotifyMessageToServerOptions options, object clientData, OnMessageToServerCallback notificationFn)
	{
		if (notificationFn == null)
		{
			throw new ArgumentNullException("notificationFn");
		}
		AddNotifyMessageToServerOptionsInternal options2 = default(AddNotifyMessageToServerOptionsInternal);
		options2.Set(ref options);
		IntPtr clientDataPointer = IntPtr.Zero;
		Helper.AddCallback(out clientDataPointer, clientData, notificationFn);
		ulong num = Bindings.EOS_AntiCheatClient_AddNotifyMessageToServer(base.InnerHandle, ref options2, clientDataPointer, OnMessageToServerCallbackInternalImplementation.Delegate);
		Helper.Dispose(ref options2);
		Helper.AssignNotificationIdToCallback(clientDataPointer, num);
		return num;
	}

	public ulong AddNotifyPeerActionRequired(ref AddNotifyPeerActionRequiredOptions options, object clientData, OnPeerActionRequiredCallback notificationFn)
	{
		if (notificationFn == null)
		{
			throw new ArgumentNullException("notificationFn");
		}
		AddNotifyPeerActionRequiredOptionsInternal options2 = default(AddNotifyPeerActionRequiredOptionsInternal);
		options2.Set(ref options);
		IntPtr clientDataPointer = IntPtr.Zero;
		Helper.AddCallback(out clientDataPointer, clientData, notificationFn);
		ulong num = Bindings.EOS_AntiCheatClient_AddNotifyPeerActionRequired(base.InnerHandle, ref options2, clientDataPointer, OnPeerActionRequiredCallbackInternalImplementation.Delegate);
		Helper.Dispose(ref options2);
		Helper.AssignNotificationIdToCallback(clientDataPointer, num);
		return num;
	}

	public ulong AddNotifyPeerAuthStatusChanged(ref AddNotifyPeerAuthStatusChangedOptions options, object clientData, OnPeerAuthStatusChangedCallback notificationFn)
	{
		if (notificationFn == null)
		{
			throw new ArgumentNullException("notificationFn");
		}
		AddNotifyPeerAuthStatusChangedOptionsInternal options2 = default(AddNotifyPeerAuthStatusChangedOptionsInternal);
		options2.Set(ref options);
		IntPtr clientDataPointer = IntPtr.Zero;
		Helper.AddCallback(out clientDataPointer, clientData, notificationFn);
		ulong num = Bindings.EOS_AntiCheatClient_AddNotifyPeerAuthStatusChanged(base.InnerHandle, ref options2, clientDataPointer, OnPeerAuthStatusChangedCallbackInternalImplementation.Delegate);
		Helper.Dispose(ref options2);
		Helper.AssignNotificationIdToCallback(clientDataPointer, num);
		return num;
	}

	public Result BeginSession(ref BeginSessionOptions options)
	{
		BeginSessionOptionsInternal options2 = default(BeginSessionOptionsInternal);
		options2.Set(ref options);
		Result result = Bindings.EOS_AntiCheatClient_BeginSession(base.InnerHandle, ref options2);
		Helper.Dispose(ref options2);
		return result;
	}

	public Result EndSession(ref EndSessionOptions options)
	{
		EndSessionOptionsInternal options2 = default(EndSessionOptionsInternal);
		options2.Set(ref options);
		Result result = Bindings.EOS_AntiCheatClient_EndSession(base.InnerHandle, ref options2);
		Helper.Dispose(ref options2);
		return result;
	}

	public Result GetProtectMessageOutputLength(ref GetProtectMessageOutputLengthOptions options, out uint outBufferSizeBytes)
	{
		GetProtectMessageOutputLengthOptionsInternal options2 = default(GetProtectMessageOutputLengthOptionsInternal);
		options2.Set(ref options);
		Result result = Bindings.EOS_AntiCheatClient_GetProtectMessageOutputLength(base.InnerHandle, ref options2, out outBufferSizeBytes);
		Helper.Dispose(ref options2);
		return result;
	}

	public unsafe Result PollStatus(ref PollStatusOptions options, out AntiCheatClientViolationType outViolationType, out Utf8String outMessage)
	{
		outViolationType = AntiCheatClientViolationType.Invalid;
		outMessage = Utf8String.EmptyString;
		PollStatusOptionsInternal disposable = default(PollStatusOptionsInternal);
		disposable.Set(ref options);
		Result num;
		fixed (byte* value = PollStatusStaticBuffer)
		{
			num = Bindings.EOS_AntiCheatClient_PollStatus(outMessage: new IntPtr(value), handle: base.InnerHandle, options: ref disposable, outViolationType: out outViolationType);
			if (num == Result.Success)
			{
				outMessage = new Utf8String(PollStatusStaticBuffer);
			}
		}
		Helper.Dispose(ref disposable);
		return num;
	}

	public unsafe Result ProtectMessage(ref ProtectMessageOptions options, ArraySegment<byte> outBuffer, out uint outBytesWritten)
	{
		outBytesWritten = 0u;
		Result result;
		fixed (byte* ptr = options.Data.Array)
		{
			IntPtr data = IntPtr.Add((IntPtr)ptr, options.Data.Offset);
			ProtectMessageOptionsInternal options2 = default(ProtectMessageOptionsInternal);
			options2.m_ApiVersion = 1;
			options2.m_Data = data;
			options2.m_DataLengthBytes = (uint)options.Data.Count;
			options2.m_OutBufferSizeBytes = options.OutBufferSizeBytes;
			fixed (byte* ptr2 = outBuffer.Array)
			{
				IntPtr outBuffer2 = IntPtr.Add((IntPtr)ptr2, outBuffer.Offset);
				result = Bindings.EOS_AntiCheatClient_ProtectMessage(base.InnerHandle, ref options2, outBuffer2, out outBytesWritten);
			}
		}
		return result;
	}

	public Result ReceiveMessageFromPeer(ref ReceiveMessageFromPeerOptions options)
	{
		ReceiveMessageFromPeerOptionsInternal options2 = default(ReceiveMessageFromPeerOptionsInternal);
		options2.Set(ref options);
		Result result = Bindings.EOS_AntiCheatClient_ReceiveMessageFromPeer(base.InnerHandle, ref options2);
		Helper.Dispose(ref options2);
		return result;
	}

	public Result ReceiveMessageFromServer(ref ReceiveMessageFromServerOptions options)
	{
		ReceiveMessageFromServerOptionsInternal options2 = default(ReceiveMessageFromServerOptionsInternal);
		options2.Set(ref options);
		Result result = Bindings.EOS_AntiCheatClient_ReceiveMessageFromServer(base.InnerHandle, ref options2);
		Helper.Dispose(ref options2);
		return result;
	}

	public Result RegisterPeer(ref RegisterPeerOptions options)
	{
		RegisterPeerOptionsInternal options2 = default(RegisterPeerOptionsInternal);
		options2.Set(ref options);
		Result result = Bindings.EOS_AntiCheatClient_RegisterPeer(base.InnerHandle, ref options2);
		Helper.Dispose(ref options2);
		return result;
	}

	public void RemoveNotifyClientIntegrityViolated(ulong notificationId)
	{
		Bindings.EOS_AntiCheatClient_RemoveNotifyClientIntegrityViolated(base.InnerHandle, notificationId);
		Helper.RemoveCallbackByNotificationId(notificationId);
	}

	public void RemoveNotifyMessageToPeer(ulong notificationId)
	{
		Bindings.EOS_AntiCheatClient_RemoveNotifyMessageToPeer(base.InnerHandle, notificationId);
		Helper.RemoveCallbackByNotificationId(notificationId);
	}

	public void RemoveNotifyMessageToServer(ulong notificationId)
	{
		Bindings.EOS_AntiCheatClient_RemoveNotifyMessageToServer(base.InnerHandle, notificationId);
		Helper.RemoveCallbackByNotificationId(notificationId);
	}

	public void RemoveNotifyPeerActionRequired(ulong notificationId)
	{
		Bindings.EOS_AntiCheatClient_RemoveNotifyPeerActionRequired(base.InnerHandle, notificationId);
		Helper.RemoveCallbackByNotificationId(notificationId);
	}

	public void RemoveNotifyPeerAuthStatusChanged(ulong notificationId)
	{
		Bindings.EOS_AntiCheatClient_RemoveNotifyPeerAuthStatusChanged(base.InnerHandle, notificationId);
		Helper.RemoveCallbackByNotificationId(notificationId);
	}

	public Result Reserved01(ref Reserved01Options options, out int outValue)
	{
		Reserved01OptionsInternal options2 = default(Reserved01OptionsInternal);
		options2.Set(ref options);
		Result result = Bindings.EOS_AntiCheatClient_Reserved01(base.InnerHandle, ref options2, out outValue);
		Helper.Dispose(ref options2);
		return result;
	}

	public unsafe Result UnprotectMessage(ref UnprotectMessageOptions options, ArraySegment<byte> outBuffer, out uint outBytesWritten)
	{
		outBytesWritten = 0u;
		Result result;
		fixed (byte* ptr = options.Data.Array)
		{
			IntPtr data = IntPtr.Add((IntPtr)ptr, options.Data.Offset);
			UnprotectMessageOptionsInternal options2 = default(UnprotectMessageOptionsInternal);
			options2.m_ApiVersion = 1;
			options2.m_Data = data;
			options2.m_DataLengthBytes = (uint)options.Data.Count;
			options2.m_OutBufferSizeBytes = options.OutBufferSizeBytes;
			fixed (byte* ptr2 = outBuffer.Array)
			{
				IntPtr outBuffer2 = IntPtr.Add((IntPtr)ptr2, outBuffer.Offset);
				result = Bindings.EOS_AntiCheatClient_UnprotectMessage(base.InnerHandle, ref options2, outBuffer2, out outBytesWritten);
			}
		}
		return result;
	}

	public Result UnregisterPeer(ref UnregisterPeerOptions options)
	{
		UnregisterPeerOptionsInternal options2 = default(UnregisterPeerOptionsInternal);
		options2.Set(ref options);
		Result result = Bindings.EOS_AntiCheatClient_UnregisterPeer(base.InnerHandle, ref options2);
		Helper.Dispose(ref options2);
		return result;
	}
}
