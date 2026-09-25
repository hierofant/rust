public class MasterKey : Keycard
{
	public static readonly Flags Flag_BreakingIn = Flags.Reserved3;

	public override void OnHeldChanged()
	{
		base.OnHeldChanged();
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags);
		flagsUpdateScope.Set(Flag_BreakingIn, b: false);
	}
}
