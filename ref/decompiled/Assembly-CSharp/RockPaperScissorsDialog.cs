using Rust.UI;
using UnityEngine;

public class RockPaperScissorsDialog : SingletonComponent<RockPaperScissorsDialog>
{
	public Canvas Canvas;

	public RustText[] InputTexts;

	public RustSlider TimerBar;

	public GameObject ActiveRoot;

	public GameObject[] MadeSelection;
}
