using UnityEngine;

public class UIDialog : ListComponent<UIDialog>
{
	[Tooltip("Should the nametags be hidden while this dialog is open?")]
	public bool hideNametags;

	public SoundDefinition openSoundDef;

	public SoundDefinition closeSoundDef;
}
