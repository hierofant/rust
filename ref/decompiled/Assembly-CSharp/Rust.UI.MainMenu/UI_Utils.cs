namespace Rust.UI.MainMenu;

public static class UI_Utils
{
	private static Translate.Phrase _monthSingularPhrase = new Translate.Phrase("time.month", "month");

	private static Translate.Phrase _monthsPhrase = new Translate.Phrase("time.months", "months");

	private static Translate.Phrase _weekSingularPhrase = new Translate.Phrase("time.week", "week");

	private static Translate.Phrase _weeksPhrase = new Translate.Phrase("time.weeks", "weeks");

	private static Translate.Phrase _daysSingularPhrase = new Translate.Phrase("time.day", "day");

	private static Translate.Phrase _daysPhrase = new Translate.Phrase("time.days", "days");

	private static Translate.Phrase _hourSingularPhrase = new Translate.Phrase("time.hour", "hour");

	private static Translate.Phrase _hoursPhrase = new Translate.Phrase("time.hours", "hours");

	private static Translate.Phrase _minuteSingularPhrase = new Translate.Phrase("time.minute", "minute");

	private static Translate.Phrase _minutesPhrase = new Translate.Phrase("time.minutes", "minutes");

	private static Translate.Phrase _secondSingularPhrase = new Translate.Phrase("time.second", "second");

	private static Translate.Phrase _secondsPhrase = new Translate.Phrase("time.seconds", "seconds");

	private static bool HasStreamerMode(BasePlayer ply = null)
	{
		if (ply != null)
		{
			return ply.net.connection.info.GetBool("global.streamermode");
		}
		return false;
	}

	public static string StreamerModeSanitize(string text, BasePlayer ply = null)
	{
		if (HasStreamerMode(ply))
		{
			return "STREAMER MODE ENABLED";
		}
		return text;
	}

	public static string StreamerModeSanitizeCustomMessage(string text, string message, BasePlayer ply = null)
	{
		if (HasStreamerMode(ply))
		{
			return message;
		}
		return text;
	}

	public static string StreamerModeSanitizeShort(string text, BasePlayer ply = null)
	{
		if (HasStreamerMode(ply))
		{
			return "?";
		}
		return text;
	}
}
