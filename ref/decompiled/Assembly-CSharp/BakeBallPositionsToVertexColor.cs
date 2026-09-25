using UnityEngine;

public class BakeBallPositionsToVertexColor : MonoBehaviour
{
	private const int NUM_BALLS = 100;

	[SerializeField]
	private Mesh originalMesh;

	[SerializeField]
	private int subMeshIndex = 1;
}
