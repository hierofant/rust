using System;
using Rust.UI;

public class PickAFriend : UIDialog
{
	public RustInput rustInput;

	public bool AutoSelectInputField;

	public bool AllowMultiple;

	public Action<ulong, string> onSelected;

	public SteamFriendsList friendsList;

	public Func<ulong, bool> shouldShowPlayer
	{
		set
		{
			if (friendsList != null)
			{
				friendsList.shouldShowPlayer = value;
			}
		}
	}
}
