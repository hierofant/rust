public class MobileInventoryEntity : BaseEntity
{
	public SoundDefinition ringingLoop;

	public SoundDefinition silentLoop;

	public const Flags Ringing = Flags.Reserved1;

	public static Flags Flag_Silent = Flags.Reserved2;

	public void ToggleRinging(bool state)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved1, state);
	}

	public void SetSilentMode(bool wantsSilent)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flag_Silent, wantsSilent);
	}
}
