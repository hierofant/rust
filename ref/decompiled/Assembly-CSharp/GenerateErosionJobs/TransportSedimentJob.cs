using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace GenerateErosionJobs;

[BurstCompile(FloatMode = FloatMode.Deterministic)]
internal struct TransportSedimentJob : IJobParallelFor
{
	public NativeArray<float> SedimentMap;

	public NativeArray<float>.ReadOnly SedimentReadOnlyMap;

	public NativeArray<float2>.ReadOnly VelocityMap;

	public int Res;

	public float DT;

	public void Execute(int index)
	{
		int num = index % Res;
		int num2 = index / Res;
		float2 @float = VelocityMap[index];
		int valueToClamp = (int)((float)num - DT * @float.x);
		int valueToClamp2 = (int)((float)num2 - DT * @float.y);
		valueToClamp = math.clamp(valueToClamp, 0, Res - 1);
		valueToClamp2 = math.clamp(valueToClamp2, 0, Res - 1);
		SedimentMap[index] = SedimentReadOnlyMap[valueToClamp2 * Res + valueToClamp];
	}
}
