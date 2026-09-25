using Rust.UI;

public class UIClanRankCreator : BaseMonoBehaviour
{
	public static readonly Translate.Phrase CreateRankFailure = new Translate.Phrase("clan.create_rank.fail", "Failed to create the new rank.");

	public static readonly Translate.Phrase CreateRankDuplicate = new Translate.Phrase("clan.create_rank.duplicate", "There is already a rank in your clan with that name.");

	public static readonly Translate.Phrase CreateRankInvalidLength = new Translate.Phrase("clan.create_rank.name_invalid_length", "Clan rank names must be between {0} and {1} characters long.");

	public static readonly Translate.Phrase CreateRankNameInvalidCharacters = new Translate.Phrase("clan.create_rank.name_invalid_characters", "Clan rank names can only contain letters, numbers, spaces, apostrophes, and hyphens.");

	public static readonly Translate.Phrase CreateRankNameInvalid = new Translate.Phrase("clan.create_rank.name_invalid", "The clan rank name you typed in is not valid.");

	public UIClans UiClans;

	public RustInput RankName;

	public RustButton Submit;
}
