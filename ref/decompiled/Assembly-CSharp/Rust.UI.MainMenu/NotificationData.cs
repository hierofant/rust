using System;

namespace Rust.UI.MainMenu;

[Serializable]
public struct NotificationData
{
	public NotificationType NotificationType;

	public string Title;

	public string NotificationText;

	public string NotificationLink;

	public bool IsInternal;

	public Translate.Phrase Phrase;

	public int? Id;

	public object[] PhraseArguments;

	public string SeenKey;

	public bool HasLink => !string.IsNullOrEmpty(NotificationLink);

	public NotificationData(NotificationType type, string text, string link = "", bool isInternal = true, Translate.Phrase phrase = null, int? id = null, string seenKey = null, string title = null, params object[] arguments)
	{
		NotificationType = type;
		Title = title;
		NotificationText = text;
		NotificationLink = link;
		IsInternal = isInternal;
		Phrase = phrase;
		Id = id;
		SeenKey = seenKey;
		PhraseArguments = arguments;
	}

	public override bool Equals(object obj)
	{
		if (obj is NotificationData notificationData)
		{
			if (NotificationText == notificationData.NotificationText)
			{
				return NotificationLink == notificationData.NotificationLink;
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (NotificationText?.GetHashCode() ?? 0) + (NotificationLink?.GetHashCode() ?? 0);
	}
}
