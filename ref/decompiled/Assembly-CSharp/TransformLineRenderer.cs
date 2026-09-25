using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

public class TransformLineRenderer : MonoBehaviour, IClientComponent
{
	internal struct LineRendererUpdateJob : IJobParallelForTransform
	{
		[NativeMatchesParallelForLength]
		[WriteOnly]
		public NativeArray<Vector3> ResultWorldPositions;

		public void Execute(int index, [Unity.Collections.ReadOnly] TransformAccess transform)
		{
			ResultWorldPositions[index] = transform.position;
		}
	}

	public Transform[] TransformSequence;

	public LineRenderer TargetRenderer;

	public bool IsEnabled = true;

	[Tooltip("Will keep the line renderer enabled when IsEnabled is false, allowing you to freeze the update while keeping the visuals.")]
	public bool KeepLastUpdate;
}
