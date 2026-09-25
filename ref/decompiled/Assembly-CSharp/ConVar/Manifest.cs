using Facepunch;

namespace ConVar;

public class Manifest
{
	[ClientVar(Help = "(Generated) Prints the Facepunch application manifest in a formatted summary showing app name, version, and platform details")]
	[ServerVar(Help = "(Generated) Prints the Facepunch application manifest in a formatted summary showing app name, version, and platform details")]
	public static object PrintManifest()
	{
		return Application.Manifest;
	}

	[ServerVar(Help = "(Generated) Prints the raw contents of the Facepunch manifest file as an unformatted string")]
	[ClientVar(Help = "(Generated) Prints the raw contents of the Facepunch manifest file as an unformatted string")]
	public static object PrintManifestRaw()
	{
		return Facepunch.Manifest.Contents;
	}
}
