public interface ICanFireHelicopterFlares
{
	BaseEntity flareEntity { get; }

	HelicopterFlares FlaresInstance { get; }

	void FireFlares();

	void FlareFireFailed(BasePlayer player);
}
