using System.Collections.Generic;
using UnityEngine;

public class ClientIOLine : BaseMonoBehaviour, INotifyLOD
{
	public LineRenderer _line;

	public NotifyLOD lod;

	public Material directionalMaterial;

	public Material defaultMaterial;

	public IOEntity.IOType lineType;

	public WireTool.WireColour colour;

	public static List<ClientIOLine> allLines = new List<ClientIOLine>();

	public IOEntity ownerIOEnt;

	public float[] slackLevels = new float[18];

	public Vector3[] originalPositions = new Vector3[18];
}
