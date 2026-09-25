using System;
using Facepunch.Nexus;
using Facepunch.Nexus.Models;

public class NexusClanEventHandler : INexusClanEventListener
{
	private readonly NexusClanBackend _backend;

	private readonly IClanChangeSink _changeSink;

	public NexusClanEventHandler(NexusClanBackend backend, IClanChangeSink changeSink)
	{
		_backend = backend ?? throw new ArgumentNullException("backend");
		_changeSink = changeSink ?? throw new ArgumentNullException("changeSink");
	}

	public void OnDisbanded(in ClanDisbandedEvent args)
	{
		_changeSink.ClanDisbanded(args.ClanId);
		foreach (ulong member in args.Members)
		{
			_changeSink.MembershipChanged(member, null);
		}
	}

	public void OnInvitation(in ClanInvitedEvent args)
	{
		_changeSink.InvitationCreated(args.PlayerId, args.ClanId);
	}

	public void OnJoined(in ClanJoinedEvent args)
	{
		_changeSink.MembershipChanged(args.PlayerId, args.ClanId);
	}

	public void OnKicked(in ClanKickedEvent args)
	{
		_changeSink.MembershipChanged(args.PlayerId, null);
	}

	public void OnChanged(in ClanChangedEvent args)
	{
		_backend.UpdateWrapper(args.ClanId);
		_changeSink.ClanChanged(args.ClanId, ClanDataSource.All);
	}

	public void OnUnload(in long clanId)
	{
		_backend.RemoveWrapper(clanId);
	}

	void INexusClanEventListener.OnDisbanded(in ClanDisbandedEvent args)
	{
		OnDisbanded(in args);
	}

	void INexusClanEventListener.OnInvitation(in ClanInvitedEvent args)
	{
		OnInvitation(in args);
	}

	void INexusClanEventListener.OnJoined(in ClanJoinedEvent args)
	{
		OnJoined(in args);
	}

	void INexusClanEventListener.OnKicked(in ClanKickedEvent args)
	{
		OnKicked(in args);
	}

	void INexusClanEventListener.OnChanged(in ClanChangedEvent args)
	{
		OnChanged(in args);
	}

	void INexusClanEventListener.OnUnload(in long clanId)
	{
		OnUnload(in clanId);
	}
}
