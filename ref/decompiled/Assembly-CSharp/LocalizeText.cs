using Rust.Localization;
using UnityEngine;

public class LocalizeText : MonoBehaviour, IClientComponent, ILocalize
{
	public enum SpecialMode
	{
		None,
		AllUppercase,
		AllLowercase
	}

	[LocalizationToken]
	public string token;

	public string append;

	public SpecialMode specialMode;

	public string LanguageToken
	{
		get
		{
			return token;
		}
		set
		{
			token = value;
		}
	}

	public string LanguageEnglish => Translate.Get(token);
}
