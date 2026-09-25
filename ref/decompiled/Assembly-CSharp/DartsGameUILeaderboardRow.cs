using System;
using System.Collections.Generic;
using ProtoBuf;
using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class DartsGameUILeaderboardRow : FacepunchBehaviour
{
	public RustText PlayerName;

	public RustText DartsThrown;

	public RustText TimeTaken;

	[Header("Row Styles")]
	[Tooltip("The image tinted with the colours below - light, dark, or current winner.")]
	public Image rowImage;

	public Color lightColour = Color.white;

	public Color darkColour = Color.grey;

	public Color currentWinnerColour = Color.yellow;

	[Header("Text Colours")]
	[Tooltip("Every text in here gets the normal colour, or the winner colour on the current winner row.")]
	public List<RustText> rowTexts = new List<RustText>();

	public Color normalTextColour = Color.white;

	public Color winnerTextColour = Color.black;

	[Tooltip("Turned on for the current winner row only, off on every other row.")]
	public GameObject currentWinnerIcon;

	public void SetLeaderboardRowStats(DartsGameLeaderboard.DartsGameLeaderboardEntry leaderboardEntry, int position)
	{
		string text = NameHelper.Get(leaderboardEntry.userid, leaderboardEntry.playerName);
		PlayerName.SetText(string.IsNullOrEmpty(text) ? leaderboardEntry.playerName : text);
		DartsThrown.SetText(leaderboardEntry.dartsThrown.ToString());
		TimeTaken.SetText(TimeSpan.FromSeconds(leaderboardEntry.timeTaken).ToString("m\\:ss"));
		bool flag = position == 0;
		Color color = (flag ? currentWinnerColour : ((position % 2 == 1) ? lightColour : darkColour));
		if (rowImage != null)
		{
			rowImage.color = color;
		}
		Color color2 = (flag ? winnerTextColour : normalTextColour);
		foreach (RustText rowText in rowTexts)
		{
			if (rowText != null)
			{
				rowText.color = color2;
			}
		}
		if (currentWinnerIcon != null)
		{
			currentWinnerIcon.SetActive(flag);
		}
	}
}
