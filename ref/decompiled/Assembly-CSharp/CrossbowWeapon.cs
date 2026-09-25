public class CrossbowWeapon : ArrowWeapon
{
	public override void DidAttackServerside()
	{
		SendNetworkUpdateImmediate();
	}
}
