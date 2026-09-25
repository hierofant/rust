namespace ConVar;

[Factory("dartsgame")]
public class DartsGame : ConsoleSystem
{
	[ReplicatedVar(Saved = true, Help = "Sets the score target in Darts. Standard games are either 301 or 501.")]
	public static int scoreTarget = 301;

	[ReplicatedVar(Saved = true)]
	public static bool needDoubleToWin = true;

	[ReplicatedVar(Saved = true)]
	public static float cooldownBetweenThrows = 3f;

	[ReplicatedVar(Saved = true)]
	public static float maxThrowTimer = 10f;

	[ReplicatedVar(Saved = true)]
	public static float holdFocusDuration = 2f;

	[ReplicatedVar(Saved = true)]
	public static float holdFocusAddedFriction = 0.45f;

	[ReplicatedVar(Saved = true)]
	public static float minAccuracy = 0.1f;

	[ReplicatedVar(Saved = true)]
	public static float maxAccuracy = 0f;

	[ReplicatedVar(Saved = true)]
	public static float accurateVelocityThreshold = 0.05f;

	[ReplicatedVar(Saved = true)]
	public static float accuracyAdjustSpeed = 1f;

	[ReplicatedVar(Saved = true)]
	public static float reticleFriction = 0.5f;

	[ReplicatedVar(Saved = true)]
	public static float reticleInputSpeed = 0.01f;

	[ReplicatedVar(Saved = true)]
	public static float reticleInputBufferApplication = 100f;

	[ReplicatedVar(Saved = true)]
	public static float reticleSpawnPointRadiusOffset = 0.5f;

	[ReplicatedVar(Saved = true)]
	public static float reticleMaxVelocity = 3f;

	[ReplicatedVar(Saved = true)]
	public static float reticleRandomForceStrength = 0.5f;

	[ReplicatedVar(Saved = true)]
	public static int idleKickSeconds = 180;

	[ReplicatedVar(Saved = true)]
	public static float maxZoom = 0.5f;

	[ReplicatedVar(Saved = true)]
	public static bool debugCanPlayAgainstSelf = false;
}
