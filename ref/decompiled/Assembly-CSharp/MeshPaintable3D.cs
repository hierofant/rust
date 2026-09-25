using UnityEngine;

public class MeshPaintable3D : BaseMeshPaintable
{
	[ClientVar(Help = "(Generated) Scale multiplier for the mesh paint brush size when painting on a 3D paintable surface")]
	public static float brushScale = 2f;

	[ClientVar(Help = "(Generated) Scale multiplier for the UV buffer texture used when projecting paint onto a 3D surface; higher values improve paint resolution")]
	public static float uvBufferScale = 2f;

	public string replacementTextureName = "_MainTex";

	public int textureWidth = 256;

	public int textureHeight = 256;

	public Camera cameraPreview;

	public Camera camera3D;
}
