using Rust.UI;
using UnityEngine;

public class DartsGameUIScoreRow : FacepunchBehaviour
{
	public RustText ScoreText;

	public Color baseColour;

	public Color crossedOutColour;

	public void SetText(int score)
	{
		SetText(score.ToString());
	}

	public void SetText(string text)
	{
		ScoreText.SetText(text);
		ScoreText.color = baseColour;
	}

	public void CrossOutText()
	{
		ScoreText.SetText("<s>" + ScoreText.text + "</s>");
		ScoreText.color = crossedOutColour;
	}
}
