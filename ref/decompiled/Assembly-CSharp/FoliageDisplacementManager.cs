using UnityEngine;

public class FoliageDisplacementManager : SingletonComponent<FoliageDisplacementManager>, IClientComponent
{
	[SerializeField]
	private Material clearDisplacementMat;
}
