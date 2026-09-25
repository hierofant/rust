using UnityEngine;

[ExecuteAlways]
public class VertexNormalDebugger : MonoBehaviour
{
	private static readonly int positionsBufferId = Shader.PropertyToID("_VertexPositionsBuffer");

	private static readonly int modelMatrixId = Shader.PropertyToID("_ModelMatrix");

	private static readonly int invModelMatrixId = Shader.PropertyToID("_InvModelMatrix");

	[SerializeField]
	private Mesh mesh;

	[SerializeField]
	private Material material;

	private GraphicsBuffer vertexPositionsBuffer;

	private Vector3[] vertexPositions;

	private void OnEnable()
	{
		if (!(mesh == null) && !(material == null))
		{
			vertexPositionsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, GraphicsBuffer.UsageFlags.None, mesh.vertexCount * 2, 12);
			vertexPositions = new Vector3[mesh.vertexCount * 2];
			for (int i = 0; i < vertexPositions.Length; i += 2)
			{
				int num = i / 2;
				vertexPositions[i] = mesh.vertices[num];
				vertexPositions[i + 1] = mesh.vertices[num] + mesh.normals[num];
			}
		}
	}

	private void LateUpdate()
	{
		vertexPositionsBuffer.SetData(vertexPositions);
		Shader.SetGlobalBuffer(positionsBufferId, vertexPositionsBuffer);
		Shader.SetGlobalMatrix(modelMatrixId, base.transform.localToWorldMatrix);
		Shader.SetGlobalMatrix(invModelMatrixId, base.transform.worldToLocalMatrix);
		Graphics.DrawProcedural(material, new Bounds(Vector3.zero, Vector3.one), MeshTopology.Lines, mesh.vertexCount * 2);
	}

	private void OnDisable()
	{
		vertexPositionsBuffer?.Dispose();
		vertexPositionsBuffer = null;
		vertexPositions = null;
	}
}
