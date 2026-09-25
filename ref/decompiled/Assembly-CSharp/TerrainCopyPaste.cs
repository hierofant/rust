using UnityEngine;

public class TerrainCopyPaste : MonoBehaviour, IEditorComponent
{
	public Vector3 Size = new Vector3(100f, 10f, 100f);

	public bool CopyHeightMap = true;

	public bool CopySplatMap = true;

	public bool CopyBiomeMap = true;

	public bool CopyAlphaMap = true;

	public bool CopyTopologyMap = true;

	public bool CopyWaterMap = true;

	[HideInInspector]
	[SerializeField]
	private bool _hasCopied;

	[HideInInspector]
	[SerializeField]
	private bool _isUndo;

	[HideInInspector]
	[SerializeField]
	private Vector3 _copySize;

	[HideInInspector]
	[SerializeField]
	private RectInt _heightMapRect;

	[HideInInspector]
	[SerializeField]
	private Color[] _heightMapData;

	[SerializeField]
	[HideInInspector]
	private RectInt _splat0Rect;

	[SerializeField]
	[HideInInspector]
	private Color[] _splat0Data;

	[SerializeField]
	[HideInInspector]
	private RectInt _splat1Rect;

	[SerializeField]
	[HideInInspector]
	private Color[] _splat1Data;

	[SerializeField]
	[HideInInspector]
	private RectInt _biomeRect;

	[HideInInspector]
	[SerializeField]
	private Color[] _biomeData;

	[SerializeField]
	[HideInInspector]
	private RectInt _alphaRect;

	[SerializeField]
	[HideInInspector]
	private Color[] _alphaData;

	[SerializeField]
	[HideInInspector]
	private RectInt _topologyRect;

	[SerializeField]
	[HideInInspector]
	private Color[] _topologyData;

	[SerializeField]
	[HideInInspector]
	private RectInt _waterRect;

	[SerializeField]
	[HideInInspector]
	private Color[] _waterData;

	public bool HasCopied => _hasCopied;

	public bool IsUndo => _isUndo;
}
