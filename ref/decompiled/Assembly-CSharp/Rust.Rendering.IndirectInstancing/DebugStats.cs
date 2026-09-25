namespace Rust.Rendering.IndirectInstancing;

public ref struct DebugStats
{
	public int calls_submitted;

	public int commands_submitted;

	public uint instances_submitted;

	public int num_transparent_calls;

	public int num_graphics_calls_multi_draw;

	public int num_graphics_calls_mesh_draw;

	public long cull_result_bits_0;

	public long cull_result_bits_1;

	public const int cull_result_bits = 128;

	public bool IsSet(int bit)
	{
		return (((bit < 64) ? cull_result_bits_0 : cull_result_bits_1) & (1L << bit)) != 0;
	}
}
