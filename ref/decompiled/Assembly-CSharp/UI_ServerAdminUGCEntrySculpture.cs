using Facepunch.MarchingCubes;
using UnityEngine;
using UnityEngine.UI;

public class UI_ServerAdminUGCEntrySculpture : UI_ServerAdminUGCEntry
{
	[SerializeField]
	private Camera renderCamera;

	[SerializeField]
	private Material materialToUse;

	[SerializeField]
	private RawImage targetRawImage;

	[SerializeField]
	private SDFSet SDFSet;
}
