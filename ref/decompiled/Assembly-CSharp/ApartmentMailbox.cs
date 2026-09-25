using System;
using Facepunch;
using ProtoBuf;
using TMPro;
using UnityEngine;

public class ApartmentMailbox : Mailbox, LootPanel.IHasLootPanel
{
	[Header("Apartment Mailbox")]
	public string RoomNumber;

	public TextMeshPro TextMesh;

	[NonSerialized]
	public ApartmentRoom Room;

	private static Translate.Phrase mailboxPanelTitlePhrase = new Translate.Phrase("apartment.mailbox.lootpanel.title", "Mailbox of room {0}");

	Translate.Phrase LootPanel.IHasLootPanel.LootPanelTitle => string.Format(mailboxPanelTitlePhrase.translated, RoomNumber);

	public override bool PlayerIsOwner(BasePlayer player)
	{
		if (Room != null)
		{
			return Room.IsAuthed(player.userID);
		}
		return false;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.apartmentMailbox = Pool.Get<ProtoBuf.ApartmentMailbox>();
		info.msg.apartmentMailbox.roomNumber = RoomNumber;
		info.msg.apartmentMailbox.roomId = ((Room != null) ? Room.net.ID : default(NetworkableId));
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.apartmentMailbox != null)
		{
			RoomNumber = info.msg.apartmentMailbox.roomNumber;
		}
	}
}
