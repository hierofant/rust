using System.Text;
using Rust.UI;

public class NucleusInformationPanel : ItemInformationPanel
{
	public InfoBar xpDisplay;

	public RustText gradeLabel;

	public RustText nextLevelLabel;

	public static readonly Translate.Phrase GradePhrase = new Translate.Phrase("nucleus.grade", "GRADE {0}");

	public static readonly Translate.Phrase XPPhrase = new Translate.Phrase("nucleus.xp", "{0} XP");

	public static readonly Translate.Phrase XPRequiredPhrase = new Translate.Phrase("nucleus.required", "{0} XP REQUIRED");

	public static readonly Translate.Phrase MaxPhrase = new Translate.Phrase("nucleus.max", "MAX LEVEL");

	private static StringBuilder builder;
}
