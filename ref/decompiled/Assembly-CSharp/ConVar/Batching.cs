namespace ConVar;

[Factory("batching")]
public class Batching : ConsoleSystem
{
	[ClientVar(Help = "(Generated) Enables the mesh batching system, which combines nearby static meshes into single draw calls to reduce rendering overhead")]
	public static bool enabled = true;

	[ClientVar(ClientAdmin = true, Help = "(Generated) Enables multi-threaded renderer processing for the batching system, spreading batch rebuild work across CPU cores")]
	public static bool renderer_threading = true;

	[ClientVar(ClientAdmin = true, Help = "(Generated) Maximum number of renderer components the batching system will process; increase for dense scenes, decrease to limit memory use")]
	public static int renderer_capacity = 30000;

	[ClientVar(ClientAdmin = true, Help = "(Generated) Maximum vertex count of a single mesh allowed into a batch; meshes above this limit are excluded from batching")]
	public static int renderer_vertices = 1000;

	[ClientVar(ClientAdmin = true, Help = "(Generated) Maximum number of sub-meshes a renderer can have to be eligible for batching; complex multi-material meshes above this threshold are excluded")]
	public static int renderer_submeshes = 2;

	[ClientVar(Help = "(Generated) When enabled, industrial pipe meshes are included in the mesh batching pass; disabled by default as pipes frequently change state")]
	public static bool batch_industrial_pipes = false;

	[ClientVar(Help = "(Generated) Verbosity level for static batching debug output; 0 = off, higher values print more detail about batch operations to the console")]
	[ServerVar(Help = "(Generated) Verbosity level for static batching debug output; 0 = off, higher values print more detail about batch operations to the console")]
	public static int verbose = 0;
}
